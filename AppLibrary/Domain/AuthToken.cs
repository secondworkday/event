using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace App.Library
{


    /// <summary>
    /// Supplies the ability to login to the application. Supports both "User" and "Object" logins
    /// </summary>
    public class ItemPinAuthTemplate : WebAuthTemplate
    {
        private const string AUTH_TOKEN_TYPE_NAME = "ItemPin";
        private const string AUTH_CODE_PATTERN = "####";

        public static readonly TimeSpan? defaultDuration = null;

        //!! thoughts on the right URL?
        public static ItemPinAuthTemplate Instance = new ItemPinAuthTemplate(defaultDuration, "/pin/", AUTH_CODE_PATTERN, true, "/goober");

        /*
                // We use XML to persist the fields in this Data class.
                public class Data : XmlSerializableClass<Data>
                {
                    public string TargetTable { get; set; }
                    public int TargetID { get; set; }

                    public Data()
                    {
                        Debug.Assert(!string.IsNullOrEmpty(this.TargetTable));
                        Debug.Assert(this.TargetID > 0);
                    }
                }
        */

        protected ItemPinAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }

        public static Uri RevokeAndCreateNewUserUrl(WebUtilityDC dc, int userID, int usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need a permission check?

            var userLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(dc, () => QueryValidUsers(dc, userID), () => CreateAuthToken(dc, typeof(User), userID, usageQuota, validDuration));

            var url = Instance.GetUrl(userLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }


#if false
        public static Uri RevokeAndCreateNewItemUrl(AppDC accountsDC, Type targetType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
                //return null;
            //}

            var projectLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration));

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        public static Uri ExtendOrCreateNewItemUrl(AppDC accountsDC, Type targetType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
                //return null;
            //}

            var projectLoginAuthToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidEventSessions(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration), validDuration);

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }
#endif


        public static AuthToken ExtendOrCreateNewEventSession(AppDC accountsDC, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
            //return null;
            //}

            var authToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidEventSessions(accountsDC, targetID), () => CreateAuthToken(accountsDC, typeof(EventSession), targetID, usageQuota, validDuration), validDuration);
            return authToken;
        }




        public static AuthToken ExtendOrCreateNew<T>(AppDC accountsDC, T item, int? usageQuota, TimeSpan? validDuration)
            where T : IObject
        {
            var authToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidItems(accountsDC, item), () => CreateAuthToken(accountsDC, typeof(T), item.ObjectID, usageQuota, validDuration), validDuration);
            return authToken;
        }

        public static Uri ExtendOrCreateNewUrl<T>(AppDC accountsDC, T item, int? usageQuota, TimeSpan? validDuration)
            where T : IObject
        {
            var authToken = ExtendOrCreateNew(accountsDC, item, usageQuota, validDuration);
            var url = Instance.GetUrl(authToken, UtilityContext.Current.RequireSslLogin);
            return url;
        }












        //!! think we can switch over from "Project" to "Event"!!!!
#if false
        public static Uri RevokeAndCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration));

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        public static Uri ExtendOrCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration), validDuration);

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }
#endif

        private static AuthToken CreateAuthToken(WebUtilityDC dc, Type targetTableType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            // Login tokens are defined within a Group scope (either TenantGroup or UserGroup)
            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

            var issuedByUserID = dc.TransactionAuthorizedBy.UserIDOrNull;

            var authToken = AuthToken.Create(dc, AUTH_CODE_PATTERN, authCode => AuthToken.Create(dc, teamEPScope, AUTH_TOKEN_TYPE_NAME, authCode, targetTableType, targetID, null, usageQuota, validDuration, issuedByUserID));
            return authToken;
        }


        public static IQueryable<AuthToken> Query(UtilityDC dc)
        {
            return AuthTemplate.QueryAuthTokens(dc, AUTH_TOKEN_TYPE_NAME);
        }
        public static IQueryable<AuthToken> QueryValid(UtilityDC dc)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, AUTH_TOKEN_TYPE_NAME);
        }


        /*

                public static Uri ExpireAndCreateNewUserUrl(int clientID, int userID, TimeSpan? validDuration)
                {
                    return ExpireAndCreateNewUser(clientID, userID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
                }

                public static Uri ExpireAndCreateNewProjectUrl(int clientID, int projectID, TimeSpan? validDuration)
                {
                    return ExpireAndCreateNewProject(clientID, projectID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
                }

                private static LoginAuthToken ExpireAndCreateNewUser(int clientID, int userID, TimeSpan? validDuration)
                {
                    return AuthToken.ExpireAndCreateNew(() => FindValidUser(clientID, userID), () => CreateNew(clientID, userID, null, validDuration));
                }

                private static AuthToken ExpireAndCreateNewProject(UtilityDC dc, int clientID, int projectID, TimeSpan? validDuration)
                {
                    return AuthToken.ExpireAndCreateNew(() => FindValidProject(dc, projectID), () => CreateNew(clientID, null, projectID, validDuration));
                }

                private static AuthToken CreateNew(int clientID, int? userID, int? projectID, TimeSpan? validDuration)
                {
                    return AuthToken.CreateNew(authCode => new LoginAuthToken(authCode, clientID, userID, projectID, validDuration));
                }
        */
        public static IQueryable<AuthToken> QueryValidUsers(UtilityDC dc, int userID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, AUTH_TOKEN_TYPE_NAME)
                .Where(authToken => authToken.TargetTable == typeof(User).Name)
                .Where(authToken => authToken.TargetID == userID);
        }

        //!! we can specific ExtendedProperties on things that aren't actually database types
        //   


        public static IQueryable<AuthToken> QueryValidItems<T>(UtilityDC dc, T item)
            where T : IObject
        {
            return AuthTemplate.QueryValidAuthTokens(dc, AUTH_TOKEN_TYPE_NAME)
                .Where(authToken => authToken.TargetTable == typeof(T).Name)
                .Where(authToken => authToken.TargetID == item.ObjectID);
        }


        public static IQueryable<AuthToken> QueryValidEventSessions(UtilityDC dc, int targetID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, AUTH_TOKEN_TYPE_NAME)
                .Where(authToken => authToken.TargetTable == typeof(EventSession).Name)
                .Where(authToken => authToken.TargetID == targetID);
        }


