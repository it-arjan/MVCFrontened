using EasyHttp.Http;
using MVCFrontend.Helpers;
using MVCFrontend.Models;
using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using MVCFrontend.Extentions;
using MVCFrontend.Overrides.Filters;
using System.Net;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class ServiceSelectionController : MyBaseController
    {
        // GET: SelectedServices
        public ServiceSelectionController(ILogger logger, IMakeStaticsMockable injectMockMe) : base(logger)
        {
        }

        public ActionResult Index()
        {
            // Get the actual service config
            var result = Post(GetServiceConfigdata());
            var model = CreateModel(result);

            return PartialView("~/views/Message/ServiceSelection-Styled.cshtml", model);
        }
        private void LogValuesToFile(FormCollection values)
        {
            var result = string.Empty;
            foreach (string key in values.Keys)
            {
                result += string.Format("\n{0} : {1}", key, values[key]);
            }
            _logger.Debug("Posted values: {0}", result);
        }
        [HttpPost]
        public ActionResult Submit(FormCollection values)
        {
            List<int> result;
            var list = GetFirstThreeSelectedvalues(values);
            if (list == null)
            {
                WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "Less the 3 web services selected, nothing is configured.");
                result = Post(GetServiceConfigdata());
            }
            else
            {
                CmdPostData data = new CmdPostData();
                data.cmdType = "SetServiceConfig";
                data.Service1Nr = list[0];
                data.Service2Nr = list[1];
                data.Service3Nr = list[2];
                data.SocketToken = ClaimsPrincipal.Current.GetClaimValue("qm_socket_id");
                data.UserName = ClaimsPrincipal.Current.GetClaimValue("given_name");
                data.ApiFeedToken = ClaimsPrincipal.Current.GetClaimValue("api_feed_socket_id");
                data.AspSessionId = Session.SessionID;
                data.LogDropRequest = !RequestLog.IgnoreIp(HttpContext.Request.GetOwinContext().Request.RemoteIpAddress);

                result = Post(data);
                if (list.Contains("7"))
                {
                    WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "You have selected postal code lookup. Post a message like \"1234Ab 12\" and We'll lookup some public info about this property.\n\n");
                }
                else // default message
                {
                    WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "The first three services in your selection are configured");
                }
            }
            var model = CreateModel(result);

            return PartialView("~/views/Message/ServiceSelection-Styled.cshtml", model);
        }

        private CmdPostData GetServiceConfigdata()
        {
            CmdPostData data = new CmdPostData();
            data.cmdType = "GetServiceConfig";
            data.Service1Nr = string.Empty;
            data.Service2Nr = string.Empty;
            data.Service3Nr = string.Empty;
            data.SocketToken = ClaimsPrincipal.Current.GetClaimValue("qm_socket_id");
            data.UserName = ClaimsPrincipal.Current.GetClaimValue("given_name");
            data.ApiFeedToken = ClaimsPrincipal.Current.GetClaimValue("api_feed_socket_id");
            data.AspSessionId = Session.SessionID;
            data.LogDropRequest = !RequestLog.IgnoreIp(HttpContext.Request.GetOwinContext().Request.RemoteIpAddress);
            return data;
        }

        private List<string> GetFirstThreeSelectedvalues(FormCollection values)
        {
            var result = new List<string>();
            var serviceSelectedCounter = 0;
            var serviceNr = 0;
            foreach (string key in values.Keys)
            {
                if (key.StartsWith("service"))
                {
                    serviceNr++;
                    if (values[key].ToLower() == "true")
                    {
                        serviceSelectedCounter++;
                        result.Add(serviceNr.ToString());
                        if (serviceSelectedCounter == 3) break;
                    }
                }
            }
            if (result.Count != 3)
            {
                _logger.Warn("Less then 3 selected services in posted values, returning null.");
                return null;
            }
            else _logger.Debug("Found 3 selected services in posted values .. as expected.");

            return result;
        }

        private ServiceSelectionModel CreateModel(List<int> activeServiceList)
        {
            var result = new ServiceSelectionModel();

            // if (!activeServiceList.Any) =>Something is WRONG, inform user

            result.Services.Add(new WebService { Id = 1, Prompt = "Fake", Selected = activeServiceList.Contains(1) });
            result.Services.Add(new WebService { Id = 2, Prompt = "Ms WebApi2", Selected = activeServiceList.Contains(2) });
            result.Services.Add(new WebService { Id = 3, Prompt = "Real 404", Selected = activeServiceList.Contains(3) });
            result.Services.Add(new WebService { Id = 4, Prompt = "Real Crash", Selected = activeServiceList.Contains(4) });
            result.Services.Add(new WebService { Id = 5, Prompt = "Ordercheck(NancyFx)", Selected = activeServiceList.Contains(5) });
            //result.Services.Add(new WebService { Id = 6, Prompt = "Servicesstack", Selected = activeServiceList.Contains(6) });
            result.Services.Add(new WebService { Id = 7, Prompt = "Postal Code lookup", Selected = activeServiceList.Contains(7) });
            //result.Services.Add(new WebService { Id = 8, Prompt = "Wcf service", Selected = activeServiceList.Contains(8) });

            return result;
        }

        private List<int> Post(CmdPostData data = null)
        {
            var result = new List<int>();

            var easyHttp = new HttpClient();
            var apiUrl = string.Format("{0}/api/CmdQueue", Configsettings.EntrypointUrl());
            var auth_header = string.Format("bearer {0}", ClaimsPrincipal.Current.GetClaimValue("ajax_cors_token"));
            easyHttp.Request.AddExtraHeader("Authorization", auth_header);
            easyHttp.Request.Accept = HttpContentTypes.ApplicationJson;

            try
            {
                _logger.Debug("Settting service config at {0} with data {1}", apiUrl, JsonConvert.SerializeObject(data));
                easyHttp.Post(apiUrl, data, HttpContentTypes.ApplicationJson);
                if (easyHttp.Response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.Debug("Settting service config returned = {0}", easyHttp.Response.RawText);
                    result = JsonResponseToIntList(easyHttp.Response.RawText);
                }
                else if (easyHttp.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "The Queue Manager denied access, is your CORS token expired?");
                }
                else
                {
                    WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "The Queue Manager returned " + easyHttp.Response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception getting or setting the service config, poss connection issue with Entrypoint.");
                _logger.Error("{0}", ex.ToString());
            }
            return result;
        }

        private List<int> JsonResponseToIntList(string rawResponse)
        {
            var result = new List<int>();
            var anoType = new { Message = "", CmdResult = "" };
            anoType = JsonConvert.DeserializeAnonymousType(rawResponse, anoType);
            if (anoType.CmdResult != null)
            {
                var int_arr = anoType.CmdResult.Split(',');

                result.Add(Convert.ToInt16(int_arr[0]));
                result.Add(Convert.ToInt16(int_arr[1]));
                result.Add(Convert.ToInt16(int_arr[2]));
            }
            else //ERROR, return empty list 
            {
                WebNotification.Send(ClaimsPrincipal.Current.GetClaimValue("notification_socket_id"), "An error ocurred parsing the json of the server response");
            }
            return result;
        }

    }

    public class CmdPostData
    {
        public string cmdType;
        public string Service1Nr { get; set; }
        public string Service2Nr { get; set; }
        public string Service3Nr { get; set; }
        public string SocketToken { get; set; }
        public string UserName { get; set; }
        public string ApiFeedToken { get; set; }
        public string AspSessionId { get; set; }
        public bool LogDropRequest { get; set; }
    }
}