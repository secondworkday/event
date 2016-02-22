using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using MS.Utility;
using MS.TemplateReports;

using MS.WebUtility;

namespace App.Library
{

    public partial class ParticipantGroup
    {
        partial void OnLoaded()
        {
            // (ensure DB timestamps are correctly marked as UTC.)
            this._CreatedTimestamp = DateTime.SpecifyKind(this.CreatedTimestamp, DateTimeKind.Utc);
            this._LastModifiedTimestamp = DateTime.SpecifyKind(this.LastModifiedTimestamp, DateTimeKind.Utc);
        }
    }
    public partial class ParticipantGroup : ExtendedObject<ParticipantGroup>, IEPScopeObject
    {
        protected override int objectID { get { return this.ID; } }
        protected override ExtendedPropertyScopeType objectScopeType { get { return this.ScopeType; } }
        protected override int? objectScopeID { get { return this.ScopeID; } }


        // Global Scope shows
        protected ParticipantGroup(DateTime createdTimestamp, string name)
            : this()
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            this.ScopeType = ExtendedPropertyScopeType.Global;
            this.ScopeID = null;

            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;

            this.Name = name;
        }

        protected ParticipantGroup(DateTime createdTimestamp, EPScope epScope, string name)
            : this()
        {
            this.CreatedTimestamp = createdTimestamp;
            this.ScopeType = epScope.ScopeType;
            this.ScopeID = epScope.ID;

            this.Name = name;
        }

        public static ParticipantGroup GenerateRandom(AppDC dc)
        {
            JToken data = new
            {
                name = "Eastside Elementary"
            }
            .ToJson().FromJson() as JToken;

            var result = ParticipantGroup.createLock(dc, () =>
            {
                var newParticipantGroup = ParticipantGroup.Create(dc, data);
                return newParticipantGroup;
            });

            return result;
        }

        public static ParticipantGroup Create(AppDC dc, JToken data)
        {
            return ParticipantGroup.exCreateLock(dc, data, () =>
            {
                var createdTimestamp = dc.TransactionTimestamp;
                var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

                var name = data.Value<string>("name");

                var newItem = new ParticipantGroup(createdTimestamp, teamEPScope, name);

          // optional
          var badgeName = data.Value<string>("badgeName");
                newItem.BadgeName = badgeName;

                var contactName = data.Value<string>("contactName");
                newItem.ContactName = contactName;
                var overview = data.Value<string>("overview");
                newItem.Overview = overview;

                dc.Save(newItem);
                Debug.Assert(newItem.ID > 0);

                var primaryPhoneNumber = data.Value<string>("primaryPhoneNumber");
                if (!string.IsNullOrEmpty(primaryPhoneNumber))
                {
                    newItem.SetContactPhoneNumber(dc, primaryPhoneNumber);
                }

                var primaryEmail = data.GetMailAddress("primaryEmail");
                if (primaryEmail != null)
                {
                    newItem.AssignMailAddress(dc, primaryEmail);
                }

                return newItem;
            });
        }

        private void updateData(AppDC dc, dynamic data)
        {
            this.Name = (string)data.name;
            this.ContactName = (string)data.contactName;
            this.Overview = (string)data.overview;
            this.SetNotes(dc, (string)data.notes);
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
            var deleteItem = dc.ParticipantGroups
                      .FirstOrDefault(item => item.ID == itemID);

            if (deleteItem == null)
            {
                return HubResult.CreateError("Not found");
            }

            dc.Participants.DeleteAllOnSubmit(deleteItem.Participants);
            dc.ParticipantGroups.DeleteOnSubmit(deleteItem);

            //!! TODO remove any Tags that have their last reference with this Pipeline
            //!! Should we have an ExtendedObject call to remove all extended properties?
            dc.SubmitChanges();

            var notifyExpression = new NotifyExpression();
            notifyExpression.AddDeletedID(itemID);
            NotifyClients(dc, notifyExpression);

            return HubResult.Success;
        }






        public static HubResult Parse(AppDC dc, int eventID, string parseData)
        {
            BulkUpload.ColumnHandler[] availableColumnHandlers = new[]
            {
                new BulkUpload.ColumnHandler("name", BulkUpload.ColumnOptions.Required, "school"),
                new BulkUpload.ColumnHandler("contactName", BulkUpload.ColumnOptions.Optional, "counselor", "contact name"),
                new BulkUpload.ColumnHandler("primaryPhoneNumber", BulkUpload.ColumnOptions.Optional, "phone"),
                new BulkUpload.ColumnHandler("primaryEmail", BulkUpload.ColumnOptions.Optional, "email"),
            };

            return BulkUpload.Parse(parseData, availableColumnHandlers);
        }



