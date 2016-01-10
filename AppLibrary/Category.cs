using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Net.Mail;
using System.Web;
using System.Threading;
using System.Configuration;
using System.Runtime.Serialization;

using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

using MS.Utility;
using MS.WebUtility;

namespace App.Library
{
    /// <summary>
    /// These are EPCategory values that are shared between multiple object types - and as a result can't be defined as type-specific
    /// </summary>
    public class SharedEPCategory
    {


        // Used for general application-assigned things - Extended Properties used by the application itself to keep track of things.
        public static readonly EPCategory AppAssignedCategory = EPCategory.CreateMultiple("AppAssigned", 1);


        public static readonly EPCategory VideoTagsCategory = EPCategory.CreateSingle<Video>("Quality");

        // NzbPost
        public static EPCategory NzbPostGroupsCategory = EPCategory.CreateMultiple<NzbPost>("Group");
        // Note: (we track processing via EPCategory_App)


        // Used for tracking items present in "Feeds" - New Feed, Suggested Feed, Missing Feed, etc.
        //!! public static readonly EPCategory FeedCategory = EPCategory.CreateMultiple("Feed", 201);
        public static readonly string UnrecognizedFeedTagName = "Unrecognized";                  // Item we don't have 
        public static readonly string UnknownFeedTagName = "Unknown";                  // Item we don't have 

        public const string DiscoveryFeedTagName = "Discovery";       // Item we 

        public const string MissingFeedTagName = "Missing";          // Item we have later episodes for - it fills in a gap 
        public const string SuggestedFeedTagName = "Suggested";      // A item that has characteristics we feel recommends it  

        public const string MetadataFeedTagName = "Metadata";        // Need Metadata for this item  


        // Used for tracking items present in "Feeds" - New Feed, Suggested Feed, Missing Feed, etc.
        //!! public static readonly EPCategory ListCategory = EPCategory.CreateMultiple("List", 202);

        public const string SkipListTagName = "Skip";                     // Not Interested in this one   
        public const string HaveListTagName = "Have";                     // Have this one   
        public const string AutoDownloadListTagName = "AutoDownload";     // Download requested   
    }
}