using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MyData;
using MVCFrontend;
using MVCFrontend.Helpers;
using NLogWrapper;
using MVCFrontend.Overrides;

[assembly: PreApplicationStartMethod(typeof(MvcApplication), "PreAppStartRegisterModules")]
namespace MVCFrontend
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ILogger _logger = LogManager.CreateLogger(typeof(MvcApplication), Configsettings.LogLevel());

        public static void PreAppStartRegisterModules()
        {
            //RegisterModule(typeof(RequestLogModule));
        }

        // adding an empty Session_Start solves a cookie issue causing endless redict on auth success
        protected void Session_Start()
        {
            _logger.Debug("Session_Start!");
            Session["asp_session_start_time"]= DateTime.Now;
            Session["asp_session_exp_time"] = DateTime.Now.AddMinutes(Session.Timeout);
        }
        
        protected void Session_End()
        {
            var start = Convert.ToDateTime(Session["asp_session_start_time"]);
            var end = DateTime.Now;
            _logger.Debug("Session_End: Session started {0} and ends '{1}', duration (secs) = '{2}'", 
                start.ToLongTimeString(), end.ToLongTimeString(), (end-start).TotalSeconds
                );
            Session.Clear();
        }

        private void CheckHealth()
        {
            _logger.Info("Checking config settings..");
            _logger.Info("Running under: Environment.UserName= {0}, Environment.UserDomainName= {1}", Environment.UserName, Environment.UserDomainName);
            SettingsChecker.CheckPresenceAllPlainSettings(typeof(Configsettings));

            _logger.Info("all requried config settings seem present..");
            _logger.Info("Url = {0}", Configsettings.HostUrl());
            _logger.Info("Socket server Url = {0}", Configsettings.SocketServerUrl());
            _logger.Info("Auth server Url= {0}", Configsettings.AuthUrl());
            _logger.Info("..done with config checks.");
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //only for etf dbtype new MyData.DataFactory(MyDbType.ApiDbNancy).DbSetup();
            ControllerBuilder.Current.SetControllerFactory(new CustomControllerFactory());

            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = Helpers.IdSrv3.UniqueClaimOfAntiForgeryToken;

            CheckHealth();
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // only handles exception that don't have filterContext.ExceptionHandled = true
            var raisedException = Server.GetLastError();
            _logger.Error("Unhandled exception: {0}", raisedException != null? raisedException.ToString(): "no message");
            if (Response.StatusCode == 404)
            {
                Response.Redirect("/error/error_404");
            }
            else
            Response.Redirect("/static/application_error.html");
            // Process exception
        }
    }
}
