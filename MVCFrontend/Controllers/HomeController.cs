using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLogWrapper;
using System.Security.Claims;

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
            if (User.Identity.IsAuthenticated)
                Request.GetOwinContext().Authentication.SignOut(); //need to provide the token in order to get back here

            return Redirect("/");
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "You are logged on.";

            var model = new AboutModel();
            var claimsprincipal = User as ClaimsPrincipal;
            model.Claims = claimsprincipal.Claims;

            return View(model);
        }
    }
}