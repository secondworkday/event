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

    public partial class Event
    {
        partial void OnLoaded()
        {
            // (ensure DB timestamps are correctly marked as UTC.)
            this._CreatedTimestamp = DateTime.SpecifyKind(this.CreatedTimestamp, DateTimeKind.Utc);
            this._LastModifiedTimestamp = DateTime.SpecifyKind(this.LastModifiedTimestamp, DateTimeKind.Utc);
        }
    }
    public partial class Event : ExtendedObject<Event>, IEPScopeObject
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return this.ScopeType; } }
        protected override int? objectScopeID { get { return this.ScopeID; } }


        protected Event(DateTime createdTimestamp, EPScope epScope, string name)
            : this()
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;

            this.ScopeType = epScope.ScopeType;
            this.ScopeID = epScope.ID;

            this.Name = name;
        }



        private static Func<IQueryable<Event>, string, IQueryable<Event>> termFilter = (query, searchTermLower) =>
        {
            return query.Where(item =>
                item.Name.Contains(searchTermLower));
        };


        private static IQueryable<Event> query(AppDC dc)
        {
            var result = dc.Events
                .Select(item => item);
            return result;
        }

        public static IQueryable<Event> Query(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrInvalid;

            var result = query(dc)
                // http://stackoverflow.com/questions/586097/compare-nullable-types-in-linq-to-sql
                .Where(show => show.ScopeType == ExtendedPropertyScopeType.Global ||
                    show.ScopeType == teamEPScope.ScopeType && object.Equals(show.ScopeID, teamEPScope.ID));

            return result;
        }

        public static IQueryable<Event> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression, termFilter);
            return query;
        }

        public static IQueryable<Event> Query(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
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

#if false
        public static IQueryable<Show> QueryUpcomingShowsAvailable(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var result = Query(dc)
                .Where(show => show.Name == "scubba");

            return result;
        }
#endif


        protected static IQueryable<ExtendedItem<Event>> ExtendedQuery(AppDC dc)
        {
            return ExtendedQuery(dc, Query(dc));
        }

        public static IQueryable<ExtendedItem<Event>> ExtendedQuery(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            var exQuery = ExtendedQuery(dc, query);
            return exQuery;
        }

        public static IQueryable<ExtendedItem<Event>> ExtendedQuery(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var epQuery = ExtendedQuery(dc, searchExpression);
            epQuery = epQuery.SortBy(sortExpression, startRowIndex, maximumRows);
            return epQuery;
        }



        public class SearchItem : ExtendedSearchItem
        {
            //public string type { get; internal set; }
            public string name { get; internal set; }
            public string overview { get; internal set; }

            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }
            [JsonProperty("lastModifiedTimestamp")]
            public DateTime LastModifiedTimestamp { get; internal set; }

            public SearchItem(ExtendedItem<Event> exItem, SearchItemContext context)
                : base(exItem, context)
            {
                //this.type = exItem.item.Ty
                this.name = exItem.item.Name;
                this.overview = exItem.item.Overview;

                this.CreatedTimestamp = exItem.item.CreatedTimestamp;
                this.LastModifiedTimestamp = exItem.item.LastModifiedTimestamp;
            }

            public static SearchItem Create(ExtendedItem<Event> item, SearchItemContext context)
            {
                var optionTags = context.TeamEPScopeItemTagCategoryGroups
                    .Where(group => group.Key == EPCategory.OptionCategory)
                    .SelectMany(groupElement => groupElement
                        .Select(epTagsItem => epTagsItem.Tag.Name))
                    .ToArray();

                return new SearchItem(item, context);
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






        public static Event GenerateRandom(AppDC dc)
        {
            var random = RandomProvider.GetThreadRandom();

            var sponsor = new[]
            {
                "Fred Meyer",
                "Walmart",
                "Target",
                "Costco",
            }.ChooseRandom();


            var data = new
            {
                name = "Demo Event " + DateTime.Now.ToString(),
                sponsor = sponsor,
            };

            var randomEvent = Event.createLock(dc, () =>
            {
                var newItem = Event.Create(dc, data);
                Debug.Assert(newItem != null && newItem.ID > 0);

                return newItem;
            });
            Debug.Assert(randomEvent != null);


            // Ensure we've got some participant groups
            var demoParticipantGroupNames = new []
            {
                "BEST High School",
                "Eastlake High School",
                "International Community School",
                "Lake Washington High School",
                "Redmond High School",
                "STEM High School",

                "Stella Schola",
                "International Community School",
                "Northstar Middle School",
                "Inglewood Middle School",
                "Kirkland Middle School",
                "Tyee Middle School",
                "Evergreen Middle School",
                "Odle Middle School",

                "Alcott Elementary",
                "Audubon Elementary",
                "Dickinson Elementary",
                "Einstein Elementary",
                "Explorer Community School",
                "Redmond Elementary",
                "Rockwell Elementary",
                "Rush Elementary",
            };

            var demoParticipantGroupBadgeNames = new[]
            {   
                "Eagles",
                "Tigers",
                "Panthers",
                "Bulldogs",
                "Wildcats",
                "Warriors",
                "Lions",
                "Cougars",
                "Indians",
                "Knights",
                "Mustangs",
                "Falcons",
                "Trojans",
                "Vikings",
                "Rams",
                "Cardinals",
                "Raiders",
                "Spartans",
                "Patriots",
                "Pirates",
                "Hornets",
                "Crusaders",
                "Bears",
                "Hawks",
                "Rebels",
                "Bobcats",
                "Blue Devils",
                "Wolverines",
                "Wolves",
                "Huskies",
                "Jaguars",
                "Titans",
                "Chargers",
                "Dragons",
                "Saints",
                "Braves",
                "Red Devils",
                "Rockets",
                "Pioneers"
            };

            int numParticipantGroups = 6 + random.Next(8);
            var participantGroupNames = demoParticipantGroupNames.ChooseMultiple(numParticipantGroups);
            var participantGroupBadgeNames = demoParticipantGroupBadgeNames.ChooseMultiple(numParticipantGroups)
                .ToArray();

            var participantGroups = participantGroupNames
                .Select((participantGroupName, index) =>
                    ParticipantGroup.FindByName(dc, participantGroupName) ?? ParticipantGroup.Create(dc, new { name = participantGroupName, badgeName = participantGroupBadgeNames[index] })
                )
                .ToArray();

            var participantGroupIDs = participantGroups
                .Select(participantGroup => participantGroup.ID)
                .ToArray();


            // Ensure we've got enough participants, and stick each one into a participant group
            int numParticipants = 50 + random.Next(350);
            //!!
            numParticipants = 5;
            var participants = Enumerable.Range(0, numParticipants)
                .Select(index => Participant.GenerateRandom(dc, participantGroupIDs))
                .ToArray();


            // Now Create a random number of Event Sessions
            int numSessions = 2 + random.Next(3);

            // choose a date for our first EventSession
            var firstSessionDate = DateTime.UtcNow.AddDays(15 + random.Next(30)).Date;


            for (int sessionIndex = 0; sessionIndex < numSessions; sessionIndex++)
            {
                var sessionStartDate = firstSessionDate.AddDays(sessionIndex);
                if (sessionStartDate.DayOfWeek == DayOfWeek.Saturday || sessionStartDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    sessionStartDate += TimeSpan.FromHours(new [] { 13, 14, 15 }.ChooseRandom());
                } else {
                    sessionStartDate += TimeSpan.FromHours(new [] { 18.5, 19, 19.5, 20 }.ChooseRandom());
                }
                var sessionEndDate = sessionStartDate.AddHours(new [] { 2, 2.5, 3 }.ChooseRandom());

                var sessionData = new 
                {
                    eventID = randomEvent.ID,
                    name = "Session " + (sessionIndex + 1).ToString(),
                    location = sponsor,
                    startDate = sessionStartDate,
                    endDate = sessionEndDate,
                };

                var eventSession = EventSession.Create(dc, sessionData);
                Debug.Assert(eventSession != null);

                //!! add in participants to the session!

            }

            return randomEvent;
        }

        public static Event Create(AppDC dc, dynamic data)
        {
            return Event.createLock(dc, () =>
            {
                var createdTimestamp = dc.TransactionTimestamp;
                var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

                var name = (string)data.name;

                var newItem = new Event(createdTimestamp, teamEPScope, name);
                // optional
                var sponsor = (string)data.sponsor;
                newItem.Sponsor = sponsor;

                dc.Save(newItem);
                // (have to save to obtain an ID before we can save ExtendedProperties
                Debug.Assert(newItem.ID > 0);

                // After we've got our ID, advance Status to Opened
                //!! newItem.setStatus(dc, ProjectStatus.Submitted);

                //!! newItem.updateData(dc, data);

                return newItem;
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


        public static Event FindByID(AppDC dc, int showID)
        {
            var result = Event.Query(dc)
                .FirstOrDefault<Event>(show => show.ID == showID);
            return result;
        }

        // probably need to use ShowNameAlias.FindShowOrCreateUnmatched() instead
        internal static Event FindByName(AppDC dc, string showName)
        {
            var existingShow = Event.Query(dc)
                .Where(show => show.Name == showName)
                .FirstOrDefault();

            return existingShow;
        }





        private static HubResult CreateLock(AppDC dc, Func<Event> createHandler)
        {
            var newCase = createLock(dc, createHandler);
            if (newCase != null)
            {
                return HubResult.CreateSuccessData(new { id = newCase.ID });
            }

            return HubResult.Error;
        }

        private static Event createLock(AppDC dc, Func<Event> createHandler)
        {
            return CreateLock(dc, NotifyClients, createHandler);
        }

        internal static T ReadLock<T>(AppDC dc, int itemID, Func<Event, T> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult ReadLock(AppDC dc, int itemID, Func<Event, HubResult> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult WriteLock(AppDC dc, int itemID, Func<Event, NotifyExpression, HubResult> writeHandler)
        {
            return WriteLock(dc, itemID, FindByID, NotifyClients, writeHandler);
        }






        internal static void NotifyClients(AppDC dc, NotifyExpression notifyExpression)
        {
            var siteContext = SiteContext.Current;

            var notification = Event.Search(dc, notifyExpression, null, 0, int.MaxValue);

            var hubClients = siteContext.ConnectionManager.GetHubContext("siteHub").Clients;
            Debug.Assert(hubClients != null);
            hubClients.All.updateShows(notification);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}