using System.ComponentModel.DataAnnotations;

namespace CustomerPoint.Service.MotInspections.Models
{
    public enum Status
    {
        Outstanding,
        Passed,
        Failed,
        [Display(Name = "Failed to attend")]
        Failed_to_Attend,
        Completed,
        Cancelled
    }

    public enum ReservationReason
    {
        [Display(Name = "Bay closed")]
        Bay_Closed,
        [Display(Name = "Taxi or private hire only")]
        Taxi_or_PH_only,
        [Display(Name = "Guildford BC only")]
        Guildford_BC_only,
        [Display(Name = "Waverley BC only")]
        Waverley_BC_only,
        [Display(Name = "Woking BC only")]
        Woking_BC_only,
        [Display(Name = "Taxi test only")]
        Taxi_Test_Only
    }
}
