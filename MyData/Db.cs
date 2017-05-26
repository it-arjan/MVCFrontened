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

        public FrontendDbContext GetEtfdb()
        {
            return _etfDb;
        }

        public void SaveChanges()
        {
            _etfDb.SaveChanges();
        }
    }
}
