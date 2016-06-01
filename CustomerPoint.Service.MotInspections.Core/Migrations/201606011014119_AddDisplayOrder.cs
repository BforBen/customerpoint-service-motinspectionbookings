namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDisplayOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "DisplayOrder", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "DisplayOrder");
        }
    }
}
