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
        public void Configuration(IAppBuilder app)
        {
            if (Appsettings.AzureIgnoreCertificateErrors())
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                            (sender, cert, chain, sslPolicyErrors) => true;
            }

            _logger.Info("startup starting");

            CheckHealth();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ExpireTimeSpan = TimeSpan.FromMinutes(Appsettings.CookieTimeoutMinutes()),
                SlidingExpiration = Appsettings.CookieSlidingExpiration(),
            });

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
                    SecurityTokenValidated = notification => /* Anonymous function */
                    {
                        _logger.Debug("SecurityTokenValidated notfication detected");

                        var identity = notification.AuthenticationTicket.Identity;
                        identity.AddClaim(new Claim("id_token", notification.ProtocolMessage.IdToken)); //id_token is for commnication with idSrv
                        identity.AddClaim(new Claim("access_token", notification.ProtocolMessage.AccessToken)); //access_token is for commnication with api

                        var expDate = DateTime.Now.AddMinutes(Appsettings.CookieTimeoutMinutes());
                        var expdateXml = System.Xml.XmlConvert.ToString(expDate, XmlDateTimeSerializationMode.Local);

                        identity.AddClaim(new Claim("auth_cookie_timeout", expdateXml)); // store it here
                        _logger.Debug("Notification(SecurityTokenValidated): cookie exp = {0}", expdateXml);

                        identity.AddClaim(new Claim("socket_token", Guid.NewGuid().ToString()));
                        identity.AddClaim(new Claim("msg_done_token", Guid.NewGuid().ToString()));

                        // not sure why this is needed
                        notification.AuthenticationTicket = new AuthenticationTicket(identity, notification.AuthenticationTicket.Properties);
                        return Task.FromResult(0);// return = irrelevant
                    },
                    RedirectToIdentityProvider = notification =>
                    {
                        //string appBaseUrl = notification.Request.Scheme + "://" + notification.Request.Host + notification.Request.PathBase;
                        //notification.ProtocolMessage.RedirectUri = appBaseUrl + SettingsHelper.LoginRedirectRelativeUri;
                        //notification.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl + SettingsHelper.LogoutRedirectRelativeUri;

                        _logger.Debug("RedirectToIdentityProvider notification detected");
                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        {
                            // set the session max age
                            var max_age = Appsettings.SessionMaxAgeMinutes() * 60;
                            _logger.Info("Setting notification.ProtocolMessage.MaxAge to {0} secs", max_age);
                            notification.ProtocolMessage.MaxAge = max_age.ToString();
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
                Authority = Appsettings.AuthUrl(),
                ValidationMode = ValidationMode.Both, 
                RequiredScopes = new[] { IdSrv3.ScopeMvcFrontEnd }
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
