using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CustomerPoint.Service.MotInspections.Models;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("settings/resources")]
    public class ResourceController : Controller
    {
        private MotData db = new MotData();

        [HttpGet]
        [Route]
        public async Task<ActionResult> Index(int? parent = null)
        {
            ViewBag.Parent = true;
            var resources = await db.Resources.Where(r => !r.ParentId.HasValue).ToListAsync();

            if (parent.HasValue)
            {
                resources = await db.Resources.Where(r => r.ParentId == parent).ToListAsync();
                ViewBag.Parent = false;
            }

            return View(resources.OrderBy(r => r.Name));
        }

        [HttpGet]
        [Route("{id:int?}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resource resource = await db.Resources.FindAsync(id);
            if (resource == null)
            {
                return HttpNotFound();
            }
            return View(resource);
        }

        [Route("add")]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.Resources.Where(s => !s.ParentId.HasValue), "Id", "Name");
            return View();
        }

        [Route("add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Start,End,ParentId")] Resource resource)
        {
            if (ModelState.IsValid)
            {
                db.Resources.Add(resource);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Resources.Where(s => !s.ParentId.HasValue), "Id", "Name", resource.ParentId);
            return View(resource);
        }

        [Route("{id:int}/edit")]
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var resource = await db.Resources.FindAsync(id);

            if (resource == null)
            {
                return HttpNotFound();
            }

            ViewBag.ParentId = new SelectList(db.Resources.Where(s => !s.ParentId.HasValue && s.Id != id), "Id", "Name", resource.ParentId);

            return View(resource);
        }

        [HttpPost]
        [Route("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Start,End,ParentId")] Resource resource)
        {
            if (ModelState.IsValid)
            {
                db.Entry(resource).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Resources.Where(s => !s.ParentId.HasValue && s.Id != resource.Id), "Id", "Name", resource.ParentId);

            return View(resource);
        }

        // GET: Resources/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resource resource = await db.Resources.FindAsync(id);
            if (resource == null)
            {
                return HttpNotFound();
            }
            return View(resource);
        }

        // POST: Resources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Resource resource = await db.Resources.FindAsync(id);
            db.Resources.Remove(resource);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
