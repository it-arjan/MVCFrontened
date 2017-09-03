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
using System.Threading.Tasks;

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
        public async Task<ActionResult> Index()
        {
            if (ClaimsPrincipal.Current.isAdmin())
            {
                return PartialView(await db.GetRecentPostbacksAsync(50));
            }
            else
            {
                return PartialView(await db.GetRecentPostbacksAsync(50, Session.SessionID));
            }
        }

        // GET: PostbackDatas/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = await db.FindPostbackAsync(Convert.ToInt32(id)); 
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

        // GET: PostbackDatas/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostbackData postbackData = await db.FindPostbackAsync(Convert.ToInt32(id));
            if (postbackData == null)
            {
                return HttpNotFound();
            }
            return View(postbackData);
        }

         // POST: PostbackDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            int intPk = Convert.ToInt16(id);
            await db.RemovePostbackAsync(intPk);
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
