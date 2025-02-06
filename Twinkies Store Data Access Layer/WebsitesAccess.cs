using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TwinkiesStoreDataAccessLayer.Utils;

namespace TwinkiesStoreDataAccessLayer
{
    /// <summary>
    /// Provides data access methods for Website-related database operations.
    /// </summary>
    /// <remarks>
    /// Database Table Structure:
    /// - WebsiteID (PK): Integer
    /// - WebsiteName: String
    /// </remarks
    public static class WebsitesAccess
    {
        //Fields: websiteID, websiteName

        #region Synchronous Methods

        /// <summary>
        /// Retrieves details of a specific website from the Websites table.
        /// </summary>
        /// <param name="websiteId">The ID of the website to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified website.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetWebsiteDetails(int websiteId) // Find
        {
            string query = "EXEC SP_GetWebsiteDetails @WebsiteID";
            try
            {
                return ConnectionUtils.GetTable(query, websiteId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Adds a new website to the database by calling the stored procedure [dbo].[SP_AddWebsite].
        /// </summary>
        /// <param name="websiteName">The name of the website to be added.</param>
        /// <returns>The primary key (Website ID) of the newly added website, or -1 if the operation fails.</returns>
        /// <exception cref="SqlException">SqlException: Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException: Thrown when the connection is not open or is invalid.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException: Thrown when a required parameter is null.</exception>
        /// <exception cref="Exception">Exception: Thrown when there is an error executing the SQL command or any other unexpected error occurs.</exception>
        public static int AddWebsite(string websiteName) // _AddNew
        {
            string query = "EXEC [dbo].[SP_AddWebsite] @WebsiteName";

            try
            {
                // Add the website and return the new Website ID
                return ConnectionUtils.AddRowToTable(query, websiteName);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // Logger.Error(ex);
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Deletes a website from the Websites table by its ID.
        /// </summary>
        /// <param name="websiteId">The ID of the website to be deleted.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool DeleteWebsite(int websiteId) // Delete
        {
            string query = "EXEC SP_DeleteWebsite @WebsiteID";
            try
            {
                return ConnectionUtils.DeleteTableRow(query, websiteId);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Edits the details of a website in the Websites table.
        /// </summary>
        /// <param name="websiteId">The ID of the website to be updated.</param>
        /// <param name="websiteName">The new name of the website.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static bool EditWebsite(int websiteId, string websiteName) // _Update
        {
            string query = "EXEC SP_EditWebsite @WebsiteID, @WebsiteName";
            try
            {
                return ConnectionUtils.UpdateTableRow(query, websiteId, websiteName);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all websites from the Websites table.
        /// </summary>
        /// <returns>A DataTable containing the website details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetAllWebsites() // GetTable
        {
            string query = "EXEC SP_GetAllWebsites";
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

        #endregion  

        #region Asynchronous Methods

        /// <summary>
        /// Asynchronously retrieves details of a specific website.
        /// </summary>
        /// <param name="websiteId">The ID of the website to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing the website details.</returns>
        /// <exception cref="ArgumentException">Thrown when websiteId is invalid.</exception>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetWebsiteDetailsAsync(
            int websiteId,
            CancellationToken cancellationToken = default)
        {
            if (websiteId <= 0)
                throw new ArgumentException("Website ID must be positive.",
                    nameof(websiteId));

            string query = "EXEC SP_GetWebsiteDetails @WebsiteID";
            try
            {
                return await ConnectionUtils.GetTableAsync(
                    query,
                    cancellationToken,
                    websiteId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously adds a new website.
        /// </summary>
        /// <param name="websiteName">The name of the website to add.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The ID of the new website, or -1 if the operation fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when websiteName is null or empty.</exception>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<int> AddWebsiteAsync(
            string websiteName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(websiteName))
                throw new ArgumentNullException(nameof(websiteName));

            string query = "EXEC [dbo].[SP_AddWebsite] @WebsiteName";

            try
            {
                return await ConnectionUtils.AddRowToTableAsync(
                    query,
                    cancellationToken,
                    websiteName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously deletes a website.
        /// </summary>
        /// <param name="websiteId">The ID of the website to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when websiteId is invalid.</exception>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<bool> DeleteWebsiteAsync(
            int websiteId,
            CancellationToken cancellationToken = default)
        {
            if (websiteId <= 0)
                throw new ArgumentException("Website ID must be positive.",
                    nameof(websiteId));

            string query = "EXEC SP_DeleteWebsite @WebsiteID";
            try
            {
                return await ConnectionUtils.DeleteTableRowAsync(
                    query,
                    cancellationToken,
                    websiteId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates a website's details.
        /// </summary>
        /// <param name="websiteId">The ID of the website to update.</param>
        /// <param name="websiteName">The new name for the website.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if update was successful, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when websiteId is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when websiteName is null or empty.</exception>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<bool> EditWebsiteAsync(
            int websiteId,
            string websiteName,
            CancellationToken cancellationToken = default)
        {
            if (websiteId <= 0)
                throw new ArgumentException("Website ID must be positive.",
                    nameof(websiteId));
            if (string.IsNullOrWhiteSpace(websiteName))
                throw new ArgumentNullException(nameof(websiteName));

            string query = "EXEC SP_EditWebsite @WebsiteID, @WebsiteName";
            try
            {
                return await ConnectionUtils.UpdateTableRowAsync(
                    query,
                    cancellationToken,
                    websiteId,
                    websiteName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves all websites.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A DataTable containing all website details.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static async Task<DataTable> GetAllWebsitesAsync(
            CancellationToken cancellationToken = default)
        {
            string query = "EXEC SP_GetAllWebsites";
            try
            {
                return await ConnectionUtils.GetTableAsync(query, cancellationToken);
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
