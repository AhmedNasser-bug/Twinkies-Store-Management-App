using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TestAccessLayer.Utils;

namespace TestAccessLayer
{
    public static class WebsitesAccess
    {
        /// <summary>
        /// Retrieves details of a specific website from the Websites table.
        /// </summary>
        /// <param name="websiteId">The ID of the website to be retrieved.</param>
        /// <returns>A DataTable containing the details of the specified website.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not properly opened.</exception>
        public static DataTable GetWebsiteDetails(int websiteId)
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
        public static int AddWebsite(string websiteName)
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
        public static bool DeleteWebsite(int websiteId)
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
        public static bool EditWebsite(int websiteId, string websiteName)
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
        public static DataTable GetAllWebsites()
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
    }
}
