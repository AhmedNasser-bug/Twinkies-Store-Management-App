using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using TestAccessLayer.Utils;

namespace TestAccessLayer
{
    public static class TransactionsAccess
    {
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
    }

}
