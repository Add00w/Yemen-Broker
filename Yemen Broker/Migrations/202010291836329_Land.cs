namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Land : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Lands",
                c => new
                    {
                        AdId = c.Long(nullable: false),
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
            DropForeignKey("dbo.Lands", "AdId", "dbo.Ads");
            DropIndex("dbo.Lands", new[] { "AdId" });
            DropTable("dbo.Lands");
        }
    }
}
