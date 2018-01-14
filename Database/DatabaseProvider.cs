using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Dapper;

using Models;

namespace Database
{
    public class DatabaseProvider : IDatabaseProvider
    {

        private SqlConnection _sqlConnection;
        private string _connectionString;
        private bool _silent;

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <param name="silent">if true any methods won't throw any exceptions</param>
        public DatabaseProvider(string connectionString, bool silent)
        {
            _connectionString = connectionString;
            _sqlConnection = new SqlConnection(connectionString);
            _silent = silent;
        }

        private bool ExecuteNonQuery(Action<SqlConnection> query)
        {
            try
            {
                if (_sqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    _sqlConnection.Open();
                }
                query(_sqlConnection);
                return true;
            }
            catch (Exception)
            {
                if (!_silent)
                {
                    throw;
                }
                return false;
            }
        }

        /// <summary>
        /// Add new row into request table
        /// </summary>
        /// <param name="requestData">data to add</param>
        /// <returns>return true if sucess and false otherwise</returns>
        public bool AddRequest(RequestData requestData)
        {
            return ExecuteNonQuery((connection) =>
            {
                using (var command = connection.CreateCommand())
                {
                    // Set parameters
                    if (string.IsNullOrEmpty(requestData.QueryString))
                    {
                        command.Parameters.Add(new SqlParameter("@QueryString", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@QueryString", requestData.QueryString));
                    }
                    if(requestData.DataSize < 0)
                    {
                        command.Parameters.Add(new SqlParameter("@DataSize", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@DataSize", requestData.DataSize));
                    }
                    command.Parameters.Add(new SqlParameter("@Route", requestData.Route));
                    command.Parameters.Add(new SqlParameter("@ClientHostname", requestData.ClientHostname));
                    command.Parameters.Add(new SqlParameter("@ClientLocation", requestData.ClientLocation));
                    command.Parameters.Add(new SqlParameter("@StatusCode", requestData.StatusCode));
                    command.Parameters.Add(new SqlParameter("@Datetime", requestData.Datetime));
                    
                    command.CommandText = @"
                        BEGIN TRANSACTION;
	                    INSERT INTO Requests(Route, QueryString, ClientHostname, ClientLocation, StatusCode, DataSize, Datetime)
		                    VALUES(@Route, @QueryString, @ClientHostname, @ClientLocation, @StatusCode, @DataSize, @Datetime);
	                    COMMIT TRANSACTION;
                    ";

                    command.ExecuteNonQuery();
                }
            });
        }

        public void Dispose()
        {
            _sqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
