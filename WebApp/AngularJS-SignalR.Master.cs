using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MS.Utility;
using MS.WebUtility;


namespace WebApp
{
    public partial class AngularJS_SignalR : System.Web.UI.MasterPage
    {
        public bool includeAngularUI = false;
        public bool includeAngularStrap = false;
        public bool includeAngularStrapTypeahead = false;
        public bool includeAngularMaterial = false;
        public bool includeAngularMaterialCdnLatest = false;

        public bool includeD3 = false;


        public bool includePlayingCards = false;

        protected string sysinfoObjectJson
        {
            get
            {
                var result = new
                {
                    isProductionSite = UtilityContext.Current.IsProductionSite,
                    isDevelopmentSite = UtilityContext.Current.IsDevelopmentSite,
                    serverStartTimeSeconds = UtilityContext.Current.ServerStart.ToCacheBreakSeconds(),
                }
                .ToJson();
                return result;
            }
        }

        protected string signinPageCustomObjectJson
        {
            get
            {
                var request = this.Request;
                Debug.Assert(request != null);
                var hostDomainName = request.Url.Host;

                var result = MS.Utility.TenantGroup.GetSigninPageCustomData(hostDomainName)
                    .ToJson();

                return result;
            }
        }

        protected string userStateObjectJson
        {
            get
            {
                var result = new
                {
                    // (there's probably a better way to do this that accounts for new UserStates. Hopefully this code will be seen if we do end up adding new UserState values)
                    active = UserState.Active.ToString(),
                    disabled = UserState.Disabled.ToString(),
                }
                .ToJson();
                return result;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Debug.Assert(!(includeAngularStrap && includeAngularUI));
            Debug.Assert(!(includeAngularStrap && includeAngularStrapTypeahead));

            this.Page.Title = "[placeholder]";

            var utilityContext = UtilityContext.Current;
            if (utilityContext != null)
            {
                this.Page.Title = utilityContext.AppInfo.ProductName ?? "Need Product Name";
            }
        }
    }
}