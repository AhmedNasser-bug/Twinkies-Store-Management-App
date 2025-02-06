using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using TwinkiesStoreDataAccessLayer;

namespace TwinkiesStoreBusinessLayer
{
    /// <summary>
    /// Represents a shipping entity in the business layer, providing CRUD operations and validation.
    /// </summary>
    /// <remarks>
    /// This class implements the business logic for shipping with the following features:
    /// - Create and Update operations (Delete not supported as per business rules)
    /// - Shipping status management
    /// - Carrier relationship handling
    /// - Environment type tracking
    /// - Data validation and business rules enforcement
    /// - Property change notification
    /// - Error information interface
    /// - Asynchronous operation support
    /// 
    /// Business Rules:
    /// - Shipping records cannot be deleted
    /// - Shipping can be cancelled
    /// - Shipping can be marked as arrived
    /// - Shipping environment must be tracked
    /// 
    /// Usage Example:
    /// <code>
    /// // Create a new shipping
    /// var shipping = new Shipping 
    /// {
    ///     ShippingCarrierID = 1,
    ///     StartDate = DateTime.Now,
    ///     Status = "Pending",
    ///     ShippingEnv = 1
    /// };
    /// 
    /// // Save to database
    /// bool success = await shipping.SaveAsync();
    /// </code>
    /// </remarks>
    public class Shipping : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Constants
        public static class ShippingStatus
        {
            public const string Pending = "Pending";
            public const string InTransit = "InTransit";
            public const string Arrived = "Arrived";
            public const string Cancelled = "Cancelled";
        }

        public static class ShippingEnvironment
        {
            public const byte Normal = 0;
            public const byte Refrigerated = 1;
            public const byte Frozen = 2;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _shippingId;
        private int _shippingCarrierId;
        private DateTime _startDate;
        private string _status;
        private byte _shippingEnv;
        private ShippingCarrier _shippingCarrier;
        private static readonly object _cacheLock = new object();
        private static DataTable _cachedTable;
        private static DateTime _cacheExpiration;
        private const int CACHE_DURATION_MINUTES = 15;
        #endregion

