using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAccessLayer.Utils;
using Microsoft.Data.SqlClient;

namespace TestAccessLayer
{
    public static class ShippingAccess
    {
        /// <summary>
        /// Adds a new shipping record to the database by calling the stored procedure [dbo].[sp_AddShipping].
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier.</param>
        /// <param name="startDate">The start date of the shipping.</param>
        /// <param name="status">The status of the shipping.</param>
        /// <param name="shippingEnv">The shipping environment type (TINYINT).</param>
        /// <returns>The primary key (Shipping ID) of the newly added shipping record, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddShipping(int shippingCarrierId, DateTime startDate, string status, byte shippingEnv)
        {
            string query = "EXEC [dbo].[sp_AddShipping] @ShippingCarrierID, @StartDate, @Status, @ShippingEnv";

            try
            {
                // Add the shipping record and return the new Shipping ID
                return ConnectionUtils.AddRowToTable(query, shippingCarrierId, startDate, status, shippingEnv);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Cancels a shipping record by updating its status to 'Cancelled' by calling the stored procedure [dbo].[SP_CancelShipping].
        /// </summary>
        /// <param name="shippingId">The ID of the shipping record to be canceled.</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static bool CancelShipping(int shippingId)
        {
            string query = "EXEC [dbo].[SP_CancelShipping] @ShippingID";

            try
            {
                // Update the shipping status to 'Cancelled'
                return ConnectionUtils.UpdateTableRow(query, shippingId);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Retrieves all shippings from the Shippings table.
        /// </summary>
        /// <returns>A DataTable containing the shipping details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllShippings()
        {
            string query = "EXEC sp_GetAllShippings";
            try
            {
                return ConnectionUtils.GetTable(query);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all pending shippings from the Shippings table where the status is not 'Arrived'.
        /// </summary>
        /// <returns>A DataTable containing the details of pending shippings.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetPendingShippings()
        {
            string query = "EXEC sp_GetPendingShippings";
            try
            {
                return ConnectionUtils.GetTable(query);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves details of a specific shipping entry from the Shippings table.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping entry to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified shipping entry.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetShippingDetails(int shippingId)
        {
            string query = "EXEC sp_GetShippingDetails @ShippingID";
            try
            {
                return ConnectionUtils.GetTable(query, shippingId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Marks the status of orders and shipping as arrived based on the provided shipping ID.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping to be marked as arrived.</param>
        /// <returns>True if the status was successfully updated; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool ShippingArrived(int shippingId)
        {
            string query = "EXEC SP_ShippingArrived @ShippingID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, shippingId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }
    }
}
