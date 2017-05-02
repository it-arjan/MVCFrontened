using MVCFrontend.Models;
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

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(MessageController), Appsettings.LogLevel());
        // GET: Message
        public ActionResult Index()
        {
            var model = new MessageViewModel();

            ClaimsPrincipal.Current.AddUpdateClaim("ajax_remote_queue_token", IdSrv3.NewSiliconClientToken(IdSrv3.ScopeEntryQueueApi));
            Session["exp_cors_token_time"] = Utils.GetDateTimeClaimFromToken(
                                                        ClaimsPrincipal.Current.GetClaim("ajax_remote_queue_token")
                                                        , "exp");
            Session["exp_cookie_time"] = XmlConvert.ToDateTime(
                                            ClaimsPrincipal.Current.GetClaim("auth_cookie_timeout")
                                            , XmlDateTimeSerializationMode.Local);

            model.UserName = ClaimsPrincipal.Current.GetClaim("given_name");
            model.Roles = string.Join(", ", ClaimsPrincipal.Current.GetAllClaims("role"));

            ViewBag.Message = CheckSessionSettings();
            return View("SendMessage", model);
        }

        private string CheckSessionSettings()
        {
            var msg = string.Empty;
            msg += string.Format("<span class='Info'>Idsrv3.UseTokenLifetime={0}</span><br/>", Appsettings.UseTokenLifetime());
            //msg += string.Format("<span class='Info'>Idsrv3.SessionMaxAgeMinutes={0}</span><br/>", Appsettings.SessionMaxAgeMinutes());
            msg += string.Format("<span class='Info'>Cookie.Auth.SlidingExp={0}</span><br/>", Appsettings.CookieSlidingExpiration());
            //Appsettings.SessionMaxAgeMinutes();

            if (Convert.ToDateTime(Session["asp_session_exp_time"]) <= Convert.ToDateTime(Session["exp_cookie_time"]))
                msg += string.Format("<span class='Warn'>Warning: Asp session {0} expires before the auth cookie {1}, this config does not work well!</span><br/>", 
                    Session["asp_session_exp_time"], Session["exp_cookie_time"]);

            return msg;
        }

        [HttpGet]
        public string AuthPing()
        {
            return "Silicon token valid";
        }

        [HttpPost]
        public ActionResult Postback(PostbackData data)
        {
            PostbackData protect = new PostbackData(data);
           _logger.Debug("Data is posted back:  '{0}'", JsonConvert.SerializeObject(data));
            //Task.Delay(10).Wait();
            try
            {
                // ETF handles the data scurity
                var db = new DAL.FrontendDbContext();
                db.Postbacks.Add(data);
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                string msg = ex.Message;
                while (ex.InnerException != null)
                {
                    msg = string.Format("{0} \n{1}", msg, ex.InnerException.Message);
                    ex = ex.InnerException;
                }
                _logger.Error("Error saving postback in dbcontext: {0}/n posted values: '{1}'", msg, JsonConvert.SerializeObject(data));
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
            
        }
        
        private class EntryQApiResult
        {
            public string message { get; set; }
        }
        private class QueuePostdata
        {
            public string MessageId { get; set; }
            public string PostBackUrl { get; set; }
            public string SocketToken { get; set; }
            public string DoneToken { get; set; }
            public string UserName { get; set; }
        }
    }
}