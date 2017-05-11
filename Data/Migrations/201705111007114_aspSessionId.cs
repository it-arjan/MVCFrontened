namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aspSessionId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PostbackDatas", "AspSessionId", c => c.String());
            AddColumn("dbo.RequestLogEntries", "AspSessionId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RequestLogEntries", "AspSessionId");
            DropColumn("dbo.PostbackDatas", "AspSessionId");
        }
    }
}
