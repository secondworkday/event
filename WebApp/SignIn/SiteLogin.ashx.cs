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
    public class SiteLogin : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
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
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.SubStatusCode = 2;
                    response.Write("No credentials provided.");
                    return;
                }

                dynamic body = bodyString.FromJson();

                if (body == null)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.SubStatusCode = 3;
                    response.Write("No credentials provided.");
                    return;
                }

                try
                {
                    string userName = body.email;
                    string password = body.password;
                    bool rememberMe = ((bool?)body.rememberMe) ?? false;

                    if (string.IsNullOrEmpty(userName))
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.SubStatusCode = 5;
                        response.Write("No email provided.");
                        return;
                    }

                    var user = MS.Utility.User.AuthenticateUser(userName, password);
                    if (user != null)
                    {
                        WebIdentityAuthentication.LoginSession(user, rememberMe);
                        return;
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
                    // Bad news - we can't talk to SQL.
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.SubStatusCode = 43;
                    response.Write("Sorry, the site is temporarily not accessible");

                    // If this occurs with any regularity, we should have a separate error message saying we're temporarily offline.
                    utilityContext.EventLog.LogCritical("SQL error preventing logins, site: " + utilityContext.SiteName);
                    return;
                }
                catch (Exception ex)
                {
                    // If this occurs with any regularity, we should have a separate error message saying we're temporarily offline.
                    utilityContext.EventLog.LogException(ex);
                }


                response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.SubStatusCode = 6;
                response.Write("Sorry, that account was not recognized. Please try again.");
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}