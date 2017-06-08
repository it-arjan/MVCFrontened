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
using MVCFrontend.Filters;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    public class HomeController : Controller
    {
        public HomeController(ILogger logger)
        {
            _logger = logger;
            _logger.SetLevel(Appsettings.LogLevel());
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
                Request.GetOwinContext().Authentication.SignOut();
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