using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinkiesStoreDataAccessLayer.Utils;
using Microsoft.Data.SqlClient;

namespace TwinkiesStoreDataAccessLayer
{
    /// <summary>
    /// Provides data access methods for Shipping-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all shipping-related database operations with the following features:
    /// - CRUD operations (except Delete as per business rules)
    /// - Shipping status management
    /// - Carrier relationship handling
    /// - Environment type tracking
    /// - Async and sync operation support
    /// 
    /// Database Table Structure:
    /// - ShippingID (PK): Integer
    /// - ShippingCarrierID (FK): Integer
    /// - StartDate: DateTime
    /// - Status: String
    /// - ShippingEnv: Byte
    /// 
    /// Related Tables:
    /// - ShippingCarriers (via ShippingCarrierID)
    /// - Orders (via ShippingID)
    /// 
    /// Business Rules:
    /// - Shipping records cannot be deleted
    /// - Shipping can be cancelled
    /// - Shipping can be marked as arrived
    /// - Shipping environment must be tracked
    /// </remarks>
    public static class ShippingAccess
    {
        // Fields: ShippingID (PK), ShippingCarrierID (FK), StartDate, Status, ShippingEnv
        // Note: The class does not implement a Delete method as per business rules

        #region Synchronous Methods
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
        public static int AddShipping(int shippingCarrierId, DateTime startDate, string status, byte shippingEnv) // Add
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
        /// Updates shipping details in the Shippings table.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping to update.</param>
        /// <param name="shippingCarrierId">The ID of the shipping carrier.</param>
        /// <param name="startDate">The start date of the shipping.</param>
        /// <param name="status">The status of the shipping.</param>
        /// <param name="shippingEnv">The shipping environment type.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the database connection fails.</exception>
        /// <exception cref="ArgumentException">Thrown when invalid arguments are provided.</exception>
        public static bool UpdateShipping(int shippingId, int shippingCarrierId, DateTime startDate, string status, byte shippingEnv) // Update
        {
            string query = "SP_UpdateShipping @ShippingID, @ShippingCarrierID, @StartDate, @Status, @ShippingEnv";

            try
            {
                return ConnectionUtils.UpdateTableRow(query, shippingId, shippingCarrierId, startDate, status, shippingEnv);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                return false;
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
        public static DataTable GetAllShippings() // static
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
        public static DataTable GetPendingShippings() // static
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
        public static DataTable GetShippingDetails(int shippingId) // Find
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

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// Asynchronously adds a new shipping record to the database.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier.</param>
        /// <param name="startDate">The start date of the shipping.</param>
        /// <param name="status">The status of the shipping.</param>
        /// <param name="shippingEnv">The shipping environment type (TINYINT).</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the primary key (Shipping ID) of the newly added shipping record, or -1 if the operation fails.
        /// </returns>
        public static async Task<int> AddShippingAsync(
            int shippingCarrierId,
            DateTime startDate,
            string status,
            byte shippingEnv,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (shippingCarrierId <= 0)
                throw new ArgumentException("Shipping carrier ID must be greater than zero.",
                    nameof(shippingCarrierId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));
            if (startDate == default)
                throw new ArgumentException("Start date cannot be default value.",
                    nameof(startDate));

            string query = "EXEC [dbo].[sp_AddShipping] @ShippingCarrierID, @StartDate, " +
                          "@Status, @ShippingEnv";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    shippingCarrierId,
                    startDate,
                    status,
                    shippingEnv);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates shipping details.
        /// </summary>
        public static async Task<bool> UpdateShippingAsync(
            int shippingId,
            int shippingCarrierId,
            DateTime startDate,
            string status,
            byte shippingEnv,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));
            if (shippingCarrierId <= 0)
                throw new ArgumentException("Shipping carrier ID must be greater than zero.",
                    nameof(shippingCarrierId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));
            if (startDate == default)
                throw new ArgumentException("Start date cannot be default value.",
                    nameof(startDate));

            string query = "SP_UpdateShipping @ShippingID, @ShippingCarrierID, @StartDate, " +
                          "@Status, @ShippingEnv";

            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    shippingId,
                    shippingCarrierId,
                    startDate,
                    status,
                    shippingEnv);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously cancels a shipping record.
        /// </summary>
        public static async Task<bool> CancelShippingAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            string query = "EXEC [dbo].[SP_CancelShipping] @ShippingID";

            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    shippingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all shippings.
        /// </summary>
        public static async Task<DataTable> GetAllShippingsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC sp_GetAllShippings";
            try
            {
                return await ConnectionUtils.GetTableAsync(query, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all pending shippings.
        /// </summary>
        public static async Task<DataTable> GetPendingShippingsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC sp_GetPendingShippings";
            try
            {
                return await ConnectionUtils.GetTableAsync(query, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves details of a specific shipping entry.
        /// </summary>
        public static async Task<DataTable> GetShippingDetailsAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            string query = "EXEC sp_GetShippingDetails @ShippingID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    shippingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously marks a shipping as arrived.
        /// </summary>
        public static async Task<bool> ShippingArrivedAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            string query = "EXEC SP_ShippingArrived @ShippingID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    shippingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        #endregion
    }
}
