using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MVCFrontend.Helpers
{
    public static class Appsettings
    {
        public const string SchemeKey = "scheme";
        public const string AuthServerKey = "authserver";
        public const string AuthSessionLengthKey = "authserver.session.minutes";
        public const string SocketPortKey = "websocket.port";
        public const string SocketSchemeKey = "websocket.scheme";
        public const string SocketServerHostnameKey = "websocket.server.hostname";
        
        public const string HostnameKey = "hostname";
        public const string QueueApiUrlKey = "queue.deliver.api";

        public const string SiliconClientIdKey = "SiliconClientId";
        public const string SiliconClientSecretKey = "SiliconClientSecret";

        public const string FrontendClientIdKey = "FrontendClientId";

        public static string AuthUrl()
        {
            return string.Format("{0}://{1}", Scheme(), AuthServer());
        }
        public static string AuthServer()
        {
            return GetWithSlash(AuthServerKey);
        }

        public static string HostUrl()
        {
            return string.Format("{0}://{1}/", Scheme(), Hostname());
        }
        public static string QueueApiUrl()
        {
            return string.Format("{0}://{1}", Scheme(), QueueApi());
        }
        public static string QueueApi()
        {
            return GetWithSlash(QueueApiUrlKey);
        }

        public static string SocketServerUrl()
        {
            return string.Format("{0}://{1}:{2}", SocketScheme(), SocketServerHostname(), SocketPort());
        }

        public static string Scheme()
        {
            return ConfigurationManager.AppSettings.Get(SchemeKey);
        }

        public static int AuthSessionLengthMinutes()
        {
            return Convert.ToInt16(ConfigurationManager.AppSettings.Get(AuthSessionLengthKey));
        }
        public static string SocketServerHostname()
        {
            return ConfigurationManager.AppSettings.Get(SocketServerHostnameKey);
        }

        public static string Hostname()
        {
            return ConfigurationManager.AppSettings.Get(HostnameKey);
        }

        private static string SocketPort()
        {
            return ConfigurationManager.AppSettings.Get(SocketPortKey);
        }
        public static string SocketScheme()
        {
            return ConfigurationManager.AppSettings.Get(SocketSchemeKey);
        }

        public static string SiliconClientId()
        {
            return ConfigurationManager.AppSettings.Get(SiliconClientIdKey);
        }

        public static string SiliconClientSecret()
        {
            return ConfigurationManager.AppSettings.Get(SiliconClientSecretKey);
        }
        public static string FrontendClientId()
        {
            return ConfigurationManager.AppSettings.Get(FrontendClientIdKey);
        }

        private static string GetWithSlash(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            return value.Trim().EndsWith("/") ? value : value.Trim() + "/";
        }
    }
}
