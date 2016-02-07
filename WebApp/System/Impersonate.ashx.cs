using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

using App.Library;

namespace WebApp
{
    /// <summary>
    /// Summary description for Impersonate
    /// </summary>
    public class Impersonate : HubResultHttpHandler
    {
        public override HubResult ProcessRequest(HttpContext context)
        {
            Identity authorizedBy = context.GetIdentity();
            DateTime requestTimestamp = context.UtcTimestamp();

            // Only these roles should be attempting impersonate, right?
            Debug.Assert(authorizedBy.IsSystemAdmin || authorizedBy.IsTenantAdmin);

            var utilityContext = UtilityContext.Current;
            using (var dc = utilityContext.CreateDefaultAccountsOnlyDC<AppDC>(requestTimestamp, authorizedBy))
            {
                var tenantID = context.Request.QueryString.GetNullableInt32("tid");
                var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantID);
                if (tenantGroupInfo != null)
                {
                    WebAuthentication.AccessCheckOrRedirectToLoginPage(authorizedBy, SystemRole.SystemAdmin.ToEnumerable());

                    MS.WebUtility.Authentication.WebIdentityAuthentication.ImpersonateSystem(authorizedBy, tenantGroupInfo);
                    return HubResult.Success;
                }

                // Make sure we've got authoritity to impersonate
                WebAuthentication.AccessCheckOrRedirectToLoginPage(authorizedBy, SystemRole.SystemAdmin.ToEnumerable());
                Debug.Assert(context.User.IsInRole("SystemAdmin"));

                User impersonatedUser = context.Request.GetUserOrTransfer(dc, "uid", "/default.aspx");
                WebIdentityAuthentication.ImpersonateUser(dc, authorizedBy, impersonatedUser);
                return HubResult.Success;
            }
        }
    }
}