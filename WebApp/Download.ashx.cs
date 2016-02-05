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
using System.IO;

namespace WebApp
{
    public class Download : IHttpHandler
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

                //!! nextgen - implement these additional parameters too
                //string onetCode = Request.QueryString["onet"];
                // opens a new window if it's an attachment
                //bool isAttachment = Request.QueryString.GetBoolean("attach", true);
                //string templateName = Request.QueryString["template"] ?? "Standard";
                //string[] reportCategories = Request.QueryString.GetStringsOrNull("rc");

                switch (type)
                {
                    case "tenants":
                        var tenantsRowData = TenantGroup.GetExportRows(appDC);
                        response.SendCsvFileToBrowser("Tenants.csv", tenantsRowData);
                        return;

                    case "users":
                        var usersRowData = User.GetExportRows(appDC);
                        response.SendCsvFileToBrowser("Users.csv", usersRowData);
                        return;

                    case "reminderForm":
                        var participantID = request.QueryString.GetNullableInt32("id");
                        if (participantID.HasValue)
                        {
                            downloadReminderForm(response, appDC, siteContext, participantID.Value);
                            return;
                        }
                        return;


                    case "eventParticipants":
                        EventParticipant.GetExportRows(response, appDC);
                        return;



                    case "reminderFormForSchool":
                        var participantGroupID = request.QueryString.GetNullableInt32("participantGroupID");
                        if (participantGroupID.HasValue)
                        {
                            downloadReminderFormForSchool(response, appDC, siteContext, participantGroupID.Value);
                            return;
                        }
                        return;
                    case "reminderFormForEventParticipant":
                        participantGroupID = request.QueryString.GetNullableInt32("participantGroupID");
                        var eventSessionID = request.QueryString.GetNullableInt32("eventSessionID");
                        if (participantGroupID.HasValue && eventSessionID.HasValue)
                        {
                            downloadReminderFormForEventParticipants(response, appDC, siteContext, participantGroupID.Value, eventSessionID.Value);
                            return;
                        }
                        return;
#if false
                    case "clients":
                        var clientsRowData = Client.GetExportRows(appDC, searchExpression);
                        response.SendCsvFileToBrowser("Clients.csv", clientsRowData);
                        return;

                    case "projects":
                        var projectsRowData = Project.GetExportRows(appDC, searchExpression);
                        response.SendCsvFileToBrowser("Projects.csv", projectsRowData);
                        return;

                    case "projectReport":
                        var reportFormat = request.QueryString.GetReportFormat("format");
                        var projectID = request.QueryString.GetNullableInt32("id");

                        if (projectID.HasValue)
                        {
                            exportProjectReport(response, appDC, siteContext, projectID.Value, reportFormat);
                            return;
                        }
                        //!! next gen hmm probably need some form of error page or something to handle exports gone wrong
                        return;
#endif
                    default:
                        Debug.Fail("Unexpected type: " + type);
                        return;
                }
            }
        }
#if false
        private void exportProjectReport(HttpResponse response, AppDC appDC, SiteContext siteContext, int projectID, ReportFormat reportFormat)
        {
            var reportGenerator = Project.GetReportGenerator(appDC, siteContext, projectID, reportFormat);

            if (reportGenerator != null)
            {
                //!! string reportName = item.PatientName + " " + templateName + " Report" + reportGenerator.ReportFileExtension;

                //IEnumerable<string> reportCategories = this.GetJobCounselorReportCategories();
                IEnumerable<string> reportCategories = null;

                var reportStream = reportGenerator.Generate(reportCategories);
                reportStream.Position = 0;
                response.DownloadToBrowser("ProjectReport" + reportGenerator.ReportFileExtension, reportGenerator.ReportContentType, reportStream);
            }
        }
#endif

        private void downloadReminderFormForSchool(HttpResponse response, AppDC appDC, SiteContext siteContext, int participantGroupID)
        {
            var participantIDs = ParticipantGroup.GetParticipantsYYY(appDC, participantGroupID);

            List<MemoryStream> individualPdfReportStreams = new List<MemoryStream>();

            var reportFileExtension = ".pdf";
            var reportContentType = "pdf";

            foreach (int pID in participantIDs)
            {
                var reportGenerator = Participant.GetReportGenerator(appDC, "OSB-Reminder-Form-2.docx", ReportFormat.Pdf, pID);
                reportFileExtension = reportGenerator.ReportFileExtension;
                reportContentType = reportGenerator.ReportContentType;

                if (reportGenerator != null)
                {
                    var reportStream = reportGenerator.Generate();
                    //reportStream.Position = 0;
                    individualPdfReportStreams.Add(reportStream);
                }
            }

            var multipagePdfStream = PdfPlayground.MergePdfDocuments(individualPdfReportStreams);
            multipagePdfStream.Position = 0;
            response.DownloadToBrowser("ReminderForm" + DateTime.Now.ToString() + reportFileExtension, reportContentType, multipagePdfStream);
        }

        private void downloadReminderFormForEventParticipants(HttpResponse response, AppDC appDC, SiteContext siteContext, int participantGroupID, int eventSessionID)
        {
            var searchExpressionString = string.Format("$participantGroup:{0} $eventSession:{1}",
                /*0*/ participantGroupID,
                /*1*/ eventSessionID);
            var searchExpression = SearchExpression.Create(searchExpressionString);
            var eventParticipantIDsQuery = EventParticipant.Query(appDC, searchExpression)
                .Select(eventParticipant => eventParticipant.ID);

            List<MemoryStream> individualPdfReportStreams = new List<MemoryStream>();
            var reportFileExtension = ".pdf";
            var reportContentType = "pdf";

            foreach (int epID in eventParticipantIDsQuery)
            {
                var reportGenerator = EventParticipant.GetReportGenerator(appDC, "OSB-Reminder-Form-2.docx", ReportFormat.Pdf, epID);
                reportFileExtension = reportGenerator.ReportFileExtension;
                reportContentType = reportGenerator.ReportContentType;

                if (reportGenerator != null)
                {
                    var reportStream = reportGenerator.Generate();
                    individualPdfReportStreams.Add(reportStream);
                }
            }

            var multipagePdfStream = PdfPlayground.MergePdfDocuments(individualPdfReportStreams);
            multipagePdfStream.Position = 0;
            response.DownloadToBrowser("ReminderForm" + DateTime.Now.ToString() + reportFileExtension, reportContentType, multipagePdfStream);
        }

        private void downloadReminderForm(HttpResponse response, AppDC appDC, SiteContext siteContext, int itemID)
        {
            var reportGenerator = Participant.GetReportGenerator(appDC, "OSB-Reminder-Form-2.docx", ReportFormat.Pdf, itemID);

            if (reportGenerator != null)
            {
                var reportStream = reportGenerator.Generate();
                reportStream.Position = 0;

                response.DownloadToBrowser("ReminderForm" + DateTime.Now.ToString() + reportGenerator.ReportFileExtension, reportGenerator.ReportContentType, reportStream);
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}