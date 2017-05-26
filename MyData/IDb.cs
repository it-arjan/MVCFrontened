using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyData.Models;
using MyData;

namespace MyData
{
    public interface IDb
    {
        void Add<T>(T data);
        FrontendDbContext GetEtfdb();
        void SaveChanges();
    }
}
