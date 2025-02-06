using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TwinkiesStoreDataAccessLayer;

namespace TwinkiesStoreBusinessLayer
{
    /// <summary>
    /// Represents an order entity in the business layer, providing CRUD operations and validation.
    /// </summary>
    /// <remarks>
    /// This class implements the business logic for orders with the following features:
    /// - Create and Update operations (Delete not supported as per business rules)
    /// - Order status management
    /// - Order-product relationships
    /// - Customer order history
    /// - Shipping integration
    /// - Data validation and business rules enforcement
    /// 
    /// Business Rules:
    /// - Orders cannot be deleted (soft delete via status change)
    /// - Orders can be cancelled
    /// - Orders can be marked as delivered
    /// - Orders track shipping environment
    /// </remarks>
    public class Order : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Enums
        public enum OrderStatus
        {
            Cancelled = 0,
            Shipping = 1,
            Arrived = 2,
            Delivered = 3
        }

        public enum ShippingEnvironment
        {
            Land = 1,
            Sea = 2,
            Air = 3
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields
        private enum enMode { AddNew, Update }
        private enMode _Mode;
        private int _orderId;
        private int _customerId;
        private OrderStatus _status;
        private ShippingEnvironment _shippingEnv;
        private string _notes;
        private DateTime _orderTimeStamp;
        private int? _shippingId;
        private string _paymentMethod;
        private decimal _price;
        private decimal _depositAmount;
        private string _transactionNotes;
        private Customer _customer;
        private Shipping _shipping;
        private static readonly object _cacheLock = new object();
        private static DataTable _cachedTable;
        private static DateTime _cacheExpiration;
        private const int CACHE_DURATION_MINUTES = 15;
        #endregion

