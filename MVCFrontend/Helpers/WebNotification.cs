using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using NLogWrapper;
using MVCFrontend.Helpers;

namespace MVCFrontend.Helpers
{
    public static class WebNotification
    {
        private static ILogger _logger = LogManager.CreateLogger(typeof(WebNotification), Appsettings.LogLevel());

        public static void Send(string sessionToken, string msg, params object[] msgPars)
        {
           
            var _wsClient = Connect(Appsettings.SocketServerUrl());
            if (Connected(_wsClient))
            {
                var tokSrc = new CancellationTokenSource();

                string total_msg = string.Format("{0}#-_-_-#Azure Backend: {1}", sessionToken, string.Format(msg, msgPars));
                var tsk = _wsClient.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(total_msg)),
                                                    WebSocketMessageType.Text,
                                                    true,
                                                    tokSrc.Token
                                                    );
                tsk.Wait(); tsk.Dispose();
                tokSrc.Dispose();
                Close(_wsClient);
            }
        }

        private static ClientWebSocket Connect(string url)
        {
            _logger.Info("Connecting to {0}", url);
            var _wsClient = new ClientWebSocket();

            if (Appsettings.Ssl())
            {
                _logger.Info("Loading certificate from file");
                _wsClient.Options.ClientCertificates.Add(Security.GetCertificateFromFile());
            }
            var tokSrc = new CancellationTokenSource();
            //cannot use await within lock
            var task = _wsClient.ConnectAsync(new Uri(url), tokSrc.Token);
            task.Wait(); task.Dispose();

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
                tsk.Wait(); tsk.Dispose();
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