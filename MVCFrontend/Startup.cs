using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OpenIdConnect;
using NLogWrapper;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using IdentityServer3.AccessTokenValidation;
using System.Net;
using MVCFrontend.Helpers;
using System.Xml;
using Microsoft.AspNet.Identity.Owin;
using Nancy.Owin;
using IdentityModel.Client;

[assembly: OwinStartup(typeof(MVCFrontend.Startup))]

namespace MVCFrontend
{
    public class Startup
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(Startup), Appsettings.LogLevel());
        private void CheckHealth()
        {
            _logger.Info("Checking config settings..");
            _logger.Info("Running under: Environment.UserName= {0}, Environment.UserDomainName= {1}", Environment.UserName, Environment.UserDomainName);
            if (Appsettings.Scheme() == null) throw new Exception(Appsettings.SchemeKey + " is not present in app.config");
            if (Appsettings.Hostname() == null) throw new Exception(Appsettings.HostnameKey + " is not present in app.config");

            if (Appsettings.AuthServer() == null) throw new Exception(Appsettings.AuthServerKey + " is not present in app.config");
            if (Appsettings.SiliconClientId() == null) throw new Exception(Appsettings.SiliconClientIdKey + " is not present in app.config");
            if (Appsettings.SiliconClientSecret() == null) throw new Exception(Appsettings.SiliconClientSecretKey + " is not present in app.config");
            if (Appsettings.FrontendClientId() == null) throw new Exception(Appsettings.FrontendClientIdKey + " is not present in app.config");

            _logger.Info("all requried config settings present..");
            _logger.Info("Url = {0}", Appsettings.HostUrl());
            _logger.Info("Socket server Url = {0}", Appsettings.SocketServerUrl());
            _logger.Info("Auth server Url= {0}", Appsettings.AuthUrl());
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
            if (Appsettings.OnAzure())
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                            (sender, cert, chain, sslPolicyErrors) => true;
            }

            _logger.Info("startup starting");

            CheckHealth();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            //app.Use(async (Context, next) =>
            //{
            //    _logger.Debug("OWIN DEBUG::1 ==>Before cookie, before OIDC");
            //    await next.Invoke();
            //    _logger.Debug("OWIN DEBUG::6 ==>after cookie, after Bearer");
            //});

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ExpireTimeSpan = TimeSpan.FromMinutes(Appsettings.CookieTimeoutMinutes()),
                SlidingExpiration = Appsettings.CookieSlidingExpiration(),
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

            //app.Use(async (Context, next) =>
            //{
            //    _logger.Debug("OWIN DEBUG::2 ==>after cookie, before OIDC");
            //    await next.Invoke();
            //    _logger.Debug("OWIN DEBUG:: ", Context.Response.StatusCode != 200 ? "3. Intervention from OIDC" : "5. after Bearer");
            //});

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = Appsettings.FrontendClientId(),
                Authority = Appsettings.AuthUrl(),
                RedirectUri = Appsettings.HostUrl(),
                ResponseType = "token id_token",
                Scope = "openid roles " + IdSrv3.ScopeMcvFrontEndHuman,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                PostLogoutRedirectUri = Appsettings.HostUrl(),
                UseTokenLifetime = Appsettings.UseTokenLifetime(),

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n => /* Anonymous function */
                    {
                        _logger.Debug("OpenIdConnect: SecurityTokenValidated");

                        var identity = n.AuthenticationTicket.Identity;
                        identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken)); //id_token is for commnication with idSrv
                        identity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken)); //access_token is for commnication with api

                        identity.AddClaim(new Claim("qm_socket_id", Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim("notification_socket_id", Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim("msg_done_id", Guid.NewGuid().ToString()));

                        //var UserInfoEndpoint = string.Format("{0}{1}", Appsettings.AuthUrl(), "connect/userinfo");
                        //var userInfoClient = new UserInfoClient(new Uri(UserInfoEndpoint), n.ProtocolMessage.AccessToken);
                        //var userInfoResponse = userInfoClient.GetAsync().Result;
                        //var userClaims = userInfoResponse.GetClaimsIdentity().Claims;
                        //identity.AddClaims(userClaims);

                        var corsToken = IdSrv3.NewSiliconClientToken(IdSrv3.ScopeEntryQueueApi);
                        identity.AddClaim(new Claim("ajax_cors_token", corsToken));

                        // not sure why creating a new AuthenticationTicket is needed
                        n.AuthenticationTicket = new AuthenticationTicket(identity, n.AuthenticationTicket.Properties);
                        return Task.FromResult(0);// return = irrelevant
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        _logger.Debug("OpenIdConnect: RedirectToIdentityProvider");
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        {
                            // set the session max age
                            var max_age = Appsettings.SessionMaxAgeMinutes() * 60;
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
                Authority = Appsettings.AuthUrl(),
                ValidationMode = ValidationMode.Both, 
                RequiredScopes = new[] { IdSrv3.ScopeMvcFrontEnd }
            });
            
            // Later figure out auth config for nancy
            //app.UseNancy(new NancyOptions
            //{
                
            //});

            _logger.Info("startup executed");
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
