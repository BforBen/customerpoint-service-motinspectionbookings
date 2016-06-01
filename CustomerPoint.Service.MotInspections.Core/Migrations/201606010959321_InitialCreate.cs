namespace CustomerPoint.Service.MotInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        LedgerCode = c.String(maxLength: 10),
                        Hidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ServiceCustomers",
                c => new
                    {
                        CustomerId = c.Int(nullable: false),
                        ServiceId = c.Int(nullable: false),
                        ChargeAmount = c.Decimal(storeType: "money"),
                    })
                .PrimaryKey(t => new { t.CustomerId, t.ServiceId })
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Charge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HiddenPublic = c.Boolean(nullable: false),
                        HiddenStaff = c.Boolean(nullable: false),
                        ParentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ParentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.Slots",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Bay = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Status = c.Int(),
                        ServiceId = c.Int(),
                        VehicleMake = c.String(maxLength: 100),
                        VehicleModel = c.String(maxLength: 100),
                        VehicleRegistration = c.String(maxLength: 20),
                        VehiclePlate = c.String(maxLength: 20),
                        CustomerId = c.Int(),
                        Name = c.String(maxLength: 100),
                        Telephone = c.String(maxLength: 100),
                        Inspector = c.String(maxLength: 50),
                        TestSerialNumber = c.String(maxLength: 50),
                        InspectorNotes = c.String(),
                        BookedBy_Name = c.String(maxLength: 300),
                        BookedBy_Username = c.String(maxLength: 300),
                        Cancelled = c.DateTime(),
                        PriceToPay = c.Decimal(storeType: "money"),
                        PaymentRef = c.String(),
                        OverridePayment = c.Boolean(),
                        Reason = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId)
                .Index(t => t.CustomerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Slots", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.Slots", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.Services", "ParentId", "dbo.Services");
            DropForeignKey("dbo.ServiceCustomers", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceCustomers", "CustomerId", "dbo.Customers");
            DropIndex("dbo.Slots", new[] { "CustomerId" });
            DropIndex("dbo.Slots", new[] { "ServiceId" });
            DropIndex("dbo.Services", new[] { "ParentId" });
            DropIndex("dbo.ServiceCustomers", new[] { "ServiceId" });
            DropIndex("dbo.ServiceCustomers", new[] { "CustomerId" });
            DropTable("dbo.Slots");
            DropTable("dbo.Services");
            DropTable("dbo.ServiceCustomers");
            DropTable("dbo.Customers");
        }
    }
}
