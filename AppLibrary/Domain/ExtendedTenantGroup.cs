using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using MS.Utility;


namespace App.Library
{

#if true
    public class ClientInfo : TenantGroupInfo
    {
        public static readonly new ClientInfo[] EmptyArray = new ClientInfo[0];

        // These hold information about our heirarchy.
        //internal int? parentClientID { get; private set; }
        //public ClientInfo ParentInfo { get; private set; }
        //private List<ClientInfo> children = new List<ClientInfo>();

        //public int ClientID { get; private set; }
        //public string ClientName { get; private set; }
        //public string TimeZone { get; private set; }
        //public TimeZoneInfo TimeZoneInfo { get; private set; }

        public new ClientInfo ParentInfo
        {
            get { return base.ParentInfo as ClientInfo; }
        }

        //!! public ClientFeatures.Feature[] EnabledFeatures = ClientFeatures.EmptyFeatureArray;

        // (note null possible, and indicates the client imposes no restriction on JobPosting providers)
        public JobPostingProvider.Providers[] EnabledJobPostingProviders { get; private set; }
        public string AccountManagerEmail { get; private set; }
        public string AccountManagerFullName { get; private set; }
//        public string ReferenceDatabaseNamePrefix { get; set; }
//        public bool IsActive { get; private set; }
        public int NumberOfSeatsContracted { get; private set; }
        public int? ConcurrentSeatsContracted { get; private set; }
        public bool HasAssignedLaborMarketAreas { get; private set; }

        // We want to cascade up the hierarchy looking for an Account Manager during a get;
        private int? accountManagerUserIDField;
        public int? AccountManagerUserID
        {
            get
            {
                if (this.accountManagerUserIDField.HasValue)
                {
                    return this.accountManagerUserIDField;
                }
                if (this.ParentInfo != null)
                {
                    return this.ParentInfo.AccountManagerUserID;
                }
                return null;
            }
            private set
            {
                this.accountManagerUserIDField = value;
            }
        }

        public MailAddress AccountManagerMailAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AccountManagerEmail) && !string.IsNullOrEmpty(this.AccountManagerFullName))
                {
                    return new MailAddress(this.AccountManagerEmail, this.AccountManagerFullName);
                }
                return null;
            }
        }

#if false
        private JObject configInfoField;
        public JObject ConfigInfo { get; private set; }
        public IEnumerable<string> SecretKeys
        {
            get
            {
                var secretKeysArray = this.ConfigInfo["secretKeys"];
                if (secretKeysArray != null)
                {
                    var secretKeysEnum = secretKeysArray.Select(key => key.ToString());
                    return secretKeysEnum;
                }
                return Enumerable.Empty<string>();
            }
        }
#endif

        protected ClientInfo(TenantGroupInfo tenantGroupInfo, ExtendedTenantGroup extendedTenantGroup)
            : base(tenantGroupInfo)
        {
            this.NumberOfSeatsContracted = extendedTenantGroup.NumberOfSeatsContracted;
        }

#if false
        protected ClientInfo(ExtendedTenant extendedTenant, Tenant tenant, string staticLogoPath, string[] optionTagNames, string[] userAssignedTagNames, JObject settings)
            : base(tenant, staticLogoPath, optionTagNames, userAssignedTagNames, settings)
        {
            this.NumberOfSeatsContracted = extendedTenant.NumberOfSeatsContracted;
        }
#endif

#if false
        internal ClientInfo(int? accountManagerUserID, string accountManagerFullName, string accountManagerEmail, CustomIdMode? customIdMode,
            string referenceDatabaseNamePrefix, ClientFeatures.Feature[] enabledFeatures, JobPostingProvider.Providers[] enabledJobPostingProviders, bool isActive, int numberOfSeatsContracted, int? concurrentSeatsContracted, bool hasAssignedLaborMarketAreas, JObject configInfo)
        {
            this.AccountManagerUserID = accountManagerUserID;
            this.AccountManagerFullName = accountManagerFullName;
            this.AccountManagerEmail = accountManagerEmail;
            this.ReferenceDatabaseNamePrefix = referenceDatabaseNamePrefix;
            this.EnabledFeatures = enabledFeatures;
            this.EnabledJobPostingProviders = enabledJobPostingProviders;
            this.IsActive = isActive;
            this.NumberOfSeatsContracted = numberOfSeatsContracted;
            this.ConcurrentSeatsContracted = concurrentSeatsContracted;
            this.HasAssignedLaborMarketAreas = hasAssignedLaborMarketAreas;
            this.customIdModeField = customIdMode;
            // this.EffectiveConfigInfo = configInfo;
        }

        private ClientInfo(TenantGroupInfo tenantGroupInfo, int numberOfSeatsContracted
/*
        int? accountManagerUserID, string accountManagerFullName, string accountManagerEmail, CustomIdMode? customIdMode,
            string referenceDatabaseNamePrefix, ClientFeatures.Feature[] enabledFeatures, JobPostingProvider.Providers[] enabledJobPostingProviders, bool isActive, int numberOfSeatsContracted, int? concurrentSeatsContracted, bool hasAssignedLaborMarketAreas, JObject configInfo)
*/
        )
            : base(tenantGroupInfo)
        {
            this.NumberOfSeatsContracted = numberOfSeatsContracted;

/*
            this.AccountManagerUserID = accountManagerUserID;
            this.AccountManagerFullName = accountManagerFullName;
            this.AccountManagerEmail = accountManagerEmail;
            this.ReferenceDatabaseNamePrefix = referenceDatabaseNamePrefix;
            this.EnabledFeatures = enabledFeatures;
            this.EnabledJobPostingProviders = enabledJobPostingProviders;
            this.IsActive = isActive;
            this.NumberOfSeatsContracted = numberOfSeatsContracted;
            this.ConcurrentSeatsContracted = concurrentSeatsContracted;
            this.HasAssignedLaborMarketAreas = hasAssignedLaborMarketAreas;
            this.customIdModeField = customIdMode;
            this.configInfoField = configInfo;
*/
        }
