using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class MessageViewModel
    {
        public string AjaxBackendToken;
        public int AjaxBackendTokenExpsecs;
        public string AjaxDirectQueueToken;
        public int AjaxDirectQueueTokenExpsecs;

        public string SocketToken;
        public string DoneToken;
        public ApiResultModel ApiResult;
        public string UserName;
        public string Roles;
    }
}