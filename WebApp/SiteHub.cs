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
                        var authenticatedSearchExpression = SearchExpression.Create(authorizedBy.AuthorizedIDs.FirstOrDefault());
                        var authenticatedNotification = EventSession.Search(appDC, authenticatedSearchExpression, string.Empty, 0, int.MaxValue);

                        Clients.Caller.setAuthenticatedEventSession(authenticatedNotification);
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






#if false
        public HubResult CreateJobSeeker(dynamic data)
        {
            var demoTenantID = TenantGroup.GetCachedTenantGroupInfo()
                .Where(tenantGroupInfo => tenantGroupInfo.IsDemo)
                .Select(tenantGroupInfo => (int?)tenantGroupInfo.TenantGroupID)
                .FirstOrDefault();

            if (!demoTenantID.HasValue)
            {
                return HubResult.CreateError("No designated Demo tenant");
            }

            return demoTenantAdminIdentityHeader<AppDC>(appDC =>
            {
                var authorizedBy = appDC.TransactionAuthorizedBy;

                var email = (string)data.emailAddress;
                var mailAddress = email.ParseMailAddress();
                var password = (string)data.password;
                var firstName = (string)data.firstName;
                var lastName = (string)data.lastName;

                string errorMessage;
                var user = User.SelfCreate(appDC, authorizedBy, mailAddress, password, firstName, lastName, out errorMessage);
                if (user == null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(errorMessage));
                    return HubResult.CreateError(errorMessage);
                }

                var profilePhotoUrlString = (string)data.profilePhotoUrl;
                if (!string.IsNullOrEmpty(profilePhotoUrlString))
                {
                    var profilePhotoUrl = new Uri(profilePhotoUrlString);
                    User.SetProfilePhoto(appDC, user.ID, profilePhotoUrl);
                }

                var careerProfile = CareerProfile.Create(appDC, user, demoTenantID, out errorMessage);
                if (careerProfile == null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(errorMessage));
                    return HubResult.CreateError(errorMessage);
                }

                // Users and CareerProfiles share a common ID, so we can use the careerProfile.ID here
                var userMailAddress = User.GetPrimaryMailAddress(appDC, user.ID);

                return HubResult.CreateSuccessData(new { id = user.ID, email = userMailAddress.Address });
            });
        }

        public HubResult CreateDemoJobSeeker()
        {
            var demoTenantID = TenantGroup.GetCachedTenantGroupInfo()
                .Where(tenantGroupInfo => tenantGroupInfo.IsDemo)
                .Select(tenantGroupInfo => (int?)tenantGroupInfo.TenantGroupID)
                .FirstOrDefault();

            if (!demoTenantID.HasValue)
            {
                return HubResult.CreateError("No designated Demo tenant");
            }

            return demoTenantAdminIdentityHeader<AppDC>(appDC =>
            {
                string errorMessage;
                var demoCareerProfile = CareerProfile.CreateRandom(appDC, demoTenantID, out errorMessage);
                if (demoCareerProfile == null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(errorMessage));
                    return HubResult.CreateError(errorMessage);
                }

                // Users and CareerProfiles share a common ID, so we can use the careerProfile.ID here
                var userMailAddress = User.GetPrimaryMailAddress(appDC, demoCareerProfile.ID);

                return HubResult.CreateSuccessData(new { id = demoCareerProfile.ID, email = userMailAddress.Address });
            });
        }
#endif

#if false
        public HubResult CreateDemoUser(AppRole appRole)
        {
            var demoTenantID = TenantGroup.GetCachedTenantGroupInfo()
                .Where(tenantGroupInfo => tenantGroupInfo.IsDemo)
                .Select(tenantGroupInfo => (int?)tenantGroupInfo.TenantGroupID)
                .FirstOrDefault();

            if (!demoTenantID.HasValue)
            {
                return HubResult.CreateError("No designated Demo tenant");
            }

            return demoTenantAdminIdentityHeader<AppDC>(appDC =>
            {
                var appRoleString = appRole.ToString();
                string errorMessage;
                var demoUser = User.CreateRandom(appDC, demoTenantID, appRoleString, out errorMessage);

                if (demoUser == null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(errorMessage));
                    return HubResult.CreateError(errorMessage);
                }

                // Users and CareerProfiles share a common ID, so we can use the careerProfile.ID here
                var userMailAddress = User.GetPrimaryMailAddress(appDC, demoUser.ID);

                return HubResult.CreateSuccessData(new { id = demoUser.ID, email = userMailAddress.Address });
            });
        }
