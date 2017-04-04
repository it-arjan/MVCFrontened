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
        public static string GetClaim(this ClaimsPrincipal cp, string claimType)
        {
            if (cp.Claims.Where(c => c.Type == claimType).Any())
                return cp.Claims.Where(c => c.Type == claimType).First().Value;
            return claimType + " claim not set (yet)";
        }
    }
}