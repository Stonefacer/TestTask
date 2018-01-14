using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser.LinesSource
{
    public interface ILinesSource
    {
        long CurrentBufferPosition { get; }
        long BufferSize { get; }
        long FilePosition { get; }
        long FileTotalBytes { get; }
        bool GetLine(out string line);
    }
}
