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
    /// Provides data access methods for Order-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all order-related database operations with the following features:
    /// - CRUD operations (except Delete as per business rules)
    /// - Order status management
    /// - Order-product relationships
    /// - Customer order history
    /// - Shipping integration
    /// - Async and sync operation support
    /// 
    /// Database Table Structure:
    /// - OrderID (PK): Integer
    /// - CustomerID (FK): Integer
    /// - Status: Enum (0: cancelled, 1:Shipping, 2:Arrived, 3:Delivered)
    /// - Shipping_env: Enum (1:land, 2:sea, 3:Air shipping)
    /// - Notes: String
    /// - OrderTimeStamp: DateTime
    /// 
    /// Related Tables:
    /// - Customers (via CustomerID)
    /// - OrderProducts (via OrderID)
    /// - Shipping (via ShippingID)
    /// 
    /// Business Rules:
    /// - Orders cannot be deleted (soft delete via status change)
    /// - Orders can be cancelled
    /// - Orders can be marked as delivered
    /// - Orders track shipping environment
    /// </remarks>
    public static class OrdersAccess
    {
        // Note: This class does not implement a delete method as per business rules

        #region Synchronous Methods
        /// <summary>
        /// Retrieves a specific order by its ID from the Orders table.
        /// </summary>
        /// <param name="orderId">The ID of the order to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified order.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetOrderWithID(int orderId) // Find
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
        public static int AddOrder(int? shippingID, byte env, int customerID, string paymentMethod, decimal price, decimal depositAmount, string transactionNotes, string orderNotes) // Add
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
        /// <exception cref = "SqlException"> Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref = "InvalidOperationException"> Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref = "ArgumentNullException"> Thrown when a required parameter is null.</exception>
        /// <exception cref = "Exception"> Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
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
        public static bool ChangeOrderToDelivered(int orderId) // static + instance + PLURAL 
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
        public static DataTable GetAllOrders() // static
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
        public static DataTable GetCancelledOrdersOfCustomer(int customerId) // static
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
        #endregion 

        #region Asynchronous Methods

        /// <summary>
        /// Asynchronously retrieves a specific order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation returning the order details.</returns>
        /// <exception cref="ArgumentException">Thrown when orderId is less than or equal to 0.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetOrderWithIDAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            string query = "EXEC SP_GetOrderWithID @OrderID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    orderId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously adds a new order to the database.
        /// </summary>
        /// <param name="shippingID">The shipping ID associated with the order. Can be null.</param>
        /// <param name="env">The environment type (TINYINT).</param>
        /// <param name="customerID">The ID of the customer placing the order.</param>
        /// <param name="paymentMethod">The payment method used for the order.</param>
        /// <param name="price">The total price of the order.</param>
        /// <param name="depositAmount">The deposit amount for the order.</param>
        /// <param name="transactionNotes">Notes related to the transaction.</param>
        /// <param name="orderNotes">Notes related to the order.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the primary key (Order ID) of the newly added order, or -1 if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when required numeric parameters are invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when required string parameters are null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<int> AddOrderAsync(
            int? shippingID,
            byte env,
            int customerID,
            string paymentMethod,
            decimal price,
            decimal depositAmount,
            string transactionNotes,
            string orderNotes,
            CancellationToken cancellationToken = default)
        {
            if (customerID <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerID));
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentNullException(nameof(paymentMethod));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));
            if (depositAmount < 0)
                throw new ArgumentException("Deposit amount cannot be negative.", nameof(depositAmount));

            string query = "EXEC [dbo].[AddOrder] @ShippingID, @Env, @CustomerID, @PaymentMethod, " +
                          "@Price, @DepositAmount, @TransactionNotes, @OrderNotes";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    shippingID,
                    env,
                    customerID,
                    paymentMethod,
                    price,
                    depositAmount,
                    transactionNotes ?? string.Empty,
                    orderNotes ?? string.Empty);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously cancels an order and flags its transaction for return.
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// true if the cancellation was successful, false otherwise.
        /// </returns>
        public static async Task<bool> CancelOrderAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            // Validate order ID to prevent unnecessary database calls
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            string query = "EXEC [dbo].[SP_CancelOrder] @Order_ID";

            try
            {
                // Execute the cancellation stored procedure
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    orderId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously marks an order and its products as delivered.
        /// </summary>
        /// <param name="orderId">The ID of the order to mark as delivered.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// true if the status update was successful, false otherwise.
        /// </returns>
        public static async Task<bool> ChangeOrderToDeliveredAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            // Validate order ID to prevent unnecessary database calls
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            string query = "EXEC SP_ChangeOrderToDelivered @OrderID";
            try
            {
                // Update order status to delivered
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    orderId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all orders from the database.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all orders.
        /// </returns>
        public static async Task<DataTable> GetAllOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            // Simple query to get all orders - no parameters needed
            string query = "EXEC sp_GetAllOrders";
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
        /// Asynchronously retrieves all cancelled orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all cancelled orders for the specified customer.
        /// </returns>
        public static async Task<DataTable> GetCancelledOrdersOfCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // Validate customer ID to prevent unnecessary database calls
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));

            string query = "EXEC SP_GetCancelledOrdersOFCustomer @CustomerID";
            try
            {
                // Retrieve cancelled orders for the specified customer
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    customerId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all products associated with a specific order.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all products in the specified order.
        /// </returns>
        public static async Task<DataTable> GetOrderProductsAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            // Validate order ID to prevent unnecessary database calls
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            string query = "EXEC SP_GetOrderProducts @OrderID";
            try
            {
                // Retrieve all products for the specified order
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    orderId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all orders for the specified customer.
        /// </returns>
        public static async Task<DataTable> GetOrdersOfCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // Validate customer ID to prevent unnecessary database calls
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));

            string query = "EXEC SP_GetOrdersOfCustomer @CustomerID";
            try
            {
                // Retrieve all orders for the specified customer
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    customerId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all orders associated with a specific shipping ID.
        /// </summary>
        /// <param name="shippingId">The shipping ID to query.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all orders for the specified shipping ID.
        /// </returns>
        public static async Task<DataTable> GetOrdersOfShippingAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            // Validate shipping ID to prevent unnecessary database calls
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.", nameof(shippingId));

            string query = "EXEC SP_GetOrdersOfShipping @ShippingID";
            try
            {
                // Retrieve all orders for the specified shipping ID
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

        #endregion 
    }
}
