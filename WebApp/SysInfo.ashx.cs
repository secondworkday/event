using System;
using System.Diagnostics;
using System.Web;
using System.Xml.Linq;

using Torq.Library;
using Torq.Library.Domain;
using Torq.Library.Security;

public class SysInfo : IHttpHandler
{
    public void ProcessRequest (HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        context.Response.CacheControl = "no-cache";

        TorqContext torq = TorqContext.Current;

        string publishedV2ReferenceDatabaseName = torq.GetPublishedReferenceDatabaseNameByNamePrefix(torq.v2_ReferenceDatabaseName);
        string publishedV3ReferenceDatabaseName = torq.GetPublishedReferenceDatabaseNameByNamePrefix(torq.v3_ReferenceDatabaseName);
        string publishedV4ReferenceDatabaseName = torq.GetPublishedReferenceDatabaseNameByNamePrefix(torq.v4_ReferenceDatabaseName);
        string publishedV6ReferenceDatabaseName = torq.GetPublishedReferenceDatabaseNameByNamePrefix(torq.v6_ReferenceDatabaseName);
        string publishedV7ReferenceDatabaseName = torq.GetPublishedReferenceDatabaseNameByNamePrefix(torq.v7_ReferenceDatabaseName);

        XDocument sysInfoDocument = new XDocument(
            new XElement("SysInfo",
                new XElement("PublishedReferenceDatabases",
                    new XElement("torq_reference_v2", publishedV2ReferenceDatabaseName),
                    new XElement("torq_reference_v3", publishedV3ReferenceDatabaseName),
                    new XElement("torq_reference_v4", publishedV4ReferenceDatabaseName),
                    new XElement("torq_reference_v6", publishedV6ReferenceDatabaseName)
                )
            )
        );

        context.Response.Write(sysInfoDocument.ToString());
    }

    private static string getPublishedReferenceDatabaseName(TorqContext torq, string referenceDatabaseNamePrefix)
    {
        string publishedReferenceDatabaseName = string.Empty;
        if (!string.IsNullOrEmpty(referenceDatabaseNamePrefix))
        {
            var referenceDatabase = torq.GetPublishedReferenceDatabaseByNamePrefix(referenceDatabaseNamePrefix);
            if (referenceDatabase != null)
            {
                publishedReferenceDatabaseName = referenceDatabase.DatabaseName;
            }
        }
        return publishedReferenceDatabaseName;
    }

    public bool IsReusable
    {
        get { return true; }
    }
}
