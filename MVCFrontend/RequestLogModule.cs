﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLogWrapper;
using MVCFrontend.Helpers;
using System.Security.Claims;

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
                if ((MvcHandler)HttpContext.Current.Handler != null)
                {
                    string username = HttpContext.Current.Request.IsAuthenticated 
                                        ? ClaimsPrincipal.Current.NameClaim()
                                        : "Anonymous";

                    _logger.Info("Request for {0} by {1}", 
                                                HttpContext.Current.Request.Url, username);
                }

            }
        }
    }
}