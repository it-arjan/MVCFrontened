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

[assembly: OwinStartup(typeof(MVCFrontend.Startup))]

namespace MVCFrontend
{
    public class Startup
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(Startup));
        private void CheckHealth()
        {
            _logger.Debug("Checking config settings..");

            if (Helpers.Appsettings.Scheme() == null) throw new Exception(Helpers.Appsettings.SchemeKey + " is not present in app.config");
            if (Helpers.Appsettings.Hostname() == null) throw new Exception(Helpers.Appsettings.HostnameKey + " is not present in app.config");

            if (Helpers.Appsettings.AuthServer() == null) throw new Exception(Helpers.Appsettings.AuthServerKey + " is not present in app.config");
            if (Helpers.Appsettings.SiliconClientId() == null) throw new Exception(Helpers.Appsettings.SiliconClientIdKey + " is not present in app.config");
            if (Helpers.Appsettings.SiliconClientSecret() == null) throw new Exception(Helpers.Appsettings.SiliconClientSecretKey + " is not present in app.config");
            if (Helpers.Appsettings.FrontendClientId() == null) throw new Exception(Helpers.Appsettings.FrontendClientIdKey + " is not present in app.config");

            _logger.Debug("config setting seem ok..");
            _logger.Debug("Url = {0}", Helpers.Appsettings.HostUrl());
            _logger.Debug("Socket server Url = {0}", Helpers.Appsettings.SocketServerUrl());
            _logger.Debug("Auth server Url= {0}", Helpers.Appsettings.AuthUrl());
            _logger.Debug("..done with config checks");
        }
        public void Configuration(IAppBuilder app)
        {
            _logger.Info("startup starting");

            CheckHealth();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ExpireTimeSpan = TimeSpan.FromMinutes(15 * Helpers.IdSrv3.SessionSetting),
                Provider = new CookieAuthenticationProvider
                {
                    // do not redirect the ajax call from send message
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                        else _logger.Debug("Ajax call not redirected..");
                    }
                }
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = Helpers.Appsettings.FrontendClientId(),
                Authority = Helpers.Appsettings.AuthUrl(),
                RedirectUri = Helpers.Appsettings.HostUrl(),
                ResponseType = "token id_token",
                Scope = "openid roles " + Helpers.IdSrv3.ScopeMcvFrontEndHuman, 
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                PostLogoutRedirectUri = Helpers.Appsettings.HostUrl(),

                UseTokenLifetime =false, 
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = notification => /* Anonymous function */
                    {
                        var identity = notification.AuthenticationTicket.Identity;
                        identity.AddClaim(new Claim("id_token", notification.ProtocolMessage.IdToken)); //id_token is for commnication with idSrv
                        identity.AddClaim(new Claim("access_token", notification.ProtocolMessage.AccessToken)); //access_token is for commnication with api

                        // not sure why this is needed, disable it
                        //notification.AuthenticationTicket = new AuthenticationTicket(identity, notification.AuthenticationTicket.Properties);
                        return Task.FromResult(0);// return = irrelevant
                    },
                    RedirectToIdentityProvider = notification =>
                    {
                        //string appBaseUrl = notification.Request.Scheme + "://" + notification.Request.Host + notification.Request.PathBase;
                        //notification.ProtocolMessage.RedirectUri = appBaseUrl + SettingsHelper.LoginRedirectRelativeUri;
                        //notification.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl + SettingsHelper.LogoutRedirectRelativeUri;

                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        {
                            // set the session max age
                            notification.ProtocolMessage.MaxAge = (60 * Helpers.Appsettings.AuthSessionLengthMinutes() * Helpers.IdSrv3.SessionSetting).ToString();
                        }

                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            //set the tokenHint (not the token) to the actual token .. very inuitive ;)
                            var IdTokenclaim = notification.OwinContext.Authentication.User.FindFirst("id_token");
                            notification.ProtocolMessage.IdTokenHint = IdTokenclaim != null ?IdTokenclaim.Value:"";
                        }

                        return Task.FromResult(0);
                    }   
                }
            });

            // silicon client authorization
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = Helpers.Appsettings.AuthUrl(),
                ValidationMode = ValidationMode.ValidationEndpoint, 
                RequiredScopes = new[] { Helpers.IdSrv3.ScopeMcvFrontEnd },
                ValidationResultCacheDuration= TimeSpan.FromMinutes(15)
            });

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
