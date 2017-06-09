using MVCFrontend.Controllers;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Overrides
{
    public class CustomControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            ILogger logger = LogManager.CreateLogger(controllerType);
            IMakeStaticsMockable staticsMockable = new MakeStaticsMockable();
            IController controller = Activator.CreateInstance(controllerType, new object [] { logger, staticsMockable }) as Controller;
            return controller;
        }
    }
}