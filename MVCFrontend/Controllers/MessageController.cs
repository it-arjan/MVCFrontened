using MyData;
using MyData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using NLogWrapper;
using System.Net.Http.Headers;

using EasyHttp.Http;
using System.Net;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using System.Security.Permissions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVCFrontend.Helpers;
using System.Xml;
using MVCFrontend.Extentions;
using System.Configuration;
using MVCFrontend.Models;
using MVCFrontend.Filters;

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private ILogger _logger;
        // GET: Message
        public MessageController(ILogger logger)
        {
            _logger = logger;
            _logger.SetLevel(Appsettings.LogLevel());
        }
        [LogRequests]
        public ActionResult Index()
        {
            //Session.Abandon();
            var model = new MessageViewModel();

            //ClaimsPrincipal.Current.AddUpdateClaim("ajax_cors_token", IdSrv3.NewSiliconClientToken(IdSrv3.ScopeEntryQueueApi));
            Session["exp_cors"] = Utils.GetClaimFromToken(ClaimsPrincipal.Current.GetClaimValue("ajax_cors_token"), "exp");
            Session["exp_cors_token_time_utc"] = Utils.GetTimeClaimFromToken(DateTime.UtcNow - DateTime.UtcNow,
                                                        ClaimsPrincipal.Current.GetClaimValue("ajax_cors_token"), "exp");

            Session["exp_coookie"] = ClaimsPrincipal.Current.GetClaimValue("auth_cookie_exp");
            Session["exp_cookie_time_utc"] = ClaimsPrincipal.Current.HasClaim(c => c.Type == "auth_cookie_exp")
                ? Utils.TimestampToTime(DateTime.UtcNow - DateTime.UtcNow, ClaimsPrincipal.Current.GetClaimValue("auth_cookie_exp")).AddSeconds(Appsettings.CookieTimeoutExpireOffset())
                : DateTime.Now.AddHours(-5);


            model.UserName = ClaimsPrincipal.Current.GetClaimValue("given_name");
            model.Roles = string.Join(", ", ClaimsPrincipal.Current.GetAllClaims("role"));

            ViewBag.Message = CheckSessionSettings();
            return View("SendMessage", model);
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

        [HttpPost]
        public ActionResult Postback(DataFromQM MqResult)
        {
           _logger.Debug("Data is posted back:  '{0}'", JsonConvert.SerializeObject(MqResult));

            try
            {
                var db = DbFactory.Db();
                WebNotification.Send(MqResult.NotificationToken, "Postback received.");
                // manually log the request
                // in oder to be able to related it to the human session
                RequestLog.StoreRequestForSessionId(db, MqResult.AspSessionId);

                var data = new PostbackData();
                data.MessageId = MqResult.MessageId;
                data.Start = MqResult.Start;
                data.End = MqResult.End;
                data.AspSessionId = MqResult.AspSessionId;
                data.Content = MqResult.Content;
                data.Duration = MqResult.Duration;
                data.UserName = MqResult.UserName;

                db.Add(data);
                db.Commit();
                db.Dispose();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                string msg = ex.Message;
                while (ex.InnerException != null)
                {
                    msg = string.Format("{0} \n{1}", msg, ex.InnerException.Message);
                    ex = ex.InnerException;
                }
                _logger.Error("Error saving postback in dbcontext: {0}/n posted values: '{1}'", msg, JsonConvert.SerializeObject(MqResult));
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            
        }
        [HttpGet]
        public JsonResult GetPostbacks()
        {
            var db = DbFactory.Db();
            var recentPostbacks = db.GetRecentPostbacks(10);
            db.Dispose();
            return Json(JsonConvert.SerializeObject(recentPostbacks), JsonRequestBehavior.AllowGet );

        }
        public class DataFromQM
        {
            public string MessageId { get; set; }
            public string UserName { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public decimal Duration { get; set; }
            public string Content { get; set; }
            public string AspSessionId { get; set; }
            public string NotificationToken { get; set; }
        }
    }
}