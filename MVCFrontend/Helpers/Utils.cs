using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MVCFrontend.Helpers
{
    public static class Utils
    {
        public static int GetExpFromToken(string jwt)
        {
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
            return (int)payload.Exp;
        }

        public static DateTime ConvertUnixTimestampToCetTime(string UnixTimestamp)
        {
            var val = Convert.ToUInt64(UnixTimestamp);
            var utdDiff= (DateTime.Now- DateTime.UtcNow).TotalHours;
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(val).AddHours(utdDiff);
        }
    }
}