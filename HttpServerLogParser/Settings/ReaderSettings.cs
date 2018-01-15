using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLogParser.Settings
{
    public class FileReaderSettings
    {
        /// <summary>
        /// Maximal count of lines can be stored in memory as buffer
        /// </summary>
        public int MaxBufferSize { get; set; }
    }
}
