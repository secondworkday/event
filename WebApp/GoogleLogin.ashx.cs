using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

using MS.Utility;
using MS.WebUtility;
using MS.WebUtility.Authentication;

namespace WebApp
{
    /// <summary>
    /// Summary description for GoogleLogin
    /// </summary>
    public class GoogleLogin : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var utilityContext = UtilityContext.Current;

            using (OpenIdRelyingParty openid = new OpenIdRelyingParty())
            {
                var response = openid.GetResponse();
                if (response != null)
                {
                    switch (response.Status)
                    {
                        case AuthenticationStatus.Authenticated:
                            // This is where you would look for any OpenID extension responses included
                            // in the authentication assertion.
                            var claimsResponse = response.GetExtension<ClaimsResponse>();

                            //Database.ProfileFields = claimsResponse;

                            // Store off the "friendly" username to display -- NOT for username lookup
                            //Database.FriendlyLoginName = response.FriendlyIdentifierForDisplay;

                            // Use FormsAuthentication to tell ASP.NET that the user is now logged in,
                            // with the OpenID Claimed Identifier as their username.
                            //FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, false);

                            var user = User.AuthenticateUser(response);

                            if (user != null)
                            {
                                WebIdentityAuthentication.LoginSession(user, false);
                                WebIdentityAuthentication.RedirectFromLoginPage();
                                Debug.Fail("Shouldn't get here");
                            }

                            WebIdentityAuthentication.ErrorRedirectBackToLoginPage("AuthPolicy");
                            Debug.Fail("Shouldn't get here");
                            break;
                        case AuthenticationStatus.Canceled:
                            WebIdentityAuthentication.ErrorRedirectBackToLoginPage();
                            Debug.Fail("Shouldn't get here");
                            break;
                        case AuthenticationStatus.Failed:
                            WebIdentityAuthentication.ErrorRedirectBackToLoginPage("AuthFailed");
                            Debug.Fail("Shouldn't get here");
                            break;
                        default:
                            Debug.Fail("Unexpected GoogleLogin response status: " + response.Status);
                            break;
                    }

                    // Nope - back to the Login page ...
                    // We Transfer (rather than Response.Redirect()) so we can display an appropriate error without having to include that error condition in the URL
                    WebIdentityAuthentication.ErrorRedirectBackToLoginPage("AuthFailed");
                    Debug.Fail("Shouldn't get here");
                }
                else
                {
                    try
                    {
                        IAuthenticationRequest request = openid.CreateRequest("https://www.google.com/accounts/o8/id");

                        // This is where you would add any OpenID extensions you wanted
                        // to include in the authentication request.
                        request.AddExtension(new ClaimsRequest
                        {
                            Country = DemandLevel.Request,
                            Email = DemandLevel.Require,
                            Gender = DemandLevel.Require,
                            PostalCode = DemandLevel.Require,
                            TimeZone = DemandLevel.Require,
                        });

                        // Send your visitor to their Provider for authentication.
                        request.RedirectToProvider();
                        Debug.Fail("Shouldn't get here");
                    }
                    catch (ProtocolException ex)
                    {
                        // The user probably entered an Identifier that
                        // was not a valid OpenID endpoint.

                        //this.openidValidator.Text = ex.Message;
                        //this.openidValidator.IsValid = false;
                    }
                }
            }

            WebIdentityAuthentication.RedirectFromLoginPage();
            //!!context.Response.Redirect("/");
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}