#endif

#if false
        public static ClientInfo Create(ExtendedTenant extendedTenant, Tenant tenant, string staticLogoPath, string[] optionTagNames, string[] userAssignedTagNames, JObject settings)
        {
            var clientInfo = new ClientInfo(extendedTenant, tenant, staticLogoPath, optionTagNames, userAssignedTagNames, settings);
            return clientInfo;
        }
#endif

        public static TenantGroupInfoCollection Create(UtilityContext utilityContext)
        {
            var siteContext = utilityContext as SiteContext;

            if (utilityContext.AccountsDatabase == null)
            {
                // (Can't do much unless we have a database configured...)
                return TenantGroupInfoCollection.Empty;
            }

            // First, call the standard framework implementation - to get the standard collection
            var tenantGroupInfoCollection = TenantGroupInfoCollection.Create(utilityContext);

            using (var dc = utilityContext.CreateSystemDefaultAccountsOnlyDC<AppDC>())
            {
                // Second, grab all our ExtendedTenant info and regenerate each of the standard TenantGroupInfo nodes as extended ClientInfo nodes
                var extendedTenants = dc.ExtendedTenantGroups
                    .ToArray();

                var tenantGroupInfoList = (
                    from tenantGroupInfo in tenantGroupInfoCollection
                    join extendedTenant in dc.ExtendedTenantGroups on tenantGroupInfo.TenantGroupID equals extendedTenant.ID
                    select new { tenantGroupInfo, extendedTenant })
                    .Select(tenantGroupInfoJoin => new ClientInfo(tenantGroupInfoJoin.tenantGroupInfo, tenantGroupInfoJoin.extendedTenant))
                    .ToArray();

                var extendedTenantGroupInfoCollection = TenantGroupInfoCollection.Create(tenantGroupInfoList);
                return extendedTenantGroupInfoCollection;
            }
        }

        public new IEnumerable<ClientInfo> DescendentsAndSelf
        {
            get
            {
                return base.DescendentsAndSelf
                    .Cast<ClientInfo>();
            }
        }

        public new IEnumerable<ClientInfo> Descendents
        {
            get
            {
                return base.Descendents
                    .Cast<ClientInfo>();
            }
        }

        public new IEnumerable<ClientInfo> AncestorsAndSelf
        {
            get
            {
                return base.AncestorsAndSelf
                    .Cast<ClientInfo>();
            }
        }

        public new IEnumerable<ClientInfo> Ancestors
        {
            get
            {
                return base.Ancestors
                    .Cast<ClientInfo>();
            }
        }

        public new ClientInfo Root
        {
            get
            {
                return AncestorsAndSelf.Last();
            }
        }

        //!! nextgen - need a way to ensure this is called
#if false
        internal static void InitHierarchy(TenantGroupInfoCollection tenantGroupInfoCollection)
        {
            // Make another pass to cascade the clientConfig info
            foreach (var tenantGroupInfo in tenantGroupInfoCollection)
            {
                var clientInfo = tenantGroupInfo as ClientInfo;
                Debug.Assert(clientInfo != null);


                // aggregate together all the config information from the root down to this node
                var aggregateConfigInfo = clientInfo.AncestorsAndSelf.Reverse().Aggregate(new JObject(), (sum, hierarchyItem) => sum.Merge(hierarchyItem.configInfoField));
                clientInfo.ConfigInfo = aggregateConfigInfo;
            }
        }
#endif
    }
#endif

