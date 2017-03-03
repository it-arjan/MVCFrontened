using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCFrontend.DAL;
using MVCFrontend.Models;
using System.Security.Claims;
using MVCFrontend.Helpers;

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class PostbackDatasController : Controller
    {
        private FrontendDbContext db = new FrontendDbContext();

        // GET: PostbackDatas
        public ActionResult Index()
        {
            //principal.g
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.Postbacks.OrderByDescending(c=>c.Started).ToList());
            }
            else
            {
                var userName = ClaimsPrincipal.Current.Claims.Where(c => c.Type == "given_name").Select(c => c.Value).FirstOrDefault();
                return PartialView(db.Postbacks.Where(p => p.UserName == userName && p.Started >= DateTime.Today).OrderByDescending(c => c.Started).ToList());
            }
        }

        // GET: PostbackDatas/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = db.Postbacks.Find(Convert.ToInt32(id)); //string I have to edit generated code that worked before
            if (postbackData == null)
            {
                return HttpNotFound();
            }
            return View(postbackData);
        }

        // GET: PostbackDatas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PostbackDatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StartTime,Duration,UserName,Content")] PostbackData postbackData)
        {
            if (ModelState.IsValid)
            {
                db.Postbacks.Add(postbackData);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(postbackData);
        }

        // GET: PostbackDatas/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = db.Postbacks.Find(id);
            if (postbackData == null)
            {
                return HttpNotFound();
            }
            return View(postbackData);
        }

        // POST: PostbackDatas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StartTime,Duration,UserName,Content")] PostbackData postbackData)
        {
            if (ModelState.IsValid)
            {
                db.Entry(postbackData).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(postbackData);
        }

        // GET: PostbackDatas/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = db.Postbacks.Find(id);
            if (postbackData == null)
            {
                return HttpNotFound();
            }
            return View(postbackData);
        }

        // POST: PostbackDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PostbackData postbackData = db.Postbacks.Find(id);
            db.Postbacks.Remove(postbackData);
            db.SaveChanges();
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
