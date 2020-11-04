namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModelValidationsAdded : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Ads", "AdDescribtion", c => c.String(nullable: false));
            AlterColumn("dbo.Homes", "DetailSystem", c => c.String(nullable: false));
            AlterColumn("dbo.Homes", "StreetsArea", c => c.String(nullable: false));
            AlterColumn("dbo.Shops", "StreetArea", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Shops", "StreetArea", c => c.String(maxLength: 200));
            AlterColumn("dbo.Homes", "StreetsArea", c => c.String());
            AlterColumn("dbo.Homes", "DetailSystem", c => c.String());
            AlterColumn("dbo.Ads", "AdDescribtion", c => c.String());
        }
    }
}
