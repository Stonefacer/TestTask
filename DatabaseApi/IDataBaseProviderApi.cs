using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

namespace DatabaseApi
{
    public interface IDatabaseProviderApi: IDisposable
    {
        /// <summary>
        /// Get top clients of all time
        /// </summary>
        /// <param name="maxCount">count of clients to return</param>
        IEnumerable<ClientRating> GetTopClients(int maxCount);

        /// <summary>
        /// Get top clients after given datetime
        /// </summary>
        /// <param name="maxCount">count of clients to return</param>
        IEnumerable<ClientRating> GetTopClients(int maxCount, DateTime after);

        /// <summary>
        /// Get top clients in given interval
        /// </summary>
        /// <param name="maxCount">count of clients to return</param>
        IEnumerable<ClientRating> GetTopClients(int maxCount, DateTime after, DateTime before);

        /// <summary>
        /// Get top routes of all time
        /// </summary>
        /// <param name="maxCount">count of routes to return</param>
        IEnumerable<RouteRating> GetTopRoutes(int maxCount);

        /// <summary>
        /// Get top routes after given datetime
        /// </summary>
        /// <param name="maxCount">count of routes to return</param>
        IEnumerable<RouteRating> GetTopRoutes(int maxCount, DateTime after);

        /// <summary>
        /// Get top routes in given interval
        /// </summary>
        /// <param name="maxCount">count of routes to return</param>
        IEnumerable<RouteRating> GetTopRoutes(int maxCount, DateTime after, DateTime before);

        /// <summary>
        /// Get log of routes. Sorted by datetime ascending
        /// </summary>
        /// <param name="offset">count of requests to skip</param>
        /// <param name="limit">count of requests to return</param>
        IEnumerable<RequestData> GetLog(long offset, int limit);

        /// <summary>
        /// Get log of routes after given datetime. Sorted by datetime ascending
        /// </summary>
        /// <param name="offset">count of requests to skip</param>
        /// <param name="limit">count of requests to return</param>
        /// <param name="start">start of interval</param>
        IEnumerable<RequestData> GetLog(long offset, int limit, DateTime start);

        /// <summary>
        /// Get log of routes in given interval. Sorted by datetime ascending
        /// </summary>
        /// <param name="offset">count of requests to skip</param>
        /// <param name="limit">count of requests to return</param>
        /// <param name="start">start of interval</param>
        /// <param name="end">end of interval</param>
        IEnumerable<RequestData> GetLog(long offset, int limit, DateTime start, DateTime end);
    }
}
