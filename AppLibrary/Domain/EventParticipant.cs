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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using MS.Utility;
using MS.TemplateReports;

using MS.WebUtility;

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


                var tagName = data.Value<string>("#");
                if (!string.IsNullOrEmpty(tagName))
                {
                    tagName = tagName.TrimStart('#');
                    newItem.ModifyGlobalTag(dc, EPCategory.UserAssigned, tagName, true);
                }

                var tagNames = data.Value<string[]>("tags");
                if (tagNames != null && tagNames.Any())
                {
                    // newItem.ModifyGlobalTag(dc, EPCategory.UserAssigned, tagName, true);
                }

                var comment = data.Value<string>("comment");
                if (!string.IsNullOrEmpty(comment))
                {
                    newItem.AddComment(dc, EPCategory.UserAssigned, comment);
                }


                return newItem;
            });
        }


        enum ColumnXXX
        {
            FirstName,
            LastName,
            Gender,
            Grade,
            ParticipantGroupName,
        };


        public static HubResult Parse(AppDC dc, int eventID, string parseData)
        {
            var genderNormalizationValues = new[] {
                BulkUpload.NormalizationValue.Create("M", "masculine", "male", "man", "boy"),
                BulkUpload.NormalizationValue.Create("F", "feminine", "female", "woman", "girl"),
                BulkUpload.NormalizationValue.Create(" ", "transgender", "ftm", "mtf")
            };

            BulkUpload.ColumnHandler[] availableColumnHandlers = new[]
            {
                new BulkUpload.ColumnHandler("firstName", BulkUpload.ColumnOptions.Required, "first name", "first"),
                new BulkUpload.ColumnHandler("lastName", BulkUpload.ColumnOptions.Required, "last name", "last"),
                new BulkUpload.ColumnHandler("gender", BulkUpload.ColumnOptions.Required, genderNormalizationValues, "sex"),
                new BulkUpload.ColumnHandler("grade", BulkUpload.ColumnOptions.Optional),
                new BulkUpload.ColumnHandler("participantGroupName", BulkUpload.ColumnOptions.Optional, "school"),
            };

            return BulkUpload.Parse(parseData, availableColumnHandlers);
        }





    public static int? CreateParticipantAndEventParticipant(AppDC dc, int eventID, JToken data)
    {
      var defaultParticipantGroupID = data.Value<int?>("participantGroupID");
      var defaultParticipantGroup = defaultParticipantGroupID.HasValue ? ParticipantGroup.FindByID(dc, defaultParticipantGroupID.Value) : null;

      //return CreateParticipantAndEventParticipant(dc, eventID, data, defaultParticipantGroup, null);
      return CreateParticipantAndEventParticipant(dc, eventID, data, null, null);
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

    private void updateData(AppDC dc, dynamic data)
    {
      
      // Update Participant
      Participant.Edit(dc, this.ParticipantID, data);

      // Update EventParticipant
      var eventSessionID = (int?)data.eventSessionID;
      if (eventSessionID.HasValue)
      {
        this.EventSessionID = eventSessionID;
      }
      this.Grade = (uint)data.grade;
    }

    public static HubResult Delete(AppDC dc, int itemID)
    {
      var deleteItem = dc.EventParticipants
          .FirstOrDefault(item => item.ID == itemID);
      Debug.Assert(deleteItem != null);

      if (deleteItem == null)
      {
        return HubResult.NotFound;
      }

      //!! We won't delete Participant
      dc.EventParticipants.DeleteOnSubmit(deleteItem);

      //!! TODO remove any Tags that have their last reference with this Pipeline
      //!! Should we have an ExtendedObject call to remove all extended properties?

      string activityDescription = "Deleted 1 EventParticipant";
      var epScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;
      var activityType = ActivityType.Deleted;
      ActivityItem.Log(dc, epScope, activityType, activityDescription, typeof(EventParticipant), itemID);


      dc.SubmitChanges();

      var notifyExpression = new NotifyExpression();
      notifyExpression.AddDeletedID(itemID);
      NotifyClients(dc, notifyExpression);

      return HubResult.Success;
    }

    public static HubResult Delete(AppDC dc, int[] itemIDs)
    {
        Debug.Assert(itemIDs != null);
        if (itemIDs == null)
        {
            return HubResult.Error;
        }
        if (!itemIDs.Any())
        {
            return HubResult.NotFound;
        }

        dc.SubmitLock(() =>
        {
            var deleteItems = dc.EventParticipants
                .Where(item => itemIDs.Contains(item.ID));

            //!! TODO remove any Tags that have their last reference with this Pipeline
            //!! Should we have an ExtendedObject call to remove all extended properties?


            //!! We won't delete Participant
            dc.EventParticipants.DeleteAllOnSubmit(deleteItems);

            //!! create a bulk delete TAG
            int bulkTagIDThingy = 0;

            string activityDescription = string.Format("Bulk Delete {0} EventParticipant(s)",
                /*0*/ itemIDs.Length);
            var epScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;
            var activityType = ActivityType.BulkDeleted;
            ActivityItem.Log(dc, epScope, activityType, activityDescription, typeof(EventParticipant), bulkTagIDThingy);
        });

        var notifyExpression = new NotifyExpression();
        itemIDs.ForEach(itemID =>
        {
            notifyExpression.AddDeletedID(itemID);
        });
        NotifyClients(dc, notifyExpression);

        return HubResult.Success;
    }







    public static HubResult Upload(AppDC dc, int eventID, JToken uploadData)
    {
        // take a submit lock
        // go through each EventParticipant
        // add them to the table
        // return CRUD results

        var hubResult = dc.SubmitLock<HubResult>(() =>
        {
            var defaultParticipantGroupID = uploadData.Value<int?>("participantGroupID");
            var defaultParticipantGroup = defaultParticipantGroupID.HasValue ? ParticipantGroup.FindByID(dc, defaultParticipantGroupID.Value) : null;

            var defaultEventSessionID = uploadData.Value<int?>("eventSessionID"); 

            var eventParticipants = uploadData["itemsData"]
                .Select(itemData =>
                {
                  return CreateParticipantAndEventParticipant(dc, eventID, itemData, defaultParticipantGroup, defaultEventSessionID);
                })
                .ToArray();


            //!! create a bulk delete TAG
            int bulkTagIDThingy = 0;

            string activityDescription = string.Format("Bulk Create {0} EventParticipant(s)",
                /*0*/ eventParticipants.Length);
            var epScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;
            var activityType = ActivityType.BulkCreated;
            ActivityItem.Log(dc, epScope, activityType, activityDescription, typeof(EventParticipant), bulkTagIDThingy);


            return HubResult.CreateSuccessData(eventParticipants);
        });

        return hubResult;
    }

    private static int? CreateParticipantAndEventParticipant(AppDC dc, int eventID, JToken itemData, ParticipantGroup defaultParticipantGroup, int? defaultEventSessionID)
    {
        var participantGroupID = itemData.Value<int?>("participantGroupID");
        var participantGroupName = itemData.Value<string>("participantGroupName");

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

        itemData["participantGroupID"] = participantGroup.ID;
        var participant = Participant.Create(dc, itemData);
        Debug.Assert(participant != null);
        if (participant == null)
        {
            //!! should return a reason code here ...
            return (int?)null;
        }

        itemData["eventID"] = eventID;
        itemData["participantID"] = participant.ID;
        if (defaultEventSessionID.HasValue)
        {
            itemData["eventSessionID"] = defaultEventSessionID.Value;
        }
        var eventParticipant = EventParticipant.Create(dc, itemData);
        if (eventParticipant != null)
        {
            return (int?)eventParticipant.ID;
        }
        else
        {
            return (int?)null;
        }
    }

    public static HubResult CheckIn(AppDC dc, int itemID)
    {
        return WriteLock(dc, itemID, (item, notifyExpression) =>
        {
            Debug.Assert(!item.CheckInTimestamp.HasValue);
            item.CheckInTimestamp = dc.TransactionTimestamp;

            notifyExpression.AddModifiedID(item.ID);
            return HubResult.Success;
        });
    }

    public static HubResult UndoCheckIn(AppDC dc, int itemID)
    {
        return WriteLock(dc, itemID, (item, notifyExpression) =>
        {
            Debug.Assert(item.CheckInTimestamp.HasValue);
            item.CheckInTimestamp = null;

            notifyExpression.AddModifiedID(item.ID);
            return HubResult.Success;
        });
    }

    public static HubResult CheckOut(AppDC dc, int itemID)
    {
        return WriteLock(dc, itemID, (item, notifyExpression) =>
        {
            Debug.Assert(item.CheckInTimestamp.HasValue);
            Debug.Assert(!item.CheckOutTimestamp.HasValue);
            item.CheckOutTimestamp = dc.TransactionTimestamp;

            notifyExpression.AddModifiedID(item.ID);
            return HubResult.Success;
        });
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

        public static IQueryable<EventParticipant> Query(AppDC dc, SearchExpression searchExpression)
        {
            var query = Query(dc);
            query = FilterBy(dc, query, searchExpression);

            // support filtering to a specific Event. eg. $event:{eventID}
            query = searchExpression.FilterByAnyNamedStateTerm2(query, "event",
                searchTerm => item => item.EventID.ToString() == searchTerm);

            // support filtering to a specific EventSession. eg. eventSession:{eventSessionID}
            query = searchExpression.FilterByAnyNamedStateTerm2(query, "eventSession",
                searchTerm => item => item.EventSessionID.ToString() == searchTerm || 
                    (!item.EventSessionID.HasValue && string.IsNullOrEmpty(searchTerm)));



            // eg. $notCheckedIn, $checkedIn
            query = searchExpression.FilterByAnyStateTerm2(query,
                searchTerm =>
                {
                    System.Linq.Expressions.Expression<Func<EventParticipant, bool>> notCheckedInPredicate = item => !item.CheckInTimestamp.HasValue;
                    System.Linq.Expressions.Expression<Func<EventParticipant, bool>> checkedInPredicate = item => item.CheckInTimestamp.HasValue && !item.CheckOutTimestamp.HasValue;
                    System.Linq.Expressions.Expression<Func<EventParticipant, bool>> checkedOutPredicate = item => item.CheckOutTimestamp.HasValue;

                    switch (searchTerm)
                    {
                        case "notCheckedIn":
                            return notCheckedInPredicate;
                        case "checkedIn":
                            return checkedInPredicate;
                        case "checkedOut":
                            return checkedOutPredicate;
                        default:
                            Debug.Fail("Unexpected searchTerm: " + searchTerm);
                            return exItem => false;
                    }
                });


            // EventParticipants involve Partipants, ParticipantGroups & EventSessions, all of which can be involved with searches
            // So we widen the query here so we can filter on all those things
            var searchTermQuery = 
                from eventParticipant in query
                // inner join - required
                join participant in Participant.Query(dc) on eventParticipant.ParticipantID equals participant.ID
                join participantGroup in ParticipantGroup.Query(dc) on participant.ParticipantGroupID equals participantGroup.ID
                // outer join - optional
                join session in EventSession.Query(dc) on eventParticipant.EventSessionID equals session.ID into eventParticipantSessionGroup
                from session in eventParticipantSessionGroup.DefaultIfEmpty()
                select new {eventParticipant, participant, participantGroup, session };

            searchTermQuery = searchExpression.FilterByTextTerms(searchTermQuery, (termQuery, searchTermLower) =>
            {
                return termQuery.Where(item =>
                    (item.eventParticipant.Grade.HasValue && item.eventParticipant.Grade.Value.ToString().Contains(searchTermLower)) ||
                    item.participant.FirstName.Contains(searchTermLower) ||
                    item.participant.LastName.Contains(searchTermLower) ||
                    item.participantGroup.Name.Contains(searchTermLower) ||
                    (item.session != null && item.session.Name.Contains(searchTermLower))
                    );
            });

            // support filtering to a specific ParticipantGroup. eg. participantGroup:{participantGroupID}
            searchTermQuery = searchExpression.FilterByAnyNamedStateTerm2(searchTermQuery, "participantGroup",
                searchTerm => item => item.participantGroup.ID.ToString() == searchTerm);


            query = searchTermQuery
                .Select(searchItem => searchItem.eventParticipant);




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


        public static void GetExportRows(HttpResponse response, AppDC dc, SearchExpression searchExpression)
        {
            var query =
                from eventParticipant in EventParticipant.Query(dc, searchExpression)
                join participant in Participant.Query(dc) on eventParticipant.ParticipantID equals participant.ID
                join participantGroup in ParticipantGroup.Query(dc) on participant.ParticipantGroupID equals participantGroup.ID
                join myEvent in Event.Query(dc) on eventParticipant.EventID equals myEvent.ID

                // outer join - optional
                join session in EventSession.Query(dc) on eventParticipant.EventSessionID equals session.ID into eventParticipantSessionGroup
                from session in eventParticipantSessionGroup.DefaultIfEmpty()
                select new {eventParticipant, participant, participantGroup, myEvent, session };

            var headerMap = new[]
            {
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
            }
            .ToDictionary(item => item.key, item => item.value);

            response.SendCsvFileToBrowser("EventParticipants.csv", query, headerMap);
        }


        public class SearchItem : ExtendedSearchItem
        {
            [JsonProperty("participantID")]
            public int ParticipantID { get; internal set; }
            [JsonProperty("eventID")]
            public int EventID { get; internal set; }

            [JsonProperty("createdTimestamp")]
            public DateTime CreatedTimestamp { get; internal set; }

            [JsonProperty("participantGroupID")]
            public int ParticipantGroupID { get; internal set; }
            [JsonProperty("participantGroupName")]
            public string ParticipantGroupName { get; internal set; }

            [JsonProperty("eventSessionID")]
            public int? EventSessionID { get; internal set; }

            [JsonProperty("firstName")]
            public string FirstName { get; internal set; }
            [JsonProperty("lastName")]
            public string LastName { get; internal set; }
            [JsonProperty("fullName")]
            public string FullName { get; internal set; }

            [JsonProperty("gender"), JsonConverter(typeof(StringEnumConverter))]
            public UserGender? Gender { get; internal set; }

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
                this.EventID = exItem.item.EventID;

                this.CreatedTimestamp = exItem.item.CreatedTimestamp;

                Debug.Assert(exItem.Participant.ParticipantGroupID == exItem.ParticipantGroup.ID);
                this.ParticipantGroupID = exItem.Participant.ParticipantGroupID;
                this.ParticipantGroupName = exItem.ParticipantGroup.Name;

                this.EventSessionID = exItem.item.EventSessionID;

                this.FirstName = exItem.Participant.FirstName;
                this.LastName = exItem.Participant.LastName;
                this.FullName = exItem.Participant.FullName;

                this.Gender = exItem.Participant.Gender;

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
            public ParticipantGroup ParticipantGroup { get; internal set; }
        }

        public static SearchResult<SearchItem> Search(AppDC dc, SearchExpression searchExpression, string sortExpression, int startRowIndex, int maximumRows)
        {
            var clientQuery =
                // Note: We don't define a searchExpression termFilter above as we need to do the join first
                from exEventParticipant in EventParticipant.ExtendedQuery(dc, searchExpression)
                join participant in Participant.Query(dc) on exEventParticipant.item.ParticipantID equals participant.ID
                join participantGroup in ParticipantGroup.Query(dc) on participant.ParticipantGroupID equals participantGroup.ID

                select new ExtendedEventParticipantItem
                {
                    itemID = exEventParticipant.item.ID,
                    item = exEventParticipant.item,

                    ExEventParticipant = exEventParticipant,
                    Participant = participant,
                    ParticipantGroup = participantGroup,
                };


            //clientQuery = searchExpression.FilterByTextTerms(clientQuery, (query, searchTermLower) => 
            //{
                //return query.Where(item => item.Participant.FirstName.Contains(searchTermLower)
                    //|| item.Participant.LastName.Contains(searchTermLower)
                    //|| (item.ExEventParticipant.item.Grade.HasValue && item.ExEventParticipant.item.Grade.Value.ToString().Contains(searchTermLower))
                    //); 
            //});

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
            return createLock(dc, NotifyClients, createHandler);
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
            var notification = EventParticipant.Search(dc, notifyExpression, null, 0, int.MaxValue);
            NotifyClients("siteHub", notifyExpression, notification, (hubClients, notificationItem) => hubClients.All.updateEventParticipants(notificationItem));
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

                DateTimeProviderTag.Create("EventDate", exResult.epSession.StartDate, TimeZones.Pacific),
                DateTimeProviderTag.Create("EventTime", exResult.epSession.StartDate, TimeZones.Pacific),
                StringProviderTag.Create("EventLocation", exResult.epSession.Location),
                
                StringProviderTag.Create("Address", "16722 NE 116th Street, Redmond WA 98052"),

                StringProviderTag.Create("SchoolCounselorName", exResult.epParticipantGroup.ContactName),

                // Check in is between «CheckinTimeStart» and «CheckinTimeEnd». All shopping must be completed by «ShoppingTimeEnd».
                DateTimeProviderTag.Create("CheckinTimeStart", exResult.epSession.StartDate.AddMinutes(-30), TimeZones.Pacific),
                DateTimeProviderTag.Create("CheckinTimeEnd", exResult.epSession.StartDate.AddHours(1), TimeZones.Pacific),
                DateTimeProviderTag.Create("ShoppingTimeEnd", exResult.epSession.EndDate.ToLocalTime()),

            };

            return TagProvider.Create(tags);
        }

        public override string ToString()
        {
            return string.Format("EventID: {0}, ParticipantID: {1}",
                /*0*/this.EventID,
                /*1*/this.ParticipantID);
        }
    }
}