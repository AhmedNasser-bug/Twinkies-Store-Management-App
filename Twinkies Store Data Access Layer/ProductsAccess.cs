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
    /// Provides data access methods for Product-related database operations.
    /// </summary>
    /// <remarks>
    /// This class handles all product-related database operations with the following features:
    /// - Complete CRUD operations
    /// - Product inventory management
    /// - Product availability control
    /// - Website integration
    /// - Store location management
    /// - Async and sync operation support
    /// 
    /// Database Table Structure:
    /// - ProductID (PK): Integer
    /// - Name: String
    /// - Description: String
    /// - Price: Decimal
    /// - Quantity: Integer
    /// - IsAvailable: Boolean
    /// - WebsiteID (FK): Integer?
    /// - StoreAddress: String?
    /// 
    /// Related Tables:
    /// - Websites (via WebsiteID)
    /// - OrderProducts (via ProductID)
    /// - ShippingProducts (via ProductID)
    /// 
    /// Business Rules:
    /// - Products can be marked as available/unavailable
    /// - Products can be associated with websites
    /// - Products can have store locations
    /// - Quantity can be managed incrementally
    /// - Zero quantity products can be bulk deleted
    /// </remarks>
    public static class ProductsAccess
    {
        // Fields: ProductID (PK), Name, Description, Price, Quantity, IsAvailable, WebsiteID (FK), StoreAddress

        #region Synchronous Methods
        /// <summary>
        /// Retrieves a specific product by its ID from the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified product.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetProductByID(int productId) // Find
        {
            string query = "EXEC SP_GetProductByID @ProductID";
            try
            {
                return ConnectionUtils.GetTable(query, productId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Adds a new product to the Products table.
        /// </summary>
        /// <param name="name">The name of the product.</param>
        /// <param name="description">The description of the product.</param>
        /// <param name="price">The price of the product.</param>
        /// <param name="quantity">The initial quantity of the product (default: 0).</param>
        /// <param name="isAvailable">The availability status of the product (default: false).</param>
        /// <param name="websiteId">The associated website ID (optional).</param>
        /// <param name="storeAddress">The store address (optional).</param>
        /// <returns>The ID of the newly added product, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
        public static int AddProduct(string name, string description, decimal price,
                                   int quantity = 0, bool isAvailable = false,
                                   int? websiteId = null, string storeAddress = null) // AddNew
        {
            string query = "EXEC SP_AddProduct @Name, @Description, @Price, @Quantity, @IsAvailable, @WebsiteID, @StoreAddress";
            try
            {
                return ConnectionUtils.AddRowToTable(query, name, description, price,
                                                  quantity, isAvailable, websiteId, storeAddress);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves products from the Products table that match the specified name.
        /// </summary>
        /// <param name="productName">The name of the product to search for.</param>
        /// <returns>A DataTable containing the details of the products that match the specified name.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetProductByName(string productName) // static Find (overload) + PLURAL
        {
            string query = "EXEC SP_GetProductByName @ProductName";
            try
            {
                return ConnectionUtils.GetTable(query, productName);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Changes the quantity of a product by executing the SP_EditProduct stored procedure.
        /// </summary>
        /// <param name="productId">The ID of the product to be updated.</param>
        /// <param name="newQuantity">The new quantity to set for the product.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool ChangeProductQuantity(int productId, int newQuantity) 
        {
            string query = "EXEC SP_ChangeProductQuantity @ProductID, @NewQuantity";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId, newQuantity);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Decreases the quantity of a product by a specified amount.
        /// </summary>
        /// <param name="productId">The ID of the product to be updated.</param>
        /// <param name="amount">The amount to decrease from the product's quantity.</param>
        /// <returns>True if the quantity was successfully decreased, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DecreaseProductQuantity(int productId, int amount) 
        {
            string query = "EXEC SP_DecreaseProductQuantity @ProductID, @Amount";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId, amount);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Increases the quantity of a specific product in the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product whose quantity is to be increased.</param>
        /// <param name="amount">The amount by which to increase the product's quantity.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static void IncreaseProductQuantity(int productId, int amount)
        {
            string query = "EXEC SP_IncreaseProductQuantity @ProductID, @Amount";
            try
            {
                ConnectionUtils.UpdateTableRow(query, productId, amount);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Changes the status of a product to delivered and updates the order status if all products are delivered.
        /// </summary>
        /// <param name="productId">The ID of the product to be updated.</param>
        /// <param name="orderId">The ID of the order associated with the product.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool ChangeProductToDelivered(int productId, int orderId) 
        {
            string query = "EXEC SP_ChangeProductToDelivered @ProductID, @OrderID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId, orderId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a product from the Products table by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to be deleted.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DeleteProductByID(int productId) // Delete
        {
            string query = "EXEC SP_DeleteProductByID @ProductID";
            try
            {
                return ConnectionUtils.DeleteTableRow(query, productId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a product from the Products table by its name.
        /// </summary>
        /// <param name="productName">The name of the product to be deleted.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DeleteProductByName(string productName) // static
        {
            string query = "EXEC SP_DeleteProductByName @ProductName";
            try
            {
                return ConnectionUtils.DeleteTableRow(query, productName);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes all products from the Products table that have a quantity of zero.
        /// </summary>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DeleteZeroQuantityProducts() // static
        {
            string query = "EXEC SP_DeleteZeroQuantityProducts";
            try
            {
                return ConnectionUtils.DeleteTableRow(query);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Edits the details of a product in the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product to be updated.</param>
        /// <param name="name">The new name of the product (optional).</param>
        /// <param name="description">The new description of the product (optional).</param>
        /// <param name="price">The new price of the product (optional).</param>
        /// <param name="quantity">The new quantity of the product (optional).</param>
        /// <param name="isAvailable">The availability status of the product (optional).</param>
        /// <param name="websiteId">The ID of the website associated with the product (optional).</param>
        /// <param name="storeAddress">The new store address for the product (optional).</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool EditProduct(int productId, string? name = null, string? description = null,
                                        decimal? price = null, int? quantity = null,
                                        bool? isAvailable = null, int? websiteId = null,
                                        string? storeAddress = null) // Update
        {
            string query = "EXEC SP_EditProduct @ProductID, @Name, @Description, @Price, @Quantity, @IsAvailable, @WebsiteID, @StoreAddress";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId, name, description, price, quantity, isAvailable, websiteId, storeAddress);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all products from the Products table.
        /// </summary>
        /// <returns>A DataTable containing the product details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllProducts() // static only
        {
            string query = "EXEC SP_GetAllProducts";
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
        /// Retrieves all available products from the Products table.
        /// </summary>
        /// <returns>A DataTable containing the available product details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAvailableProducts() // static only
        {
            string query = "EXEC SP_GetAvailableProducts";
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
        /// Retrieves all products associated with a specific shipping ID from the Products table.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping whose products are to be retrieved.</param>
        /// <returns>A DataTable containing the details of the products associated with the specified shipping ID.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetProductsOfShipping(int shippingId) // static only
        {
            string query = "EXEC SP_GetProductsOfShipping @ShippingID";
            try
            {
                return ConnectionUtils.GetTable(query, shippingId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all products that are associated with a website from the Products table.
        /// </summary>
        /// <returns>A DataTable containing the details of the requested products.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetRequestedProducts() // static only
        {
            string query = "EXEC SP_GetRequestedProducts";
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
        /// Marks a specific product as available in the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product to be marked as available.</param>
        /// <returns>True if the product was successfully marked as available; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool MakeProductAvailable(int productId)
        {
            string query = "EXEC SP_MakeProductAvailable @ProductID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Marks a specific product as unavailable in the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product to be marked as unavailable.</param>
        /// <returns>True if the product was successfully marked as unavailable; otherwise, false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool MakeProductUnavailable(int productId)
        {
            string query = "EXEC SP_MakeProductUnavailable @ProductID";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, productId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        #endregion

        #region Core Operations

        /// <summary>
        /// Asynchronously retrieves a product by its ID.
        /// </summary>
        public static async Task<DataTable> GetProductByIDAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));

            string query = "EXEC SP_GetProductByID @ProductID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    productId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously adds a new product.
        /// </summary>
        public static async Task<int> AddProductAsync(
            string name,
            string description,
            decimal price,
            int quantity = 0,
            bool isAvailable = false,
            int? websiteId = null,
            string storeAddress = null,
            CancellationToken cancellationToken = default)
        {
            // Validate required parameters
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
            if (websiteId.HasValue && websiteId.Value <= 0)
                throw new ArgumentException("Website ID must be greater than zero.",
                    nameof(websiteId));

            string query = "EXEC SP_AddProduct @Name, @Description, @Price, @Quantity, " +
                          "@IsAvailable, @WebsiteID, @StoreAddress";
            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    name,
                    description,
                    price,
                    quantity,
                    isAvailable,
                    websiteId,
                    storeAddress);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves products by name.
        /// </summary>
        public static async Task<DataTable> GetProductByNameAsync(
            string productName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentNullException(nameof(productName));

            string query = "EXEC SP_GetProductByName @ProductName";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    productName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }
        #endregion

        #region Quantity Management Operations

        /// <summary>
        /// Asynchronously changes the quantity of a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="newQuantity">The new quantity to set.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if update was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when productId is invalid or newQuantity is negative.</exception>
        public static async Task<bool> ChangeProductQuantityAsync(
            int productId,
            int newQuantity,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));
            if (newQuantity < 0)
                throw new ArgumentException("Quantity cannot be negative.",
                    nameof(newQuantity));

            string query = "EXEC SP_ChangeProductQuantity @ProductID, @NewQuantity";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId,
                    newQuantity);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously decreases the quantity of a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="amount">The amount to decrease.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if decrease was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when productId is invalid or amount is negative.</exception>
        public static async Task<bool> DecreaseProductQuantityAsync(
            int productId,
            int amount,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.",
                    nameof(amount));

            string query = "EXEC SP_DecreaseProductQuantity @ProductID, @Amount";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId,
                    amount);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously increases the quantity of a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="amount">The amount to increase.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when productId is invalid or amount is negative.</exception>
        public static async Task IncreaseProductQuantityAsync(
            int productId,
            int amount,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.",
                    nameof(amount));

            string query = "EXEC SP_IncreaseProductQuantity @ProductID, @Amount";
            try
            {
                // Note: Original method doesn't return a value, maintaining that behavior
                await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId,
                    amount);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously changes the status of a product to delivered.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="orderId">The ID of the associated order.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if status change was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when productId or orderId is invalid.</exception>
        public static async Task<bool> ChangeProductToDeliveredAsync(
            int productId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameters
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.",
                    nameof(orderId));

            string query = "EXEC SP_ChangeProductToDelivered @ProductID, @OrderID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId,
                    orderId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Asynchronously retrieves all products from the database.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing all products.</returns>
        /// <remarks>
        /// This method retrieves all products regardless of their status or availability.
        /// Consider using pagination for large datasets.
        /// </remarks>
        public static async Task<DataTable> GetAllProductsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetAllProducts";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all available products.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing all available products.</returns>
        /// <remarks>
        /// Returns only products where IsAvailable = true.
        /// Products with quantity = 0 might still be included if marked as available.
        /// </remarks>
        public static async Task<DataTable> GetAvailableProductsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetAvailableProducts";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all products associated with a specific shipping.
        /// </summary>
        /// <param name="shippingId">The ID of the shipping to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing products in the specified shipping.</returns>
        /// <exception cref="ArgumentException">Thrown when shippingId is invalid.</exception>
        /// <remarks>
        /// This method returns products that are part of the specified shipping order,
        /// including their current status and quantity.
        /// </remarks>
        public static async Task<DataTable> GetProductsOfShippingAsync(
            int shippingId,
            CancellationToken cancellationToken = default)
        {
            // Validate shipping ID
            if (shippingId <= 0)
                throw new ArgumentException("Shipping ID must be greater than zero.",
                    nameof(shippingId));

            string query = "EXEC SP_GetProductsOfShipping @ShippingID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    shippingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all products that are associated with a website.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>DataTable containing all requested products.</returns>
        /// <remarks>
        /// Returns products that have a non-null WebsiteID,
        /// indicating they were requested through a website.
        /// </remarks>
        public static async Task<DataTable> GetRequestedProductsAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetRequestedProducts";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        #endregion

        #region Availability Management Operations

        /// <summary>
        /// Asynchronously marks a product as available.
        /// </summary>
        /// <param name="productId">The ID of the product to mark as available.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when productId is invalid.</exception>
        /// <remarks>
        /// This operation only changes the IsAvailable flag;
        /// it does not affect the product's quantity.
        /// </remarks>
        public static async Task<bool> MakeProductAvailableAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            // Validate product ID
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));

            string query = "EXEC SP_MakeProductAvailable @ProductID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously marks a product as unavailable.
        /// </summary>
        /// <param name="productId">The ID of the product to mark as unavailable.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when productId is invalid.</exception>
        /// <remarks>
        /// This operation only changes the IsAvailable flag;
        /// it does not affect the product's quantity or other properties.
        /// </remarks>
        public static async Task<bool> MakeProductUnavailableAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            // Validate product ID
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.",
                    nameof(productId));

            string query = "EXEC SP_MakeProductUnavailable @ProductID";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    productId);
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
