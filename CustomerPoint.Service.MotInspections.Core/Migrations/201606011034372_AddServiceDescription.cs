namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddServiceDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "Description");
        }
    }
}
