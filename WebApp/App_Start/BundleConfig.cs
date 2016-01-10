using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

using MS.WebUtility;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254726
        public static void RegisterBundles(BundleCollection bundles)
        {
            var webUtilityContext = WebUtilityContext.Current;

            //!! using Bundle instead of ScriptBundle because our controllers aren't coded to be minified yet
            var clientJS = new Bundle("~/bundles/clientJS", new CacheBreakTransform())
                .IncludeDirectory("~/client", "*.js", true);
            bundles.Add(clientJS);

            var clientTemplates = new NgTemplateBundle("myApp", "~/bundles/clientTemplates")
                .IncludeDirectory("~/client", "*.html", true);
            bundles.Add(clientTemplates);

            // Turn on Optimizations for customer visible sites
            BundleTable.EnableOptimizations = !webUtilityContext.IsDevelopmentSite;

            RouteTable.Routes.AddBundleRoutes();
        }
    }
}