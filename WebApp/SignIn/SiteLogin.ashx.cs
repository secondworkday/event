using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Data.SqlClient;


using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp
{
    /// <summary>
    /// Allows clients to obtain a site auth cookie via username/password credentials
    /// </summary>
    public class SiteLogin : HubResultHttpHandler
    {
        public override HubResult ProcessRequest(HttpContext context)
        {
            var utilityContext = UtilityContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Debug.Assert(!utilityContext.RequireSslLogin || request.IsSecureConnection);

            response.ContentType = "text/plain";
            response.ContentEncoding = Encoding.UTF8;

            request.InputStream.Position = 0;
            using (var inputStream = new StreamReader(request.InputStream))
            {
                var bodyString = inputStream.ReadToEnd();

                if (string.IsNullOrEmpty(bodyString))
                {
                    context.Response.SubStatusCode = 2;
                    return HubResult.CreateError("No credentials provided.");
                }

                dynamic body = bodyString.FromJson();

                if (body == null)
                {
                    context.Response.SubStatusCode = 3;
                    return HubResult.CreateError("No credentials provided.");
                }

                try
                {
                    string authCodeValue = body.authCode;
                    if (!string.IsNullOrEmpty(authCodeValue))
                    {
                        var authCode = AuthCode.FromValue(authCodeValue);
                        var hubResult = AuthTemplate.Redeem(authCode, body);
                        return hubResult;
                    }


                    string userName = body.email;
                    string password = body.password;
                    bool rememberMe = ((bool?)body.rememberMe) ?? false;

                    if (string.IsNullOrEmpty(userName))
                    {
                        context.Response.SubStatusCode = 5;
                        return HubResult.CreateError("No email provided.");
                    }

                    var user = MS.Utility.User.AuthenticateUser(userName, password);
                    if (user != null)
                    {
                        WebIdentityAuthentication.LoginSession(user, rememberMe);
                        return HubResult.Success;
                    }


                    string userIP = request.UserHostAddress;

                    //!!!!!! consider logging back to our DB as well.

                    // log this error. Are we being hacked?
                    string loginFailureString = string.Format("{0} | User:{1} | IP:{2}\r\n",
                        /*0*/DateTime.UtcNow.ToString(),
                        /*1*/userName ?? "-none-",
                        /*2*/userIP ?? "-none-");

                    if (utilityContext != null)
                    {
                        utilityContext.EventLog.Add(new ErrorItem("LoginFailure", loginFailureString));
                    }

                    loginFailureString = string.Format("{0} | User:{1} | Password:{2} | IP:{3}\r\n",
                        /*0*/DateTime.UtcNow.ToString(),
                        /*1*/userName ?? "-none-",
                        /*2*/password ?? "-none-",
                        /*3*/userIP ?? "-none-");

                    // lock to synchronize write access to this file
                    //lock (loginFailureLogSyncRoot)
                    //{
                    //File.AppendAllText(HttpContext.Current.Server.MapPath("/App_Data/LoginFailure.log"), loginFailureString);
                    //}

                }
                catch (SqlException)
                {
                    // If this occurs with any regularity, we should have a separate error message saying we're temporarily offline.
                    utilityContext.EventLog.LogCritical("SQL error preventing logins, site: " + utilityContext.SiteName);

                    // Bad news - we can't talk to SQL.
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.SubStatusCode = 43;
                    return HubResult.CreateError("Sorry, the site is temporarily not accessible.");
                }
                catch (Exception ex)
                {
                    // If this occurs with any regularity, we should have a separate error message saying we're temporarily offline.
                    utilityContext.EventLog.LogException(ex);
                }

                context.Response.SubStatusCode = 6;
                return HubResult.CreateError(HubResult.Forbidden, "Sorry, that account was not recognized. Please try again.");
            }
        }
    }
}