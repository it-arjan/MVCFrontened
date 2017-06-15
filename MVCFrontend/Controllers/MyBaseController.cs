using MVCFrontend.Helpers;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Controllers
{
    public class MyBaseController : Controller
    {
        protected ILogger _logger;
        public MyBaseController(ILogger logger)
        {
            _logger = logger;
            _logger.SetLevel(Configsettings.LogLevel());
        }
    }
}