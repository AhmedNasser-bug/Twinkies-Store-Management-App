

using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace TestAccessLayer.Utils
{
    internal static class ConnectionUtils 
    {
        public static string ConnectionString = "Data Source=.;Initial Catalog=Twinkies Store;Persist Security Info=True;User ID=sa;Password=123456;Encrypt=False;TrustServerCertificate=True";
        public static string[] NullValues = { "-1", "", DateTime.MinValue.ToString() };

        /// <summary>
        /// Checks if the given value is identified as null from the globlal static string[] NullValues.
        /// </summary>
        /// <returns>True if the given value is null, Fasle otherwise.</returns>
        private static bool IsNull(string value)
        {
            foreach (string Null in NullValues)
            {
                if (Null == value) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the names of the parameters in the given query.
        /// </summary>
        /// <returns></returns>
        private static List<string> _GetParamNames(string query)
        {
            List<string> paramNames = new List<string>();

            StringBuilder Current = new StringBuilder();

            for (int i = 0; i < query.Length; i++)
            {
                if (query[i] == '@')
                {
                    while (i < query.Length && query[i] != ' ' && query[i] != ',' && query[i] != ';' && query[i] != ')')
                    {
                        Current.Append(query[i]);
                        i++;
                    }

                    paramNames.Add(Current.ToString());
                    Current = new StringBuilder();
                }

            }
            return paramNames;

        }

        /// <summary>
        /// Initiates a connection with the database using the ConnectionString.
        /// </summary>
        /// <returns>The SqlConnection Initialized by the method.</returns>
        public static SqlConnection InitiateConnection()
        {
            try
            {
                // Connect with credintials specified by the connection string
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
            }
            return null;
        }

        /// <summary>
        /// Ensures the success of a non query command.
        /// </summary>
        /// <returns>The amount of rows affected of the given non-query command.</returns>
        public static int EnsureNonQuerySuccess(ref SqlCommand Command, ref SqlConnection connection)
        {
            int rowsAffected = 0;
            try
            {
                rowsAffected = Command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }
            finally
            {
                connection.Close();
                Command.Dispose();
            }
            return rowsAffected;
        }

        /// <summary>
        /// Executes a scalar command and returns the result as a string.
        /// </summary>
        public static string ExecuteScalar(ref SqlCommand Command, ref SqlConnection connection)
        {
            string result = null;
            try
            {
                result = Convert.ToString(Command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }
            finally
            {
                connection.Close();
                Command.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Adds the given parameters to the given command as a query parameters.
        /// </summary>
        public static void AddArgsToCommand(ref SqlCommand command, string query, params object[] paramaters)
        {
            List<string> queryParameters = _GetParamNames(query);
            for (int idx = 0; idx < paramaters.Length; idx++)
            {
                object param = paramaters[idx];
                if (!IsNull(param.ToString())) command.Parameters.AddWithValue(queryParameters[idx], param);
                else command.Parameters.AddWithValue(queryParameters[idx], DBNull.Value);
            }
        }

        public static SqlDataReader? GetRow<T>(string query, T PrimaryKey)
        {
            SqlConnection connection = InitiateConnection();

            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, PrimaryKey);

            try
            {
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                connection.Close();
                command.Dispose();
                return reader;


            }
            catch (Exception ex)
            {
                //  Logger.Error(ex);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        public static int AddRowToTable(string query, params object[] parameters)
        {
            SqlConnection connection = InitiateConnection();

            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, parameters);

            string result = ExecuteScalar(ref command, ref connection);

            if (result is null)
            {
                return -1;
            }
            return Convert.ToInt32(result);

        }

        public static bool UpdateTableRow(string query, params object[] parameters)
        {
            SqlConnection connection = InitiateConnection();

            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, parameters);

            int RowsAffected = EnsureNonQuerySuccess(ref command, ref connection);

            return RowsAffected > 0;

        }

        public static bool DeleteTableRow(string query, params object[] parameters)
        {
            SqlConnection connection = InitiateConnection();

            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, parameters);

            int RowsAffected = EnsureNonQuerySuccess(ref command, ref connection);

            return RowsAffected > 0;

        }

        public static bool IsRowExist(string query, params object[] parameters)
        {
            SqlConnection connection = InitiateConnection();

            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, parameters);
            bool isFound = ExecuteScalar(ref command, ref connection) != "";

            return isFound;

        }

        public static DataTable GetTable(string query, params object[] parameters)
        {
            DataTable dt = new DataTable();

            SqlConnection connection = InitiateConnection();


            SqlCommand command = new SqlCommand(query, connection);

            AddArgsToCommand(ref command, query, parameters);

            try
            {
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    dt.Load(reader);
                }

                reader.Close();

            }
            catch (Exception ex)
            {
                //  Logger.Error(ex);
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }

            return dt;
        }
    }
}
