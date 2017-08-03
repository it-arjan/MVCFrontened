using MVCFrontend.Controllers;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCFrontend.Overrides
{
    public class CustomControllerFactory : DefaultControllerFactory
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(CustomControllerFactory), "Error");
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            IController result = null;
            if (controllerType == null)
            {
                var msg = String.Format("no controller found at path {0}", requestContext.HttpContext.Request.Path);
                _logger.Error("404! {0}", msg);
                requestContext.HttpContext.Response.StatusCode = 404; // without this, statuscode is OK in Application_Error
                throw new HttpException(404, msg);
            }
            ILogger logger = LogManager.CreateLogger(controllerType);
            IMakeStaticsMockable staticsMockable = new MakeStaticsMockable();

            result = Activator.CreateInstance(controllerType, new object[] { logger, staticsMockable }) as Controller;
            return result;
            
        }
    }
}