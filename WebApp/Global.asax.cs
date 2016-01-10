using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.IO;
using System.Web.Security;
using System.Web.SessionState;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using MS.Utility;
using MS.WebUtility;

using App.Library;
//using App.Library.Domain;

namespace WebApp
{
    public class Global : WebApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            // (Referencing a resource causes it to be loaded and therefore registered. We do this here for our Library generated EPCategories that the library framework isn't aware of.)
            //var foo = SharedEPCategory..StandardReportTitle;
            var foo = SharedEPCategory.AppAssignedCategory;

            var appInfo = AppInfo.Create("osb", "osb.socialventuresoftware.org");
            appInfo.SetProductName("OSB");
            appInfo.SetDevelopmentDomainName(".torqlab.com");
            appInfo.SetProductionSiteDomainName("torq3.torqworks.com");
            appInfo.SetEmailSignature("The TORQ Team");

            appInfo.SetOnInitialUserCreatedHandler((utilityDC, user) =>
            {
                var appDC = utilityDC as AppDC;
                Debug.Assert(appDC != null);

                new string []
                {
                    //@"CREATE NONCLUSTERED INDEX [IX_Episode_Show] ON [dbo].[Episode] ([ShowID]) INCLUDE ([TenantID])",
                    //@"CREATE NONCLUSTERED INDEX [IX_Video_Tenant_Show_Episode] ON [dbo].[Video] ([TenantID],[ShowID],[EpisodeID])",
                    //@"CREATE NONCLUSTERED INDEX [IX_ExtendedProperty_DeletedUser_TargetTable_Target_Category_ScopeType_Scope_EPType] ON [dbo].[ExtendedProperty] ([DeletedByUserID],[TargetTable],[TargetID],[Category],[ScopeType],[ScopeID],[EPType])",
                }.ForEach(sqlCommand =>
                {
                    try
                    {
                        appDC.ExecuteCommand(sqlCommand);
                    }
                    catch (Exception ex)
                    {
                        //!! wait - I don't think SiteContext.Current is setup yet is it?
                        SiteContext.Current.EventLog.LogException(ex);
                    }
                });



                //string errorMessage;
                //var mercerFamily = Tenant.Create(dc, dc.TransactionAuthorizedBy, "Mercer Family", "mercersoftware.com", TimeZones.Pacific, out errorMessage);
                //Debug.Assert(string.IsNullOrEmpty(errorMessage));
                //var actBridge = Tenant.Create(dc, dc.TransactionAuthorizedBy, "ACT Bridge", "actb.com", TimeZones.Eastern, out errorMessage);
                //var trinityHealth = Tenant.Create(dc, dc.TransactionAuthorizedBy, "Trinity Health", "trinity-health.org", TimeZones.Eastern, out errorMessage);
                //var acmeInc = Tenant.Create(dc, dc.TransactionAuthorizedBy, "Acme Inc.", "acme.com", TimeZones.Eastern, out errorMessage);
            });

            var signupPolicy = new SignupPolicy();
            var signinPolicy = new SigninPolicy();

            appInfo.SetSignupPolicy(signupPolicy);
            appInfo.SetSigninPolicy(signinPolicy);

            appInfo.AddOpenIDDomain("TORQworks", "mercersoftware.com");
            appInfo.AddOpenIDDomain("TORQworks", "torqworks.com");


