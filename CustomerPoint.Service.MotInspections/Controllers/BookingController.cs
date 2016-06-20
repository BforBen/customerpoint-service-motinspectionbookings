using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using CustomerPoint.Service.MotInspections.Models;
using GuildfordBoroughCouncil.Linq;

namespace CustomerPoint.Service.MotInspections.Controllers
{
    [RoutePrefix("book")]
    public class BookingController : Controller
    {
        private MotData db = new MotData();

        [HttpGet]
        [Route("~/")]
        [Route("{customer}")]
        public ActionResult SelectService(string customer = null)
        {
            var Services = db.ServiceCustomers
                .WhereIf(!string.IsNullOrWhiteSpace(customer), s => s.Customer.Slug == customer)
                .WhereIf(Settings.Internal, s => !s.HiddenStaff)
                .WhereIf(!Settings.Internal, s => !s.HiddenPublic)
                .GroupBy(s => s.Customer).OrderBy(s => s.Key.Name);

            return View(Services);
        }

        [Route("{customer}/{service}")]
        public async Task<ActionResult> SelectTime(string service, string customer, DateTime? availability = null, DateTime? slot = null)
        {
            var Service = await db.ServiceCustomers
                .Where(s => s.Customer.Slug == customer && s.Service.Slug == service)
                .SingleOrDefaultAsync();

            if (Service == null)
            {
                return HttpNotFound();
            }

            if (slot.HasValue)
            {
                TempData["Slot"] = slot;

                return RedirectToAction("MakeBooking", new { service = service, customer = customer });
            }

            if (!availability.HasValue)
            {
                availability = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            var StartDate = availability.Value;
            var EndDate = new DateTime(StartDate.Year, StartDate.Month, DateTime.DaysInMonth(StartDate.Year, StartDate.Month));
            ViewBag.ActiveMonth = new DateTime(StartDate.Year, StartDate.Month, 1);

            while (StartDate.DayOfWeek != DayOfWeek.Monday)
            {
                StartDate = StartDate.AddDays(-1);
            }

            while (EndDate.DayOfWeek != DayOfWeek.Sunday)
            {
                EndDate = EndDate.AddDays(1);
            }


            var Bookings = await db.Slots.OfType<Booking>().Where(b => DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(EndDate) && !b.Cancelled.HasValue).ToListAsync();
            var Reservations = await db.Slots.OfType<Reservation>().Where(b => DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(EndDate) && (!b.Expires.HasValue || b.Expires > DateTime.Now)).ToListAsync();
//TODO filter reservations to type of booking?

            var AvailableSlots = new Dictionary<DateTime, List<TimeSpan>>();
            var TheDate = StartDate;

            var StartOfMorning = new TimeSpan(8, 00, 00);
            var Lunch = new TimeSpan(13, 00, 00);
            var StartOfAfternoon = new TimeSpan(13, 30, 00);
            var EndOfDay = new TimeSpan(16, 30, 00);

            var ResourceCount = 2;

            do
            {
                if (TheDate.DayOfWeek > DayOfWeek.Sunday && TheDate.DayOfWeek < DayOfWeek.Saturday)
                {
                    var Times = new List<TimeSpan>();
                    var TheSlot = StartOfMorning;

                    var DayBookings = Bookings.Where(b => b.Date.Date == TheDate.Date);
                    var DayReservations = Reservations.Where(b => b.Date.Date == TheDate.Date);

                    do
                    {
                        var TotalSlots = DayBookings.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count() +
                            DayReservations.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count();
                        if (TotalSlots < ResourceCount)
                        {
                            Times.Add(TheSlot);
                        }

                        TheSlot = TheSlot.Add(new TimeSpan(1, 0, 0));
                    } while (TheSlot < Lunch);

                    if (TheDate.DayOfWeek != DayOfWeek.Friday)
                    {
                        TheSlot = StartOfAfternoon;

                        do
                        {
                            if (DayBookings.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count() < ResourceCount)
                            {
                                Times.Add(TheSlot);
                            }

                            TheSlot = TheSlot.Add(new TimeSpan(1, 0, 0));
                        } while (TheSlot < EndOfDay);
                    }

                    AvailableSlots.Add(TheDate, Times);
                }
                TheDate = TheDate.AddDays(1);

            } while (TheDate <= EndDate);

            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.Service = Service.Service.Name;
            return View(AvailableSlots);
        }

        [HttpGet]
        [Route("{customer}/{service}/book")]
        public async Task<ActionResult> MakeBooking(string service, string customer)
        {
            var Service = await db.ServiceCustomers
                .Where(s => s.Customer.Slug == customer && s.Service.Slug == service)
                .SingleOrDefaultAsync();

            if (Service == null)
            {
                return HttpNotFound();
            }

            var Slot = TempData["Slot"] as DateTime?;

            if (!Slot.HasValue)
                return RedirectToAction("SelectTime", new { service = service, customer = customer });

            // Reserve slot against session ID for 15 minutes.
            var Reservation = new Reservation
            {
                Date = Slot.Value,
                Expires = DateTime.Now.AddMinutes(15),
                SessionId = Session.SessionID,
                Reason = ReservationReason.Booking_in_progress,
                ResourceId = 1
            };

            db.Slots.Add(Reservation);
            var x = await db.SaveChangesAsync();

            var Booking = new BookingModel
            {
                ReservationId = Reservation.Id,
                Customer = Service.Customer.Name,
                CustomerId = Service.CustomerId,
                Service = Service.Service.Name,
                ServiceId = Service.ServiceId,
                Slot = Slot.Value,
                PriceToPay = Service.Price
            };

            ViewBag.ServiceSlug = service;
            ViewBag.CustomerSlug = customer;

            return View(Booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{customer}/{service}/book")]
        public async Task<ActionResult> MakeBooking(string service, string customer, BookingModel booking)
        {
            var Service = await db.ServiceCustomers
                .Where(s => s.Customer.Slug == customer && s.Service.Slug == service)
                .SingleOrDefaultAsync();

            if (Service == null)
            {
                return HttpNotFound();
            }

            var Reservation = await db.Slots.OfType<Reservation>().Where(r => r.Id == booking.ReservationId).SingleOrDefaultAsync();

            if (Reservation == null)
            {
                return HttpNotFound();
            }

            if (Reservation.Expires < DateTime.Now)
            {
                // Reservation has expired
                ModelState.AddModelError("", "Your reservation has expired");
            }

            if (Service.Customer.Name != "Public" && string.IsNullOrWhiteSpace(booking.VehiclePlate))
            {
                ModelState.AddModelError("VehiclePlate", "Private hire and taxi vehicles must enter their plate number");
            }

            // Extend reservation to another 10 minutes
            Reservation.Expires = DateTime.Now.AddMinutes(10);
            var x = await db.SaveChangesAsync();

            if (ModelState.IsValid)
            {
                var b = new Booking();

                b.BookedBy = (Settings.Internal ? User.Identity.GetUser() : new GuildfordBoroughCouncil.Data.Models.User("customerpoint"));
                b.CustomerId = Service.CustomerId;
                b.Date = Reservation.Date;
                b.IgnoreReservation = true;
                b.Name = booking.Name;
                b.PriceToPay = Service.Price;
                b.ResourceId = 1;
                b.ServiceId = Service.ServiceId;
                b.Status = Status.Outstanding;
                b.Telephone = booking.Telephone;
                b.VehicleMake = booking.VehicleMake;
                b.VehicleModel = booking.VehicleModel;
                b.VehiclePlate = booking.VehiclePlate;
                b.VehicleRegistration = booking.VehicleReg;

                db.Slots.Add(b);

                x = await db.SaveChangesAsync();

                var RedirectUrl = Url.Action("Finish", new { service = service, customer = customer, id = b.Id });

                if (booking.PriceToPay > 0)
                {
                    //always take the amount to pay from the DB
                    b.CollectPaymentAtEvent = false;

                    RedirectUrl = b.GetPaymentsUrl(User.Identity.Name, Url.Action("Finish", "Booking", new { service = service, customer = customer, id = b.Id }, "http"), false, Settings.Internal, Service.Customer.LedgerCode);

                    x = await db.SaveChangesAsync();
                }                

                return Redirect(RedirectUrl);
            }

            ViewBag.ServiceSlug = service;
            ViewBag.CustomerSlug = customer;

            booking.Slot = Reservation.Date;
            booking.Customer = Service.Customer.Name;
            booking.CustomerId = Service.CustomerId;
            booking.Service = Service.Service.Name;
            booking.ServiceId = Service.ServiceId;
            booking.PriceToPay = Service.Price;

            return View(booking);
        }

        [Route("{customer}/{service}/book/{id:int}")]
        public ActionResult Finish(string customer, string service, int id, CustomerPoint.Models.AdelanteResponse PaymentStatus)
        {
            //ViewBag.AnalyticsPage = Url.Action("PaymentStatus", new { Uprn = Uprn }).Replace(Uprn.ToString() + "/", "");

            string Message = "";
            string AlertType = "";
            string Heading = "";
            bool TryAgain = true;

            #region Adelante
            if (PaymentStatus.ErrorStatus.HasValue)
            {
                // Completed
                if (PaymentStatus.ErrorStatus.Value == 1)
                {
                    if (PaymentStatus.AuthStatus.HasValue && PaymentStatus.AuthStatus.Value == 1)
                    {
                        AlertType = "success";
                        Heading = "Payment successful";
                        Message = "Thank you, your payment has been successful. The receipt number is " + PaymentStatus.IasOrderNo + ". Please allow 24 hours for changes to your subscription to appear on this website. New or additional bins will be delivered in four to six weeks.";
                        TryAgain = false;
                    }
                    else
                    {
                        AlertType = "danger";
                        Heading = "Payment declined";
                        Message = "Sorry, this payment has been declined. Please use a different credit or debit card and try again.";
                    }
                }
                else
                {
                    if (PaymentStatus.ErrorCode.HasValue && PaymentStatus.ErrorCode.Value == 51)
                    {
                        AlertType = "danger";
                        Heading = "Payment problem";
                        Message = @"Sorry, something has gone wrong and the payments system has been unable to verify if the payment was successful. Please contact Customer Services on 01483 444499.";
                        TryAgain = false;
                    }
                    else
                    {
                        AlertType = "danger";
                        Heading = "Payment not successful";
                        Message = @"Sorry, something has gone wrong and this payment has been unsuccessful. Please try again. If this problem continues please try a different credit or debit card or contact Customer Services on 01483 444499.";
                    }
                }

                if (PaymentStatus.ErrorStatus.Value == 0 || (PaymentStatus.ErrorStatus.Value == 1 && PaymentStatus.AuthStatus.HasValue && PaymentStatus.AuthStatus.Value == 0))
                {
                    try
                    {
                        throw new Exception(String.IsNullOrWhiteSpace(PaymentStatus.ErrorDescription) ? "Adelante response code: " + PaymentStatus.ErrorCode : "Adelante response: " + PaymentStatus.ErrorDescription + (PaymentStatus.ErrorCode.HasValue ? " [" + PaymentStatus.ErrorCode + "]" : ""));
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error(ex, "Payment failed");
                    }
                }
            }
            #endregion

            ViewBag.Message = Message;
            ViewBag.Heading = Heading;
            ViewBag.AlertType = AlertType;
            ViewBag.TryAgain = TryAgain;

            return View(id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}