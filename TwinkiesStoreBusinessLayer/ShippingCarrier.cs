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
    /// Represents a shipping carrier entity in the business layer, providing CRUD operations and validation.
    /// </summary>
    /// <remarks>
    /// This class implements the business logic for shipping carriers with the following features:
    /// - Full CRUD operations (Create, Read, Update, Delete)
    /// - Data validation and business rules enforcement
    /// - Property change notification
    /// - Error information interface
    /// - Asynchronous operation support
    /// - Caching mechanism for frequently accessed data
    /// 
    /// Business Rules:
    /// - Shipping carrier names must be unique
    /// - Order time must be positive (greater than 0)
    /// - Shipping cost must be non-negative (greater than or equal to 0)
    /// 
    /// Usage Example:
    /// <code>
    /// // Create a new shipping carrier
    /// var carrier = new ShippingCarrier 
    /// {
    ///     ShippingCarrierName = "Express Delivery",
    ///     OrderTime = 60,
    ///     ShippingCost = 25.99m
    /// };
    /// 
    /// // Save to database
    /// bool success = await carrier.SaveAsync();
    /// </code>
    /// </remarks>
    /// <seealso cref="INotifyPropertyChanged"/>
    /// <seealso cref="IDataErrorInfo"/>
    /// <exception cref="ValidationException">
    /// Thrown when validation fails during property assignment or save operations.
    /// </exception>
    /// <exception cref="SqlException">
    /// Thrown when database operations fail due to SQL-related issues.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when operations are attempted in an invalid state.
    /// </exception>
    public class ShippingCarrier
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _shippingCarrierId;
        private string _shippingCarrierName;
        private int _orderTime;
        private decimal _shippingCost;
        private static readonly object _cacheLock = new object();
        private static DataTable _cachedTable;
        private static DateTime _cacheExpiration;
        private const int CACHE_DURATION_MINUTES = 15;
        #endregion

        #region Properties
        [Required(ErrorMessage = "Shipping Carrier ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Shipping Carrier ID must be positive")]
        public int ShippingCarrierID
        {
            get => _shippingCarrierId;
            private set
            {
                if (_shippingCarrierId != value)
                {
                    _shippingCarrierId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Shipping Carrier Name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Shipping Carrier Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$",
            ErrorMessage = "Shipping Carrier Name contains invalid characters")]
        public string ShippingCarrierName
        {
            get => _shippingCarrierName;
            set
            {
                if (_shippingCarrierName != value)
                {
                    _shippingCarrierName = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Order Time is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Order Time must be positive")]
        public int OrderTime
        {
            get => _orderTime;
            set
            {
                if (_orderTime != value)
                {
                    _orderTime = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Shipping Cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Shipping Cost cannot be negative")]
        public decimal ShippingCost
        {
            get => _shippingCost;
            set
            {
                if (_shippingCost != value)
                {
                    _shippingCost = value;
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
        private ShippingCarrier(int shippingCarrierId, string shippingCarrierName,
            int orderTime, decimal shippingCost)
        {
            ShippingCarrierID = shippingCarrierId;
            ShippingCarrierName = shippingCarrierName;
            OrderTime = orderTime;
            ShippingCost = shippingCost;
            _Mode = enMode.Update;
        }

        public ShippingCarrier()
        {
            ShippingCarrierID = -1;
            ShippingCarrierName = string.Empty;
            OrderTime = 0;
            ShippingCost = 0;
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
            ValidateProperty(ShippingCarrierName);
            ValidateProperty(OrderTime);
            ValidateProperty(ShippingCost);
            return ShippingCarriersAccess.UpdateShippingCarrier(
                ShippingCarrierID, ShippingCarrierName, OrderTime, ShippingCost);
        }

        private async Task<bool> _UpdateAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            return await ShippingCarriersAccess.UpdateShippingCarrierAsync(
                ShippingCarrierID, ShippingCarrierName, OrderTime, ShippingCost, cancellationToken);
        }

        private bool _AddNew()
        {
            ValidateProperty(ShippingCarrierName);
            ValidateProperty(OrderTime);
            ValidateProperty(ShippingCost);
            ShippingCarrierID = ShippingCarriersAccess.AddShippingCarrier(
                ShippingCarrierName, OrderTime, ShippingCost);
            return ShippingCarrierID != -1;
        }

        private async Task<bool> _AddNewAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            ShippingCarrierID = await ShippingCarriersAccess.AddShippingCarrierAsync(
                ShippingCarrierName, OrderTime, ShippingCost, cancellationToken);
            return ShippingCarrierID != -1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the current shipping carrier to the database.
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
        /// Asynchronously saves the current shipping carrier to the database.
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
        /// Deletes the current shipping carrier from the database.
        /// </summary>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public bool Delete()
        {
            return ShippingCarriersAccess.DeleteShippingCarrier(ShippingCarrierID);
        }

        /// <summary>
        /// Asynchronously deletes the current shipping carrier from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
        {
            return await ShippingCarriersAccess.DeleteShippingCarrierAsync(
                ShippingCarrierID, cancellationToken);
        }

        /// <summary>
        /// Finds a shipping carrier by its ID.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to find.</param>
        /// <returns>A ShippingCarrier instance if found, null otherwise.</returns>
        public static ShippingCarrier Find(int shippingCarrierId)
        {
            DataTable dt = ShippingCarriersAccess.GetShippingCarrierDetails(shippingCarrierId);

            if (dt?.Rows.Count > 0)
            {
                return new ShippingCarrier(
                    Convert.ToInt32(dt.Rows[0]["ShippingCarrierID"]),
                    dt.Rows[0]["ShippingCarrierName"].ToString(),
                    Convert.ToInt32(dt.Rows[0]["OrderTime"]),
                    Convert.ToDecimal(dt.Rows[0]["ShippingCost"])
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously finds a shipping carrier by its ID.
        /// </summary>
        /// <param name="shippingCarrierId">The ID of the shipping carrier to find.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A ShippingCarrier instance if found, null otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<ShippingCarrier> FindAsync(
            int shippingCarrierId,
            CancellationToken cancellationToken = default)
        {
            var dt = await ShippingCarriersAccess.GetShippingCarrierDetailsAsync(
                shippingCarrierId, cancellationToken);

            if (dt?.Rows.Count > 0)
            {
                return new ShippingCarrier(
                    Convert.ToInt32(dt.Rows[0]["ShippingCarrierID"]),
                    dt.Rows[0]["ShippingCarrierName"].ToString(),
                    Convert.ToInt32(dt.Rows[0]["OrderTime"]),
                    Convert.ToDecimal(dt.Rows[0]["ShippingCost"])
                );
            }
            return null;
        }

        /// <summary>
        /// Gets all shipping carriers from the database with caching support.
        /// </summary>
        /// <returns>A DataTable containing all shipping carriers.</returns>
        public static DataTable GetTable()
        {
            lock (_cacheLock)
            {
                if (_cachedTable == null || DateTime.Now >= _cacheExpiration)
                {
                    _cachedTable = ShippingCarriersAccess.GetAllShippingCarriers();
                    _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                }
                return _cachedTable.Copy();
            }
        }

        /// <summary>
        /// Asynchronously gets all shipping carriers from the database with caching support.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all shipping carriers.</returns>
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
                        _cachedTable = ShippingCarriersAccess.GetAllShippingCarriers();
                        _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                    }
                    return _cachedTable.Copy();
                }
            }, cancellationToken);
        }
        #endregion
    }
}