#if false
        public static IQueryable<AuthToken> QueryValidProjects(UtilityDC dc, int projectID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(Project).Name)
                .Where(authToken => authToken.TargetID == projectID);
        }
#endif

        /*
        public Uri CreateSingle(WebUtilityDC dc, int userID)
        {
            var utilityContext = UtilityContext.Current;

            var passwordResetAuthToken = CreateSingleAuthToken(dc, userID);

            if (utilityContext.RequireSslLogin)
            {
                var secureUrl = GetSecureUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
                return secureUrl;
            }

            var url = GetUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
            return url;
        }
*/


        protected override HubResult OnRedeemed(UtilityDC utilityDC, AuthToken authToken, dynamic data)
        {
            if (authToken.Redeem(utilityDC))
            {
                var authTokenEPScope = authToken.ItemEPScope;
                Debug.Assert(authTokenEPScope.ScopeType == ExtendedPropertyScopeType.TenantGroupID);
                Debug.Assert(authToken.TargetID.HasValue);


                // Our item's tenant is our tenant
                var tenantGroupID = authTokenEPScope.ID.Value;
                var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantGroupID);

                Debug.Assert(authToken.ContextUserID.HasValue);
                var authTokenContextUserID = authToken.ContextUserID.Value;

                var authorizedBy = utilityDC.TransactionAuthorizedBy;
                var authTokenAuthorizingIdentity = authorizedBy.GetImpersonatedIdentity(utilityDC, authTokenContextUserID);

                var appRoles = new string[] { "EventSessionVolunteer" };

                // If we're given a firstName/lastName, we'll have a User Volunteer. Otherwise an Anonymous Volunteer
                string firstName = data.firstName;
                string lastName = data.lastName;

                //!! do we need a non-null display name?
                string identityDisplayName = null;
                int? identityUserID = null;
                TimeZoneInfo identityTimeZoneInfo = tenantGroupInfo.TimeZoneInfo;

                User volunteerUser = null;
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    var trimFirstName = firstName.Trim();
                    var trimLastName = lastName.Trim();

                    volunteerUser = User.FindOrCreateVisitor(utilityDC, authTokenAuthorizingIdentity, trimFirstName, trimLastName, appRoles);
                    Debug.Assert(volunteerUser != null);
                    Debug.Assert(volunteerUser.ID > 0);

                    identityDisplayName = volunteerUser.DisplayName;
                    identityUserID = volunteerUser.ID;
                    identityTimeZoneInfo = volunteerUser.EffectiveTimeZoneInfo;
                }

                // If they authenticate this way, we only allow these roles. (Log in normally if your User account has greater permissions)
                var systemRoles = Enumerable.Empty<SystemRole>();

                IdentityData identityData = IdentityData.Create(authTokenEPScope, tenantGroupID, identityUserID, identityDisplayName, systemRoles, appRoles, null, identityTimeZoneInfo);

                //var systemIdentity = authorizedBy.GetSystemIdentity(dc, tenantGroupInfo);
                //Debug.Assert(systemIdentity != null);

                utilityDC.UsingIdentity<AppDC>(authTokenAuthorizingIdentity, authTokenIdentityDC =>
                {
                    if (authToken.TargetID.HasValue)
                    {
                        WebIdentityAuthentication.StartObjectSession(authTokenIdentityDC, identityData, TimeZones.Pacific, tenantGroupID, identityUserID, typeof(EventSession), authToken.TargetID.Value, authToken.Type, authToken.ID);
                    }
                });
                return HubResult.Success;
            }
            return HubResult.Forbidden;
        }



        protected override AuthTokenState OnTokenClicked(HttpContext context, WebUtilityDC dc, AuthToken authToken)
        {
            UtilityContext utilityContext = UtilityContext.Current;

            //!! nextgen - do we require an indication on the AuthToken if it requires SSL?

            if (utilityContext.RequireSslLogin)
            {
                // (If we redirect here, the link the user clicked on will remain in the browser URL)
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            var authTokenState = base.OnTokenClicked(context, dc, authToken);



            if (authTokenState == AuthTokenState.Valid)
            {
                //!! how do we pass firstName/lastName? Is the intent to show UI? Or specify the volunteer Name in the authToken? Guess if we have it, we should pass a userID. 
                var result = OnRedeemed(dc, authToken, new {});

                if (result == HubResult.Success)
                {
                    //!! hmm should we always jump to the SPA, success or no on redeeming?
                    context.Response.Redirect("/Spas/VolunteerSpa.aspx");
                    Debug.Fail("Shouldn't get here...");
                }
            }

            // (this page handles both valid and invalid clicks)

            string queryString = string.Format("authCode={0}",
                /*0*/ FormatAuthCode(authToken.AuthCode, AuthCodeFormat.Url));

            context.RewritePath("/ResetPassword.aspx", string.Empty, queryString);

            return authTokenState;
        }


        /*
                private LoginAuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, TimeSpan? validDuration)
                    : base(authCode, clientID, userID, projectID, NullSettings, validDuration)
                {
                    // Login as a user or to a project, but not both
                    Debug.Assert(userID.HasValue ^ projectID.HasValue);
                }
        */

        /*



                public Uri SecureUrl
                {
                    get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
                }
        */
        /*
                protected override void OnClicked(HttpContext context, bool isValid)
                {
                    TorqContext torq = TorqContext.Current;

                    base.OnClicked(context, isValid);

                    if (isValid)
                    {
                        if (torq.RequireSslLogin)
                        {
                            context.RedirectToSecureConnection();
                            Debug.Assert(context.Request.IsSecureConnection);
                        }

                        // Expire - as this is a one-shot AuthToken
                        this.expireAndUpdateDB();

                        Debug.Assert(this.AuthUserID.HasValue ^ this.AuthProjectID.HasValue);

                        if (this.AuthUserID.HasValue)
                        {
                            TorqUserAuthentication.LoginAuthTokenSession(this.AuthUser, Product.cTORQ);

                            //!! is this really where we want to redirect?
                            // Redirect - so the client can save the AuthCookie
                            string qs = context.Request.QueryString.ToString();
                            context.Response.Redirect("/project/" + qs);
                            Debug.Fail("Shouldn't get here...");
                        }

                        if (this.AuthProjectID.HasValue)
                        {
                            TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                            //!! is this really where we want to redirect?
                            // Redirect - so the client can save the AuthCookie
                            string qs = context.Request.QueryString.ToString();
                            if (qs.Length > 0)
                            {
                                qs = "&" + qs;
                            }
                            context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                            Debug.Fail("Shouldn't get here...");
                        }
                    }

                    string requestUrlString = context.Request.Url.ToString();

                    context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
                    Debug.Fail("Shouldn't get here...");
                }
        */
        /*
                public override bool IsValid
                {
                    get
                    {
                        if (this.AuthProject == null && this.AuthUser == null)
                        {
                            return false;
                        }
                        return base.IsValid;
                    }
                }
        */
    }









    /// <summary>
    /// Supplies the ability to login to the application. Supports both "User" and "Object" logins
    /// </summary>
    public class LoginAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "Login";
        private const string authCodePattern = "@##-&&&&-10#";

        public static readonly TimeSpan? defaultDuration = null;

        public static LoginAuthTemplate Instance = new LoginAuthTemplate(defaultDuration, "/login/", authCodePattern, true, "/goober");

