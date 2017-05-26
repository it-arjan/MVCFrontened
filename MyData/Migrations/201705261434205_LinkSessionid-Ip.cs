namespace MyData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkSessionidIp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.IpSessionIds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SessionID = c.String(nullable: false),
                        Ip = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.IpSessionIds");
        }
    }
}
