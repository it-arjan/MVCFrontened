using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MVCFrontend.DAL;
using MVCFrontend;

[assembly: PreApplicationStartMethod(typeof(MvcApplication), "RegisterAdditionalModules")]
namespace MVCFrontend
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterAdditionalModules()
        {
            HttpApplication.RegisterModule(typeof(RequestLogModule));
        }
        // adding an empty Session_Start solves a cookie issue causing endless redict on auth success
        protected void Session_Start() { }
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