/*
        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string TargetTable { get; set; }
            public int TargetID { get; set; }

            public Data()
            {
                Debug.Assert(!string.IsNullOrEmpty(this.TargetTable));
                Debug.Assert(this.TargetID > 0);
            }
        }
*/

        protected LoginAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }

        public static Uri RevokeAndCreateNewUserUrl(WebUtilityDC dc, int userID, int usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need a permission check?

            var userLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(dc, () => QueryValidUsers(dc, userID), () => CreateAuthToken(dc, typeof(User), userID, usageQuota, validDuration));

            var url = Instance.GetUrl(userLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }


#if false
        public static Uri RevokeAndCreateNewItemUrl(AppDC accountsDC, Type targetType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
                //return null;
            //}

            var projectLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration));

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        public static Uri ExtendOrCreateNewItemUrl(AppDC accountsDC, Type targetType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
                //return null;
            //}

            var projectLoginAuthToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidEventSessions(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration), validDuration);

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }
#endif


        public static AuthToken ExtendOrCreateNewEventSession(AppDC accountsDC, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?

            //var authCheckProject = Project.Get(accountsDC, projectID);
            //if (authCheckProject == null)
            //{
            //return null;
            //}

            var authToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidEventSessions(accountsDC, targetID), () => CreateAuthToken(accountsDC, typeof(EventSession), targetID, usageQuota, validDuration), validDuration);
            return authToken;
        }




        public static AuthToken ExtendOrCreateNew<T>(AppDC accountsDC, T item, int? usageQuota, TimeSpan? validDuration)
            where T : IObject
        {
            var authToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidItems(accountsDC, item), () => CreateAuthToken(accountsDC, typeof(T), item.ObjectID, usageQuota, validDuration), validDuration);
            return authToken;
        }

        public static Uri ExtendOrCreateNewUrl<T>(AppDC accountsDC, T item, int? usageQuota, TimeSpan? validDuration)
            where T : IObject
        {
            var authToken = ExtendOrCreateNew(accountsDC, item, usageQuota, validDuration);
            var url = Instance.GetUrl(authToken, UtilityContext.Current.RequireSslLogin);
            return url;
        }












        //!! think we can switch over from "Project" to "Event"!!!!
#if false
        public static Uri RevokeAndCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration));

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        public static Uri ExtendOrCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration), validDuration);

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }
#endif

        private static AuthToken CreateAuthToken(WebUtilityDC dc, Type targetTableType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            // Login tokens are defined within a Group scope (either TenantGroup or UserGroup)
            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

            var issuedByUserID = dc.TransactionAuthorizedBy.UserIDOrNull;

            var authToken = AuthToken.Create(dc, authCodePattern, authCode => AuthToken.Create(dc, teamEPScope, LoginAuthTemplate.authTokenTypeName, authCode, targetTableType, targetID, null, usageQuota, validDuration, issuedByUserID));
            return authToken;
        }


        public static IQueryable<AuthToken> Query(UtilityDC dc)
        {
            return AuthTemplate.QueryAuthTokens(dc, LoginAuthTemplate.authTokenTypeName);
        }
        public static IQueryable<AuthToken> QueryValid(UtilityDC dc)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName);
        }


