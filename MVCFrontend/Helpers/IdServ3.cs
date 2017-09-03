using IdentityModel.Client;
using MVCFrontend.Extentions;
using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class IdSrv3
    {
        public const string ScopeMcvFrontEndHuman = "mvc-frontend-human";

        public const string ScopeMvcFrontEnd = "mvc-frontend-silicon";
        public const string ScopeEntryQueueApi = "entry-queue-api";
        public const string ScopeNancyApi = "nancy-api";
        public const string ScopeFrontendDataApi = "frontend-data-api";
        public const string ScopeServiceStackApi = "servicestack-api";
        public const string ScopeWcfService = "wcf-service";
        public const string ScopeMsWebApi = "ms-webapi2";

        public const string ClaimQmFeedId = "socket_qm_feed";
        public const string ClaimNotificationFeedId = "socket_notification_feed";
        public const string ClaimApiFeedId = "socket_api_feed";
        public const string ClaimScoketAccess = "socket_access_token";

        public const string ClaimPostbackCompleted = "postback_completed_sign";

        public const string ClaimCorsToken = "oath_ajax_cors";
        public const string ClaimApiToken = "oath_data_api";

        public const string ClaimCookieExp= "auth_cookie_exp";

        public static string UniqueClaimOfAntiForgeryToken = "given_name";

        public static int SessionRefreshTimeoutSecs = 3600;

        private static ILogger _logger = LogManager.CreateLogger(typeof(IdSrv3));

        private static Dictionary<string, string> claimTypeScopeMap = new Dictionary<string, string>
        {
            { IdSrv3.ClaimCorsToken, IdSrv3.ScopeEntryQueueApi },
            { IdSrv3.ClaimApiToken, IdSrv3.ScopeFrontendDataApi}
        };

        public static string NewSiliconClientToken(string scope)
        {
            var tokenUrl = string.Format("{0}connect/token", Configsettings.AuthUrl());
            _logger.Debug("Getting a new Client Token for scope {1} at {0} ", tokenUrl, scope );

            var client = new TokenClient(tokenUrl, Configsettings.SiliconClientId(), Configsettings.SiliconClientSecret());

            var token = client.RequestClientCredentialsAsync(scope).Result;
            if (token.IsError) return "Error Getting a Silicon Token for scope " + scope;
            return token.AccessToken;
        }

        public static bool EnsureTokenClaimIsValid(string claimType)
        {
            if (TokenAlmostExpired(ClaimsPrincipal.Current.GetClaimValue(claimType), claimTypeScopeMap[claimType]))
            {
                ClaimsPrincipal.Current.AddUpdateClaim(claimType, NewSiliconClientToken(claimTypeScopeMap[claimType]));
                return true;
            }
            return true;
        }

        private static bool TokenAlmostExpired(string jwt, string scope)
        {
            if (jwt.Contains("not set")) return true;
            _logger.Trace("Checking expiration of token({1}) {0}", jwt, scope);
            // #PastedCode
            //
            //=> Retrieve the 2nd part of the JWT token (this the JWT payload)
            var payloadBytes = jwt.Split('.')[1];

            //=> Padding the raw payload with "=" chars to reach a length that is multiple of 4
            var mod4 = payloadBytes.Length % 4;
            if (mod4 > 0) payloadBytes += new string('=', 4 - mod4);

            //=> Decoding the base64 string
            var payloadBytesDecoded = Convert.FromBase64String(payloadBytes);

            //=> Retrieve the "exp" property of the payload's JSON
            var payloadStr = Encoding.UTF8.GetString(payloadBytesDecoded, 0, payloadBytesDecoded.Length);
            var payload = JsonConvert.DeserializeAnonymousType(payloadStr, new { Exp = 0UL });


            var date1970CET = new DateTime(1970, 1, 1, 0, 0, 0).AddHours(1);
            //_logger.Debug("Expired Check: the token({1}) is valid until {0}.", date1970CET.AddSeconds(payload.Exp), scope);

            //=> Get the current timestamp
            var currentTimestamp = (ulong)(DateTime.UtcNow.AddHours(1) - date1970CET).TotalSeconds;
            // Compare
            var isExpired = currentTimestamp + 10 > payload.Exp; // 10 sec = margin
            var logMsg = isExpired ? string.Format("Expired Check: token({0}) is expired.", scope)
                                    : string.Format("Expired Check: token({0}) still valid.", scope);
            _logger.Info(logMsg);

            return isExpired;
        }
    }
}