using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using MS.Utility;
using MS.TemplateReports;

namespace App.Library
{
    public partial class Participant
    {
        partial void OnLoaded()
        {
            // (ensure DB timestamps are correctly marked as UTC.)
            this._CreatedTimestamp = DateTime.SpecifyKind(this.CreatedTimestamp, DateTimeKind.Utc);
            this._LastModifiedTimestamp = DateTime.SpecifyKind(this.LastModifiedTimestamp, DateTimeKind.Utc);
        }
    }
    public partial class Participant : ExtendedObject<Participant>, IEPScopeObject
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return this.ScopeType; } }
        protected override int? objectScopeID { get { return this.ScopeID; } }

        public string FullName
        {
            get { return User.GenerateFullName(this.FirstName, this.LastName); }
        }


        private static IQueryable<Participant> query(AppDC dc)
        {
            var result = dc.Participants
                .Select(item => item);
            return result;
        }

        public static IQueryable<Participant> Query(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrInvalid;

            var result = query(dc)
                .FilterBy(teamEPScope);

            return result;
        }

        private static Func<IQueryable<Participant>, string, IQueryable<Participant>> termFilter = (query, searchTermLower) =>
        {
            return query.Where(item =>
                item.FirstName.Contains(searchTermLower) ||
                item.LastName.Contains(searchTermLower));
        };

        public static IQueryable<Participant> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression, termFilter);
            return query;
        }

        public static IQueryable<Participant> Query(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var query = Query(dc, searchExpression);
            query = query.SortBy(sortExpression, startRowIndex, maximumRows);
            return query;
        }

        public static int QueryCount(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            return query.Count();
        }


        public static IQueryable<Participant> Query(AppDC dc, int participantGroupID)
        {
            var result = Query(dc)
                .Where(episode => episode.ParticipantGroupID == participantGroupID);
            return result;
        }

        public static Participant FindByID(AppDC dc, int itemID)
        {
            var result = Participant.Query(dc)
                .FirstOrDefault(item => item.ID == itemID);
            return result;
        }


        public static IQueryable<Participant> Query(AppDC dc, ParticipantGroup itemGroup)
        {
            var query = Query(dc, itemGroup.ID);

            return query;
        }



        protected static IQueryable<ExtendedItem<Participant>> ExtendedQuery(AppDC dc)
        {
            return ExtendedQuery(dc, Query(dc));
        }

        public static IQueryable<ExtendedItem<Participant>> ExtendedQuery(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            var exQuery = ExtendedQuery(dc, query);

#if false
            var useNetFilter = searchExpression.GetNamedStateTerms("source").Contains("UseNet");
            if (useNetFilter)
            {
                exQuery =
                    from exItem in exQuery
                    where (from nzbPost in NzbPost.Query(dc)
                           select nzbPost.EpisodeID.Value)
                           .Contains(exItem.item.ID)
                    select exItem;
            }
#endif

            // code to allow us to track EpisodeStatus - if we add that in to our data model
#if false
            if (searchExpression.StateTerms.Any())
            {
                // Query for an OR of any of these states using PredicateBuilder http://www.albahari.com/nutshell/predicatebuilder.aspx
                var predicate = PredicateBuilder.False<ExtendedItem<Episode>>();
                searchExpression.StateTerms.ForEach(stateString =>
                {
                    var status = stateString.ParseAsEnumOrNull<EpisodeStatus>();
                    if (status.HasValue)
                    {
                        predicate = predicate.Or(exItem => exItem.item.Status == status);
                    }
                });
                exQuery = exQuery
                    .Where(predicate);
            }
#endif
            return exQuery;
        }

        public static IQueryable<ExtendedItem<Participant>> ExtendedQuery(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var epQuery = ExtendedQuery(dc, searchExpression);
            epQuery = epQuery.SortBy(sortExpression, startRowIndex, maximumRows);
            return epQuery;
        }











        public class SearchItem : ExtendedSearchItem
        {
            [JsonProperty("firstName")]
            public string FirstName { get; internal set; }
            [JsonProperty("lastName")]
            public string LastName { get; internal set; }

            [JsonProperty("participantGroupID")]
            public int ParticipantGroupID { get; internal set; }

            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }

            public SearchItem(ExtendedItem<Participant> exItem, SearchItemContext context)
                : base(exItem, context)
            {
                this.FirstName = exItem.item.FirstName;
                this.LastName = exItem.item.LastName;
                this.ParticipantGroupID = exItem.item.ParticipantGroupID;

                this.CreatedTimestamp = exItem.item.CreatedTimestamp;

                // https://s3.amazonaws.com/gopvr-com-lab-public/tvdb/S-124971/P-posters/124971-1.jpg
                //this.imageUrl = string.IsNullOrEmpty(exItem.item.ImageS3Path) ? null :
                //new Uri(SiteContext.Current.PublicBucketUrl + exItem.item.ImageS3Path);
            }

            public static SearchItem Create(ExtendedItem<Participant> item, SearchItemContext context)
            {
                return new SearchItem(item, context);
            }
        }


        public static SearchResult<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            // basic query that determines the objects that pass the search expression.
            // (if the search expression contains Tags or other 1:many items, we'll need to do more work to filter down to those
            var resultSetQuery = ExtendedQuery(dc, searchExpression);

            //resultSetQuery = resultSetQuery
            //.OrderBy(foo => foo.item.ReleasedTimestamp);

            var results = Search(dc, searchExpression, sortExpression, startRowIndex, maximumRows, resultSetQuery, SearchItem.Create);
            return results;
        }




