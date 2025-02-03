using Microsoft.Data.SqlClient;
using System.Data;
using TestAccessLayer.Utils;

namespace TestAccessLayer
{
    public static class CustomersAccess
    {
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
        public static int AddCustomer(string name, string address)
        {
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
        /// <returns>A DataTable containing the customer details, or null if not found.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <summary>
        /// Finds a customer in the Customers table by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to be retrieved.</param>
        /// <param name="customerData">The DataTable containing the customer details.</param>
        /// <returns>True if the customer is found, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool FindCustomerWithID(int customerId, ref DataTable customerData)
        {
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
        /// Retrieves all customers from the Customers table.
        /// </summary>
        /// <returns>A DataTable containing the customer details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllCustomers()
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
        public static DataTable GetCustomersOfProduct(int productId)
        {
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
    }
}
