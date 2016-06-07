using System.Linq;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;

namespace CustomerPoint.Service.MotInspections.Controllers
{
    [RoutePrefix("history")]
    public class HistoryController : Controller
    {
        private MotData db = new MotData();

        [Route]
        public PartialViewResult Vehicle(string vrm)
        {
            vrm = vrm.ToUpper().Replace(" ", "");

            var Bookings = db.Slots.OfType<Booking>().Where(b => b.VehicleRegistration == vrm).OrderBy(b => b.Date);

            return PartialView(Bookings);
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