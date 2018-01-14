using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser.LinesSource
{
    public class LinesSourceMemory : ILinesSource
    {

        /// <summary>
        /// Creates new instance of current class
        /// </summary>
        /// <param name="fileInfo">file with data needs to read</param>
        /// <returns>new instance</returns>
        public static LinesSourceMemory CreateFromFile(FileInfo fileInfo)
        {
            using (StreamReader sr = new StreamReader(fileInfo.OpenRead()))
            {
                var data = sr.ReadToEnd();
                return new LinesSourceMemory(data);
            }
        }

        private int _lastIndex = 0;
        private string _dataSource;
        private Object _syncRoot = new Object(); // object for syncranizations

        public long FilePosition { get => _lastIndex; }
        public long FileTotalBytes { get => _dataSource.Length; }
        public long CurrentBufferPosition { get => _lastIndex; }
        public long BufferSize { get => _dataSource.Length; }

        private LinesSourceMemory(string data)
        {
            _dataSource = data;
        }

        /// <summary>
        /// Thread-safe function. Return next line of log file
        /// </summary>
        /// <param name="line">new line or empty string if end reached</param>
        /// <returns>resturns false if end of file reached and true otherwise</returns>
        public bool GetLine(out string line)
        {
            lock(_syncRoot)
            {
                if(_lastIndex >= _dataSource.Length) // certanly end of data and nothing to return
                {
                    line = string.Empty;
                    return false;
                }
                var newIndex = _dataSource.IndexOf('\n', _lastIndex);
                if (newIndex == -1) // end of data but there is some data left
                {
                    newIndex = _dataSource.Length; // return rest of data
                }
                var resultLength = newIndex - _lastIndex;
                if(_dataSource[newIndex-1] == '\r') // in case new line resprented by two symbols
                {
                    resultLength--;
                }
                line = _dataSource.Substring(_lastIndex, resultLength);
                _lastIndex = newIndex + 1;
                return true;
            }
        }
    }
}
