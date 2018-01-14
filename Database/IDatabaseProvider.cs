using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

namespace Database
{
    public interface IDatabaseProvider: IDisposable
    {
        bool AddRequest(RequestData requestData);
    }
}
