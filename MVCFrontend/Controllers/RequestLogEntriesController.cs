﻿using System;
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
using System.Threading.Tasks;

namespace MVCFrontend.Controllers
{
    [LogRequests]
    [Authorize]
    public class RequestLogEntriesController : MyBaseController
    {
        private IData db;
        public RequestLogEntriesController(ILogger logger, IMakeStaticsMockable injectMockMe) : base(logger)
        {
            IdSrv3.EnsureTokenClaimIsValid(IdSrv3.ClaimApiToken);
            db = new DataFactory(MyDbType.ApiDbNancy).Db(
                        Configsettings.DataApiUrl(),
                        ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiToken),
                            ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess),
                        ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimApiFeedId)
                ); 
        }

        // GET: RequestLogEntries
        public async Task<ActionResult> Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(await db.GetRecentRequestLogsAsync(50));
            }

            return PartialView(await db.GetRecentRequestLogsAsync(50, Session.SessionID));
        }

        // GET: RequestLogEntries/Details/5
        public async Task<ActionResult> Details(int? id)
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
            RequestLogEntry requestLogEntry = await db.FindRequestLogAsync((int)id);
            if (requestLogEntry == null)
            {
                return HttpNotFound();
            }
            return PartialView(requestLogEntry);
        }

         // POST: RequestLogEntries/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            if (!ClaimsPrincipal.Current.isAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            await db.RemoveRequestlogAsync(id);
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
