namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResourceParent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "Start", c => c.DateTime());
            AddColumn("dbo.Resources", "End", c => c.DateTime());
            AddColumn("dbo.Resources", "ParentId", c => c.Int());
            CreateIndex("dbo.Resources", "ParentId");
            AddForeignKey("dbo.Resources", "ParentId", "dbo.Resources", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Resources", "ParentId", "dbo.Resources");
            DropIndex("dbo.Resources", new[] { "ParentId" });
            DropColumn("dbo.Resources", "ParentId");
            DropColumn("dbo.Resources", "End");
            DropColumn("dbo.Resources", "Start");
        }
    }
}
