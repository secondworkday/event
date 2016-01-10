using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp.Bootstrap
{
    public partial class Default : SitePage
    {
        protected string SiteConfigData;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var utilityContext = UtilityContext.Current;

            if (utilityContext.RequireSslLogin)
            {
                this.Context.RedirectToSecureConnection();
                Debug.Assert(this.Request.IsSecureConnection);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //!! this.DemoUsersJson = MS.Utility.User.DemoQuery().ToJson();
            // this.DemoUsersJson = "{ test:'Hi there' }";

            var utilityContext = UtilityContext.Current;


            var master = this.Master as AngularJS_SignalR;
            Debug.Assert(master != null);

            master.includeAngularMaterial = true;

            TimeZoneInfo timeZoneInfo = TimeZones.Parse("Pacific");

            this.SiteConfigData = new {


                //data: { allowedRoles: [AUTHORIZATION_ROLES.anonymous] }
                allowedRoles = new string[] { "Anonymous" },


                //** Values from the running application

                serverStart = utilityContext.ServerStart.ToNowRelativeString(timeZoneInfo, DateTimeDisplayFormat.AbbreviationTimeZone),
                serverUptime = utilityContext.ServerUptime.ToBucketPhrase(),
                applicationFolder = Request.PhysicalApplicationPath,
                systemAccount = Environment.UserName,


                sqlUserLogin = UtilityContext.Current.SqlUserLogin,
                sqlAdminUserLogin = UtilityContext.Current.SqlAdminUserLogin,

                //** Values from site.config file (if any)

                siteName = utilityContext.SiteName,
                awsAccessKey = utilityContext.AwsAccessKey,
                awsSecretAccessKey = utilityContext.AwsSecretAccessKeyMasked,

                localServerName = utilityContext.LocalServerName,

                localServerSqlAdminName = utilityContext.LocalServerSqlAdminName,
                localServerSqlAdminPasswordMasked = utilityContext.LocalServerSqlAdminPasswordMasked,

                localRuntimeServerUserPasswordMasked = utilityContext.LocalRuntimeServerUserPasswordMasked,
                localRuntimeServerAdminPasswordMasked = utilityContext.LocalRuntimeServerAdminPasswordMasked,
                remoteRuntimeServerUserPasswordMasked = utilityContext.RemoteRuntimeServerUserPasswordMasked,
                remoteRuntimeServerAdminPasswordMasked = utilityContext.RemoteRuntimeServerAdminPasswordMasked,

            }.ToJson();


        }
    }
}