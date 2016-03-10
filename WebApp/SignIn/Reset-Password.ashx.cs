using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Data.SqlClient;

using MS.Utility;
using MS.TemplateReports;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp
{
    public class ResetPassword : HubResultHttpHandler
    {
        public override HubResult ProcessRequest(HttpContext context)
        {
            var utilityContext = UtilityContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Debug.Assert(!utilityContext.RequireSslLogin || request.IsSecureConnection);

            request.InputStream.Position = 0;
            using (var inputStream = new StreamReader(request.InputStream))
            {
                var bodyString = inputStream.ReadToEnd();
                dynamic body = bodyString.FromJson();

                string authCodeString = body.authCode;
                string newPassword = body.newPassword;

                bool rememberMe = ((bool?)body.rememberMe) ?? false;

                if (string.IsNullOrEmpty(authCodeString) || string.IsNullOrEmpty(newPassword))
                {
                    return HubResult.NotFound;
                }

                //!! do we need to use this flavor?
                //!!var authCode2 = WebAuthTemplate.ParseAuthCode(authTokenTypeName, authCodeValue);
                var authCode = AuthCode.FromValue(authCodeString);

                try
                {
                    User passwordResetUser;
                    var hubResult =  MS.Utility.User.ResetPassword(authCode, newPassword, out passwordResetUser);
                    if (hubResult == HubResult.Success)
                    {
                        Debug.Assert(passwordResetUser != null);
                        WebIdentityAuthentication.LoginSession(passwordResetUser, rememberMe);
                    }

                    return hubResult;
                }
                catch (SqlException)
                {
                    // Bad news - we can't talk to SQL.

                    // If this occurs with any regularity, we should have a separate error message saying we're temporarily offline.
                    utilityContext.EventLog.LogCritical("SQL error preventing passwordResets, site: " + utilityContext.SiteName);

                    return HubResult.CreateError("Sorry, the site is temporarily not accessible.");
                }
            }
        }
    }
}