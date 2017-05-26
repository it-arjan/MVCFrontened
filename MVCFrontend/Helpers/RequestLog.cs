using MVCFrontend.Extentions;
using MyData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class RequestLog
    {
        public static void StoreRequestForSessionId(MyData.IDb db, string AspSessionId)
        {
            string username = HttpContext.Current.Request.IsAuthenticated
                              ? ClaimsPrincipal.Current.GetClaim("name")
                              : "Anonymous";
            if (!IgnoreSessionId(db, AspSessionId))
            {

                var logEntry = RequestLog.CreateApiLogEntryWithRequestData(HttpContext.Current.Request);
                logEntry.AspSessionId = AspSessionId;
                var dbg = db.GetEtfdb().Postbacks.Where(le => System.Data.Entity.DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10);
                logEntry.RecentContributions = username == "Anonymous" ? 0
                    : db.GetEtfdb().Postbacks.Where(le => System.Data.Entity.DbFunctions.DiffMinutes(DateTime.Now, le.End) <= 10).Count();

                db.GetEtfdb().RequestLogEntries.Add(logEntry);
                db.SaveChanges();
            }
        }

        public static RequestLogEntry CreateApiLogEntryWithRequestData(HttpRequest request)
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
        public static bool IgnoreSessionId(MyData.IDb db, string sessionId)
        {
            return db.GetEtfdb().IpSessionIds.Where(I => I.SessionID == sessionId).Any();
        }
    }
}