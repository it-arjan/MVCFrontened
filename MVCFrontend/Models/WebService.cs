using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class WebService
    {
        public int Id { get; set; }
        public string Prompt { get; set; }
        public bool Selected { get; set; }
    }
}