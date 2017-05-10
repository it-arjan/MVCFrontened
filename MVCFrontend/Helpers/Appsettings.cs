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
        public const string AuthServerKey = "auth.server";
        public const string AuthSessionLengthKey = "authserver.session.minutes";

        public const string CookieTimeoutKey = "cookie.timeout.minutes";
        public const string CookieSlidingExpirationKey = "cookie.sliding.expire";
        public const string CookieTimeoutExpireOffsetKey = "cookie.expire.offset";
        

        public const string SessionMaxAgeKey = "idsrv3.session.maxage.minutes";
        public const string UseTokenLifetimeKey = "idsrv3.use.token.lifetime.instead.of.cookie.middleware";
        
        public const string SocketPortKey = "websocket.port";
        public const string SocketSchemeKey = "websocket.scheme";
        public const string SocketServerHostnameKey = "websocket.server.hostname";
        public const string HostnameKey = "hostname";
        public const string EntrypointKey = "entrypoint.server";
        public const string SiliconClientIdKey = "SiliconClientId";
        public const string SiliconClientSecretKey = "SiliconClientSecret";
        public const string FrontendClientIdKey = "FrontendClientId";
        public const string LogLevelKey = "log.level";
        public const string AzureIgnoreCertificateErrorsKey = "azure.ignore.cert.errors";
        public const string LogRequestIgnoreIpListKey = "log.request.ignore.ip.csv";


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
        public static string EntrypointUrl()
        {
            return string.Format("{0}://{1}/", Scheme(), Entrypoint());
        }
        public static string Entrypoint()
        {
            return ConfigurationManager.AppSettings.Get(EntrypointKey);
        }

        public static string SocketServerUrl()
        {
            return string.Format("{0}://{1}:{2}", SocketScheme(), SocketServerHostname(), SocketPort());
        }

        public static string Scheme()
        {
            return ConfigurationManager.AppSettings.Get(SchemeKey);
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

        public static string LogLevel()
        {
            return ConfigurationManager.AppSettings.Get(LogLevelKey).ToLower(); 
        }
        
        public static List<string> LogRequestIgnoreIpList()
        {
            return ConfigurationManager.AppSettings.Get(LogRequestIgnoreIpListKey).Replace(" ",string.Empty).Split(',').ToList<string>();
        }
        public static bool AzureIgnoreCertificateErrors()
        {
            return GetBoolSetting(AzureIgnoreCertificateErrorsKey);
        }
        public static int CookieTimeoutExpireOffset()
        {
            return GetIntSetting(CookieTimeoutExpireOffsetKey);
        }
        public static int CookieTimeoutMinutes()
        {
            return GetIntSetting(CookieTimeoutKey);
        }
        public static bool CookieSlidingExpiration()
        {
            return GetBoolSetting(CookieSlidingExpirationKey);
        }
        public static bool UseTokenLifetime()
        {
            return GetBoolSetting(UseTokenLifetimeKey);
        }

        public static int SessionMaxAgeMinutes()
        {
            return GetIntSetting(SessionMaxAgeKey);
        }
        
        private static bool GetBoolSetting(string key)
        {
            if (ConfigurationManager.AppSettings.Get(key) != null)
                return Convert.ToBoolean(ConfigurationManager.AppSettings.Get(key));
            return false;
        }

        private static int GetIntSetting(string key)
        {
            if (ConfigurationManager.AppSettings.Get(key) != null)
                return Convert.ToInt16(ConfigurationManager.AppSettings.Get(key));
            return 0;
        }
        private static string GetWithSlash(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            return value.Trim().EndsWith("/") ? value : value.Trim() + "/";
        }
    }
}
