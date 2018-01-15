using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLogParser.Settings
{
    [Serializable]
    public class ParserSettings
    {
        /// <summary>
        /// Array of extensions must be skipped
        /// </summary>
        public string[] SkippibaleExtensions { get; set; }
    }
}
