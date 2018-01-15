using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser.LinesSource
{
    public class LinesSourceFile : ILinesSource
    {

        private long _filePosition;
        private long _fileLength;
        private FileInfo _fileInfo;
        private Queue<string> _linesQueue;
        private Object _syncRoot = new Object();
        private int _maxBufferSize;

        public long FilePosition { get => _filePosition; }

        public long FileTotalBytes { get => _fileLength; }

        public long CurrentBufferPosition { get => _maxBufferSize - _linesQueue.Count; }

        public long BufferSize { get => _maxBufferSize; }

        public LinesSourceFile(FileInfo fileInfo, int maxBufferSize)
        {
            _fileInfo = fileInfo;
            _filePosition = 0;
            _fileLength = fileInfo.Length;
            _maxBufferSize = maxBufferSize;
            _linesQueue = new Queue<string>(_maxBufferSize);
        }

        /// <summary>
        /// Reads line from stream but don't read symbols any further
        /// </summary>
        /// <returns>readed line or empty string</returns>
        private string ReadLine(Stream stream)
        {
            var result = new List<char>();
            // save code of new line symbol '\n' for further search
            var newLineSymbolCode = (int)'\n';
            while (true)
            {
                var currentSymbol = stream.ReadByte();
                if(currentSymbol == newLineSymbolCode || currentSymbol == -1) // end of stream reached or new line symbol found
                {
                    break;
                }
                result.Add((char)currentSymbol);
            } 
            if(result[result.Count - 1] == '\r')
            {
                result.RemoveAt(result.Count - 1);
            }
            return new string(result.ToArray());
        }

        /// <summary>
        /// Fills queue with data. Very slow operation
        /// </summary>
        private void FillBuffer()
        {
            using (var fileStream = _fileInfo.OpenRead())
            {
                fileStream.Seek(_filePosition, SeekOrigin.Begin); // seek to the lastly readed position
                while (_linesQueue.Count < _maxBufferSize && fileStream.Position != fileStream.Length )
                {
                    var newLine = ReadLine(fileStream);
                    _linesQueue.Enqueue(newLine);
                }
                _filePosition = fileStream.Position; // save position for further reads
                _fileLength = fileStream.Length;
            }
        }

        /// <summary>
        /// Gets new line from buffer or file
        /// </summary>
        /// <param name="line">result</param>
        /// <returns>false if nothing is left to read and true otherwise</returns>
        public bool GetLine(out string line)
        {
            lock (_syncRoot)
            {
                if (_linesQueue.Count == 0) // buffer is empty
                {
                    if (_filePosition == _fileLength) // nothing is left to read
                    {
                        line = string.Empty;
                        return false;
                    }
                    else // some data can be readed from file
                    {
                        FillBuffer();
                    }
                }
                line = _linesQueue.Dequeue();
                return true;
            }
        }
    }
}
