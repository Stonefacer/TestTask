using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

using Models;

namespace DatabaseApi
{
    public class DatabaseProviderApi: IDatabaseProviderApi
    {
        private SqlConnection _sqlConnection;
        private string _connectionString;
        private bool _silent;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <param name="silent">if true any methods won't throw any exceptions</param>
        public DatabaseProviderApi(string connectionString, bool silent)
        {
            _connectionString = connectionString;
            _sqlConnection = new SqlConnection(connectionString);
            _silent = silent;
        }

        private T ExecuteQuery<T>(Func<SqlConnection, T> query)
        {
            try
            {
                if (_sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    _sqlConnection.Open();
                }
                return query(_sqlConnection);
            }
            catch (Exception)
            {
                if (!_silent)
                {
                    throw;
                }
                return default(T);
            }
        }

        /////////////
        // Clients //
        /////////////

        public IEnumerable<ClientRating> GetTopClients(int maxCount)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.ClientHostname as Hostname, COUNT(rl.RequestId) as Rating
                    FROM Requests as rl
                    GROUP BY rl.ClientHostname
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                return connection.QueryMultiple(command).Read<ClientRating>();
            });
        }

        public IEnumerable<ClientRating> GetTopClients(int maxCount, DateTime after)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.ClientHostname as Hostname, COUNT(RequestId) as Rating
                    FROM Requests as rl
                    WHERE rl.Datetime >= @StartDatetime
                    GROUP BY rl.ClientHostname
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                var commandParams = new
                {
                    StartDatetime = after
                };
                return connection.QueryMultiple(command, commandParams).Read<ClientRating>();
            });
        }

        public IEnumerable<ClientRating> GetTopClients(int maxCount, DateTime after, DateTime before)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.ClientHostname as Hostname, COUNT(RequestId) as Rating
                    FROM Requests as rl
                    WHERE rl.Datetime >= @StartDatetime AND rl.Datetime < @EndDatetime
                    GROUP BY rl.ClientHostname
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                var commandParams = new
                {
                    StartDatetime = after,
                    EndDatetime = before
                };
                return connection.QueryMultiple(command, commandParams).Read<ClientRating>();
            });
        }
        
        ////////////
        // ROUTES //
        ////////////

        public IEnumerable<RouteRating> GetTopRoutes(int maxCount)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.Route as Route, COUNT(RequestId) as Rating
                    FROM Requests as rl
                    GROUP BY rl.Route
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                return connection.QueryMultiple(command).Read<RouteRating>();
            });
        }

        public IEnumerable<RouteRating> GetTopRoutes(int maxCount, DateTime after)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.Route as Route, COUNT(RequestId) as Rating
                    FROM Requests as rl
                    WHERE rl.Datetime >= @StartDatetime
                    GROUP BY rl.Route
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                var commandParams = new
                {
                    StartDatetime = after
                };
                return connection.QueryMultiple(command, commandParams).Read<RouteRating>();
            });
        }

        public IEnumerable<RouteRating> GetTopRoutes(int maxCount, DateTime after, DateTime before)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT DISTINCT TOP {0} rl.Route as Route, COUNT(RequestId) as Rating
                    FROM Requests as rl
                    WHERE rl.Datetime >= @StartDatetime AND Datetime < @EndDatetime
                    GROUP BY rl.Route
                    Order BY Rating DESC
                ";
                command = string.Format(command, maxCount);
                var commandParams = new
                {
                    StartDatetime = after,
                    EndDatetime = before
                };
                return connection.QueryMultiple(command, commandParams).Read<RouteRating>();
            });
        }

        //////////////
        //// LOGS ////
        //////////////

        public IEnumerable<RequestData> GetLog(long offset, int limit)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT Route, QueryString, ClientHostname, ClientLocation, StatusCode, DataSize, Datetime
                    FROM Requests
                    ORDER BY Datetime ASC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY
                ";
                var commandParams = new
                {
                    Offset = offset,
                    Count = limit
                };
                return connection.QueryMultiple(command, commandParams).Read<RequestData>();
            });
        }

        public IEnumerable<RequestData> GetLog(long offset, int limit, DateTime start)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT Route, QueryString, ClientHostname, ClientLocation, StatusCode, DataSize, Datetime
                    FROM Requests
                    WHERE Datetime >= @StartDatetime
                    ORDER BY Datetime ASC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY
                ";
                var commandParams = new
                {
                    Offset = offset,
                    Count = limit,
                    StartDatetime = start
                };
                return connection.QueryMultiple(command, commandParams).Read<RequestData>();
            });
        }

        public IEnumerable<RequestData> GetLog(long offset, int limit, DateTime start, DateTime end)
        {
            return ExecuteQuery((connection) =>
            {
                var command = @"
                    SELECT Route, QueryString, ClientHostname, ClientLocation, StatusCode, DataSize, Datetime
                    FROM Requests
                    WHERE Datetime >= @StartDatetime AND Datetime < @EndDatetime
                    ORDER BY Datetime ASC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Count ROWS ONLY
                ";
                var commandParams = new
                {
                    Offset = offset,
                    Count = limit,
                    StartDatetime = start,
                    EndDatetime = end
                };
                return connection.QueryMultiple(command, commandParams).Read<RequestData>();
            });
        }

        public void Dispose()
        {
            _sqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
