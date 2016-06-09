using System;
using System.Collections.Generic;
using System.Linq;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Validation;

using CustomerPoint.Models;
using GuildfordBoroughCouncil.Linq;

namespace CustomerPoint.Service.MotInspections.Models
{
    [DisplayColumn("Name")]
    public class Service
    {
        private string _Name = null;

        public Service()
        {
            Charge = 0;
            DisplayOrder = 0;
        }

        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                Slug = value.ToLower().Replace(" ", "-");
            }
        }
        public string Description { get; set; }

        [DataType(DataType.Currency)]
        [Display(Description = "This value can be overridden on a per customer basis.")]
        public decimal Charge { get; set; }

        [Display(Name = "Display order", Description = "Use this to override the alphabetical listing of services.")]
        public byte DisplayOrder { get; set; }

        [Display(Name = "Parent", Description = "This allows services to be grouped. Choosing a parent is optional.")]
        public int? ParentId { get; set; }

        public string Slug { get; private set; }

        public virtual Service Parent { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<ServiceCustomer> Customers { get; set; }
    }

    [DisplayColumn("Name")]
    public class Customer
    {
        private string _Name = null;

        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                Slug = value.ToLower().Replace(" ", "-");
            }
        }
        public string Description { get; set; }
        [MaxLength(10)]
        [Display(Name = "Ledger code", Description = "This is the ledger code payments will be recorded against")]
        public string LedgerCode { get; set; }
        public bool Hidden { get; set; }

        public string Slug { get; private set; }

        public virtual ICollection<ServiceCustomer> Services { get; set; }
        
    }

    public class ServiceCustomer
    {
        public ServiceCustomer()
        {
            HiddenPublic = false;
            HiddenStaff = false;
        }

        [Key, Column(Order = 0)]
        public int CustomerId { get; set; }
        [Key, Column(Order = 1)]
        public int ServiceId { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "Money")]
        public decimal? ChargeAmount { get; set; }
        public bool HiddenPublic { get; set; }
        public bool HiddenStaff { get; set; }

        [NotMapped]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal? Price
        {
            get
            {
                if (ChargeAmount.HasValue)
                {
                    return ChargeAmount.Value;
                }
                else if (Service != null)
                {
                    return Service.Charge;
                }
                return null;
            }
        }

        public virtual Customer Customer { get; set; }
        public virtual Service Service { get; set; }
    }

    public abstract class Slot
    {
        [Key]
        public int Id { get; set; }
        public int ResourceId { get; set; }

        [DisplayFormat(DataFormatString = "{0:ddd d MMM yyyy H:mm}")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public Resource Resource { get; set; }

    }

    public class Reservation : Slot
    {
        public ReservationReason Reason { get; set; }
        public DateTime? Expires { get; set; }
        public string SessionId { get; set; }
    }

    public class Booking : Slot
    {
        public Status Status { get; set; }
        [Display(Name = "Service")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must select a service.")]
        public int ServiceId { get; set; }

        [Display(Name = "Make")]
        [MaxLength(100)]
        [DisplayFormat(NullDisplayText = "-")]
        public string VehicleMake { get; set; }
        [Display(Name = "Model")]
        [MaxLength(100)]
        [DisplayFormat(NullDisplayText = "-")]
        public string VehicleModel { get; set; }
        [Display(Name = "Registration")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must specify the registration of the vehicle being inspected.")]
        [MaxLength(20)]
        public string VehicleRegistration { get; set; }
        [Display(Name = "License Plate")]
        [MaxLength(20)]
        [DisplayFormat(NullDisplayText = "-")]
        public string VehiclePlate { get; set; }

        [Display(Name = "Customer")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must select a customer type.")]
        public int CustomerId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must specify the name of the person making the booking.")]
        [MaxLength(100)]
        [DisplayFormat(NullDisplayText = "-")]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must specify a contact telephone number.")]
        [MaxLength(100)]
        [DisplayFormat(NullDisplayText = "-")]
        [DataType(DataType.PhoneNumber)]
        public string Telephone { get; set; }

        [MaxLength(50)]
        [DisplayFormat(NullDisplayText = "-")]
        public string Inspector { get; set; }
        [MaxLength(50)]
        [Display(Name = "MoT test serial number")]
        [DisplayFormat(NullDisplayText = "-")]
        public string TestSerialNumber { get; set; }
        [DisplayFormat(NullDisplayText = "-")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Inspector's Notes")]
        public string InspectorNotes { get; set; }

        [Display(Name = "Booked by")]
        public GuildfordBoroughCouncil.Data.Models.User BookedBy { get; set; }

        [NotMapped]
        public bool IgnoreReservation { get; set; }

        public DateTime? Cancelled { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Service Service { get; set; }

        [NotMapped]
        [DataType(DataType.Currency)]
        public decimal? Cost
        {
            get
            {
                try
                {
                    if (Customer.Name == "Public" && Status == Models.Status.Failed_to_Attend)
                    {
                        return 0;
                    }

                    var Cost = Service.Charge;

                    if (Status == Models.Status.Failed_to_Attend && Cost > 0)
                    {
                        return 57;
                    }

                    return Cost;
                }
                catch
                {
                    return null;
                }
            }
        }

        //[Display(Name = "Email address", Description = "Your email address will be used to contact you if there any queries")]
        //[DataType(DataType.EmailAddress)]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email address so we can contact you if there are any queries")]
        //public string Email { get; set; }

        [Display(Name = "Price to pay")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "Money")]
        public decimal? PriceToPay { get; set; }

        public string PaymentRef { get; private set; }

        [Display(Name = "Mark booking as paid", Description = "Use this option to mark the booking as paid when an alternative means has been used, such as cash.")]
        public bool OverridePayment { get; set; }

        public bool CollectPaymentAtEvent { get; set; }

        public virtual string GetPaymentsUrl(string User, string ReturnUrl, bool RegeneratePayRef = false, bool Internal = false)
        {
            if (String.IsNullOrWhiteSpace(PaymentRef) || RegeneratePayRef)
            {
                var payRef = "MotInspM" + Id.ToString();

                PaymentRef = payRef + "-" + (payRef + DateTime.UtcNow.ToString()).CalculateSha1Hash().ToLower().Substring(0, 6);
            }

            var PaymentTemplate = "https://payments.guildford.gov.uk/pay/wsconn_pay.asp?channel={0}&sessionid={1}&amount={2}&fundcode={3}&custref1={4}&custref2={5}&custref3={6}&custref4={7}&description={8}&returnmethod={9}&returnurl={10}&sendmail=n";

            if (PriceToPay.HasValue && PriceToPay > 0)
            {
                return String.Format(PaymentTemplate,
                        (Internal) ? "CPi" : "CPx",
                        PaymentRef,
                        (int)(PriceToPay * 100),
                        "08",
                        Customer.LedgerCode,
                        VehicleRegistration,
                        "M" + Id,
                        User,
                        VehicleRegistration + " - " + Name,
                        "post",
                        ReturnUrl);
            }
            return null;
        }

        public virtual bool PaymentSuccessful
        {
            get
            {
                return ((Payment != null && Payment.Sum(p => p.Amount) > 0) || OverridePayment);
            }
        }

        [NotMapped]
        private IEnumerable<Transaction> _Payment { get; set; }

        [NotMapped]
        public virtual IEnumerable<Transaction> Payment
        {
            get
            {
                if (_Payment == null)
                {
                    _Payment = Payments.Lookup.Payments(reference: new string[] { "M" + Id.ToString() }, fund: "08").Result;
                }

                return _Payment;
            }
        }
    }

    [DisplayColumn("Name")]
    public class Resource
    {
        [Key]
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
    }

    public class MotData : DbContext
    {
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCustomer> ServiceCustomers { get; set; }

        public DbSet<Resource> Resources { get; set; }

        protected override DbEntityValidationResult ValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            var result = new DbEntityValidationResult(entityEntry, new List<DbValidationError>());

            if (entityEntry.Entity is Reservation && entityEntry.State == EntityState.Added)
            {
                var nb = entityEntry.Entity as Reservation;

                //if (Slots.OfType<Reservation>().Where(b => (b.Date == nb.Date) && (b.Bay == nb.Bay)).Count() > 0)
                //{
                //    result.ValidationErrors.Add(new DbValidationError("Date", "There is already a reservation for this bay and time."));
                //}

                //if (Slots.OfType<Booking>().Where(b => (b.Date == nb.Date) && (b.Bay == nb.Bay) && !b.Cancelled.HasValue).Count() > 0)
                //{
                //    result.ValidationErrors.Add(new DbValidationError("Date", "There is already a booking for this bay and time."));
                //}

                if ((nb.Date.DayOfWeek == DayOfWeek.Saturday) || (nb.Date.DayOfWeek == DayOfWeek.Sunday))
                {
                    result.ValidationErrors.Add(new DbValidationError("Date", "There are no bookings at weekends."));
                }
            }

            if (entityEntry.Entity is Booking)
            {
                var nb = entityEntry.Entity as Booking;

                nb.VehicleRegistration = System.Text.RegularExpressions.Regex.Replace(nb.VehicleRegistration.ToUpper(), @"\s+", "");

                if (entityEntry.State == EntityState.Added)
                {
                    if ((nb.Date.DayOfWeek == DayOfWeek.Saturday) || (nb.Date.DayOfWeek == DayOfWeek.Sunday))
                    {
                        result.ValidationErrors.Add(new DbValidationError("Date", "There are no bookings at weekends."));
                    }

                    // Check for an existing booking, but exclude cancelled ones

                    //if (Slots.OfType<Booking>().Where(b => (b.Date == nb.Date) && (b.Bay == nb.Bay) && !b.Cancelled.HasValue).Count() > 0)
                    //{
                    //    result.ValidationErrors.Add(new DbValidationError("Date", "There is already a booking for this bay and time."));
                    //}

                    // Check for a reservation.
                    //if (!nb.IgnoreReservation)
                    //{
                    //    if (Slots.OfType<Reservation>().Where(b => (b.Date == nb.Date) && (b.Bay == nb.Bay)).Count() > 0)
                    //    {
                    //        result.ValidationErrors.Add(new DbValidationError("Date", "There is already a booking for this bay and time."));
                    //    }
                    //}
                }
            }

            if (result.ValidationErrors.Count > 0)
            {
                return result;
            }
            else
            {
                return base.ValidateEntity(entityEntry, items);
            }
        }

        //public DbSet<Booking> Bookings { get; set; }
    }
}
