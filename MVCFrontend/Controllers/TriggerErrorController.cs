using MVCFrontend.Overrides.Filters;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    [Authorize]
    [LogRequests]
    public class TriggerErrorController : MyBaseController
    {
        public TriggerErrorController(ILogger logger, IMakeStaticsMockable injectMockMe) : base(logger)
        {

        }
        // GET: TriggerError
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Error500()
        {
            // this does not set the status code: 
            // throw new HttpException(500, "programmed 500 error");
            //ControllerContext.HttpContext.Response.StatusCode = 500;
            throw new Exception("programmed 500 error");
            //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
        public ActionResult Error404()
        {
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        
    }
}