/*

        public static Uri ExpireAndCreateNewUserUrl(int clientID, int userID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewUser(clientID, userID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewProjectUrl(int clientID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewProject(clientID, projectID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        private static LoginAuthToken ExpireAndCreateNewUser(int clientID, int userID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidUser(clientID, userID), () => CreateNew(clientID, userID, null, validDuration));
        }

        private static AuthToken ExpireAndCreateNewProject(UtilityDC dc, int clientID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidProject(dc, projectID), () => CreateNew(clientID, null, projectID, validDuration));
        }

        private static AuthToken CreateNew(int clientID, int? userID, int? projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new LoginAuthToken(authCode, clientID, userID, projectID, validDuration));
        }
*/
        public static IQueryable<AuthToken> QueryValidUsers(UtilityDC dc, int userID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(User).Name)
                .Where(authToken => authToken.TargetID == userID);
        }

        //!! we can specific ExtendedProperties on things that aren't actually database types
        //   


        public static IQueryable<AuthToken> QueryValidItems<T>(UtilityDC dc, T item)
            where T : IObject
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(T).Name)
                .Where(authToken => authToken.TargetID == item.ObjectID);
        }


        public static IQueryable<AuthToken> QueryValidEventSessions(UtilityDC dc, int targetID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(EventSession).Name)
                .Where(authToken => authToken.TargetID == targetID);
        }


#if false
        public static IQueryable<AuthToken> QueryValidProjects(UtilityDC dc, int projectID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(Project).Name)
                .Where(authToken => authToken.TargetID == projectID);
        }
#endif

/*
        public Uri CreateSingle(WebUtilityDC dc, int userID)
        {
            var utilityContext = UtilityContext.Current;

            var passwordResetAuthToken = CreateSingleAuthToken(dc, userID);

            if (utilityContext.RequireSslLogin)
            {
                var secureUrl = GetSecureUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
                return secureUrl;
            }

            var url = GetUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
            return url;
        }
*/



        protected override AuthTokenState OnTokenClicked(HttpContext context, WebUtilityDC dc, AuthToken authToken)
        {
            UtilityContext utilityContext = UtilityContext.Current;

            //!! nextgen - do we require an indication on the AuthToken if it requires SSL?

            if (utilityContext.RequireSslLogin)
            {
                // (If we redirect here, the link the user clicked on will remain in the browser URL)
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            var authTokenState = base.OnTokenClicked(context, dc, authToken);


            var authorizedBy = dc.TransactionAuthorizedBy;

            if (authTokenState == AuthTokenState.Valid)
            {
                var authTokenEPScope = authToken.ItemEPScope;
                Debug.Assert(authTokenEPScope.ScopeType == ExtendedPropertyScopeType.TenantGroupID);
                Debug.Assert(authToken.TargetID.HasValue);


                // Impersonate the TenantGroup "System" - so we're contrained to the correct EPScope
                var tenantGroupID = authTokenEPScope.ID.Value;
                var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantGroupID);


                Debug.Assert(authToken.ContextUserID.HasValue);
                var contextUserID = authToken.ContextUserID.Value;

                var authTokenAuthorizingIdentity = authorizedBy.GetImpersonatedIdentity(dc, contextUserID);


                //var systemIdentity = authorizedBy.GetSystemIdentity(dc, tenantGroupInfo);
                //Debug.Assert(systemIdentity != null);

                dc.UsingIdentity<AppDC>(authTokenAuthorizingIdentity, tenantDC =>
                    {
                        if (authToken.TargetID.HasValue)
                        {
                            int? nullUserID = null;
                            var displayName = "login auth token display name";
                            var systemRoles = Enumerable.Empty<SystemRole>();
                            var appRoles = Enumerable.Empty<string>();
                            var tenantGroupTimeZoneInfo = tenantGroupInfo.TimeZoneInfo;

                            IdentityData identityData = IdentityData.Create(authTokenEPScope, tenantGroupID, nullUserID, displayName, systemRoles, appRoles, null, tenantGroupTimeZoneInfo);

                            WebIdentityAuthentication.StartObjectSession(dc, identityData, TimeZones.Pacific, tenantGroupID, null, typeof(EventSession), authToken.TargetID.Value, null, null);
                            context.Response.Redirect("/Spas/VolunteerSpa.aspx");
                            Debug.Fail("Shouldn't get here...");
                        }
                    });

            }

            // (this page handles both valid and invalid clicks)

            string queryString = string.Format("authCode={0}",
                /*0*/ FormatAuthCode(authToken.AuthCode, AuthCodeFormat.Url));

            context.RewritePath("/ResetPassword.aspx", string.Empty, queryString);


            return authTokenState;
        }




/*
        private LoginAuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, TimeSpan? validDuration)
            : base(authCode, clientID, userID, projectID, NullSettings, validDuration)
        {
            // Login as a user or to a project, but not both
            Debug.Assert(userID.HasValue ^ projectID.HasValue);
        }
*/

