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
    /// Represents a phone number entity in the business layer, providing CRUD operations and validation.
    /// </summary>
    /// <remarks>
    /// This class implements the business logic for phone numbers with the following features:
    /// - Create and Update operations (Delete not supported as per business rules)
    /// - Data validation and business rules enforcement
    /// - Property change notification
    /// - Error information interface
    /// - Asynchronous operation support
    /// - Integration with Customer entity
    /// 
    /// Business Rules:
    /// - Phone numbers cannot be deleted
    /// - Each phone number must be associated with a customer
    /// - Phone numbers must be unique per customer
    /// - Phone numbers must follow the format: +?[1-9]\d{1,14}
    /// 
    /// Usage Example:
    /// <code>
    /// // Create a new phone number
    /// var phone = new Phone 
    /// {
    ///     PhoneNumber = "+1234567890",
    ///     CustomerID = 1
    /// };
    /// 
    /// // Save to database
    /// bool success = await phone.SaveAsync();
    /// </code>
    /// </remarks>
    /// <seealso cref="INotifyPropertyChanged"/>
    /// <seealso cref="IDataErrorInfo"/>
    public class Phone : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Constants
        private const string PHONE_NUMBER_PATTERN = @"^\+?[1-9]\d{1,14}$";
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _phoneId;
        private string _phoneNumber;
        private int _customerId;
        private Customer _customer;
        #endregion

        #region Properties
        [Required(ErrorMessage = "Phone ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Phone ID must be positive")]
        public int PhoneID
        {
            get => _phoneId;
            private set
            {
                if (_phoneId != value)
                {
                    _phoneId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(PHONE_NUMBER_PATTERN,
            ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber != value)
                {
                    _phoneNumber = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be positive")]
        public int CustomerID
        {
            get => _customerId;
            set
            {
                if (_customerId != value)
                {
                    _customerId = value;
                    _customer = null; // Reset cached customer
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public Customer Customer
        {
            get
            {
                if (_customer == null && CustomerID > 0)
                {
                    _customer = Customer.Find(CustomerID);
                }
                return _customer;
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
        private Phone(int phoneId, string phoneNumber, int customerId)
        {
            PhoneID = phoneId;
            PhoneNumber = phoneNumber;
            CustomerID = customerId;
            _Mode = enMode.Update;
        }

        public Phone()
        {
            PhoneID = -1;
            PhoneNumber = string.Empty;
            CustomerID = -1;
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
            ValidateProperty(PhoneNumber);
            return PhonesAccess.UpdatePhone(PhoneNumber, PhoneID);
        }

        private async Task<bool> _UpdateAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            return await PhonesAccess.UpdatePhoneAsync(PhoneNumber, PhoneID, cancellationToken);
        }

        private bool _AddNew()
        {
            ValidateProperty(PhoneNumber);
            ValidateProperty(CustomerID);
            PhoneID = PhonesAccess.AddPhone(PhoneNumber, CustomerID);
            return PhoneID != -1;
        }

        private async Task<bool> _AddNewAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);
            PhoneID = await PhonesAccess.AddPhoneAsync(
                PhoneNumber, CustomerID, cancellationToken);
            return PhoneID != -1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the current phone number to the database.
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
        /// Asynchronously saves the current phone number to the database.
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
        /// Gets all phone numbers associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A DataTable containing the phone numbers for the specified customer.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        public static DataTable GetPhonesOf(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return PhonesAccess.GetPhonesOf(customerId);
        }

        /// <summary>
        /// Asynchronously gets all phone numbers associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the phone numbers for the specified customer.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetPhonesOfAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return await PhonesAccess.GetPhonesOfAsync(customerId, cancellationToken);
        }

        /// <summary>
        /// Creates a list of Phone objects from a DataTable.
        /// </summary>
        /// <param name="dataTable">The DataTable containing phone data.</param>
        /// <returns>A list of Phone objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dataTable is null.</exception>
        public static List<Phone> CreateFromDataTable(DataTable dataTable)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            var phones = new List<Phone>();
            foreach (DataRow row in dataTable.Rows)
            {
                phones.Add(new Phone(
                    Convert.ToInt32(row["PhoneID"]),
                    row["PhoneNumber"].ToString(),
                    Convert.ToInt32(row["CustomerID"])
                ));
            }
            return phones;
        }

        /// <summary>
        /// Asynchronously creates a list of Phone objects from a DataTable.
        /// </summary>
        /// <param name="dataTable">The DataTable containing phone data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A list of Phone objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dataTable is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<List<Phone>> CreateFromDataTableAsync(
            DataTable dataTable,
            CancellationToken cancellationToken = default)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            return await Task.Run(() =>
            {
                var phones = new List<Phone>();
                foreach (DataRow row in dataTable.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    phones.Add(new Phone(
                        Convert.ToInt32(row["PhoneID"]),
                        row["PhoneNumber"].ToString(),
                        Convert.ToInt32(row["CustomerID"])
                    ));
                }
                return phones;
            }, cancellationToken);
        }
        #endregion
    }
}
