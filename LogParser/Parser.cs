using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Models;
using LogParser.LinesSource;
using GeoLocation;

namespace LogParser
{
    public class Parser : IParser
    {

        private ILinesSource _linesSource;
        private string _currentLine;
        private int _currentIndex; // number of symbols to skip
        private HashSet<string> _skippibleExtensions;
        private int _extensionMaximalLength;

        public string CurrentLine { get => _currentLine; }

        public Parser(ILinesSource linesSource, string[] skippibleExtensions)
        {
            _linesSource = linesSource;
            _skippibleExtensions = new HashSet<string>(skippibleExtensions.Distinct());
            _extensionMaximalLength = _skippibleExtensions.Max(x=>x.Length);
        }

        // position of _currentIndex can be any
        private string ParseHostName()
        {
            var startIndex = 0;
            var endIndex = _currentLine.IndexOf(' ');
            _currentIndex = endIndex + 1;
            return _currentLine.Substring(startIndex, endIndex - startIndex);
        }

        // position of _currentIndex can be any
        private DateTime ParseDateTime()
        {
            var startIndex = _currentLine.IndexOf('[') + 1;
            var endIndex = _currentLine.IndexOf(']', startIndex);
            var dateTimeString = _currentLine.Substring(startIndex, endIndex - startIndex);
            _currentIndex = endIndex + 2; // one for symbol ']' and one for space
            var result = DateTime.ParseExact(dateTimeString, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            return result.ToUniversalTime();
        }

        // position of _currentIndex can be any
        /// <summary>
        /// Parse data in quotes
        /// </summary>
        /// <param name="queryString">string with query parameters</param>
        /// <returns>local path (route)</returns>
        private string ParseUri(out string queryString)
        {
            var startIndex = _currentLine.IndexOf('"') + 1;
            var endIndex = _currentLine.LastIndexOf('"');
            _currentIndex = endIndex + 2; // skip symbol '"' at end and space after it

            // substring will return:
            // GET /twiki/bin/view/Main/WebHome?skin=print&rev=1.25 HTTP/1.0
            var requestInfo = _currentLine.Substring(startIndex, endIndex - startIndex);
            startIndex = requestInfo.IndexOf(' ') + 1;
            endIndex = requestInfo.Length;

            // get rid of protocol version
            if (requestInfo.EndsWith("HTTP/1.0") || requestInfo.EndsWith("HTTP/1.1"))
            {
                endIndex -= 8 + 1; // minus length of string HTTP/1.0 and space before
            }

            // tring to find end of route and start of query string
            var separator = requestInfo.IndexOf('?', startIndex);
            if (separator == -1) // there is no query string
            {
                queryString = String.Empty;
                return requestInfo.Substring(startIndex, endIndex - startIndex);
            }
            queryString = requestInfo.Substring(separator + 1, endIndex - separator - 1);
            return requestInfo.Substring(startIndex, separator - startIndex);
        }

        // position of _currentIndex can be any
        private int ParseResponseCode()
        {
            var startIndex = _currentLine.LastIndexOf('"') + 2;
            var endIndex = _currentLine.IndexOf(' ', startIndex);
            _currentIndex = endIndex + 1;
            var resultString = _currentLine.Substring(startIndex, endIndex - startIndex);
            return int.Parse(resultString);
        }

        // position of _currentIndex must be
        // ----------------------------------------------------------------------------------------------------------------------|
        // example-host.com - - [11/Mar/2004:13:07:02 -0800] "GET /twiki/bin/view/Main/WebHome?skin=print&rev=1.25 HTTP/1.0" 200 7762
        private long ParseDataSize()
        {
            var startIndex = _currentIndex;
            _currentIndex = _currentLine.Length;
            var resultString = _currentLine.Substring(startIndex);
            if (resultString == "-")
            {
                return -1;
            }
            return long.Parse(resultString);
        }

        private bool IsSkippible(string route)
        {
            if(route.Length == 0)
            {
                return false;
            }
            if (route[route.Length - 1] == '/')
            {
                return false;
            }
            var maxExtensionLength = _extensionMaximalLength;
            if (maxExtensionLength > route.Length)
            {
                maxExtensionLength = route.Length;
            }
            var extensionIndex = route.LastIndexOf('.', route.Length - 1, maxExtensionLength);
            if(extensionIndex == -1)
            {
                return false;
            }
            var extension = route.Substring(extensionIndex + 1).TrimEnd(' ').ToLowerInvariant();
            if (_skippibleExtensions.Contains(extension))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Process one line and returns requestData as result
        /// </summary>
        /// <returns>true if data must be saved and false otherwise</returns>
        public bool ProcessOneLine(out RequestData requestData, out bool keepAlive)
        {
            keepAlive = _linesSource.GetLine(out _currentLine);

            // skip empty lines
            while (keepAlive && _currentLine == String.Empty)
            {
                keepAlive = _linesSource.GetLine(out _currentLine);
            }

            // check end of data
            if (!keepAlive) 
            {
                requestData = null;
                return false;
            }

            // parse data in quotes first to check is it skippable line or not
            string query;
            var route = ParseUri(out query);
            if (IsSkippible(route))
            {
                requestData = null;
                return false;
            }
            requestData = new RequestData();
            requestData.Route = route;
            requestData.QueryString = query;

            // hostname
            requestData.ClientHostname = ParseHostName();

            // datetime
            requestData.Datetime = ParseDateTime();

            // response code
            requestData.StatusCode = ParseResponseCode();

            // data size
            requestData.DataSize = ParseDataSize();

            return true;
        }
    }
}
