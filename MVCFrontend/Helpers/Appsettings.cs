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
        public static string AuthUrlKey()
        {
            return string.Format("facing.{0}.authserver", ConfigurationManager.AppSettings.Get("facing").ToLower());
        }

        public static string HostUrlKey()
        {
            return string.Format("facing.{0}.host.url", ConfigurationManager.AppSettings.Get("facing").ToLower());
        }

        public static string QueueApiUrlKey()
        {
            return string.Format("facing.{0}.queue.api", ConfigurationManager.AppSettings.Get("facing").ToLower());
        }

        public static string SocketServerUrlKey()
        {
            return string.Format("facing.{0}.socketserver.url", ConfigurationManager.AppSettings.Get("facing").ToLower());
        }

        public static string AuthUrl()
        {
            var key = AuthUrlKey();
            return GetWithSlash(key);
        }
        public static string HostUrl()
        {
            var key = HostUrlKey();
            return GetWithSlash(key);
        }

        public static string QueueApiUrl()
        {
            var key = QueueApiUrlKey();
            return GetWithSlash(key);
        }
        public static string SocketServerUrl()
        {
            var key = SocketServerUrlKey();
            return GetWithSlash(key);
        }

        public static string SiliconClientId()
        {
            return ConfigurationManager.AppSettings.Get("SiliconClientId");
        }

        public static string SiliconClientSecret()
        {
            return ConfigurationManager.AppSettings.Get("SiliconClientSecret");
        }

        private static string GetWithSlash(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            return value.Trim().EndsWith("/") ? value : value.Trim() + "/";
        }
    }
}
