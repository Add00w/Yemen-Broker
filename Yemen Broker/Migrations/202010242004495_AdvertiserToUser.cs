namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdvertiserToUser : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Ads", name: "AdvertiserId", newName: "UserId");
            RenameIndex(table: "dbo.Ads", name: "IX_AdvertiserId", newName: "IX_UserId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Ads", name: "IX_UserId", newName: "IX_AdvertiserId");
            RenameColumn(table: "dbo.Ads", name: "UserId", newName: "AdvertiserId");
        }
    }
}
