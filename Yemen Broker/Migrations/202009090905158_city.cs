namespace Yemen_Broker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class city : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CityModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Ads", "City_Id", c => c.Int());
            CreateIndex("dbo.Ads", "City_Id");
            AddForeignKey("dbo.Ads", "City_Id", "dbo.CityModels", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ads", "City_Id", "dbo.CityModels");
            DropIndex("dbo.Ads", new[] { "City_Id" });
            DropColumn("dbo.Ads", "City_Id");
            DropTable("dbo.CityModels");
        }
    }
}
