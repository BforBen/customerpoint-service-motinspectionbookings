using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;
using CustomerPoint.Service.MotInspections.Admin.Models;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("bookings")]
    public class BookingsController : Controller
    {
        private MotData db = new MotData();

        [HttpGet]
        [Route]
        public async Task<ActionResult> Index(DateTime? Date = null, DateTime? Month = null)
        {
            if (!Date.HasValue)
            {
                Date = DateTime.Today.Date;
            }

            ViewBag.Date = Date;

            var Bookings = await db.Slots.OfType<Booking>()
                        .Include(b => b.Resource)
                        .Include(b => b.Customer)
                        .Include(b => b.Service)
                        .Where(b => DbFunctions.TruncateTime(b.Date) == Date.Value && !b.Cancelled.HasValue)
                        .OrderBy(b => b.Date).ToListAsync();

            var Reservations = await db.Slots.OfType<Reservation>()
                        .Include(b => b.Resource)
                        .Where(b => DbFunctions.TruncateTime(b.Date) == Date.Value && b.Reason != ReservationReason.Booking_in_progress)
                        .OrderBy(b => b.Date).ToListAsync();

            var Resources = await db.Resources.ToListAsync();

            var StartDate = new DateTime(Date.Value.Year, Date.Value.Month, 1);
            var EndDate = new DateTime(StartDate.Year, StartDate.Month, DateTime.DaysInMonth(StartDate.Year, StartDate.Month));

            while (StartDate.DayOfWeek != DayOfWeek.Monday)
            {
                StartDate = StartDate.AddDays(-1);
            }

            while (EndDate.DayOfWeek != DayOfWeek.Sunday)
            {
                EndDate = EndDate.AddDays(1);
            }

            ViewBag.ActiveMonth = new DateTime(Date.Value.Year, Date.Value.Month, 1);
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.BankHolidays = await Data.BankHolidays();

            return View(new BookingViewModel
                {
                    Bookings = Bookings,
                    Reservations = Reservations,
                    Resources = Resources
                });
        }
        
        [HttpGet]
        [Route("{id:int?}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var booking = await db.Slots.FindAsync(id) as Booking;
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        //// GET: Bookings/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Booking booking = await db.Slots.FindAsync(id) as Booking;
        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ResourceId = new SelectList(db.Resources, "Id", "Name", booking.ResourceId);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Name", booking.CustomerId);
        //    ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name", booking.ServiceId);
        //    return View(booking);
        //}

        //// POST: Bookings/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,ResourceId,Date,Status,ServiceId,VehicleMake,VehicleModel,VehicleRegistration,VehiclePlate,CustomerId,Name,Telephone,Inspector,TestSerialNumber,InspectorNotes,BookedBy,Cancelled,PriceToPay,PaymentRef,OverridePayment,CollectPaymentAtEvent")] Booking booking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(booking).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ResourceId = new SelectList(db.Resources, "Id", "Name", booking.ResourceId);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Name", booking.CustomerId);
        //    ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name", booking.ServiceId);
        //    return View(booking);
        //}

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
