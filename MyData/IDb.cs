﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyData.Models;
using MyData;

namespace MyData
{
    public interface IDb
    {
        void        Add<T>(T data);
        void        Remove<T>(T data);

        bool        IpSessionIdExists(string sessionId, string ip);
        bool        SessionIdExists(string aspSessionId);

        List<RequestLogEntry> GetRequestLogs    (int nr);
        List<RequestLogEntry> GetRequestLog     (int nr, string SessionId);

        RequestLogEntry     FindRequestLog  (int id);
        List<PostbackData>  GetPostbacks(int nr);
        List<PostbackData>  GetPostbacks(int nr, string SessionId);

        PostbackData FindPostback    (int id);
        IpSessionId         FindIpSessionId (int id);

        List<PostbackData>  GetPostbacksFromToday();
        void Commit();
        void Dispose();
    }
}
