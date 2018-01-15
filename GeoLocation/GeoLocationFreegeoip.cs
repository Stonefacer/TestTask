using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;
using System.Threading;

namespace GeoLocation
{
    public class GeoLocationFreegeoip : IGeoLocationFinder
    {
        /// <summary>
        /// Results will be cached until cache size exceeds that number
        /// </summary>
        private const int MaxCacheSize = 1000000;

        /// <summary>
        /// If server failed to respond we will try again and again untill count of tries exceeds that number
        /// </summary>
        private const int MaxTriesCount = 10;

        /// <summary>
        /// Count of ms need to wait between tries
        /// </summary>
        private const int TryTimeout = 100;

        
        private string _requestUrlTemplate;
        private ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        public GeoLocationFreegeoip(string freegeoipHostname)
        {
            _requestUrlTemplate = string.Format("{0}/json/{{0}}", freegeoipHostname);
        }

        /// <summary>
        /// Gets country name by hostname or ip-address using freegeoip.net api
        /// </summary>
        /// <param name="host">host or ip-address</param>
        /// <param name="country">country name</param>
        /// <returns>true if sucess and false otherwise</returns>
        private bool SendRequest(string host, out string country)
        {
            var httpRequestUrl = string.Format(_requestUrlTemplate, host);
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                string jsonString = String.Empty;
                try
                {
                    jsonString = webClient.DownloadString(httpRequestUrl);
                }
                catch (WebException webException)
                {
                    var response = webException.Response as HttpWebResponse;
                    country = string.Empty;
                    // if server respond 404 not found it means given hostname or ip-address not found in database
                    // so we can determine it as sucess
                    if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return true;
                    }
                    return false;
                }
                var jObject = JObject.Parse(jsonString);
                country = jObject.Value<string>("country_name");
                return true;
            }
        }

        /// <summary>
        /// Tries to find country name in cache and send request to gelocation api if failed
        /// </summary>
        /// <param name="host">host or ip-address</param>   
        /// <returns>country name</returns>
        public string GetCountryName(string host)
        {
            if (_cache.ContainsKey(host))
            {
                return _cache[host];
            }
            string country;
            var result = SendRequest(host, out country);
            for(var tries = 1;tries < MaxTriesCount && !result; tries++)
            {
                Thread.Sleep(TryTimeout);
                result = SendRequest(host, out country);
            }
            if (!result)
            {
                throw new WebException("server is down or limit of requests exceeded");
            }
            if(_cache.Count < MaxCacheSize)
            {
                _cache[host] = country;
            }
            return country;
        }
    }
}
