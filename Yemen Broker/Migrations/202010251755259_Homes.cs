namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Homes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Homes",
                c => new
                    {
                        AdId = c.Long(nullable: false),
                        NumberOfFloors = c.Int(nullable: false),
                        DetailSystem = c.String(),
                        NumberOfLand = c.Int(nullable: false),
                        PlateNumber = c.Int(nullable: false),
                        StreetsArea = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AdId)
                .ForeignKey("dbo.Ads", t => t.AdId,cascadeDelete:true)
                .Index(t => t.AdId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Homes", "AdId", "dbo.Ads");
            DropIndex("dbo.Homes", new[] { "AdId" });
            DropTable("dbo.Homes");
        }
    }
}
