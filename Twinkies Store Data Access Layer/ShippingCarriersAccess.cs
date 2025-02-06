using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinkiesStoreDataAccessLayer.Utils;

namespace TwinkiesStoreDataAccessLayer
{
    /// <summary>
    /// Provides data access methods for ShippingCarrier-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all shipping carrier-related database operations with the following features:
    /// - Complete CRUD operations
    /// - Caching support for frequently accessed data
    /// - Input validation and business rules enforcement
    /// 
    /// Database Table Structure:
    /// - ShippingCarrierID (PK): Integer
    /// - ShippingCarrierName: String
    /// - OrderTime: Integer (minutes)
    /// - ShippingCost: Decimal
    /// 
    /// Business Rules:
    /// - Shipping carrier names must be unique
    /// - Order time must be positive
    /// - Shipping cost must be non-negative
    /// </remarks>
    public static class ShippingCarriersAccess
    {

        #region Synchronous Methods

        /// <summary>
        /// Adds a new shipping carrier to the database by calling the stored procedure [dbo].[sp_AddShippingCarrier].
        /// </summary>
        /// <param name="shippingCarrierName">The name of the shipping carrier.</param>
        /// <param name="orderTime">The order time associated with the shipping carrier.</param>
        /// <param name="shippingCost">The shipping cost associated with the shipping carrier.</param>
        /// <returns>The primary key (Shipping Carrier ID) of the newly added shipping carrier, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddShippingCarrier(string shippingCarrierName, int orderTime, decimal shippingCost)
        {
            string query = "EXEC [dbo].[sp_AddShippingCarrier] @ShippingCarrierName, @OrderTime, @ShippingCost";

            try
            {
                // Add the shipping carrier and return the new Shipping Carrier ID
                return ConnectionUtils.AddRowToTable(query, shippingCarrierName, orderTime, shippingCost);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Deletes a shipping carrier from the ShippingCarriers table by its ID.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to be deleted.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DeleteShippingCarrier(int shippingCarrierId)
        {
            string query = "EXEC sp_DeleteShippingCarrier @ShippingCarrierID";
            try
            {
                return ConnectionUtils.DeleteTableRow(query, shippingCarrierId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all shipping carriers from the ShippingCarriers table.
        /// </summary>
        /// <returns>A DataTable containing the shipping carrier details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllShippingCarriers()
        {
            string query = "EXEC sp_GetAllShippingCarriers";
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
        /// Retrieves details of a specific shipping carrier from the ShippingCarriers table.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified shipping carrier.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetShippingCarrierDetails(int shippingCarrierId)
        {
            string query = "EXEC sp_GetShippingCarrierDetails @ShippingCarrierID";
            try
            {
                return ConnectionUtils.GetTable(query, shippingCarrierId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Updates the details of a specific shipping carrier in the ShippingCarriers table.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to be updated.</param>
        /// <param name="shippingCarrierName">The new name of the shipping carrier.</param>
        /// <param name="orderTime">The new order time for the shipping carrier.</param>
        /// <param name="shippingCost">The new shipping cost for the shipping carrier.</param>
        /// <returns>True if the shipping carrier was successfully updated; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool UpdateShippingCarrier(int shippingCarrierId, string shippingCarrierName, int orderTime, decimal shippingCost)
        {
            string query = "EXEC sp_UpdateShippingCarrier @ShippingCarrierID, @ShippingCarrierName, @OrderTime, @ShippingCost";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, shippingCarrierId, shippingCarrierName, orderTime, shippingCost);
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
        /// Asynchronously adds a new shipping carrier.
        /// </summary>
        /// <param name="shippingCarrierName">The name of the carrier.</param>
        /// <param name="orderTime">The order time in minutes.</param>
        /// <param name="shippingCost">The shipping cost.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The ID of the new shipping carrier, or -1 if operation fails.</returns>
        /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
        public static async Task<int> AddShippingCarrierAsync(
            string shippingCarrierName,
            int orderTime,
            decimal shippingCost,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(shippingCarrierName))
                throw new ArgumentNullException(nameof(shippingCarrierName));
            if (orderTime <= 0)
                throw new ArgumentException("Order time must be positive.", nameof(orderTime));
            if (shippingCost < 0)
                throw new ArgumentException("Shipping cost cannot be negative.",
                    nameof(shippingCost));

            string query = "EXEC [dbo].[sp_AddShippingCarrier] @ShippingCarrierName, " +
                          "@OrderTime, @ShippingCost";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    shippingCarrierName,
                    orderTime,
                    shippingCost);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously deletes a shipping carrier.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when shippingCarrierId is invalid.</exception>
        public static async Task<bool> DeleteShippingCarrierAsync(
            int shippingCarrierId,
            CancellationToken cancellationToken = default)
        {
            if (shippingCarrierId <= 0)
                throw new ArgumentException("Shipping carrier ID must be positive.",
                    nameof(shippingCarrierId));

            string query = "EXEC sp_DeleteShippingCarrier @ShippingCarrierID";
            try
            {
                return await ConnectionUtils.DeleteTableRowAsync(
                    query,
                    cancellationToken,
                    shippingCarrierId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all shipping carriers.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing all shipping carriers.</returns>
        public static async Task<DataTable> GetAllShippingCarriersAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC sp_GetAllShippingCarriers";
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
        /// Asynchronously retrieves details of a specific shipping carrier.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing the shipping carrier details.</returns>
        /// <exception cref="ArgumentException">Thrown when shippingCarrierId is invalid.</exception>
        public static async Task<DataTable> GetShippingCarrierDetailsAsync(
            int shippingCarrierId,
            CancellationToken cancellationToken = default)
        {
            if (shippingCarrierId <= 0)
                throw new ArgumentException("Shipping carrier ID must be positive.",
                    nameof(shippingCarrierId));

            string query = "EXEC sp_GetShippingCarrierDetails @ShippingCarrierID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    shippingCarrierId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates a shipping carrier's details.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to update.</param>
        /// <param name="shippingCarrierName">The new name of the carrier.</param>
        /// <param name="orderTime">The new order time in minutes.</param>
        /// <param name="shippingCost">The new shipping cost.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if update was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
        public static async Task<bool> UpdateShippingCarrierAsync(
            int shippingCarrierId,
            string shippingCarrierName,
            int orderTime,
            decimal shippingCost,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (shippingCarrierId <= 0)
                throw new ArgumentException("Shipping carrier ID must be positive.",
                    nameof(shippingCarrierId));
            if (string.IsNullOrWhiteSpace(shippingCarrierName))
                throw new ArgumentNullException(nameof(shippingCarrierName));
            if (orderTime <= 0)
                throw new ArgumentException("Order time must be positive.", nameof(orderTime));
            if (shippingCost < 0)
                throw new ArgumentException("Shipping cost cannot be negative.",
                    nameof(shippingCost));

            string query = "EXEC sp_UpdateShippingCarrier @ShippingCarrierID, " +
                          "@ShippingCarrierName, @OrderTime, @ShippingCost";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    shippingCarrierId,
                    shippingCarrierName,
                    orderTime,
                    shippingCost);
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
