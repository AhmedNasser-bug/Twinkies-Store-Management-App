using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAccessLayer.Utils;

namespace TestAccessLayer
{
    public static class OrdersAccess
    {
        /// <summary>
        /// Retrieves a specific order by its ID from the Orders table.
        /// </summary>
        /// <param name="orderId">The ID of the order to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified order.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetOrderWithID(int orderId)
        {
            string query = "EXEC SP_GetOrderWithID @OrderID";
            try
            {
                return ConnectionUtils.GetTable(query, orderId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Adds a new order to the database by calling the stored procedure [dbo].[AddOrder].
        /// </summary>
        /// <param name="shippingID">The shipping ID associated with the order. Can be null.</param>
        /// <param name="env">The environment type (TINYINT).</param>
        /// <param name="customerID">The ID of the customer placing the order.</param>
        /// <param name="paymentMethod">The payment method used for the order.</param>
        /// <param name="price">The total price of the order.</param>
        /// <param name="depositAmount">The deposit amount for the order.</param>
        /// <param name="transactionNotes">Notes related to the transaction.</param>
        /// <param name="orderNotes">Notes related to the order.</param>
        /// <returns>The primary key (Order ID) of the newly added order, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        public static int AddOrder(int? shippingID, byte env, int customerID, string paymentMethod, decimal price, decimal depositAmount, string transactionNotes, string orderNotes)
        {
            string query = "EXEC [dbo].[AddOrder] @ShippingID, @Env, @CustomerID, @PaymentMethod, @Price, @DepositAmount, @TransactionNotes, @OrderNotes";

            try
            {
                // Add the order and return the new Order ID
                return ConnectionUtils.AddRowToTable(query, shippingID, env, customerID, paymentMethod, price, depositAmount, transactionNotes, orderNotes);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Cancels an order by updating its status and flags the associated transaction for return by calling the stored procedure [dbo].[SP_CancelOrder].
        /// </summary>
        /// <param name="orderId">The ID of the order to be canceled.</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception>SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception>InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception>ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception>Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static bool CancelOrder(int orderId)
        {
            string query = "EXEC [dbo].[SP_CancelOrder] @Order_ID";

            try
            {
                // Update the order status and flag the transaction for return
                return ConnectionUtils.UpdateTableRow(query, orderId);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Changes the status of an order and its associated products to delivered.
        /// </summary>
        /// <param name="orderId">The ID of the order to be updated.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool ChangeOrderToDelivered(int orderId)
        {
            string query = "EXEC SP_ChangeOrderToDelivered @OrderID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, orderId);
            }
            catch (SqlException ex)
            {
                // Logger.Error(ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all orders from the Orders table.
        /// </summary>
        /// <returns>A DataTable containing the order details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllOrders()
        {
            string query = "EXEC sp_GetAllOrders";
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
        /// Retrieves all cancelled orders for a specific customer from the Orders table.
        /// </summary>
        /// <param name="customerId">The ID of the customer whose cancelled orders are to be retrieved.</param>
        /// <returns>A DataTable containing the cancelled order details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetCancelledOrdersOfCustomer(int customerId)
        {
            string query = "EXEC SP_GetCancelledOrdersOFCustomer @CustomerID";
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

        /// <summary>
        /// Retrieves all products associated with a specific order from the OrderProducts table.
        /// </summary>
        /// <param name="orderId">The ID of the order whose products are to be retrieved.</param>
        /// <returns>A DataTable containing the product details for the specified order.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetOrderProducts(int orderId)
        {
            string query = "EXEC SP_GetOrderProducts @OrderID";
            try
            {
                return ConnectionUtils.GetTable(query, orderId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all orders for a specific customer from the Orders table.
        /// </summary>
        /// <param name="customerId">The ID of the customer whose orders are to be retrieved.</param>
        /// <returns>A DataTable containing the order details for the specified customer.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetOrdersOfCustomer(int customerId)
        {
            string query = "EXEC SP_GetOrdersOfCustomer @CustomerID";
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

        /// <summary>
        /// Retrieves all orders associated with a specific shipping ID from the OrdersInShipping table.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping whose orders are to be retrieved.</param>
        /// <returns>A DataTable containing the order details for the specified shipping ID.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetOrdersOfShipping(int shippingId)
        {
            string query = "EXEC SP_GetOrdersOfShipping @ShippingID";
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
    }
}
