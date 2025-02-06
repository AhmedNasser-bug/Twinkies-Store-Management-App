using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using TwinkiesStoreDataAccessLayer;
using System.Data;

namespace TwinkiesStoreBusinessLayer
{
    /// <summary>
    /// Represents a transaction entity and provides methods for transaction-related operations.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for:
    /// - Creating and updating transactions
    /// - Managing transaction status (done/return)
    /// - Retrieving transactions by various criteria
    /// - Both synchronous and asynchronous operations
    /// </remarks>
    public class Transaction : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _transactionId;
        private string _notes;
        private decimal _amount;
        private byte _paymentMethod;
        private byte _reason;
        private string _sender;
        private string _receiver;
        private bool _type;

        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private const string CACHE_KEY = "AllTransactions";
        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));
        #endregion

        #region Properties
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Transaction ID")]
        public int TransactionID
        {
            get => _transactionId;
            private set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Payment method is required")]
        [Range(0, 255, ErrorMessage = "Invalid payment method")]
        public byte PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Reason is required")]
        [Range(0, 255, ErrorMessage = "Invalid reason")]
        public byte Reason
        {
            get => _reason;
            set
            {
                if (_reason != value)
                {
                    _reason = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Sender is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Sender name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Sender name contains invalid characters")]
        public string Sender
        {
            get => _sender;
            set
            {
                if (_sender != value)
                {
                    _sender = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Receiver is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Receiver name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Receiver name contains invalid characters")]
        public string Receiver
        {
            get => _receiver;
            set
            {
                if (_receiver != value)
                {
                    _receiver = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public bool Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                var validationResults = new List<ValidationResult>();
                var property = GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(this);
                    var validationContext = new ValidationContext(this)
                    {
                        MemberName = propertyName
                    };

                    if (!Validator.TryValidateProperty(value, validationContext, validationResults))
                    {
                        return string.Join(Environment.NewLine,
                            validationResults.Select(r => r.ErrorMessage));
                    }
                }
                return string.Empty;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Private constructor to create a Transaction instance with specific values.
        /// </summary>
        private Transaction(int transactionId, string notes, decimal amount,
            byte paymentMethod, byte reason, string sender, string receiver, bool type)
        {
            TransactionID = transactionId;
            Notes = notes;
            Amount = amount;
            PaymentMethod = paymentMethod;
            Reason = reason;
            Sender = sender;
            Receiver = receiver;
            Type = type;
            _Mode = enMode.Update;
        }

        /// <summary>
        /// Initializes a new instance of the Transaction class with default values.
        /// </summary>
        public Transaction()
        {
            TransactionID = -1;
            Notes = string.Empty;
            Amount = 0;
            PaymentMethod = 0;
            Reason = 0;
            Sender = string.Empty;
            Receiver = string.Empty;
            Type = false;
            _Mode = enMode.AddNew;
        }
        #endregion

        #region Private Methods
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            var validationContext = new ValidationContext(this)
            {
                MemberName = propertyName
            };

            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(value, validationContext, validationResults))
            {
                throw new ValidationException(
                    string.Join(Environment.NewLine,
                    validationResults.Select(r => r.ErrorMessage)));
            }
        }

        private async Task ValidatePropertiesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                var validationContext = new ValidationContext(this);
                var validationResults = new List<ValidationResult>();

                if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
                {
                    throw new ValidationException(
                        string.Join(Environment.NewLine,
                        validationResults.Select(r => r.ErrorMessage)));
                }
            }, cancellationToken);
        }
        #endregion

        #region Private Core Methods
        /// <summary>
        /// Updates the current transaction in the database.
        /// </summary>
        /// <returns>True if update was successful; otherwise, false.</returns>
        private bool _Update()
        {
            return TransactionsAccess.EditTransaction(
                TransactionID, Notes, Amount, PaymentMethod,
                Reason, Sender, Receiver, Type);
        }

        /// <summary>
        /// Adds the current transaction to the database.
        /// </summary>
        /// <returns>True if addition was successful; otherwise, false.</returns>
        private bool _AddNew()
        {
            TransactionID = TransactionsAccess.AddTransaction(
                Notes, Amount, PaymentMethod, Reason,
                Sender, Receiver, Type);
            return TransactionID != -1;
        }

        /// <summary>
        /// Asynchronously updates the current transaction.
        /// </summary>
        private async Task<bool> _UpdateAsync(CancellationToken cancellationToken)
        {
            return await TransactionsAccess.EditTransactionAsync(
                TransactionID, Notes, Amount, PaymentMethod,
                Reason, Sender, Receiver, Type, cancellationToken);
        }

        /// <summary>
        /// Asynchronously adds the current transaction.
        /// </summary>
        private async Task<bool> _AddNewAsync(CancellationToken cancellationToken)
        {
            TransactionID = await TransactionsAccess.AddTransactionAsync(
                Notes, Amount, PaymentMethod, Reason,
                Sender, Receiver, Type, cancellationToken);
            return TransactionID != -1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the current transaction to the database.
        /// </summary>
        /// <returns>True if the save operation was successful; otherwise, false.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        public bool Save()
        {
            var validationContext = new ValidationContext(this);
            Validator.ValidateObject(this, validationContext, true);

            bool result = _Mode switch
            {
                enMode.AddNew => _AddNew(),
                enMode.Update => _Update(),
                _ => false
            };

            if (result && _Mode == enMode.AddNew)
            {
                _Mode = enMode.Update;
                _cache.Remove(CACHE_KEY); // Invalidate cache after successful save
            }

            return result;
        }

        /// <summary>
        /// Asynchronously saves the current transaction to the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the save operation was successful; otherwise, false.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        /// <exception cref="SqlException">Thrown when database operation fails.</exception>
        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);

            bool result = _Mode switch
            {
                enMode.AddNew => await _AddNewAsync(cancellationToken),
                enMode.Update => await _UpdateAsync(cancellationToken),
                _ => false
            };

            if (result && _Mode == enMode.AddNew)
            {
                _Mode = enMode.Update;
                await Task.Run(() => _cache.Remove(CACHE_KEY), cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Flags the current transaction as done.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when transaction is in AddNew mode or has invalid ID.
        /// </exception>
        public bool FlagAsDone()
        {
            if (_Mode == enMode.AddNew || TransactionID <= 0)
                throw new InvalidOperationException("Cannot flag an unsaved transaction as done.");

            return TransactionsAccess.FlagTransactionForDone(TransactionID);
        }

        /// <summary>
        /// Asynchronously flags the current transaction as done.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when transaction is in AddNew mode or has invalid ID.
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> FlagAsDoneAsync(CancellationToken cancellationToken = default)
        {
            if (_Mode == enMode.AddNew || TransactionID <= 0)
                throw new InvalidOperationException("Cannot flag an unsaved transaction as done.");

            return await TransactionsAccess.FlagTransactionForDoneAsync(
                TransactionID, cancellationToken);
        }

        /// <summary>
        /// Flags the current transaction for return.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when transaction is in AddNew mode or has invalid ID.
        /// </exception>
        public bool FlagForReturn()
        {
            if (_Mode == enMode.AddNew || TransactionID <= 0)
                throw new InvalidOperationException("Cannot flag an unsaved transaction for return.");

            return TransactionsAccess.FlagTransactionForReturn(TransactionID);
        }

        /// <summary>
        /// Asynchronously flags the current transaction for return.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when transaction is in AddNew mode or has invalid ID.
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> FlagForReturnAsync(CancellationToken cancellationToken = default)
        {
            if (_Mode == enMode.AddNew || TransactionID <= 0)
                throw new InvalidOperationException("Cannot flag an unsaved transaction for return.");

            return await TransactionsAccess.FlagTransactionForReturnAsync(
                TransactionID, cancellationToken);
        }

        #region Static Methods
        /// <summary>
        /// Finds a transaction by its ID.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to find.</param>
        /// <returns>A Transaction instance if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown when transactionId is invalid.</exception>
        public static Transaction Find(int transactionId)
        {
            var dt = TransactionsAccess.GetTransaction(transactionId);
            if (dt?.Rows.Count > 0)
            {
                return new Transaction(
                    Convert.ToInt32(dt.Rows[0]["TransactionID"]),
                    dt.Rows[0]["Notes"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Amount"]),
                    Convert.ToByte(dt.Rows[0]["PaymentMethod"]),
                    Convert.ToByte(dt.Rows[0]["Reason"]),
                    dt.Rows[0]["Sender"].ToString(),
                    dt.Rows[0]["Receiver"].ToString(),
                    Convert.ToBoolean(dt.Rows[0]["Type"])
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously finds a transaction by its ID.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to find.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A Transaction instance if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown when transactionId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<Transaction> FindAsync(
            int transactionId,
            CancellationToken cancellationToken = default)
        {
            var dt = await TransactionsAccess.GetTransactionAsync(transactionId, cancellationToken);
            if (dt?.Rows.Count > 0)
            {
                return new Transaction(
                    Convert.ToInt32(dt.Rows[0]["TransactionID"]),
                    dt.Rows[0]["Notes"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Amount"]),
                    Convert.ToByte(dt.Rows[0]["PaymentMethod"]),
                    Convert.ToByte(dt.Rows[0]["Reason"]),
                    dt.Rows[0]["Sender"].ToString(),
                    dt.Rows[0]["Receiver"].ToString(),
                    Convert.ToBoolean(dt.Rows[0]["Type"])
                );
            }
            return null;
        }

        /// <summary>
        /// Gets all transactions from the database.
        /// </summary>
        /// <returns>A DataTable containing all transactions.</returns>
        public static DataTable GetTable()
        {
            if (!_cache.TryGetValue(CACHE_KEY, out DataTable cachedData))
            {
                cachedData = TransactionsAccess.GetTransactions();
                _cache.Set(CACHE_KEY, cachedData, _cacheOptions);
            }
            return cachedData;
        }

        /// <summary>
        /// Asynchronously gets all transactions from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all transactions.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetTableAsync(
            CancellationToken cancellationToken = default)
        {
            if (!_cache.TryGetValue(CACHE_KEY, out DataTable cachedData))
            {
                cachedData = await TransactionsAccess.GetTransactionsAsync(cancellationToken);
                _cache.Set(CACHE_KEY, cachedData, _cacheOptions);
            }
            return cachedData;
        }
        /// <summary>
        /// Gets all unpaid transactions from the database.
        /// </summary>
        /// <returns>A DataTable containing all unpaid transactions.</returns>
        /// <remarks>
        /// Retrieves transactions that are marked as requiring return.
        /// </remarks>
        public static DataTable GetUnpaidTransactions()
        {
            return TransactionsAccess.GetAllUnpaidTransactions();
        }

        /// <summary>
        /// Asynchronously gets all unpaid transactions from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all unpaid transactions.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetUnpaidTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            return await TransactionsAccess.GetAllUnpaidTransactionsAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all received transactions from the database.
        /// </summary>
        /// <returns>A DataTable containing all received transactions.</returns>
        /// <remarks>
        /// Retrieves transactions where Type is true (received).
        /// </remarks>
        public static DataTable GetReceivedTransactions()
        {
            return TransactionsAccess.GetReceivedTransactions();
        }

        /// <summary>
        /// Asynchronously gets all received transactions from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all received transactions.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetReceivedTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            return await TransactionsAccess.GetReceivedTransactionsAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all sent transactions from the database.
        /// </summary>
        /// <returns>A DataTable containing all sent transactions.</returns>
        /// <remarks>
        /// Retrieves transactions where Type is false (sent).
        /// </remarks>
        public static DataTable GetSentTransactions()
        {
            return TransactionsAccess.GetSentTransactions();
        }

        /// <summary>
        /// Asynchronously gets all sent transactions from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all sent transactions.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetSentTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            return await TransactionsAccess.GetSentTransactionsAsync(cancellationToken);
        }
        #endregion
        #endregion
    }
}
