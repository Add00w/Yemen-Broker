namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Apartment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Apartments",
                c => new
                    {
                        AdId = c.Long(nullable: false),
                        FloorNumber = c.Int(nullable: false),
                        NumberOfDoors = c.Int(nullable: false),
                        NumberOfBathrooms = c.Int(nullable: false),
                        NumberOfKitchens = c.Int(nullable: false),
                        TypeOfFinishing = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AdId)
                .ForeignKey("dbo.Ads", t => t.AdId,cascadeDelete:true)
                .Index(t => t.AdId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Apartments", "AdId", "dbo.Ads");
            DropIndex("dbo.Apartments", new[] { "AdId" });
            DropTable("dbo.Apartments");
        }
    }
}
