using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    public class SystemLayoutController : MyBaseController
    {
        public SystemLayoutController(ILogger logger, MakeStaticsMockable staticMocker) : base(logger)
        {

        }
        // GET: SystemLayout
        public ActionResult Index()
        {
            return View();
        }
    }
}