        public static HubResult Upload(AppDC dc, int eventID, JToken uploadData)
        {
            // take a submit lock
            // go through each EventParticipant
            // add them to the table
            // return CRUD results

            var hubResult = dc.SubmitLock<HubResult>(() =>
            {
                return Upload<ParticipantGroup>(dc, uploadData, itemData =>
                {
                    var eventParticipant = ParticipantGroup.Create(dc, itemData);
                    if (eventParticipant != null)
                    {
                        return (int?)eventParticipant.ID;
                    }
                    else
                    {
                        return (int?)null;
                    }
                });
            });

            return hubResult;
        }











        public static List<int> GetParticipantsYYY(AppDC dc, int ParticipantGroupID)
        {
            var myQuery = from epParticipantGroup in ExtendedQuery(dc)
                          join epParticipant in Participant.Query(dc) on epParticipantGroup.item.ID equals ParticipantGroupID
                          select new { epParticipant };

            var exResult = myQuery
                .Where(exItem => exItem.epParticipant.ParticipantGroupID == ParticipantGroupID);

            List<int> participantIDs = new List<int>();

            foreach (var p in exResult)
            {
                participantIDs.Add(p.epParticipant.ID);
            }

            return participantIDs;
        }

        private static Func<IQueryable<ParticipantGroup>, string, IQueryable<ParticipantGroup>> termFilter = (query, searchTermLower) =>
        {
            return query.Where(item =>
                item.Name.Contains(searchTermLower) ||
                item.ContactName.Contains(searchTermLower));
        };


        private static IQueryable<ParticipantGroup> query(AppDC dc)
        {
            var result = dc.ParticipantGroups
                .Select(item => item);
            return result;
        }

        public static IQueryable<ParticipantGroup> Query(AppDC dc)
        {
            Debug.Assert(dc.TransactionAuthorizedBy != null);

            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrInvalid;

            var result = query(dc)
                // http://stackoverflow.com/questions/586097/compare-nullable-types-in-linq-to-sql
                .Where(show => show.ScopeType == ExtendedPropertyScopeType.Global ||
                    show.ScopeType == teamEPScope.ScopeType && object.Equals(show.ScopeID, teamEPScope.ID));

            return result;
        }

