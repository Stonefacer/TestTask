using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Text;

using Newtonsoft.Json;

using Models;
using DatabaseApi;

namespace WebApi.Controllers
{
    public class RoutesController : ApiController
    {

        private IDatabaseProviderApi _databaseProvider;

        public RoutesController(IDatabaseProviderApi databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        // GET api/routes/{count}
        public IEnumerable<RouteRating> Get(int count = 10)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopRoutes(count);
        }

        // GET api/routes/{count}?start={datetime}
        public IEnumerable<RouteRating> Get(int count, [FromUri]DateTime start)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopRoutes(count, start);
        }

        // GET api/routes/{count}?start={datetime}&end={datetime}
        public IEnumerable<RouteRating> Get(int count, [FromUri]DateTime start, [FromUri]DateTime end)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopRoutes(count, start, end);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _databaseProvider?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
