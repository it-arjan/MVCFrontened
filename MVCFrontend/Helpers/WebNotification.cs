using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using NLogWrapper;
using MVCFrontend.Extentions;
using System.Security.Claims;

namespace MVCFrontend.Helpers
{
    public static class WebNotification
    {
        private static ILogger _logger = LogManager.CreateLogger(typeof(WebNotification), Configsettings.LogLevel());
        private static object serializer = new object();
        public static void Send(string feedId, string msg, params object[] msgPars)
        {
            lock (serializer)
            {
                var _wsClient = Connect(Configsettings.SocketServerUrl(), ClaimsPrincipal.Current.GetClaimValue(IdSrv3.ClaimScoketAccess));
                if (Connected(_wsClient))
                {

                    string total_msg = string.Format("{0}#-_-_-#Backend notification: {1}", feedId, string.Format(msg, msgPars));
                    var tokSrc = new CancellationTokenSource();
                    var tsk = _wsClient.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(total_msg)),
                                                        WebSocketMessageType.Text,
                                                        true,
                                                        tokSrc.Token
                                                        );
                    if (!tsk.IsFaulted) tsk.Wait();
                    tsk.Dispose();
                    tokSrc.Dispose();
                    Close(_wsClient);
                }
            }
        }

        private static ClientWebSocket Connect(string url, string accessToken)
        {
            var _wsClient = new ClientWebSocket();
            _wsClient.Options.SetRequestHeader("Sec-WebSocket-Protocol", accessToken);
            _logger.Info("Connecting to {0}", url);

            var tokSrc = new CancellationTokenSource();
            // We cannot use await within lock
            var task = _wsClient.ConnectAsync(new Uri(url), tokSrc.Token);
            if (!task.IsFaulted) task.Wait();
            task.Dispose();

            _logger.Info("Opened ClientWebSocket to {0}", url);
            tokSrc.Dispose();
            return _wsClient;
        }

        private static void Close(ClientWebSocket _wsClient)
        {
            if (_wsClient.State == WebSocketState.Open)
            {
                var tokSrc = new CancellationTokenSource();
                var tsk = _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", tokSrc.Token);
                if (!tsk.IsFaulted) tsk.Wait();
                tsk.Dispose();
                tokSrc.Dispose();
            }
            _logger.Debug("ClientWebSocket CLOSED");
        }

        public static bool Connected(ClientWebSocket _wsClient)
        {
             return _wsClient != null && _wsClient.State == WebSocketState.Open; 
        }
    }
}