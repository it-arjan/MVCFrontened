﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MyData;
using MyData.Models;
using System.Security.Claims;
using MVCFrontend.Extentions;
using MVCFrontend.Filters;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class PostbackDatasController : Controller
    {
        private IDb db = DbFactory.Db();

        // GET: PostbackDatas
        public ActionResult Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.GetPostbacks(50).OrderByDescending(c=>c.Start).ToList());
            }
            else
            {
                return PartialView(db.GetPostbacks(50, Session.SessionID).OrderByDescending(c => c.End).ToList());
            }
        }

        // GET: PostbackDatas/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = db.FindPostback(Convert.ToInt32(id)); 
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
                db.Commit();
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
            PostbackData postbackData = db.FindPostback(Convert.ToInt32(id));
            if (postbackData == null)
            {
                return HttpNotFound();
            }
            return View(postbackData);
        }

         // POST: PostbackDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            int intPk = Convert.ToInt16(id);
            PostbackData postbackData = db.FindPostback(intPk);
            db.Remove(postbackData);
            db.Commit();
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
