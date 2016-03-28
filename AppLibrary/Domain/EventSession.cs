using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using MS.Utility;

namespace App.Library
{

    public enum EventSessionStatus
    {
        // The date/time of the event session hasn't arrived, and nobody as hit the "run" button to kick off the event session yet
        //Plan = 1,
        // The event session is underway! Let the participants flow
        //Active = 2,
        //Completed = 3,
        //Archived = 4,

        Upcoming = 1,
        Live = 2,
        Past = 3,


    }


    public partial class EventSession
    {
        partial void OnLoaded()
        {
            // (ensure DB timestamps are correctly marked as UTC.)
            this._CreatedTimestamp = DateTime.SpecifyKind(this.CreatedTimestamp, DateTimeKind.Utc);
            this._LastModifiedTimestamp = DateTime.SpecifyKind(this.LastModifiedTimestamp, DateTimeKind.Utc);

            Debug.Assert(this._StartDate.Kind == DateTimeKind.Unspecified);

            this._StartDate = DateTime.SpecifyKind(this.StartDate, DateTimeKind.Utc);
            this._EndDate = DateTime.SpecifyKind(this.EndDate, DateTimeKind.Utc);
        }
    }
    public partial class EventSession : ExtendedObject<EventSession>, IEPScopeObject
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return this.ScopeType; } }
        protected override int? objectScopeID { get { return this.ScopeID; } }


        protected EventSession(DateTime createdTimestamp, EPScope epScope, int eventID)
            : this()
        {
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;

            this.ScopeType = epScope.ScopeType;
            this.ScopeID = epScope.ID;

            this.EventID = eventID;

            this.CheckInOpen = false;
        }

        protected EventSession(DateTime createdTimestamp, EPScope epScope, int eventID, string name, string location, DateTime startDate, DateTime endDate)
            : this(createdTimestamp, epScope, eventID)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            this.Name = name;
            this.Location = location;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        private static Func<IQueryable<EventSession>, string, IQueryable<EventSession>> termFilter = (query, searchTermLower) =>
        {
            return query.Where(item =>
                item.Name.Contains(searchTermLower));
        };


        private static IQueryable<EventSession> query(AppDC dc)
        {
            var result = dc.EventSessions
                .Select(item => item);
            return result;
        }

        public static IQueryable<EventSession> Query(AppDC dc)
        {
            var authorizedBy = dc.TransactionAuthorizedBy;
            Debug.Assert(authorizedBy != null);

            var query = EventSession.query(dc);

            if (authorizedBy.IsSystem)
            {
                return query;
            }


            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrInvalid;
            var result = query
                // http://stackoverflow.com/questions/586097/compare-nullable-types-in-linq-to-sql
                .Where(item => item.ScopeType == ExtendedPropertyScopeType.Global ||
                    item.ScopeType == teamEPScope.ScopeType && object.Equals(item.ScopeID, teamEPScope.ID));

            return result;
        }

        public static IQueryable<EventSession> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression, termFilter);

            // support filtering to a specific Event. eg. $event:{eventID}
            query = searchExpression.FilterByAnyNamedStateTerm2(query, "event",
                searchTerm => item => item.EventID.ToString() == searchTerm);

            return query;
        }

        public static IQueryable<EventSession> Query(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var query = Query(dc, searchExpression);

            if (searchExpression.ParentIDs != null)
            {
                query = query.Where(eventSession => searchExpression.ParentIDs.Contains(eventSession.EventID));
            }

            query = query.SortBy(sortExpression, startRowIndex, maximumRows);
            return query;
        }

        public static int QueryCount(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            return query.Count();
        }

#if false
        public static IQueryable<Show> QueryUpcomingShowsAvailable(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var result = Query(dc)
                .Where(show => show.Name == "scubba");

            return result;
        }
