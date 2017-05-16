using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcFrontendData;
using MvcFrontendData.Models;

namespace MvcFrontendData
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
            if(typeof(T) == typeof(RequestLogEntry))
            {
                var x = data as RequestLogEntry;
                _etfDb.RequestLogEntries.Add(x);
            }
            if (typeof(T) == typeof(PostbackData))
            {
                var x = data as PostbackData;
                _etfDb.Postbacks.Add(x);
            }
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
