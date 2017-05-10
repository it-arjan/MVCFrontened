using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLogWrapper;
using MVCFrontend.Extentions;
using Data;
using Data.Models;
using MVCFrontend.Helpers;
using System.Security.Claims;
using System.Net.Http;
using System.Data.Entity;

namespace MVCFrontend
{
    public class RequestLogModule : IHttpModule
    {
        private ILogger _logger = LogManager.CreateLogger(typeof(RequestLogModule), LogManager.ILogLevel.Info);
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.LogRequest += LogEvent;
        }

        private void LogEvent(object src, EventArgs args)
        {
            if (HttpContext.Current.CurrentNotification == RequestNotification.LogRequest)
            {
                var mvcHandler = HttpContext.Current.Handler as MvcHandler;
                if (mvcHandler != null)
                {

                    string username = HttpContext.Current.Request.IsAuthenticated 
                                        ? ClaimsPrincipal.Current.GetClaim("name")
                                        : "Anonymous";

                    var logEntry = CreateApiLogEntryWithRequestData(HttpContext.Current.Request);
                    if (!Appsettings.LogRequestIgnoreIpList().Contains(logEntry.Ip.Trim()))
                    {
                        var db = Data.DbFactory.Db();
                        logEntry.RecentContributions = username != "Anonymous" 
                            ? db.GetEtfdb().Postbacks.Where(le => DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10).Count() 
                            : 0;
                        db.GetEtfdb().RequestLogEntries.Add(logEntry);
                        db.SaveChanges();
                    }
                }

            }
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