﻿using MVCFrontend.Helpers;
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

        private bool IgnoreIp()
        {
            return Appsettings.LogRequestIgnoreIpList().Contains(HttpContext.Current.Request.GetOwinContext().Request.RemoteIpAddress);
        }
 
    }
}