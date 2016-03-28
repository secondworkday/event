using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

using App.Library;
//using System.Configuration;

namespace WebApp
{
    [HubName("siteHub")]
    public class SiteHub : BaseHub
    {
        /// <summary>
        /// Authenticates based on Context identity, and uses provides routines to handle authenticated and anonymous cases
        /// </summary>
        protected T standardHeader<T>(Func<SiteContext, AppDC, T> authenticatedHandler, Func<T> anonymousHandler)
        {
            return IdentityHeader(BaseHub.AccountsOnlyDataContextFactory<AppDC>, dc => authenticatedHandler(SiteContext.Current, dc), anonymousHandler);
        }

        protected HubResult authTokenHeader(AuthCode authCode, Func<SiteContext, AppDC, AuthToken, HubResult> authTokenHandler)
        {
            return AuthTokenHeader<AppDC>(authCode, (dc, authToken) => authTokenHandler(SiteContext.Current, dc, authToken));
        }

        /// <summary>
        /// Authenticates based on Context identity, only processes authenticated case. Anonymous users receive HubResult.Unauthorized
        /// </summary>
        protected HubResult accountsOnlyHeader(Func<SiteContext, AppDC, HubResult> accountsOnlyHandler)
        {
            return IdentityHeader(BaseHub.AccountsOnlyDataContextFactory<AppDC>, dc => accountsOnlyHandler(SiteContext.Current, dc));
        }

        protected HubResult accountsOnlyHeader(Func<SiteContext, AppDC, HubResult> accountsOnlyHandler, string callingMethod)
        {
            MsEventSourceActivities.MsEventSource.Log.RequestStart(callingMethod);
            var header = IdentityHeader(BaseHub.AccountsOnlyDataContextFactory<AppDC>, dc => accountsOnlyHandler(SiteContext.Current, dc));
            MsEventSourceActivities.MsEventSource.Log.RequestStop(true);
            return header;
        }

#if false
        /// <summary>
        /// Authenticates as a TenantAdmin for our Demo Tenant - for creating tenant stuff
        /// </summary>
        internal HubResult demoTenantAdminIdentityHeader<DC>(Func<DC, HubResult> systemHandler)
            where DC : DataContextBase
        {
            var utilityContext = UtilityContext.Current;

            //!! Note - this currently is providing full SYSTEM access - need to limit down to just DemoTenant to improve security
            using (var dc = utilityContext.CreateDemoTenantAdminDefaultAccountsOnlyDC<DC>())
            {
                if (!utilityContext.IsProductionSite)
                {
                    //!! can we dynamically find the name of the hub? perhaps object.GetType()?

                    StackFrame callerStackFrame = new StackFrame(1);
                    string callerMethodName = "Hub: " + callerStackFrame.GetMethod().Name;

                    dc.TrackDatabaseCalls(performanceItem =>
                    {
                        RequestItem requestItem = RequestItem.Create(callerMethodName, dc.TransactionAuthorizedBy, performanceItem);
                        utilityContext.RequestLog.Add(requestItem);
                    });
                }
                try
                {
                    var result = systemHandler(dc);
                    return result;
                }
                catch (HubResultException hubResultException)
                {
                    utilityContext.EventLog.LogException(hubResultException);
                    return hubResultException.HubResult;
                }
                catch (Exception ex)
                {
                    utilityContext.EventLog.LogException(ex);
                    return HubResult.Error;
                }
            }
        }
#endif


