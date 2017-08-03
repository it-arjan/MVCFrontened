using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class ErrorHandling
    {
        public static string GetviewForStatusCode(int statuscode)
        {
            var viewname = string.Empty;
            switch (statuscode)
            {
                case 500:
                    viewname = "~/Views/Error/Error_500.cshtml";
                    break;
                case 404:
                    viewname = "~/Views/Error/Error_404.cshtml";
                    break;
                case 401:
                    viewname = "~/Views/Error/Error_401.cshtml";
                    break;
                default:
                    viewname = "~/Views/Error/Error.cshtml";
                    break;
            }

            return viewname;
        }
    }
}