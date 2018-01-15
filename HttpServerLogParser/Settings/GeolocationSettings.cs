using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLogParser.Settings
{
    [Serializable]
    public class GeolocationSettings
    {
        /// <summary>
        /// Geolocation server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Results will be cached until cache size exceeds that number
        /// </summary>
        public int MaxCacheSize { get; set; }

        /// <summary>
        /// If server failed to respond we will try again and again untill count of tries exceeds that number
        /// </summary>
        public int MaxTriesCount { get; set; }

        /// <summary>
        /// Count of ms need to wait between tries
        /// </summary>
        public int TryTimeout { get; set; }
    }
}
