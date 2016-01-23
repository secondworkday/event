using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Net.Mail;
using System.Web;
using System.Threading;
using System.Configuration;
using System.Runtime.Serialization;

using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

using MS.Utility;
using MS.WebUtility;

namespace App.Library
{
    public class SiteContext : WebUtilityContext
    {
        public new static SiteContext Current
        {
            get { return UtilityContext.Current as SiteContext; }
        }

        public string AppDataFolder { get { return base.AppDataPath; } }

        protected SiteContext(UtilityContextSerializer contextDataSerializer, AppInfo appInfo, string siteRootPath, string appDataPath, IConnectionManager connectionManager, IEnumerable<DataContextFactory> dataContextFactories, IEnumerable<WebAuthTemplate> webAuthTemplates)
            : base(contextDataSerializer, appInfo, siteRootPath, appDataPath, connectionManager, dataContextFactories, webAuthTemplates)
        {
        }

        public static SiteContext Create(AppInfo appInfo, string rootPath, string appDataPath, params DataContextFactory[] dataContextFactories)
        {
            return SiteContext.Create(appInfo, rootPath, appDataPath, null, dataContextFactories);
        }

        /// <summary>
        /// Optional call to initialize a SiteContext for callers that don't create their own. 
        /// </summary>
        /// <param name="appInfo"></param>
        /// <param name="rootPath"></param>
        /// <param name="appDataPath"></param>
        /// <param name="connectionManager"></param>
        /// <param name="dataContextFactories"></param>
        /// <returns></returns>
        public static new SiteContext Create(AppInfo appInfo, string rootPath, string appDataPath, IConnectionManager connectionManager, params DataContextFactory[] dataContextFactories)
        {
            var contextDataFileName = Path.Combine(appDataPath, @"site.config");
            var utilityContextSerializer = new UtilityContextSerializer<WebUtilityContextData>(contextDataFileName);

            var webAuthTemplates = new WebAuthTemplate[] 
            {
                //!! nextgen - add in the compelte list here
                //LoginAuthTemplate.Instance,
                //CreateSsoUserAuthTemplate.Instance,
                //CreateUserAuthTemplate.Instance
            };

            var siteContext = new SiteContext(utilityContextSerializer, appInfo, rootPath, appDataPath, connectionManager, dataContextFactories, webAuthTemplates);
            Debug.Assert(siteContext != null);

            if (siteContext == null)
            {
                return null;
            }

            Debug.Assert(UtilityContext.Current == null);
            UtilityContext.Current = siteContext;
            Debug.Assert(UtilityContext.Current != null);
            Debug.Assert(UtilityContext.Current == siteContext);

            siteContext.PublishDefaultDatabases();

            //-- Add app-specific startup configuration here
            if (siteContext.AccountsDatabase != null)
            {
                //siteContext.CreateDefaultAccountsOnlyDCSingleTenantJob<AppDC>(DateTime.UtcNow, (dc) =>
                    //{
                    //});
            }

            siteContext.JobManager.Run();

            return siteContext;
        }

        public string ImagesFolder
        {
            get
            {
                string folder = Path.Combine(this.RootPath, "Images");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }
    }
}