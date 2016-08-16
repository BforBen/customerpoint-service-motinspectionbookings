using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("settings/services")]
    public class ServiceController : Controller
    {
        private MotData db = new MotData();

        [Route("{service?}")]
        [HttpGet]
        public async Task<ActionResult> Index(string service = null)
        {
            ViewBag.Parent = true;
            var services = await db.Services.Where(s => !s.ParentId.HasValue).ToListAsync();

            if (!string.IsNullOrWhiteSpace(service))
            {
                var srv = services.Where(s => s.Slug == service).SingleOrDefault();

                if (srv != null)
                {
                    services = await db.Services.Where(s => s.ParentId == srv.Id).ToListAsync();
                    ViewBag.Parent = false;
                }
            }

            return View(services.OrderBy(s => s.Name));
        }

        [Route("add")]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.Services.Where(s => !s.ParentId.HasValue), "Id", "Name");

            return View();
        }

        [HttpPost]
        [Route("add")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ParentId,Name,Description,Charge,DisplayOrder")] MotInspections.Models.Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Services.Where(s => !s.ParentId.HasValue), "Id", "Name", service.ParentId);

            return View(service);
        }

        [Route("{id:int}/edit")]
        [HttpGet]
        public async Task<ActionResult> Edit(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var service = await db.Services.FindAsync(id);

            if (service == null)
            {
                return HttpNotFound();
            }

            ViewBag.ParentId = new SelectList(db.Services.Where(s => !s.ParentId.HasValue && s.Id != id), "Id", "Name", service.ParentId);

            return View(service);
        }

        [HttpPost]
        [Route("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentId,Name,Description,Charge,DisplayOrder")] MotInspections.Models.Service service)
        {
            if (ModelState.IsValid)
            {
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Services.Where(s => !s.ParentId.HasValue && s.Id != service.Id), "Id", "Name", (service != null ? service.ParentId : null));

            return View(service);
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