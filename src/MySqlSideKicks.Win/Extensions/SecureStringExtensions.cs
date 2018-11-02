using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace System
{
    internal static class SecureStringExtensions
    {
        private static readonly byte[] Entropy = Encoding.Unicode.GetBytes("Don't trouble yourself with matters you truly cannot change.");

        public static SecureString DecryptToSecureString(this string base64EncryptedData)
        {
            return ToSecureString(DecryptToString(base64EncryptedData));
        }

        public static string DecryptToString(this string base64EncryptedData)
        {
            try
            {
                var decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(base64EncryptedData), Entropy, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return default;
            }
        }

        public static SecureString ToSecureString(this string input)
        {
            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToEncryptedBase64(this SecureString input)
        {
            return ToEncryptedBase64(ToInsecureString(input));            
        }

        public static string ToEncryptedBase64(this string input)
        {
            var encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(input), Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static string ToInsecureString(this SecureString input)
        {
            string returnValue;
            var ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}