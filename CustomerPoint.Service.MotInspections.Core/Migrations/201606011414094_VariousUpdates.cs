namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariousUpdates : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Resources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Customers", "Description", c => c.String());
            AddColumn("dbo.ServiceCustomers", "HiddenPublic", c => c.Boolean(nullable: false));
            AddColumn("dbo.ServiceCustomers", "HiddenStaff", c => c.Boolean(nullable: false));
            AddColumn("dbo.Slots", "BayId", c => c.Int(nullable: false));
            AddColumn("dbo.Slots", "CollectPaymentAtEvent", c => c.Boolean());
            CreateIndex("dbo.Slots", "BayId");
            AddForeignKey("dbo.Slots", "BayId", "dbo.Resources", "Id", cascadeDelete: true);
            DropColumn("dbo.Services", "HiddenPublic");
            DropColumn("dbo.Services", "HiddenStaff");
            DropColumn("dbo.Slots", "Bay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Slots", "Bay", c => c.Int(nullable: false));
            AddColumn("dbo.Services", "HiddenStaff", c => c.Boolean(nullable: false));
            AddColumn("dbo.Services", "HiddenPublic", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.Slots", "BayId", "dbo.Resources");
            DropIndex("dbo.Slots", new[] { "BayId" });
            DropColumn("dbo.Slots", "CollectPaymentAtEvent");
            DropColumn("dbo.Slots", "BayId");
            DropColumn("dbo.ServiceCustomers", "HiddenStaff");
            DropColumn("dbo.ServiceCustomers", "HiddenPublic");
            DropColumn("dbo.Customers", "Description");
            DropTable("dbo.Resources");
        }
    }
}
