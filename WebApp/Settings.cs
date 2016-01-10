using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

using Torq.Library;
using Torq.Library.Domain;

namespace WebApp
{
    public static class Settings
    {
        private static SystemRole[] allowedSystemRoles = new [] {SystemRole.SystemAdmin, SystemRole.TenantAdmin};
        private static string[] allowedAppRoles = null;

        private static AuthenticatedSyncLock syncLock = new AuthenticatedSyncLock(allowedSystemRoles, allowedAppRoles, false);

        public static HubResult GetSettings(Identity authorizedBy, int tentantGroupID)
        {
            return readLock(authorizedBy, () =>
            {
                //!! hmm - might be overkill to use an AuthenticatedSyncLock here is this PermissionCheck should encompass it all
                if (!authorizedBy.PermissionCheck(MS.Utility.Permission.TenantGroupSettings, tentantGroupID))
                {
                    return HubResult.Forbidden;
                }

                var settingsObject = Settings.getSettings(tentantGroupID);
                if (settingsObject != null)
                {
                    return HubResult.CreateSuccessData(settingsObject);
                }
                return HubResult.Error;
            });
        }

        public static HubResult AddTeamUrlResource(AppDC dc, EPCategory epCategory, string name, string urlString)
        {
            var notificationTenantGroupID = dc.TransactionAuthorizedBy.TenantID.Value;

            return writeLock(dc, notificationTenantGroupID, () =>
            {
                var authorizedBy = dc.TransactionAuthorizedBy;
                Debug.Assert(authorizedBy != null);

                var epScope = authorizedBy.TeamEPScopeOrInvalid;

                switch (epScope.ScopeType)
                {
                    case ExtendedPropertyScopeType.TenantGroupID:
                        var tenantID = authorizedBy.TenantGroupID;

                        Uri url;
                        if (Uri.TryCreate(urlString, UriKind.Absolute, out url))
                        {
                            TenantGroup.AddTeamUrlResource(dc, epScope.ID.Value, epCategory, name, url);
                            return HubResult.Success;
                        }
                        //!! should be URL invalid!;
                        return HubResult.UrlInvalid;

                    case ExtendedPropertyScopeType.UserID:
                    default:
                        Debug.Fail("Unexpected ScopeType: " + epScope.ScopeType);
                        return HubResult.Error;
                }
            });
        }


        public static HubResult AddPublicBlobResource(AppDC dc, EPCategory epCategory, HttpPostedFile httpPostedFile)
        {
            return AddPublicBlobResource(dc, epCategory, (ulong)httpPostedFile.ContentLength, httpPostedFile.ContentType, httpPostedFile.InputStream);
        }

        public static HubResult AddPublicBlobResource(AppDC dc, EPCategory epCategory, ulong contentLength, string contentType, System.IO.Stream stream)
        {
            var notificationTenantGroupID = dc.TransactionAuthorizedBy.TenantID.Value;

            return writeLock(dc, notificationTenantGroupID, () =>
            {
                var authorizedBy = dc.TransactionAuthorizedBy;
                Debug.Assert(authorizedBy != null);

                var epScope = authorizedBy.TeamEPScopeOrThrow;
                Debug.Assert(epScope.ScopeType == ExtendedPropertyScopeType.TenantGroupID);

                switch (epScope.ScopeType)
                {
                    case ExtendedPropertyScopeType.TenantGroupID:

                        //!! what's a good namespace strategy here?
                        string s3ObjectName = string.Format("tenants/{0}/resources/{1}",
                            /*0*/ epScope.ID.Value,
                            // f2165312-5e7e-414a-97ba-cc4b8b817203   
                            // we'll just take the last block - should be random enough
                            /*1*/ Guid.NewGuid().ToString().Substring(24));

                        var description = epCategory.Name;

                        ExtendedProperty.AddBlobResource(dc, typeof(TenantGroup), epScope.ID.Value, epCategory, epScope, ResourceType.PublicBlob, s3ObjectName, description, contentLength, contentType, stream);
                        return HubResult.Success;

                    case ExtendedPropertyScopeType.UserID:
                    default:
                        Debug.Fail("Unexpected ScopeType: " + epScope.ScopeType);
                        return HubResult.Error;
                }
            });
        }







        public static HubResult AddPrivateBlobResource(AppDC dc, EPCategory epCategory, HttpPostedFile httpPostedFile)
        {
            return AddPrivateBlobResource(dc, epCategory, (ulong)httpPostedFile.ContentLength, httpPostedFile.ContentType, httpPostedFile.InputStream);
        }

        public static HubResult AddPrivateBlobResource(AppDC dc, EPCategory epCategory, ulong contentLength, string contentType, System.IO.Stream stream)
        {
            var notificationTenantGroupID = dc.TransactionAuthorizedBy.TenantID.Value;

            return writeLock(dc, notificationTenantGroupID, () =>
            {
                var authorizedBy = dc.TransactionAuthorizedBy;
                Debug.Assert(authorizedBy != null);

                var epScope = authorizedBy.TeamEPScopeOrThrow;
                Debug.Assert(epScope.ScopeType == ExtendedPropertyScopeType.TenantGroupID);

                switch (epScope.ScopeType)
                {
                    case ExtendedPropertyScopeType.TenantGroupID:


                        //!! what's a good namespace strategy here?
                        //!! pass in a template - or take it from the EPCategory
                        // {0} tenantID
                        // {1} targetID
                        // {2} new GUID
                        // {3} resourceID

                        // public objects uploaded by tenants shouldn't be guessable
                        // private objects should be able to be authorized via naming structure without a DB lookup

                        string s3ObjectName = string.Format("tenants/{0}/resources/{1}",
                            /*0*/ epScope.ID.Value,
                            // f2165312-5e7e-414a-97ba-cc4b8b817203   
                            // we'll just take the last block - should be random enough
                            /*1*/ Guid.NewGuid().ToString().Substring(24));

                        var description = epCategory.Name;

                        ExtendedProperty.AddBlobResource(dc, typeof(TenantGroup), epScope.ID.Value, epCategory, epScope, ResourceType.PrivateBlob, s3ObjectName, description, contentLength, contentType, stream);
                        return HubResult.Success;

                    case ExtendedPropertyScopeType.UserID:
                    default:
                        Debug.Fail("Unexpected ScopeType: " + epScope.ScopeType);
                        return HubResult.Error;
                }
            });
        }




