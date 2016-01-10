using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp
{
    public partial class ResetPasswordPage : SitePage
    {
        protected AuthToken passwordResetAuthToken;
        protected TenantGroupInfo tenantGroupInfo;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var utilityContext = UtilityContext.Current;

            if (utilityContext.RequireSslLogin)
            {
                // We shouldn't be with unless we've already got a secure connection - since we generally Server.Transfer to this page
                Debug.Assert(this.Request.IsSecureConnection);
                this.Context.RedirectToSecureConnection();
                Debug.Assert(this.Request.IsSecureConnection);
            }
        }

        protected string AuthCode { get; private set; }
        protected string TenantName { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.passwordResetAuthToken = this.Request.GetAuthTokenOrTransfer(ResetPasswordAuthTemplate.AuthTokenTypeName, "authCode", "/Default.aspx");

            if (this.passwordResetAuthToken != null)
            {
                this.AuthCode = passwordResetAuthToken.AuthCode.ToString();
            }

            if (this.passwordResetAuthToken != null && this.passwordResetAuthToken.ScopeType == ExtendedPropertyScopeType.TenantGroupID)
            {
                Debug.Assert(this.passwordResetAuthToken.ScopeID.HasValue);
                if (this.passwordResetAuthToken.ScopeID.HasValue)
                {
                    var tenantGroupID = this.passwordResetAuthToken.ScopeID.Value;

                    var tenantGroupInfo = TenantGroup.GetCachedTenantGroupInfo(tenantGroupID);
                    if (tenantGroupInfo != null)
                    {
                        this.TenantName = tenantGroupInfo.Name;
                        this.Title = string.Format("Sign in &middot; {0}",
                            /*0*/ tenantGroupInfo.Name);
                    }
                }
            }

            //if this authCode has expired, show error message and hide the form
            if (passwordResetAuthToken == null || !passwordResetAuthToken.IsActive(DateTime.UtcNow))
            {
                //this.Blurb.Visible = false;
                //this.PasswordDisplayPanel.Visible = false;
                //this.ChangePasswordErrorLabel.InnerHtml = "<span class=\"ui-icon ui-icon-info\"></span><span class=\"warning\">This link has expired. You will need to make a new password reset request.</span>";
                //this.ChangePasswordErrorLabel.Visible = true;
            }
        }

#if false
        protected void ResetPassword_Click(object sender, EventArgs e)
        {
            var utilityContext = UtilityContext.Current;

            //this.ChangePasswordMessageLabel.Visible = false;
            //this.ChangePasswordErrorLabel.Visible = false;

            if (this.Page.IsValid)
            {
                string newPassword = this.Password.Text;
                //string confirmPassword = this.ConfirmNewPasswordTextBox.Text;

                // change password
                //if (newPassword == confirmPassword && newPassword.Length > 0)
               // {
                    var user = MS.Utility.User.ResetPassword(this.passwordResetAuthToken.AuthCode, newPassword);

                    if (user != null)
                    {
                        //this.ChangePasswordMessageLabel.Visible = true;

                        //sign in user with new credentials
                        //!! wire up the persistent cookie bool to a checkbox...
                        WebUserAuthentication.LoginSession(user, false);
                        //this.Blurb.Visible = false;
                        //this.PasswordDisplayPanel.Visible = false;
                        //this.HomePageLinkDiv.Visible = true;

                        //string relativeUrl = this.HomePageLink.HRef;
                        //string unsecureRelativeUrl = HttpContext.Current.GetUnsecureRedirectUrl(relativeUrl);
                        //this.HomePageLink.HRef = unsecureRelativeUrl;

                        Response.Redirect("/");
                    }
                    else
                    {
                        //this.ChangePasswordErrorLabel.Visible = true;
                    }
               // }
            }
        }
#endif


    }
}