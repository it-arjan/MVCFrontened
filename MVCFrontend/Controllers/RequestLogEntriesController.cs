using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
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
    public class RequestLogEntriesController : Controller
    {
        private IDb db = DbFactory.Db();

        // GET: RequestLogEntries
        public ActionResult Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.GetRequestLogs(50).OrderByDescending(rq => rq.Timestamp).ToList());
            }

            return PartialView(db.GetRequestLog(50, Session.SessionID).OrderByDescending(rq => rq.Timestamp).ToList());
        }

        // GET: RequestLogEntries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!ClaimsPrincipal.Current.isAdmin())
            {
                return new ContentResult
                {
                    Content = "You seem to be lacking the admin role",
                    ContentType = "text/html"
                };
            }
            RequestLogEntry requestLogEntry = db.FindRequestLog((int)id);
            if (requestLogEntry == null)
            {
                return HttpNotFound();
            }
            return PartialView(requestLogEntry);
        }

         // GET: RequestLogEntries/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    if (!ClaimsPrincipal.Current.isAdmin())
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
        //    }
        //    RequestLogEntry requestLogEntry = db.RequestLogEntries.Find(id);
        //    if (requestLogEntry == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(requestLogEntry);
        //}

        // POST: RequestLogEntries/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!ClaimsPrincipal.Current.isAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            RequestLogEntry requestLogEntry = db.FindRequestLog(id);
            db.Remove(requestLogEntry);
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
