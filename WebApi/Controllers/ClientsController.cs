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
    public class ClientsController : ApiController
    {

        private IDatabaseProviderApi _databaseProvider;

        public ClientsController(IDatabaseProviderApi databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        // GET api/clients/{count}
        [OutputCache(Duration = 60, VaryByParam = "count")]
        public IEnumerable<ClientRating> Get(int count = 10)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopClients(count);
        }

        // GET api/clients/{count}?start={datetime}
        public IEnumerable<ClientRating> Get(int count, [FromUri]DateTime start)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopClients(count, start);
        }

        // GET api/clients/{count}?start={datetime}&end={datetime}
        public IEnumerable<ClientRating> Get(int count, [FromUri]DateTime start, [FromUri]DateTime end)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return _databaseProvider.GetTopClients(count, start, end);
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