        public override Task OnConnected()
        {
            // ** Good news. We've just had a client connect! In a SPA application, this is our opportunity to inform that client of their Identity.

            // call the base first - to setup our tenant notification group
            return standardHeader((siteContext, dc) =>
            {
                var authorizedBy = dc.TransactionAuthorizedBy;
                if (authorizedBy != null)
                {
                    // (haven't tested non-Tenant identities yet - so asserting here so we can validate that code path)
                    Debug.Assert(authorizedBy.TenantID.HasValue);
                    Debug.Assert(authorizedBy.TenantGroupID.HasValue);

                    var appDC = dc as AppDC;
                    Debug.Assert(appDC != null);

                    // Generate a notification which lets the client know their authenticated identity.
                    if (authorizedBy.AuthorizedTableHashCode == EventSession.TableHashCode)
                    {
#if false
                        var authenticatedSearchExpression = SearchExpression.Create(authorizedBy.AuthorizedIDs.FirstOrDefault());
                        var authenticatedNotification = EventSession.Search(appDC, authenticatedSearchExpression, string.Empty, 0, int.MaxValue);


                        //       model.authenticatedIdentity = msIdentity.create('eventSession', authenticatedItem.id, authenticatedItem.name, appRoles, systemRoles, authenticatedItem.profilePhotoUrl);

                        Debug.Assert(authorizedBy.UserIDOrNull != null);

                        var eventSessionNotification = new
                        {
                            type = "eventSessionUser",
                            userID = authorizedBy.UserIDOrNull,
                            eventSessionID = authorizedBy.AuthorizedIDs.FirstOrDefault(),
                        };
#endif

                        var authenticatedNotification = authorizedBy.GetClientNotification(dc, "eventSession");

                        Clients.Caller.setAuthenticatedItemUserSession(authenticatedNotification);
                    }

                    // Send user stats
                    //var result = Case.GetMyCasesReadStats(appDC, authorizedBy.GetAuthenticatedUser(appDC).ID);
                    //Clients.Caller.updateUserStats(result);
                }

                return base.OnConnected();
            },
            () => base.OnConnected());
        }



        public HubResult CreateParticipantGroup(dynamic data)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var newParticipantGroup = ParticipantGroup.Create(dc, data);
                return HubResult.CreateSuccessData(newParticipantGroup.ID);
            });
        }

        public HubResult EditParticipantGroup(int itemID, dynamic editParticipantGroupData)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return ParticipantGroup.Edit(dc, itemID, editParticipantGroupData);
            });
        }

        public HubResult DeleteParticipantGroup(int itemID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return ParticipantGroup.Delete(dc, itemID);
            });
        }

        public HubResult SearchParticipantGroups(string searchExpressionString)
        {
            return SearchParticipantGroups(searchExpressionString, string.Empty, 0, int.MaxValue);
        }

        public HubResult SearchParticipantGroups(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = ParticipantGroup.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }


        public HubResult ParseParticipantGroups(int eventID, string parseData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = ParticipantGroup.Parse(dc, eventID, parseData);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult UploadParticipantGroups(int eventID, dynamic data)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = ParticipantGroup.Upload(dc, eventID, data);
                return HubResult.CreateSuccessData(result);
            });
        }










        public HubResult CreateParticipant(dynamic data)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var newParticipant = Participant.Create(dc, data);
                return HubResult.CreateSuccessData(newParticipant.ID);
            });
        }

        public HubResult SearchParticipants(string searchExpressionString)
        {
            return SearchParticipants(searchExpressionString, string.Empty, 0, int.MaxValue);
        }

        public HubResult SearchParticipants(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = Participant.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }




#if false
        /// <summary>
        /// Variant that requires an AuthCode to authorize access to a System DC
        /// </summary>
        protected T systemHeader<T, AC>(AuthCode authCode, Func<TorqContext, AppDC, AuthToken, T> handler)
            where AC : AuthTemplate
        {
            return base.systemHeader<T, AC>(authCode, (utilityContext, dc, authToken) =>
            {
                return handler(utilityContext as TorqContext, dc as AppDC, authToken);
            });
        }


        private async Task<HubResult> standardHeaderAsync(Func<TorqContext, AppDC, Task<HubResult>> standardHandler)
        {
            //await Task.Run<HubResult>(standardHeader(standardHandler));

            Identity authorizedBy = Context.User.Identity as Identity;
            var siteContext = TorqContext.Current;

            using (var dc = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                var result = await standardHandler(siteContext, dc);
                return result;
            }
        }