#if false
//!! should be able to use the Tenant one right?
    public class ClientInfoCollection : Collection<ClientInfo>
    {
        public static readonly ClientInfoCollection Empty = new ClientInfoCollection();

        private ClientInfoCollection()
        { }

        private ClientInfoCollection(IList<ClientInfo> clientInfoList)
            : base(clientInfoList)
        { }


        public static ClientInfoCollection Create(AppDC accountsDC)
        {
            var sqlQuery = from extendedTenants in accountsDC.ExtendedTenants
                           join clientLma in accountsDC.ClientLaborMarketAreas on extendedTenants.ID equals clientLma.ClientID into clientLmas
                           select new
                           {
                               extendedTenants.IsActive,
                               extendedTenants.NumberOfSeatsContracted,
                               extendedTenants.ConcurrentSeatsContracted,
                               extendedTenants.ClientSettings,
                               // calculated so we can quickly determine where in a hierarchy we need to look to find the governing set of LMAs
                               HasAssignedNonInheritedLmas = clientLmas.Any() ? true : false,
                           };

            var clientInfoList = sqlQuery
                .AsEnumerable()
                .Select(client => new
                {
                    client.IsActive,
                    client.NumberOfSeatsContracted,
                    client.ConcurrentSeatsContracted,
                    Blob = XmlSerializer<ExtendedTenant.Data>.LoadString(client.ClientSettings),
                    client.HasAssignedNonInheritedLmas,
                })
                .Select(clientItem => new
                {
                    clientItem,
                    accountManagerItem = !clientItem.Blob.AccountManagerUserID.HasValue ? null : accountsDC.Users
                        .Where(user => user.ID == clientItem.Blob.AccountManagerUserID.Value)
                        .Select(user => new
                        {
                            user.ID,
                            FullName = user.FirstName + " " + user.LastName,
                            //!! nextgen - need to join to get email address
                            //user.Email,
                            Email = "foo@email.com",
                        })
                        .FirstOrDefault(),
                    //!! nextgen hmm - get our config info
                    //clientConfigInfo = clientsConfigInfo[clientItem.ClientId.ToString()] as JObject,
                    clientConfigInfo = new JObject(),
                })
                .Select(clientAccountManagerItem => new ClientInfo(
                    clientAccountManagerItem.clientItem.Blob.AccountManagerUserID,
                    clientAccountManagerItem.accountManagerItem != null ? clientAccountManagerItem.accountManagerItem.FullName : string.Empty,
                    clientAccountManagerItem.accountManagerItem != null ? clientAccountManagerItem.accountManagerItem.Email : string.Empty,
                    clientAccountManagerItem.clientItem.Blob.CustomIdMode,
                    clientAccountManagerItem.clientItem.Blob.RequestedReferenceDB ?? "torq_reference_v2",
                    clientAccountManagerItem.clientItem.Blob.EnabledFeatures ?? ClientFeatures.EmptyFeatureArray,
                    clientAccountManagerItem.clientItem.Blob.EnabledJobPostingProviders,
                    clientAccountManagerItem.clientItem.IsActive,
                    clientAccountManagerItem.clientItem.NumberOfSeatsContracted,
                    clientAccountManagerItem.clientItem.ConcurrentSeatsContracted,
                    clientAccountManagerItem.clientItem.HasAssignedNonInheritedLmas,
                    clientAccountManagerItem.clientConfigInfo))
                .ToList();

            var clientInfoCollection = new ClientInfoCollection(clientInfoList);

            return clientInfoCollection;
        }


        public static ClientInfoCollection Create(IList<ClientInfo> clientInfoList)
        {
            var clientInfoCollection = new ClientInfoCollection(clientInfoList);

         //!! nextgen   ClientInfo.InitHierarchy(clientInfoCollection);

            return clientInfoCollection;
        }
#if false
        internal ClientInfo Find(int clientID)
        {
            return this.FirstOrDefault(clientInfo => clientInfo.ID == clientID);
        }
#endif
    }
