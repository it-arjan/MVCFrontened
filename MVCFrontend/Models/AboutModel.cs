using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class AboutModel
    {
        public IEnumerable<System.Security.Claims.Claim> Claims { get; set; }
        public DateTime TokenSessionStart;
        public DateTime TokenSessionEnd;
    }
}