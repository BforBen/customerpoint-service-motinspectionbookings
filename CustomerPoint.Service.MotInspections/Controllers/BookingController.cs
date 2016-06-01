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
        [Route("~/{customer?}")]
        public ActionResult SelectService(string customer = null)
        {
            var Services = db.ServiceCustomers
                .WhereIf(!string.IsNullOrWhiteSpace(customer), s => s.Customer.Slug == customer)
                .WhereIf(Settings.Internal, s => !s.HiddenStaff)
                .WhereIf(!Settings.Internal, s => !s.HiddenPublic)
                .GroupBy(s => s.Customer).OrderBy(s => s.Key.Name);

            return View(Services);
        }

        //[HttpGet, HttpPost]
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

            var AvailableSlots = new Dictionary<DateTime, List<TimeSpan>>();
            var TheDate = StartDate;

            var StartOfMorning = new TimeSpan(8, 00, 00);
            var Lunch = new TimeSpan(13, 00, 00);
            var StartOfAfternoon = new TimeSpan(8, 00, 00);
            var EndOfDay = new TimeSpan(16, 30, 00);

            var ResourceCount = 2;

            do
            {
                if (TheDate.DayOfWeek > DayOfWeek.Sunday && TheDate.DayOfWeek < DayOfWeek.Saturday)
                {
                    var Times = new List<TimeSpan>();
                    var TheSlot = StartOfMorning;

                    do
                    {
                        if (Bookings.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count() < ResourceCount)
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
                            if (Bookings.Where(b => b.Date.TimeOfDay.Equals(TheSlot)).Count() < ResourceCount)
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

            return View(AvailableSlots);
        }

        [HttpGet]
        [Route("{customer}/{service}/book")]
        public async Task<ActionResult> MakeBooking(string service, string customer)
        {
            // Reserve slot against session ID for 10 minutes.

            var Service = await db.ServiceCustomers
                .Where(s => s.Customer.Slug == customer && s.Service.Slug == service)
                .SingleOrDefaultAsync();

            if (Service == null)
            {
                return HttpNotFound();
            }

            return View();
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