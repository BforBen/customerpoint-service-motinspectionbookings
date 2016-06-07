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
            var Reservations = await db.Slots.OfType<Reservation>().Where(b => DbFunctions.TruncateTime(b.Date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(b.Date) <= DbFunctions.TruncateTime(EndDate)).ToListAsync();

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
                        if (DayBookings.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count() < ResourceCount)
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