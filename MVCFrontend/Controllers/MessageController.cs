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

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(MessageController), Helpers.Appsettings.LogLevel());
        // GET: Message
        public ActionResult Index()
        {
            var model = new MessageViewModel();
            model.AjaxAccessToken = NewSiliconClientToken(Helpers.IdSrv3.ScopeMcvFrontEnd).AccessToken;
            model.AjaxQueueAccessToken = NewSiliconClientToken(Helpers.IdSrv3.ScopeEntryQueueApi).AccessToken;
            model.SocketToken = Guid.NewGuid().ToString();
            model.DoneToken = Guid.NewGuid().ToString();
            model.UserName = GetClaimValuesFromPrincipal("given_name").FirstOrDefault();
            model.Roles = string.Join(", ", GetClaimValuesFromPrincipal("role"));
            return View("SendMessage", model);
        }
        [HttpGet]
        public string AuthPing()
        {
            return "Silicon token valid";
        }

        [HttpPost]
        public string ToRemoteQueue(string message, string socketToken, string doneToken)
        {
            string result = null;
            var model = new MessageViewModel();
            if (!string.IsNullOrEmpty(message.Trim()))
            {
                // btw: Request.Headers.Authorization.Parameter == null?
                var token = NewSiliconClientToken(Helpers.IdSrv3.ScopeEntryQueueApi); 
                if (!token.IsError)
                {

                    var apiUrl = string.Format("{0}/Drop",Helpers.Appsettings.QueueApiUrl());

                    var auth_header = string.Format("bearer {0}", token.AccessToken);

                    var easyHttp = new HttpClient();
                     
                    easyHttp.Request.AddExtraHeader("Authorization", auth_header);
                    easyHttp.Request.Accept = HttpContentTypes.ApplicationJson;

                    var data = new QueuePostdata();
                    data.MessageId = message;
                    data.PostBackUrl = string.Format("{0}/Message/Postback", Helpers.Appsettings.HostUrl());
                    data.SocketToken = socketToken;
                    data.DoneToken = doneToken;
                    // Note: Claims still contain the human values because this tokens scope is a resource scope
                    data.UserName = GetClaimValuesFromPrincipal("given_name").FirstOrDefault();

                    easyHttp.Post(apiUrl, data, "application/json");

                    model.ApiResult = new ApiResultModel();
                    var errorText = easyHttp.Response.StatusCode != HttpStatusCode.OK
                        ? string.Format("Queue Api returned {0} and ",easyHttp.Response.StatusCode)
                        : string.Empty;

                    model.ApiResult.Message = string.Format("{0} '{1}'", errorText, JsonConvert.DeserializeObject<EntryQApiResult>(easyHttp.Response.RawText).message);
                }
                else
                {
                    _logger.Error("Error getting the silicon client token: {0}", token.Error);
                    model.ApiResult = new ApiResultModel();
                    model.ApiResult.Message = "Error getting token for the the silicon client, Queue Api not called ";
                }
                result = model.ApiResult.Message;
            }
            else result= "Please enter a Message";

            return result;
        }

        private List<string> GetClaimValuesFromPrincipal(string claimType)
        {
            return ClaimsPrincipal.Current
                        .Claims.Where(c => c.Type == claimType)
                        .Select(c => c.Value).ToList();
        }

        private TokenResponse NewSiliconClientToken(string scope)
        {
            var tokenUrl = string.Format("{0}connect/token", Helpers.Appsettings.AuthUrl());
            _logger.Info("Getting a silicon client token at {0}", tokenUrl);
            var client = new TokenClient(tokenUrl, Helpers.Appsettings.SiliconClientId(), Helpers.Appsettings.SiliconClientSecret());

            var token = client.RequestClientCredentialsAsync(scope).Result;
            if (token.IsError) _logger.Error("Error getting Token for silicon Client: {0} ", token.Error);

            return token;
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