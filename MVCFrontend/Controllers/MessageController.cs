using EasyHttp.Http;
using MVCFrontend.Extentions;
using MVCFrontend.Helpers;
using MVCFrontend.Models;
using MVCFrontend.Overrides.Filters;
using MyData;
using MyData.Models;
using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class MessageController : MyBaseController
    {
        // GET: Message
        public MessageController(ILogger logger, IMakeStaticsMockable injectMokcables): base(logger)
        {
        }
        [LogRequests]
        public ActionResult Index()
        {
            //Session.Abandon();
            var model = new MessageViewModel();

            Session["exp_cors"] = Utils.GetClaimFromToken(ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCorsToken), "exp");
            Session["exp_cors_token_time_utc"] = Utils.GetTimeClaimFromToken(DateTime.UtcNow - DateTime.UtcNow,
                                                        ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCorsToken), "exp");
            
            Session["exp_coookie"] = ClaimsPrincipal.Current.GetClaimValue("auth_cookie_exp");
            Session["exp_cookie_time_utc"] = ClaimsPrincipal.Current.HasClaim(c => c.Type == "auth_cookie_exp")
                ? Utils.TimestampToTime(DateTime.UtcNow - DateTime.UtcNow, ClaimsPrincipal.Current.GetClaimValue("auth_cookie_exp"))
                : DateTime.Now.AddHours(-5);


            model.UserName = ClaimsPrincipal.Current.GetClaimValue("given_name");
            model.Roles = string.Join(", ", ClaimsPrincipal.Current.GetAllClaims("role"));
            model.LogDropRequest = !RequestLog.IgnoreIp( HttpContext.Request.GetOwinContext().Request.RemoteIpAddress );
            ViewBag.Message = CheckSessionSettings();

            var registerUrl = string.Format("{0}/api/CheckinToken", Configsettings.EntrypointUrl());
            
            // when restarting entrypoint, we want to reregister on reload of page
            RegisterSocketServerAccessToken(
                registerUrl, 
                ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCorsToken), 
                ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess)
                );

            return View("SendMessage", model);
        }

        private void RegisterSocketServerAccessToken(string url, string corsToken, string socketServerAccessToken)
        {
            var eHttp = new EasyHttp.Http.HttpClient();
            eHttp.Request.AddExtraHeader("Authorization", string.Format("bearer {0}", corsToken));
            eHttp.Request.AddExtraHeader("X-socketServerAccessToken", socketServerAccessToken);
            eHttp.Post(url, socketServerAccessToken, HttpContentTypes.ApplicationXWwwFormUrlEncoded);

            if (eHttp.Response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.Error("RegisterSocketServerAccessToken returned {0}", eHttp.Response.StatusCode);
                throw new EasyHttp.Infrastructure.HttpException(eHttp.Response.StatusCode, eHttp.Response.StatusDescription);
            }
        }

        private string CheckSessionSettings()
        {
            var msg = string.Empty;

            if (Convert.ToDateTime(Session["asp_session_exp_time"]) <= Convert.ToDateTime(Session["exp_cookie_time_utc"]))
                msg += string.Format("<span class='Warn'>Warning: Asp session ({0}) expires before the auth cookie ({1}), this config does not work well!</span><br/>", 
                    Session["asp_session_exp_time"], Session["exp_cookie_time_utc"]);

            return msg;
        }

        [HttpGet]
        [LogRequests]
        public string AuthPing()
        {
            return "Auth Cookie still valid.";
        }
    }
}