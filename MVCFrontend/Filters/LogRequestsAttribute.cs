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
                if (IgnoreIp())
                {
                    LinkSessionIdIp(filterContext);
                }
                else
                {
                    var db = MyData.DbFactory.Db();
                    var AspSessionId = filterContext.HttpContext.Session.SessionID;

                    RequestLog.StoreRequestForSessionId(db, AspSessionId);
                    db.Dispose();
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void LinkSessionIdIp(ActionExecutingContext filterContext)
        {
            var db = MyData.DbFactory.Db();
            var sessionID = filterContext.HttpContext.Session.SessionID;
            var remoteIpAddress = HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress;

            if (!db.IpSessionIdExists(sessionID,remoteIpAddress))
            {
                var x = new IpSessionId { SessionID = sessionID, Ip = remoteIpAddress };
                db.Add(x);
                db.Commit();
                db.Dispose();
            }
        }

        private bool IgnoreIp()
        {
            return Appsettings.LogRequestIgnoreIpList().Contains(HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress);
        }
 
    }
}