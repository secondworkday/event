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
    public partial class EventParticipant
    {
        partial void OnLoaded()
        {
            // (ensure DB timestamps are correctly marked as UTC.)
            this._CreatedTimestamp = DateTime.SpecifyKind(this.CreatedTimestamp, DateTimeKind.Utc);
            this._LastModifiedTimestamp = DateTime.SpecifyKind(this.LastModifiedTimestamp, DateTimeKind.Utc);

            if (this.CheckInTimestamp.HasValue)
            {
                this._CheckInTimestamp = DateTime.SpecifyKind(this.CheckInTimestamp.Value, DateTimeKind.Utc);
            }
            if (this.CheckOutTimestamp.HasValue)
            {
                this._CheckOutTimestamp = DateTime.SpecifyKind(this.CheckOutTimestamp.Value, DateTimeKind.Utc);
            }
        }
    }
    public partial class EventParticipant : ExtendedObject<EventParticipant>, IEPScopeObject
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return this.ScopeType; } }
        protected override int? objectScopeID { get { return this.ScopeID; } }

        protected EventParticipant(DateTime createdTimestamp, EPScope epScope, int eventID, int participantID, uint? grade)
            : this()
        {
            this.CreatedTimestamp = createdTimestamp;
            this.ScopeType = epScope.ScopeType;
            this.ScopeID = epScope.ID;

            this.EventID = eventID;
            this.ParticipantID = participantID;

            this.Grade = grade;
        }

        public static EventParticipant Create(AppDC dc, JToken data)
        {
            return EventParticipant.createLock(dc, () =>
            {
                var createdTimestamp = dc.TransactionTimestamp;
                var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

                var eventID = data.Value<int>("eventID");
                var participantID = data.Value<int>("participantID");

                var grade = data.Value<uint?>("grade");


                var newItem = new EventParticipant(createdTimestamp, teamEPScope, eventID, participantID, grade);

                // optional parameters
                var dataJToken = data as JToken;
                Debug.Assert(dataJToken != null, "Need to call .ToJson().FromJson()?");

                newItem.EventSessionID = dataJToken.Value<int?>("eventSessionID");

                dc.Save(newItem);
                Debug.Assert(newItem.ID > 0);

                return newItem;
            });
        }


        enum Column
        {
            FirstName,
            LastName,
            Gender,
            School,
            Grade
        };

        public static HubResult Parse(AppDC dc, int eventID, string parseData)
        {
            Debug.Assert(!string.IsNullOrEmpty(parseData));
            if (string.IsNullOrEmpty(parseData))
            {
                return HubResult.NotFound;
            }

            var lines = parseData
                // we want to count line numbers - include empty lines
                .Split(UtilityExtensions.LineTerminators, StringSplitOptions.None);

            Debug.Assert(lines != null);

            bool hasHeaderRow = true;

            // The order 
            var columnOrder = new[] { Column.FirstName, Column.LastName, Column.Gender, Column.Grade, Column.School };

            //!! if we're passed CSV?
            //var rows2 = lines
                //.Skip(hasHeaderRow ? 1 : 0)
                //.Select(line => new { line, lineColumns = line.DecodeCsvLine() })
                //.ToArray();


            var rows = lines
                .Skip(hasHeaderRow ? 1 : 0)
                .Select((line, index) => new { line, lineNumber = index, lineColumns = line.Split('\t') })
                //.Where(lineInfo => lineInfo.lineColumns.Length >= columnOrder.Length)
                .Select(lineInfo =>
                {
                    if (string.IsNullOrWhiteSpace(lineInfo.line))
                    {
                        return new { status = "empty", lineInfo.line, lineInfo.lineNumber } as object;
                    }

                    if (lineInfo.lineColumns.Length == columnOrder.Length)
                    {
                        return new { status = "ok", lineInfo.line, lineInfo.lineNumber,
                            firstName = lineInfo.lineColumns[0],
                            lastName = lineInfo.lineColumns[1],
                            gender = lineInfo.lineColumns[2],
                            grade = lineInfo.lineColumns[3],
                            participantGroupName = lineInfo.lineColumns[4],
                        } as object;
                    }

                    return new { status = "error", lineInfo.line, lineInfo.lineNumber } as object;
                })
                .ToArray();

            return HubResult.CreateSuccessData(rows);

#if false
        var uploadData = {
          participantGroupID: 1,
          eventParticipantsData: [
            { firstName: 'betty', lastName: 'rubbles' },
            { firstName: 'barney', lastName: 'rubble', participantGroupName: "Stella Schola" },
            { firstName: 'fred', lastName: 'flintstone', participantGroupID: 3 }]
        };
#endif
        }


        public static HubResult Upload(AppDC dc, int eventID, JToken uploadData)
        {
            // take a submit lock
            // go through each EventParticipant
            // add them to the table
            // return CRUD results

            var hubResult = dc.SubmitLock<HubResult>(() =>
            {
#if false
                JToken eventParticipantsData = new [] 
                {
                    new { firstName = "pepe" },
                    new { firstName = "frank" },
                }
                .ToJson().FromJson() as JToken;
#endif
                var defaultParticipantGroupID = uploadData.Value<int?>("participantGroupID");
                var defaultParticipantGroup = defaultParticipantGroupID.HasValue ? ParticipantGroup.FindByID(dc, defaultParticipantGroupID.Value) : null;

                var eventParticipants = uploadData["eventParticipantsData"]
                    .Select(eventParticipantData =>
                    {
                        var participantGroupID = eventParticipantData.Value<int?>("participantGroupID");
                        var participantGroupName = eventParticipantData.Value<string>("participantGroupName");

                        var participantGroup =
                            (participantGroupID.HasValue ? ParticipantGroup.FindByID(dc, participantGroupID.Value) : null) ??
                            (!string.IsNullOrEmpty(participantGroupName) ? ParticipantGroup.FindByName(dc, participantGroupName) : null) ??
                            defaultParticipantGroup;

                        Debug.Assert(participantGroup != null);
                        if (participantGroup == null)
                        {
                            //!! should return a reason code here ...
                            return (int?)null;
                        }

                        eventParticipantData["participantGroupID"] = participantGroup.ID;
                        var participant = Participant.Create(dc, eventParticipantData);
                        Debug.Assert(participant != null);
                        if (participant == null)
                        {
                            //!! should return a reason code here ...
                            return (int?)null;
                        }

                        eventParticipantData["eventID"] = eventID;
                        eventParticipantData["participantID"] = participant.ID;
                        var eventParticipant = EventParticipant.Create(dc, eventParticipantData);
                        if (eventParticipant != null)
                        {
                            return (int?)eventParticipant.ID;
                        }
                        else
                        {
                            return (int?) null;
                        }
                    })
                    .ToArray();

                return HubResult.CreateSuccessData(eventParticipants);
            });

            return hubResult;
        }


        private static IQueryable<EventParticipant> query(AppDC dc)
        {
            var result = dc.EventParticipants
                .Select(item => item);
            return result;
        }

        public static IQueryable<EventParticipant> Query(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrInvalid;

            var result = query(dc)
                .FilterBy(teamEPScope);

            return result;
        }

        private static Func<IQueryable<EventParticipant>, string, IQueryable<EventParticipant>> termFilter = (query, searchTermLower) =>
        {
            return query.Where(item => true);
                //item.Grade.HasValue && item.Grade.Value.ToString().Contains(searchTermLower));
        };

        public static IQueryable<EventParticipant> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression, termFilter);
            return query;
        }

        public static IQueryable<EventParticipant> Query(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
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


        public static IQueryable<EventParticipant> QueryByEventID(AppDC dc, int eventID)
        {
            var result = Query(dc)
                .Where(eventParticipant => eventParticipant.EventID == eventID);
            return result;
        }

        public static EventParticipant FindByID(AppDC dc, int itemID)
        {
            var result = EventParticipant.Query(dc)
                .FirstOrDefault(item => item.ID == itemID);
            return result;
        }

        protected static IQueryable<ExtendedItem<EventParticipant>> ExtendedQuery(AppDC dc)
        {
            return ExtendedQuery(dc, Query(dc));
        }

        public static IQueryable<ExtendedItem<EventParticipant>> ExtendedQuery(AppDC dc, SearchExpression searchExpression)
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

        public static IQueryable<ExtendedItem<EventParticipant>> ExtendedQuery(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var epQuery = ExtendedQuery(dc, searchExpression);
            epQuery = epQuery.SortBy(sortExpression, startRowIndex, maximumRows);
            return epQuery;
        }




        public class SearchItem : ExtendedSearchItem
        {
            [JsonProperty("participantID")]
            public int ParticipantID { get; internal set; }
            [JsonProperty("participantGroupID")]
            public int ParticipantGroupID { get; internal set; }
            [JsonProperty("eventID")]
            public int EventID { get; internal set; }

            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }

            [JsonProperty("eventSessionID")]
            public int? EventSessionID { get; internal set; }

            [JsonProperty("firstName")]
            public string FirstName { get; internal set; }
            [JsonProperty("lastName")]
            public string LastName { get; internal set; }
            [JsonProperty("fullName")]
            public string FullName { get; internal set; }

            [JsonProperty("grade")]
            public uint? Grade { get; internal set; }

            [JsonProperty("checkInTimestamp")]
            public DateTime? CheckInTimestamp { get; internal set; }
            [JsonProperty("checkOutTimestamp")]
            public DateTime? CheckOutTimestamp { get; internal set; }

            [JsonProperty("donationLimit")]
            public Decimal? DonationLimit { get; internal set; }
            [JsonProperty("donationAmount")]
            public Decimal? DonationAmount { get; internal set; }

            public SearchItem(ExtendedEventParticipantItem exItem, SearchItemContext context)
                : base(exItem, context)
            {
                this.ParticipantID = exItem.Participant.ID;
                this.ParticipantGroupID = exItem.Participant.ParticipantGroupID;
                this.EventID = exItem.item.EventID;

                this.CreatedTimestamp = exItem.item.CreatedTimestamp;

                this.EventSessionID = exItem.item.EventSessionID;

                this.FirstName = exItem.Participant.FirstName;
                this.LastName = exItem.Participant.LastName;
                this.FullName = exItem.Participant.FullName;

                this.Grade = exItem.ExEventParticipant.item.Grade;

                this.CheckInTimestamp = exItem.item.CheckInTimestamp;
                this.CheckOutTimestamp = exItem.item.CheckOutTimestamp;

                this.DonationLimit = exItem.item.DonationLimit;
                this.DonationAmount = exItem.item.DonationAmount;
            }

            public static SearchItem Create(ExtendedEventParticipantItem item, params SearchItemContext[] searchItemContext)
            {
                var eventParticipantSearchItemContext = searchItemContext[0];
                var userSearchItemContext = searchItemContext[1];


                return new SearchItem(item, eventParticipantSearchItemContext);
            }
        }


        // We need to join EventParticipant with Participant to make sense of things.
        // This structure allows us to roundtrip that through the Search mechanism
        public class ExtendedEventParticipantItem : ExtendedItem<EventParticipant>
        {
            public ExtendedItem<EventParticipant> ExEventParticipant { get; internal set; }
            public Participant Participant { get; internal set; }
        }

        public static SearchResult<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {


            var clientQuery =
                // Note: We don't define a searchExpression termFilter above as we need to do the join first
                from exEventParticipant in EventParticipant.ExtendedQuery(dc, searchExpression)
                join participant in Participant.Query(dc) on exEventParticipant.item.ParticipantID equals participant.ID

                select new ExtendedEventParticipantItem
                {
                    itemID = exEventParticipant.item.ID,
                    item = exEventParticipant.item,

                    ExEventParticipant = exEventParticipant,
                    Participant = participant,
                };


            clientQuery = searchExpression.FilterByTextTerms(clientQuery, (query, searchTermLower) => 
            {
                return query.Where(item => item.Participant.FirstName.Contains(searchTermLower)
                    || item.Participant.LastName.Contains(searchTermLower)
                    || (item.ExEventParticipant.item.Grade.HasValue && item.ExEventParticipant.item.Grade.Value.ToString().Contains(searchTermLower))
                    ); 
            });

            // Simple case - a trivial selector that retrieves the object as an ExtendedItem<T>
            Func<object, Tuple<Type, int>>[] itemSelectors =
            {
                // (Put the 'main' type first ...)
                objectItem => Tuple.Create(typeof(EventParticipant), ((dynamic)objectItem).itemID),
                // (... and extra types after)
                objectItem => Tuple.Create(typeof(Participant), ((dynamic)objectItem).itemID)
            };



            // Simple case - we passed in a single selector, so expect back single item arrays to be mapped
            Func<object, SearchItemContext[], SearchItem> resultMapper = (itemObject, searchItemContexts) =>
            {
                var fda = itemObject as ExtendedEventParticipantItem;

                var asdf = SearchItem.Create(fda, searchItemContexts);
                return asdf;
            };


            //var searchResults = Search(dc, searchExpression, sortExpression, startRowIndex, maximumRows, clientQuery2, itemSelector.ToEnumerable(), SearchItem.Create);
            var searchResults = Search(dc, searchExpression, sortExpression, startRowIndex, maximumRows, clientQuery, itemSelectors, resultMapper);

            //var searchResults = SearchResult<SearchItem>.Create(searchResults.Items, searchResults.totalCount, searchResults.DeletedKeys);
            return searchResults;








#if false
            // basic query that determines the objects that pass the search expression.
            // (if the search expression contains Tags or other 1:many items, we'll need to do more work to filter down to those
            var resultSetQuery = ExtendedQuery(dc, searchExpression);

            //resultSetQuery = resultSetQuery
            //.OrderBy(foo => foo.item.ReleasedTimestamp);

            var results = Search(dc, searchExpression, sortExpression, startRowIndex, maximumRows, resultSetQuery, SearchItem.Create);
            return results;
#endif
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


        private static HubResult CreateLock(AppDC dc, Func<EventParticipant> createHandler)
        {
            var newCase = createLock(dc, createHandler);
            if (newCase != null)
            {
                return HubResult.CreateSuccessData(new { id = newCase.ID });
            }

            return HubResult.Error;
        }

        private static EventParticipant createLock(AppDC dc, Func<EventParticipant> createHandler)
        {
            return CreateLock(dc, NotifyClients, createHandler);
        }

        internal static T ReadLock<T>(AppDC dc, int itemID, Func<EventParticipant, T> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult ReadLock(AppDC dc, int itemID, Func<EventParticipant, HubResult> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult WriteLock(AppDC dc, int itemID, Func<EventParticipant, NotifyExpression, HubResult> writeHandler)
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
            var siteContext = SiteContext.Current;

            var notification = EventParticipant.Search(dc, notifyExpression, null, 0, int.MaxValue);

            var hubClients = siteContext.ConnectionManager.GetHubContext("siteHub").Clients;
            Debug.Assert(hubClients != null);
            hubClients.All.updateEventParticipants(notification);
        }

        public override string ToString()
        {
            return string.Format("EventID: {0}, ParticipantID: {1}",
                /*0*/this.EventID,
                /*1*/this.ParticipantID);
        }
        public static List<int> GetSessionEventParticipants(AppDC dc, int eventSessionID, int ParticipantGroupID)
        {
            var exQuery = from epEventParticipant in ExtendedQuery(dc)
                          join epParticipant in Participant.Query(dc) on epEventParticipant.item.ParticipantID equals epParticipant.ID
                          select new { epEventParticipant, epParticipant };

            var exResult = exQuery
                .Where(exItem => exItem.epEventParticipant.item.EventSessionID == eventSessionID && exItem.epParticipant.ParticipantGroupID == ParticipantGroupID);

            List<int> eventParticipantIDs = new List<int>();

            foreach (var p in exResult)
            {
                eventParticipantIDs.Add(p.epEventParticipant.item.ID);
            }

            return eventParticipantIDs;
        }

        public static ReportGenerator GetReportGenerator(AppDC appDC, string templateName, ReportFormat reportFormat, int itemID)
        {
            var siteContext = SiteContext.Current;

            TagProvider tagProvider = null;
            tagProvider = EventParticipant.createParticipantTagProvider(appDC, itemID);

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
            var exQuery = from epEventParticipant in ExtendedQuery(appDC)
                          join epParticipant in Participant.Query(appDC) on epEventParticipant.item.ParticipantID equals epParticipant.ID
                          join epParticipantGroup in ParticipantGroup.Query(appDC) on epParticipant.ParticipantGroupID equals epParticipantGroup.ID
                          join epSession in EventSession.Query(appDC) on epEventParticipant.item.EventSessionID equals epSession.ID
                          select new { epEventParticipant, epParticipant, epParticipantGroup, epSession };

            var exResult = exQuery
                .Where(exItem => exItem.epEventParticipant.item.ID == itemID)
                .FirstOrDefault();

            //var eventParticipant = exResult.epEventParticipant;

            IEnumerable<ProviderTag> tags = new ProviderTag[]
            {
                StringProviderTag.Create("FirstName", exResult.epParticipant.FirstName),
                StringProviderTag.Create("LastName", exResult.epParticipant.LastName),
                StringProviderTag.Create("FullName", exResult.epParticipant.FullName),

                StringProviderTag.Create("SchoolName", exResult.epParticipantGroup.Name),

                DateTimeProviderTag.Create("EventDate", exResult.epSession.StartDate),
                DateTimeProviderTag.Create("EventTime", exResult.epSession.StartDate),
                StringProviderTag.Create("EventLocation", exResult.epSession.Location),
                
                StringProviderTag.Create("Address", "16722 NE 116th Street, Redmond WA 98052"),

                StringProviderTag.Create("SchoolCounselorName", "Lorrie Smith"),

                // Check in is between «CheckinTimeStart» and «CheckinTimeEnd». All shopping must be completed by «ShoppingTimeEnd».
                DateTimeProviderTag.Create("CheckinTimeStart", exResult.epSession.StartDate.AddMinutes(-30)),
                DateTimeProviderTag.Create("CheckinTimeEnd", exResult.epSession.StartDate.AddHours(1)),
                DateTimeProviderTag.Create("ShoppingTimeEnd", exResult.epSession.StartDate.AddHours(2)),

            };

            return TagProvider.Create(tags);
        }

    }
}