        #region Properties
        [Required(ErrorMessage = "Shipping ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Shipping ID must be positive")]
        public int ShippingID
        {
            get => _shippingId;
            private set
            {
                if (_shippingId != value)
                {
                    _shippingId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Shipping Carrier ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Shipping Carrier ID must be positive")]
        public int ShippingCarrierID
        {
            get => _shippingCarrierId;
            set
            {
                if (_shippingCarrierId != value)
                {
                    _shippingCarrierId = value;
                    _shippingCarrier = null; // Reset cached carrier
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Status must be between 3 and 50 characters")]
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Shipping Environment is required")]
        [Range(0, 2, ErrorMessage = "Invalid shipping environment value")]
        public byte ShippingEnv
        {
            get => _shippingEnv;
            set
            {
                if (_shippingEnv != value)
                {
                    _shippingEnv = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public ShippingCarrier ShippingCarrier
        {
            get
            {
                if (_shippingCarrier == null && ShippingCarrierID > 0)
                {
                    _shippingCarrier = ShippingCarrier.Find(ShippingCarrierID);
                }
                return _shippingCarrier;
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
        private Shipping(int shippingId, int shippingCarrierId, DateTime startDate,
            string status, byte shippingEnv)
        {
            ShippingID = shippingId;
            ShippingCarrierID = shippingCarrierId;
            StartDate = startDate;
            Status = status;
            ShippingEnv = shippingEnv;
            _Mode = enMode.Update;
        }

        public Shipping()
        {
            ShippingID = -1;
            ShippingCarrierID = -1;
            StartDate = DateTime.Now;
            Status = ShippingStatus.Pending;
            ShippingEnv = ShippingEnvironment.Normal;
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
                cancellationToken.ThrowIfCancellationRequested();
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

        private bool _Update()
        {
            ValidateProperty(ShippingCarrierID);
            ValidateProperty(StartDate);
            ValidateProperty(Status);
            ValidateProperty(ShippingEnv);
            return ShippingAccess.UpdateShipping(ShippingID, ShippingCarrierID,
                StartDate, Status, ShippingEnv);
        }

        private async Task<bool> _UpdateAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            return await ShippingAccess.UpdateShippingAsync(ShippingID, ShippingCarrierID,
                StartDate, Status, ShippingEnv, cancellationToken);
        }

        private bool _AddNew()
        {
            ValidateProperty(ShippingCarrierID);
            ValidateProperty(StartDate);
            ValidateProperty(Status);
            ValidateProperty(ShippingEnv);
            ShippingID = ShippingAccess.AddShipping(ShippingCarrierID, StartDate,
                Status, ShippingEnv);
            return ShippingID != -1;
        }

        private async Task<bool> _AddNewAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            ShippingID = await ShippingAccess.AddShippingAsync(ShippingCarrierID,
                StartDate, Status, ShippingEnv, cancellationToken);
            return ShippingID != -1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the current shipping record to the database.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        public bool Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (_AddNew())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _Update();

                default:
                    return false;
            }
        }

        /// <summary>
        /// Asynchronously saves the current shipping record to the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewAsync(cancellationToken))
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return await _UpdateAsync(cancellationToken);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Cancels the current shipping.
        /// </summary>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        public bool Cancel()
        {
            if (Status == ShippingStatus.Arrived)
                throw new InvalidOperationException("Cannot cancel an arrived shipping.");

            if (ShippingAccess.CancelShipping(ShippingID))
            {
                Status = ShippingStatus.Cancelled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously cancels the current shipping.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> CancelAsync(CancellationToken cancellationToken = default)
        {
            if (Status == ShippingStatus.Arrived)
                throw new InvalidOperationException("Cannot cancel an arrived shipping.");

            if (await ShippingAccess.CancelShippingAsync(ShippingID, cancellationToken))
            {
                Status = ShippingStatus.Cancelled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Marks the shipping as arrived.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool MarkAsArrived()
        {
            if (Status == ShippingStatus.Cancelled)
                throw new InvalidOperationException("Cannot mark a cancelled shipping as arrived.");

            if (ShippingAccess.ShippingArrived(ShippingID))
            {
                Status = ShippingStatus.Arrived;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously marks the shipping as arrived.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> MarkAsArrivedAsync(CancellationToken cancellationToken = default)
        {
            if (Status == ShippingStatus.Cancelled)
                throw new InvalidOperationException("Cannot mark a cancelled shipping as arrived.");

            if (await ShippingAccess.ShippingArrivedAsync(ShippingID, cancellationToken))
            {
                Status = ShippingStatus.Arrived;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds a shipping by its ID.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping to find.</param>
        /// <returns>A Shipping instance if found, null otherwise.</returns>
        public static Shipping Find(int shippingId)
        {
            DataTable dt = ShippingAccess.GetShippingDetails(shippingId);

            if (dt?.Rows.Count > 0)
            {
                return new Shipping(
                    Convert.ToInt32(dt.Rows[0]["ShippingID"]),
                    Convert.ToInt32(dt.Rows[0]["ShippingCarrierID"]),
                    Convert.ToDateTime(dt.Rows[0]["StartDate"]),
                    dt.Rows[0]["Status"].ToString(),
                    Convert.ToByte(dt.Rows[0]["ShippingEnv"])
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously finds a shipping by its ID.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping to find.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A Shipping instance if found, null otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<Shipping> FindAsync(int shippingId,
            CancellationToken cancellationToken = default)
        {
            var dt = await ShippingAccess.GetShippingDetailsAsync(shippingId, cancellationToken);

            if (dt?.Rows.Count > 0)
            {
                return new Shipping(
                    Convert.ToInt32(dt.Rows[0]["ShippingID"]),
                    Convert.ToInt32(dt.Rows[0]["ShippingCarrierID"]),
                    Convert.ToDateTime(dt.Rows[0]["StartDate"]),
                    dt.Rows[0]["Status"].ToString(),
                    Convert.ToByte(dt.Rows[0]["ShippingEnv"])
                );
            }
            return null;
        }

        /// <summary>
        /// Gets all shippings from the database with caching support.
        /// </summary>
        /// <returns>A DataTable containing all shippings.</returns>
        public static DataTable GetTable()
        {
            lock (_cacheLock)
            {
                if (_cachedTable == null || DateTime.Now >= _cacheExpiration)
                {
                    _cachedTable = ShippingAccess.GetAllShippings();
                    _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                }
                return _cachedTable.Copy();
            }
        }

        /// <summary>
        /// Gets all pending shippings from the database.
        /// </summary>
        /// <returns>A DataTable containing pending shippings.</returns>
        public static DataTable GetPendingShippings()
        {
            return ShippingAccess.GetPendingShippings();
        }

        /// <summary>
        /// Asynchronously gets all shippings from the database with caching support.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all shippings.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetTableAsync(
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                lock (_cacheLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_cachedTable == null || DateTime.Now >= _cacheExpiration)
                    {
                        _cachedTable = ShippingAccess.GetAllShippings();
                        _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                    }
                    return _cachedTable.Copy();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Asynchronously gets all pending shippings from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing pending shippings.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetPendingShippingsAsync(
            CancellationToken cancellationToken = default)
        {
            return await ShippingAccess.GetPendingShippingsAsync(cancellationToken);
        }
        #endregion
    }
}
