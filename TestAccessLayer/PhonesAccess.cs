using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.SqlServer;
using System.Text;
using System.Threading.Tasks;
using TestAccessLayer.Utils;
using System.Data;

namespace TwinkiesStoreDataAccessLayer
{
    public static class PhonesAccess
    {
        /// TODO
        /// SP_AddPhone
        /// SP_GetPhonesOf
        /// <summary>
        /// Adds a new phone number for a customer by calling the stored procedure [dbo].[SP_AddPhone].
        /// </summary>
        /// <param name="phoneNumber">The phone number to be added.</param>
        /// <param name="customerId">The ID of the customer associated with the phone number.</param>
        /// <returns>The primary key (Phone ID) of the newly added phone number, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref = "Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddPhone(string phoneNumber, int customerId)
        {
            string query = "EXEC [dbo].[SP_AddPhone] @PhoneNumber, @Customer_ID";

            try
            {
                // Add the phone number and return the new Phone ID
                return ConnectionUtils.AddRowToTable(query, phoneNumber, customerId);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Retrieves all phone numbers associated with a specific customer from the Phones table.
        /// </summary>
        /// <param name="customerId">The ID of the customer whose phone numbers are to be retrieved.</param>
        /// <returns>A DataTable containing the phone numbers for the specified customer.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetPhonesOf(int customerId)
        {
            string query = "EXEC SP_GetPhonesOf @Customer_ID";
            try
            {
                return ConnectionUtils.GetTable(query, customerId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }
    }
}
