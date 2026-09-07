using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class SecuredDataService
    {
        public static string Encrypted(string setValue)
        {
            try
            {
                byte[] data = UTF8Encoding.UTF8.GetBytes(setValue);
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes("BryanJayAranzasoBodino@Security_Data12345"));
                    using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                    {
                        ICryptoTransform transform = tripDes.CreateEncryptor();
                        byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                        setValue = Convert.ToBase64String(results, 0, results.Length);
                        setValue = HttpUtility.UrlEncode(setValue);
                    }
                }
            }
            catch
            {
            }
            return setValue;
        }
        public static string Decrypted(string setValue)
        {
            try
            {
                string urlDecoded = WebUtility.UrlDecode(setValue);
                byte[] data = SafeBase64UrlDecode(urlDecoded);
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes("BryanJayAranzasoBodino@Security_Data12345"));
                    using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                    {
                        ICryptoTransform transform = tripDes.CreateDecryptor();
                        byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                        setValue = UTF8Encoding.UTF8.GetString(results);
                    }
                }
            }
            catch
            {
                setValue = "";
            }
            return setValue;
        }
        private static byte[] SafeBase64UrlDecode(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    throw new ArgumentException("Input is null or empty.");

                // URL-decode the string
                string base64 = WebUtility.UrlDecode(input);

                // Replace spaces (if any) with '+', common issue if URL wasn't encoded properly
                base64 = base64.Replace(' ', '+');

                // Remove any invalid characters manually (optional safety check)
                base64 = Regex.Replace(base64, @"[^A-Za-z0-9\+/=]", "");

                // Add padding if necessary
                int padding = 4 - (base64.Length % 4);
                if (padding < 4)
                {
                    base64 += new string('=', padding);
                }

                return Convert.FromBase64String(base64);
            }
            catch (FormatException ex)
            {
                // Log or rethrow with context
                throw new InvalidOperationException("Invalid Base64 input after decoding.", ex);
            }
        }

    }
}