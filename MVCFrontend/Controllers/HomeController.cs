using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLogWrapper;
using System.Security.Claims;
using MVCFrontend.Helpers;
using MVCFrontend.Models;
using MVCFrontend.Extentions;
using MVCFrontend.Overrides.Filters;

namespace MVCFrontend.Controllers
{
    public class MakeStaticsMockable : IMakeStaticsMockable
    {
        [Authorize]
        public virtual void SignOut(HttpRequestBase request)
        {
            request.GetOwinContext().Authentication.SignOut();
        }
    }
    [LogRequests]
    public class HomeController : Controller
    {
        private IMakeStaticsMockable Mockme;
        public HomeController(ILogger logger, IMakeStaticsMockable injectMockMe)
        {
            _logger = logger;
            _logger.SetLevel(Configsettings.LogLevel());
            Mockme = injectMockMe;
        }
        private ILogger _logger;
        public ActionResult Index()
        {
            ViewBag.Message = User.Identity.IsAuthenticated
                ? "You are logged on."
                : "You are not logged on.";
            return View();
        }

        [HttpGet]
        public string AnonymousPing()
        {
            return "Ok";
        }
        
        public ActionResult Logout()
        {
            Session.Abandon();
            if (User.Identity.IsAuthenticated)
                Mockme.SignOut(Request);
            //else
            return Redirect("/");
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "You are logged on.";

            var model = new AboutModel();
            var cp = User as ClaimsPrincipal;
            model.Claims = cp.Claims;
            model.TokenSessionStart = Utils.ConvertUnixTimestampToCetTime(cp.GetClaimValue("auth_time"));
            model.TokenSessionEnd = Utils.ConvertUnixTimestampToCetTime(
                    Utils.GetClaimFromToken(cp.GetClaimValue("access_token"), "exp")
                    );


            return View(model);
        }
    }
}