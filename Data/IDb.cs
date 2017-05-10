using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using Data;

namespace Data
{
    public interface IDb
    {
        void Add<T>(T data);
        FrontendDbContext GetEtfdb();
        void SaveChanges();
    }
}
