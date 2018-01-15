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
        
        private string _requestUrlTemplate;
        private ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private int _maxTriesCount;
        private int _tryTimeout;
        private int _maxCacheSize;

        public GeoLocationFreegeoip(string freegeoipHostname, int maxTriesCount, int tryTimeout, int maxCacheSize)
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
            for(var tries = 1;tries < _maxTriesCount && !result; tries++)
            {
                Thread.Sleep(_tryTimeout);
                result = SendRequest(host, out country);
            }
            if (!result)
            {
                throw new WebException("server is down or limit of requests exceeded");
            }
            if(_cache.Count < _maxCacheSize)
            {
                _cache[host] = country;
            }
            return country;
        }
    }
}
