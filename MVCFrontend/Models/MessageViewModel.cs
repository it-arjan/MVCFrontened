using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class MessageViewModel
    {
        public string AjaxAccessToken;
        public string AjaxQueueAccessToken;
        public string SocketToken;
        public string DoneToken;
        public ApiResultModel ApiResult;
        public string UserName;
        public string Roles;
    }
}