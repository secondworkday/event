using System.Xml.Serialization;

using MS.WebUtility;

namespace App.Library
{

    public class ClientFeatures
    {
        public enum Feature
        {
            // retired 12/16/2013 shareProject,           // share project via "Personal Link" URL 
            shareProject,           // share project via "Personal Link" URL 

            createPepProjectLink,   // create PEP projects via "Training Link" URL or "Workshop Code"
            createWorkshop,         // create a workshop 
            educationFoS,           // allow education Field of Study to be specified on the basic info page

            userAccountInvites,     // send authenticatedUrls via email to users inviting them to create TORQ accounts

            placeholder,    // doesn't do anything - but shows where code changes need to be made to add a feature to Ops/Config

            noSharedProjectAccess,          //default, users can only view/edit projects that they have created
            groupAdminSharedProjectAccess,  //allows group admins to view/edit all projects in their client group, regardless of creator
            allUserSharedProjectAccess,     //allows all users to view edit/edit all projects in their client group, regardless of creator
        }

        public static readonly ClientFeatures[] EmptyArray = new ClientFeatures[0];
        public static readonly Feature[] EmptyFeatureArray = new Feature[0];

        [XmlAttribute]
        public int ClientId;

        [XmlElement("Feature")]
        public Feature[] EnabledFeatures = EmptyFeatureArray;
    }


    [XmlRoot("ConfigSettings")]
    public class SiteContextData : WebUtilityContextData
    {

    }
}