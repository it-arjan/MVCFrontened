using MVCFrontend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCFrontend.Overrides.Filters
{
    public class HandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        // This needs cutomErrors to be On
        // #DoNotUse
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            string viewname = ErrorHandling.GetviewForStatusCode(filterContext.HttpContext.Response.StatusCode);
            filterContext.Result = new ViewResult
            {
                ViewName = viewname
            };
        }
    }
}