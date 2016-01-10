using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

using MS.Utility;
using MS.WebUtility;

namespace App.Library
{
    public partial class ExtendedUser
    {

        public static EPCategory UserDefaultProductCategory = EPCategory.CreateSingle<ExtendedUser>("DefaultProduct");


        public static readonly AccessPermissions[] EmptyAccessPermissions = new AccessPermissions[0];
        public static readonly AccessPermissions[] StandardAccessPermissions = new AccessPermissions[] { AccessPermissions.Standard };
        //!!public static readonly string StandardRole = AccessPermissions.Standard.ToString();
        public static readonly string GroupAdminRole = AccessPermissions.GroupAdmin.ToString();
        public static readonly string SystemAdminRole = AccessPermissions.SystemAdmin.ToString();
        public static readonly string AccountAdminRole = AccessPermissions.AccountAdmin.ToString();
        //!!public static readonly string DatabaseAdminRole = AccessPermissions.DatabaseAdmin.ToString();

        private static readonly AccessPermissions[] PartnerApiAssignablePermissions = 
        {
            AccessPermissions.Standard,
            AccessPermissions.GroupAdmin,
        };

        private static readonly AccessPermissions[] GroupAdminAssignablePermissions = 
        {
            AccessPermissions.Standard,
        };

        private static readonly AccessPermissions[] SystemAdminAssignablePermissions = 
        {
            AccessPermissions.Standard,
            AccessPermissions.GroupAdmin,
            AccessPermissions.SystemAdmin,
        };

        private static readonly AccessPermissions[] SecurityAdminAssignablePermissions = 
        {
            AccessPermissions.Standard,
            AccessPermissions.GroupAdmin,
            AccessPermissions.AccountAdmin,
            AccessPermissions.SystemAdmin,
            AccessPermissions.DatabaseAdmin,
            AccessPermissions.OperationsAdmin,
            AccessPermissions.SecurityAdmin
        };

        public static IEnumerable<AccessPermissions> GetAssignablePermissions(Identity authorizedBy)
        {
            IEnumerable<AccessPermissions> assignablePermissions = Enumerable.Empty<AccessPermissions>();

            if (authorizedBy.IsPartnerApi)
            {
                assignablePermissions = assignablePermissions.Union(PartnerApiAssignablePermissions);
            }
            if (authorizedBy.IsSecurityAdmin)
            {
                assignablePermissions = assignablePermissions.Union(SecurityAdminAssignablePermissions);
            }
            if (authorizedBy.IsSystemAdmin)
            {
                assignablePermissions = assignablePermissions.Union(SystemAdminAssignablePermissions);
            }
            if (authorizedBy.IsInRole(AppRole.GroupAdmin))
            {
                assignablePermissions = assignablePermissions.Union(GroupAdminAssignablePermissions);
            }

            return assignablePermissions;
        }

        public enum AccessPermissions
        {
            Standard,
            GroupAdmin,
            AccountAdmin,
            SystemAdmin,
            DatabaseAdmin,
            OperationsAdmin,
            SecurityAdmin,
        }

        public enum AccessFilters
        {
            Standard,
            GroupAdmin,
            AccountAdmin,
            SystemAdmin,
            DatabaseAdmin,
            OperationsAdmin,
            SecurityAdmin,
            BetaUser
        }

        public class Signature
        {
            [XmlAttribute]
            public bool Enabled { get; set; }
            [XmlText]
            public string Value { get; set; }
        }

        public class Data : XmlSerializableClass<Data>
        {
            public Signature Signature { get; set; }
        }

        private Data customPropertiesDataField;
        private Data customPropertiesData
        {
            get
            {
                if (this.customPropertiesDataField == null)
                {
                    this.customPropertiesDataField = XmlSerializer<Data>.LoadString(this.CustomProperties);
                }
                return this.customPropertiesDataField;
            }
        }

        partial void OnLoaded()
        {
        }



        // All Users - within the same Tenant as the caller 
        public static IQueryable<ExtendedUser> Query(AppDC dc)
        {
            var authorizedBy = dc.TransactionAuthorizedBy;
            Debug.Assert(authorizedBy != null);
            if (authorizedBy == null)
            {
                return Enumerable.Empty<ExtendedUser>()
                    .AsQueryable();
            }

            var tenantGroupID = authorizedBy.TenantGroupID;
            if (!tenantGroupID.HasValue)
            {
                return Enumerable.Empty<ExtendedUser>()
                    .AsQueryable();
            }

            var userExtendedQuery =
                from user in MS.Utility.User.TeamQuery(dc)
                join extendedUser in dc.ExtendedUsers on user.ID equals extendedUser.ID into extendedUserGroup
                from extendedUser in extendedUserGroup.DefaultIfEmpty()
                select new { user, extendedUser };

            var resultQuery = userExtendedQuery
                .Where(userExtended => userExtended.user.TenantGroupID == tenantGroupID.Value)
                .Select(userExtended => userExtended.extendedUser);

            return resultQuery;
        }


        public static ExtendedUser FindByID(AppDC dc, int userID)
        {
            var result = ExtendedUser.Query(dc)
                .Where(extendedUser => extendedUser.ID == userID)
                .FirstOrDefault();
            return result;
        }





        public static HubResult SendSignupInvitation(AppDC dc, string toEmailAddresses, dynamic data)
        {
            Debug.Assert(dc != null);
            Debug.Assert(!string.IsNullOrEmpty(toEmailAddresses));

            if (string.IsNullOrEmpty(toEmailAddresses))
            {
                return HubResult.EmailInvalid;
            }

//!! for now, default to inviting users into the caller's TenantGroup.
            var authorizedBy = dc.TransactionAuthorizedBy;
            var authorizedByTenantGroupID = authorizedBy.TenantGroupID.Value;

            var result = createAndSendInvitations(dc, authorizedByTenantGroupID, toEmailAddresses);
            return result;
        }


        private static HubResult createAndSendInvitations(AppDC dc, /*CachedTorqReferenceDataContext referenceDC,*/ /* Client inviteClient,*/ int tenantGroupID, string toEmailAddresses)
        {
            //Debug.Assert(referenceDC != null);
            Debug.Assert(tenantGroupID > 0);
            Debug.Assert(!string.IsNullOrEmpty(toEmailAddresses));

            var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantGroupID);
            var extendedTenantGroupInfo = tenantGroupInfo as ClientInfo;

            Debug.Assert(tenantGroupInfo != null);
            if (tenantGroupInfo == null)
            {
                return HubResult.CreateError("Tenant Group not found");
            }

            if (tenantGroupInfo.HasDescendents)
            {
                return HubResult.CreateError("Users cannot be invited to parent groups");
            }

            var validatedMailAddressCollection = ValidatedMailAddressCollection.Parse(toEmailAddresses);

            // check if they have any seats open
            if ((extendedTenantGroupInfo.NumberOfSeatsContracted - tenantGroupInfo.ActiveUsersCount) < validatedMailAddressCollection.Count())
            {
                return HubResult.CreateError("This client group doesn't have enough seats contracted to complete your request");
            }

            if (validatedMailAddressCollection.ContainsInvalid)
            {
                return HubResult.CreateError("Some email addresses are invalid");
            }