/*



        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }
*/
/*
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (isValid)
            {
                if (torq.RequireSslLogin)
                {
                    context.RedirectToSecureConnection();
                    Debug.Assert(context.Request.IsSecureConnection);
                }

                // Expire - as this is a one-shot AuthToken
                this.expireAndUpdateDB();

                Debug.Assert(this.AuthUserID.HasValue ^ this.AuthProjectID.HasValue);

                if (this.AuthUserID.HasValue)
                {
                    TorqUserAuthentication.LoginAuthTokenSession(this.AuthUser, Product.cTORQ);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    context.Response.Redirect("/project/" + qs);
                    Debug.Fail("Shouldn't get here...");
                }

                if (this.AuthProjectID.HasValue)
                {
                    TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    if (qs.Length > 0)
                    {
                        qs = "&" + qs;
                    }
                    context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                    Debug.Fail("Shouldn't get here...");
                }
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
*/
/*
        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null && this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
*/
    }






    public class CreateSsoUserAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "CreateSsoUser";

        //!! nextgen - fix the pattern
        private const string authCodePattern = "@##-&&&&-10#";

        public static CreateSsoUserAuthTemplate Instance = new CreateSsoUserAuthTemplate(defaultDuration, "/createuser/", authCodePattern, true, "/goober");



        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);
        //public static readonly CreateUserSsoAuthorized[] EmptyArray = new CreateUserSsoAuthorized[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public AppRole AppRole { get; set; }
            //!! public Product Product { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        //private Data data;
#if false
        partial void OnLoaded()
        {
            this.data = XmlSerializer<Data>.Load(this.Settings);
        }


        public MailAddress MailAddress
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(this.data.Email));
                return this.data.Email.ParseMailAddress();
            }
        }

        public string Email
        {
            get
            {
                string email = string.Empty;
                if (!string.IsNullOrEmpty(this.data.Email))
                {
                    email = this.data.Email;
                }
                return email;
            }
        }

        public string FirstName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.FirstName))
                {
                    name = this.data.FirstName;
                }
                return name;
            }
        }

        public string LastName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.LastName))
                {
                    name = this.data.LastName;
                }
                return name;
            }
        }

        public AppRole AppRole
        {
            get
            {
                return this.data.app.AccessPermission;
            }
        }

        public Product Product
        {
            get
            {
                return this.data.Product;
            }
        }

        //public CreateUserAuthTokenStatus Status
        //{
        //    get
        //    {
        //        CreateUserAuthTokenStatus status = CreateUserAuthTokenStatus.Expired;

        //        // see if the email is being used
        //        if (this.MailAddress != null)
        //        {
        //            User user = User.GetByEmail(this.MailAddress);
        //            if (user != null)
        //            {
        //                status = CreateUserAuthTokenStatus.Accepted;
        //            }
        //            else if (this.IsValid)
        //            {
        //                status = CreateUserAuthTokenStatus.Pending;
        //            }
        //        }

        //        return status;
        //    }
        //}

        public string PreferredState
        {
            get
            {
                string lms = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredState))
                {
                    lms = this.data.PreferredState;
                }
                return lms;
            }
        }

        public string PreferredLaborMarketArea
        {
            get
            {
                string lma = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredLaborMarketArea))
                {
                    lma = this.data.PreferredLaborMarketArea;
                }
                return lma;
            }
        }

        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }
#endif
        protected CreateSsoUserAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }



        private static AuthToken CreateAuthToken(WebUtilityDC dc, Data data, TimeSpan? validDuration)
        {
            // Login tokens are defined within a Group scope (either TenantGroup or UserGroup)
            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

            var propertiesDataString = data.ToString();

            var issuedByUserID = dc.TransactionAuthorizedBy.UserIDOrNull;

            var authToken = AuthToken.Create(dc, authCodePattern, authCode => AuthToken.Create(dc, teamEPScope, CreateSsoUserAuthTemplate.authTokenTypeName, authCode, null, null, propertiesDataString, 1, validDuration, issuedByUserID));
            return authToken;
        }


        private static AuthToken CreateAuthToken(WebUtilityDC dc, int authorizedByUserID, int clientID, AppRole appRole, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Access
            data.AppRole = appRole;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            var authToken = CreateAuthToken(dc, data, duration);
            Debug.Assert(authToken.ID > 0);

            // For LINQ purposes, we need to save the base type
            //!! AuthToken authToken = result;
            //!! dc.Save(authToken);

            return authToken;
        }

        internal static AuthToken FindValid(AppDC dc, int clientID, MailAddress mailAddress)
        {
            var validAuthToken = AuthTemplate.QueryValidAuthTokens(dc, CreateSsoUserAuthTemplate.authTokenTypeName)
                .AsEnumerable()

                .Select(authToken => new { authToken, properties = XmlSerializer<Data>.LoadString(authToken.Properties) })
                .Where(authTokenData => authTokenData.properties.Email == mailAddress.Address)
                .Select(authTokenData => authTokenData.authToken)

                .FirstOrDefault();

            return validAuthToken;
        }

#if false
        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }


        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }
#endif

#if false
        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }
#endif

#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif

#if false
        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }
#endif

#if false
        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
#endif

    }






