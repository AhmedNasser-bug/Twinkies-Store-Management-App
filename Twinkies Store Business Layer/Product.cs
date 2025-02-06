using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using TwinkiesStoreBusinessLayer;
using TwinkiesStoreDataAccessLayer;

namespace TwinkiesStoreBusinessLayer
{
    /// <summary>
    /// Represents a product entity in the business layer.
    /// </summary>
    public class Product : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Enums
        private enum enMode { AddNew, Update }
        #endregion

        #region Private Fields
        private enMode _Mode;
        private int _productId;
        private string _name;
        private string _description;
        private decimal _price;
        private int _quantity;
        private bool _isAvailable;
        private int _websiteId;
        private string _storeAddress;
        #endregion

        #region Properties
        public int ProductID
        {
            get => _productId;
            private set
            {
                if (_productId != value)
                {
                    _productId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Product name must be between 3 and 100 characters")]
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

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int WebsiteID
        {
            get => _websiteId;
            set
            {
                if (_websiteId != value)
                {
                    _websiteId = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        public Website AssociatedWebsite
        {
            get => Website.Find(WebsiteID);
        }

        [StringLength(200, ErrorMessage = "Store address cannot exceed 200 characters")]
        public string StoreAddress
        {
            get => _storeAddress;
            set
            {
                if (_storeAddress != value)
                {
                    _storeAddress = value;
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
        private Product(int productId, string name, string description, decimal price,
                       int quantity, bool isAvailable, int websiteId, string storeAddress)
        {
            ProductID = productId;
            Name = name;
            Description = description;
            Price = price;
            Quantity = quantity;
            IsAvailable = isAvailable;
            WebsiteID = websiteId;
            StoreAddress = storeAddress;
            _Mode = enMode.Update;
        }

        public Product()
        {
            ProductID = -1;
            Name = string.Empty;
            Description = string.Empty;
            Price = 0;
            Quantity = 0;
            IsAvailable = false;
            WebsiteID = -1;
            StoreAddress = string.Empty;
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

        private bool _Update()
        {
            return ProductsAccess.EditProduct(ProductID, Name, Description, Price,
                                            Quantity, IsAvailable, WebsiteID, StoreAddress);
        }

        private bool _AddNew()
        {
            ProductID = ProductsAccess.AddProduct(Name, Description, Price,
                                                Quantity, IsAvailable, WebsiteID, StoreAddress);
            return ProductID != -1;
        }
        #endregion

        #region Private Async Methods
        private async Task ValidatePropertiesAsync()
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
            });
        }

        private async Task<bool> _UpdateAsync()
        {
            await ValidatePropertiesAsync();
            return await Task.Run(() => _Update());
        }

        private async Task<bool> _AddNewAsync()
        {
            await ValidatePropertiesAsync();
            return await Task.Run(() => _AddNew());
        }
        #endregion

        #region Public Synchronous Methods
        /// <summary>
        /// Saves the current product to the database.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
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
        /// Deletes the current product from the database.
        /// </summary>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public bool Delete()
        {
            return ProductsAccess.DeleteProductByID(ProductID);
        }

        /// <summary>
        /// Changes the quantity of the product.
        /// </summary>
        /// <param name="newQuantity">The new quantity to set.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ChangeQuantity(int newQuantity)
        {
            if (ProductsAccess.ChangeProductQuantity(ProductID, newQuantity))
            {
                Quantity = newQuantity;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Decreases the product quantity.
        /// </summary>
        /// <param name="amount">Amount to decrease.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool DecreaseQuantity(int amount)
        {
            if (ProductsAccess.DecreaseProductQuantity(ProductID, amount))
            {
                Quantity -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Increases the product quantity.
        /// </summary>
        /// <param name="amount">Amount to increase.</param>
        public void IncreaseQuantity(int amount)
        {
            ProductsAccess.IncreaseProductQuantity(ProductID, amount);
            Quantity += amount;
        }

        /// <summary>
        /// Makes the product available.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool MakeAvailable()
        {
            if (ProductsAccess.MakeProductAvailable(ProductID))
            {
                IsAvailable = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Makes the product unavailable.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool MakeUnavailable()
        {
            if (ProductsAccess.MakeProductUnavailable(ProductID))
            {
                IsAvailable = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes the product status to delivered.
        /// </summary>
        /// <param name="orderId">The associated order ID.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ChangeToDelivered(int orderId)
        {
            return ProductsAccess.ChangeProductToDelivered(ProductID, orderId);
        }
        #endregion

        #region Public Asynchronous Methods
        /// <summary>
        /// Asynchronously saves the current product to the database.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public async Task<bool> SaveAsync()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewAsync())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return await _UpdateAsync();

                default:
                    return false;
            }
        }

        /// <summary>
        /// Asynchronously deletes the current product.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<bool> DeleteAsync()
        {
            return await Task.Run(() => Delete());
        }

        /// <summary>
        /// Asynchronously changes the product quantity.
        /// </summary>
        /// <param name="newQuantity">The new quantity.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<bool> ChangeQuantityAsync(int newQuantity)
        {
            return await Task.Run(() => ChangeQuantity(newQuantity));
        }
        #endregion

        #region Public Static Synchronous Methods
        /// <summary>
        /// Finds a product by its ID.
        /// </summary>
        /// <param name="productId">The ID to search for.</param>
        /// <returns>Product if found, null otherwise.</returns>
        public static Product Find(int productId)
        {
            DataTable dt = ProductsAccess.GetProductByID(productId);

            if (dt?.Rows.Count > 0)
            {
                return new Product(
                    Convert.ToInt32(dt.Rows[0]["ProductID"]),
                    dt.Rows[0]["Name"].ToString(),
                    dt.Rows[0]["Description"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Price"]),
                    Convert.ToInt32(dt.Rows[0]["Quantity"]),
                    Convert.ToBoolean(dt.Rows[0]["IsAvailable"]),
                    Convert.ToInt32(dt.Rows[0]["WebsiteID"]),
                    dt.Rows[0]["StoreAddress"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Finds products by their names.
        /// </summary>
        /// <param name="names">List of names to search for.</param>
        /// <returns>List of found products.</returns>
        public static List<Product> FindByNames(List<string> names)
        {
            var products = new List<Product>();
            foreach (var name in names)
            {
                DataTable dt = ProductsAccess.GetProductByName(name);
                if (dt?.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        products.Add(new Product(
                            Convert.ToInt32(row["ProductID"]),
                            row["Name"].ToString(),
                            row["Description"].ToString(),
                            Convert.ToDecimal(row["Price"]),
                            Convert.ToInt32(row["Quantity"]),
                            Convert.ToBoolean(row["IsAvailable"]),
                            Convert.ToInt32(row["WebsiteID"]),
                            row["StoreAddress"].ToString()
                        ));
                    }
                }
            }
            return products;
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>DataTable of all products.</returns>
        public static DataTable GetTable()
        {
            return ProductsAccess.GetAllProducts();
        }

        /// <summary>
        /// Deletes a product by its name.
        /// </summary>
        /// <param name="productName">Name of product to delete.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool DeleteByName(string productName)
        {
            return ProductsAccess.DeleteProductByName(productName);
        }

        /// <summary>
        /// Deletes all products with zero quantity.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool DeleteZeroQuantityProducts()
        {
            return ProductsAccess.DeleteZeroQuantityProducts();
        }

        /// <summary>
        /// Gets all available products.
        /// </summary>
        /// <returns>DataTable of available products.</returns>
        public static DataTable GetAvailableProducts()
        {
            return ProductsAccess.GetAvailableProducts();
        }

        /// <summary>
        /// Gets products of a specific shipping.
        /// </summary>
        /// <param name="shippingId">Shipping ID to search for.</param>
        /// <returns>DataTable of shipping products.</returns>
        public static DataTable GetProductsOfShipping(int shippingId)
        {
            return ProductsAccess.GetProductsOfShipping(shippingId);
        }

        /// <summary>
        /// Gets all requested products.
        /// </summary>
        /// <returns>DataTable of requested products.</returns>
        public static DataTable GetRequestedProducts()
        {
            return ProductsAccess.GetRequestedProducts();
        }
        #endregion

        #region Public Static Asynchronous Methods
        /// <summary>
        /// Asynchronously finds a product by ID.
        /// </summary>
        /// <param name="productId">ID to search for.</param>
        /// <returns>Product if found, null otherwise.</returns>
        public static async Task<Product?> FindAsync(int productId)
        {
            var dt = await Task.Run(() => ProductsAccess.GetProductByID(productId));

            if (dt?.Rows.Count > 0)
            {
                return new Product(
                    Convert.ToInt32(dt.Rows[0]["ProductID"]),
                    dt.Rows[0]["Name"].ToString(),
                    dt.Rows[0]["Description"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Price"]),
                    Convert.ToInt32(dt.Rows[0]["Quantity"]),
                    Convert.ToBoolean(dt.Rows[0]["IsAvailable"]),
                    Convert.ToInt32(dt.Rows[0]["WebsiteID"]),
                    dt.Rows[0]["StoreAddress"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously gets all products.
        /// </summary>
        /// <returns>DataTable of all products.</returns>
        public static async Task<DataTable> GetTableAsync()
        {
            return await Task.Run(() => ProductsAccess.GetAllProducts());
        }
        #endregion
    }
}