        #region Properties
        [Required(ErrorMessage = "Order ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be positive")]
        public int OrderID
        {
            get => _orderId;
            private set
            {
                if (_orderId != value)
                {
                    _orderId = value;
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

        [Required(ErrorMessage = "Status is required")]
        public OrderStatus Status
        {
            get => _status;
            private set // Private set as status should only be changed through specific methods
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Shipping environment is required")]
        public ShippingEnvironment ShippingEnv
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

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
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

        public DateTime OrderTimeStamp
        {
            get => _orderTimeStamp;
            private set
            {
                if (_orderTimeStamp != value)
                {
                    _orderTimeStamp = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int? ShippingID
        {
            get => _shippingId;
            set
            {
                if (_shippingId != value)
                {
                    _shippingId = value;
                    _shipping = null; // Reset cached shipping
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Payment method must be between 2 and 50 characters")]
        public string PaymentMethod
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

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
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

        [Required(ErrorMessage = "Deposit amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Deposit amount cannot be negative")]
        public decimal DepositAmount
        {
            get => _depositAmount;
            set
            {
                if (_depositAmount != value)
                {
                    _depositAmount = value;
                    NotifyPropertyChanged();
                    ValidateProperty(value);
                }
            }
        }

        [StringLength(500, ErrorMessage = "Transaction notes cannot exceed 500 characters")]
        public string TransactionNotes
        {
            get => _transactionNotes;
            set
            {
                if (_transactionNotes != value)
                {
                    _transactionNotes = value;
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

        public Shipping Shipping
        {
            get
            {
                if (_shipping == null && ShippingID.HasValue)
                {
                    _shipping = Shipping.Find(ShippingID.Value);
                }
                return _shipping;
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
        private Order(int orderId, int customerId, OrderStatus status,
            ShippingEnvironment shippingEnv, string notes, DateTime orderTimeStamp,
            int? shippingId, string paymentMethod, decimal price, decimal depositAmount,
            string transactionNotes)
        {
            OrderID = orderId;
            CustomerID = customerId;
            Status = status;
            ShippingEnv = shippingEnv;
            Notes = notes;
            OrderTimeStamp = orderTimeStamp;
            ShippingID = shippingId;
            PaymentMethod = paymentMethod;
            Price = price;
            DepositAmount = depositAmount;
            TransactionNotes = transactionNotes;
            _Mode = enMode.Update;
        }

        public Order()
        {
            OrderID = -1;
            CustomerID = -1;
            Status = OrderStatus.Shipping;
            ShippingEnv = ShippingEnvironment.Land;
            Notes = string.Empty;
            OrderTimeStamp = DateTime.Now;
            ShippingID = null;
            PaymentMethod = string.Empty;
            Price = 0;
            DepositAmount = 0;
            TransactionNotes = string.Empty;
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

        private bool ValidateStatusTransition(OrderStatus newStatus)
        {
            switch (Status)
            {
                case OrderStatus.Cancelled:
                    return false; // Cannot transition from cancelled state
                case OrderStatus.Shipping:
                    return newStatus == OrderStatus.Arrived || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Arrived:
                    return newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Delivered:
                    return false; // Cannot transition from delivered state
                default:
                    return false;
            }
        }

        private bool _AddNew()
        {
            ValidateProperty(CustomerID);
            ValidateProperty(ShippingEnv);
            ValidateProperty(PaymentMethod);
            ValidateProperty(Price);
            ValidateProperty(DepositAmount);

            OrderID = OrdersAccess.AddOrder(
                ShippingID,
                (byte)ShippingEnv,
                CustomerID,
                PaymentMethod,
                Price,
                DepositAmount,
                TransactionNotes ?? string.Empty,
                Notes ?? string.Empty);

            return OrderID != -1;
        }

        private async Task<bool> _AddNewAsync(CancellationToken cancellationToken = default)
        {
            await ValidatePropertiesAsync(cancellationToken);

            OrderID = await OrdersAccess.AddOrderAsync(
                ShippingID,
                (byte)ShippingEnv,
                CustomerID,
                PaymentMethod,
                Price,
                DepositAmount,
                TransactionNotes ?? string.Empty,
                Notes ?? string.Empty,
                cancellationToken);

            return OrderID != -1;
        }

        private bool _Update()
        {
            throw new NotSupportedException("Direct update of orders is not supported. " +
                "Use specific status change methods instead.");
        }

        private async Task<bool> _UpdateAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Direct update of orders is not supported. " +
                "Use specific status change methods instead.");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the current order to the database.
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
        /// Asynchronously saves the current order to the database.
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
        /// Cancels the current order.
        /// </summary>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown when order cannot be cancelled.</exception>
        public bool Cancel()
        {
            if (!ValidateStatusTransition(OrderStatus.Cancelled))
                throw new InvalidOperationException($"Cannot cancel order in {Status} status.");

            if (OrdersAccess.CancelOrder(OrderID))
            {
                Status = OrderStatus.Cancelled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously cancels the current order.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown when order cannot be cancelled.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> CancelAsync(CancellationToken cancellationToken = default)
        {
            if (!ValidateStatusTransition(OrderStatus.Cancelled))
                throw new InvalidOperationException($"Cannot cancel order in {Status} status.");

            if (await OrdersAccess.CancelOrderAsync(OrderID, cancellationToken))
            {
                Status = OrderStatus.Cancelled;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Marks the order as delivered.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown when order cannot be marked as delivered.</exception>
        public bool ChangeToDelivered()
        {
            if (!ValidateStatusTransition(OrderStatus.Delivered))
                throw new InvalidOperationException($"Cannot mark order as delivered in {Status} status.");

            if (OrdersAccess.ChangeOrderToDelivered(OrderID))
            {
                Status = OrderStatus.Delivered;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously marks the order as delivered.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown when order cannot be marked as delivered.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<bool> ChangeToDeliveredAsync(CancellationToken cancellationToken = default)
        {
            if (!ValidateStatusTransition(OrderStatus.Delivered))
                throw new InvalidOperationException($"Cannot mark order as delivered in {Status} status.");

            if (await OrdersAccess.ChangeOrderToDeliveredAsync(OrderID, cancellationToken))
            {
                Status = OrderStatus.Delivered;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes multiple orders to delivered status.
        /// </summary>
        /// <param name="orderIds">List of order IDs to mark as delivered.</param>
        /// <returns>True if all operations were successful, false if any failed.</returns>
        public static bool ChangeOrdersToDelivered(IEnumerable<int> orderIds)
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentException("Order IDs list cannot be null or empty.", nameof(orderIds));

            return orderIds.All(id => OrdersAccess.ChangeOrderToDelivered(id));
        }

        /// <summary>
        /// Asynchronously changes multiple orders to delivered status.
        /// </summary>
        /// <param name="orderIds">List of order IDs to mark as delivered.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if all operations were successful, false if any failed.</returns>
        public static async Task<bool> ChangeOrdersToDeliveredAsync(
            IEnumerable<int> orderIds,
            CancellationToken cancellationToken = default)
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentException("Order IDs list cannot be null or empty.", nameof(orderIds));

            var tasks = orderIds.Select(id =>
                OrdersAccess.ChangeOrderToDeliveredAsync(id, cancellationToken));
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        /// <summary>
        /// Finds an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to find.</param>
        /// <returns>An Order instance if found, null otherwise.</returns>
        public static Order Find(int orderId)
        {
            DataTable dt = OrdersAccess.GetOrderWithID(orderId);

            if (dt?.Rows.Count > 0)
            {
                return new Order(
                    Convert.ToInt32(dt.Rows[0]["OrderID"]),
                    Convert.ToInt32(dt.Rows[0]["CustomerID"]),
                    (OrderStatus)Convert.ToInt32(dt.Rows[0]["Status"]),
                    (ShippingEnvironment)Convert.ToByte(dt.Rows[0]["Shipping_env"]),
                    dt.Rows[0]["Notes"].ToString(),
                    Convert.ToDateTime(dt.Rows[0]["OrderTimeStamp"]),
                    dt.Rows[0]["ShippingID"] == DBNull.Value ? null :
                        (int?)Convert.ToInt32(dt.Rows[0]["ShippingID"]),
                    dt.Rows[0]["PaymentMethod"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Price"]),
                    Convert.ToDecimal(dt.Rows[0]["DepositAmount"]),
                    dt.Rows[0]["TransactionNotes"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Asynchronously finds an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to find.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An Order instance if found, null otherwise.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<Order> FindAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var dt = await OrdersAccess.GetOrderWithIDAsync(orderId, cancellationToken);

            if (dt?.Rows.Count > 0)
            {
                return new Order(
                    Convert.ToInt32(dt.Rows[0]["OrderID"]),
                    Convert.ToInt32(dt.Rows[0]["CustomerID"]),
                    (OrderStatus)Convert.ToInt32(dt.Rows[0]["Status"]),
                    (ShippingEnvironment)Convert.ToByte(dt.Rows[0]["Shipping_env"]),
                    dt.Rows[0]["Notes"].ToString(),
                    Convert.ToDateTime(dt.Rows[0]["OrderTimeStamp"]),
                    dt.Rows[0]["ShippingID"] == DBNull.Value ? null :
                        (int?)Convert.ToInt32(dt.Rows[0]["ShippingID"]),
                    dt.Rows[0]["PaymentMethod"].ToString(),
                    Convert.ToDecimal(dt.Rows[0]["Price"]),
                    Convert.ToDecimal(dt.Rows[0]["DepositAmount"]),
                    dt.Rows[0]["TransactionNotes"].ToString()
                );
            }
            return null;
        }

        /// <summary>
        /// Gets all orders from the database with caching support.
        /// </summary>
        /// <returns>A DataTable containing all orders.</returns>
        public static DataTable GetTable()
        {
            lock (_cacheLock)
            {
                if (_cachedTable == null || DateTime.Now >= _cacheExpiration)
                {
                    _cachedTable = OrdersAccess.GetAllOrders();
                    _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                }
                return _cachedTable.Copy();
            }
        }

        /// <summary>
        /// Asynchronously gets all orders from the database with caching support.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all orders.</returns>
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
                        _cachedTable = OrdersAccess.GetAllOrders();
                        _cacheExpiration = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                    }
                    return _cachedTable.Copy();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Gets all cancelled orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A DataTable containing cancelled orders.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        public static DataTable GetCancelledOrdersOfCustomer(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return OrdersAccess.GetCancelledOrdersOfCustomer(customerId);
        }

        /// <summary>
        /// Asynchronously gets all cancelled orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing cancelled orders.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetCancelledOrdersOfCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return await OrdersAccess.GetCancelledOrdersOfCustomerAsync(
                customerId, cancellationToken);
        }

        /// <summary>
        /// Gets all products associated with this order.
        /// </summary>
        /// <returns>A DataTable containing the order's products.</returns>
        public DataTable GetProducts()
        {
            return OrdersAccess.GetOrderProducts(OrderID);
        }

        /// <summary>
        /// Gets all products associated with a specific order.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <returns>A DataTable containing the order's products.</returns>
        /// <exception cref="ArgumentException">Thrown when orderId is invalid.</exception>
        public static DataTable GetOrderProducts(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.",
                    nameof(orderId));

            return OrdersAccess.GetOrderProducts(orderId);
        }

        /// <summary>
        /// Asynchronously gets all products associated with this order.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the order's products.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public async Task<DataTable> GetProductsAsync(
            CancellationToken cancellationToken = default)
        {
            return await OrdersAccess.GetOrderProductsAsync(OrderID, cancellationToken);
        }

        /// <summary>
        /// Gets all orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A DataTable containing the customer's orders.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        public static DataTable GetOrdersOfCustomer(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return OrdersAccess.GetOrdersOfCustomer(customerId);
        }

        /// <summary>
        /// Asynchronously gets all orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the customer's orders.</returns>
        /// <exception cref="ArgumentException">Thrown when customerId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetOrdersOfCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.",
                    nameof(customerId));

            return await OrdersAccess.GetOrdersOfCustomerAsync(customerId, cancellationToken);
        }

        /// <summary>
        /// Gets all orders associated with a specific shipping.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping.</param>
        /// <returns>A DataTable containing the shipping's orders.</returns>
        /// <exception cref="ArgumentException">Thrown when shippingId is invalid.</exception>
        public static DataTable GetOrdersOfShipping(int shippingId)
        {
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            return OrdersAccess.GetOrdersOfShipping(shippingId);
        }

        /// <summary>
        /// Asynchronously gets all orders associated with a specific shipping.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the shipping's orders.</returns>
        /// <exception cref="ArgumentException">Thrown when shippingId is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<DataTable> GetOrdersOfShippingAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            return await OrdersAccess.GetOrdersOfShippingAsync(shippingId, cancellationToken);
        }

        /// <summary>
        /// Creates a list of Order objects from a DataTable.
        /// </summary>
        /// <param name="dataTable">The DataTable containing order data.</param>
        /// <returns>A list of Order objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dataTable is null.</exception>
        public static List<Order> CreateFromDataTable(DataTable dataTable)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            var orders = new List<Order>();
            foreach (DataRow row in dataTable.Rows)
            {
                orders.Add(new Order(
                    Convert.ToInt32(row["OrderID"]),
                    Convert.ToInt32(row["CustomerID"]),
                    (OrderStatus)Convert.ToInt32(row["Status"]),
                    (ShippingEnvironment)Convert.ToByte(row["Shipping_env"]),
                    row["Notes"].ToString(),
                    Convert.ToDateTime(row["OrderTimeStamp"]),
                    row["ShippingID"] == DBNull.Value ? null :
                        (int?)Convert.ToInt32(row["ShippingID"]),
                    row["PaymentMethod"].ToString(),
                    Convert.ToDecimal(row["Price"]),
                    Convert.ToDecimal(row["DepositAmount"]),
                    row["TransactionNotes"].ToString()
                ));
            }
            return orders;
        }

        /// <summary>
        /// Asynchronously creates a list of Order objects from a DataTable.
        /// </summary>
        /// <param name="dataTable">The DataTable containing order data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A list of Order objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dataTable is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when operation is canceled.</exception>
        public static async Task<List<Order>> CreateFromDataTableAsync(
            DataTable dataTable,
            CancellationToken cancellationToken = default)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            return await Task.Run(() =>
            {
                var orders = new List<Order>();
                foreach (DataRow row in dataTable.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    orders.Add(new Order(
                        Convert.ToInt32(row["OrderID"]),
                        Convert.ToInt32(row["CustomerID"]),
                        (OrderStatus)Convert.ToInt32(row["Status"]),
                        (ShippingEnvironment)Convert.ToByte(row["Shipping_env"]),
                        row["Notes"].ToString(),
                        Convert.ToDateTime(row["OrderTimeStamp"]),
                        row["ShippingID"] == DBNull.Value ? null :
                            (int?)Convert.ToInt32(row["ShippingID"]),
                        row["PaymentMethod"].ToString(),
                        Convert.ToDecimal(row["Price"]),
                        Convert.ToDecimal(row["DepositAmount"]),
                        row["TransactionNotes"].ToString()
                    ));
                }
                return orders;
            }, cancellationToken);
        }
        #endregion
    }

}
