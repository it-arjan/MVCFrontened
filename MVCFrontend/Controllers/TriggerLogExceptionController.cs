using MVCFrontend.Overrides.Filters;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    public class TriggerLogExceptionController : MyBaseController
    {
        public TriggerLogExceptionController(ILogger logger, IMakeStaticsMockable injectMockMe): base(logger)
        {

        }
        // GET: TriggerLogException
        public ActionResult Index()
        {
            return View();
        }
    }
}