#if false

    /// <summary>
    /// Provides authority to one-time login to a user or project. Used by single-sign-on.
    /// </summary>
    public partial class LoginAuthToken
    {
        public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(LoginAuthToken));

        private LoginAuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, TimeSpan? validDuration)
            : base(authCode, clientID, userID, projectID, NullSettings, validDuration)
        {
            // Login as a user or to a project, but not both
            Debug.Assert(userID.HasValue ^ projectID.HasValue);
        }

        private static LoginAuthToken CreateNew(int clientID, int? userID, int? projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new LoginAuthToken(authCode, clientID, userID, projectID, validDuration));
        }

        public static LoginAuthToken FindValidUser(int clientID, int userID)
        {
            return AuthToken.FindValid<LoginAuthToken>(clientID)
                .Where(authToken => authToken.AuthUserID == userID)
                .FirstOrDefault();
        }

        public static LoginAuthToken FindValidProject(int clientID, int projectID)
        {
            return AuthToken.FindValid<LoginAuthToken>(clientID)
                .Where(authToken => authToken.AuthProjectID == projectID)
                .FirstOrDefault();
        }

        private static LoginAuthToken ExpireAndCreateNewUser(int clientID, int userID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidUser(clientID, userID), () => CreateNew(clientID, userID, null, validDuration));
        }

        private static LoginAuthToken ExpireAndCreateNewProject(int clientID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidProject(clientID, projectID), () => CreateNew(clientID, null, projectID, validDuration));
        }

        public static Uri ExpireAndCreateNewUserUrl(int clientID, int userID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewUser(clientID, userID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewProjectUrl(int clientID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewProject(clientID, projectID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (isValid)
            {
                if (torq.RequireSslLogin)
                {
                    context.RedirectToSecureConnection();
                    Debug.Assert(context.Request.IsSecureConnection);
                }

                // Expire - as this is a one-shot AuthToken
                this.expireAndUpdateDB();

                Debug.Assert(this.AuthUserID.HasValue ^ this.AuthProjectID.HasValue);

                if (this.AuthUserID.HasValue)
                {
                    TorqUserAuthentication.LoginAuthTokenSession(this.AuthUser, Product.cTORQ);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    context.Response.Redirect("/project/" + qs);
                    Debug.Fail("Shouldn't get here...");
                }

                if (this.AuthProjectID.HasValue)
                {
                    TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    if (qs.Length > 0)
                    {
                        qs = "&" + qs;
                    }
                    context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                    Debug.Fail("Shouldn't get here...");
                }
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null && this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }

#endif
#if false

    /// <summary>
    /// Provides authority to change a password in a specific User account.
    /// </summary>
    public partial class PasswordResetAuthToken
    {
        private static TimeSpan defaultDuration = TimeSpan.FromDays(1.0);
        private static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(PasswordResetAuthToken));

        private PasswordResetAuthToken(AuthCode authCode, int clientID, int userID)
            : base(authCode, clientID, userID, NullProjectID, NullSettings, defaultDuration)
        { }

        private static PasswordResetAuthToken CreateNew(int clientID, int userID)
        {
            return AuthToken.CreateNew(authCode => new PasswordResetAuthToken(authCode, clientID, userID));
        }

        private static PasswordResetAuthToken FindValid(int clientID, int userID)
        {
            return AuthToken.FindValid<PasswordResetAuthToken>(clientID)
                .Where(authToken => authToken.AuthUserID.HasValue && authToken.AuthUserID.Value == userID)
                .FirstOrDefault();
        }

        private static PasswordResetAuthToken ExpireAndCreateNew(int clientID, int userID)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValid(clientID, userID), () => CreateNew(clientID, userID));
        }

        public static PasswordResetAuthToken FindValid(AuthCode authCode)
        {
            return AuthToken.FirstOrDefault<PasswordResetAuthToken>(authCode);
        }

        public static Uri ExpireAndCreateNewUrl(int clientID, int userID)
        {
            PasswordResetAuthToken passwordResetAuthToken = ExpireAndCreateNew(clientID, userID);

            if (TorqContext.Current.RequireSslLogin)
            {
                return passwordResetAuthToken.SecureUrl;
            }

            return passwordResetAuthToken.Url;
        }

        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (torq.RequireSslLogin)
            {
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            // (this page handles both valid and invalid clicks)

            string queryString = string.Format("authCode={0}",
                /*0*/ this.AuthCodeString);
            context.RewritePath("/ResetPassword.aspx", string.Empty, queryString);
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }

    /// <summary>
    /// Provides authority to view/edit one specific TORQ project
    /// </summary>
    public partial class ProjectAuthToken
    {
        public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(ProjectAuthToken));

        private ProjectAuthToken(AuthCode authCode, int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
            : base(authCode, clientID, issuedByUserID, projectID, NullSettings, validDuration)
        { }

        private static ProjectAuthToken CreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new ProjectAuthToken(authCode, clientID, issuedByUserID, projectID, validDuration));
        }

        public static ProjectAuthToken FindValid(int clientID, int projectID)
        {
            return AuthToken.FindValid<ProjectAuthToken>(clientID)
                .Where(authToken => authToken.AuthProjectID == projectID)
                .FirstOrDefault();
        }

        private static ProjectAuthToken ExtendOrCreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExtendOrCreateNew(() => FindValid(clientID, projectID), () => CreateNew(clientID, issuedByUserID, projectID, validDuration), validDuration);
        }

        private static ProjectAuthToken ExpireAndCreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValid(clientID, projectID), () => CreateNew(clientID, issuedByUserID, projectID, validDuration));
        }

        public static Uri ExtendOrCreateNewUrl(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return ExtendOrCreateNew(clientID, issuedByUserID, projectID, validDuration).ToUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewUrl(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNew(clientID, issuedByUserID, projectID, validDuration).ToUrl(AuthTokenType.UrlPatternPrefix);
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                //!! is this really where we want to redirect?
                // Redirect - so the client can save the AuthCookie
                string qs = context.Request.QueryString.ToString();
                if (qs.Length > 0)
                {
                    qs = "&" + qs;
                }
                context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                Debug.Fail("Shouldn't get here...");
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }
#endif


#if true
    /// <summary>
    /// Provides authority to create a new TORQ user
    /// </summary>
    public partial class CreateUserAuthTemplate : MS.WebUtility.CreateUserAuthTemplate<CreateUserAuthTemplate.Data>
    {
        // We use XML to persist the fields in this Data class.
        public new class Data : MS.WebUtility.CreateUserAuthTemplate<CreateUserAuthTemplate.Data>.Data
        {
            //!! public User.AccessPermissions AccessPermission { get; set; }
            //public Product Product { get; set; }
            //public DateTime LastSentDate { get; set; }
            //public string PreferredState { get; set; }
            //public string PreferredLaborMarketArea { get; set; }
        }

        protected CreateUserAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }


        public static AuthToken FindAuthToken(UtilityDC dc, AuthCode authCode)
        {
            return AuthTemplate.FindAuthToken<CreateUserAuthTemplate>(dc, authCode);
        }

        public static AuthToken FindAuthToken<T>(UtilityDC dc, AuthCode authCode, out T data)
        {
            return AuthTemplate.FindAuthToken<T, CreateUserAuthTemplate>(dc, authCode, out data);
        }


    }
