using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    public class ErrorController : MyBaseController
    {
        public ErrorController(ILogger logger, IMakeStaticsMockable injectMockMe):base(logger)
        {

        }
        // GET: Error
        public ActionResult Error_500()
        {
            Response.StatusCode = 500;
            return View();
        }
        public ActionResult Error_404()
        {
            Response.StatusCode = 404;
            return View();
        }
        public ActionResult Error()
        {
            return View();
        }
    }
}