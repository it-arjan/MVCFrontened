using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLogWrapper;
using System.Security.Claims;
using MVCFrontend.Helpers;
using MVCFrontend.Models;

namespace MVCFrontend.Controllers
{
    public class HomeController : Controller
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(HomeController), Helpers.Appsettings.LogLevel());
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
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
            model.Claims = ClaimsPrincipal.Current.Claims;
            model.TokenSessionStart = Utils.ConvertUnixTimestampToCetTime(ClaimsPrincipal.Current.GetClaim("auth_time"));
            model.TokenSessionEnd = Utils.ConvertUnixTimestampToCetTime(
                    Utils.GetClaimFromToken(ClaimsPrincipal.Current.GetClaim("access_token"), "exp")
                    );


            return View(model);
        }
    }
}