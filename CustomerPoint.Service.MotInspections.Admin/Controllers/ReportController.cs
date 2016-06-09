using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;
using CustomerPoint.Service.MotInspections.Admin.Models;
using GuildfordBoroughCouncil.Linq;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("report")]
    public class ReportController : Controller
    {
        private MotData db = new MotData();

        [Route]
        public ActionResult Index()
        {
            ViewBag.Customer = new SelectList(db.Customers.OrderBy(c => c.Name), "Id", "Name");
            if (ControllerContext.IsChildAction)
            {
                return PartialView(new ReportModel());
            }
            return View(new ReportModel());
        }

        [Route]
        [HttpPost]
        public ActionResult Index(ReportModel r)
        {
            if (ModelState.IsValid)
            {
                var Bookings = db.Slots.OfType<Booking>().Where(b => (DateTime.Compare(r.From, DbFunctions.TruncateTime(b.Date).Value) <= 0) && (DateTime.Compare(r.To, DbFunctions.TruncateTime(b.Date).Value) >= 0) && !b.Cancelled.HasValue)
                    .WhereIf(r.Customer.HasValue, b => b.CustomerId == r.Customer)
                    .WhereIf(r.Status.HasValue, b => b.Status == r.Status.Value)
                    .Include("Service").Include("Customer");

                ViewBag.From = r.From;
                ViewBag.To = r.To;

                return View(r.Action, Bookings);
            }

            ViewBag.Customer = new SelectList(db.Customers.OrderBy(c => c.Name), "Id", "Name", r.Customer);
            return View();
        }

        [Route("~/prices")]
        [HttpGet]
        public async Task<ActionResult> Prices()
        {
            var Services = await db.ServiceCustomers.GroupBy(c => c.Customer.Name).ToListAsync();

            return View(Services);
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