using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data;
using Data.Models;
namespace NancyApi.Modules
{
    public class RequestLogModule : Nancy.NancyModule
    {
        public RequestLogModule() : base("api/nancy/requestlogs")
        {
            Get["/"] = _ => Hello(_.SessionId);
            Get["/{SessionId}"]=  _ => GetLogs(_.SessionId);
        }
        private List<RequestLogEntry> GetLogs(string SessionId)
        {
            var db = Data.DbFactory.Db();
            return db.GetEtfdb().RequestLogEntries.Where(rq => rq.AspSessionId == SessionId).ToList();
        }
        private string Hello(string SessionId)
        {
            return "Helloooo " + SessionId;
        }
    }
}