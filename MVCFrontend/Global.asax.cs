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

[assembly: PreApplicationStartMethod(typeof(MvcApplication), "PreAppStartRegisterModules")]
namespace MVCFrontend
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ILogger _logger = LogManager.CreateLogger(typeof(MvcApplication), Appsettings.LogLevel());

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
            _logger.Debug("Session_End: Session started {0} and ends '{1}', diff (secs) = '{2}'", 
                start.ToLongTimeString(), end.ToLongTimeString(), (end-start).TotalSeconds
                );
            Session.Clear();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            EtfSetup.InitDB();

            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = Helpers.IdSrv3.UniqueClaimOfAntiForgeryToken;
        }
        
    }
}
