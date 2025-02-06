using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.SqlServer;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TwinkiesStoreDataAccessLayer.Utils;
using System.Data;

namespace TwinkiesStoreDataAccessLayer
{
    /// <summary>
    /// Provides data access methods for Phone-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all phone-related database operations with the following features:
    /// - CRUD operations (except Delete as per business rules)
    /// - Customer phone number management
    /// - Phone number validation
    /// - Async and sync operation support
    /// 
    /// Database Table Structure:
    /// - PhoneID (PK): Integer
    /// - PhoneNumber: String
    /// - CustomerID (FK): Integer
    /// 
    /// Related Tables:
    /// - Customers (via CustomerID)
    /// 
    /// Business Rules:
    /// - Phone numbers cannot be deleted
    /// - Each phone number must be associated with a customer
    /// - Phone numbers must be unique per customer
    /// - Phone numbers must follow a valid format
    /// </remarks>
    public static class PhonesAccess
    {
        // Fields: phoneId (PK), phoneNumber, customerId (FK)
        // Note: This class does not implement a delete method as per business rules

        #region Constants
        // Regular expression for phone number validation
        private const string PHONE_NUMBER_PATTERN = @"^\+?[1-9]\d{1,14}$";
        #endregion

        #region Synchronous Methods
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
        public static int AddPhone(string phoneNumber, int customerId) // Add
        {
            string query = "EXEC [dbo].[SP_AddPhone] @PhoneNumber, @Customer_ID";

            // Validate phone number format
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            // Validate phone number format using regex
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, PHONE_NUMBER_PATTERN))
                throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));

            // Validate customer ID
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

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
        /// Updates the phone number in the Phones table.
        /// </summary>
        /// <param name="newPhone">The new phone number to set.</param>
        /// <param name="phoneId">The ID of the phone to update.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">If there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">If the connection is not open or the command is invalid.</exception>
        public static bool UpdatePhone(string newPhone, int phoneId) // Update
        {

            string query = "EXEC SP_EditPhone @NewPhone, @PhoneID";

            // Validate new phone number
            if (string.IsNullOrWhiteSpace(newPhone))
                throw new ArgumentNullException(nameof(newPhone));

            // Validate phone number format using regex
            if (!System.Text.RegularExpressions.Regex.IsMatch(newPhone, PHONE_NUMBER_PATTERN))
                throw new ArgumentException("Invalid phone number format.", nameof(newPhone));

            // Validate phone ID
            if (phoneId <= 0)
                throw new ArgumentException("Phone ID must be greater than zero.",
                    nameof(phoneId));

            try
            {
                return ConnectionUtils.UpdateTableRow(query, newPhone, phoneId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all phone numbers associated with a specific customer from the Phones table.
        /// </summary>
        /// <param name="customerId">The ID of the customer whose phone numbers are to be retrieved.</param>
        /// <returns>A DataTable containing the phone numbers for the specified customer.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetPhonesOf(int customerId) // static only
        { 
            // Validate customer ID
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

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
        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// Asynchronously adds a new phone number for a customer.
        /// </summary>
        /// <param name="phoneNumber">The phone number to be added.</param>
        /// <param name="customerId">The ID of the customer associated with the phone number.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the primary key (Phone ID) of the newly added phone number, or -1 if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when phoneNumber is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when phoneNumber is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<int> AddPhoneAsync(
            string phoneNumber,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // Validate phone number format
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            // Validate phone number format using regex
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, PHONE_NUMBER_PATTERN))
                throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));

            // Validate customer ID
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            string query = "EXEC [dbo].[SP_AddPhone] @PhoneNumber, @Customer_ID";

            try
            {
                // Add the phone number and return the new Phone ID
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    phoneNumber,
                    customerId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates a phone number.
        /// </summary>
        /// <param name="newPhone">The new phone number.</param>
        /// <param name="phoneId">The ID of the phone to update.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// true if the update was successful, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when newPhone is invalid or phoneId is less than or equal to 0.</exception>
        /// <exception cref="ArgumentNullException">Thrown when newPhone is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<bool> UpdatePhoneAsync(
            string newPhone,
            int phoneId,
            CancellationToken cancellationToken = default)
        {
            // Validate new phone number
            if (string.IsNullOrWhiteSpace(newPhone))
                throw new ArgumentNullException(nameof(newPhone));

            // Validate phone number format using regex
            if (!System.Text.RegularExpressions.Regex.IsMatch(newPhone, PHONE_NUMBER_PATTERN))
                throw new ArgumentException("Invalid phone number format.", nameof(newPhone));

            // Validate phone ID
            if (phoneId <= 0)
                throw new ArgumentException("Phone ID must be greater than zero.",
                    nameof(phoneId));

            string query = "EXEC SP_EditPhone @NewPhone, @PhoneID";

            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    newPhone,
                    phoneId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all phone numbers for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all phone numbers for the specified customer.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when customerId is less than or equal to 0.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetPhonesOfAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // Validate customer ID
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            string query = "EXEC SP_GetPhonesOf @Customer_ID";
            try
            {
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

        #endregion
    }
}
