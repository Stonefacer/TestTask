using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    //199.72.81.55 - - [01/Jul/1995:00:00:01 -0400] "GET /history/apollo/ HTTP/1.0" 200 6245
    public class RequestData
    {
        /// <summary>
        /// Date and time of current request
        /// </summary>
        public DateTime Datetime { get; set; }

        /// <summary>
        /// Client hostname or an ip-address
        /// </summary>
        public string ClientHostname { get; set; }

        /// <summary>
        /// Target route
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// GET parameters
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// HTTP response code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Data size in bytes
        /// </summary>
        public long DataSize { get; set; }

        /// <summary>
        /// Depends on ClientHost
        /// </summary>
        public string ClientLocation { get; set; }
    }
}