#endif


    public partial class ExtendedTenantGroup : ExtendedObject<ExtendedTenantGroup>
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return ExtendedPropertyScopeType.Invalid; } }
        protected override int? objectScopeID { get { return null; } }


        //!! see if we can get rid of these and only use the ActivityStatusTags below
        public static readonly string FullTORQLicenseTag = "License";
        public static readonly string ActiveStatusTag = "Active";
        public static readonly string InactiveStatusTag = "Inactive";
        public static readonly string cTORQLicenseTag = "cTORQ";

        //!! nextgen - should be able to pass 1 no, can't the base class add if necessary?
        // Are these library scope or class scope?
        public static EPCategory TenantFeaturesCategory = EPCategory.CreateMultiple<ExtendedTenantGroup>("Features");






        public static HubResult CreateTenant(UtilityDC dc, string name, dynamic data)
        {
            HubResult hubResult = TenantGroup.CreateTenant(dc, name, data, false);
            Debug.Assert(hubResult.StatusCode == HubResultCode.Success);

            if (hubResult.StatusCode == HubResultCode.Success)
            {
                dynamic responseData = hubResult.ResponseData;

                int tenantID = (int)JObject.Parse(hubResult.ToJson())["ResponseData"]["id"];
                return Create(dc, tenantID, data);
            }

            return hubResult;
        }

        public static HubResult CreateGroup(UtilityDC dc, string name, int parentTenantGroupID, dynamic data)
        {
            HubResult hubResult = TenantGroup.CreateGroup(dc, name, parentTenantGroupID, data, false);
            Debug.Assert(hubResult.StatusCode == HubResultCode.Success);

            if (hubResult.StatusCode == HubResultCode.Success)
            {
                dynamic responseData = hubResult.ResponseData;

                int groupID = (int)JObject.Parse(hubResult.ToJson())["ResponseData"]["id"];
                return Create(dc, groupID, data);
            }

            return hubResult;
        }


        private static HubResult Create(UtilityDC dc, int tenantGroupID, dynamic data)
        {
            Debug.Assert(tenantGroupID > 0);

            var authorizedBy = dc.TransactionAuthorizedBy;
            Debug.Assert(authorizedBy != null);
            if (authorizedBy == null || !authorizedBy.PermissionCheck(Permission.ProvisionAccount))
            {
                return HubResult.Forbidden;
            }

            var newItem = new ExtendedTenantGroup()
            {
                 ID = tenantGroupID,

                 Address1 = (string)data.Address1,
                 Address2 = (string)data.Address2,
                 City = (string)data.City,
                 State = (string)data.State,
                 Zip = (string)data.Zip,
                 Phone = (string)data.Phone,
                 Website = (string)data.Website,
                 NumberOfSeatsContracted = (int?)data.NumberOfSeatsContracted ?? int.MaxValue,
                 ConcurrentSeatsContracted = (int?)data.ConcurrentSeatsContracted,
                //!! nextgen - Notes = migrateClient.Notes,
                 ClientSettings = (string)data.ClientSettings,
            };

            dc.Save(newItem);



//            newItem.updateData(dc, data);
//            dc.SubmitChanges();

            // (This includes a tenant cache refresh)
            TenantGroup.NotifyClients(dc, NotifyExpression.CreateModifiedID(newItem.ID));

            return HubResult.CreateSuccessData(new { id = newItem.ID });
        }



        // add LicensedProducts
        // change Active to a bit field

        public class Data : XmlSerializableClass<Data>
        {
            [XmlElement("Feature")]
            public ClientFeatures.Feature[] EnabledFeatures;
        }

        //private Data clientSettingsData;

        //partial void OnLoaded()
        //{
            //this.clientSettingsData = XmlSerializer<Data>.LoadString(this.ClientSettings);
        //}

        private void OnSaving()
        {
            //this.ClientSettings = this.clientSettingsData.ToString();
        }

        public static bool AccessCheck(Identity authorizedBy, int clientID)
        {
            Debug.Assert(authorizedBy != null);
            if (authorizedBy == null)
            {
                return false;
            }

            if (authorizedBy.IsSecurityAdmin || authorizedBy.IsSystemAdmin)
            {
                // SecAs & SAs can access all clients
                return true;
            }

            if (authorizedBy.TenantGroupID.Value == clientID)
            {
                // Users in the client group have access
                return true;
            }

            var clientInfo = TenantGroup.GetCachedTenantGroupInfo(authorizedBy.TenantGroupID.Value) as ClientInfo;
            Debug.Assert(clientInfo != null);
            if (clientInfo == null)
            {
                return false;
            }

            //!! I don't think we have a hierarchy of Tenant Groups do we?

            if (authorizedBy.IsInRole(AppRole.EventManager) &&
                clientInfo.DescendentsAndSelf.Any(ci => ci.TenantGroupID == clientID))
            {
                // Group Admins have access to any groups in their branch
                return true;
            }

            return false;
        }


#if false
        public ClientFeatures.Feature[] EnabledFeatures
        {
            get { return this.clientSettingsData.EnabledFeatures ?? ClientFeatures.EmptyFeatureArray; }
            // Caller must ensure OnSaving() is called to rollup the XML blob, then save to DB, then cachedClientFeatures.Refresh()
            set { this.clientSettingsData.EnabledFeatures = value; }
        }

        public JobPostingProvider.Providers[] EnabledJobPostingProviders
        {
            get { return this.clientSettingsData.EnabledJobPostingProviders ?? JobPostingProvider.EmptyProviderArray; }
            // Caller must ensure OnSaving() is called to rollup the XML blob, then save to DB, then cachedClientFeatures.Refresh()
            set { this.clientSettingsData.EnabledJobPostingProviders = value; }
        }

        public int? AccountManagerUserID
        {
            get { return this.clientSettingsData.AccountManagerUserID; }
            private set { this.clientSettingsData.AccountManagerUserID = value; }
        }

        public CustomIdMode? CustomIdMode
        {
            get { return this.clientSettingsData.CustomIdMode; }
            private set { this.clientSettingsData.CustomIdMode = value; }
        }
#endif

#if false
        public string RequestedReferenceDB
        {
            get { return this.clientSettingsData.RequestedReferenceDB; }
            // Caller must ensure OnSaving() is called to rollup the XML blob, then save to DB, then cachedClientFeatures.Refresh()
            set { this.clientSettingsData.RequestedReferenceDB = value; }
        }
#endif

#if false
        public bool IsProductLicensed(Product product)
        {
            return this.LicensedProducts.Contains(product);
        }

        public Product[] LicensedProducts
        {
            get
            {
                if (string.IsNullOrEmpty(this.LicensedProductTokens))
                {
                    return new Product[0];
                }

                return this.LicensedProductTokens
                    .Split(' ')
                    .Select(tag => tag.ParseAsEnum<Product>())
                    .ToArray();
            }
            private set
            {
                this.LicensedProductTokens = value
                    .Select(status => status.ToString())
                    .Join(" ");
            }
        }
#endif

        //!! nextgen - handled by Tenant, no?
#if false
        public IEnumerable<ClientInfo> ValidParents
        {
            get
            {
                var allClients = GetCachedClientInfo();

                var myClientInfo = allClients
                    .FirstOrDefault(client => client.TenantGroupID == this.ClientId);
                Debug.Assert(myClientInfo != null);

                var myDescendentsAndSelf = myClientInfo.DescendentsAndSelf;

                var availableParents = allClients
                    .Except(myDescendentsAndSelf);

                return availableParents;
            }
        }

        public bool HasDescendents
        {
            get
            {
                bool hasDescendents = this.ClientInfo.HasDescendents;
                return hasDescendents;
            }
        }
