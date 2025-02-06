using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TwinkiesStoreDataAccessLayer.Utils
{
    internal class CryptograpgyUtils
    {
        public static string? ComputeHash(string word)
        {
            try
            {
                string hashString;

                using (SHA256 sha256 = SHA256.Create())
                {
                    // compute the hash of the byte array of the word
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(word));

                    // Convert the byte array to its string form
                    hashString = BitConverter.ToString(hashBytes);
                }

                return hashString;
            }
            catch (Exception ex)
            {
                // Add to log
            }

            return null;

        }
    }
}
