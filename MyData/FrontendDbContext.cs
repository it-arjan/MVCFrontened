using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using MyData.Models;
using System.Data.Entity.Infrastructure;
using MyData.Migrations;

namespace MyData
{
    public class FrontendDbContext : DbContext
    {
        public FrontendDbContext() : base("FrontendDbContext")
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<PostbackData> Postbacks { get; set; }
        public DbSet<RequestLogEntry> RequestLogEntries { get; set; }
        public DbSet<IpSessionId> IpSessionIds { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // possibility to set some conventions
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}