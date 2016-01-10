using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

using Torq.Library;
using Torq.Library.Domain;

namespace WebApp
{
    /// <summary>
    /// Summary description for Impersonate
    /// </summary>
    public class Impersonate : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            Identity authorizedBy = context.GetIdentity();
            DateTime requestTimestamp = context.UtcTimestamp();

            // Only these roles should be attempting impersonate, right?
            Debug.Assert(authorizedBy.IsSystemAdmin || authorizedBy.IsTenantAdmin);

            var utilityContext = UtilityContext.Current;
            using (var dc = utilityContext.CreateDefaultAccountsOnlyDC<AppDC>(requestTimestamp, authorizedBy))
            {
                var tenantID = context.Request.QueryString.GetNullableInt32("tid");
                if (tenantID.HasValue)
                {
                    WebAuthentication.AccessCheckOrRedirectToLoginPage(authorizedBy, SystemRole.SystemAdmin.ToEnumerable());

                    var tenant = TenantGroup.FindByID(dc, tenantID.Value);
                    MS.WebUtility.Authentication.WebIdentityAuthentication.ImpersonateSystem(authorizedBy, tenant);
                    return;
                }

                // Make sure we've got authoritity to impersonate
                WebAuthentication.AccessCheckOrRedirectToLoginPage(authorizedBy, SystemRole.SystemAdmin.ToEnumerable());
                Debug.Assert(context.User.IsInRole("SystemAdmin"));

                User impersonatedUser = context.Request.GetUserOrTransfer(dc, "uid", "/default.aspx");
                WebIdentityAuthentication.ImpersonateUser(dc, authorizedBy, impersonatedUser);
                return;
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}