#endif



        public HubResult CreateTenant(string name, dynamic data)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return ExtendedTenantGroup.CreateTenant(dc, name, data);
            });
        }

        public HubResult CreateGroup(string name, int parentTenantGroupID, dynamic data)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return ExtendedTenantGroup.CreateGroup(dc, name, parentTenantGroupID, data);
            });
        }






        public HubResult GetSettings(int tenantGroupID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var settings = Settings.GetSettings(dc.TransactionAuthorizedBy, tenantGroupID);
                return settings;
            });
        }


        public HubResult AddTeamUrlResource(int epCategoryID, string name, string urlString)
        {
            EPCategoryCode ePCategoryCode = EPCategoryCode.FromID(epCategoryID);

            var epCategory = EPCategory.Find(ePCategoryCode);
            Debug.Assert(epCategory != null);

            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Settings.AddTeamUrlResource(dc, epCategory, name, urlString);
            });
        }

        public HubResult RemoveResource(int id)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Settings.RemoveResource(dc, id);
            });
        }







        public HubResult SendUserSignupInvitation(string email, dynamic data)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return ExtendedUser.SendSignupInvitation(dc, email, data);
            });
        }


        public HubResult RedeemUserSignupInvitation(AuthCode authCode, string password, dynamic options)
        {
            return authTokenHeader(authCode, (torqContext, dc, authToken) =>
            {
                string errorMessage;
                User user = ExtendedUser.RedeemSignupInvitation(dc, authCode, password, options, out errorMessage);
                if (user != null)
                {
                    return HubResult.CreateSuccessData(user.ID);
                }
                return HubResult.CreateError(errorMessage);
            });
        }


        public HubResult GetEventSessionVolunteerAuthInfo(int itemID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventSession.GetVolunteerAuthInfo(dc, itemID);
            });
        }





#if false
        public HubResult SendSessionVolunteerInvitationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return HubResult.CreateError("No email address provided.");
            }

            var mailAddress = email.ParseMailAddress();

            if (mailAddress == null)
            {
                return HubResult.CreateError("Invalid email address provided.");
            }

            return systemIdentityHeader<WebUtilityDC, HubResult>(dc =>
            {
                return ResetPasswordAuthTemplate.SendResetPasswordEmail(dc, mailAddress);
            });
        }
