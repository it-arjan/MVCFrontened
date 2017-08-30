using System;
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
using MVCFrontend.Overrides.Filters;
using NLogWrapper;
using MVCFrontend.Helpers;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class PostbackDatasController : MyBaseController
    {
        private IData db;
        public PostbackDatasController(ILogger logger, IMakeStaticsMockable injectMockMe) : base(logger)
        {
            IdSrv3.EnsureTokenClaimIsValid(IdSrv3.ClaimApiToken);
            db = new DataFactory(MyDbType.ApiDbNancy).Db(
                        Configsettings.DataApiUrl(), 
                        ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiToken),
                            ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess),
                        ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiFeedId)
            );
    }

    // GET: PostbackDatas
    public ActionResult Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.GetRecentPostbacks(50));
            }
            else
            {
                return PartialView(db.GetRecentPostbacks(50, Session.SessionID));
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
                db.AddPostback(postbackData);
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
            db.RemovePostback(intPk);
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
