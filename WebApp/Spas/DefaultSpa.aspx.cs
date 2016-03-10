using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MS.Utility;
using MS.WebUtility;
//using App.Library;

namespace WebApp.Spas
{
    public partial class DefaultSpaPage : SitePage
    {
        protected string SignInPageDemoDataJson { get; private set; }

        protected string USStatesArrayJson { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                //Server.Transfer("/Signin/default.aspx");
                //Debug.Fail("Shouldn't get here");
            }

            var master = this.Master as AngularJS_SignalR;

            master.spaName = "User";

            //master.includeAngularUI = true;
            //master.includeAngularStrap = true;
            //master.includeAngularStrapTypeahead = true;

            master.includeAngularMaterial = true;
            //!! temporarily switching to the CDN until 0.9.0 is available in nuget
            //master.includeAngularMaterial = false;
            //master.includeAngularMaterialCdnLatest = true;
            master.includeD3 = false;
            master.includeAngularTranslate = true;


            var request = this.Request;
            Debug.Assert(request != null);
            var hostDomainName = request.Url.Host;

            this.SignInPageDemoDataJson = ", demo:" + MS.Utility.TenantGroup.GetSigninPageCustomData(hostDomainName).ToJson();

            this.USStatesArrayJson = MS.Utility.StateCodes.Data
                .Select(state => new 
                {
                    name = state.Name,
                    abbreviation = state.Abbreviation,
                    code = state.Code,
                }).ToJson();


#if false

            Identity authorizedBy = Context.GetIdentity();

            bool dashboardApproved = authorizedBy.IsSystemAdmin 
                //|| authorizedBy.IsInRole(AppRole.Radiologist)
                ;

            if (dashboardApproved)
            {
                //Server.Transfer("/Home.aspx");
                //Debug.Fail("Shouldn't get here");
            }

            if (authorizedBy.IsOperationsAdmin)
            {
                //Server.Transfer("/Ops/Default.aspx");
                //Debug.Fail("Shouldn't get here");
            }
#endif
            // Hmm. User doesn't have sufficient permissions to go anyplace interesting. Park them here.
        }
    }
}