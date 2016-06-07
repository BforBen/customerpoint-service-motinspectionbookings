using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerPoint.Service.MotInspections.Models
{
    public class ServicePicker
    {
        public int CustomerId { get; set; }
        public string Customer { get; set; }
        public string Service { get; set; }
        public string ServiceSlug { get; set; }
    }

    public class BookingModel
    {
        public int ReservationId { get; set; }
        public int CustomerId { get; set; }
        public int ServiceId { get; set; }
        public string Customer { get; set; }
        public string Service { get; set; }

        public string Name { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Telephone", Description = "We'll use this number to contact you about your booking")]
        public string Telephone { get; set; }

        [MaxLength(8)]
        [Display(Name = "Please enter the vehicle registration")]
        public string VehicleReg { get; set; }
        [Display(Name = "Vehicle make")]
        public string VehicleMake { get; set; }
        [Display(Name = "Vehicle model")]
        public string VehicleModel { get; set; }
        [MaxLength(10)]
        [Display(Name = "License plate")]
        public string VehiclePlate { get; set; }

        [Display(Name = "Pay now")]
        [DataType(DataType.Currency)]
        public decimal? PriceToPay { get; set; }
        [Display(Name = "Pay at the MoT station", Description = "This option should only be used in exceptional circumstances")]
        public bool PayLater { get; set; }

        [DisplayFormat(DataFormatString = "{0:dddd d MMMM yyyy h:mm tt}")]
        public DateTime Slot { get; set; }
    }
}