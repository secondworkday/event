using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using MS.Utility;
using MS.TemplateReports;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp
{
    public class Signout : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var utilityContext = UtilityContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            var identity = context.GetIdentity();
            if (identity != null)
            {
                WebIdentityAuthentication.EndSession(identity);
            }

            response.Redirect("/", false);
            // Note: This form of Response.Redirect doesn't end the current request, so we will get here
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}