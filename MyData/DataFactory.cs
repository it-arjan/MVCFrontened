using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyData
{
    public enum MyDbType { EtfDb, NancyApiDb, WebApiOdataDb };
    public class DataFactory
    {
        private MyDbType _dbType;
        public DataFactory(MyDbType type)
        {
            _dbType = type;
        }
        public IData Db()
        {
            return new Etf.EntityFrameworkDb();
        }
        public IDataSetup DbSetup()
        {
            return new Etf.EtfSetup();
        }
    }
}
