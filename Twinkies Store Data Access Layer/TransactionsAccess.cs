using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using TwinkiesStoreDataAccessLayer.Utils;

namespace TwinkiesStoreDataAccessLayer
{


    /// <summary>
    /// Provides data access methods for Transaction-related database operations.
    /// </summary>
    /// <remarks>
    /// Database Table Structure:
    /// - TransactionID (PK): Integer
    /// - Notes: String
    /// - Amount: Decimal
    /// - PaymentMethod: Byte
    /// - Reason: Byte
    /// - Sender: String
    /// - Receiver: String
    /// - Type: Boolean (0 for sent, 1 for received)
    /// </remarks>
    public static class TransactionsAccess
    {

        #region Synchronous Methods

        /// <summary>
        /// Adds a new transaction to the database by calling the stored procedure [dbo].[SP_AddTransaction].
        /// </summary>
        /// <param name="notes">Notes related to the transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="paymentMethod">The payment method used (tinyint).</param>
        /// <param name="reason">The reason for the transaction (tinyint).</param>
        /// <param name="sender">The sender of the transaction.</param>
        /// <param name="receiver">The receiver of the transaction.</param>
        /// <param name="type">The type of the transaction (bit).</param>
        /// <returns>The primary key (Transaction ID) of the newly added transaction, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddTransaction(string notes, decimal amount, byte paymentMethod, byte reason, string sender, string receiver, bool type)
        {
            string query = "EXEC [dbo].[SP_AddTransaction] @Notes, @Amount, @Payment_Method, @Reason, @Sender, @Reciever, @Type";

            try
            {
                // Add the transaction and return the new Transaction ID
                return ConnectionUtils.AddRowToTable(query, notes, amount, paymentMethod, reason, sender, receiver, type);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Edits the details of a transaction in the Transactions table.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to be updated.</param>
        /// <param name="notes">The new notes for the transaction.</param>
        /// <param name="amount">The new amount for the transaction.</param>
        /// <param name="paymentMethod">The new payment method for the transaction.</param>
        /// <param name="reason">The reason for the transaction.</param>
        /// <param name="sender">The sender of the transaction.</param>
        /// <param name="receiver">The receiver of the transaction.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool EditTransaction(int transactionId, string notes, decimal amount,
                                            byte paymentMethod, byte reason,
                                            string sender, string receiver, bool type)
        {
            string query = "EXEC SP_EditTransaction @TransactionID, @Notes, @Amount, @Payment_Method, @Reason, @Sender, @Reciever, @Type";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, transactionId, notes, amount, paymentMethod, reason, sender, receiver, type);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Flags a transaction as done by updating its notes in the Transactions table.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to be updated.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool FlagTransactionForDone(int transactionId)
        {
            string query = "EXEC SP_FlagTransactionForDone @TransactionID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, transactionId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Flags a transaction for return by updating its notes in the Transactions table.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to be updated.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool FlagTransactionForReturn(int transactionId)
        {
            string query = "EXEC SP_FlagTransactionForReturn @TransactionID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, transactionId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all unpaid transactions from the Transactions table where notes indicate 'MUST RETURN'.
        /// </summary>
        /// <returns>A DataTable containing the unpaid transaction details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllUnpaidTransactions()
        {
            string query = "EXEC SP_GetAllUnpaidTransactions";
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
        /// Retrieves all received transactions from the Transactions table where the type is 1.
        /// </summary>
        /// <returns>A DataTable containing the details of the received transactions.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetReceivedTransactions()
        {
            string query = "EXEC SP_GetRecievedTransactions";
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
        /// Retrieves all sent transactions from the Transactions table where the type is 0.
        /// </summary>
        /// <returns>A DataTable containing the details of the sent transactions.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetSentTransactions()
        {
            string query = "EXEC SP_GetSentTransactions";
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
        /// Retrieves a specific transaction by its ID from the Transactions table.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified transaction.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetTransaction(int transactionId)
        {
            string query = "EXEC SP_GetTransaction @TransactionID";
            try
            {
                return ConnectionUtils.GetTable(query, transactionId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all transactions from the Transactions table.
        /// </summary>
        /// <returns>A DataTable containing all transactions.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetTransactions()
        {
            string query = "EXEC SP_GetTransactions";
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

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// Asynchronously adds a new transaction.
        /// </summary>
        /// <param name="notes">Notes related to the transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="paymentMethod">The payment method used (tinyint).</param>
        /// <param name="reason">The reason for the transaction (tinyint).</param>
        /// <param name="sender">The sender of the transaction.</param>
        /// <param name="receiver">The receiver of the transaction.</param>
        /// <param name="type">The type of the transaction (bit).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The ID of the new transaction, or -1 if operation fails.</returns>
        /// <exception cref="ArgumentException">Thrown when amount is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown when required string parameters are null.</exception>
        public static async Task<int> AddTransactionAsync(
            string notes,
            decimal amount,
            byte paymentMethod,
            byte reason,
            string sender,
            string receiver,
            bool type,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentNullException(nameof(sender));
            if (string.IsNullOrWhiteSpace(receiver))
                throw new ArgumentNullException(nameof(receiver));
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            string query = "EXEC [dbo].[SP_AddTransaction] @Notes, @Amount, @Payment_Method, " +
                          "@Reason, @Sender, @Reciever, @Type";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    notes ?? string.Empty,
                    amount,
                    paymentMethod,
                    reason,
                    sender,
                    receiver,
                    type);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously edits a transaction's details.
        /// </summary>
        public static async Task<bool> EditTransactionAsync(
            int transactionId,
            string notes,
            decimal amount,
            byte paymentMethod,
            byte reason,
            string sender,
            string receiver,
            bool type,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be positive.",
                    nameof(transactionId));
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentNullException(nameof(sender));
            if (string.IsNullOrWhiteSpace(receiver))
                throw new ArgumentNullException(nameof(receiver));
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            string query = "EXEC SP_EditTransaction @TransactionID, @Notes, @Amount, " +
                          "@Payment_Method, @Reason, @Sender, @Reciever, @Type";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    transactionId,
                    notes ?? string.Empty,
                    amount,
                    paymentMethod,
                    reason,
                    sender,
                    receiver,
                    type);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously flags a transaction as done.
        /// </summary>
        public static async Task<bool> FlagTransactionForDoneAsync(
            int transactionId,
            CancellationToken cancellationToken = default)
        {
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be positive.",
                    nameof(transactionId));

            string query = "EXEC SP_FlagTransactionForDone @TransactionID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    transactionId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously flags a transaction for return.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to flag.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when transactionId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<bool> FlagTransactionForReturnAsync(
            int transactionId,
            CancellationToken cancellationToken = default)
        {
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be positive.",
                    nameof(transactionId));

            string query = "EXEC SP_FlagTransactionForReturn @TransactionID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    transactionId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all unpaid transactions.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing unpaid transaction details.</returns>
        /// <remarks>
        /// Retrieves transactions where notes indicate 'MUST RETURN'.
        /// </remarks>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetAllUnpaidTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetAllUnpaidTransactions";
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
        /// Asynchronously retrieves all received transactions.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing received transaction details.</returns>
        /// <remarks>
        /// Retrieves transactions where type = 1 (received).
        /// </remarks>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetReceivedTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetRecievedTransactions";
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
        /// Asynchronously retrieves all sent transactions.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing sent transaction details.</returns>
        /// <remarks>
        /// Retrieves transactions where type = 0 (sent).
        /// </remarks>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetSentTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetSentTransactions";
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
        /// Asynchronously retrieves a specific transaction by its ID.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the transaction details.</returns>
        /// <exception cref="ArgumentException">Thrown when transactionId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetTransactionAsync(
            int transactionId,
            CancellationToken cancellationToken = default)
        {
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be positive.",
                    nameof(transactionId));

            string query = "EXEC SP_GetTransaction @TransactionID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    transactionId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all transactions.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all transaction details.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetTransactions";
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

        #endregion
    }

}