#endif


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
                for (int i=0; i < numberOfParticipants; i++) { 
                    var randomParticipant = Participant.GenerateRandom(dc, participantGroupID.ToEnumerable().ToArray());
                }
                return HubResult.CreateSuccessData(participantGroupID);
            });
        }

        public HubResult GenerateRandomParticipant(int participantGroupID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var randomParticipant = Participant.GenerateRandom(dc, new [] {participantGroupID});
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
            });
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
        });
    }

        public HubResult CheckInEventParticipants(int[] itemIDs)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.CheckIn(dc, itemIDs);
            });
        }

        public HubResult UndoCheckInEventParticipants(int[] itemIDs)
        {
            return accountsOnlyHeader((utilityContext, dc) =>
            {
                return EventParticipant.UndoCheckIn(dc, itemIDs);
            });
        }



        public HubResult CheckInEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.CheckIn(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult UndoCheckInEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.UndoCheckIn(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
        }


        public HubResult CheckOutEventParticipant(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var result = EventParticipant.CheckOut(dc, itemID);
                return HubResult.CreateSuccessData(result);
            });
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































#if false
        #region Career Profiles

        //!! a career profile ID is a user ID, right?

        public HubResult GetCareerProfile(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerProfile.Get(dc, itemID);
            });
        }


        public HubResult SearchCareerProfiles(string searchExpressionString)
        {
            return SearchCareerProfiles(searchExpressionString, string.Empty, 0, int.MaxValue);
        }

        public HubResult SearchCareerProfiles(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = CareerProfile.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult ModifyCareerProfileTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerProfile.ModifyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyCareerProfileMyTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerProfile.ModifyMyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyCareerProfileMyFavorite(int itemID, bool isFavorite)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerProfile.ModifyMyFavorite(dc, itemID, isFavorite);
            });
        }

        #endregion 

        #region Career Step

        public HubResult GetCareerStep(int userID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                //!! for now, grab the first project we have access to
                var fakeJobCounselorProject = Project.Query(dc)
                    .OfType<JobCounselorProject>()
                    .FirstOrDefault();


                //var projectID = 16095;
                //var jobCounselorProject = Project.Get(dc, projectID) as JobCounselorProject;

                // A careerStep is roughly equivalent to a Personal Employment Plan. It can live on its own, or it can be tied to a User (that is a job seeker)
                var careerStep = new
                {
                    id = fakeJobCounselorProject.ID,
                    userID = userID,

                    zipCodeInfo = new
                    {
                        zipCode = fakeJobCounselorProject.JobsZip,
                        name = "FakePittsburg, PA",
                    },

                    nextOccupationIDs = fakeJobCounselorProject.NextOccupations
                        .Select(nextOccupation => nextOccupation.OccupationCodeTo)
                        .ToArray(),
                };


                return HubResult.CreateSuccessData(careerStep);
            });
        }

        public HubResult SearchCareerSteps(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = CareerStep.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult SetCareerStepZipCode(int careerStepID, string zipCodeString)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.SetZipCode(dc, careerStepID, zipCodeString);
            });
        }

        public HubResult ModifyCareerStepSettings(int careerStepID, dynamic settings)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.ModifySettings(dc, careerStepID, settings);
            });
        }






        public HubResult AddCareerStepCareerHistoryWork(int careerStepID, WorkPeriod workPeriod)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.AddCareerHistoryWork(dc, careerStepID, workPeriod);
            });
        }

        public HubResult RemoveCareerStepCareerHistoryWork(int careerStepID, int workPeriodIndex)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.RemoveCareerHistoryWork(dc, careerStepID, workPeriodIndex);
            });
        }

        public HubResult ModifyCareerStepCareerHistoryWork(int careerStepID, int workPeriodIndex, WorkPeriod workPeriod)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.ModifyCareerHistoryWork(dc, careerStepID, workPeriodIndex, workPeriod);
            });
        }




        public HubResult AddCareerStepOccupation(int careerStepID, string occupationCodeString)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.AddOccupation(dc, careerStepID, occupationCode);
            });
        }

        public HubResult RemoveCareerStepOccupation(int careerStepID, string occupationCodeString)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.RemoveOccupation(dc, careerStepID, occupationCode);
            });
        }

        public HubResult SkipCareerStepOccupation(int careerStepID, string occupationCodeString)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.SkipOccupation(dc, careerStepID, occupationCode);
            });
        }



