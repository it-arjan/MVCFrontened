using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Data
{
    public class EtfSetup
    {
        public static void InitDB()
        {
            Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseAlways<FrontendDbContext>());
            // To not init db, use Database.SetInitializer<SchoolContext>(null);
            using (var db = new FrontendDbContext())
            {
                db.Database.Initialize(false); // trigger db init
            }
        }
    }
}