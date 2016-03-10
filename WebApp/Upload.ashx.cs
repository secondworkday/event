using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;

using MS.Utility;
using MS.WebUtility;

using Torq.Library;
using Torq.Library.Domain;
using Torq.Library.Education;

namespace WebApp
{
    public class Upload : HubResultHttpHandler
    {
        // upload.ashx?type=userprofilephoto
        // upload.ashx?type=tenantlogo
        public override HubResult ProcessRequest(HttpContext context)
        {
            var siteContext = TorqContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            if (request.Files.Count == 0)
            {
                return HubResult.Error;
            }

            DateTime requestTimestamp = context.UtcTimestamp();
            var authorizedBy = context.GetIdentity();

            using (var appDC = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(requestTimestamp, authorizedBy))
            {
                HttpFileCollection files = request.Files;
                HttpPostedFile httpPostedFile = files[0];

                var type = request.Form["type"];
                var subType = request.Form["subType"];
                var tenantID = authorizedBy.TenantGroupID.Value;
                var itemCode = request.Form["code"];
                var itemID = request.Form.GetNullableInt32("id");

                switch (type)
                {
                    case "userProfilePhoto":
                        if (!itemID.HasValue)
                        {
                            return;
                        }
                        User.SetProfilePhoto(appDC, itemID.Value, (ulong)httpPostedFile.ContentLength, httpPostedFile.ContentType, httpPostedFile.InputStream);
                        break;

                    case "careerProfilePhoto":
                        if (!itemID.HasValue)
                        {
                            return;
                        }
                        CareerProfile.SetProfilePhoto(appDC, itemID.Value, (ulong)httpPostedFile.ContentLength, httpPostedFile.ContentType, httpPostedFile.InputStream);
                        break;

                    case "occupationHeroImage":
                        OccupationCode occupationCode = OccupationCode.Create(itemCode);
                        if (occupationCode == null)
                        {
                            return;
                        }

                        using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v", requestTimestamp, authorizedBy))
                        {
                            siteContext.OccupationAuxiliaryData.SetHeroImage(referenceDC, occupationCode, httpPostedFile);
                        }

                        break;

                    case "tenantLogo":
                        Settings.AddPublicBlobResource(appDC, EPCategory.TenantLogoResource, httpPostedFile);
                        // This resource is part of the authenticatedTenant data - so we push out a notification
                        TenantGroup.NotifyClients(appDC, SearchExpression.Create(tenantID));

                        //!! Tenant.SetLogo(appDC, tenantID, (ulong)httpPostedFile.ContentLength, httpPostedFile.ContentType, httpPostedFile.InputStream);
                        break;

                    case "standardReportLogo":
                        Settings.AddPrivateBlobResource(appDC, SharedEPCategory.StandardReportLogoImage, httpPostedFile);
                        break;

                    case "wioaEducationData":

                        using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v", requestTimestamp, authorizedBy))
                        using(var postedFileStream = httpPostedFile.InputStream)
                        {

                            HubResult hubResult;

                            switch (subType)
                            {
                                case "providers":
                                    hubResult = StateWioaEducationProviderItem.Load(referenceDC, httpPostedFile);
                                    break;
                                case "programs":
                                    hubResult = StateWioaEducationProgramItem.Load(referenceDC, httpPostedFile);
                                    break;
                                default:
                                    Debug.Fail("Unexpected SubType: " + subType);
                                    hubResult = HubResult.NotFound;
                                    break;
                            }

                            response.StatusCode = hubResult.HttpStatusCode;
                            response.TrySkipIisCustomErrors = true;

                            var hubResultString = hubResult.ToJson();
                            Debug.Assert(!string.IsNullOrEmpty(hubResultString));

                            response.ContentType = "application/json";
                            response.ContentEncoding = Encoding.UTF8;

                            response.Write(hubResultString);
                        }
                        break;

                    default:
                        Debug.Fail("Unexpected type: " + type);
                        return HubResult.CreateError("Unexpected type: " + type);
                }
            }
        }
    }
}