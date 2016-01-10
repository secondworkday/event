using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Net;

//!! add when I get internet back...  using Amazon.Runtime;

using MS.Utility;
using MS.WebUtility;

using App.Library;

namespace WebApp
{
    /// <summary>
    /// Summary description for Resource
    /// </summary>
    public class ResourceHandler : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var siteContext = SiteContext.Current;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            var authorizedBy = context.GetIdentity();

            if (authorizedBy == null)
            {
                //!! currently using profile photos on the home

                //response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //return;
            }


            //!! Beef this up to handle whatever other type of custom resources we dream up.
            //   request.PathInfo holds the sub-path we need to parse

            string requestPathInfo = request.PathInfo;
            Debug.Assert(requestPathInfo.StartsWith("/"));
            var s3ObjectName = requestPathInfo.TrimStart('/');


            S3DownloadItem s3DownloadItem = null;
            try
            {
                s3DownloadItem = await Resource.GetResourceAsync(authorizedBy, s3ObjectName);
            }
#if false
            catch (AmazonServiceException amazonServiceException)
            {
                // (Happens if we're running with no internet connection)
                Debug.Assert(webException.Status == WebExceptionStatus.ProtocolError);

                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }
#endif
            catch (Exception)
            {
                // (Happens if we're running with no internet connection)
                //!! Debug.Fail("Huh?");

                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }

            if (s3DownloadItem == null)
            {
                context.Response.Redirect("http://api.randomuser.me/portraits/med/men/54.jpg", true);
                return;
            }

            if (!string.IsNullOrEmpty(s3DownloadItem.ETag))
            {
                response.AddHeader("ETag", s3DownloadItem.ETag);
            }
            if (s3DownloadItem.LastModifiedTimestamp > DateTimeExtensions.UnixMinValue)
            {
                Debug.Assert(s3DownloadItem.LastModifiedTimestamp.Kind == DateTimeKind.Utc);
                response.AddHeader("Last-Modified", s3DownloadItem.LastModifiedTimestamp.ToString("R"));
            }
            response.ContentType = s3DownloadItem.ContentType;
            context.Response.BinaryWrite(s3DownloadItem.MemoryStream.ToArray());

            response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}