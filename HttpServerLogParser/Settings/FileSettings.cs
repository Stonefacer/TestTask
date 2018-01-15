using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLogParser.Settings
{
    [Serializable]
    public class FileSettings
    {
        /// <summary>
        /// Database connection string
        /// </summary>
        public string ConnectionString { get; set; }

        public GeolocationSettings Geolocation { get; set; }

        public ParserSettings Parser { get; set; }

        public FileReaderSettings FileReader { get; set; }
    }
}
