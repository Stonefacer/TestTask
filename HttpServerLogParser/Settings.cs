using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLogParser
{
    public class Settings
    {
        public int ThreadsCount { get; set; }
        public string Filename { get; set; }
        public string ConnectionString { get; set; }
        public string GeolocationServer { get; set; }
    }
}