/*
 * 
        Startup Phase
        Beginning fetches for job boards {simmplyHired, Indeed, us.jobs}
        Progress - simply hired, 220 jobs available, 100 jobs fetched)
 *      Progress - simply hired, 34 jobs already excluded
 *      Progress - simply hired, autocoding 46 remaining jobs
 *      Progress - simply hired, autocoded 46 job postings
 *      Progress - simply hired, completed
 *      
 *      Progress  overall completed
 * 
 * * 
 * 
 * 
 * 
 * 
 * 
 * * 
 * 
 * 
 */


        // init
        // running
        // completed
        // cancelled

        // progress map?  task map?

        public async Task<HubResult> GetJobOpenings(string occupationCodeString, string zipCodeString, int searchRadius, string[] jobBoardCodes, IProgress<TaskContext> iProgress)
        {
            Debug.Assert(!string.IsNullOrEmpty(occupationCodeString));
            Debug.Assert(!string.IsNullOrEmpty(zipCodeString));

            if (string.IsNullOrEmpty(occupationCodeString))
            {
                return HubResult.NotFound;
            }

            var taskProgress = new JobOpeningTaskProgress();
            var taskContext = TaskContext.Create(iProgress, taskProgress);

            string taskName = "Cool Task";

            var jobBoards = jobBoardCodes.ParseAsEnumArray<BaseExternalProvider.Providers>();

            var timeZoneInfo = TimeZones.Eastern;
            Identity authorizedBy = Context.User.Identity as Identity;
            if (authorizedBy != null)
            {
                timeZoneInfo = authorizedBy.TimeZoneInfo;
            }
            var siteContext = TorqContext.Current;

            using (var appDC = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v"))
                {
                    var occupationCode = OccupationCode.Create(occupationCodeString);
                    var onetOccupation = OnetOccupation.GetByOnetCode(referenceDC, occupationCodeString);

                    var lmaNotReallyNeeded = LaborMarketArea.ParseZipCode(referenceDC, zipCodeString);
                    Debug.Assert(lmaNotReallyNeeded != null);

                    var emptySkipList = new Uri[0];


                    var jobOpeningsTask = JobOpening.GetJobOpeningsAsync2(onetOccupation, lmaNotReallyNeeded, zipCodeString, searchRadius, jobBoards, emptySkipList, taskProgress);

                    //return jobOpeningsTask;
                    //var jobOpeningsTask = CareerStep.GetJobOpeningsAsync2(appDC, careerStepID, onetOccupation, lma, zipCodeString, searchRadius, taskProgress);

                    var jobOpenings = await jobOpeningsTask;

                    return HubResult.CreateSuccessData(jobOpenings);
                }
            }


#if false
            return deliverPackage(cancellationTokenSource.Token, hubProgressProxy).ContinueWith(task =>
            {
                return TaskItem.Create(taskID, taskName, TaskStatus.Canceled).ToHubResult();
            }, TaskContinuationOptions.NotOnRanToCompletion);

            // return HubResult.CreateError("Oh snap. An error occured.");
#endif



#if false
            var siteContext = TorqContext.Current;

            var companyCache = siteContext.CompanyCache;

            var timeZoneInfo = TimeZones.Eastern;
            var authorizedBy = Context.User.Identity as Identity;
            if (authorizedBy != null)
            {
                timeZoneInfo = authorizedBy.TimeZoneInfo;
            }

            using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v"))
            {
                var occupationCode = OccupationCode.FromOnetCode(occupationCodeString);

                var occupation = referenceDC.OnetOccupations
                    .Where(onetOccupation => onetOccupation.OnetCode == occupationCode.OnetCode)
                    .FirstOrDefault();

                var jobBoardName = JobPostingProvider.GetName(JobPostingProvider.Providers.SimplyHired);

                var jobPostings = await TorqContext.Current.GetSimplyHiredJobsAsync(occupation, zipCodeString, searchRadius);

                // Augment the response with xxxDisplay dates
                var results = jobPostings
                    .Select(jobPosting => new
                    {
                        JobPosting = jobPosting,
                        CompanyInfo = siteContext.CompanyCache.Get(jobPosting.Company, jobBoardName),
                    })
                    //.Select(jobPostingInfo => jobPostingInfo.JobPosting.ToJsonObject(timeZoneInfo, jobBoardName, jobPostingInfo.CompanyInfo.LogoUrl));
                    .Select(jobPostingInfo => ClientJobPosting.Create(jobPostingInfo.JobPosting, companyCache, jobBoardName));

                return HubResult.CreateSuccessData(results);
            }
#endif
        }







        public async Task<HubResult> GetCareerStepJobOpenings2(int careerStepID, string occupationCodeString, string zipCodeString, int searchRadius, IProgress<TaskContext> iProgress)
        {
            // outcomes? Success - new job, Failure, Exception  - default - choose success?
            // processingTime

            var taskProgress = new JobOpeningTaskProgress();
            var taskContext = TaskContext.Create(iProgress, taskProgress);

            //var progressProxy = JobOpeningProgress.Create(progress);


            //taskContext.Report(HubProgress.Create("Starting..."));

            string taskName = "Cool Task";


            Debug.Assert(careerStepID != 0);
            Debug.Assert(!string.IsNullOrEmpty(occupationCodeString));

            if (careerStepID == 0 || string.IsNullOrEmpty(occupationCodeString))
            {
                return HubResult.NotFound;
            }



            var timeZoneInfo = TimeZones.Eastern;
            Identity authorizedBy = Context.User.Identity as Identity;
            if (authorizedBy != null)
            {
                timeZoneInfo = authorizedBy.TimeZoneInfo;
            }
            var siteContext = TorqContext.Current;

            using (var appDC = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v"))
                {
                    var occupationCode = OccupationCode.Create(occupationCodeString);
                    var onetOccupation = OnetOccupation.GetByOnetCode(referenceDC, occupationCodeString);

                    var lma = LaborMarketArea.ParseZipCode(referenceDC, zipCodeString);
                    Debug.Assert(lma != null);

                    var jobOpeningsTask = CareerStep.GetJobOpeningsAsync2(appDC, careerStepID, onetOccupation, lma, zipCodeString, searchRadius, taskProgress);
                    var jobOpenings = await jobOpeningsTask;

                    return HubResult.CreateSuccessData(jobOpenings);
                }
            }


#if false
            return deliverPackage(cancellationTokenSource.Token, hubProgressProxy).ContinueWith(task =>
            {
                return TaskItem.Create(taskID, taskName, TaskStatus.Canceled).ToHubResult();
            }, TaskContinuationOptions.NotOnRanToCompletion);

            // return HubResult.CreateError("Oh snap. An error occured.");
#endif

        }




        #region JobOpportunities (items in a CareerStep) and JobPostings (potential JobOpporunities we show to Job Seekers)
        public HubResult AddCareerStepJobOpportunity(int careerStepID, string occupationCodeString, JobPosting jobPosting)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.AddJobPosting(dc, careerStepID, occupationCode, jobPosting);
            });
        }

        // (not passing occupationCodeString - as client might be dealing with a flat list of JobPostings)
        public HubResult RemoveCareerStepJobOpportunity(int careerStepID, JobPosting jobPosting)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.RemoveJobPosting(dc, careerStepID, jobPosting);
            });
        }

        public HubResult SkipCareerStepJobPosting(int careerStepID, JobPosting jobPosting)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.SkipJobPosting(dc, careerStepID, jobPosting);
            });
        }
        #endregion





        #region Education related


        public HubResult GetCareerStepEducationOptions(int careerStepID, string occupationCodeString, string zipCodeString, int searchRadius)
        {
            string taskName = "Cool Task";


            Debug.Assert(careerStepID != 0);
            Debug.Assert(!string.IsNullOrEmpty(occupationCodeString));

            if (careerStepID == 0 || string.IsNullOrEmpty(occupationCodeString))
            {
                return HubResult.NotFound;
            }

            var timeZoneInfo = TimeZones.Eastern;
            Identity authorizedBy = Context.User.Identity as Identity;
            if (authorizedBy != null)
            {
                timeZoneInfo = authorizedBy.TimeZoneInfo;
            }
            var siteContext = TorqContext.Current;

            using (var appDC = siteContext.CreateDefaultAccountsOnlyDC<AppDC>(DateTime.UtcNow, authorizedBy))
            {
                var occupationCode = OccupationCode.Create(occupationCodeString);

                var hubResult = CareerStep.GetEducationOptions(appDC, careerStepID, occupationCode, zipCodeString, searchRadius);
                return hubResult;
            }
        }



        public HubResult AddCareerStepEducationOpportunity(int careerStepID, string occupationCodeString, StateWioaEducationProgramItem educationProgram)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.AddEducationProgram(dc, careerStepID, occupationCode, educationProgram);
            });
        }

        // (not passing occupationCodeString - as client might be dealing with a flat list of JobPostings)
        public HubResult RemoveCareerStepEducationOpportunity(int careerStepID, StateWioaEducationProgramItem educationProgram)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.RemoveEducationProgram(dc, careerStepID, educationProgram);
            });
        }

        public HubResult SkipCareerStepEducationOpportunity(int careerStepID, StateWioaEducationProgramItem educationProgram)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return CareerStep.SkipEducationProgram(dc, careerStepID, educationProgram);
            });
        }
        #endregion










