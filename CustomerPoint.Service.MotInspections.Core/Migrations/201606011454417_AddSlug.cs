namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSlug : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "Slug", c => c.String());
            AddColumn("dbo.Services", "Slug", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "Slug");
            DropColumn("dbo.Customers", "Slug");
        }
    }
}
