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
using MVCFrontend.Overrides.Filters;
using NLogWrapper;
using MVCFrontend.Helpers;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class RequestLogEntriesController : MyBaseController
    {
        private IData db;
        public RequestLogEntriesController(ILogger logger, IMakeStaticsMockable injectMockMe) : base(logger)
        {
            IdSrv3.EnsureTokenClaimIsValid("data_api_token");
            db = new DataFactory(MyDbType.ApiDbNancy).Db(
                        Configsettings.DataApiUrl(),
                        ClaimsPrincipal.Current.GetClaimValue("data_api_token"),
                        ClaimsPrincipal.Current.GetClaimValue("api_feed_socket_id")
                ); 
        }

        // GET: RequestLogEntries
        public ActionResult Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(db.GetRecentRequestLogs(50));
            }

            return PartialView(db.GetRecentRequestLogs(50, Session.SessionID));
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
            db.RemoveRequestlog(id);
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