#endif




#if false
    /// <summary>
    /// Provides authority to create a new TORQ user
    /// </summary>
    public partial class CreateUserAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "CreateUser";
        private const string authCodePattern = "&23&&-&&&&-&&";

        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);

        public static CreateUserAuthTemplate Instance = new CreateUserAuthTemplate("/invite/", authCodePattern, true, "/goober");

        //public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(CreateUserAuthToken));
        //public static readonly CreateUserAuthToken[] EmptyArray = new CreateUserAuthToken[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public MS.Utility.Xml.MailAddress MailAddress { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            //!! public User.AccessPermissions AccessPermission { get; set; }
            public SystemRole[] SystemRoles { get; set; }
            public AppRole[] AppRoles { get; set; }
            public Product Product { get; set; }
            public DateTime LastSentDate { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        //!! nextgen - some properties used to call this, now to account for it?
#if false
        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }
#endif

        protected CreateUserAuthTemplate(string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }

        private static AuthToken CreateAuthToken(AppDC dc, EPScope epScope, MailAddress inviteeMailAddress)
        {

            Debug.Assert(inviteeMailAddress != null);

            Data data = new Data();

            data.MailAddress = inviteeMailAddress;

            // Save Email
            data.Email = inviteeMailAddress.Address;

            // Product
            //data.Product = product;

            // Access
            //data.AccessPermission = permission;

            // LMA
            //data.PreferredState = lms;
            //data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = inviteeMailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            //!! return AuthToken.CreateNew(authCode => new CreateUserAuthToken(authCode, authorizedByUserID, clientID, data, duration));


            var authToken = WebAuthTemplate.CreateAuthToken(dc, authCodePattern, authCode => new AuthToken(epScope, authTokenTypeName, authCode, data, 1, defaultDuration));
            return authToken;
        }



/*
        private static CreateUserAuthToken CreateNew(int authorizedByUserID, int clientID, User.AccessPermissions permission, Product product, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Product
            data.Product = product;

            // Access
            data.AccessPermission = permission;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            return AuthToken.CreateNew(authCode => new CreateUserAuthToken(authCode, authorizedByUserID, clientID, data, duration));
        }
*/

        public static AuthToken FindValidAuthToken(AppDC dc, MailAddress mailAddress)
        {
            return AuthToken.QueryValid(dc, authTokenTypeName)
                // bring all the valid AuthTokens to the web server, so we can filter on values in the data field
                .AsEnumerable()

                .Select(authToken => new { authToken, properties = XmlSerializer<Data>.LoadString(authToken.Properties) })
                .Where(authTokenData => authTokenData.properties.Email == mailAddress.Address)
                .Select(authTokenData => authTokenData.authToken)

                .FirstOrDefault();
        }

        public static AuthToken FindAuthToken(AppDC dc, string authCodeValue)
        {
            var authToken = FindAuthToken(dc, CreateUserAuthTemplate.authTokenTypeName, authCodeValue);
            return authToken;
        }

        public static AuthToken FindAuthToken(AppDC dc, string authCodeValue, out Data data)
        {
            var authToken = FindAuthToken(dc, CreateUserAuthTemplate.authTokenTypeName, authCodeValue, out data);
            return authToken;
        }

        public static Uri GetUrl(AuthToken authToken)
        {
            var url = Instance.GetUrl(authToken.AuthCode, false);
            return url;
        }

        //CreateProjectAuthTemplate.Data createProjectAuthTokenProperties;
        //var createProjectAuthToken = AuthToken.Find<CreateProjectAuthTemplate.Data>(dc, createProjectAuthCode, out createProjectAuthTokenProperties);


        public static AuthToken ExpireAndCreateNew(AppDC dc, EPScope epScope, MailAddress inviteeMailAddress)
        {
            var utilityContext = UtilityContext.Current;

            var authToken = WebAuthTemplate.ExpireAndCreateNewAuthToken(dc, () => FindValidAuthToken(dc, inviteeMailAddress), () => CreateAuthToken(dc, epScope, inviteeMailAddress));
            return authToken;
        }