#if false
        public HubResult SkipCareerStepJobPosting(int careerStepID, CareerStep.JobPosting careerStepJobPosting)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                OccupationCode occupationCode = OccupationCode.Create(occupationCodeString);
                return CareerStep.SkipOccupation(dc, careerStepID, occupationCode);
            });
        }
#endif










        public HubResult SearchCareerStepTransitions(int careerStepID, string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = CareerStep.SearchTransitions(dc, careerStepID, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }





        #endregion
#endif



#if false
        #region Transitions

        public HubResult SearchTransitions(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {

            return standardReferenceOnlyHeader("torq_reference_v", (siteContext, referenceDC) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString, SearchExpression.Detail.High);

                Torq.Library.Touch.HigherWageTouchStrategy.GetOccupations(referenceDC, 


                GetTORQSco



                var result = OnetOccupation.Search(referenceDC, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });




            var asdf = SearchOccupations(searchExpressionString, sortField, startRowIndex, maximumRows);
            return await Task.FromResult(asdf);
        }

        #endregion
#endif





#if false
        public HubResult ModifyGroupAutoDownloadShow(int showID, bool isAutoDownload)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Show.ModifyGroupAutoDownload(dc, showID, isAutoDownload);
            });
        }
#endif

#if false
        public HubResult ModifyPerUserFavoriteDirectory(int directoryID, bool isFavorite)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Directory.ModifyPerUserFavorite(dc, directoryID, isFavorite);
            });
        }
