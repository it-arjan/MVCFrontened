using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyData;
using MyData.Models;

namespace MyData
{
    public class Db : IDb
    {
        public FrontendDbContext _etfDb;
        public Db()
        {
            _etfDb = new FrontendDbContext();
        }

        public void Add<T>(T data)
        {
            bool typeFound = false;
            if(typeof(T) == typeof(RequestLogEntry))
            {
                var x = data as RequestLogEntry;
                _etfDb.RequestLogEntries.Add(x);
                typeFound = true;
            }
            if (typeof(T) == typeof(PostbackData))
            {
                var x = data as PostbackData;
                _etfDb.Postbacks.Add(x);
                typeFound = true;
            }
            if (typeof(T) == typeof(IpSessionId))
            {
                var x = data as IpSessionId;
                _etfDb.IpSessionIds.Add(x);
                typeFound = true;
            }
            if (!typeFound) throw new Exception("You need to add type " + typeof(T).Name + " in this generic Add function" );
        }

        public void Remove<T>(T data)
        {
            bool typeFound = false;
            if (typeof(T) == typeof(RequestLogEntry))
            {
                var x = data as RequestLogEntry;
                _etfDb.RequestLogEntries.Remove(x);
                typeFound = true;
            }
            if (typeof(T) == typeof(PostbackData))
            {
                var x = data as PostbackData;
                _etfDb.Postbacks.Remove(x);
                typeFound = true;
            }
            if (typeof(T) == typeof(IpSessionId))
            {
                var x = data as IpSessionId;
                _etfDb.IpSessionIds.Remove(x);
                typeFound = true;
            }
            if (!typeFound) throw new Exception("You need to add type " + typeof(T).Name + " in this generic Add function");
        }

        public bool IpSessionIdExists(string sessionId, string ip)
        {
            return _etfDb.IpSessionIds.Where(I => I.SessionID == sessionId && I.Ip == ip).Any();
        }

        public void Commit()
        {
            _etfDb.SaveChanges();
        }

        public List<RequestLogEntry> GetRecentRequestLogs(int nr)
        {
            return _etfDb.RequestLogEntries.OrderByDescending(rq => rq.Timestamp).Take(nr).ToList();
        }

        public List<RequestLogEntry> GetRecentRequestLog(int nr, string SessionId)
        {
            return _etfDb.RequestLogEntries.Where(rq => rq.AspSessionId == SessionId).OrderByDescending(rq => rq.Timestamp).ToList();
        }

        public List<PostbackData> GetPostbacksFromToday()
        {
            return _etfDb.Postbacks.Where(pb => System.Data.Entity.DbFunctions.DiffDays(pb.End, DateTime.Now) < 1).ToList();
        }

        public bool SessionIdExists(string aspSessionId)
        {
            return _etfDb.IpSessionIds.Where(ips => ips.SessionID == aspSessionId).Any();
        }

        public RequestLogEntry FindRequestLog(int id)
        {
            return _etfDb.RequestLogEntries.Find(id);
        }

        public PostbackData FindPostback(int id)
        {
            return _etfDb.Postbacks.Find(id);
        }

        public IpSessionId FindIpSessionId(int id)
        {
            return _etfDb.IpSessionIds.Find(id);
        }

        public List<PostbackData> GetRecentPostbacks(int nr)
        {
            return _etfDb.Postbacks.OrderByDescending(c => c.End).Take(nr).ToList();
        }

        public List<PostbackData> GetRecentPostbacks(int nr, string SessionId)
        {
            return _etfDb.Postbacks.Where(pb => pb.AspSessionId==SessionId).OrderByDescending(c => c.Start).Take(nr).ToList();
        }

        public void Dispose()
        {
            _etfDb.Dispose();
        }
    }
}