#endif

        internal ClientInfo ClientInfo
        {
            // Let's see if we can keep ClientInfo an internal implementation detail
            get { return GetCachedClientInfo(this.ID); }
        }

        //!! nextgen - handled by Tenant, right?
#if false
        public string HierarchyName
        {
            get { return this.ClientInfo.HierarchyName; }
        }

        public string ParentHierarchyName
        {
            get { return this.ClientInfo.ParentHierarchyName; }
        }

        public bool HasParent
        {
            get
            {
                return this.ParentClientId.HasValue;
            }
        }

        public Client Parent
        {
            get
            {
                if (this.ParentClientId.HasValue)
                {
                    var parentClient = Client.GetByClientID(this.ParentClientId.Value);

                    return parentClient;
                }

                return null;
            }
        }

        public Client Root
        {
            get
            {
                if (HasParent)
                {
                    return Parent.Root;
                }
                return this;
            }
        }
#endif

        public static TimeZoneInfo GetClientTimeZoneInfo(int clientID)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null)
            {
                //!! nextgen - is this a good value to display?
                return clientInfo.TimeZoneInfo;
            }

            return null;
        }

        public static string GetClientName(int clientID)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null)
            {
                return clientInfo.Name;
            }

            return string.Empty;
        }

        public static string GetHierarchyName(int clientID)
        {
            var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(clientID);
            Debug.Assert(tenantGroupInfo != null);

            if (tenantGroupInfo != null)
            {
                return tenantGroupInfo.HierarchyName;
            }

            return string.Empty;
        }

        public static string GetParentName(int clientID)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null && clientInfo.ParentInfo != null)
            {
                return clientInfo.ParentInfo.Name;
            }

            return string.Empty;
        }

        public static string GetParentHierarchyName(int clientID)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null)
            {
                return clientInfo.ParentHierarchyName;
            }

            return string.Empty;
        }

        public static string GetRootName(int clientID)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null)
            {
                return clientInfo.Root.Name;
            }

            return string.Empty;
        }

        public static string GetSsoSignOutUrl(int clientID)
        {
            var ssoSettingsObject = GetAggregateSettingsObject(clientID, "ssoConfig");
            if (ssoSettingsObject != null)
            {
                var ssoSignOutUrl = ssoSettingsObject["signOutUrl"];
                if (ssoSignOutUrl != null)
                {
                    return ssoSignOutUrl.ToString();
                }
            }
            return null;
        }

        public static JObject GetAggregateSettingsObject(int clientID, string key)
        {
            ClientInfo clientInfo = TenantGroup.GetCachedTenantGroupInfo(clientID) as ClientInfo;
            Debug.Assert(clientInfo != null);

            if (clientInfo != null)
            {
                var settingsObject = clientInfo.GetAggregateSettingsObject(key);
                return settingsObject;
            }

            return null;
        }

        internal static IEnumerable<ClientInfo> GetCachedClientInfo()
        {
            return TenantGroup.GetCachedTenantGroupInfo()
                .Cast<ClientInfo>();
        }

        internal static ClientInfo GetCachedClientInfo(int clientID)
        {
            return GetCachedClientInfo()
                .FirstOrDefault(clientInfo => clientInfo.TenantGroupID == clientID);
        }

        public static string GetAccountManagerHtml(int clientID)
        {
            var cachedClientInfo = GetCachedClientInfo(clientID);
            Debug.Assert(cachedClientInfo != null);

            if (cachedClientInfo != null)
            {
                var accountManagerEmail = cachedClientInfo.AccountManagerMailAddress;

                if (accountManagerEmail != null)
                {
                    return accountManagerEmail.ToHtml();
                }
                return "none";
            }

            return string.Empty;
        }

        public static int? GetAccountManagerUserID(int clientID)
        {
            var cachedClientInfo = GetCachedClientInfo(clientID);
            Debug.Assert(cachedClientInfo != null);

            if (cachedClientInfo != null)
            {
                return cachedClientInfo.AccountManagerUserID;
            }

            return null;
        }






#if false
        public static TorqDataContext GetTorqDC(int clientID)
        {
            TorqContext torq = TorqContext.Current;

            SqlDatabase accountsDatabase = torq.AccountsDatabase;
            SqlDatabase referenceDatabase = GetReferenceDatabase(clientID);

            return TorqDataContext.GetTorq(accountsDatabase, referenceDatabase);
        }

        public static CachedTorqReferenceDataContext GetReferenceDC(Identity torqIdentity)
        {
            Debug.Assert(torqIdentity != null);
            return GetReferenceDC(torqIdentity.TenantGroupID.Value);
        }

        public static CachedTorqReferenceDataContext GetReferenceDC(int clientID)
        {
            SqlDatabase referenceDatabase = GetReferenceDatabase(clientID);
            Debug.Assert(referenceDatabase != null);
            if (referenceDatabase != null)
            {
                return TorqDataContext.GetReferenceOnly(referenceDatabase);
            }
            return null;
        }
#endif


