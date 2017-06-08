using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Extensions
{
    public class CustomControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            ILogger logger = LogManager.CreateLogger(controllerType);
            IController controller = Activator.CreateInstance(controllerType, new[] { logger }) as Controller;
            return controller;
        }
    }
}