#endif

#if false
        public HubResult ModifyPerUserWatchedEpisode(int episodeID, bool isWatched)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Episode.ModifyPerUserWatchedEpisode(dc, episodeID, isWatched);
            });
        }
#endif


#if false
        public HubResult NormalizeFileName(int videoID)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Video.NormalizeFileName(dc, videoID);
            });
        }
#endif


#if false
        public HubResult RenameDirectory(int directoryID, string showName)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Directory.RenameDirectory(dc, directoryID, showName);
            });
        }



        public HubResult AddDirectory(string directoryName, dynamic data)
        {
            return standardHeader((siteContext, dc) =>
            {
                directoryName = directoryName ?? "My Directory";

                Directory directory = Directory.Create(dc, directoryName, data);
                Debug.Assert(directory != null);

                if (directory == null)
                {
                    return HubResult.CreateError("Unable to add directory");
                }

                return HubResult.CreateSuccessData(directory.ID);
            });
        }

        public HubResult GetDirectories()
        {
            return GetDirectories(0, uint.MaxValue);
        }

        public HubResult GetDirectories(uint startRowIndex, uint maximumRows)
        {
            return standardHeader((siteContext, dc) =>
            {
                var sortField = "Name";
                var result = Directory.Search(dc, SearchExpression.Empty, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult GetFavoriteDirectories(uint startRowIndex, uint maximumRows)
        {
            return standardHeader((siteContext, dc) =>
            {
                var sortField = "Name";
                var searchExpression = SearchExpression.Create("#Favorite", SearchExpression.Detail.High);
                var result = Directory.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult SearchDirectories(string searchExpressionString, uint startRowIndex, uint maximumRows)
        {
            return standardHeader((siteContext, dc) =>
            {
                var sortField = "Name";
                var searchExpression = SearchExpression.Create(searchExpressionString, SearchExpression.Detail.High);
                var result = Directory.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }
#endif




#if false
        // Kicks of a long running Task and immediately returns. Caller can use VespaTrackJob if interested in progress/outcome.
        // JobDataHubResult
        public HubResult StartAutocoderThroughputTestTask(object parameters)
        {
            var siteContext = SiteContext.Current;

            var hubResult = siteContext.TaskManager.StartNewTask(async (cancelationToken, progress) => await AutoCoderThroughputTest.Run(parameters, cancelationToken, progress));
            return hubResult;
        }
#endif

#if false
        public async Task<HubResult> VespaTrackTask(int taskID, IProgress<HubProgress> iProgress)
        {
            TaskInfo taskInfo;
            if (taskInfoMap.TryGetValue(taskID, out taskInfo))
            {
                // wire up our proxy - so that when then task reports progress we pass it on to our SignalR client
                taskInfo.HubProgressProxy.SetIProgress(iProgress);

                try
                {
                    var hubResult = await taskInfo.Task;
                    return hubResult;
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    return TaskItem.Create(taskID, string.Empty, TaskStatus.Canceled).ToHubResult();
                }
            }
            return TaskItem.Create(taskID, string.Empty, TaskStatus.Undefined).ToHubResult();
        }
#endif

        public HubResult CancelTask(int taskID)
        {
            var siteContext = SiteContext.Current;
            return siteContext.TaskManager.CancelTask(taskID);
        }







#if false
        public HubResult GetProjects()
        {
            return GetProjects(0, int.MaxValue);
        }

        public HubResult GetProjects(int startRowIndex, int maximumRows)
        {

            using (var referenceDC = TorqContext.Current.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v6"))
            {
                var zipCodeLma = LaborMarketArea.ParseZipCode(referenceDC, "98077");
                if (zipCodeLma != null)
                {
                }
            }



            return accountsOnlyHeader((siteContext, dc) =>
            {
                var sortField = "name";
                var result = Project.Search(dc, SearchExpression.Empty, sortField, startRowIndex, maximumRows);

#if false

                var result = Project.Query(dc, SearchExpression.Empty, sortField, startRowIndex, maximumRows)
                    .Select(project => new 
                    {
                        id = project.ProjectID,
                        name = project.Name,
                    })
                    .ToArray();
#endif
                return HubResult.CreateSuccessData(result);
            });
        }



        public HubResult SearchProjects(string searchExpressionString)
        {
            return SearchProjects(searchExpressionString, string.Empty, 0, int.MaxValue);
        }

        public HubResult SearchProjects(string searchExpressionString, string sortField, int startRowIndex, int maximumRows)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                var searchExpression = SearchExpression.Create(searchExpressionString);
                var result = Project.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult ModifyProjectTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Project.ModifyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyProjectMyTag(int itemID, string tagName, bool isAssigned)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Project.ModifyMyTag(dc, itemID, tagName, isAssigned);
            });
        }

        public HubResult ModifyMyFavoriteProject(int itemID, bool isFavorite)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Project.ModifyMyFavorite(dc, itemID, isFavorite);
            });
        }



        public HubResult MigrateProject(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return JobCounselorProject.Migrate(dc, itemID);
            });
        }

        public HubResult MigrateDemoProject(int itemID)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return JobCounselorProject.MigrateDemo(dc, itemID);
            });
        }



        public HubResult EmailProjectReport(int itemID, string emailType, dynamic mailMessageData)
        {
            return accountsOnlyHeader((siteContext, dc) =>
            {
                return Project.EmailReport(dc, itemID, emailType, mailMessageData);
            });
        }
