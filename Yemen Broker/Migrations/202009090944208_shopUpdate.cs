namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shopUpdate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Ads", "AdLocation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Ads", "AdLocation", c => c.String(nullable: false));
        }
    }
}
