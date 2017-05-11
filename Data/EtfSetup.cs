using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Data.Migrations;

namespace Data
{
    public class EtfSetup
    {
        public static void InitDB()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<FrontendDbContext, Configuration>());
            // To not init db, use Database.SetInitializer<SchoolContext>(null);
            using (var db = new FrontendDbContext())
            {
                db.Database.Initialize(false); // trigger db init
            }
        }
    }
}