            CreateUserAuthTemplate.SendEmails(dc, validatedMailAddressCollection, tenantGroupID);


#if false
            if (inviteClient != null && !validatedMailAddressCollection.ContainsInvalid)
            {
                // Client validation, users can only be associated with Client groups that don't have any child Client groups (not a parent group)

                // get product
                Product product = this.GetProduct();

                // check if client is licensed for the product
                if (!inviteClient.IsProductLicensed(product))
                {
                    // Invalid client/product selection
                    this.ErrorMessage.InnerText = "This client group is not licensed for that product.";
                    return 0;
                }

                // get lma
                string lmsCode = this.StateDropDown.SelectedValue ?? string.Empty;
                string lmaCode = Request.Form["ctl00$MainContentPlaceHolder$RegionDropDown"] ?? string.Empty;

                // check if client group is licensed for the selected lmas
                if (lmsCode.Length > 0)
                {
                    // If a state code has been selected, verify it's a valid selection for this group
                    var licencedLaborMarketStates = Client.GetLicensedStates(this.TorqIdentity, inviteClient.ClientId);
                    if (!licencedLaborMarketStates.Any(laborMarketState => laborMarketState.LaborMarketStateCode == lmsCode))
                    {
                        Debug.Assert(this.TorqIdentity.IsSystemAdmin);
                        this.ErrorMessage.InnerText = "The selected state is not associated with this client group.";
                        this.StateDropDown.SelectedIndex = -1;
                        this.RegionDropDown.SelectedIndex = -1;
                        return 0;
                    }

                    if (lmaCode.Length > 0)
                    {
                        int lmaClientID;
                        var licensedLaborMarketAreas = Client.GetLicensedLaborMarketAreas(this.TorqIdentity, inviteClient.ClientId, out lmaClientID);

                        if (!licensedLaborMarketAreas.Any(laborMarketArea => laborMarketArea.LaborMarketAreaCode == lmaCode))
                        {
                            this.ErrorMessage.InnerText = "The selected LMA is not associated with this client group.";
                            this.StateDropDown.SelectedIndex = -1;
                            this.RegionDropDown.SelectedIndex = -1;
                            return 0;
                        }
                    }
                }

                // permissions
                // default to Standard
                T.User.AccessPermissions accessPermissions = T.User.AccessPermissions.Standard;

                // ... and then override with SystemAdmin settings if necessary
                if (this.UserIdentity.IsSystemAdmin)
                {
                    accessPermissions = this.AccessPermissions.SelectedValue.ParseAsEnum<T.User.AccessPermissions>();
                }

                TimeSpan duration = TimeSpan.FromDays(3650);

                if (validatedMailAddressCollection.Count() > 0)
                {
                    string subject = "Create your TORQ account today!"; //"TORQ Invitation from " + this.UserIdentity.Name;

                    foreach (MailAddress address in validatedMailAddressCollection)
                    {
                        // create user auth token
                        CreateUserAuthToken authToken = CreateUserAuthToken.ExpireAndCreateNew(this.TorqIdentity.UserIDOrThrow, inviteClient.ClientId, accessPermissions, product, lmsCode, lmaCode, address, duration);

                        if (authToken != null)
                        {
                            // send invite
                            string body = this.MessageTextBox.Text;
                            string url = authToken.Url.AbsoluteUri;
                            body += "\r\n\r\nClick on the Invite Link* below to get started.\r\n*Invite Link: " + url;


                            var result = sendInviteEmail(mailAddress, "User Invite Email", "User invite email body. Good stuff.");


                            bool sent = this.SendInviteEmail(address, subject, body);
                            if (sent)
                            {
                                // Remember we sent this one
                                string sentReceipt = string.Format("To: {0}, Url: {1}",
                                    /*0*/ address,
                                    /*1*/ url);
                                sentInvitations.Add(sentReceipt);

                                // Update date sent
                                authToken.SetLastSentDate(this.Context.UtcTimestamp());
                            }
                        }
                        else
                        {
                            // Error creating CreateUserAuthToken
                            Debug.Fail("CreateUserAuthToken failed.");
                        }
                    }

                    if (sentInvitations.Count > 0)
                    {
                        TorqContext torq = TorqContext.Current;

                        string description = string.Format("User Invite Sent, Client: {0} [ClientId:{1}]\r\nSent by: {2} [UID:{3}]\r\n{4}",
                            /*0*/ inviteClient.Name,
                            /*1*/ inviteClient.ClientId,
                            /*2*/ this.TorqIdentity.Name,
                            /*3*/ this.TorqIdentity.UserIDOrThrow,
                            /*4*/ sentInvitations.Join("\r\n"));
                        var usabilityItem = UsabilityItem.Create(description);
                        torq.EventLog.Add(usabilityItem);
                    }
                }
                else
                {
                    this.ErrorMessage.InnerText = "Error sending invitations.";
                }
            }

            return sentInvitations.Count;

#endif

            return HubResult.Success;
        }

















#if false



        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                var createUserAuthCode = Request.GetAuthCode<CreateUserAuthToken>("authCode");

                if (createUserAuthCode.HasValue)
                {
                    // Create user
                    User user = this.CreateUser(createUserAuthCode.Value);

                    if (user != null)
                    {
                        // log them in
                        TorqUserAuthentication.LoginSession(user, user.DefaultProduct, false);

                        // show welcome
                        this.ActivateAccount.Visible = false;
                        this.ActivateWelcome.Visible = true;

                        // send welcome email
                        this.SendWelcomeEmail(user);
                    }
                }
            }
        }
#endif

#if true
        /// <summary>
        /// Create new User from the auth token
        /// </summary>
        /// <returns>User object</returns>
        public static User RedeemSignupInvitation(AppDC dc, AuthCode authCode, string password, dynamic options, out string errorMessage)
        {
            CreateUserAuthTemplate.Data data;
            var createUserAuthToken = CreateUserAuthTemplate.FindAuthToken(dc, authCode, out data);
            if (createUserAuthToken == null)
            {
                errorMessage = "nope";
                return null;
            }

            MailAddress mailAddress = data.MailAddress;
            Debug.Assert(mailAddress != null);

            var adsf = data.SystemRoles;
            var ffsf = data.AppRoles;


            var systemRoles = new SystemRole[] { SystemRole.TenantAdmin };
            var appRoles = new [] { AppRole.GroupAdmin }.Select(appRole => appRole.ToString()).ToArray();


            //!! var user = User.Create(createUserAuthCode, token.MailAddress, newPassword, firstName, lastName, token.Product, token.AuthClientID.GetValueOrDefault(0), out errorMessage);

            // (handles firstName, lastName too)
            try
            {
                User user = User.Create(dc, authCode, mailAddress, systemRoles, appRoles, password, options);
                if (user == null)
                {
                    errorMessage = null;
                    return null;
                }


                var extendedUser = new ExtendedUser()
                {
                    ID = user.ID,

                    //PreferredState = migrateUser.PreferredState,
                    //PreferredLaborMarketArea = migrateUser.PreferredLaborMarketArea,

                    //Title = migrateUser.Title,
                    //Organization = migrateUser.Organization,
                    //Address1 = migrateUser.Address1,
                    //Address2 = migrateUser.Address2,
                    //City = migrateUser.City,
                    //State = migrateUser.State,
                    //Zip = migrateUser.Zip,
                    //Phone = migrateUser.Phone,
                    //PhoneExtension = migrateUser.PhoneExtension,

                    //SecureProperties = null,
                    //CustomProperties = migrateUser.UserSettings == null ? null : migrateUser.UserSettings
                    //.Replace("userSettings", "Data")
                    //.Replace("signature", "Signature")
                    //.Replace("enabled", "Enabled")
                    //.Replace("True", "true")
                    //.Replace("False", "false"),
                };
                dc.Save(extendedUser);

                errorMessage = null;
                return user;

#if false
            if (errorMessage.Length > 0)
            {
                this.CreateUserError.InnerHtml = errorMessage;
                this.CreateUserError.Visible = true;
            }
            else
            {
                // Save preferred state and lma
                if (token.PreferredState.Length > 0)
                {
                    user.PreferredState = token.PreferredState;
                    if (token.PreferredLaborMarketArea.Length > 0)
                    {
                        user.PreferredLaborMarketArea = token.PreferredLaborMarketArea;
                    }
                    user.DBUpdate();
                }
            }
#endif
            }
            catch (HubResultException hubResultException)
            {
                errorMessage = hubResultException.HubResult.ErrorMessage;
                return null;
            }
        }

        private void SendWelcomeEmail(UtilityDC dc, User user)
        {
            var userPrimaryEmail = User.GetPrimaryMailAddress(dc, user.ID);


            string body = @"Dear {0},

Congratulations! You have successfully set up your TORQ account. To access your TORQ account and use TORQ any time, just follow these quick steps:

1.	Go to http://torq3.torqworks.com.  (You’ll want to bookmark this page in your Web browser.)
2.	Enter your email address in the box marked “Email”: {1}
3.	Enter your password. (Use the “forgot password?” link to retrieve your password in the event you cannot remember it.) 
4.	 . . .and click “Sign In!”

Good luck! We hope you will enjoy using TORQ with your clients. If you have questions, comments, or concerns, contact your TORQ Group administrator(s) or contact TORQworks directly using the “Feedback” button on any page in TORQ. 

Contact Info for your Group Administrator(s)
{2}: {3}

Best wishes,
TORQworks
www.torqworks.com";

            // 0 - full name, 1 - email, 2 - group admin name, 3 - group admin email
            User admin = null;
            //!!User admin = user.TenantGroup.Users.FirstOrDefault(u => u.IsGroupAdmin == true);

            if (admin == null)
            {
                body = @"Dear {0},

Congratulations! You have successfully set up your TORQ account. To access your TORQ account and use TORQ any time, just follow these quick steps:

1.	Go to http://torq3.torqworks.com.  (You’ll want to bookmark this page in your Web browser.)
2.	Enter your email address in the box marked “Email”: {1}
3.	Enter your password. (Use the “forgot password?” link to retrieve your password in the event you cannot remember it.) 
4.	 . . .and click “Sign In!”

Good luck! We hope you will enjoy using TORQ with your clients. If you have questions, comments, or concerns, contact TORQworks directly using the “Feedback” button on any page in TORQ. 

Best wishes,
TORQworks
www.torqworks.com";

                body = String.Format(body, user.FullName, userPrimaryEmail);

            }
            else
            {
                //!! body = String.Format(body, user.FullName, userPrimaryEmail, admin.FullName, admin.Email);
            }

            string htmlBody = HttpUtility.UrlDecode(body).Replace("\r\n", "<br/>");
            string subject = "Welcome to TORQ!";

            SiteContext siteContext = SiteContext.Current;

            MailAddress toEmailAddress = userPrimaryEmail;

            //!! straighten out our from email address
            MailMessage mailMessage = new MailMessage();
            mailMessage.To.Add(toEmailAddress);
            mailMessage.Subject = HttpUtility.UrlDecode(subject);
            mailMessage.Body = htmlBody;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;

            try
            {
                siteContext.Emailer.SendEmail(mailMessage);
            }
            catch (Exception ex)
            {
                siteContext.EventLog.LogException(ex);
            }
        }

