using MVCFrontend.Helpers;
using MVCFrontend.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MyData.Models;
using MyData;
using NLogWrapper;

namespace MVCFrontend.Overrides.Filters
{
    // [HandleError] error filters not work with error in Filters
    public class LogRequestsAttribute : ActionFilterAttribute
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(LogRequestsAttribute), Helpers.Configsettings.LogLevel());

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var mvcHandler = HttpContext.Current.Handler as MvcHandler;
            if (mvcHandler != null)
            {
                try
                {
                    ThrowIfTriggerLogExceptionRequest();
                    if (!RequestLog.IgnoreIp(HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress))
                    {
                        IdSrv3.EnsureTokenClaimIsValid(IdSrv3.ClaimApiToken);
                        var db = new DataFactory(MyDbType.ApiDbNancy).Db(
                            Configsettings.DataApiUrl(),
                            ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiToken),
                            ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess),
                           ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiFeedId)
                            );

                        var AspSessionId = filterContext.HttpContext.Session.SessionID;
                        // todo, going async here gets us into trouble
                        RequestLog.StoreRequestForSessionId(db, AspSessionId);
                        db.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Log Exception! {0}", ex.ToString());
                    // swallow or let IIS handle custom error page
                    if (!Configsettings.SwalloWLogExceptions()) filterContext.HttpContext.Response.StatusCode = 500;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void ThrowIfTriggerLogExceptionRequest()
        {
            if (HttpContext.Current.Request.Path.Contains("TriggerLogException"))
                throw new Exception("programmed Log Exception!");
        }
    }
}