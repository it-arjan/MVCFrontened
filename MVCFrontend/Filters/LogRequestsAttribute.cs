using MVCFrontend.Helpers;
using MVCFrontend.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MvcFrontendData.Models;

namespace MVCFrontend.Filters
{
    public class LogRequestsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var mvcHandler = HttpContext.Current.Handler as MvcHandler;
            if (mvcHandler != null)
            {

                string username = HttpContext.Current.Request.IsAuthenticated
                                    ? ClaimsPrincipal.Current.GetClaim("name")
                                    : "Anonymous";

                // TODO: store combination of asp sessionId and IP (when ip is in ignore ip list)
                // to enable filtering the postbacks
                var logEntry = CreateApiLogEntryWithRequestData(HttpContext.Current.Request);
                if (!Appsettings.LogRequestIgnoreIpList().Contains(logEntry.Ip.Trim()))
                {
                    logEntry.AspSessionId= filterContext.HttpContext.Session.SessionID;

                    var db = MvcFrontendData.DbFactory.Db();
                    var dbg = db.GetEtfdb().Postbacks.Where(le => DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10);
                    logEntry.RecentContributions = username == "Anonymous" ? 0
                        : db.GetEtfdb().Postbacks.Where(le => DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10).Count();
                        
                    db.GetEtfdb().RequestLogEntries.Add(logEntry);
                    db.SaveChanges();
                }
            }

            base.OnActionExecuting(filterContext);
        }
        private RequestLogEntry CreateApiLogEntryWithRequestData(HttpRequest request)
        {

            return new RequestLogEntry
            {
                User = request.IsAuthenticated
                                        ? ClaimsPrincipal.Current.GetClaim("name")
                                        : "Anonymous",
                ContentType = request.ContentType,
                Ip = request.GetOwinContext().Request.RemoteIpAddress ?? "OwinContext.Request.RemoteIpAddress not set",
                Method = request.RequestType,
                Timestamp = DateTime.Now,
                Path = request.Path
            };
        }
    }
}