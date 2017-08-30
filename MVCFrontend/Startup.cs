using IdentityServer3.AccessTokenValidation;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVCFrontend.Helpers;
using NLogWrapper;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(MVCFrontend.Startup))]

namespace MVCFrontend
{
    public class Startup
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(Startup), Configsettings.LogLevel());
        private void CheckHealth()
        {
            _logger.Info("Checking config settings..");
            _logger.Info("Running under: Environment.UserName= {0}, Environment.UserDomainName= {1}", Environment.UserName, Environment.UserDomainName);
            SettingsChecker.CheckPresenceAllPlainSettings(typeof(Configsettings));

            _logger.Info("all requried config settings seem present..");
            _logger.Info("Url = {0}", Configsettings.HostUrl());
            _logger.Info("Socket server Url = {0}", Configsettings.SocketServerUrl());
            _logger.Info("Auth server Url= {0}", Configsettings.AuthUrl());
            _logger.Info("..done with config checks.");
        }

        private static Task GetAuthCookieExp(CookieValidateIdentityContext context)
        {
            if (!IsAjaxRequest(context.Request))
            {
                // here we get the cookie expiry time
                var ticksUntil_1970 = new DateTime(1970, 1, 1).Ticks;
                var expireUtc = (context.Properties.ExpiresUtc.Value.UtcTicks - ticksUntil_1970) / 10000000;

                // add the expiry time back to cookie as one of the claims, called 'myExpireUtc'
                // to ensure that the claim has latest value, we must keep only one claim
                // otherwise we will be having multiple claims with same type but different values
                var claimType = "auth_cookie_exp";
                var identity = context.Identity;
                if (identity.HasClaim(c => c.Type == claimType))
                {
                    var existingClaim = identity.FindFirst(claimType);
                    identity.RemoveClaim(existingClaim);
                }
                var newClaim = new Claim(claimType, expireUtc.ToString());
                context.Identity.AddClaim(newClaim);
            }
            return Task.FromResult(0);
        }

        public void Configuration(IAppBuilder app)
        {
            if (Configsettings.OnAzure())
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                            (sender, cert, chain, sslPolicyErrors) => true;
            }

            _logger.Info("startup starting");

            CheckHealth();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            //debugging middleware code
            //app.Use(async (Context, next) =>
            //{
            //    _logger.Debug("OWIN DEBUG::1 ==>Before cookie, before OIDC");
            //    await next.Invoke();
            //    _logger.Debug("OWIN DEBUG::6 ==>after cookie, after Bearer");
            //});

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ExpireTimeSpan = TimeSpan.FromMinutes(Configsettings.CookieTimeoutMinutes()),
                SlidingExpiration = Configsettings.CookieSlidingExpiration(),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = GetAuthCookieExp,
                    OnApplyRedirect = ctx =>
                    {
                        // doesn't hit, UseOpenIdConnectAuthentication is doing the redirection
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    },
                    OnResponseSignIn = ctx => {
                        if (true) { }
                    },
                    OnException = ctx => {
                        if (true) { }
                    },
                    OnResponseSignedIn = ctx => {
                        if (true) { }
                    },
                    OnResponseSignOut = ctx => {
                        if (true) { }
                    },
                }
            });
            //debugging middleware code
            //app.Use(async (Context, next) =>
            //{
            //    _logger.Debug("OWIN DEBUG::2 ==>after cookie, before OIDC");
            //    await next.Invoke();
            //    _logger.Debug("OWIN DEBUG:: ", Context.Response.StatusCode != 200 ? "3. Intervention from OIDC" : "5. after Bearer");
            //});

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = Configsettings.FrontendClientId(),
                Authority = Configsettings.AuthUrl(),
                RedirectUri = Configsettings.HostUrl(),
                ResponseType = "token id_token",
                Scope = "openid roles " + IdSrv3.ScopeMcvFrontEndHuman,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                PostLogoutRedirectUri = Configsettings.HostUrl(),
                UseTokenLifetime = Configsettings.UseTokenLifetime(),

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n => /* Anonymous function */
                    {
                        _logger.Debug("OpenIdConnect: SecurityTokenValidated");

                        var identity = n.AuthenticationTicket.Identity;

                        CleanupClaims(identity);

                        // Add new, no need for std id_token & access_token, we use cookie auth
                        identity.AddClaim(new Claim(IdSrv3.ClaimCorsToken, IdSrv3.NewSiliconClientToken(IdSrv3.ScopeEntryQueueApi)));
                        identity.AddClaim(new Claim(IdSrv3.ClaimApiToken, IdSrv3.NewSiliconClientToken(IdSrv3.ScopeFrontendDataApi)));
                        identity.AddClaim(new Claim(IdSrv3.ClaimScoketAccess, Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim(IdSrv3.ClaimQmFeedId, Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim(IdSrv3.ClaimNotificationFeedId, Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim(IdSrv3.ClaimApiFeedId, Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim(IdSrv3.ClaimPostbackCompleted, Guid.NewGuid().ToString()));


                        // not sure why creating a new AuthenticationTicket is needed
                        n.AuthenticationTicket = new AuthenticationTicket(identity, n.AuthenticationTicket.Properties);
                        return Task.FromResult(0); //return = irrelevant
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        _logger.Debug("OpenIdConnect: RedirectToIdentityProvider");
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        {
                            // set the session max age
                            var max_age = Configsettings.SessionMaxAgeMinutes() * 60;
                            // _logger.Info("Setting notification.ProtocolMessage.MaxAge to {0} secs", max_age);
                            //notification.ProtocolMessage.MaxAge = max_age.ToString();
                        }

                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            //set the tokenHint (not the token) to the actual token .. very inuitive ;)
                            var IdTokenclaim = n.OwinContext.Authentication.User.FindFirst("id_token");
                            n.ProtocolMessage.IdTokenHint = IdTokenclaim != null ? IdTokenclaim.Value : "";
                        }

                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = n =>
                    {
                        return Task.FromResult(0);
                    },
                    AuthorizationCodeReceived = n =>
                    {
                        // doesn't get here
                        _logger.Debug("OpenIdConnect: AuthorizationCodeReceived."); 
                        return Task.FromResult(0);
                    },
                    MessageReceived = n=>
                    {
                        return Task.FromResult(0);
                    },
                    SecurityTokenReceived = n=>
                    {
                        return Task.FromResult(0);
                    },


                }
            });

              // silicon client authorization
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = Configsettings.AuthUrl(),
                ValidationMode = ValidationMode.Both, 
                RequiredScopes = new[] { IdSrv3.ScopeMvcFrontEnd }
            });
            
            // Later figure out auth config for nancy
            //app.UseNancy(new NancyOptions
            //{
                
            //});

            _logger.Info("startup executed");
        }

        private static void CleanupClaims(ClaimsIdentity identity)
        {
            var claimsToRemove = new List<Claim>();
            foreach (var claim in identity.Claims)
            {
                if (claim.Type == "iss" ||
                    claim.Type == "nbf" ||
                    claim.Type == "aud" ||
                    claim.Type == "iat" ||
                    claim.Type == "at_hash" ||
                    claim.Type == "sid" ||
                    claim.Type == "idp" ||
                    claim.Type == "nonce" ||
                    claim.Type == "amr")
                {
                    claimsToRemove.Add(claim);
                }
            }
            claimsToRemove.ForEach(c => identity.RemoveClaim(c));
        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }
    }
}
