using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

namespace LogParser
{
    public interface IParser
    {
        string CurrentLine { get; }
        bool ProcessOneLine(out RequestData requestData, out bool keepAlive);
    }
}
