using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcFrontendData.Models;
using MvcFrontendData;

namespace MvcFrontendData
{
    public interface IDb
    {
        void Add<T>(T data);
        FrontendDbContext GetEtfdb();
        void SaveChanges();
    }
}