#if false
        public static SearchResults<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, uint startRowIndex, uint maximumRows)
        {

            // var groupEPScope = dc.TransactionAuthorizedBy.GroupEPScope;
            // var perUserEPScope = dc.TransactionAuthorizedBy.PerUserEPScopeOrNull;

#if false
            //!! until we've got a background scanner going
            if (searchExpression.Tags.Contains("Rename"))
            {
                //!! change this to a join to be more efficient in grabbing the Tags...
                foreach(var video in Query(dc).Take(250))
                {
                    var canonicalFileName = video.GetCanonicalFileName(dc);
                    bool renameRequired = !string.Equals(video.FileName, canonicalFileName, StringComparison.InvariantCulture);

                    Video.ModifyTag(dc, video.ID, "Rename", renameRequired);
                }
            }
#endif
            // We need to respect parameters startRowIndex & maximumRows, but that's after filtering/sorting.
            // So have to initially grab all rows
            var query = Episode.Query(dc, searchExpression, sortExpression, 0, int.MaxValue);

            // After lots of trial & error, it seems like the only way to keep this down to a few SQL queries is to 
            // separately grab Tags & Episodes, then join locally 

            var epQuery =
                from episode in query
                //join epTag in Show.QueryEPTags(dc) on show.ID equals epTag.ID into epTagGroup
                //from epTag in epTagGroup.DefaultIfEmpty()

                //join applicationEPTag in ExtendedProperty.QueryAssignedTags<Episode>(dc, ApplicationEPCategory) on episode.ID equals applicationEPTag.ID into applicationEPTagGroup

                join applicationPerUserEPTag in QueryPerUserEPTags(dc, EPCategory.UserAssigned) on episode.ID equals applicationPerUserEPTag.ID into applicationPerUserEPTagJoin
                join applicationGroupEPTag in QueryGroupEPTags(dc, EPCategory.UserAssigned) on episode.ID equals applicationGroupEPTag.ID into applicationGroupEPTagJoin



                //!! need something like this if we accept user-defined tags
                //join perUserEPTag in ExtendedObject<Episode>.QueryPerUserEPTags(dc, EPCategory.UserAssigned) on episode.ID equals userEPTag.ID into userEPTagGroup
                //from userEPTag in userEPTagGroup.DefaultIfEmpty()

                select new
                {
                    episode,
                    applicationPerUserEPTagJoin,
                    applicationGroupEPTagJoin,
                };
            var epQuerySelect = epQuery
                .Select(epEpisode => new
                {
                    epEpisode.episode.ID,
                    epEpisode.episode.Name,
                    epEpisode.episode.Overview,

                    epEpisode.episode.SeasonNumber,
                    epEpisode.episode.EpisodeNumber,
                    epEpisode.episode.ReleasedTimestamp,

                    epEpisode.episode.ImageS3Path,
                    ApplicationPerUserTagNames = epEpisode.applicationPerUserEPTagJoin
                        .Select(epTag => epTag.Tag.Name)
                        .ToArray(),
                    ApplicationGroupTagNames = epEpisode.applicationGroupEPTagJoin
                        .Select(epTag => epTag.Tag.Name)
                        .ToArray(),
                });

            if (searchExpression != null)
            {
                searchExpression.Tags.ForEach(searchTag =>
                {
                    epQuerySelect = epQuerySelect
                        .Where(epShow =>
                            epShow.ApplicationPerUserTagNames.Any(tagName => tagName == searchTag) ||
                            epShow.ApplicationGroupTagNames.Any(tagName => tagName == searchTag)
                        );
                });
            }

            var epQuerySelectLimited = epQuerySelect
                .Skip((int)startRowIndex)
                .Take((int)maximumRows);

            var epQuerySelectArray = epQuerySelectLimited
                .OrderByDescending(episode => episode.SeasonNumber)
                .ThenByDescending(episode => episode.EpisodeNumber)
                .ThenByDescending(episode => episode.ReleasedTimestamp)
                .ToArray();

            if (searchExpression.ContentDetail != SearchExpression.Detail.High)
            {
                // Don't bother querying for this extra detail
                epQuerySelectLimited = epQuerySelectLimited
                    .Where(videoItem => false);
            }


            // Augment each show with information about available Episodes
            var videosQuery =
                from epEpisode in epQuerySelectLimited
                join video in Video.Query(dc) on epEpisode.ID equals video.EpisodeID into videoGroup
                select new
                {
                    EpisodeID = epEpisode.ID,
                    videoGroup,
                };
            var episodeVideosQuerySelectArray = videosQuery
                .Select(episodeVideo => new
                {
                    episodeVideo.EpisodeID,

                    VideoAvailableEpisodeIDs = episodeVideo.videoGroup
                        .Where(video => video.EpisodeID.HasValue)
                        .Select(video => video.EpisodeID.Value)
                        .ToArray(),
                })
                .ToArray();


            var currentReleasedTimestamp = DateTime.UtcNow.Date;

            var epVideoEpisodesQuery =
                from epEpisode in epQuerySelectArray
                join episodeVideos in episodeVideosQuerySelectArray on epEpisode.ID equals episodeVideos.EpisodeID into episodeVideosGroup
                from episodeVideos in episodeVideosGroup.DefaultIfEmpty()
                select new
                {
                    epEpisode,

#if false
                    SeriesAvailabilityMapData = epQuerySelectArray
                        .GroupBy(episode => episode.SeasonNumber)
                        .Select(episodeGroup => new
                        {
                            SeasonNumber = episodeGroup.Key,
                            UnReleasedEpisodeNumbers = episodeGroup
                                .Where(episode => episode.ReleasedTimestamp != null)
                                .Where(episode => episode.ReleasedTimestamp.Value > currentReleasedTimestamp)
                                .Where(episode => episode.EpisodeNumber != null)
                                .Select(episode => episode.EpisodeNumber.Value)
                                .OrderBy(episodeNumber => episodeNumber)
                                .ToArray(),
                            KnownEpisodeNumbers = episodeGroup
                                .Where(episode => episode.EpisodeNumber != null)
                                .Select(episode => episode.EpisodeNumber.Value)
                                .OrderBy(episodeNumber => episodeNumber)
                                .ToArray(),
                            VideoAvailableEpisodeNumbers = episodeGroup
                                .Where(episode => episode.EpisodeNumber != null)
                                .Where(episode => episodeVideos.VideoAvailableEpisodeIDs.Contains(episode.ID))
                                .Select(episode => episode.EpisodeNumber.Value)
                                .OrderBy(episodeNumber => episodeNumber)
                                .ToArray(),
                            ReleaseTimestamps = episodeGroup
                                .Where(episode => episode.ReleasedTimestamp != null)
                                .Select(episode => episode.ReleasedTimestamp.Value)
                                .OrderBy(releasedTimestamp => releasedTimestamp)
                                .ToArray(),
                        })
                        .OrderByDescending(episodeGroup => episodeGroup.SeasonNumber)
                        .ToArray(),

                    MonthlyReleaseAvailabilityMapData = epQuerySelectArray
                        .Where(episode => episode.ReleasedTimestamp.HasValue)
                        .GroupBy(episode => new { episode.ReleasedTimestamp.Value.Year, episode.ReleasedTimestamp.Value.Month })
                        .Select(episodeGroup => new
                        {
                            YearMonth = episodeGroup.Key,
                            EpisodeReleasedDayOfMonth = episodeGroup
                                .Select(episode => episode.ReleasedTimestamp.Value.Day)
                                .OrderBy(day => day)
                                .ToArray(),
                            VideoAvailableDayOfMonth = episodeGroup
                                .Where(episode => episodeVideos.VideoAvailableEpisodeIDs.Contains(episode.ID))
                                .Select(episode => episode.ReleasedTimestamp.Value.Day)
                                .OrderBy(day => day)
                                .ToArray(),
                        })
                        .OrderByDescending(episodeGroup => episodeGroup.YearMonth.Year)
                        .ThenByDescending(episodeGroup => episodeGroup.YearMonth.Month)
                        .ToArray(),

                    LatestEpisode = epQuerySelectArray
                        .FirstOrDefault(),
#endif
                    VideoAvailabilityData = episodeVideos.VideoAvailableEpisodeIDs,
                };

            var result = new SearchResults<SearchItem>()
            {
                items = epVideoEpisodesQuery
                    .Select(epShowEpisodesGroup => new SearchItem()
                    {
                        id = epShowEpisodesGroup.epEpisode.ID,

                        seasonNumber = epShowEpisodesGroup.epEpisode.SeasonNumber,
                        episodeNumber = epShowEpisodesGroup.epEpisode.EpisodeNumber,
                        releasedTimestamp = epShowEpisodesGroup.epEpisode.ReleasedTimestamp,

                        mark = FormatEpisodeMark(
                            epShowEpisodesGroup.epEpisode.SeasonNumber,
                            epShowEpisodesGroup.epEpisode.EpisodeNumber,
                            epShowEpisodesGroup.epEpisode.ReleasedTimestamp),

                        name = epShowEpisodesGroup.epEpisode.Name,
                        overview = epShowEpisodesGroup.epEpisode.Overview,
                        //!! posterUrl =    new Uri(@"http://thetvdb.com/banners/" + epShowEpisodesGroup.epShow.PosterS3Path),

                        // https://s3.amazonaws.com/lab-static.gopvr.com/tvdb-S-77623-P-posters/77623-3.jpg
                        imageUrl = string.IsNullOrEmpty(epShowEpisodesGroup.epEpisode.ImageS3Path) ? null :
                            new Uri(@"https://s3.amazonaws.com/lab-static.gopvr.com/" + epShowEpisodesGroup.epEpisode.ImageS3Path),

                        //!! userTags = epShowEpisodesGroup.epEpisode.UserTagNames,

                        isPerUserWatched = epShowEpisodesGroup.epEpisode.ApplicationPerUserTagNames
                            .Select(epTag => epTag)
                            .FirstOrDefault(tag => tag == Episode.Watched_PerUser_TagName) != null,

#if false
                        seriesAvailabilityMap = epShowEpisodesGroup.SeriesAvailabilityMapData == null ? null : epShowEpisodesGroup.SeriesAvailabilityMapData
                            .Where(episode => episode.SeasonNumber > 0)
                            .Select(seasonMapData => new
                            {
                                seasonNumber = seasonMapData.SeasonNumber,
                                map = Episode.GetEpisodeAvailabilityMap(seasonMapData.KnownEpisodeNumbers, seasonMapData.VideoAvailableEpisodeNumbers, seasonMapData.UnReleasedEpisodeNumbers)
                            })
                            .ToArray(),

                        miniSeriesAvailabilityMap = epShowEpisodesGroup.SeriesAvailabilityMapData == null ? null : epShowEpisodesGroup.SeriesAvailabilityMapData
                            .Where(seasonMapData => seasonMapData.SeasonNumber == 0)
                            .Select(seasonMapData => new
                            {
                                map = Episode.GetEpisodeAvailabilityMap(seasonMapData.KnownEpisodeNumbers, seasonMapData.VideoAvailableEpisodeNumbers, seasonMapData.UnReleasedEpisodeNumbers)
                            })
                            .ToArray(),

                        monthlyAvailabilityMap = epShowEpisodesGroup.MonthlyReleaseAvailabilityMapData == null ? null : epShowEpisodesGroup.MonthlyReleaseAvailabilityMapData
                            .Select(monthlySeasonMapData => new
                            {
                                year = monthlySeasonMapData.YearMonth.Year,
                                month = monthlySeasonMapData.YearMonth.Month,
                                map = Episode.GetWeeknightAvailabilityMap(monthlySeasonMapData.YearMonth.Year, monthlySeasonMapData.YearMonth.Month, monthlySeasonMapData.EpisodeReleasedDayOfMonth, monthlySeasonMapData.VideoAvailableDayOfMonth),
                            })
                            .ToArray(),
#endif
                    })
                    .ToArray(),
                totalCount = epQuerySelect.Count(),
                deletedIDs = searchExpression
                    .DeletedIDs
                    .ToArray(),
            };

            return result;
        }
