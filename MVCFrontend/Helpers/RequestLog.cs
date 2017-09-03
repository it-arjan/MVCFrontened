using MVCFrontend.Extentions;
using MyData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class RequestLog
    {
        public static void StoreRequestForSessionId(MyData.IData db, string AspSessionId)
        {
            string username = HttpContext.Current.Request.IsAuthenticated
                              ? ClaimsPrincipal.Current.GetClaimValue("name")
                              : "Anonymous";

            var logEntry = CreateApiLogEntryWithRequestData(HttpContext.Current.Request);
            logEntry.AspSessionId = AspSessionId;

            if (username == "Anonymous")
                logEntry.RecentContributions = 0;
            else
            {
                // todo return count from method
                var result = db.GetPostbacksFromToday();
                logEntry.RecentContributions = result.Count();
            }
            db.AddRequestlog(logEntry);
            db.Commit();
        }

        public static RequestLogEntry CreateApiLogEntryWithRequestData(HttpRequest request)
        {

            return new RequestLogEntry
            {
                User = request.IsAuthenticated
                                        ? ClaimsPrincipal.Current.GetClaimValue("name")
                                        : "Anonymous",
                ContentType = request.ContentType,
                Ip = request.GetOwinContext().Request.RemoteIpAddress ?? "OwinContext.Request.RemoteIpAddress not set",
                Method = request.RequestType,
                Timestamp = DateTime.Now,
                Path = request.Path
            };
        }

        public static bool IgnoreIp(string ip)
        {
            return Configsettings.LogRequestIgnoreIpList().Contains(ip);
        }

    }
}