#endif



        public async Task<string> DoLongRunningThing(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 5)
            {
                await Task.Delay(200);
                progress.Report(i);
            }
            return "Job complete!";
        }


#if false
        public async Task<HubResult> GetTouchJobs(string occupationCodeString, string zipCodeString, int searchRadius, IProgress<string> progress)
        {
            var siteContext = TorqContext.Current;

            var companyCache = siteContext.CompanyCache;

            var timeZoneInfo = TimeZones.Eastern;
            var authorizedBy = Context.User.Identity as Identity;
            if (authorizedBy != null)
            {
                timeZoneInfo = authorizedBy.TimeZoneInfo;
            }

            using (var referenceDC = siteContext.CreateRuntimeReferenceOnlyDC<CachedTorqReferenceDataContext>("torq_reference_v"))
            {
                var occupationCode = OccupationCode.FromOnetCode(occupationCodeString);

                var occupation = referenceDC.OnetOccupations
                    .Where(onetOccupation => onetOccupation.OnetCode == occupationCode.OnetCode)
                    .FirstOrDefault();

                var jobBoardName = JobPostingProvider.GetName(JobPostingProvider.Providers.SimplyHired);

                var jobPostings = await TorqContext.Current.GetSimplyHiredJobsAsync(occupation, zipCodeString, searchRadius);

                // Augment the response with xxxDisplay dates
                var results = jobPostings
                    .Select(jobPosting => new {
                        JobPosting = jobPosting,
                        CompanyInfo = siteContext.CompanyCache.Get(jobPosting.Company, jobBoardName),
                    })
                    //.Select(jobPostingInfo => jobPostingInfo.JobPosting.ToJsonObject(timeZoneInfo, jobBoardName, jobPostingInfo.CompanyInfo.LogoUrl));
                    .Select(jobPostingInfo => ClientJobPosting.Create(jobPostingInfo.JobPosting, companyCache, jobBoardName));

                return HubResult.CreateSuccessData(results);
            }
        }
#endif


