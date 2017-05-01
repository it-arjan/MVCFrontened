using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MVCFrontend.DAL;
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
            RegisterModule(typeof(RequestLogModule));
        }
        // adding an empty Session_Start solves a cookie issue causing endless redict on auth success
        protected void Session_Start() {
            Session["SessionStartTime"]= DateTime.Now;
        }
        protected void Session_End()
        {
            _logger.Debug("Session_End: Clearing the session");
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