            appInfo.SetPermissionHandler(permissionCheckHandler);




#if false
            var appInfo = AppInfo.Create(
                applicationAbbreviation: "actooba",

                customerVisibleDomainNameSuffix: ".actooba.com",
                productionDomainName: "www.actooba.com",

                productionKeyStoreBucket: "production.keystore.actooba.com",
                nonProductionKeyStoreBucket: "keystore.actooba.com",

                productionExternalStoreBucket: "production.externalstore.actooba.com",
                nonProductionExternalStoreBucket: "externalstore.actooba.com",

                //!! referenceBucket: "reference.backups.actooba.com",
                referenceBucket: string.Empty,
                referenceDBNamePrefix: "actooba_reference_",
                versionedReferenceDBNamePrefix: "actooba_reference_v",

                accountsBucket: "accounts.backups.actooba.com",
                accountsDBNamePrefix: "actooba_accounts_",
                anonymousAccountsDBNamePrefix: "actooba_accounts_anonback_",
                developmentAccountsDBNamePrefix: "actooba_accounts_",

                productionAccountsFullBucket: "production.backups.actooba.com",
                productionAccountsDBNameFullPrefix: "actooba_accounts_production_full_",

                productionAccountsDifferentialBucket: "differential.backups.actooba.com",
                productionAccountsDBNameDifferentialPrefix: "actooba_accounts_production_differential_",

                hubConnectionContextLookup: hubName =>
                {
                    // We use SignalR - so provide a way for the libraries we rely on to obtain a IHubConnectionContext - so they can send client notifications
                    var hubconnectionContext = GlobalHost.ConnectionManager.GetHubContext(hubName).Clients;
                    return hubconnectionContext;
                }
            );
#endif
            string rootPath = HttpContext.Current.Server.MapPath("~");
            string appDataPath = Path.Combine(rootPath, @"App_Data");

            base.OnApplicationStart(() =>
            {
                // We use SignalR - so provide a way for the libraries we rely on to obtain a IHubConnectionContext - so they can send client notifications
                //!! nextgen var connectionManager = GlobalHost.ConnectionManager;

                var connectionManager = GlobalHost.ConnectionManager;

                var siteContext = SiteContext.Create(appInfo, rootPath, appDataPath, connectionManager, AppDC.DataContextFactory);

                return siteContext;
            });

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }



        // This routine extends the MS.Utility provided "Permission" check infrastructure to include Application layer permissions
        private bool permissionCheckHandler(Identity authorizedBy, MS.Utility.Permission permission, params object[] parameters)
        {
            // better safe than sorry...
            return false;
        }

        void Application_End(object sender, EventArgs e)
        {
            base.OnApplicationEnd();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            base.OnError(sender, e);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            base.OnBeginRequest();
        }


        private bool isSpaRequestHandler(HttpContext context)
        {
            //string requestHostLower = request.Url.Host.ToLower();
            string requestPath = context.Request.Url.AbsolutePath;
            string requestPathLower = requestPath.ToLower();
            //string refererPathLower = string.Empty;
            //Uri urlReferer = request.UrlReferrer;
            //if (urlReferer != null)
            //{
            //Debug.Assert(!string.IsNullOrEmpty(urlReferer.AbsolutePath));
            //refererPathLower = (urlReferer.AbsolutePath ?? string.Empty).ToLower();
            //}

            // Html5 mode - our SPA will handle these requests

            var utilityContext = UtilityContext.Current;

            bool spaRootRequest = requestPathLower.LastIndexOf('/') == 0;
            if (spaRootRequest)
            {
                if (requestPathLower.EndsWith(".ashx"))
                {
                    spaRootRequest = false;
                }
            }


            //!! the application layer should be able to configure these paths.
            // Html5 mode - our SPA will handle these requests
            if (spaRootRequest ||
                //requestPathLower == "/" ||
                //requestPathLower == "/home" ||
                requestPathLower == "/home/" ||
                requestPathLower.StartsWith("/jobs/") ||

                //!! what is the correct one here?
                requestPathLower.StartsWith("/c/") ||
                requestPathLower.StartsWith("/pro/") ||

                requestPathLower.StartsWith("/ops/") ||
                requestPathLower.StartsWith("/database/") ||
                requestPathLower.StartsWith("/system/") ||
                requestPathLower.StartsWith("/admin/") ||
                requestPathLower.StartsWith("/goodies/") ||
                // Signup ...
                requestPathLower.StartsWith("/signup/") ||
                // AuthTokens ...
                requestPathLower.StartsWith("/resetpassword/") ||
                // CareerStep ...
                requestPathLower.StartsWith("/my-plan/") ||
                // Occupations ...
                requestPathLower.StartsWith("/occupations/") ||
                // Conversations ...
                requestPathLower.StartsWith("/messages/") ||
                requestPathLower.StartsWith("/inbox")
                )
            {
                context.RewritePath("/app/default.aspx");
                return true;
            }

            return false;
        }

        public void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            base.OnAuthorizeRequest(isSpaRequestHandler);
        }

        public void Application_EndRequest(object sender, EventArgs e)
        {
            base.OnEndRequest();
        }
    }
}