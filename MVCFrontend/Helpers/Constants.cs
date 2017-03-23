using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class IdSrv3
    {
        public static string ScopeEntryQueueApi = "EntryQueueApi";
        public static string ScopeMcvFrontEndHuman = "MvcFrontEndHuman";
        public static string ScopeMcvFrontEnd = "MvcFrontEnd";
        public static string UniqueClaimOfAntiForgeryToken = "given_name";
        public static int SessionSetting = 1; // search refs for all session time related settings
        public static int CookieTimeoutSecs = 900; 
        public static int SessionRefreshTimeoutSecs = 1800;
    }
}