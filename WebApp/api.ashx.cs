using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Net;

using MS.Utility;
using MS.TemplateReports;
using MS.WebUtility;

using App.Library;

namespace WebApp
{
    public class Api : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var siteContext = SiteContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            DateTime requestTimestamp = context.UtcTimestamp();
            var authorizedBy = context.GetIdentity();

            using (var appDC = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(requestTimestamp, authorizedBy))
            {
                var type = request.QueryString["type"];
                var searchExpressionString = request.QueryString["searchExpression"];
                var searchExpression = SearchExpression.Create(searchExpressionString);

                switch (type)
                {
                    case "GenerateRandomStudent":
                        GenerateRandomParticipant(appDC);
                        return;

                    case "GenerateRandomSchool":
                        GenerateRandomParticipantGroup(appDC);
                        return;

                    default:
                        Debug.Fail("Unexpected type: " + type);
                        return;
                }
            }
        }

        private void GenerateRandomParticipant(AppDC appDC)
        {
            var random = Participant.GenerateRandom(appDC, 1);
        }

        private void GenerateRandomParticipantGroup(AppDC appDC)
        {
            var random = ParticipantGroup.GenerateRandom(appDC);
        }

        private void GenerateRandomData(AppDC appDC)
        {
            throw new NotImplementedException();
        }


        public bool IsReusable
        {
            get { return true; }
        }
    }
}