#endif


        protected static IQueryable<ExtendedItem<EventSession>> ExtendedQuery(AppDC dc)
        {
            return ExtendedQuery(dc, Query(dc));
        }

        public static IQueryable<ExtendedItem<EventSession>> ExtendedQuery(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            var exQuery = ExtendedQuery(dc, query);
            return exQuery;
        }

        public static IQueryable<ExtendedItem<EventSession>> ExtendedQuery(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var epQuery = ExtendedQuery(dc, searchExpression);
            epQuery = epQuery.SortBy(sortExpression, startRowIndex, maximumRows);
            return epQuery;
        }



        public class SearchItem : ExtendedSearchItem
        {
            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }
            [JsonProperty("lastModifiedTimestamp")]
            public DateTime LastModifiedTimestamp { get; internal set; }

            //!! [JsonProperty("status"), JsonConverter(typeof(StringEnumConverter))]
            //!! public EventSessionStatus State { get; internal set; }

            [JsonProperty("checkInOpen")]
            public bool CheckInOpen { get; internal set; }

            //public string type { get; internal set; }
            [JsonProperty("name")]
            public string Name { get; internal set; }
            [JsonProperty("overview")]
            public string Overview { get; internal set; }

            [JsonProperty("startDate")]
            public DateTime StartDate { get; internal set; }
            [JsonProperty("endDate")]
            public DateTime EndDate { get; internal set; }

            [JsonProperty("eventID")]
            public int EventID { get; internal set; }

            [JsonProperty("location")]
            public string Location { get; internal set; }

            [JsonProperty("locationStreetAddress")]
            public string LocationStreetAddress { get; internal set; }
            [JsonProperty("locationCity")]
            public string LocationCity { get; internal set; }
            [JsonProperty("locationState")]
            public string LocationState { get; internal set; }
            [JsonProperty("locationZipCode")]
            public string LocationZipCode { get; internal set; }

            [JsonProperty("authInfo")]
            public object AuthInfo { get; internal set; }


            public SearchItem(ExtendedItem<EventSession> exItem, SearchItemContext context, object authInfo)
                : base(exItem, context)
            {
                this.CreatedTimestamp = exItem.item.CreatedTimestamp;
                this.LastModifiedTimestamp = exItem.item.LastModifiedTimestamp;

                Debug.Assert(this.CreatedTimestamp.Kind == DateTimeKind.Utc);
                Debug.Assert(this.LastModifiedTimestamp.Kind == DateTimeKind.Utc);

                this.CheckInOpen = exItem.item.CheckInOpen;

                this.Name = exItem.item.Name;
                this.Overview = exItem.item.Overview;

                this.StartDate = exItem.item.StartDate;
                this.EndDate = exItem.item.EndDate;

                Debug.Assert(this.StartDate.Kind == DateTimeKind.Utc);
                Debug.Assert(this.EndDate.Kind == DateTimeKind.Utc);

                this.EventID = exItem.item.EventID;
                this.Location = exItem.item.Location;

                this.LocationStreetAddress = exItem.item.LocationStreetAddress;
                this.LocationCity = exItem.item.LocationCity;
                this.LocationState = exItem.item.LocationState;
                this.LocationZipCode = exItem.item.LocationZipCode;

                this.AuthInfo = authInfo;
            }

            public static SearchItem Create(ExtendedItem<EventSession> item, SearchItemContext context)
            {
                var optionTags = context.TeamEPScopeItemTagCategoryGroups
                    .Where(group => group.Key == EPCategory.OptionCategory)
                    .SelectMany(groupElement => groupElement
                        .Select(epTagsItem => epTagsItem.Tag.Name))
                    .ToArray();

                AppDC appDC = context.UtilityDC as AppDC;
                Debug.Assert(appDC != null);

                var volunteerAuthInfo = getVolunteerAuthInfo(appDC, item.itemID);

                return new SearchItem(item, context, volunteerAuthInfo);
            }
        }




        public static SearchResult<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            // basic query that determines the objects that pass the search expression.
            // (if the search expression contains Tags or other 1:many items, we'll need to do more work to filter down to those
            var resultSetQuery = ExtendedQuery(dc, searchExpression);

            var results = Search(dc, searchExpression, sortExpression, startRowIndex, maximumRows, resultSetQuery, SearchItem.Create);
            return results;
        }







        public static EventSession Create(AppDC dc, dynamic data)
        {
            return EventSession.createLock(dc, () =>
            {
                var createdTimestamp = dc.TransactionTimestamp;
                var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

                var eventID = (int)data.eventID;
                var name = (string)data.name;
                var location = (string)data.location;
                var startDate = (DateTime)data.startDate;
                var endDate = (DateTime)data.endDate;

                var newItem = new EventSession(createdTimestamp, teamEPScope, eventID, name, location, startDate, endDate);
                //var newItem = new EventSession(createdTimestamp, teamEPScope, eventID);

                dc.Save(newItem);
                // (have to save to obtain an ID before we can save ExtendedProperties
                Debug.Assert(newItem.ID > 0);

                // After we've got our ID, advance Status to Opened
                //!! newItem.setStatus(dc, ProjectStatus.Submitted);

                newItem.updateData(dc, data);

                return newItem;
            });
        }

        private void updateData(AppDC dc, dynamic data)
        {
            this.Name = (string)data.name;
            this.Location = (string)data.location;
            this.StartDate = (DateTime)data.startDate;
            this.EndDate = (DateTime)data.endDate;
            this.LocationStreetAddress = (string)data.locationStreetAddress;
            this.LocationCity = (string)data.locationCity;
            this.LocationState = (string)data.locationState;
            this.LocationZipCode = (string)data.locationZipCode;
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

        public static HubResult Delete(AppDC dc, int itemID)
        {
            var deleteItem = dc.EventSessions
                .FirstOrDefault(item => item.ID == itemID);
            Debug.Assert(deleteItem != null);

            if (deleteItem == null)
            {
                return HubResult.NotFound;
            }

            var eventSessionParticipantsQuery = dc.EventParticipants
                .Where(eventParticipant => eventParticipant.EventSessionID == itemID);
            if (eventSessionParticipantsQuery.Any())
            {
                return HubResult.CreateError("Not empty");
            }

            dc.EventSessions.DeleteOnSubmit(deleteItem);

            //!! TODO remove any Tags that have their last reference with this Pipeline
            //!! Should we have an ExtendedObject call to remove all extended properties?
            dc.SubmitChanges();

            var notifyExpression = new NotifyExpression();
            notifyExpression.AddDeletedID(itemID);
            NotifyClients(dc, notifyExpression);

            return HubResult.Success;
        }


        public static HubResult SetCheckInOpen(AppDC dc, int itemID, bool isOpen)
        {
            return WriteLock(dc, itemID, (item, notifyExpression) =>
            {
                item.CheckInOpen = isOpen;

                notifyExpression.AddModifiedID(item.ID);
                return HubResult.Success;
            });
        }

#if false
        public static HubResult SetState(AppDC dc, int itemID, EventSessionState state)
        {
            return WriteLock(dc, itemID, (item, notifyExpression) =>
            {
                item.State = state;

                notifyExpression.AddModifiedID(item.ID);
                return HubResult.Success;
            });
        }
#endif



        public static HubResult GetVolunteerAuthInfo(AppDC dc, int itemID)
        {
            var volunteerAuthInfo = getVolunteerAuthInfo(dc, itemID);
            return HubResult.CreateSuccessData(volunteerAuthInfo);
        }

        public static object getVolunteerAuthInfo(AppDC dc, int itemID)
        {
            Debug.Assert(dc != null);

            return ReadLock(dc, itemID, item =>
            {
                // This verifies the Event Session really exists and we have access

                var authToken = ItemPinAuthTemplate.ExtendOrCreateNew(dc, item, null, null);

                var url = ItemPinAuthTemplate.Instance.GetUrl(authToken, UtilityContext.Current.RequireSslLogin);

                var result = new
                {
                    authCode = authToken.AuthCodeValue,
                    url = url.ToString()
                };

                return result;
            });
        }





        public static HubResult ModifyMyFavorite(AppDC dc, int itemID, bool isFavorite)
        {
            return WriteLock(dc, itemID, (item, notifyExpression) =>
            {
                item.ModifyMyFavorite(dc, isFavorite);

                notifyExpression.AddModifiedID(item.ID);
                return HubResult.Success;
            });
        }

        public static HubResult ModifyMyTag(AppDC dc, int itemID, string tagName, bool isAssigned)
        {
            return WriteLock(dc, itemID, (item, notifyExpression) =>
            {
                item.ModifyMyTag(dc, EPCategory.UserAssigned, tagName, isAssigned);

                notifyExpression.AddModifiedID(item.ID);
                return HubResult.Success;
            });
        }

        public static HubResult ModifyTag(AppDC dc, int itemID, string tagName, bool isAssigned)
        {
            return WriteLock(dc, itemID, (item, notifyExpression) =>
            {
                item.ModifyTeamTag(dc, EPCategory.UserAssigned, tagName, isAssigned);

                notifyExpression.AddModifiedID(item.ID);
                return HubResult.Success;
            });
        }







#if false
        public static SearchResults<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, uint startRowIndex, uint maximumRows)
        {

            var groupEPScope = dc.TransactionAuthorizedBy.GroupEPScopeOrInvalid;
            var perUserEPScope = dc.TransactionAuthorizedBy.PerUserEPScopeOrInvalid;

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
            var showQuery = Show.Query(dc, searchExpression, sortExpression, 0, int.MaxValue);

            // After lots of trial & error, it seems like the only way to keep this down to a few SQL queries is to 
            // separately grab Tags & Episodes, then join locally 

            var epShowQuery =
                from show in showQuery
                join applicationPerUSerEPTag in ExtendedProperty.QueryAssignedTags<Show>(dc, EPCategory.UserAssigned, perUserEPScope) on show.ID equals applicationPerUSerEPTag.ID into applicationPerUserEPTagJoin
                join applicationGroupEPTag in ExtendedProperty.QueryAssignedTags<Show>(dc, EPCategory.UserAssigned, groupEPScope) on show.ID equals applicationGroupEPTag.ID into applicationGroupEPTagJoin

                select new
                {
                    show,
                    applicationPerUserEPTagJoin,
                    applicationGroupEPTagJoin,
                };
            var epShowQuerySelect = epShowQuery
                .Select(epShow => new
                {
                    epShow.show.ID,
                    epShow.show.Name,
                    epShow.show.Overview,
                    epShow.show.PosterS3Path,
                    ApplicationPerUserTagNames = epShow.applicationPerUserEPTagJoin
                        .Select(epTag => epTag.Tag.Name)
                        .ToArray(),
                    ApplicationGroupTagNames = epShow.applicationGroupEPTagJoin
                        .Select(epTag => epTag.Tag.Name)
                        .ToArray(),
                });

            if (searchExpression != null)
            {
                searchExpression.Tags.ForEach(searchTag =>
                {
                    epShowQuerySelect = epShowQuerySelect
                        .Where(epShow =>
                            epShow.ApplicationPerUserTagNames.Any(tagName => tagName == searchTag) ||
                            epShow.ApplicationGroupTagNames.Any(tagName => tagName == searchTag)
                        );
                });
            }

            var epShowQuerySelectLimited = epShowQuerySelect
                .Skip((int)startRowIndex)
                .Take((int)maximumRows);

            var epShowQuerySelectArray = epShowQuerySelectLimited
                .ToArray();

            if (searchExpression.ContentDetail != SearchExpression.Detail.High)
            {
                // Don't bother querying for this extra detail
                epShowQuerySelectLimited = epShowQuerySelectLimited
                    .Where(showItem => false);
            }

            // Augment each show with information about available Episodes
            var showEpisodesQuery =
                from epShow in epShowQuerySelectLimited
                join episode in Episode.Query(dc) on epShow.ID equals episode.ShowID into episodeGroup
                select new
                {
                    ShowID = epShow.ID,
                    episodeGroup,
                };
            var showEpisodesQuerySelectArray = showEpisodesQuery
                .Select(showEpisode => new
                {
                    showEpisode.ShowID,

                    EpisodesReleaseOrderDescending = showEpisode.episodeGroup
                        .Select(episode => new
                        {
                            episode.ID,
                            episode.SeasonNumber,
                            episode.EpisodeNumber,
                            episode.ReleasedTimestamp,
                        })
                        .OrderByDescending(episode => episode.SeasonNumber)
                        .ThenByDescending(episode => episode.EpisodeNumber)
                        .ThenByDescending(episode => episode.ReleasedTimestamp)
                        .ToArray(),
                })
                .ToArray();


            // Augment each show with information about available Episodes
            var showVideosQuery =
                from epShow in epShowQuerySelectLimited
                join video in Video.Query(dc) on epShow.ID equals video.ShowID into videoGroup
                select new
                {
                    ShowID = epShow.ID,
                    videoGroup,
                };
            var showVideosQuerySelectArray = showVideosQuery
                .Select(showVideo => new
                {
                    showVideo.ShowID,

                    VideoAvailableEpisodeIDs = showVideo.videoGroup
                        .Where(video => video.EpisodeID.HasValue)
                        .Select(video => video.EpisodeID.Value)
                        .ToArray(),
                })
                .ToArray();


            var currentReleasedTimestamp = DateTime.UtcNow.Date;

            var epShowEpisodesQuery =
                from epShow in epShowQuerySelectArray
                join showEpisodes in showEpisodesQuerySelectArray on epShow.ID equals showEpisodes.ShowID into showEpisodesGroup
                from showEpisodes in showEpisodesGroup.DefaultIfEmpty()
                join showVideos in showVideosQuerySelectArray on epShow.ID equals showVideos.ShowID into showVideosGroup
                from showVideos in showVideosGroup.DefaultIfEmpty()
                select new
                {
                    epShow,

                    SeriesAvailabilityMapData = showEpisodes == null ? null : showEpisodes.EpisodesReleaseOrderDescending
                        .GroupBy(episode => episode.SeasonNumber)
                        .Select(episodeGroup => new
                        {
                            SeasonNumber = episodeGroup.Key,
                            EpisodeIDs = episodeGroup
                                .OrderBy(episode => episode.EpisodeNumber)
                                .Select(episode => episode.ID)
                                .ToArray(),
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
                                .Where(episode => showVideos.VideoAvailableEpisodeIDs.Contains(episode.ID))
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

                    MonthlyReleaseAvailabilityMapData = showEpisodes == null ? null : showEpisodes.EpisodesReleaseOrderDescending
                        .Where(episode => episode.ReleasedTimestamp.HasValue)
                        .GroupBy(episode => new {episode.ReleasedTimestamp.Value.Year, episode.ReleasedTimestamp.Value.Month})
                        .Select(episodeGroup => new
                        {
                            EpisodeIDs = episodeGroup
                                .OrderBy(episode => episode.ReleasedTimestamp.Value.Day)
                                .Select(episode => episode.ID)
                                .ToArray(),
                            YearMonth = episodeGroup.Key,
                            EpisodeReleasedDayOfMonth = episodeGroup
                                .Select(episode => episode.ReleasedTimestamp.Value.Day)
                                .OrderBy(day => day)
                                .ToArray(),
                            VideoAvailableDayOfMonth = episodeGroup
                                .Where(episode => showVideos.VideoAvailableEpisodeIDs.Contains(episode.ID))
                                .Select(episode => episode.ReleasedTimestamp.Value.Day)
                                .OrderBy(day => day)
                                .ToArray(),
                        })
                        .OrderByDescending(episodeGroup => episodeGroup.YearMonth.Year)
                        .ThenByDescending(episodeGroup => episodeGroup.YearMonth.Month)
                        .ToArray(),

                    VideoAvailabilityData = showVideos == null ? null : showVideos.VideoAvailableEpisodeIDs,
                };

            var result = new SearchResults<SearchItem>()
            {
                items = epShowEpisodesQuery
                    .Select(epShowEpisodesGroup => new SearchItem()
                    {
                        id = epShowEpisodesGroup.epShow.ID,

                        name = epShowEpisodesGroup.epShow.Name,
                        overview = epShowEpisodesGroup.epShow.Overview,
                        //!! posterUrl =    new Uri(@"http://thetvdb.com/banners/" + epShowEpisodesGroup.epShow.PosterS3Path),

                        // https://s3.amazonaws.com/lab-static.gopvr.com/tvdb-S-77623-P-posters/77623-3.jpg
                        posterUrl = string.IsNullOrEmpty(epShowEpisodesGroup.epShow.PosterS3Path) ? null :
                            new Uri(@"https://s3.amazonaws.com/lab-static.gopvr.com/" + epShowEpisodesGroup.epShow.PosterS3Path),

                        //userTags = epShowEpisodesGroup.epShow.UserTagNames,

                        isPerUserFavorite = epShowEpisodesGroup.epShow.ApplicationPerUserTagNames
                            .Select(epTag => epTag)
                            .FirstOrDefault(tag => tag == Show.Favorite_PerUser_TagName) != null,
                        isGroupAutomaticDownload = epShowEpisodesGroup.epShow.ApplicationGroupTagNames
                            .Select(epTag => epTag)
                            .FirstOrDefault(tag => tag == Show.AutomaticDownload_Group_TagName) != null,

                        seriesAvailabilityMap = epShowEpisodesGroup.SeriesAvailabilityMapData == null ? null : epShowEpisodesGroup.SeriesAvailabilityMapData
                            .Where(episode => episode.SeasonNumber > 0)
                            .Select(seasonMapData => new
                            {
                                seasonNumber = seasonMapData.SeasonNumber,
                                episodeIDs = seasonMapData.EpisodeIDs,
                                map = Episode.GetEpisodeAvailabilityMap(seasonMapData.KnownEpisodeNumbers, seasonMapData.VideoAvailableEpisodeNumbers, seasonMapData.UnReleasedEpisodeNumbers)
                            })
                            .ToArray(),

                        miniSeriesAvailabilityMap = epShowEpisodesGroup.SeriesAvailabilityMapData == null ? null : epShowEpisodesGroup.SeriesAvailabilityMapData
                            .Where(seasonMapData => seasonMapData.SeasonNumber == 0)
                            .Select(seasonMapData => new
                            {
                                episodeIDs = seasonMapData.EpisodeIDs,
                                map = Episode.GetEpisodeAvailabilityMap(seasonMapData.KnownEpisodeNumbers, seasonMapData.VideoAvailableEpisodeNumbers, seasonMapData.UnReleasedEpisodeNumbers)
                            })
                            .ToArray(),

                        monthlyAvailabilityMap = epShowEpisodesGroup.MonthlyReleaseAvailabilityMapData == null ? null : epShowEpisodesGroup.MonthlyReleaseAvailabilityMapData
                            .Select(monthlySeasonMapData => new
                            {
                                episodeIDs = monthlySeasonMapData.EpisodeIDs,
                                year = monthlySeasonMapData.YearMonth.Year,
                                month = monthlySeasonMapData.YearMonth.Month,
                                map = Episode.GetWeeknightAvailabilityMap(monthlySeasonMapData.YearMonth.Year, monthlySeasonMapData.YearMonth.Month, monthlySeasonMapData.EpisodeReleasedDayOfMonth, monthlySeasonMapData.VideoAvailableDayOfMonth),
                            })
                            .ToArray(),
                    })
                    .ToArray(),
                totalCount = epShowQuerySelect.Count(),
                deletedIDs = searchExpression
                    .DeletedIDs
                    .ToArray(),
            };

            return result;
        }
#endif


#if false
        protected static Show Get(AppDC dc, string showName, Func<Show> creationCallback)
        {
            var existingShow = Show.Find(dc, showName);
            if (existingShow != null)
            {
                return existingShow;
            }

            var newShow = creationCallback();

            //!! temp - see if our carry forward data file knows about this show as an alias.
            var carryForwardStandardName = ShowNameAlias.carryForwardShowNameAlias.getRecognizedShowName(showName);
            if (!string.IsNullOrEmpty(carryForwardStandardName))
            {
                newShow.Name = carryForwardStandardName;
                ShowNameAlias.carryForwardShowNameAlias.getCarryForwardAliases(carryForwardStandardName)
                    .ForEach(alias => ShowNameAlias.Add(dc, newShow, alias));
            }

            return newShow;
        }
#endif

#if false
        public static Show FindOrCreate(AppDC dc, string showName)
        {
            return FindOrCreate(dc, showName, null);
        }

        public static Show FindOrCreate(AppDC dc, string showName, dynamic data)
        {
            int? theTVDBID = null;
            if (data != null)
            {
                theTVDBID = data.seriesid;
            }

            Show existingShow = null;

            if (theTVDBID.HasValue)
            {
                existingShow = Show.FindByTheTVDBID(dc, theTVDBID.Value);
            }
            else
            {
                existingShow = Show.Find(dc, showName);
            }
            if (existingShow != null)
            {
                return existingShow;
            }

            var authorizedBy = dc.TransactionAuthorizedBy;

            EPScope epScope;
            if (authorizedBy.IsSystem)
            {
                epScope = EPScope.Global;
            }
            else
            {
                epScope = authorizedBy.TeamEPScopeOrThrow;
            }

            var newShow = new Show(dc.TransactionTimestamp, epScope, showName, theTVDBID);
            Debug.Assert(newShow != null);
            dc.Save((Show)newShow);

            var notifyExpression = new SearchExpression();
            notifyExpression.AddModifiedID(newShow.ID);
            NotifyClients(dc, notifyExpression);

            //!! temp - see if our carry forward data file knows about this show as an alias.
            var carryForwardStandardName = ShowNameAlias.carryForwardShowNameAlias.getRecognizedShowName(showName);
            if (!string.IsNullOrEmpty(carryForwardStandardName))
            {
                newShow.Name = carryForwardStandardName;
                ShowNameAlias.carryForwardShowNameAlias.getCarryForwardAliases(carryForwardStandardName)
                    .ForEach(alias => ShowNameAlias.Add(dc, newShow, alias));
            }

            return newShow;
        }
#endif


        private static readonly IEnumerable<string> volunteerRoles = new string[] { AppRole.EventSessionVolunteer.ToString() };

#if false
        // Used to create a "EventSession Identity" - for a Volunteer 
        public static IdentityData CreateIdentityDataXXX(AppDC dc, int itemID)
        {
            var eventSession = EventSession.FindByID(dc, itemID);

            if (eventSession == null)
            {
                return null;
            }

            // ** Create our identity ... A EventSessionVolunteer role able to access one EventSession!

            var itemEPScope = eventSession.ItemEPScope;
            Debug.Assert(itemEPScope.ScopeType == ExtendedPropertyScopeType.TenantGroupID);
            var tenantGroupID = itemEPScope.ID;
            var timeZoneInfo = TenantGroup.GetCachedTimeZoneInfo(tenantGroupID);
            IdentityData identityData = IdentityData.Create(itemEPScope, tenantGroupID, eventSession.Name, null, volunteerRoles, null, timeZoneInfo);
            return identityData;
        }
#endif



        public static EventSession FindByID(AppDC dc, int? itemID)
        {
            if (itemID.HasValue)
            {
                return FindByID(dc, itemID.Value);
            }

            return null;
        }

        public static EventSession FindByID(AppDC dc, int itemID)
        {
            var result = EventSession.Query(dc)
                .FirstOrDefault<EventSession>(item => item.ID == itemID);
            return result;
        }

        // probably need to use ShowNameAlias.FindShowOrCreateUnmatched() instead
        internal static EventSession FindByName(AppDC dc, string itemName)
        {
            var existingItem = EventSession.Query(dc)
                .Where(item => item.Name == itemName)
                .FirstOrDefault();

            return existingItem;
        }





        private static HubResult CreateLock(AppDC dc, Func<EventSession> createHandler)
        {
            var newCase = createLock(dc, createHandler);
            if (newCase != null)
            {
                return HubResult.CreateSuccessData(new { id = newCase.ID });
            }

            return HubResult.Error;
        }

        private static EventSession createLock(AppDC dc, Func<EventSession> createHandler)
        {
            return createLock(dc, NotifyClients, createHandler);
        }

        internal static T ReadLock<T>(AppDC dc, int itemID, Func<EventSession, T> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult ReadLock(AppDC dc, int itemID, Func<EventSession, HubResult> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult WriteLock(AppDC dc, int itemID, Func<EventSession, NotifyExpression, HubResult> writeHandler)
        {
            return WriteLock(dc, itemID, FindByID, NotifyClients, writeHandler);
        }






        internal static void NotifyClients(AppDC dc, NotifyExpression notifyExpression)
        {
            NotifyClients("siteHub", notifyExpression,
                searchExpression => EventSession.Search(dc, searchExpression, null, 0, int.MaxValue),
                (hubClients, notificationItem) => hubClients.All.updateEventSessions(notificationItem));
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}