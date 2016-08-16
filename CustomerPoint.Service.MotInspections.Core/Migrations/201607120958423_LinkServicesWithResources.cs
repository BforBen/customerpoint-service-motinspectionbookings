namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkServicesWithResources : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResourceServices",
                c => new
                    {
                        Resource_Id = c.Int(nullable: false),
                        Service_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Resource_Id, t.Service_Id })
                .ForeignKey("dbo.Resources", t => t.Resource_Id, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.Service_Id, cascadeDelete: true)
                .Index(t => t.Resource_Id)
                .Index(t => t.Service_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResourceServices", "Service_Id", "dbo.Services");
            DropForeignKey("dbo.ResourceServices", "Resource_Id", "dbo.Resources");
            DropIndex("dbo.ResourceServices", new[] { "Service_Id" });
            DropIndex("dbo.ResourceServices", new[] { "Resource_Id" });
            DropTable("dbo.ResourceServices");
        }
    }
}
