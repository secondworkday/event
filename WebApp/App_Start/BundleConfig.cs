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

            // Bundle up our client *.JS files so they can be fetched in a single request
            //!! using Bundle instead of ScriptBundle because our controllers aren't coded to be minified yet
            var defaultSpaJS = new Bundle("~/bundles/defaultSpaJS", new CacheBreakTransform())
                .IncludeDirectory("~/client/components", "*.js", true)

                .IncludeDirectory("~/client/app", "*.js", false)
                .IncludeDirectory("~/client/app/public", "*.js", true)
                .IncludeDirectory("~/client/app/system", "*.js", true)
                //.IncludeDirectory("~/client/states/public", "*.js", true)

                .IncludeDirectory("~/client/states/app", "*.js")
                .IncludeDirectory("~/client/states/app/user", "*.js", true)
                .IncludeDirectory("~/client/states/app/site-admin", "*.js", true)
                ;
            bundles.Add(defaultSpaJS);

            // Bundle up our Angular Templates. Search for 'clientTemplates' to see where they are served.
            var defaultSpaTemplates = new NgTemplateBundle("myApp", "~/bundles/defaultSpaTemplates")
                .IncludeDirectory("~/client/components", "*.html", true)

                .IncludeDirectory("~/client/app", "*.html")
                .IncludeDirectory("~/client/app/public", "*.html", true)
                .IncludeDirectory("~/client/app/system", "*.html", true)
                //.IncludeDirectory("~/client/states/public", "*.html", true)

                .IncludeDirectory("~/client/states/app", "*.html")
                .IncludeDirectory("~/client/states/app/user", "*.html", true)
                .IncludeDirectory("~/client/states/app/site-admin", "*.html", true)
                ;
            bundles.Add(defaultSpaTemplates);


            // Add SPA-specific bundles for AppRole.EventSessionVolunteer
            var volunteerSpaJS = new Bundle("~/bundles/volunteerSpaJS", new CacheBreakTransform())
                .IncludeDirectory("~/client/components", "*.js", true)

                //.IncludeDirectory("~/client/states/public", "*.js", true)
                .IncludeDirectory("~/client/app/public", "*.js", true)

                .IncludeDirectory("~/client/states/app", "*.js")
                .IncludeDirectory("~/client/states/app/event-session-volunteer", "*.js", true)
                .Include("~/client/states/app/user/event-session.controller.js")
                .Include("~/client/states/app/user/event-participants.controller.js")
                ;
            bundles.Add(volunteerSpaJS);

            // Bundle up our Angular Templates. Search for 'clientTemplates' to see where they are served.
            var volunteerSpaTemplates = new NgTemplateBundle("myApp", "~/bundles/volunteerSpaTemplates")
                .IncludeDirectory("~/client/components", "*.html", true)

                //.IncludeDirectory("~/client/states/public", "*.html", true)
                .IncludeDirectory("~/client/app/public", "*.html", true)

                .IncludeDirectory("~/client/states/app", "*.html")
                .IncludeDirectory("~/client/states/app/event-session-volunteer", "*.html", true)
                ;
            bundles.Add(volunteerSpaTemplates);

            // Turn on Optimizations for customer visible sites
            BundleTable.EnableOptimizations = !webUtilityContext.IsDevelopmentSite;

            RouteTable.Routes.AddBundleRoutes();
        }
    }
}