#endif


        private static HubResult CreateLock(AppDC dc, Func<Participant> createHandler)
        {
            var newCase = createLock(dc, createHandler);
            if (newCase != null)
            {
                return HubResult.CreateSuccessData(new { id = newCase.ID });
            }

            return HubResult.Error;
        }

        private static Participant exCreateLock(AppDC dc, JToken data, Func<Participant> createHandler)
        {
            return exCreateLock(dc, data, NotifyClients, createHandler);
        }

        private static Participant createLock(AppDC dc, Func<Participant> createHandler)
        {
            return createLock(dc, NotifyClients, createHandler);
        }

        internal static T ReadLock<T>(AppDC dc, int itemID, Func<Participant, T> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult ReadLock(AppDC dc, int itemID, Func<Participant, HubResult> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult WriteLock(AppDC dc, int itemID, Func<Participant, NotifyExpression, HubResult> writeHandler)
        {
            return WriteLock(dc, itemID, FindByID, NotifyClients, writeHandler);
        }





#if false
        internal static void NotifyClients(AppDC dc, SearchExpression notifyExpression)
        {
            var utilityContext = UtilityContext.Current;
            var authorizedBy = dc.TransactionAuthorizedBy;

            // Generate a master notification - authorized by our caller - so it's all the items they are authorized to see. Others might need to see a filtered subset.
            var notification = Case.Search(dc, notifyExpression, null, 0, int.MaxValue);


            // Fetch our filters - which allows the Application layer to swap them out if necessary
            var clientFilter = utilityContext.AppInfo.ClientFilterFactory(dc);

            // We need to create an Item Filter - which determines which cases each specific connection is allowed to see. 
            // SystemAdmins and TenantAdmins will see all cases, so they shouldn't show up here.
            // For the remaining roles, our logic will be to check if the user is authorized for a specific hospital, and if they are pass through any cases from that hospital

            var userToAuthorizedHospitalIDsMap = HospitalUser.GetGroupUserToAuthorizedHospitalIDsMap(dc.TransactionAuthorizedBy);
            PresenceManager.ItemFilter<SearchItem> itemFilter = (connection, item) =>
            {
                var connectionIdentity = connection.Identity;
                Debug.Assert(connectionIdentity != null);

                // (shouldn't be handling itemFilter for these fellas)
                Debug.Assert(!connectionIdentity.IsSystemAdmin);
                Debug.Assert(!connectionIdentity.IsTenantAdmin);

                // Others get a filtered notification, based on the Hospitals they're authorized to see
                var connectionUserID = connectionIdentity.UserIDOrNull;
                if (connectionUserID.HasValue)
                {
                    int[] connectionAuthorizedHospitalIDs;
                    if (userToAuthorizedHospitalIDsMap.TryGetValue(connectionUserID.Value, out connectionAuthorizedHospitalIDs) &&
                        connectionAuthorizedHospitalIDs.Contains(item.hospitalID))
                    {
                        // Yes - we're authorized for this item because our user is authorized at the hospital
                        return true;
                    }
                }

                if (connectionIdentity.AuthorizedTableHashCode == Hospital.TableHashCode &&
                    connectionIdentity.AuthorizedIDs.Contains(item.hospitalID))
                {
                    // Yes - we're authorized for this item because we're hospital authorized
                    return true;
                }

                return false;
            };

            var hubClients = utilityContext.ConnectionManager.GetHubContext("siteHub").Clients;
            Debug.Assert(hubClients != null);

            utilityContext.PresenceManager.NotifyClients<SearchItem>(authorizedBy, notification, clientFilter, itemFilter,
                (connectionIDs, filteredNotification) => hubClients.Clients(connectionIDs).updateCases(filteredNotification)
            );
        }
#endif


        internal static void NotifyClients(AppDC dc, NotifyExpression notifyExpression)
        {
            var notification = Participant.Search(dc, notifyExpression, null, 0, int.MaxValue);
            NotifyClients("siteHub", notifyExpression, notification, (hubClients, notificationItem) => hubClients.All.updateParticipants(notificationItem));
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public static Participant GenerateRandom(AppDC dc)
        {
            return GenerateRandom(dc, null);
        }

        public static Participant GenerateRandom(AppDC dc, int[] participantGroups)
        {
            var random = RandomProvider.GetThreadRandom();

            // generate a random contact
            var randomContactJson = User.GenerateRandomContact();

            dynamic userNameJson = randomContactJson.name;
            string firstName = userNameJson.first;
            string lastName = userNameJson.last;
            string gender = randomContactJson.gender;

            dynamic userLocationJson = randomContactJson.location;
            string streetAddress = userLocationJson.street;

            string phoneNumber = randomContactJson.phone;
            string email = randomContactJson.email;
            string profilePhotoUrlString = randomContactJson.picture;
            string ssn = randomContactJson.SSN;

            var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            var firstNameCapitalized = textInfo.ToTitleCase(firstName);
            var lastNameCapitalized = textInfo.ToTitleCase(lastName);
            var mailAddress = email.ParseMailAddress();

            var data = new
            {
                firstName = firstNameCapitalized,
                lastName = lastNameCapitalized,

                gender = gender,

                participantGroupID = participantGroups.ChooseRandom(),
            }.ToJson().FromJson();

            var result = Participant.createLock(dc, () =>
            {
                var newParticipant = Participant.Create(dc, data);
                return newParticipant;
            });

            return result;

        }

        public static Participant Create(AppDC dc, dynamic data)
        {
            return Participant.createLock(dc, () =>
            {
                var createdTimestamp = dc.TransactionTimestamp;
                var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

                var firstName = (string)data.firstName;
                var lastName = (string)data.lastName;

                Debug.Assert(!string.IsNullOrEmpty(firstName));
                Debug.Assert(!string.IsNullOrEmpty(lastName));

                var participantGroupID = (int)data.participantGroupID;

                var newItem = new Participant(createdTimestamp, teamEPScope, firstName, lastName, participantGroupID);

                var genderString = (string)data.gender;
                UserGender? userGender = genderString.ParseUserGenderOrNull();
                newItem.Gender = userGender;

                dc.Save(newItem);

                Debug.Assert(newItem.ID > 0);

                return newItem;
            });
        }

    private void updateData(AppDC dc, dynamic data)
    {
      this.FirstName = (string)data.firstName;
      this.LastName = (string)data.lastName;

      var genderString = (string)data.gender;
      UserGender? userGender = genderString.ParseUserGenderOrNull();
      this.Gender = userGender;

      var participantGroupID = (int?)data.participantGroupID;
      if (participantGroupID.HasValue)
      {
        var participantGroup = participantGroupID.HasValue ? ParticipantGroup.FindByID(dc, participantGroupID.Value) : null;
        if (participantGroup != null)
        {
          this.ParticipantGroupID = participantGroupID.Value;
        }
      }
    }

    public static HubResult Edit(AppDC dc, int itemID, dynamic data)
        {
          return WriteLock(dc, itemID, (item, notifyExpression) =>
          {
            item.updateData(dc, data);

            notifyExpression.AddModifiedID(item.ID);
            return HubResult.Success;
          });
        }

    protected Participant(DateTime createdTimestamp, EPScope epScope, string firstName, string lastName, int participantGroupID)
            : this()
        {
            Debug.Assert(!string.IsNullOrEmpty(firstName));
            Debug.Assert(!string.IsNullOrEmpty(lastName));

            this.CreatedTimestamp = createdTimestamp;
            this.ScopeType = epScope.ScopeType;
            this.ScopeID = epScope.ID;

            this.FirstName = firstName;
            this.LastName = lastName;

            this.ParticipantGroupID = participantGroupID;
        }

        public static ReportGenerator GetReportGenerator(AppDC appDC, string templateName, ReportFormat reportFormat, int itemID)
        {
            var siteContext = SiteContext.Current;

            TagProvider tagProvider = null;
            tagProvider = Participant.createParticipantTagProvider(appDC, itemID);

            ReportGenerator reportGenerator = null;
            if (tagProvider != null)
            {
                var documentTemplate = DocumentTemplate.FromResource("AppLibrary.Reports." + templateName, null);
                reportGenerator = siteContext.TemplateReports.Generate(reportFormat, tagProvider, documentTemplate);
            }

            return reportGenerator;
        }

        private static TagProvider createParticipantTagProvider(AppDC appDC, int itemID)
        {
            var exQuery =
                from epParticipant in ExtendedQuery(appDC)
                join school in ParticipantGroup.Query(appDC) on epParticipant.item.ParticipantGroupID equals school.ID
                select new { epParticipant };

            var exResult = exQuery
                .Where(exItem => exItem.epParticipant.item.ID == itemID)
                .FirstOrDefault();

            var participant = exResult.epParticipant.item;

            IEnumerable<ProviderTag> tags = new ProviderTag[]
            {
                StringProviderTag.Create("FirstName", participant.FirstName),
                StringProviderTag.Create("LastName", participant.LastName),
                StringProviderTag.Create("FullName", participant.FullName),

                StringProviderTag.Create("SchoolName", participant.ParticipantGroup.Name),
                StringProviderTag.Create("Address", "Main Street, Redmond WA 98052"),
            };

            return TagProvider.Create(tags);
        }
    }
}