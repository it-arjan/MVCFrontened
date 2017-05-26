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
using MVCFrontend.Filters;
using MVCFrontend.WebSocket;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class ServiceSelectionController : Controller
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(ServiceSelectionController), Appsettings.LogLevel());
        // GET: SelectedServices
        public ActionResult Index()
        {
            // Get the actual service config
            var result = ServiceConfig(HttpMethod.GET);
            var model = CreateModel(result);

            return PartialView("~/views/Message/ServiceSelection-Styled.cshtml", model);
        }
        private void LogValues(FormCollection values)
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
            LogValues(values);
            if (list == null)
            {
                _logger.Warn("less then 3 values selected, doing a GETserviceConfig instead.");
                result = ServiceConfig(HttpMethod.GET);
            }
            else
            {
                CmdPostData data = new CmdPostData();
                data.cmdType = values["cmdType"];
                data.Service1Nr = list[0];
                data.Service2Nr = list[1];
                data.Service3Nr = list[2];
                data.SocketToken = values["SocketToken"];

                result = ServiceConfig(HttpMethod.POST, data);
            }
            var model = CreateModel(result);

            return PartialView("~/views/Message/ServiceSelection-Styled.cshtml", model);
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
            result.Services.Add(new WebService { Id = 5, Prompt = "Nancy PC lookup", Selected = activeServiceList.Contains(5) });
            result.Services.Add(new WebService { Id = 6, Prompt = "Servicesstack api", Selected = activeServiceList.Contains(6) });
            result.Services.Add(new WebService { Id = 7, Prompt = "Wcf service", Selected = activeServiceList.Contains(7) });

            return result;
        }

        private List<int> ServiceConfig(HttpMethod method, CmdPostData data = null)
        {
            var result = new List<int>();

            var apiUrl = method == HttpMethod.GET
                ? string.Format("{0}/api/CmdQueue/{1}/GetServiceConfig", Appsettings.EntrypointUrl(), ClaimsPrincipal.Current.GetClaim("qm_socket_id") )
                : string.Format("{0}/api/CmdQueue", Appsettings.EntrypointUrl());

            var auth_header = string.Format("bearer {0}", ClaimsPrincipal.Current.GetClaim("ajax_cors_token"));
            var easyHttp = new HttpClient();

            easyHttp.Request.AddExtraHeader("Authorization", auth_header);
            easyHttp.Request.Accept = HttpContentTypes.ApplicationJson;

            try
            {
                if (method == HttpMethod.GET)
                {
                    _logger.Debug("Getting service config at {0}", apiUrl);
                    easyHttp.Get(apiUrl);
                    _logger.Debug("Getting service config returned = {0}", easyHttp.Response.RawText);
                }
                else if (method == HttpMethod.POST)
                {
                    _logger.Debug("Settting service config at {0} with data {1}", apiUrl, JsonConvert.SerializeObject(data));
                    easyHttp.Post(apiUrl, data, HttpContentTypes.ApplicationJson);
                    _logger.Debug("Settting service config returned = {0}", easyHttp.Response.RawText);
                }
                else throw new Exception("Break: invalid method" + method);

                result = JsonResponseToIntList(easyHttp.Response.RawText);
                WebNotification.Send(ClaimsPrincipal.Current.GetClaim("notification_socket_id"), "The first three selected webservices are configured.");
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
            //else ERROR, return empty list todo: build & use message system
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
    }
}