//!! nextgen - we might need some of this code down in the Tenant class?
#if false
        partial void OnTimeZoneChanged()
        {
            this.timeZoneInfo = null;
        }
        private TimeZoneInfo timeZoneInfo;
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (this.timeZoneInfo == null && !string.IsNullOrEmpty(this.TimeZone))
                {
                    this.timeZoneInfo = TimeZones.Parse(this.TimeZone);
                }
                if (this.timeZoneInfo != null)
                {
                    return this.timeZoneInfo;
                }

                Debug.Fail("Odd, client with no specified TimeZone");

                //!! until we get TimeZone information added to the database tables
                switch (this.State)
                {
                    case "NY":
                    case "IN":
                    case "ME":
                    case "PA":
                    case "DC":
                    case "MI":
                    case "VT":
                    case "FL":
                        return TimeZones.Parse("Eastern");
                    case "IL":
                    case "IA":
                    case "ND":
                        return TimeZones.Parse("Central");
                    case "WA":
                    case "OR":
                    case "CA":
                    case "NV":
                        return TimeZones.Parse("Pacific");
                }

                Debug.Fail("Odd, no default time zone");
                return TimeZones.Parse("Pacific");
            }
        }
#endif

#if false
        public int ActiveUserAccounts
        {
            get
            {
                return User.SystemQuery(AppDC.AccountsOnlyDCInstanceYYY)
                    .Where(u => u.TenantGroupID == this.ID)
                    .Where(u => u.State == UserState.Active)
                    .Count();
            }
        }
#endif
        //!! nextgen - not needed?
