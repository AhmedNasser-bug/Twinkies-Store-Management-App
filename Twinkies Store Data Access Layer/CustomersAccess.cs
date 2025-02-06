using Microsoft.Data.SqlClient;
using System.Data;
using TwinkiesStoreDataAccessLayer.Utils;

namespace TwinkiesStoreDataAccessLayer
{

    /// <summary>
    /// Provides data access methods for Customer-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all customer-related database operations with the following features:
    /// - CRUD operations (except Delete as per business rules)
    /// - Async and sync operation support
    /// - Parameter validation
    /// - Error handling
    /// - Stored procedure execution
    /// 
    /// Database Table Structure:
    /// - CustomerID (PK): Integer
    /// - Name: String
    /// - Address: String
    /// </remarks>
    public static class CustomersAccess
    {
        // Fields: CustomerID (PK), Name, Address
        // Note: This class does not implement a delete method as per business rules

        #region Synchronous Methods
        /// <summary>
        /// Adds a new customer to the database by calling the stored procedure [dbo].[SP_AddCustomer].
        /// </summary>
        /// <param name="name">The name of the customer.</param>
        /// <param name="address">The address of the customer.</param>
        /// <returns>The primary key (Customer ID) of the newly added customer, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddCustomer(string name, string address) // Add
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            string query = "EXEC [dbo].[SP_AddCustomer] @Name, @Address";

            try
            {
                // Add the customer and return the new Customer ID
                return ConnectionUtils.AddRowToTable(query, name, address);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Finds a customer in the Customers table by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to be retrieved.</param>
        /// <param name="customerData">The DataTable containing the customer details.</param>
        /// <returns>True if the customer is found, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool FindCustomerWithID(int customerId, ref DataTable customerData) // Find
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));

            string query = "EXEC SP_FindCustomerWithID @CustomerID";
            try
            {
                customerData = ConnectionUtils.GetTable(query, customerId);
                return customerData.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Updates the details of a customer in the Customers table using the specified parameters.
        /// </summary>
        /// <param name="customerId">The ID of the customer to update.</param>
        /// <param name="name">The new name of the customer.</param>
        /// <param name="address">The new address of the customer.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">If there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">If the connection is not open or the command is invalid.</exception>
        public static bool UpdateCustomerDetails(int customerId, string name, string address) // Update
        {

            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            string query = "EXEC SP_EditCustomerDetails @CustomerID, @Name, @Address";

            try
            {
                return ConnectionUtils.UpdateTableRow(query, customerId, name, address);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all customers from the Customers table.
        /// </summary>
        /// <returns>A DataTable containing the customer details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllCustomers() // static, GetTable
        {
            string query = "EXEC sp_GetAllCustomers";
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
        /// Retrieves all customers who have ordered a specific product based on the product ID.
        /// </summary>
        /// <param name="productId">The ID of the product for which to retrieve customers.</param>
        /// <returns>A DataTable containing the customers who ordered the specified product.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetCustomersOfProduct(int productId) // static
        {
            if (productId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(productId));

            string query = "EXEC SP_GetCustomersOfProduct @ProductID";
            try
            {
                return ConnectionUtils.GetTable(query, productId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }
        #endregion

        #region Async Methods

        /// <summary>
        /// Asynchronously adds a new customer to the database.
        /// </summary>
        /// <param name="name">The name of the customer.</param>
        /// <param name="address">The address of the customer.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the primary key (Customer ID) of the newly added customer, or -1 if the operation fails.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not open or invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when name or address is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<int> AddCustomerAsync(
            string name,
            string address,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            string query = "EXEC [dbo].[SP_AddCustomer] @Name, @Address";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    name,
                    address);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously finds a customer by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to find.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a tuple with a boolean indicating if the customer was found and the customer data.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<(bool found, DataTable customerData)> FindCustomerWithIDAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));

            string query = "EXEC SP_FindCustomerWithID @CustomerID";
            try
            {
                var data = await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    customerId);
                return (data.Rows.Count > 0, data);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates customer details.
        /// </summary>
        /// <param name="customerId">The ID of the customer to update.</param>
        /// <param name="name">The new name of the customer.</param>
        /// <param name="address">The new address of the customer.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// true if the update was successful, false otherwise.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when name or address is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<bool> UpdateCustomerDetailsAsync(
            int customerId,
            string name,
            string address,
            CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            string query = "EXEC SP_EditCustomerDetails @CustomerID, @Name, @Address";

            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    customerId,
                    name,
                    address);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all customers.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all customer records.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetAllCustomersAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC sp_GetAllCustomers";
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
        /// Asynchronously retrieves all customers who ordered a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with customer records who ordered the specified product.
        /// </returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="ArgumentException">Thrown when productId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetCustomersOfProductAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));

            string query = "EXEC SP_GetCustomersOfProduct @ProductID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    productId);
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