#endif



























        // Note: this.Access string contains space delimited items. Items are AccessPermissions if they don't contain a ':', otherwise a name:value tags.
        private static IEnumerable<AccessPermissions> getPermissions(string accessString)
        {
            Debug.Assert(accessString!= null);
            accessString = accessString ?? string.Empty;

            return accessString.Split(' ')
                .Where(accessItem => !string.IsNullOrEmpty(accessItem))
                // Exclude name:value pairs - see this.Access note above
                .Where(accessItem => !accessItem.Contains(':'))
                .Select(accessPermission => accessPermission.ParseAsEnum<AccessPermissions>(AccessPermissions.Standard))
                .Distinct();
        }

        private static IEnumerable<string> getTags(string accessString)
        {
            Debug.Assert(accessString != null);
            accessString = accessString ?? string.Empty;

            return accessString.Split(' ')
                .Where(accessItem => !string.IsNullOrEmpty(accessItem))
                // Exclude permissions - see this.Access note above
                .Where(accessItem => accessItem.Contains(':'));
        }
#if false
        private string getTagValue(string tagName)
        {
            Debug.Assert(!string.IsNullOrEmpty(tagName));

            string delimitedTagName = tagName + ':';

            string accessString = this.Access ?? string.Empty;
            foreach (string tag in getTags(accessString))
            {
                Debug.Assert(!string.IsNullOrEmpty(tag));
                Debug.Assert(tag.Contains(':'));

                if (tag.StartsWith(delimitedTagName))
                {
                    return tag.TrimStart(delimitedTagName);
                }
            }
            return null;
        }
#endif
#if false
        public string DomainRestriction
        {
            get { return getTagValue("domain"); }
        }
#endif
#if false
        public IEnumerable<AccessPermissions> Permissions
        {
            get
            {
                string accessString = this.Access ?? string.Empty;
                return getPermissions(accessString);
            }
        }
#endif
        public static string GetPermissionsDisplay(string access)
        {
            return ExtendedUser.getPermissions(access)
                .Select(permission => permission.ToString())
                .Join(", ");
        }
#if false
        public string PermissionsDisplay
        {
            get 
            {
                string accessString = this.Access ?? string.Empty;
                return GetPermissionsDisplay(accessString);
            }
        }
#endif
        public static string GetAccessTag(string access, string tagName)
        {
            string domainRestriction = string.Empty;
            string domainString = getTags(access).FirstOrDefault(t => t.StartsWith(tagName + ":"));
            if (!string.IsNullOrEmpty(domainString))
            {
                domainRestriction = domainString.Substring(tagName.Length + 1);
            }
            return domainRestriction;
        }

#if false
        public void DBUpdate()
        {
            //!! Eventually this should be made private.
            //   For external callers, we can expose a begin/end transaction type API

            this.LastModifiedDate = DateTime.UtcNow;
            this.AccountsDBUpdate();
        }

        partial void OnAccessChanged()
        {
            // We store this value in our AuthCookie - so need to refresh the AuthCookie when it changes
            TorqIdentity authorizedBy = HttpContext.Current.GetTorqIdentity();
            TorqUserAuthentication.RefreshSessionUserInformation(authorizedBy, this);
        }
#endif

#if false
        private static string removeTag(string accessString, string tagName)
        {
            // If it's present, we need to remove the name:value tag from accessString. Get ready for some string manipulation.
            Debug.Assert(!string.IsNullOrEmpty(tagName));

            if (!string.IsNullOrEmpty(accessString))
            {
                string delimitedTagName = tagName + ':';

                int startIndex = accessString.IndexOf(delimitedTagName);

                if (startIndex >=0)
                {
                    // if we don't find a space delimiter following this tag, delete to the end of the string
                    int count = accessString.Length - startIndex;

                    int spaceDelimiter = accessString.IndexOf(' ', startIndex);
                    if (spaceDelimiter >= 0)
                    {
                        // if we do find a space delimiter, delete just to (and including) that delimiter
                        count = spaceDelimiter - startIndex;
                    }

                    // if we removed the last tag in the list, will also need to remove the now trailing space so Trim() too.
                    accessString = accessString.Remove(startIndex, count).Trim();
                }
            }

            return accessString;
        }

        private static string addTag(string accessString, string tagName, string tagValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(tagName));
            if (string.IsNullOrEmpty(accessString))
            {
                accessString = string.Empty;
            }
            else
            {
                accessString += " ";
            }
            accessString += tagName + ':' + tagValue;
            return accessString;
        }

        private void setTag(string tagName, string tagValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(tagName));

            string currentValue = getTagValue(tagName);
            if (currentValue != tagValue)
            {
                // Note: need to preserve Permissions and other tags stored in the this.Access, so use a remove/add operation
                string accessString = this.Access ?? string.Empty;

                string modifiedAccess = removeTag(accessString, tagName);
                if (tagValue != null)
                {
                    modifiedAccess = addTag(modifiedAccess, tagName, tagValue);
                }

                // Update this.Access with new value. Note this fires OnAccessChanged(), which refreshes our AuthCookie
                this.Access = modifiedAccess;
            }
        }

        public void SetDomainRestriction(string domainRestriction)
        {
            setTag("domain", domainRestriction);
        }
#endif
#if false
        public void SetPermissions(TorqIdentity authorizedBy, IEnumerable<AccessPermissions> requestedPermissions)
        {
            Debug.Assert(requestedPermissions != null);
            if (requestedPermissions == null || !requestedPermissions.Any())
            {
                Debug.Fail("SetPermissions: Attempting to set a user with no permissions");
                requestedPermissions = new[] { AccessPermissions.Standard };
            }

            var existingPermissions = this.Permissions.ToArray();

            var permissionsToRemove = existingPermissions.Except(requestedPermissions).ToArray();
            var permissionsToAdd = requestedPermissions.Except(existingPermissions).ToArray();
            var permissionsModified = permissionsToRemove.Concat(permissionsToAdd).ToArray();

            if (!permissionsModified.Any())
            {
                // Nothing to do - not actually changing any permissions
                return;
            }

            // These are the permissions the caller is allowed to modify
            IEnumerable<AccessPermissions> assignablePermissions = GetAssignablePermissions(authorizedBy);

            var permissionsNotAuthorizedToModify = permissionsModified.Except(assignablePermissions).ToArray();

            if (permissionsNotAuthorizedToModify.Any())
            {
                var error = string.Format("SetPermissions: Attempting to modify unauthorized permissions, Authorized: {0}, Permissions: {1}",
                    /*0*/ authorizedBy.ToString(),
                    /*1*/ requestedPermissions.Select(permission => permission.ToString()).Join(", "));
                TorqContext.Current.EventLog.LogCritical(error);

                return;
            }

            if (permissionsModified.Contains(AccessPermissions.SystemAdmin) ||
                permissionsModified.Contains(AccessPermissions.AccountAdmin) ||
                permissionsModified.Contains(AccessPermissions.SecurityAdmin))
            {
                TorqContext torq = TorqContext.Current;

                string description = "Permissions Change";

                string details = string.Format("Authorized By: {0}\r\nUser: [{1}]-{2}\r\nOld Permissions: {3}\r\nNew Permissions: {4}",
                    /*0*/ authorizedBy.ToString(),
                    /*1*/ this.UserID.ToString(),
                    /*2*/ this.FullName,
                    /*3*/ this.PermissionsDisplay,
                    /*4*/ requestedPermissions.Select(permission => permission.ToString()).Join(", "));

                var eventItem = TorqEventItem.Create(EventItemType.SystemAdminNotification, TorqEventID.UserPermissionsChange, description, details, authorizedBy);
                TorqContext.Current.EventLog.Add(eventItem);
            }

            // Note: need to preserve other tags already in this.Access, so add them in too
            string accessString = this.Access ?? string.Empty;
            var existingTags = getTags(accessString);

            this.Access = requestedPermissions
                .Select(permission => permission.ToString())
                .Concat(existingTags)
                .Join(" ");
        }

        public void SetDefaultProduct(Product defaultProduct)
        {
            Product[] licensedProducts = this.Client.LicensedProducts;

            if (licensedProducts.Contains(defaultProduct))
            {
                this.DefaultProduct = defaultProduct;
            }
            else
            {   
                //!!!should return error message here                          
                Debug.Fail("Attempting to set a default product for which the user is not licensed");
            }
        }

        public bool IsStandardUser
        {
            get { return this.IsInRole(User.AccessPermissions.Standard); }
        }
        public bool IsGroupAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.GroupAdmin); }
        }
        public bool IsAccountAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.AccountAdmin); }
        }
