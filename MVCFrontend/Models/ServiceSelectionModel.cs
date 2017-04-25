using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class ServiceSelectionModel
    {
        public ServiceSelectionModel()
        {
            Services = new List<WebService>();
        }
        public List<WebService> Services { get; private set; }
    }

}