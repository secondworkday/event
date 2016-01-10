using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;
using System.Reflection;

using MS.Utility;
using MS.WebUtility;

namespace App.Library
{
    public enum AppRole
    {
        HRManager,
        HRRecruiter,
        HRAssistant,
    }

    partial class AppDC
    {
        public static new LinqAttributeDataContextFactory<AppDC> DataContextFactory { get; private set; }

        static AppDC()
        {
            DataContextFactory = new LinqAttributeDataContextFactory<AppDC>(onDatabaseCreated, DataContextBase.CreateInitialUser, anonymizeDatabase, runDatabaseMigration, runDatabaseDiagnostics);
        }

        private static void onDatabaseCreated(UtilityContext utilityContext, AppDC dc, string importDatabaseConnectionString)
        {
            new string []
            {
                @"CREATE NONCLUSTERED INDEX [IX_Episode_Show] ON [dbo].[Episode] ([ShowID]) INCLUDE ([ScopeType],[ScopeID])",
                @"CREATE NONCLUSTERED INDEX [IX_Video_Tenant_Show_Episode] ON [dbo].[Video] ([ScopeType],[ScopeID],[ShowID],[EpisodeID])",
                @"CREATE NONCLUSTERED INDEX [IX_ExtendedProperty_DeletedUser_TargetTable_Target_Category_ScopeType_Scope_EPType] ON [dbo].[ExtendedProperty] ([DeletedByUserID],[TargetTable],[TargetID],[CategoryID],[ScopeType],[ScopeID],[EPType])",

                // Unique Constraints
                @"ALTER TABLE [dbo].[Show] ADD CONSTRAINT [AK_Provider] UNIQUE ([Provider], [ProviderID])",
            }.ForEach(sqlCommand =>
            {
                try
                {
                    dc.ExecuteCommand(sqlCommand);
                }
                catch (Exception ex)
                {
                    utilityContext.EventLog.LogException(ex);
                }
            });
        }


        private static void anonymizeDatabase(UtilityContext utilityContext, AppDC dc)
        {
            //!! TODO Add code to anonymize DB - careful to handle cross-tenant issues
            User.SystemQuery(dc).ForEach(user =>
            {
                user.FirstName = user.ID.ToString() + "-First";
            });

            dc.SubmitChanges();
        }

        private static void runDatabaseMigration(UtilityContext utilityContext, AppDC dc)
        {
            Debug.Assert(utilityContext != null);
            Debug.Assert(dc != null);
        }

        private static void runDatabaseDiagnostics(UtilityContext utilityContext, AppDC dc)
        {
        }
    }
}