        public static IQueryable<ParticipantGroup> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression, termFilter);

            return query;
        }

        public static IQueryable<ParticipantGroup> Query(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
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


        protected static IQueryable<ExtendedItem<ParticipantGroup>> ExtendedQuery(AppDC dc)
        {
            return ExtendedQuery(dc, Query(dc));
        }

        public static IQueryable<ExtendedItem<ParticipantGroup>> ExtendedQuery(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc, searchExpression);
            var exQuery = ExtendedQuery(dc, query);
            return exQuery;
        }

        public static IQueryable<ExtendedItem<ParticipantGroup>> ExtendedQuery(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var epQuery = ExtendedQuery(dc, searchExpression);
            epQuery = epQuery.SortBy(sortExpression, startRowIndex, maximumRows);
            return epQuery;
        }



        public static void GetExportRows(HttpResponse response, AppDC dc, SearchExpression searchExpression)
        {
            var epQuery =
                from exParticipantGroup in ParticipantGroup.ExtendedQuery(dc, searchExpression)

                    // Join with our 1:many 
            join tags in QueryGlobalEPTags2(dc, EPCategory.UserAssigned) on exParticipantGroup.itemID equals tags.TargetID into tagsGroup

                join eventParticipant in EventParticipant.Query(dc) on exParticipantGroup.itemID equals eventParticipant.ParticipantID into eventParticipantsGroup

            //join epGlobalSystemRoleTags in User.QueryGlobalEPTags2(dc, EPCategory.SystemRoleCategory) on exParticipantGroup.itemID equals epGlobalSystemRoleTags.TargetID into epGlobalSystemRoleTagsGroup
            //join epTeamSystemRoleTags in User.QueryItemEPTags2(dc, EPCategory.SystemRoleCategory, teamEPScope) on user.ID equals epTeamSystemRoleTags.TargetID into epTeamSystemRoleTagsGroup
            //join epAppRoleTags in User.QueryItemEPTags2(dc, EPCategory.AppRoleCategory, teamEPScope) on user.ID equals epAppRoleTags.TargetID into epAppRoleTagsGroup

            //join epOptionTags in User.QueryGlobalEPTags2(dc, EPCategory.OptionCategory) on user.ID equals epOptionTags.TargetID into epOptionTagsGroup

            select new
                {
                    exParticipantGroup,
                    participantGroup = exParticipantGroup.item,

                    exParticipantGroup.item.Name,
                //user.DisplayName,
                //user.TimeZoneIndex,
                Email = exParticipantGroup.PrimaryMailAddress.Address,
                    PhoneNumber = exParticipantGroup.PrimaryPhoneNumber.Proffered_CaseSensitive,


                    tags = tagsGroup
                        .Select(mm => mm.Item.Name)
                        .Join(", "),

                    totalParticipants = eventParticipantsGroup.Count(),
                };

            var headerMap = new[]
                {
                new { key = "Name", value = "participantGroup.Name" },
                //!! need to translate term
                new { key = "Total Students", value = "totalParticipants" },
                new { key = "Contact Name", value = "participantGroup.ContactName" },
                new { key = "Email", value = "exParticipantGroup.PrimaryMailAddress.Address" },
                new { key = "Phone", value = "exParticipantGroup.PrimaryPhoneNumber.Proffered_CaseSensitive" },
                new { key = "Tags", value = "tags" },

/*
                new { key = "First Name", value = "participant.FirstName" },
                new { key = "Last Name", value = "participant.LastName" },
                new { key = "Gender", value = "participant.Gender" },

                new { key = "School", value = "participantGroup.Name" },

                new { key = "Grade", value = "eventParticipant.Grade" },
                new { key = "CheckInTimestamp", value = "eventParticipant.CheckInTimestamp" },
                new { key = "CheckOutTimestamp", value = "eventParticipant.CheckOutTimestamp" },
                new { key = "DonationLimit", value = "eventParticipant.DonationLimit" },
                new { key = "DonationAmount", value = "eventParticipant.DonationAmount" },

                new { key = "Event Name", value = "myEvent.Name" },
                new { key = "Session Name", value = "session.Name" },
*/
            }
            .ToDictionary(item => item.key, item => item.value);

            response.SendCsvFileToBrowser("ParticipantGroups.csv", epQuery, headerMap);
        }



        public class SearchItem : ExtendedSearchItem
        {
            [JsonProperty("name")]
            public string Name { get; internal set; }
            [JsonProperty("contactName")]
            public string ContactName { get; internal set; }
            [JsonProperty("badgeName")]
            public string BadgeName { get; internal set; }
            [JsonProperty("overview")]
            public string Overview { get; internal set; }

            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }
            [JsonProperty("lastModifiedTimestamp")]
            public DateTime LastModifiedTimestamp { get; internal set; }

            public SearchItem(ExtendedItem<ParticipantGroup> exItem, SearchItemContext context)
                : base(exItem, context)
            {
                //this.type = exItem.item.Ty
                this.Name = exItem.item.Name;
                this.ContactName = exItem.item.ContactName;
                this.BadgeName = exItem.item.BadgeName;
                this.Overview = exItem.item.Overview;

                this.CreatedTimestamp = exItem.item.CreatedTimestamp;
                this.LastModifiedTimestamp = exItem.item.LastModifiedTimestamp;
            }

            public static SearchItem Create(ExtendedItem<ParticipantGroup> item, SearchItemContext context)
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


        public static ParticipantGroup FindByID(AppDC dc, int showID)
        {
            var result = ParticipantGroup.Query(dc)
                .FirstOrDefault<ParticipantGroup>(show => show.ID == showID);
            return result;
        }

        // probably need to use ShowNameAlias.FindShowOrCreateUnmatched() instead
        internal static ParticipantGroup FindByName(AppDC dc, string name)
        {
            var existingItem = ParticipantGroup.Query(dc)
                .Where(item => item.Name == name)
                .FirstOrDefault();

            return existingItem;
        }





        private static HubResult CreateLock(AppDC dc, Func<ParticipantGroup> createHandler)
        {
            var newCase = createLock(dc, createHandler);
            if (newCase != null)
            {
                return HubResult.CreateSuccessData(new { id = newCase.ID });
            }

            return HubResult.Error;
        }

        private static ParticipantGroup createLock(AppDC dc, Func<ParticipantGroup> createHandler)
        {
            return createLock(dc, NotifyClients, createHandler);
        }

        private static ParticipantGroup exCreateLock(AppDC dc, JToken data, Func<ParticipantGroup> createHandler)
        {
            return exCreateLock(dc, data, NotifyClients, createHandler);
        }

        internal static T ReadLock<T>(AppDC dc, int itemID, Func<ParticipantGroup, T> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult ReadLock(AppDC dc, int itemID, Func<ParticipantGroup, HubResult> readHandler)
        {
            return ReadLock(dc, itemID, FindByID, readHandler);
        }

        internal static HubResult WriteLock(AppDC dc, int itemID, Func<ParticipantGroup, NotifyExpression, HubResult> writeHandler)
        {
            return WriteLock(dc, itemID, FindByID, NotifyClients, writeHandler);
        }






        internal static void NotifyClients(AppDC dc, NotifyExpression notifyExpression)
        {
            var notification = ParticipantGroup.Search(dc, notifyExpression, null, 0, int.MaxValue);
            NotifyClients("siteHub", notifyExpression, notification, (hubClients, notificationItem) => hubClients.All.updateParticipantGroups(notificationItem));
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}