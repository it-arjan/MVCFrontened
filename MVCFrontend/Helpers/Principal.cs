using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class Principal
    {
        public static bool isAdmin(this ClaimsPrincipal cp)
        {
            return cp.HasClaim(c => c.Type == "role" && c.Value == "admin");
        }
        public static string NameClaim(this ClaimsPrincipal cp)
        {
            if (cp.Claims.Where(c => c.Type == "name").Any())
                return cp.Claims.Where(c => c.Type == "name").First().Value;
            return "name claim not set (yet)";
        }
    }
}