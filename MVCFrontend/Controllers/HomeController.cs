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
        private ILogger _logger = LogManager.CreateLogger(typeof(HomeController));
        public ActionResult Index()
        {
            _logger.Debug("homepage Index hit!");
            return View();
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


        [Authorize]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut(); //need to provide the token in order to get back here

            return Redirect("/");
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page is public.";

            return View();
        }
    }
}