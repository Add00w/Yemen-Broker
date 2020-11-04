 namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CityId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Ads", "City_Id", "dbo.CityModels");
            DropIndex("dbo.Ads", new[] { "City_Id" });
            RenameColumn(table: "dbo.Ads", name: "City_Id", newName: "CityId");
            AlterColumn("dbo.Ads", "CityId", c => c.Int(nullable: false));
            CreateIndex("dbo.Ads", "CityId");
            AddForeignKey("dbo.Ads", "CityId", "dbo.CityModels", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ads", "CityId", "dbo.CityModels");
            DropIndex("dbo.Ads", new[] { "CityId" });
            AlterColumn("dbo.Ads", "CityId", c => c.Int());
            RenameColumn(table: "dbo.Ads", name: "CityId", newName: "City_Id");
            CreateIndex("dbo.Ads", "City_Id");
            AddForeignKey("dbo.Ads", "City_Id", "dbo.CityModels", "Id");
        }
    }
}
