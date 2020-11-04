namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PicturesAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Picture",
                c => new
                    {
                        PictureId = c.Int(nullable: false, identity: true),
                        Picture = c.String(),
                        AdId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.PictureId)
                .ForeignKey("dbo.Ads", t => t.AdId, cascadeDelete: true)
                .Index(t => t.AdId);
            
            AddColumn("dbo.Ads", "AdvertiserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Ads", "AdvertiserId");
            AddForeignKey("dbo.Ads", "AdvertiserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Picture", "AdId", "dbo.Ads");
            DropForeignKey("dbo.Ads", "AdvertiserId", "dbo.AspNetUsers");
            DropIndex("dbo.Picture", new[] { "AdId" });
            DropIndex("dbo.Ads", new[] { "AdvertiserId" });
            DropColumn("dbo.Ads", "AdvertiserId");
            DropTable("dbo.Picture");
        }
    }
}