#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif


        protected override void OnTokenClicked(HttpContext context, WebUtilityDC dc, AuthToken authToken)
        {
            UtilityContext utilityContext = UtilityContext.Current;

            //!! can this be pushed to the base class? Can the template indicate if Ssl is required?
            if (utilityContext.RequireSslLogin)
            {
                // (If we redirect here, the link the user clicked on will remain in the browser URL)
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            base.OnTokenClicked(context, dc, authToken);

            // (this page handles both valid and invalid clicks)

            //context.Server.Transfer("/app/default.aspx");
            //Debug.Fail("Shouldn't get here");


            string queryString = string.Format("authCode={0}",
                /*0*/ FormatAuthCode(authToken.AuthCode, AuthCodeFormat.Url));
            //!! context.RewritePath("/Signin/ResetPassword.aspx", string.Empty, queryString);
            //context.RewritePath("/app/default.aspx", string.Empty, queryString);
//!! need some help deciding if we should go to a client Spa or server page
            context.RewritePath("/app/default.aspx", string.Empty, queryString);
        }




#if false
        public static AuthToken FindAuthToken(TorqDataContext accountsDC, AuthCode authCode)
        {
            var authToken = AuthToken.Find(accountsDC, authCode);
            return authToken;
        }


        public static AuthToken FindValid(TorqDataContext accountsDC, AuthCode authCode)
        {
            var authToken = AuthToken.Find(accountsDC, authCode);
            return authToken;
        }


        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }

        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        public void SetLastSentDate(DateTime timestamp)
        {
            Debug.Assert(timestamp > new DateTime(2011, 6, 1));
            this.LastSentDate = timestamp;
        }

        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
#endif
    }
#endif

#if false


    /// <summary>
    /// Provides authority to create a new User via the PartnerApi
    /// </summary>
    public partial class CreateUserSsoAuthorized
    {
        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);
        public static readonly CreateUserSsoAuthorized[] EmptyArray = new CreateUserSsoAuthorized[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }

            //public User.AccessPermissions AccessPermission { get; set; }
            public AppRole AppRole { get; set; }

            public Product Product { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        private Data data;

        partial void OnLoaded()
        {
            this.data = XmlSerializer<Data>.Load(this.Settings);
        }

        public MailAddress MailAddress
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(this.data.Email));
                return this.data.Email.ParseMailAddress();
            }
        }

        public string Email
        {
            get
            {
                string email = string.Empty;
                if (!string.IsNullOrEmpty(this.data.Email))
                {
                    email = this.data.Email;
                }
                return email;
            }
        }

        public string FirstName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.FirstName))
                {
                    name = this.data.FirstName;
                }
                return name;
            }
        }

        public string LastName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.LastName))
                {
                    name = this.data.LastName;
                }
                return name;
            }
        }

        public User.AccessPermissions AccessPermission
        {
            get
            {
                return this.data.AccessPermission;
            }
        }

        public Product Product 
        {
            get
            {
                return this.data.Product;
            }
        }

        //public CreateUserAuthTokenStatus Status
        //{
        //    get
        //    {
        //        CreateUserAuthTokenStatus status = CreateUserAuthTokenStatus.Expired;

        //        // see if the email is being used
        //        if (this.MailAddress != null)
        //        {
        //            User user = User.GetByEmail(this.MailAddress);
        //            if (user != null)
        //            {
        //                status = CreateUserAuthTokenStatus.Accepted;
        //            }
        //            else if (this.IsValid)
        //            {
        //                status = CreateUserAuthTokenStatus.Pending;
        //            }
        //        }

        //        return status;
        //    }
        //}

        public string PreferredState
        {
            get
            {
                string lms = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredState))
                {
                    lms = this.data.PreferredState;
                }
                return lms;
            }
        }

        public string PreferredLaborMarketArea
        {
            get
            {
                string lma = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredLaborMarketArea))
                {
                    lma = this.data.PreferredLaborMarketArea;
                }
                return lma;
            }
        }

        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }

        private CreateUserSsoAuthorized(int authorizedByUserID, int clientID, Data data, TimeSpan duration)
            : base(AuthCode.Invalid, clientID, authorizedByUserID, NullProjectID, data.ToString(), duration)
        {
            this.data = data;
        }

        private static CreateUserSsoAuthorized CreateNew(int authorizedByUserID, int clientID, User.AccessPermissions permission, Product product, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Product
            data.Product = product;

            // Access
            data.AccessPermission = permission;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            var result = new CreateUserSsoAuthorized(authorizedByUserID, clientID, data, duration);

            // For LINQ purposes, we need to save the base type
            AuthToken authToken = result;
            authToken.AccountsDBSave();

            return result;
        }

        internal static CreateUserSsoAuthorized FindValid(int clientID, MailAddress mailAddress)
        {
            var clientTokens = AuthToken.FindValid<CreateUserSsoAuthorized>(clientID);
            //!! not so efficient, but need to iterate through and find the right MailAddress
            foreach (var clientToken in clientTokens)
            {
                if (clientToken.data != null && clientToken.MailAddress.Address == mailAddress.Address)
                {
                    return clientToken;
                }
            }

            return null;
        }

#if false
        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }


        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }
#endif
        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }
#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif
        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
    }


    

    //SsoCreateUserRequested



    public partial class ReportAuthToken
    {
        private static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(ReportAuthToken));

        public static ReportAuthToken FindValid(AuthCode authCode)
        {
            return AuthToken.FirstOrDefault<ReportAuthToken>(authCode);
        }

        public int ProjectID
        {
            get
            {
                Debug.Assert(this.AuthProjectID != null && (int)this.AuthProjectID > 0);
                if (this.AuthProjectID != null)
                {
                    return (int)this.AuthProjectID;
                }
                return 0;
            }
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }
    }

    public enum CreateUserAuthTokenStatus
    {
        Pending,
        Expired,
        Accepted
    }
#endif
}