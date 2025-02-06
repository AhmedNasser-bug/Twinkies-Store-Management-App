using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TwinkiesStoreDataAccessLayer;

namespace TwinkiesStoreBusinessLayer
{
    // <summary>
    /// Represents a customer entity and provides methods for customer-related operations.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for:
    /// - Creating and updating customer records
    /// - Finding customers by ID
    /// - Retrieving all customers
    /// - Managing customer data with validation
    /// - Both synchronous and asynchronous operations
    /// </remarks>
    public class Customer : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Events
        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _customerId;
        private string _name;
        private string _address;
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private const string CACHE_KEY = "AllCustomers";
        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        /// <value>
        /// The customer ID. Must be greater than 0 when the customer exists in the database.
        /// </value>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Customer ID")]
        public int CustomerID
        {
            get => _customerId;
            private set
            {
                if (_customerId != value)
                {
                    _customerId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        /// <value>
        /// The customer's name. Must be between 2 and 100 characters and contain only valid characters.
        /// </value>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Name contains invalid characters")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the address of the customer.
        /// </summary>
        /// <value>
        /// The customer's address. Must be between 5 and 200 characters.
        /// </value>
        [Required(ErrorMessage = "Address is required")]
        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public string Error => null;

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <returns>The error message for the property. Empty string if no error.</returns>
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
        /// Private constructor to create a Customer instance with specific values.
        /// </summary>
        /// <param name="customerId">The customer's ID.</param>
        /// <param name="name">The customer's name.</param>
        /// <param name="address">The customer's address.</param>
        private Customer(int customerId, string name, string address)
        {
            CustomerID = customerId;
            Name = name;
            Address = address;
            _Mode = enMode.Update;
        }

        /// <summary>
        /// Initializes a new instance of the Customer class with default values.
        /// </summary>
        /// <remarks>
        /// Creates a new customer instance in AddNew mode with default property values.
        /// </remarks>
        public Customer()
        {
            CustomerID = -1;
            Name = string.Empty;
            Address = string.Empty;
            _Mode = enMode.AddNew;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Notifies that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Validates a property value using data annotations.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="propertyName">The name of the property being validated.</param>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
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

        /// <summary>
        /// Updates the current customer in the database.
        /// </summary>
        /// <returns>True if update was successful; otherwise, false.</returns>
        private bool _Update()
        {
            return CustomersAccess.UpdateCustomerDetails(CustomerID, Name, Address);
        }

        /// <summary>
        /// Adds the current customer to the database.
        /// </summary>
        /// <returns>True if addition was successful; otherwise, false.</returns>
        private bool _AddNew()
        {
            CustomerID = CustomersAccess.AddCustomer(Name, Address);
            return CustomerID != -1;
        }

        /// <summary>
        /// Validates all properties of the current instance.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        private async Task ValidateAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var validationContext = new ValidationContext(this);
                Validator.ValidateObject(this, validationContext, true);
            }, cancellationToken);
        }

        /// <summary>
        /// Asynchronously adds the current customer to the database.
        /// </summary>
        private async Task<bool> AddNewAsync(CancellationToken cancellationToken)
        {
            CustomerID = await CustomersAccess.AddCustomerAsync(Name, Address, cancellationToken);
            return CustomerID != -1;
        }

        /// <summary>
        /// Asynchronously updates the current customer in the database.
        /// </summary>
        private async Task<bool> UpdateAsync(CancellationToken cancellationToken)
        {
            return await CustomersAccess.UpdateCustomerDetailsAsync(
                CustomerID, Name, Address, cancellationToken);
        }
        #endregion

        #region Synchronous Methods
        /// <summary>
        /// Saves the current customer instance to the database.
        /// </summary>
        /// <returns>
        /// True if the save operation was successful; otherwise, false.
        /// </returns>
        /// <exception cref="ValidationException">
        /// Thrown when the customer data fails validation.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
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
        /// Finds a customer by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to find.</param>
        /// <returns>
        /// A Customer instance if found; otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when customerId is less than or equal to zero.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static Customer Find(int customerId)
        {
            DataTable customerData = new DataTable();
            if (CustomersAccess.FindCustomerWithID(customerId, ref customerData) &&
                customerData.Rows.Count > 0)
            {
                return new Customer(
                    Convert.ToInt32(customerData.Rows[0]["CustomerID"]),
                    customerData.Rows[0]["Name"].ToString(),
                    customerData.Rows[0]["Address"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// </summary>
        /// <returns>
        /// A DataTable containing all customer records.
        /// </returns>
        /// <remarks>
        /// This method implements caching with a 5-minute sliding expiration.
        /// </remarks>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static DataTable GetTable()
        {
            if (!_cache.TryGetValue(CACHE_KEY, out DataTable cachedData))
            {
                cachedData = CustomersAccess.GetAllCustomers();
                _cache.Set(CACHE_KEY, cachedData, _cacheOptions);
            }
            return cachedData;
        }

        /// <summary>
        /// Retrieves all customers who have ordered a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>
        /// A DataTable containing the customers who ordered the specified product.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when productId is less than or equal to zero.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static DataTable GetCustomersOfProduct(int productId)
        {
            return CustomersAccess.GetCustomersOfProduct(productId);
        }
        #endregion

        #region Asynchronous Methods
        /// <summary>
        /// Asynchronously saves the current customer instance to the database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// true if the save operation was successful; otherwise, false.
        /// </returns>
        /// <exception cref="ValidationException">
        /// Thrown when the customer data fails validation.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            await ValidateAsync(cancellationToken);

            bool result = _Mode switch
            {
                enMode.AddNew => await AddNewAsync(cancellationToken),
                enMode.Update => await UpdateAsync(cancellationToken),
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
        /// Asynchronously finds a customer by their ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer to find.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a Customer instance if found; otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when customerId is less than or equal to zero.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static async Task<Customer> FindAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var result = await CustomersAccess.FindCustomerWithIDAsync(customerId, cancellationToken);
            if (result.found && result.customerData.Rows.Count > 0)
            {
                return new Customer(
                    Convert.ToInt32(result.customerData.Rows[0]["CustomerID"]),
                    result.customerData.Rows[0]["Name"].ToString(),
                    result.customerData.Rows[0]["Address"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously retrieves all customers from the database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with all customer records.
        /// </returns>
        /// <remarks>
        /// This method implements caching with a 5-minute sliding expiration.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static async Task<DataTable> GetTableAsync(
            CancellationToken cancellationToken = default)
        {
            if (!_cache.TryGetValue(CACHE_KEY, out DataTable cachedData))
            {
                cachedData = await CustomersAccess.GetAllCustomersAsync(cancellationToken);
                _cache.Set(CACHE_KEY, cachedData, _cacheOptions);
            }
            return cachedData;
        }

        /// <summary>
        /// Asynchronously retrieves all customers who have ordered a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a DataTable with customer records who ordered the specified product.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when productId is less than or equal to zero.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled.
        /// </exception>
        /// <exception cref="SqlException">
        /// Thrown when there is an error executing the database operation.
        /// </exception>
        public static async Task<DataTable> GetCustomersOfProductAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            return await CustomersAccess.GetCustomersOfProductAsync(productId, cancellationToken);
        }
        #endregion
    }
}