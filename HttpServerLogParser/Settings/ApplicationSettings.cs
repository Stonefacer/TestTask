using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerLogParser.Settings
{
    class ApplicationSettings
    {
        public int ThreadsCount { get; set; }
        public string Filename { get; set; }
        public CancellationTokenSource Cancelation { get; set; }
    }
}
