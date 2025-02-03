using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAccessLayer.Utils;

namespace TestAccessLayer
{
    public static class ShippingCarriersAccess
    {
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
    }
}
