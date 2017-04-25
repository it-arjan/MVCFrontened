using IdentityModel.Client;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class IdSrv3
    {
        public const string ScopeMcvFrontEndHuman = "mvc-frontend-human";

        public const string ScopeMvcFrontEnd = "mvc-frontend-silicon";
        public const string ScopeEntryQueueApi = "entry-queue-api";
        public const string ScopeNancyApi = "nancy-api";
        public const string ScopeServiceStackApi = "servicestack-api";
        public const string ScopeWcfService = "wcf-service";
        public const string ScopeMsWebApi = "ms-webapi2";

        public static string UniqueClaimOfAntiForgeryToken = "given_name";

        public static int CookieTimeoutSecs = 3600; 
        public static int SessionRefreshTimeoutSecs = 3600;

        private static ILogger _logger = LogManager.CreateLogger(typeof(IdSrv3));

        public static string NewSiliconClientToken(string scope)
        {
            var tokenUrl = string.Format("{0}connect/token", Appsettings.AuthUrl());
            _logger.Debug("Getting a new Client Token for scope {1} at {0} ", tokenUrl, scope );

            var client = new TokenClient(tokenUrl, Appsettings.SiliconClientId(), Appsettings.SiliconClientSecret());

            var token = client.RequestClientCredentialsAsync(scope).Result;
            if (token.IsError) return "Error Getting a Silicon Token for scope " + scope;
            return token.AccessToken;
        }

    }
}