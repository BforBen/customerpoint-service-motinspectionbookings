using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;
using CustomerPoint.Service.MotInspections.Admin.Models;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("settings/customers")]
    public class CustomerController : Controller
    {
        private MotData db = new MotData();

        [Route]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var customers = db.Customers;
            return View(await customers.OrderBy(c => c.Name).ToListAsync());
        }

        [Route("add")]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("add")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,Hidden,LedgerCode")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        [Route("{id:int}/edit")]
        [HttpGet]
        public async Task<ActionResult> Edit(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [Route("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Hidden,LedgerCode")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        [Route("{id:int}/services")]
        [HttpGet]
        public async Task<ActionResult> Services(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = await db.Customers.FindAsync(id);
            
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new UpdateCustomerServices();

            model.Customer = customer;
            model.Services = await db.Services.Where(s => s.Services.Count() > 0).ToListAsync();
            model.CustomerServices = customer.Services.ToList();

            return View(model);
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