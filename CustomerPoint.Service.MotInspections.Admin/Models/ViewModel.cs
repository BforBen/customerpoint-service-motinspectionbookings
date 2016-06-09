using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPoint.Service.MotInspections.Models;

namespace CustomerPoint.Service.MotInspections.Admin.Models
{
    public class UpdateCustomerServices
    {
        public Customer Customer { get; set; }

        public IEnumerable<ServiceCustomer> CustomerServices { get; set; }

        public IEnumerable<MotInspections.Models.Service> Services { get; set; }
    }

    public class ReportModel
    {
        public ReportModel()
        {
            var FirstOfMonth = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            From = FirstOfMonth.AddMonths(-1);
            To = FirstOfMonth.AddDays(-1);
        }

        [DataType(DataType.Date)]
        public DateTime From { get; set; }
        [DataType(DataType.Date)]
        public DateTime To { get; set; }
        public int? Customer { get; set; }
        public string Action { get; set; }
        public Status? Status { get; set; }
    }
}