#endif

#if false
        public bool IsSystemAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.SystemAdmin); }
        }
        public bool IsDatabaseAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.DatabaseAdmin); }
        }
        public bool IsOperationsAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.OperationsAdmin); }
        }
        public bool IsSecurityAdmin
        {
            get { return this.IsInRole(User.AccessPermissions.SecurityAdmin); }
        }
#endif

#if false
        public bool IsFullTORQUser
        {
            get { return (this.IsStandardUser || this.IsGroupAdmin || this.IsSystemAdmin); }
        }
        public bool IsBetaUser
        {
            get { return this.DomainRestriction == "beta"; }
        }
        public bool IsActive
        {
            get { return this.Status == "Active"; }
        }
        public bool IsInRole(AccessPermissions permission)
        {
            //!! hmm. Will this ever give a different answer than the UserIdentity based roles check? 
            string accessString = this.Access ?? string.Empty;
            return accessString.Contains(permission.ToString());
        }

        partial void OnDefaultProductTokenChanged()
        {
            // We store this value in our AuthCookie - so need to refresh the AuthCookie when it changes
            //!! nextgen
            //var authorizedBy = HttpContext.Current.GetTorqIdentity();
            //authorizedBy.GetAuthenticatedUser(
            //var modifiedUser = User.FindByID(
            //MS.WebUtility.Authentication.WebUserAuthentication.RefreshSessionUserInformation(authorizedBy, this);
        }
#endif
#if false
        public Product DefaultProduct
        {
            get { return this.DefaultProductToken.ParseAsEnum<Product>(); }
            private set
            {
                this.DefaultProductToken = value.ToString();
            }
        }
#endif

        public static TimeZoneInfo GetLocalTimeZoneInfo(int? timeZoneIndex, int? tenantID)
        {
            if (timeZoneIndex.HasValue)
            {
                var timeZoneInfo = TimeZones.GetTimeZoneInfo(timeZoneIndex.Value);
                Debug.Assert(timeZoneInfo != null);
                if (timeZoneInfo != null)
                {
                    return timeZoneInfo;
                }
            }

            var tenantTimeZone = TenantGroup.GetCachedTimeZoneInfo(tenantID);
            return tenantTimeZone;
        }

#if false
        public static TimeZoneInfo GetLocalTimeZoneInfo(string userTimeZone, string clientTimeZone)
        {
            if (!string.IsNullOrEmpty(userTimeZone))
            {
                return TimeZones.Parse(userTimeZone);
            }

            Debug.Assert(!string.IsNullOrEmpty(clientTimeZone));
            return TimeZones.Parse(clientTimeZone);
        }

        // Provides the user's timezone, and if one isn't specified falls back on the client's timezone
        public string EffectiveTimeZone
        {
            get { return this.TimeZone ?? this.Client.TimeZone; }
        }
#endif
        public static string GetFullName(string firstName, string lastName)
        {
            return firstName + " " + lastName;
        }
