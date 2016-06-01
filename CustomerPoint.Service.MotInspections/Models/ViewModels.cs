using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPoint.Service.MotInspections.Models
{
    public class ServicePicker
    {
        public int CustomerId { get; set; }
        public string Customer { get; set; }
        public string Service { get; set; }
        public string ServiceSlug { get; set; }
    }
}