using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Data;
using Data.Models;
using System.Security.Claims;
using MVCFrontend.Extentions;
using Data;

namespace MVCFrontend.Controllers
{
    [Authorize]
    public class PostbackDatasController : Controller
    {
        private IDb db = DbFactory.Db();

        // GET: PostbackDatas
        public ActionResult Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.GetEtfdb().Postbacks.OrderByDescending(c=>c.Start).ToList());
            }
            else // only postbacks of the current user
            {
                var userName = ClaimsPrincipal.Current.Claims.Where(c => c.Type == "given_name").Select(c => c.Value).FirstOrDefault();
                return PartialView(db.GetEtfdb().Postbacks.Where(p => p.UserName == userName && p.Start >= DateTime.Today).OrderByDescending(c => c.End).ToList());
            }
        }

        // GET: PostbackDatas/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = db.GetEtfdb().Postbacks.Find(Convert.ToInt32(id)); //string I have to edit generated code that worked before
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
        public ActionResult Create([Bind(Include = "Id,Duration,UserName,Content")] PostbackData postbackData)
        {
            if (ModelState.IsValid)
            {
                db.Add(postbackData);
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
            PostbackData postbackData = db.GetEtfdb().Postbacks.Find(id);
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
        public ActionResult Edit([Bind(Include = "Id,Duration,UserName,Content")] PostbackData postbackData)
        {
            if (ModelState.IsValid)
            {
                db.GetEtfdb().Entry(postbackData).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(postbackData);
        }

        // POST: PostbackDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            int intPk = Convert.ToInt16(id);
            PostbackData postbackData = db.GetEtfdb().Postbacks.Find(intPk);
            db.GetEtfdb().Postbacks.Remove(postbackData);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.GetEtfdb().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