#if false
        public HubResult SendJobPostingsEmail(string email, Newtonsoft.Json.Linq.JArray jobPostings)
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

            return sendJobPostingsEmail(mailAddress, jobPostings);
        }

        //!! where should this function live?
        private static HubResult sendJobPostingsEmail(MailAddress mailAddress, Newtonsoft.Json.Linq.JArray jobPostings)
        {
            var utilityContext = UtilityContext.Current;

            // Create a System DC - since (a) we need access to all User accounts and (b) this is probably an anonymous request
            using (var appDC = utilityContext.CreateSystemDefaultAccountsOnlyDC<AppDC>())
            {
                var epEmailAddressesQuery = User.QueryOrderedEPEmailAddresses(appDC)
                    .Where(epEmailAddress => epEmailAddress.EmailAddress.Address == mailAddress.Address);

                string subject = "TORQ Touch job postings";

                var htmlJobPostings = jobPostings
                    .Select(jobPosting => new
                    {
                        Url = jobPosting["Url"],
                        Title = jobPosting["Title"],
                        Company = jobPosting["Company"],
                        Location = jobPosting["Location"],
                    })
                    .Select(jobPosting => string.Format("<li>{0} <a href='{1}'><small>apply</small></a></li>",
                        /*0*/ jobPosting.Title,
                        /*1*/ jobPosting.Url))
                    .Join();

                string htmlBodyTemplate =
    @"<p>Herein lieth opportunity. Charge forth!</p>
<ul>
{0}
</ul>
<p>Cheers,<br/>
{1}</p>";

                var htmlBody = string.Format(htmlBodyTemplate,
                    /*0*/ htmlJobPostings,
                    /*1*/ utilityContext.AppInfo.EmailSignature);

                var jobPostingsBuilder = new MailMessageBuilder();
                jobPostingsBuilder.To.Add(mailAddress);
                jobPostingsBuilder.Subject = subject;
                ///passwordResetEmail.BodyEncoding = Encoding.UTF8;
                //passwordResetEmail.IsBodyHtml = true;
                jobPostingsBuilder.SetHtmlView(htmlBody);
                jobPostingsBuilder.Headers.Add("X-PostmarkTag", "JobPostings");


                string description = string.Format("User {0} requested a job posting email, {1} postings",
                    /*0*/ mailAddress.ToString(),
                    /*1*/ jobPostings.Count());

                utilityContext.Emailer.SystemSendEmail(jobPostingsBuilder, description, (sentMessage, utilityDC, outboundEmail) =>
                {
                    var epScope = EPScope.Global;

                    //!! change this to TO email address.
                    var activityItem = ActivityItem.Log(utilityDC, epScope, AppActivityType.JobPostingsEmailSent, description, typeof(OutboundEmail), outboundEmail.ID);
                    outboundEmail.ActivityItem = activityItem;
                });

                //log reset request
                utilityContext.EventLog.LogInformation(description);

                return HubResult.Success;
            }
        }
#endif


#if false

        public HubResult SearchContacts(string searchExpressionString, uint startRowIndex, uint maximumRows)
        {
            return standardHeader((siteContext, dc) =>
            {
                var sortField = "Name";
                var searchExpression = SearchExpression.Create(searchExpressionString, SearchExpression.Detail.High);
                var result = Contact.Search(dc, searchExpression, sortField, startRowIndex, maximumRows);
                return HubResult.CreateSuccessData(result);
            });
        }

        public HubResult SearchContacts(string searchExpressionString)
        {
            return standardHeader((siteContext, dc) =>
            {
                var sortField = "Name";
                var searchExpression = SearchExpression.Create(searchExpressionString, SearchExpression.Detail.High);
                var result = Contact.Search(dc, searchExpression, sortField, 0, int.MaxValue);
                return HubResult.CreateSuccessData(result);
            });
        }


        public HubResult AddRandomDirectoryContact(int directoryID)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Directory.AddRandomContact(dc, directoryID);
            });
        }

        public HubResult GetDirectoryContactsText(int directoryID)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Directory.GetContactsText(dc, directoryID);
            });
        }

        public HubResult SetDirectoryContactsText(int directoryID, string contactsText)
        {
            return standardHeader((siteContext, dc) =>
            {
                return Directory.SetContactsText(dc, directoryID, contactsText);
            });
        }
#endif