        public static HubResult RemoveResource(AppDC dc, int id)
        {
            var notificationTenantGroupID = dc.TransactionAuthorizedBy.TenantID.Value;

            return writeLock(dc, notificationTenantGroupID, () =>
            {
                var authorizedBy = dc.TransactionAuthorizedBy;
                Debug.Assert(authorizedBy != null);

                var epScope = authorizedBy.TeamEPScopeOrInvalid;

                switch (epScope.ScopeType)
                {
                    case ExtendedPropertyScopeType.TenantGroupID:

                        ExtendedProperty.DeleteResource<TenantGroup>(dc, id);

                        dc.SubmitChanges();
                        return HubResult.Success;

                    case ExtendedPropertyScopeType.UserID:
                    default:
                        Debug.Fail("Unexpected ScopeType: " + epScope.ScopeType);
                        return HubResult.Error;
                }
            });
        }






        private static HubResult readLock(Identity authorizedBy, Func<HubResult> work)
        {
            var result = syncLock.Lock(authorizedBy, () =>
            {
                try
                {
                    var workResult = work();
                    return workResult;
                }
                catch (Exception ex)
                {
                    TorqContext.Current.EventLog.LogException(ex);
                    return HubResult.Error;
                }
            });
            return result;
        }

        private static HubResult writeLock(AppDC dc, int notificationTenantGroupID, Func<HubResult> work)
        {
            var result = readLock(dc.TransactionAuthorizedBy, () =>
            {
                var workResult = work();

                if (workResult.StatusCode == HubResultCode.Success)
                {
                    dc.SubmitChanges();

                    // We use the TenantGroupInfoCache to generate our notification - so refresh it first
                    TenantGroup.RefreshTenantGroupInfoCache();

                    //!! Hmm - Feels like for settings at least we should notify the entire tenantGroup hierarchy!
                    notifyClients(dc.TransactionAuthorizedBy, notificationTenantGroupID);
                }
                return workResult;
            });

            return result;
        }

        private static void notifyClients(Identity authorizedBy, int tenantGroupID)
        {
            var utilityContext = UtilityContext.Current;
            var authorizedByTeamEPScope = authorizedBy.TeamEPScopeOrInvalid;

            // Generate a master notification - authorized by our caller - so it's all the items they are authorized to see. Others might need to see a filtered subset.
            var notification = getSettings(tenantGroupID);

            PresenceManager.IdentityFilter identityFilter = (authorizedByNotUsed, connection) =>
            {
                var connectionIdentity = connection.Identity;
                Debug.Assert(connectionIdentity != null);
                var connectionIdentityTeamEPScope = connectionIdentity.TeamEPScopeOrInvalid;

                bool allowedRoles = connectionIdentity.IsInRole(SystemRole.SystemAdmin) || connectionIdentity.IsInRole(SystemRole.TenantAdmin);
//!! fix me. This allowed stuff is complicated!!!!!
                bool allowedTenant = authorizedByTeamEPScope == connectionIdentityTeamEPScope;

                if (allowedRoles && allowedTenant)
                {
                    return PresenceManager.IdentityFilterResponse.All; 
                }

                return PresenceManager.IdentityFilterResponse.None;
            };


            var hubClients = utilityContext.ConnectionManager.GetHubContext("siteHub").Clients;
            Debug.Assert(hubClients != null);

            // !! Used to indicate if a connectionID should be excluded from the notification. Do we need that here?
            SearchExpression notifyExpression = null;

            utilityContext.PresenceManager.NotifyClients(authorizedBy, notifyExpression, identityFilter,
                connectionIDs => hubClients.Clients(connectionIDs).updateSettings(notification)
            );
        }

        private static object getSettings(int tenantGroupID)
        {
            TenantGroupInfo tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantGroupID);

            var resourceCategoriesMap = tenantGroupInfo.AggregateEPResourceCategoryGroups
                .Select(resourceGroup => new
                {
                    Category = resourceGroup.Key,
                    Value = resourceGroup.Value
                        .OrderByDescending(epResource => epResource.Timestamp)
                        .Select(epResource => epResource.Resource.ToJsonObject(epResource.ExtendedPropertyID))
                        // exclude resources that can't be converted
                        .Where(resourceJsonObject => resourceJsonObject != null)
                        .ToArray(),
                })
                // exclude any categories where we don't have any visible values
                .Where(keyPair => keyPair.Value.Any())
                .ToDictionary(keyPair => keyPair.Category.ID, keyPair => keyPair.Value);

            var settings = new
            {
                resources = new
                {
                    categories = resourceCategoriesMap
                }
            };
            return settings;
        }
    }
}