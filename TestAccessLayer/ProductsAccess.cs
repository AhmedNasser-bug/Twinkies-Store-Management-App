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
    public static class ProductsAccess
    {
        /// <summary>
        /// Retrieves a specific product by its ID from the Products table.
        /// </summary>
        /// <param name="productId">The ID of the product to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified product.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetProductByID(int productId)
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
        /// Retrieves products from the Products table that match the specified name.
        /// </summary>
        /// <param name="productName">The name of the product to search for.</param>
        /// <returns>A DataTable containing the details of the products that match the specified name.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetProductByName(string productName)
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
        public static bool DeleteProductByID(int productId)
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
        public static bool DeleteProductByName(string productName)
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
        public static bool DeleteZeroQuantityProducts()
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
                                        string? storeAddress = null)
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
        public static DataTable GetAllProducts()
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
        public static DataTable GetAvailableProducts()
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
        public static DataTable GetProductsOfShipping(int shippingId)
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
        public static DataTable GetRequestedProducts()
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
    }
}
