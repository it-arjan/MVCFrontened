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
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404,
                    String.Format(
                   "no controller found at path {0}",
                   requestContext.HttpContext.Request.Path));
            }
            ILogger logger = LogManager.CreateLogger(controllerType);
            IMakeStaticsMockable staticsMockable = new MakeStaticsMockable();
            IController controller = Activator.CreateInstance(controllerType, new object[] { logger, staticsMockable }) as Controller;
            return controller;
            
        }
    }
}