#if false
        public HubResult GetDirectoryContacts(int directoryID)
        {
            return standardHeader((siteContext, dc) =>
            {
                var directory = Directory.FindByID(dc, directoryID);

                var directoryContacts = Contact.Query(dc)
                    .Where(contact => contact.DirectoryID == directoryID)
                    .Select(contact => new
                    {
                        contact.ID,
                        contact.FirstName,
                        contact.LastName,
                        contact.PhoneNumber,
                        //!! need to add email address
                        Email = @"biff@email.com",
                        contact.ContactPhotoS3Path,

                        Spouse = contact.Spouse,
                        SpouseContactID = contact.SpouseID,
                        SpouseFirstName = contact.Spouse != null ? contact.Spouse.FirstName : null,
                        SpouseLastName = contact.Spouse != null ? contact.Spouse.LastName : null,
                    })
                    .AsEnumerable()
                    .Select(contact => new
                    {
                        id = contact.ID,

                        name = formatName(contact.FirstName, contact.LastName, contact.SpouseFirstName, contact.SpouseLastName),
                        firstName = contact.FirstName,
                        lastName = contact.LastName,
                        phoneNumber = contact.PhoneNumber,
                        email = contact.Email,
                        contactPhotoS3Path = contact.ContactPhotoS3Path,

                        spouseContactID = contact.SpouseContactID,
                        spouseFirstName = contact.SpouseFirstName,
                        spouseLastName = contact.SpouseLastName,
                    })
                    .OrderBy(contact => contact.firstName)
                    .ThenBy(contact => contact.lastName)
                    .ToArray();

                return HubResult.CreateSuccessData(directoryContacts);
            });
        }
#endif

        // tags in general are automatically created when assigned to an item
        // tags generally auto-delete when they are no longer being used
        // tenants can have a hidden object that manually created tags are assigned to to ensure they aren't automatically deleted
        // a TagSet is the set of all currently created assignable tags

        // pre-defined tags - can't be deleted
        // system tags - can't be manually assigned or removed

        // tags - shared across a tenant group

        // user tags - assigned by each user


        // #tag searches for a specific tag
        // @person searches for an actor?
        // 

#if false
        public HubResult GetShowVideos(int showID)
        {
            return standardHeader((siteContext, dc) =>
            {
                var show = Show.FindByID(dc, showID);

                var searchExpression = SearchExpression.Empty;

                var searchResults = Video.Search(dc, searchExpression, null, 0, 300);
                return HubResult.CreateSuccessData(searchResults);
            });
        }
#endif








#if false
        // maps Signalr connectionID to Device
        private static readonly Dictionary<string, Device> connectionDeviceMap = new Dictionary<string, Device>();

        private HubResult standardHeader(Func<SiteContext, UtilityDC, Device, HubResult> standardHandler)
        {
            Identity authorizedBy = Context.User.Identity as Identity;
            var siteContext = SiteContext.Current;

            Device device;
            if (!CollaborationHub.connectionDeviceMap.TryGetValue(Context.ConnectionId, out device))
            {
                // Odd - device not found. This client must be alive from a previous server session
                this.Clients.Client(Context.ConnectionId).resetConnection();
                return HubResult.Error;
            }

            using (var dc = siteContext.CreateDefaultAccountsOnlyDC<UtilityDC>(DateTime.UtcNow, authorizedBy))
            {
                var result = standardHandler(siteContext, dc, device);
                return result;
            }
        }

        public override Task OnConnected()
        {
            Identity authorizedBy = Context.User.Identity as Identity;
            var siteContext = SiteContext.Current;

            Debug.Assert(!CollaborationHub.connectionDeviceMap.ContainsKey(Context.ConnectionId));

            //!! for now since we don't persist a device cookie create a new device with each connection
            var device = Device.Create(Context.ConnectionId);
            CollaborationHub.connectionDeviceMap[Context.ConnectionId] = device;

            siteContext.CollaborationContext.OnDeviceConnected(device);

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            standardHeader((siteContext, dc, device) =>
            {
                CollaborationHub.connectionDeviceMap.Remove(Context.ConnectionId);
                siteContext.CollaborationContext.OnDeviceDisconnected(device);
                return HubResult.Success;
            });

            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            standardHeader((siteContext, dc, device) =>
            {
                siteContext.CollaborationContext.OnDeviceReconnected(device);
                return HubResult.Success;
            });

            return base.OnReconnected();
        }

        public HubResult SetDeviceName(string deviceName)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.SetDeviceName(device, deviceName);
            });
        }

        public HubResult CreateSession(string activityType)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.CreateSession(device, activityType);
            });
        }

        public HubResult EndSession()
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.EndSession(device);
            });
        }

        public HubResult UpdateParticipantSessionData(object sessionData)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.UpdateParticipantSessionData(device, sessionData);
            });
        }

        public HubResult UpdateModeratorSessionData(object sessionData)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.UpdateModeratorSessionData(device, sessionData);
            });
        }

        public HubResult BroadcastOneShotData(object oneShotData)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.BroadcastOneShotData(device, oneShotData);
            });
        }

        public HubResult UpdateMyData(object myData)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.UpdateMyData(device, myData);
            });
        }

        public HubResult DoCommand(string command, object commandData)
        {
            return standardHeader((siteContext, dc, device) =>
            {
                return siteContext.CollaborationContext.DoCommand(device, command, commandData);
            });
        }
#endif
    }
}