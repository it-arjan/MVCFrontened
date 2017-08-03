using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class MessageViewModel
    {
        //public string AjaxBackendToken;
        public DateTime CookieExpTime;
        //public string AjaxDirectQueueToken;
        public DateTime AjaxDirectQueueTokenExpTime;

        public string SocketToken;
        public string DoneToken;
        public string UserName;
        public string Roles;
        public bool LogDropRequest;

    }
}