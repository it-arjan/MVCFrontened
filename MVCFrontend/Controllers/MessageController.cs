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
            var model = new MessageViewModel();

            var corsExp = ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCorsToken);
            Session["exp_cors"] = Utils.GetClaimFromToken(corsExp, "exp");
            Session["exp_cors_utc_iso"] = TimestampToJsFormat(Utils.GetClaimFromToken(corsExp, "exp"));
            
            var cookieExp = ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCookieExp);
            Session["exp_coookie"] = cookieExp;
            Session["exp_cookie_utc_iso"] = TimestampToJsFormat(cookieExp);

            Session["exp_aspsession_utc_iso"] = ToISODateString(DateTime.UtcNow.AddMinutes(Session.Timeout));

            model.UserName = ClaimsPrincipal.Current.GetClaimValue("given_name");
            model.Roles = string.Join(", ", ClaimsPrincipal.Current.GetAllClaims("role"));
            model.LogDropRequest = !RequestLog.IgnoreIp( HttpContext.Request.GetOwinContext().Request.RemoteIpAddress );
            ViewBag.Message = CheckSessionSettings();

            var registerUrl = string.Format("{0}/api/CheckinToken", Configsettings.EntrypointUrl());

            try
            {
                // Handy for DEV
                // Register socketaccessToken here,
                // so when restarting WebEntrypoint, we only need to reload this page instead of logging out/in
                RegisterSocketServerAccessToken(
                    registerUrl,
                    ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimCorsToken),
                    ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess)
                    );
            }
            catch (Exception) {
                ViewBag.Message += "Cors Token Expired";
            }
            return View("SendMessage", model);
        }

        public string AspSessionExpInJsFormat()
        {
            return ToISODateString(DateTime.UtcNow.AddMinutes(Session.Timeout));
        }

        private string GetTimeClaimInJsFormat(string jwt)
        {
            return ToISODateString(
                Utils.GetTimeClaimFromToken(DateTime.UtcNow - DateTime.UtcNow, jwt, "exp")
                    );
        }

        private string TimestampToJsFormat(string timstamp)
        {
            return ToISODateString(
                Utils.TimestampToTime(DateTime.UtcNow - DateTime.UtcNow, timstamp)
                );
        }

        private string ToISODateString(DateTime d)
        {
            return d.ToString("yyyy-MM-ddZHH:mm:ss");
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

            if (Convert.ToDateTime(Session["asp_session_exp_time"]) <= Convert.ToDateTime(Session["exp_cookie_utc_iso"]))
                msg += string.Format("<span class='Warn'>Warning: Asp session ({0}) expires before the auth cookie ({1}), this config does not work well!</span><br/>", 
                    Session["asp_session_exp_time"], Session["exp_cookie_utc_iso"]);

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