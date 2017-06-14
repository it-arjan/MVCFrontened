using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using NLogWrapper;
using MVCFrontend.Helpers;

namespace MVCFrontend.WebSocket
{
    public class WebNotification2
    {
        private ClientWebSocket _wsClient;
        private ILogger _logger = LogManager.CreateLogger(typeof(WebNotification2), Configsettings.LogLevel());
        object _serializer = new object();
        public WebNotification2()
        {
            // TODO see if we can make it a static helper class
        }

        public void Send(string socketServerUrl, string sessionToken, string msg, params object[] msgPars)
        {
           
            lock (_serializer)
            {
                var _wsClient = Connect(socketServerUrl);
                if (Connected)
                {
                    var tokSrc = new CancellationTokenSource();

                    string total_msg = string.Format("{0}#-_-_-#-Queue Manager: {1}", sessionToken, string.Format(msg, msgPars));
                    var tsk = _wsClient.SendAsync(
                                   new ArraySegment<byte>(Encoding.UTF8.GetBytes(total_msg)),
                                                       WebSocketMessageType.Text,
                                                       true,
                                                       tokSrc.Token
                                                       );
                    tsk.Wait(); tsk.Dispose();
                    tokSrc.Dispose();
                    Close();
                }
            }
        }

        private ClientWebSocket Connect(string url)
        {
            _logger.Info("Connecting to {0}", url);
            _wsClient = new ClientWebSocket();

            if (Configsettings.Ssl())
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

        private void Close()
        {
            lock (_serializer)
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
        }

        public bool Connected
        {
            get { return _wsClient != null && _wsClient.State == WebSocketState.Open; }
        }
    }
}