using System.Data.Entity.Migrations;
using CustomerPoint.Service.MotInspections.Models;

namespace CustomerPoint.Service.MotInspections.Migrations
{
   
    internal sealed class Configuration : DbMigrationsConfiguration<MotData>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "CustomerPoint.Service.MotInspections.Models.MotData";
        }

        protected override void Seed(MotData context)
        {
            context.Resources.AddOrUpdate(
                r => r.Name,
                new Resource { Name = "MoT bay" });

            context.Customers.AddOrUpdate(
                c => new { c.Name },
                    new Customer { Id = 1, Name = "Public", Hidden = false, LedgerCode = "V8598K4979" },
                    new Customer { Id = 2, Name = "Guildford BC", Hidden = false, LedgerCode = "V8598K8907" },
                    new Customer { Id = 3, Name = "Woking BC", Hidden = false, LedgerCode = "V8598K4908" },
                    new Customer { Id = 4, Name = "Waverley BC", Hidden = false, LedgerCode = "V8598K4900" },
                    new Customer { Id = 5, Name = "Guildford Fleet", Hidden = false, LedgerCode = "" },
                    new Customer { Id = 6, Name = "Guildford Staff", Hidden = false, LedgerCode = "" },
                    new Customer { Id = 7, Name = "Guildford Administration", Hidden = false, LedgerCode = "" },
                    new Customer { Id = 8, Name = "Surrey CC", Hidden = true, LedgerCode = "" },
                    new Customer { Id = 9, Name = "Guildford Lease", Hidden = true, LedgerCode = "" }
                );

            context.Services.AddOrUpdate(
                s => new { s.Name },
                    new Models.Service { Id = 1, Name = "MOT" },
                    new Models.Service { Id = 2, Name = "Private hire" },
                    new Models.Service { Id = 3, Name = "Taxi" },

                    new Models.Service { Name = "MOT", ParentId = 1, Charge = 54.8m },
                    new Models.Service { Name = "MOT retest", ParentId = 1, Charge = 54.8m },

                    new Models.Service { Name = "Inspection", ParentId = 2, Charge = 57 },
                    new Models.Service { Name = "Inspection retest", ParentId = 2, Charge = 57 },
                    new Models.Service { Name = "Inspection & MOT", ParentId = 2, Charge = 57 },
                    new Models.Service { Name = "Inspection retest & MOT", ParentId = 2, Charge = 84.4m },
                    new Models.Service { Name = "Compliance inspection", ParentId = 2, Charge = 57 },

                    new Models.Service { Name = "Inspection", ParentId = 3, Charge = 57 },
                    new Models.Service { Name = "Inspection retest", ParentId = 3, Charge = 57 },
                    new Models.Service { Name = "Inspection & MOT", ParentId = 3, Charge = 57 },
                    new Models.Service { Name = "Inspection retest & MOT", ParentId = 3, Charge = 84.4m },
                    new Models.Service { Name = "Compliance inspection", ParentId = 3, Charge = 57 },

                    new Models.Service { Name = "Engineers report", Charge = 57 },
                    new Models.Service { Name = "Administration", Charge = 0 },
                    new Models.Service { Name = "Legacy", Charge = 0 }
                );

            context.ServiceCustomers.AddOrUpdate(
                s => new { s.CustomerId, s.ServiceId },
                    new ServiceCustomer { CustomerId = 1, ServiceId = 1 },
                    new ServiceCustomer { CustomerId = 1, ServiceId = 2 },
                    new ServiceCustomer { CustomerId = 3, ServiceId = 1 },
                    new ServiceCustomer { CustomerId = 3, ServiceId = 2 }
                );
        }
    }
}
