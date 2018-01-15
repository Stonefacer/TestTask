using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Models;
using DatabaseApi;

namespace WebApi.Controllers
{
    public class LogController : ApiController
    {

        IDatabaseProviderApi _databaseProvider;

        public LogController(IDatabaseProviderApi databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        // api/log/{offset}/{limit}
        public IEnumerable<RequestData> Get(long offset, int limit = 10)
        {
            if(offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if(limit < 1)
            {
                throw new ArgumentOutOfRangeException("limit");
            }
            return _databaseProvider.GetLog(offset, limit);
        }

        // api/log/{offset}/{limit}?start={datetime}
        public IEnumerable<RequestData> Get(long offset, int limit, [FromUri]DateTime start)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException("limit");
            }
            return _databaseProvider.GetLog(offset, limit, start);
        }

        // api/log/{offset}/{limit}?start={datetime}&end={datetime}
        public IEnumerable<RequestData> Get(long offset, int limit, [FromUri]DateTime start, [FromUri]DateTime end)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException("limit");
            }
            return _databaseProvider.GetLog(offset, limit, start, end);
        }

    }
}
