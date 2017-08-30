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

        protected override void OnException(ExceptionContext filterContext)
        {
            var msg = filterContext.Exception != null ? filterContext.Exception.ToString() : "no exception message";
            _logger.Error("MyBaseController: Exception! Message = {0}", msg);

            // Let IIS serve the custom error page using httpErrors tag
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.StatusCode = 500;
        }


    }
}