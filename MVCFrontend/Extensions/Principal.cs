using Microsoft.Owin.Security;
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
            return claimType + " not set (yet)";
        }

        public static List<string> GetAllClaims(this ClaimsPrincipal cp, string claimType)
        {
            return cp.Claims.Where(c => c.Type == claimType)
                     .Select(c => c.Value).ToList();
        }

        public static void AddUpdateClaim(this ClaimsPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return;

            // check for existing claim and remove it
            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);

            // add new claim
            identity.AddClaim(new Claim(key, value));
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties() { IsPersistent = true });
        }
    }
}