#if false
        public static IEnumerable<Client> Get(string sortExpression, int startRowIndex, int maximumRows)
        {
            // basic query
            var query = from c in TorqDataContext.AccountsOnlyDCInstance.Clients
                        select c;

            // filtering

            // sorting
            query = query.SortBy(sortExpression);

            // paging
            query = query.Skip(startRowIndex).Take(maximumRows);

            // execute
            return query.ToList();
        }

        public static int GetCount()
        {
            // basic query
            var query = from c in TorqDataContext.AccountsOnlyDCInstance.Clients
                        select c;

            // filtering

            // execute
            return query.Count();
        }

        private static IQueryable<Client> filterBy(IQueryable<Client> query, string searchExpression, int parentClientId)
        {
            // if present, look for searchExpression
            if (!string.IsNullOrEmpty(searchExpression))
            {
                //!! note this isn't very efficient (http://sqlblogcasts.com/blogs/simons/archive/2008/12/18/LINQ-to-SQL---Enabling-Fulltext-searching.aspx)
                query = query.Where(c => c.Name.ToLower().Contains(searchExpression.ToLower()));
            }

            if (parentClientId >= 0)
            {
                if (parentClientId == 0)
                {
                    // zero means no parent
                    query = query.Where(q => q.ParentClientId.HasValue == false);
                }
                else
                {
                    query = query.Where(q => q.ParentClientId.HasValue == true && q.ParentClientId.Value == parentClientId);
                }
            }

            return query;
        }

        public static IQueryable<Client> GetByFilter(string searchExpression, string sortExpression, int parentClientId, int startRowIndex, int maximumRows)
        {
            // basic query
            var query = TorqDataContext.AccountsOnlyDCInstance.Clients
                .Select(client => client);

            // filtering
            query = filterBy(query, searchExpression, parentClientId);

            // sorting
            query = query.SortBy(sortExpression);

            // paging
            query = query.Skip(startRowIndex).Take(maximumRows);

            return query;
        }

        public static int GetByFilterCount(string searchExpression, int parentClientId)
        {
            // basic query
            var query = TorqDataContext.AccountsOnlyDCInstance.Clients
                .Select(client => client);

            // filtering
            query = filterBy(query, searchExpression, parentClientId);

            // execute
            return query.Count();
        }

        public static Client GetByName(string name)
        {
            Client client = null;

            try
            {
                client = TorqDataContext.AccountsOnlyDCInstance.Clients
                    .SingleOrDefault(c => c.Name == name);
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
            return client;
        }

        public static Client Create(Identity authorizedBy, string name, int? parentClientId, bool isActive, Product[] licensedProducts, ClientFeatures.Feature[] enabledFeatures, int? accountManagerUserID, string requestedReferenceDb, int seatsContracted, int concurrentSeatsContracted, string address1, string address2, string city, string state, string zip, string phone, string timeZone, string website, out string errorMessage)
        {
            try
            {
                if (!authorizedBy.IsSystemAdmin)
                {
                    errorMessage = "Insufficient permissions to create a new client account.";
                    return null;
                }

                Client existingClient = Client.GetByName(name);
                if (existingClient != null)
                {
                    errorMessage = "A client with this name already exists.";
                    return null;
                }

                Client newClient = new Client();

                // Initialize
                newClient.OnLoaded();

                //!! ClientId is not an identity field, it can't be changed to one without dropping and recreating the table
                //   so I'm doing this for now

                int lastClientId = TorqDataContext.AccountsOnlyDCInstance.Clients
                    .Max(client => client.ClientId);

                newClient.ClientId = lastClientId + 1;
                newClient.Name = name;
                newClient.ParentClientId = parentClientId;
                if (parentClientId.HasValue)
                {
                    // make sure it's a valid parent
                    //Client.CheckParent(
                }
                
                newClient.IsActive = isActive;
                newClient.LicensedProducts = licensedProducts;
                newClient.EnabledFeatures = enabledFeatures;
                newClient.AccountManagerUserID = accountManagerUserID;
                newClient.RequestedReferenceDB = requestedReferenceDb;

                newClient.NumberOfSeatsContracted = seatsContracted;
                newClient.ConcurrentSeatsContracted = concurrentSeatsContracted;
                newClient.Address1 = address1;
                newClient.Address2 = address2;
                newClient.City = city;
                newClient.State = state;
                newClient.Zip = zip;
                newClient.Phone = phone;
                newClient.TimeZone = timeZone;
                newClient.Website = website;
                //newClient.Email = email.Address;

                newClient.OnSaving();

                newClient.AccountsDBSave();

                // Dump and refresh our ClientInfo cache
                Tenant.RefreshTenantGroupInfoCache();

                errorMessage = string.Empty;
                return newClient;

            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }

            errorMessage = "Unexpected error.";
            return null;
        }
#endif

#if false
        public void UpdateAccountManager(int? accountManagerUserID)
        {
            try
            {
                this.AccountManagerUserID = accountManagerUserID;

                this.OnSaving();

                this.AccountsDBUpdate();

                // Dump and refresh our ClientInfo cache
                Tenant.RefreshTenantGroupInfoCache();
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
        }

        internal void Update(int concurrentSeatsContracted, ClientFeatures.Feature[] features)
        {
            try
            {
                this.ConcurrentSeatsContracted = concurrentSeatsContracted;
                this.EnabledFeatures = features;

                this.OnSaving();

                this.AccountsDBUpdate();

                // Dump and refresh our ClientInfo cache
                Tenant.RefreshTenantGroupInfoCache();
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
        }

        public void Update(Identity authorizedBy, bool isActive, Product[] licensedProducts, ClientFeatures.Feature[] enabledFeatures, JobPostingProvider.Providers[] enabledJobPostingProviders, CustomIdMode? customIdMode, int? accountManagerUserID, int seatsContracted, int concurrentSeatsContracted, string address1, string address2, string city, string state, string zip, string phone, string website, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (!authorizedBy.IsSystemAdmin)
                {
                    errorMessage = "Insufficient permissions to update a client account.";
                    return;
                }

                this.LicensedProducts = licensedProducts;
                this.EnabledFeatures = enabledFeatures;
                this.EnabledJobPostingProviders = enabledJobPostingProviders;
                this.CustomIdMode = customIdMode;
                this.AccountManagerUserID = accountManagerUserID;
                this.NumberOfSeatsContracted = seatsContracted;
                this.ConcurrentSeatsContracted = concurrentSeatsContracted;
                this.Address1 = address1;
                this.Address2 = address2;
                this.City = city;
                this.State = state;
                this.Zip = zip;
                this.Phone = phone;
                this.Website = website;
                //newClient.Email = email.Address;

                this.OnSaving();

                this.AccountsDBUpdate();

                // Dump and refresh our ClientInfo cache
                Tenant.RefreshTenantGroupInfoCache();
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
                errorMessage = "Unexpected error.";
           }
        }
#endif


#if false
        public static Client GetByClientID(string clientIDString)
        {
            return GetByClientID(TorqDataContext.AccountsOnlyDCInstance, clientIDString);
        }

        public static Client GetByClientID(TorqDataContext accountsDB, string clientIDString)
        {
            int clientID;
            if (int.TryParse(clientIDString, out clientID))
            {
                return GetByClientID(accountsDB, clientID);
            }
            return null;
        }

        public static Client GetByClientID(int clientID)
        {
            return GetByClientID(TorqDataContext.AccountsOnlyDCInstance, clientID);
        }

        public static Client GetByClientID(TorqDataContext accountsDB, int clientID)
        {
            try
            {
                return accountsDB.Clients
                    .FirstOrDefault(c => c.ClientId == clientID);
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }
            return null;
        }
#endif


        public static IEnumerable<ClientInfo> GetClientsWithUsers(AppDC accountsDC, bool hasUsers)
        {
            IEnumerable<ClientInfo> query = null;
            try
            {
                var clientsWithUsersQuery = User.SystemQuery(accountsDC)
                    .Where(u => u.TenantGroupID.HasValue)
                    .Select(u => u.TenantGroupID.Value)
                    .Distinct();
                var clientsWithUsers = clientsWithUsersQuery
                    .ToArray();

                if (hasUsers)
                {
                    query = GetCachedClientInfo()
                        .Where(clientInfo => clientInfo.TenantState == TenantState.Active)
                        .Where(clientInfo => clientsWithUsers.Contains(clientInfo.TenantGroupID));
                }
                else
                {
                    query = GetCachedClientInfo()
                        .Where(clientInfo => clientInfo.TenantState == TenantState.Active)
                        .Where(clientInfo => !clientsWithUsers.Contains(clientInfo.TenantGroupID));
                }

                return query.OrderBy(c => c.HierarchyName);
            }
            catch (Exception ex)
            {
                SiteContext.Current.EventLog.LogException(ex);
            }

            return query;
        }

        public static IEnumerable<ClientInfo> GetHierarchyNodes(Identity authorizedBy)
        {
            if (authorizedBy == null)
            {
                return ClientInfo.EmptyArray;
            }

            IEnumerable<ClientInfo> query = null;

            if (authorizedBy.IsSystemAdmin)
            {
                query = GetCachedClientInfo();
            }
            else if (authorizedBy.IsInRole(AppRole.AccountAdmin))
            {
                query = GetCachedClientInfo(authorizedBy.TenantGroupID.Value).Root.DescendentsAndSelf;
            }
            else
            {
                query = GetCachedClientInfo(authorizedBy.TenantGroupID.Value).ToEnumerable();
            }

            return query
                .Where(clientInfo => clientInfo.TenantState == TenantState.Active)
                .OrderBy(clientInfo => clientInfo.HierarchyName);
        }

        public static IEnumerable<ClientInfo> GetHierarchyNonLeafNodes(Identity authorizedBy)
        {
            return GetHierarchyNodes(authorizedBy)
                .Where(clientInfo => clientInfo.HasDescendents);
        }

        public static IEnumerable<ClientInfo> GetHierarchyLeafNodes(Identity authorizedBy)
        {
            return GetHierarchyNodes(authorizedBy)
                .Where(clientInfo => !clientInfo.HasDescendents);
        }

        public static IEnumerable<ClientInfo> GetHierarchyWithUsers(AppDC appDC, Identity authorizedBy)
        {
            IEnumerable<ClientInfo> query = null;
            try
            {
                // Get all clients with users and their ancestors
                query = GetClientsWithUsers(appDC, true);

                if (!authorizedBy.IsSystemAdmin)
                {
                    if (authorizedBy.IsInRole(AppRole.AccountAdmin))
                    {
                        var clientInfo = TenantGroup.GetCachedTenantGroupInfo(authorizedBy.TenantGroupID.Value) as ClientInfo;
                        query = query.Intersect(clientInfo.Root.DescendentsAndSelf).SelectMany(q => q.AncestorsAndSelf).Distinct();
                    }
                    else
                    {
                        query = query.Where(c => c.TenantGroupID == authorizedBy.TenantGroupID.Value).SelectMany(q => q.AncestorsAndSelf).Distinct();
                    }
                }
                else
                {
                    query = query.SelectMany(q => q.AncestorsAndSelf).Distinct();
                }

                // Flatten to get all clients in the hierarchy
                query = query.OrderBy(c => c.HierarchyName);

                return query;
            }
            catch (Exception ex)
            {
                SiteContext.Current.EventLog.LogException(ex);
            }

            return query;
        }
#if false
        public bool ValidateProducts(Product[] childLicensedProducts, out string errorMessage)
        {
            bool valid = true;
            errorMessage = string.Empty;

            foreach (Product prod in childLicensedProducts)
            {
                if (!this.LicensedProducts.Contains(prod))
                {
                    errorMessage += prod.ToString() + " is not available to the selected Parent Group. ";
                    valid = false;
                }
            }
            return valid;
        }
#endif
#if false
        public bool ValidateFeatures(ClientFeatures.Feature[] childEnabledFeatures, out string errorMessage)
        {
            bool valid = true;
            errorMessage = string.Empty;
            
            foreach (ClientFeatures.Feature feature in childEnabledFeatures)
            {
                if (!this.EnabledFeatures.Contains(feature))
                {
                    errorMessage += feature.ToString() + " is not available to the selected Parent Group. ";
                    valid = false;
                }
            }
            return valid;
        }
#endif

        public bool ValidateSeatsContracted(int childClientId, int childSeats, out string errorMessage)
        {
            bool valid = true;
            errorMessage = string.Empty;

            int maxSeats = this.NumberOfSeatsContracted;

            var parentClientInfo = GetCachedClientInfo(this.ID);
            Debug.Assert(parentClientInfo != null);

            int totalChildSeats = parentClientInfo
                .Descendents
                .Where(clientInfo => clientInfo.TenantGroupID != childClientId)
                .Where(clientInfo => clientInfo.TenantState == TenantState.Active)
                .Sum(clientInfo => clientInfo.NumberOfSeatsContracted);

            if ((totalChildSeats + childSeats) > maxSeats)
            {
                errorMessage = "The Contracted TORQ 3.1 Users exceeds the number available to the selected Parent Group: " + (maxSeats - totalChildSeats).ToString();
                valid = false;
            }

            return valid;
        }

        public bool ValidateConcurrentSeats(int childClientId, int childSeats, out string errorMessage)
        {
            bool valid = true;
            errorMessage = string.Empty;

            int maxSeats = this.ConcurrentSeatsContracted.GetValueOrDefault(0);

            var parentClientInfo = GetCachedClientInfo(this.ID);
            Debug.Assert(parentClientInfo != null);

            int totalChildSeats = parentClientInfo
                .Descendents
                .Where(clientInfo => clientInfo.TenantGroupID != childClientId)
                .Where(clientInfo => clientInfo.TenantState == TenantState.Active)
                .Sum(clientInfo => clientInfo.ConcurrentSeatsContracted.GetValueOrDefault(0));

            if ((totalChildSeats + childSeats) > maxSeats)
            {
                errorMessage = "The Allotted TORQ 3.1 Concurrent Users exceeds the number available to the selected Parent Group: " + (maxSeats - totalChildSeats).ToString();
                valid = false;
            }

            return valid;
        }
    }
}