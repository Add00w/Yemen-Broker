namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nn : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Homes", "StreetsArea", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Homes", "StreetsArea", c => c.String(nullable: false));
        }
    }
}
