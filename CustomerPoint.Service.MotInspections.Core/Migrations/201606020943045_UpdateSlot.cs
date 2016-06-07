namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSlot : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Slots", name: "BayId", newName: "ResourceId");
            RenameIndex(table: "dbo.Slots", name: "IX_BayId", newName: "IX_ResourceId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Slots", name: "IX_ResourceId", newName: "IX_BayId");
            RenameColumn(table: "dbo.Slots", name: "ResourceId", newName: "BayId");
        }
    }
}