#if false
        public string FullName
        {
            get { return this.FirstName + " " + this.LastName; }
        }

        public MailAddress MailAddress
        {
            get { return new MailAddress(this.Email, this.FullName); }
        }

        public static User Create(TorqIdentity authorizedBy, MailAddress email, string password, string firstName, string lastName, Product defaultProduct, IEnumerable<AccessPermissions> accessPermission, int clientID, out string errorMessage)
        {
            return Create(TorqDataContext.AccountsOnlyDCInstance, authorizedBy, email, password, firstName, lastName, defaultProduct, accessPermission, clientID, out errorMessage);
        }

        public static User Create(AuthCode authCode, MailAddress email, string password, string firstName, string lastName, Product defaultProduct, int clientID, out string errorMessage)
        {
            CreateUserAuthToken createUserAuthToken = CreateUserAuthToken.FindValid(authCode);
            if (createUserAuthToken == null || !createUserAuthToken.TrackUsage())
            {
                //!! how do we message this to the user?
                errorMessage = "This link has expired";
                return null;
            }

            User authorizedByUser = createUserAuthToken.AuthUser;
            Debug.Assert(authorizedByUser != null);

            //!! what if the CreateUserAuthToken includes some parameters, like firstName & lastName.
            //   should we just use them? Or give an error if they don't match the passed in ones? Or ignore them?

            return Create(TorqDataContext.AccountsOnlyDCInstance, authorizedByUser.GetUserIdentity(), email, password, firstName, lastName, defaultProduct, User.StandardAccessPermissions, clientID, out errorMessage);
        }

        public static User Create(TorqDataContext accountsDB, TorqIdentity authorizedBy, MailAddress email, string password, string firstName, string lastName, Product defaultProduct, IEnumerable<AccessPermissions> requestedPermission, int clientID, out string errorMessage)
        {
            try
            {
                if (!authorizedBy.PermissionCheck(Permission.CreateUsers))
                {
                    errorMessage = "Insufficient permissions to create a new account.";
                    return null;
                }

                if (requestedPermission == null || !requestedPermission.Any())
                {
                    errorMessage = "Can't create a user with no Access Permissions.";
                    return null;
                }

                IEnumerable<AccessPermissions> assignablePermissions = GetAssignablePermissions(authorizedBy);
                if (requestedPermission.Except(assignablePermissions).Any())
                {
                    errorMessage = "Insufficient permissions to create a new account (2).";
                    return null;
                }

                // email parameter is of type MailAddress - which ensures it is valid syntax for an email address.
                if (email == null || string.IsNullOrEmpty(email.Address))
                {
                    errorMessage = "Please enter a valid email address.";
                    return null;
                }

                User existingUser = User.GetByUserName(accountsDB, email.Address);
                if (existingUser != null)
                {
                    errorMessage = "A user with this Username already exists.";
                    return null;
                }

                if (string.IsNullOrEmpty(firstName))
                {
                    errorMessage = "Please enter a first name.";
                    return null;
                }

                if (string.IsNullOrEmpty(lastName))
                {
                    errorMessage = "Please enter a last name.";
                    return null;
                }

                Client client = Client.GetByClientID(accountsDB, clientID);
                if (client == null)
                {
                    errorMessage = "Unknown ClientID.";
                    return null;
                }

                if (!Client.AccessCheck(authorizedBy, clientID))
                {
                    errorMessage = "Insufficient permission to create users in this client group.";
                    return null;
                }

                if (client.HasDescendents)
                {
                    errorMessage = "Users cannot be assigned to parent groups.";
                    return null;
                }

                if (client.ActiveUserAccounts >= client.NumberOfSeatsContracted)
                {
                    errorMessage = "This user account cannot be created because the contracted seat limit has been reached. Please contact TORQWorks if you believe this is in error.";
                    return null;
                }

                User newUser = new User();

                newUser.Status = "Active";

                //make sure the user is licensed for the passed in defaultProduct
                if (client.LicensedProducts.Contains(defaultProduct))
                {
                    newUser.DefaultProduct = defaultProduct;
                }
                else
                {
                    errorMessage = "This client group is not licensed for that product.";
                    return null;
                }
                newUser.SetPermissions(authorizedBy, requestedPermission);

                newUser.Username = email.Address;
                newUser.Client = client;

                // newUser.Title = 
                newUser.FirstName = firstName;
                newUser.LastName = lastName;
                newUser.Email = email.Address;

                newUser.CreateDate = DateTime.UtcNow;
                newUser.LastModifiedDate = newUser.CreateDate;

                accountsDB.Save(newUser);

                //need a UserId for the hashed password, so must do the Save above first
                newUser.setPassword(accountsDB, password);

                errorMessage = string.Empty;
                return newUser;
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }

            errorMessage = "Unexpected error.";
            return null;
        }

        public static User Get(TorqIdentity authorizedBy, string userIDString)
        {
            int userID;
            if (int.TryParse(userIDString, out userID))
            {
                return Get(authorizedBy, userID);
            }
            return null;
        }

        public static User Get(TorqIdentity authorizedBy, int userID)
        {
            try
            {
                var user = TorqDataContext.AccountsOnlyDCInstance.Users.FirstOrDefault(u => u.UserID == userID);
                if (user != null && user.AccessCheck(authorizedBy))
                {
                    return user;
                }
            }
            catch (Exception ex)
            {
                TorqContext torq = TorqContext.Current;

                if (torq != null)
                {
                    torq.EventLog.LogException(ex);
                }
            }
            return null;
        }

        private static User getByTag(IEnumerable<int> clientIDs, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            try
            {
                var tagUser = TorqDataContext.AccountsOnlyDCInstance.Users
                    .Where(user => user.Tag == tag)
                    .Where(user => user.Status == "Active")
                    .Where(user => clientIDs.Contains(user.ClientId))
                    .FirstOrDefault();
                return tagUser;
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }

            return null;
        }

        public static User GetByTag(TorqIdentity authorizedBy, string tag)
        {
            try
            {
                // tags aren't necessarily unique across client groups. So limit to the related hierarchy
                IEnumerable<int> hierarchyClientIDs = authorizedBy.HierarchyClientIDs;

                var user = User.getByTag(hierarchyClientIDs, tag);

                if (user != null && user.AccessCheck(authorizedBy))
                {
                    // We don't know if the caller is authorized to view this project until we load it and see.
                    // Now that we've done the AccessCheck, it's safe to return it.
                    return user;
                }
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }

            return null;
        }

        public static User GetByUserName(string userName)
        {
            return GetByUserName(TorqDataContext.AccountsOnlyDCInstance, userName);
        }

        public static User GetByUserName(TorqDataContext accountsDB, string userName)
        {
            try
            {
                return accountsDB.Users.FirstOrDefault<User>(user => user.Username == userName.ToLower());
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
            return null;
        }

        public static User GetByEmail(MailAddress mailAddress)
        {
            if (mailAddress == null)
            {
                return null;
            }

            try
            {
                string email = mailAddress.Address;
                if (string.IsNullOrEmpty(email))
                {
                    return null;
                }
                string emailLower = email.ToLower();
                return TorqDataContext.AccountsOnlyDCInstance.Users.FirstOrDefault(user => user.Email.ToLower() == emailLower);
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
            return null;
        }
#endif

#if false
        public static IQueryable<User> GetGroupAdmins(int clientId)
        {
            return GetAdmins(User.GroupAdminRole, clientId.ToEnumerable());
        }

        public static IQueryable<User> GetSystemAdmins()
        {
            return GetAdmins(User.SystemAdminRole, null);
        }
#endif
#if false
        public static IQueryable<User> GetAccountAdmins(int tenantGroupID)
        {
            var clientInfo = Tenant.GetCachedTenantGroupInfo(tenantGroupID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            IEnumerable<int> hierarchyClientIds;

            if (clientInfo != null)
            {
                hierarchyClientIds = clientInfo.Root.DescendentsAndSelf
                    .Select(ci => ci.TenantGroupID);
            }
            else
            {
                // Odd - oh well, as a fallback just return any AAs in this exact client group
                hierarchyClientIds = tenantGroupID.ToEnumerable();
            }

            return GetAdmins(AppRole.AccountAdmin, hierarchyClientIds);
        }

        private static IQueryable<User> GetAdmins(AppRole appRole, IEnumerable<int> clientIds)
        {
            var adminsQuery = AppDC.AccountsOnlyDCInstance.Users
                .Where(user => user.AppRoles.Contains(appRole.ToString()));

            if (clientIds != null)
            {
                adminsQuery = adminsQuery
                    .Where(user => user.TenantID.HasValue && clientIds.Contains(user.TenantID.Value));
            }

            return adminsQuery;
        }
#endif
        public static IEnumerable<MailAddress> GetMailAddresses(AppDC accountsDC, IEnumerable<AppRole> appRoles, int branchTenantID)
        {
            if (appRoles == null || !appRoles.Any())
            {
                return Enumerable.Empty<MailAddress>();
            }

            var requestedAppRoleNames = appRoles
                .Select(appRole => appRole.ToString())
                .ToArray();

            var clientInfo = TenantGroup.GetCachedTenantGroupInfo(branchTenantID) as ClientInfo;
            Debug.Assert(clientInfo != null);
            if (clientInfo == null)
            {
                return Enumerable.Empty<MailAddress>();
            }

            IEnumerable<int> hierarchyTenantGroupIDs = clientInfo.DescendentsAndSelf
                .Select(ci => ci.TenantGroupID);

            var epUserServerQuery =
                from user in User.TeamQuery(accountsDC)
                join epAppRoleTags in User.QueryTeamEPTags2(accountsDC, EPCategory.AppRoleCategory) on user.ID equals epAppRoleTags.TargetID into epAppRoleTagsGroup
                join epMailAddress in User.QueryOrderedEPEmailAddresses(accountsDC) on user.ID equals epMailAddress.ID
                select new 
                {
                    user,

                    AppRoleNames = epAppRoleTagsGroup
                        .Select(epAppRoleTag => epAppRoleTag.Item.Name),

                    epMailAddress.EmailAddress
                };

            var result = epUserServerQuery
                .Where(epUser => epUser.user.TenantGroupID.HasValue && hierarchyTenantGroupIDs.Contains(epUser.user.TenantGroupID.Value))
                .Where(epUser => epUser.AppRoleNames.Intersect(requestedAppRoleNames).Any())
                .Select(epUser => epUser.EmailAddress)
                .AsEnumerable()
                .Select(emailAddress => emailAddress.ToMailAddress())
                .Where(mailAddress => mailAddress != null);

            return result;
        }
#if false
        private static void addAdmins(AppDC appDC, MailAddressCollection mailAddressCollection, IQueryable<User> adminsQuery)
        {
            var epQuery =
                from user in adminsQuery
                join epMailAddress in User.QueryOrderedEPEmailAddresses(appDC) on user.ID equals epMailAddress.ID
                select new { user, epMailAddress.EmailAddress };

            epQuery
                .Select(epUser => epUser.EmailAddress)
                .AsEnumerable()
                .Select(emailAddress => emailAddress.ToMailAddress())
                .Where(mailAddress => mailAddress != null)
                .ForEach(mailAddress => mailAddressCollection.Add(mailAddress));
        }


        public Identity GetUserIdentity()
        {
            UserAuthTicketData ticketData = UserAuthTicketData.Impersonate(this);
            TorqIdentity torqIdentity = TorqIdentity.Create(false, this.FullName, this.UserID, ticketData);
            return torqIdentity;
        }
#endif
#if false
        public bool AccessCheck(Identity authorizedBy)
        {
            if (authorizedBy == null)
            {
                return false;
            }

            if (authorizedBy == TorqIdentity.System)
            {
                return true;
            }

            bool isUser = authorizedBy.IsUserID(this.UserID);
            bool isGroupAdminOfUser = authorizedBy.IsGroupAdmin && authorizedBy.IsUserAuthenticated && (authorizedBy.ClientID == this.ClientId);
            bool isSystemAdmin = authorizedBy.IsSystemAdmin;
            bool isPartner = authorizedBy.IsPartnerApi && authorizedBy.ClientID == this.ClientId;

            return isUser || isGroupAdminOfUser || isSystemAdmin || isPartner;
        }
#endif
        public static AccessFilters[] GetAccessFilters(bool includeUsers, bool includeClientGroupAdmins, bool includeAccountAdmins, bool includeSystemAdmins, bool includeDatabaseAdmins, bool includeOperationsAdmins, bool includeSecurityAdmin, bool includeBetaUsers)
        {
            if (includeUsers && includeClientGroupAdmins && includeAccountAdmins && includeSystemAdmins && includeDatabaseAdmins && includeOperationsAdmins && includeSecurityAdmin && includeBetaUsers)
            {
                // Special case of no filtering required.
                return null;
            }

            List<AccessFilters> filters = new List<AccessFilters>();

            if (includeUsers)               { filters.Add(AccessFilters.Standard); }
            if (includeClientGroupAdmins)   { filters.Add(AccessFilters.GroupAdmin); }
            if (includeAccountAdmins)       { filters.Add(AccessFilters.AccountAdmin); }
            if (includeSystemAdmins)        { filters.Add(AccessFilters.SystemAdmin); }
            if (includeDatabaseAdmins)      { filters.Add(AccessFilters.DatabaseAdmin); }
            if (includeOperationsAdmins)    { filters.Add(AccessFilters.OperationsAdmin); }
            if (includeSecurityAdmin)       { filters.Add(AccessFilters.SecurityAdmin); }
            if (includeBetaUsers)           { filters.Add(AccessFilters.BetaUser); }

            return filters.ToArray();
        }


        public static int GetCount(AppDC accountsDC, Identity authorizedBy, string searchExpression, int? clientGroupID, AccessFilters[] accessFilters)
        {
//!! this routine doesn't correctly apply filters. 

            var queryExpression = new SearchExpression();
            var userQueryCount = User.Query(accountsDC, queryExpression)
                .Count();

            return userQueryCount;

            //!! nextgen - var userQuery = User.GetAll(authorizedBy, searchExpression, clientGroupID, accessFilters, sortExpression, startRowIndex, maximumRows);
#if false
            // basic query
            var query = GetAll(authorizedBy);

            // filtering
            query = filterBy(query, searchExpression, clientGroupID, accessFilters);

            // execute
            return query.Count();
#endif
        }
#if false

        public static IQueryable<User> GetAll(TorqIdentity authorizedBy)
        {
            Debug.Assert(authorizedBy != null);

            // basic query
            var query = TorqDataContext.AccountsOnlyDCInstance.Users
                .Select(user => user);

            if (authorizedBy.IsSystemAdmin)
            {
                // no restrictions
            }
            else if (authorizedBy.IsAccountAdmin)
            {
                // restrict to all clients w/in the account's hierarchy
                ClientInfo info = Client.GetCachedClientInfo(authorizedBy.ClientID);
                if (info != null && info.AncestorsAndSelf != null)
                {
                    List<int> clientIds = info.Root.DescendentsAndSelf.Select(c => c.ClientID).ToList();
                    query = query.Where(user => clientIds.Contains(user.ClientId));
                }
            }
            else if (authorizedBy.IsGroupAdmin || (authorizedBy.IsStandardUser && authorizedBy.PermissionCheck(Permission.SharedProjectAccess)))
            {
                // restrict to just one client
                query = query.Where(user => user.ClientId == authorizedBy.ClientID);
            }
            else
            {
                // restrict to just one user
                query = query.Where(user => user.UserID == authorizedBy.UserIDOrNull);
            }

            return query;
        }

#endif

#if false
        public struct UserDisplay
        {
            public int UserID { get; set; }
            public string DisplayName { get; set; }
        }

        public static IEnumerable<UserDisplay> GetUserDisplay(AppDC dc, int clientID)
        {
            //!! nextgen - verify we've got the correct

            var epUserQuery =
                from user in User.Query(dc)
                join epEmailAddress in ExtendedProperty.QueryEmailAddresses<MS.Utility.User>(dc, EPCategory.System, EPScope.Global) on user.ID equals epEmailAddress.ID into epEmailAddressJoin
                select new
                {
                    user,
                    EmailAddress = epEmailAddressJoin
                        .Select(epEmailAddressItem => epEmailAddressItem.EmailAddress.Address)
                        .FirstOrDefault(),
                };



            if (clientID == -1)
            {
                // We're looking accross multiple groups. Provide [group] or [group - email] as a way to differentiate any users with the same first/last name.
                var allClientResult = epUserQuery
                    .GroupBy(epUser => new { epUser.user.TenantID, epUser.user.FirstName, epUser.user.LastName })
                    .Select(epUserGroup => new
                    {
                        DisplayNames = epUserGroup.Select(epUser => new UserDisplay()
                        {
                            UserID = epUser.user.UserID,
                            DisplayName = epUser.user.FirstName + " " + epUser.user.LastName
                                + " [" + epUser.user.Tenant.Name
                                + (epUserGroup.Count() > 1 ? " - " + epUser.EmailAddress : string.Empty)
                                + "]",
                        }),
                    })
                    .SelectMany(userDisplay => userDisplay.DisplayNames)
                    .OrderBy(user => user.DisplayName);

                return allClientResult;
            }

            // We're only looking at a single group. Provide [email] as a way to differentiate any users with the same first/last name.
            var result = epUserQuery
                .Where(epUser => epUser.user.TenantID == clientID)
                .GroupBy(epUser => new { epUser.user.FirstName, epUser.user.LastName })
                .Select(epUserGroup => new
                {
                    DisplayNames = epUserGroup.Select(epUser => new UserDisplay()
                    {
                        UserID = epUser.user.UserID,
                        DisplayName = epUser.user.FirstName + " " + epUser.user.LastName +
                            (epUserGroup.Count() > 1 ? " [" + epUser.EmailAddress + "]" : string.Empty),
                    })
                })
                .SelectMany(userDisplay => userDisplay.DisplayNames)
                .OrderBy(user => user.DisplayName);

            return result;
        }

#endif

#if false

        public static IQueryable<User> GetAll(TorqIdentity authorizedBy, int clientGroupID)
        {
            try
            {
                return GetAll(authorizedBy, string.Empty, clientGroupID, null, string.Empty, 0, int.MaxValue);
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
                return null;
            }
        }

        public static IQueryable<User> GetAll(TorqIdentity authorizedBy, string searchExpression, int? clientGroupID, AccessFilters[] accessFilters, string sortExpression, int startRowIndex, int maximumRows)
        {
            // basic query
            var query = GetAll(authorizedBy);

            // filtering
            query = filterBy(query, searchExpression, clientGroupID, accessFilters);

            // sorting
            if (string.IsNullOrEmpty(sortExpression))
            {
                /* default should be by name */
                sortExpression = "Username";
            }

            query = query.SortBy(sortExpression);

            // paging
            query = query.Skip(startRowIndex).Take(maximumRows);

            // execute
            return query;
        }

        private static IQueryable<User> filterBy(IQueryable<User> query, string searchExpression, int? clientGroupID, AccessFilters[] accessFilters)
        {
            // if present, look for searchExpression
            if (!string.IsNullOrEmpty(searchExpression))
            {
                searchExpression = searchExpression.ToLower();
                //!! note this isn't very efficient (http://sqlblogcasts.com/blogs/simons/archive/2008/12/18/LINQ-to-SQL---Enabling-Fulltext-searching.aspx)
                query = query.Where(u => u.Username.ToLower().Contains(searchExpression) || (u.FirstName.ToLower() + " " + u.LastName.ToLower()).Contains(searchExpression));
            }

            // if present, Client Group must match
            if (clientGroupID.HasValue && clientGroupID.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientGroupID.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    query = query.Where(u => ids.Contains(u.ClientId));
                }
                else
                {
                    query = query.Where(u => clientGroupID == u.ClientId);
                }
            }

            // just the requested Access Permissions
            if (accessFilters != null)
            {
                query = query.Where(u =>
                    (accessFilters.Contains(AccessFilters.Standard) && (u.Access == null || u.Access == "" || u.Access.Contains(AccessFilters.Standard.ToString()))) ||
                    (accessFilters.Contains(AccessFilters.GroupAdmin) && u.Access.Contains(AccessFilters.GroupAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.AccountAdmin) && u.Access.Contains(AccessFilters.AccountAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.SystemAdmin) && u.Access.Contains(AccessFilters.SystemAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.DatabaseAdmin) && u.Access.Contains(AccessFilters.DatabaseAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.OperationsAdmin) && u.Access.Contains(AccessFilters.OperationsAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.SecurityAdmin) && u.Access.Contains(AccessFilters.SecurityAdmin.ToString())) ||
                    (accessFilters.Contains(AccessFilters.BetaUser) && u.Access.Contains("domain:beta")));
            }

            return query;
        }


        public struct UserUsage
        {
            public User User { get; private set; }
            public UserSessionItem LastSession { get; private set; }

            internal UserUsage(User user, UserSessionItem userSessionItem)
                : this()
            {
                this.User = user;
                this.LastSession = userSessionItem;
            }
        }

#endif
        public static object[] GetUserUsage(AppDC accountsDC, Identity authorizedBy, string searchExpression, int? clientGroupID, AccessFilters[] accessFilters, string sortExpression, int startRowIndex, int maximumRows)
        {
            SiteContext siteContext = SiteContext.Current;

            if (sortExpression == "Name")
            {
                // fudge here - we can't sort by Name since that's not a field in the DB.
                sortExpression = "FirstName";
            }

            string siteName = siteContext.SiteName;
            if (siteName == "Production")
            {
                siteName = null;
            }

            var queryExpression = new SearchExpression();
            var userQuery = User.Query(accountsDC, queryExpression, sortExpression, startRowIndex, maximumRows);
            //!! nextgen - var userQuery = User.GetAll(authorizedBy, searchExpression, clientGroupID, accessFilters, sortExpression, startRowIndex, maximumRows);

            var userUsageQuery = UserSessionItem.GetLatestSessionStart(accountsDC)
                .Select(userSessionItem => new
                {
                    UserID = userSessionItem.AuthenticatedID,

                    userSessionItem.LastModifiedTimestamp,
                    userSessionItem.LastModifiedTimestampLocal,
                    userSessionItem.LocalTimeZoneIndex,
                    //userSessionItem.SessionStartTimestamp,

                    //!! nextgen userSessionItem.Timestamp,
                    //userSessionItem.TimestampLocal,
                });


            var epUserRoleServerQuery =
                from user in userQuery

                join epSystemRoleTag in User.QueryGlobalEPTags2(accountsDC, EPCategory.SystemRoleCategory) on user.ID equals epSystemRoleTag.TargetID into epSystemRoleTagGroup
                from epSystemRoleTag in epSystemRoleTagGroup.DefaultIfEmpty()

                //!! nextgen !! we shouldn't expose this QueryAssignedTags call - I think we need to search for Users and then join to that...
                join epAppRoleTag in ExtendedProperty.QueryAssignedTags(accountsDC, typeof(User), EPCategory.AppRoleCategory, ExtendedPropertyScopeType.TenantGroupID)
                    on new { user.ID, TenantGroupID = user.TenantGroupID } equals new { ID = epAppRoleTag.ExtendedProperty.TargetID, TenantGroupID = epAppRoleTag.ExtendedProperty.ScopeID } into epAppRoleTagGroup
                from epAppRoleTag in epAppRoleTagGroup.DefaultIfEmpty()

                select new
                {
                    UserID = user.ID,

                    SystemRoleName = epSystemRoleTag.Item.Name,
                    AppRoleName = epAppRoleTag.Tag.Name,
                };

            var epUserRoles = epUserRoleServerQuery
                // (force the query here - and do the remaining work client-side)
                .AsEnumerable()
                .GroupBy(epUserRole => epUserRole.UserID)
                .Select(epUserRoleGroup => new
                {
                    UserID = epUserRoleGroup.Key,

                    RoleNames = epUserRoleGroup.Select(epUserRole => epUserRole.SystemRoleName).Distinct()
                        .Concat(epUserRoleGroup.Select(epUserRole => epUserRole.AppRoleName).Distinct())
                        // filter out any string.Empty names - from our DefaultIfEmpty() query
                        .Where(roleName => !string.IsNullOrEmpty(roleName))
                        .Join(", "),
                });


#if false
//!! works, but causes an extra SQL roundtrip for each usage call
            var serverQuery = from user in userQuery
                              select new
                              {
                                  user.UserID,
                                  user.FirstName,
                                  user.LastName,
                                  user.Email,
                                  user.Access,
                                  user.DefaultProductToken,
                                  UserTimeZone = user.TimeZone,
                                  ClientTimeZone = user.Client.TimeZone,
                                  LastSession = usageQuery
                                    .FirstOrDefault(userSessionItem => userSessionItem.UserID == user.UserID),
                              };
#endif
            var epUserServerQuery =
                from user in userQuery
                join epEmailAddress in User.QueryOrderedEPEmailAddresses(accountsDC) on user.ID equals epEmailAddress.ID into epUserEmailAddressGroup

                join epDefaultProductTag in User.QueryTeamEPTags2(accountsDC, ExtendedUser.UserDefaultProductCategory) on user.ID equals epDefaultProductTag.TargetID into epDefaultProductTagGroup


                join userUsage in userUsageQuery on user.ID equals userUsage.UserID into userUsageGroup
                from userUsage in userUsageGroup.DefaultIfEmpty().Take(1)
                select new
                {
                    UserID = user.ID,
                    user.FirstName,
                    user.LastName,

                    Email = epUserEmailAddressGroup
                        .Select(epEmailAddressItem => epEmailAddressItem.EmailAddress.Address)
                        .FirstOrDefault(),
                    //!! nextgen user.Email,

                    //!! nextgen - user.Access,
                    Access = "",

                    //SystemRoleNames = epSystemRoleTagsGroup,
                        //.Select(epSystemRoleTag => epSystemRoleTag.Tag.Name),

                    //AppRoleNames = epAppRoleTagsGroup,
                        //.Select(epAppRoleTag => epAppRoleTag.Tag.Name),


                    //!! nextgen - user.DefaultProductToken,
                    DefaultProductName = epDefaultProductTagGroup
                        .Select(epDefaultProductTag => epDefaultProductTag.Item.Name)
                        .FirstOrDefault(),

                    UserTimeZoneIndex = user.TimeZoneIndex,
                    TenantGroupID = user.TenantGroupID,
                    //TenantID = user.TenantID,

                    //!! ClientTimeZone = user.Client.TimeZone,
                    LastSession = userUsage,
                    //!! Client = user.Client
                };

            // add in a local query to join with some in-memory data
            var localQuery =
                from epUser in epUserServerQuery.AsEnumerable()
                join userRole in epUserRoles on epUser.UserID equals userRole.UserID
                join tenantGroupInfo in TenantGroup.GetCachedTenantGroupInfo() on epUser.TenantGroupID equals tenantGroupInfo.TenantGroupID

                select new { epUser, userRole, tenantGroupInfo };

            var resultQuery = localQuery
                // force the query here as we're using some methods below that don't translate to SQL
                //.AsEnumerable()
                .Select(localResult => new
                {
                    localResult.epUser.UserID,
                    localResult.epUser.Email,

                    FullName = ExtendedUser.GetFullName(localResult.epUser.FirstName, localResult.epUser.LastName),

                    RolesDisplay = localResult.userRole.RoleNames,
                    //localResult.userUsage.SystemRoleNames.Select(aaa => aaa.TagName)
                        //.Concat(localResult.userUsage.AppRoleNames.Select(bbb => "piss" /* bbb.TagName */))
                        //.Join(", "),

                    //!! nextgen - changed from Permissions to Roles ExtendedUser.GetPermissionsDisplay(localResult.userUsage.Access),

                    //!! DefaultProduct = localResult.epUser.DefaultProductName.ParseAsEnum<Product>(Product.cTORQ),
                    DomainRestriction = ExtendedUser.GetAccessTag(localResult.epUser.Access, "domain"),

                    LastAccessLocal = localResult.epUser.LastSession != null ? localResult.epUser.LastSession.LastModifiedTimestampLocal.ToString() : string.Empty,
                    LastAccessRelative = localResult.epUser.LastSession != null ? DateTime.SpecifyKind(localResult.epUser.LastSession.LastModifiedTimestamp, DateTimeKind.Utc).ToNowRelativeString(ExtendedUser.GetLocalTimeZoneInfo(localResult.epUser.UserTimeZoneIndex, localResult.epUser.TenantGroupID)) : string.Empty,
                    LastAccessTimeZone = localResult.epUser.LastSession != null ? TimeZones.GetTimeZoneInfo(localResult.epUser.LastSession.LocalTimeZoneIndex).DisplayName : string.Empty,

                    localResult.epUser.TenantGroupID,
                })
                .Cast<object>()
                .ToArray();

            HttpContext.Current.LogPerformanceCheckpoint("GetUsersPage() query complete");

            return resultQuery;
        }

        public static object[] GetUsersPage(Identity authorizedBy, string searchExpression, int? clientGroupID, bool includeUsers, bool includeClientGroupAdmins, bool includeAccountAdmins, bool includeSystemAdmins, bool includeDatabaseAdmins, bool includeOperationsAdmins, bool includeSecurityAdmins, bool includeBetaUsers, string sortExpression, int startRowIndex, int maximumRows)
        {
            var accessFilters = ExtendedUser.GetAccessFilters(includeUsers, includeClientGroupAdmins, includeAccountAdmins, includeSystemAdmins, includeDatabaseAdmins, includeOperationsAdmins, includeSecurityAdmins, includeBetaUsers);

            using (var accountsDC = SiteContext.Current.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                var query = ExtendedUser.GetUserUsage(accountsDC, authorizedBy, searchExpression, clientGroupID, accessFilters, sortExpression, startRowIndex, maximumRows);
                return query;
            }
        }

        public static int GetUsersPageCount(Identity authorizedBy, string searchExpression, int? clientGroupID, bool includeUsers, bool includeClientGroupAdmins, bool includeAccountAdmins, bool includeSystemAdmins, bool includeDatabaseAdmins, bool includeOperationsAdmins, bool includeSecurityAdmins, bool includeBetaUsers)
        {
            var accessFilters = ExtendedUser.GetAccessFilters(includeUsers, includeClientGroupAdmins, includeAccountAdmins, includeSystemAdmins, includeDatabaseAdmins, includeOperationsAdmins, includeSecurityAdmins, includeBetaUsers);

            // forward on...
            using (var accountsDC = SiteContext.Current.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                int count = ExtendedUser.GetCount(accountsDC, authorizedBy, searchExpression, clientGroupID, accessFilters);
                return count;
            }
        }

#if false
        public class UsageStatsResult
        {
            public int UserID { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string DomainRestriction { get; set; }
            public Client Client { get; set; }
            public TimeZoneInfo TimeZoneInfo { get; set; }
            public DateTime? LastProjectTimestamp { get; set; }
            public int ProjectCount0To30 { get; set; }
            public int ProjectCount31To60 { get; set; }
            public int ProjectCount61To90 { get; set; }
            public int ProjectCountOver90 { get; set; }
        }
        public static IEnumerable<UsageStatsResult> GetUsageStats(TorqIdentity authorizedBy, string searchExpression, int? clientGroupID, bool includeUsers, bool includeClientGroupAdmins, bool includeAccountAdmins, bool includeSystemAdmins, bool includeDatabaseAdmins, bool includeOperationsAdmins, bool includeSecurityAdmins, bool includeBetaUsers, string sortExpression, int startRowIndex, int maximumRows)
        {
            var accessFilters = User.GetAccessFilters(includeUsers, includeClientGroupAdmins, includeAccountAdmins, includeSystemAdmins, includeDatabaseAdmins, includeOperationsAdmins, includeSecurityAdmins, includeBetaUsers);

            //var userQuery = GetAll(authorizedBy, searchExpression, clientGroupID, accessFilters, sortExpression, startRowIndex, maximumRows, false)
            var userQuery = GetAll(authorizedBy, searchExpression, clientGroupID, accessFilters, sortExpression, startRowIndex, maximumRows)
                .AsEnumerable();

            //!! need to handle offset for timezone here somehow.
            DateTime dtToday = DateTime.Now.Date;
            DateTime dt1DayAgo = dtToday.AddDays(-1);
            DateTime dt30DaysAgo = dt1DayAgo.AddDays(-29);
            DateTime dt60DaysAgo = dt30DaysAgo.AddDays(-30);
            DateTime dt90DaysAgo = dt60DaysAgo.AddDays(-30);

            var resultQuery = from user in userQuery
                              select new UsageStatsResult
                              {
                                  UserID = user.UserID,
                                  Name = user.FirstName + " " + user.LastName,
                                  Username = user.Username,
                                  Client = user.Client,
                                  DomainRestriction = User.GetAccessTag(user.Access, "domain"),
                                  LastProjectTimestamp = TorqDataContext.AccountsOnlyDCInstance.Projects
                                      .Where(p => p.OwnerUserID == user.UserID)
                                      .Select(p => p.CreateDate as DateTime?)
                                      .Max(),
                                  TimeZoneInfo = user.TimeZoneInfo,
                                  ProjectCount0To30 = TorqDataContext.AccountsOnlyDCInstance.Projects
                                      .Where(p => p.OwnerUserID == user.UserID)
                                      .Where(p => dt30DaysAgo <= p.CreateDate.Date)
                                      .Count(),
                                  ProjectCount31To60 = TorqDataContext.AccountsOnlyDCInstance.Projects
                                      .Where(p => p.OwnerUserID == user.UserID)
                                      .Where(p => dt60DaysAgo <= p.CreateDate.Date && p.CreateDate.Date < dt30DaysAgo)
                                      .Count(),
                                  ProjectCount61To90 = TorqDataContext.AccountsOnlyDCInstance.Projects
                                      .Where(p => p.OwnerUserID == user.UserID)
                                      .Where(p => dt90DaysAgo <= p.CreateDate.Date && p.CreateDate.Date < dt60DaysAgo)
                                      .Count(),
                                  ProjectCountOver90 = TorqDataContext.AccountsOnlyDCInstance.Projects
                                      .Where(p => p.OwnerUserID == user.UserID)
                                      .Where(p => p.CreateDate.Date < dt90DaysAgo)
                                      .Count(),
                              };

            // execute
            return resultQuery;
        }

        public static int GetUsageStatsCount(TorqIdentity authorizedBy, string searchExpression, int? clientGroupID, bool includeUsers, bool includeClientGroupAdmins, bool includeAccountAdmins, bool includeSystemAdmins, bool includeDatabaseAdmins, bool includeOperationsAdmins, bool includeSecurityAdmins, bool includeBetaUsers)
        {
            var accessFilters = User.GetAccessFilters(includeUsers, includeClientGroupAdmins, includeAccountAdmins, includeSystemAdmins, includeDatabaseAdmins, includeOperationsAdmins, includeSecurityAdmins, includeBetaUsers);

            // forward on...
            //!!return GetCount(authorizedBy, searchExpression, clientGroupID, accessFilters, false);
            return GetCount(authorizedBy, searchExpression, clientGroupID, accessFilters);
        }
#endif
        /// <summary>
        /// Get user signature from UserSettings
        /// </summary>
        /// <returns></returns>
        public static string GetSignature(AppDC dc, Identity authorizedBy)
        {
            if (authorizedBy == null || !authorizedBy.IsUserAuthenticated)
            {
                return null;
            }

            var extendedUser = ExtendedUser.FindByID(dc, authorizedBy.UserIDOrThrow);
            if (extendedUser != null)
            {
                return extendedUser.GetSignature();
            }

            return null;
        }
        /// <summary>
        /// Get user signature from UserSettings
        /// </summary>
        /// <returns></returns>
        public string GetSignature()
        {
            Debug.Assert(this.customPropertiesData != null);

            var signature = this.customPropertiesData.Signature;
            if (signature != null && signature.Enabled)
            {
                return signature.Value;
            }
            return null;
        }
#if false
        public static WebServiceStatus SendPasswordResetEmail(MailAddress mailAddress)
        {
            User user = User.GetByEmail(mailAddress);
            if (user == null)
            {
                return WebServiceStatus.InvalidEmail;
            }

            if (user.IsAccountAdmin || user.IsSecurityAdmin)
            {
                // For security, disallow these roles from resetting their passwords via email.
                return WebServiceStatus.PrivilegedEmail;
            }

            if (string.IsNullOrEmpty(user.HashedPassword))
            {
                // Disallow users that don't already have a password (such as SSO users) from resetting their passwords via email.
                return WebServiceStatus.PrivilegedEmail;
            }

            // Get an authenticated URL - which allows the password on this account to be reset
            Uri passwordResetUrl = PasswordResetAuthToken.ExpireAndCreateNewUrl(user.ClientId, user.UserID);

            string htmlBodyTemplate =
@"<p>Click this link to reset your TORQ password.</p>
<p>{0}</p>
<p>All the best.<br/>
The TORQ Team</p>";

            TorqContext torq = TorqContext.Current;

            var passwordResetEmail = new MailMessageBuilder();
            passwordResetEmail.To.Add(mailAddress);
            passwordResetEmail.Subject = "Reset your TORQ password";
            //passwordResetEmail.BodyEncoding = Encoding.UTF8;
            //passwordResetEmail.IsBodyHtml = true;
            var htmlBody = string.Format(htmlBodyTemplate,
                /*0*/ passwordResetUrl.ToString());
            passwordResetEmail.SetHtmlView(htmlBody);
            passwordResetEmail.Headers.Add("X-PostmarkTag", "PasswordReset");

            var torqIdentity = user.GetUserIdentity();

            string description = string.Format("User {0} requested a password reset email",
                /*0*/ user.UserID.ToString());

            torq.RawSendEmail(torqIdentity, passwordResetEmail, description, (sentMessage, torqDC, outboundEmail) =>
            {
                var activityItem = ActivityItem.Log(torqDC, outboundEmail.SentTimestamp, torqIdentity, (int)ActivityType.UserInitiatedPasswordReset, description, typeof(User).Name, user.UserID);
                outboundEmail.ActivityItem = activityItem;
            });

            //log reset request
            torq.EventLog.LogInformation("User ID: " + user.UserID.ToString() + " requested a reset password email.");

            return WebServiceStatus.Ok;
        }
#endif
    }
}