#endif








        public HubResult GenerateRandomEvent()
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var randomEvent = Event.GenerateRandom(dc);
                Debug.Assert(randomEvent != null);

                return HubResult.CreateSuccessData(randomEvent.ID);
            });
        }

        public HubResult CreateEvent(dynamic formData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var randomEvent = Event.Create(dc, formData);
                return HubResult.CreateSuccessData(randomEvent.ID);
            });
        }

        public HubResult DeleteEvent(int itemID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return Event.Delete(dc, itemID);
            });
        }

        public HubResult EditEvent(int itemID, dynamic editEventData)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return Event.Edit(dc, itemID, editEventData);
            });
        }

        public HubResult CreateEventSession(dynamic formData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var eventSession = EventSession.Create(dc, formData);
                return HubResult.CreateSuccessData(eventSession.ID);
            });
        }

        public HubResult DeleteEventSession(int itemID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventSession.Delete(dc, itemID);
            });
        }

        public HubResult EditEventSession(int itemID, dynamic editEventSessionData)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventSession.Edit(dc, itemID, editEventSessionData);
            });
        }


        public HubResult GenerateRandomParticipants(int participantGroupID, int numberOfParticipants)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                for (int i = 0; i < numberOfParticipants; i++)
                {
                    var randomParticipant = Participant.GenerateRandom(dc, participantGroupID.ToEnumerable().ToArray());
                }
                return HubResult.CreateSuccessData(participantGroupID);
            });
        }

        public HubResult GenerateRandomParticipant(int participantGroupID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var randomParticipant = Participant.GenerateRandom(dc, new[] { participantGroupID });
                return HubResult.CreateSuccessData(randomParticipant.ID);
            });
        }


        public HubResult GetEvents()
        {
            return GetEvents(0, int.MaxValue);
        }

        public HubResult GetEvents(int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var sortField = "name";
                var result = Event.Search(dc, SearchExpression.Empty, sortField, startRowIndex, maximumRows);

                return HubResult.CreateSuccessData(result);
            });
        }



        public HubResult SearchEvents(string searchExpressionString)
        {
            return SearchEvents(searchExpressionString, string.Empty, 0, int.MaxValue);
        }

        public HubResult SearchEvents(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = Event.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult SearchEventSessions(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = EventSession.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }




        public HubResult SetEventSessionCheckInOpen(int itemID, bool isOpen)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventSession.SetCheckInOpen(dc, itemID, isOpen);
                return HubResult.CreateSuccessData(result);
            });
        }

        /*
                public HubResult SetEventSessionState(int itemID, EventSessionState state)
                {
                    return accountsOnlyHeader((siteContext, dc) =>
                    {
                        var result = EventSession.SetState(dc, itemID, state);
                        return HubResult.CreateSuccessData(result);
                    });
                }
        */






        public HubResult SearchEventParticipants(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = EventParticipant.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult GetEventParticipantSet(string searchExpressionString)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = EventParticipant.GetSet(dc, searchExpression);
                return HubResult.CreateSuccessData(result);
            });
        }



        public HubResult ParseEventParticipants(int eventID, string parseData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.Parse(dc, eventID, parseData);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult UploadEventParticipants(int eventID, dynamic data)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.Upload(dc, eventID, data, Clients.Caller);
                return HubResult.CreateSuccessData(result);
            }, "/SiteHub/UploadEventParticipants");
        }


        public HubResult CreateEventParticipant(int eventID, dynamic data)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.CreateParticipantAndEventParticipant(dc, eventID, data);
                return HubResult.CreateSuccessData(result);
            });
        }





        public HubResult EditEventParticipant(int itemID, dynamic editEventParticipantData)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.Edit(dc, itemID, editEventParticipantData);
            });
        }

        public HubResult EditEventParticipants(int eventID, int[] itemIDs, int eventSessionID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.SetEventSession(dc, eventID, itemIDs, eventSessionID);
            }, "/SiteHub/EditEventParticipants");
        }



        public HubResult CheckInEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.CheckIn(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult CheckInEventParticipants(int eventID, int[] itemIDs)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.CheckIn(dc, eventID, itemIDs);
            }, "/SiteHub/CheckInEventParticipants");
        }

        public HubResult UndoCheckInEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.UndoCheckIn(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult UndoCheckInEventParticipants(int eventID, int[] itemIDs)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.UndoCheckIn(dc, eventID, itemIDs);
            }, "/SiteHub/UndoCheckInEventParticipants");
        }


        public HubResult CheckOutEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.CheckOut(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
        }


        public HubResult DeleteEventParticipant(int itemID)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.Delete(dc, itemID);
            });
        }

        public HubResult DeleteEventParticipants(int[] itemIDs)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.Delete(dc, itemIDs);
            }, "/SiteHub/DeleteEventParticipants");
        }
















        public HubResult ModifyEventTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Event.ModifyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyEventMyTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Event.ModifyMyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyMyFavoriteEvent(int itemID, bool isFavorite)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Event.ModifyMyFavorite(dc, itemID, isFavorite);
            });
        }

#if false
        public HubResult EmailProjectReport(int itemID, string emailType, dynamic mailMessageData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Project.EmailReport(dc, itemID, emailType, mailMessageData);
            });
        }
#endif


























        public HubResult CancelTask(int taskID)
        {
            var siteContext = SiteContext.Current;
            return siteContext.TaskManager.CancelTask(taskID);
        }



        public async Task<string> DoLongRunningThing(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 5)
            {
                await Task.Delay(200);
                progress.Report(i);
            }
            return "Job complete!";
        }
    }
}