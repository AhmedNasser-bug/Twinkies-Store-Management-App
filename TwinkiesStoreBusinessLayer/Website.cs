namespace TwinkiesStoreBusinessLayer
{
    using System;
    using System.Data;
    using TwinkiesStoreDataAccessLayer;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using System.Runtime.CompilerServices;

        /// <summary>
        /// Represents a website entity in the business layer.
        /// </summary>
        public class Website : INotifyPropertyChanged, IDataErrorInfo
        {
            #region Events
            public event PropertyChangedEventHandler PropertyChanged;
            #endregion

            #region Private Fields
            private enum enMode { AddNew, Update }
            private enMode _Mode;
            private int _websiteId;
            private string _websiteName;
            #endregion

            #region Properties
            public int WebsiteID
            {
                get => _websiteId;
                private set
                {
                    if (_websiteId != value)
                    {
                        _websiteId = value;
                        NotifyPropertyChanged();
                        ValidateProperty(value);
                    }
                }
            }

            [Required(ErrorMessage = "Website name is required")]
            [StringLength(100, MinimumLength = 3,
                ErrorMessage = "Website name must be between 3 and 100 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$",
                ErrorMessage = "Website name contains invalid characters")]
            public string WebsiteName
            {
                get => _websiteName;
                set
                {
                    if (_websiteName != value)
                    {
                        _websiteName = value;
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
            private Website(int websiteId, string websiteName)
            {
                WebsiteID = websiteId;
                WebsiteName = websiteName;
                _Mode = enMode.Update;
            }

            public Website()
            {
                WebsiteID = -1;
                WebsiteName = string.Empty;
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

            private bool _Update()
            {
                ValidateProperty(WebsiteName);
                return WebsitesAccess.EditWebsite(WebsiteID, WebsiteName);
            }

            private async Task<bool> _UpdateAsync()
            {
                await ValidatePropertiesAsync();
                return await Task.Run(() => WebsitesAccess.EditWebsite(WebsiteID, WebsiteName));
            }

            private bool _AddNew()
            {
                ValidateProperty(WebsiteName);
                WebsiteID = WebsitesAccess.AddWebsite(WebsiteName);
                return WebsiteID != -1;
            }

            private async Task<bool> _AddNewAsync()
            {
                await ValidatePropertiesAsync();
                WebsiteID = await Task.Run(() => WebsitesAccess.AddWebsite(WebsiteName));
                return WebsiteID != -1;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Saves the current website to the database.
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
            /// Asynchronously saves the current website to the database.
            /// </summary>
            /// <returns>True if the operation was successful, false otherwise.</returns>
            /// <exception cref="ValidationException">Thrown when validation fails.</exception>
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
            /// Deletes the current website from the database.
            /// </summary>
            /// <returns>True if deletion was successful, false otherwise.</returns>
            public bool Delete()
            {
                return WebsitesAccess.DeleteWebsite(WebsiteID);
            }

            /// <summary>
            /// Asynchronously deletes the current website from the database.
            /// </summary>
            /// <returns>True if deletion was successful, false otherwise.</returns>
            public async Task<bool> DeleteAsync()
            {
                return await Task.Run(() => WebsitesAccess.DeleteWebsite(WebsiteID));
            }

            /// <summary>
            /// Finds a website by its ID.
            /// </summary>
            /// <param name="websiteId">The ID of the website to find.</param>
            /// <returns>A Website instance if found, null otherwise.</returns>
            public static Website Find(int websiteId)
            {
                DataTable dt = WebsitesAccess.GetWebsiteDetails(websiteId);

                if (dt?.Rows.Count > 0)
                {
                    return new Website(
                        Convert.ToInt32(dt.Rows[0]["WebsiteID"]),
                        dt.Rows[0]["WebsiteName"].ToString()
                    );
                }
                return null;
            }

            /// <summary>
            /// Asynchronously finds a website by its ID.
            /// </summary>
            /// <param name="websiteId">The ID of the website to find.</param>
            /// <returns>A Website instance if found, null otherwise.</returns>
            public static async Task<Website> FindAsync(int websiteId)
            {
                var dt = await Task.Run(() => WebsitesAccess.GetWebsiteDetails(websiteId));

                if (dt?.Rows.Count > 0)
                {
                    return new Website(
                        Convert.ToInt32(dt.Rows[0]["WebsiteID"]),
                        dt.Rows[0]["WebsiteName"].ToString()
                    );
                }
                return null;
            }

            /// <summary>
            /// Gets all websites from the database.
            /// </summary>
            /// <returns>A DataTable containing all websites.</returns>
            public static DataTable GetTable()
            {
                return WebsitesAccess.GetAllWebsites();
            }

            /// <summary>
            /// Asynchronously gets all websites from the database.
            /// </summary>
            /// <returns>A DataTable containing all websites.</returns>
            public static async Task<DataTable> GetTableAsync()
            {
                return await Task.Run(() => WebsitesAccess.GetAllWebsites());
            }
            #endregion
        }
    }


