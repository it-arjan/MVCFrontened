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

                if (IgnoreIp())
                {
                    LinkSessionIdIp(filterContext);
                }
                else
                {
                    var db = MyData.DbFactory.Db();
                    if (!IgnoreSessionId(db, filterContext.HttpContext.Session.SessionID))
                    {
                        var logEntry = CreateApiLogEntryWithRequestData(HttpContext.Current.Request);
                        logEntry.AspSessionId = filterContext.HttpContext.Session.SessionID;

                        var dbg = db.GetEtfdb().Postbacks.Where(le => DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10);
                        logEntry.RecentContributions = username == "Anonymous" ? 0
                            : db.GetEtfdb().Postbacks.Where(le => DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10).Count();

                        db.GetEtfdb().RequestLogEntries.Add(logEntry);
                        db.SaveChanges();
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void LinkSessionIdIp(ActionExecutingContext filterContext)
        {
            var db = MyData.DbFactory.Db();
            var sessionID = filterContext.HttpContext.Session.SessionID;
            var remoteIpAddress = HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress;

            if (db.GetEtfdb().IpSessionIds.Where(I => I.SessionID == sessionID && I.Ip == remoteIpAddress).Any())
            {
                var x = new IpSessionId { SessionID = sessionID, Ip = remoteIpAddress };
                db.Add(x);
                db.SaveChanges();
            }
        }
        private bool IgnoreSessionId(MyData.IDb db, string sessionId)
        {
            return db.GetEtfdb().IpSessionIds.Where(I => I.SessionID == sessionId).Any();
        }
        private bool IgnoreIp()
        {
            return Appsettings.LogRequestIgnoreIpList().Contains(HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress);
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