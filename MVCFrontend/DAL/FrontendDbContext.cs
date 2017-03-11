using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using MVCFrontend.Models;

namespace MVCFrontend.DAL
{
    public class FrontendDbContext : DbContext
    {
        public FrontendDbContext() : base("FrontendDbContext")
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<PostbackData> Postbacks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // possibility to set some conventions
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}