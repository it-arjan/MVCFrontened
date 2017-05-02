using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class Utils
    {
        static ILogger _logger = LogManager.CreateLogger(typeof(Utils));
        public static DateTime GetDateTimeClaimFromToken(string jwt, string claim_type)
        {
            var UnixExpUtc = Convert.ToUInt64(GetClaimFromToken(jwt, claim_type));
            var difference = DateTime.Now - DateTime.UtcNow;

            DateTime UnoxStartDateLocalTime = new DateTime(1970, 1, 1).AddHours(difference.TotalHours);
            return UnoxStartDateLocalTime.AddSeconds(UnixExpUtc);
        }

        public static string GetClaimFromToken(string jwt, string claim_type)
        {
            if (string.IsNullOrEmpty(jwt)) return "0";
            if (jwt.Contains("not set (yet)")) return "0";

            //=> Retrieve the 2nd part of the JWT token (this the JWT payload)
            var payloadEncoded = jwt.Split('.')[1];

            //=> Padding the raw payload with "=" chars to reach a length that is multiple of 4
            var mod4 = payloadEncoded.Length % 4;
            if (mod4 > 0) payloadEncoded += new string('=', 4 - mod4);

            //=> Decoding the base64 string
            var payloadBytesDecoded = Convert.FromBase64String(payloadEncoded);

            //=> Retrieve the "exp" property of the payload's JSON
            var payloadStr = Encoding.UTF8.GetString(payloadBytesDecoded, 0, payloadBytesDecoded.Length);
            var payload = JsonConvert.DeserializeAnonymousType(payloadStr, 
                                        new { Exp = string.Empty, Auth_time= string.Empty, Sub=string.Empty, Client_id=string.Empty });
            switch (claim_type. ToLower())
            {
                case "exp": return payload.Exp;
                case "auth_time": return payload.Auth_time;
                case "sub": return payload.Sub;
                case "client_id": return payload.Client_id;
            }
            return payload.Exp;
        }

        public static DateTime ConvertUnixTimestampToCetTime(string UnixTimestamp)
        {
            var val = Convert.ToUInt64(UnixTimestamp.Replace("-", string.Empty));
            var utdDiff= (DateTime.Now- DateTime.UtcNow).TotalHours;
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(val).AddHours(utdDiff);
        }
    }
}