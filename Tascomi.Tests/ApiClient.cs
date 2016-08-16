using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Tascomi.Tests
{
    internal sealed class APIClient
    {
        private static APIClient _Instance;

        public string PublicKey = "";
        private string PrivateKey = "";

        public DateTime Generated;

        public static APIClient Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new APIClient();
                }
                return _Instance;
            }
        }

        private APIClient()
        {
            PublicKey = Properties.Settings.Default.ApiPublicKey;
            PrivateKey = Properties.Settings.Default.ApiPrivateKey;
        }

        public string GetToken()
        {
            string PublicKeyFormatted = PublicKey + GetDateDmy();

            byte[] bytes = Encoding.UTF8.GetBytes(PublicKeyFormatted);
            SHA256Managed hashString = new SHA256Managed();
            byte[] hash = hashString.ComputeHash(bytes);
            string salt = hash.Aggregate(string.Empty, (current, x) => current + String.Format("{0:x2}", x));

            string tokenString;
            using (var hMAC = new HMACSHA256(Encoding.UTF8.GetBytes(PrivateKey)))
            {
                hMAC.ComputeHash(Encoding.UTF8.GetBytes(salt));
                tokenString = ByteToString(hMAC.Hash).ToLower();
            }

            return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenString));
        }

        public HttpClient GetHttpClient()
        {
            var hc = new HttpClient();
            hc.BaseAddress = Properties.Settings.Default.ApiUri;
            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            hc.DefaultRequestHeaders.Add("X-Public", PublicKey);
            hc.DefaultRequestHeaders.Add("X-Hash", GetToken());

            return hc;
        }

        private string ByteToString(byte[] buff)
        {
            return buff.Aggregate("", (current, t) => current + t.ToString("X2"));
        }

        private string GetDateDmy()
        {
            Generated = DateTime.Now;

            return Generated.Year + Generated.Month.ToString().PadLeft(2, '0') + Generated.Day.ToString().PadLeft(2, '0') + Generated.Hour.ToString().PadLeft(2, '0') + Generated.Minute.ToString().PadLeft(2, '0');
        }
    }
}
