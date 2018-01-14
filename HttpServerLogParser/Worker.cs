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
    public class Worker: IDisposable
    {

        private IParser _parser;
        private IGeoLocationFinder _geoLocation;
        private IDatabaseProvider _database;

        public Worker(IParser parser, IGeoLocationFinder geoLocation, IDatabaseProvider database)
        {
            _parser = parser;
            _geoLocation = geoLocation;
            _database = database;
        }

        private void FailedMessage(string lineWithError, Exception exception)
        {
            Console.WriteLine("Failed to parse line: \"{0}\"", lineWithError);
            Console.WriteLine("-------------Exceptions------------");
            while (exception != null)
            {
                Console.WriteLine("Type: {0}", exception.GetType().FullName);
                Console.WriteLine("Message: {0}", exception.Message);
                Console.WriteLine("Stack trace: {0}", exception.StackTrace);
                Console.WriteLine();
                exception = exception.InnerException;
            }
        }

        public void ProcessFile()
        {
            var keepAlive = true;
            while (keepAlive)
            {
                RequestData requestData;
                try
                {
                    var saveData = _parser.ProcessOneLine(out requestData, out keepAlive);
                    if (saveData)
                    {
                        requestData.ClientLocation = _geoLocation.GetCountryName(requestData.ClientHostname);
                        _database.AddRequest(requestData);
                    }
                }
                catch (Exception ex)
                {
                    FailedMessage(_parser.CurrentLine, ex);
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
