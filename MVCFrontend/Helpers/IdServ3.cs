using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class IdSrv3
    {
        public const string ScopeMcvFrontEndHuman = "mvc-frontend-human";

        public const string ScopeMvcFrontEnd = "mvc-frontend-silicon";
        public const string ScopeEntryQueueApi = "entry-queue-api";
        public const string ScopeNancyApi = "nancy-api";
        public const string ScopeServiceStackApi = "servicestack-api";
        public const string ScopeWcfService = "wcf-service";
        public const string ScopeMsWebApi = "ms-webapi2";

        public static string UniqueClaimOfAntiForgeryToken = "given_name";

        public static int CookieTimeoutSecs = 60; 
        public static int SessionRefreshTimeoutSecs = 120;

        public static double GetRemainingSessionSecsFromAppCookie()
        {
            double val = -17;
            if (HttpContext.Current.Request.Cookies[".AspNet.ApplicationCookie"] != null)
            {
                val = CookieTimeoutSecs - (DateTime.Now - HttpContext.Current.Request.Cookies[".AspNet.ApplicationCookie"].Expires).TotalSeconds;
            }
            return val;
        }
    }
}