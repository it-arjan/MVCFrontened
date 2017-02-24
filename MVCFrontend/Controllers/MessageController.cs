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
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(MessageController));
        // GET: Message
        public ActionResult Index()
        {
            var model = new MessageViewModel();
            model.AjaxAccessToken = GetSiliconClientToken(Helpers.IdSrv3.ScopeMcvFrontEnd).AccessToken;
            model.SocketToken = Guid.NewGuid().ToString();
            return View("SendMessage", model);
        }

        public string AuthPing()
        {
            return "Silicon token valid";
        }

        public string ToRemoteQueue(string message, string socketToken)
        {
            string result = null;
            var model = new MessageViewModel();
            if (!string.IsNullOrEmpty(message.Trim()))
            {
                var token = GetSiliconClientToken(Helpers.IdSrv3.ScopeEntryQueueApi);
                if (!token.IsError)
                {

                    var apiUrl = Helpers.Appsettings.QueueApiUrl(); ;

                    var auth_header = string.Format("bearer {0}", token.AccessToken);
                    _logger.Debug(string.Format("Calling {0} with token: {1}", apiUrl, auth_header));

                    var easyHttp = new HttpClient();
                    easyHttp.Request.AddExtraHeader("Authorization", auth_header);
                    easyHttp.Request.Accept = HttpContentTypes.ApplicationJson;

                    var data = new PostObj();
                    data.MessageId = message;
                    data.PostBackUrl = string.Format("{0}/Message/Postback", Helpers.Appsettings.HostUrl());
                    data.SocketToken = socketToken;
                    data.UserName = GetNameFromPrincipal();

                    easyHttp.Post(apiUrl, data, "application/json");

                    model.ApiResult = new ApiResultModel();
                    model.ApiResult.Message = string.Format("Queue Api returned {0} and '{1}'", easyHttp.Response.StatusCode, JsonConvert.DeserializeObject<EntryQApiResult>(easyHttp.Response.RawText).message);
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

        private string GetNameFromPrincipal()
        {
            string result = null;
            var claimsprincipal = User as ClaimsPrincipal;

            result = claimsprincipal
                        .Claims.Where(c => c.Type == ClaimTypes.Name)
                        .Select(c => c.Value).SingleOrDefault();
            return result;
        }

        private TokenResponse GetSiliconClientToken(string scope)
        {
            var tokenUrl = string.Format("{0}connect/token", Helpers.Appsettings.AuthUrl());
            _logger.Debug("Getting a silicon client token at {0}", tokenUrl);
            var client = new TokenClient(tokenUrl, Helpers.Appsettings.SiliconClientId(), Helpers.Appsettings.SiliconClientSecret());

            var token = client.RequestClientCredentialsAsync(scope).Result;
            if (token.IsError) _logger.Error("Error getting Token for silicon Client: {0} ", token.Error);
            else _logger.Debug("Token obtained");

            return token;
        }
        [Authorize]
        [HttpPost]
        public ActionResult Postback(string json)
        {
            //var postedstring = await Request.Form.
            _logger.Info("postback called");

           _logger.Debug("receieved '{0}'", json);
            return new HttpStatusCodeResult(HttpStatusCode.Gone);
        }
        private class EntryQApiResult
        {
            public string message { get; set; }
        }
        private class PostObj
        {
            public string MessageId { get; set; }
            public string PostBackUrl { get; set; }
            public string SocketToken { get; set; }
            public string UserName { get; set; }
        }
    }
}