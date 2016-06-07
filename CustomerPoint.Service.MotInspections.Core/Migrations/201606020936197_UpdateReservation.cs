namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateReservation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Slots", "Expires", c => c.DateTime());
            AddColumn("dbo.Slots", "SessionId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Slots", "SessionId");
            DropColumn("dbo.Slots", "Expires");
        }
    }
}
