using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using LogParser;
using GeoLocation;
using Database;

namespace HttpServerLogParser
{
    public class Worker : IDisposable
    {

        private IParser _parser;
        private IGeoLocationFinder _geoLocation;
        private IDatabaseProvider _database;

        public string CurrentLine { get => _parser.CurrentLine; }

        public Worker(IParser parser, IGeoLocationFinder geoLocation, IDatabaseProvider database)
        {
            _parser = parser;
            _geoLocation = geoLocation;
            _database = database;
        }

        public void ProcessFile()
        {
            var keepAlive = true;
            while (keepAlive)
            {
                RequestData requestData;
                var saveData = _parser.ProcessOneLine(out requestData, out keepAlive);
                if (saveData)
                {
                    requestData.ClientLocation = _geoLocation.GetCountryName(requestData.ClientHostname);
                    _database.AddRequest(requestData);
                }

            }
        }

        public void Dispose()
        {
            _database.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
