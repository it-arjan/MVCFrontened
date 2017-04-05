﻿using System;
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
            return string.Format("{0}://{1}", Scheme(), Entrypoint());
        }
        public static string Entrypoint()
        {
            return GetWithSlash(EntrypointKey);
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
            return ConfigurationManager.AppSettings.Get(LogLevelKey);
        }
        // IgnoreSslErrorsKey
        public static bool AzureIgnoreCertificateErrors()
        {
            if (ConfigurationManager.AppSettings.Get(AzureIgnoreCertificateErrorsKey) != null)
                return Convert.ToBoolean(ConfigurationManager.AppSettings.Get(AzureIgnoreCertificateErrorsKey));
            return false;
        }

        private static string GetWithSlash(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            return value.Trim().EndsWith("/") ? value : value.Trim() + "/";
        }
    }
}
