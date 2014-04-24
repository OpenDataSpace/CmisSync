//-----------------------------------------------------------------------
// <copyright file="Crypto.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using CmisSync.Lib.Storage;

namespace CmisSync.Lib
{
    /// <summary>
    /// Obfuscation for sensitive data, making password harvesting a little less straightforward.
    /// Web browsers employ the same technique to store user passwords.
    /// </summary>
    public static class Crypto
    {
        /// <summary>
        /// Obfuscate a string.
        /// </summary>
        /// <param name="value">The string to obfuscate</param>
        /// <returns>The obfuscated string</returns>
        public static string Obfuscate(string value)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsObfuscate(value);
            }
            else
            {
                return UnixObfuscate(value);
            }
        }


        /// <summary>
        /// Deobfuscate a string.
        /// </summary>
        /// <param name="value">The string to deobfuscate</param>
        /// <returns>The clear string</returns>
        public static string Deobfuscate(string value)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return WindowsDeobfuscate(value);
            }
            else
            {
                return UnixDeobfuscate(value);
            }
        }


        /// <summary>
        /// Obfuscate a string on Windows.
        /// We use the recommended API for this: DPAPI (Windows Data Protection API)
        /// http://msdn.microsoft.com/en-us/library/ms995355.aspx
        /// Warning: Even though it uses the Windows user's password, it is not uncrackable.
        /// </summary>
        /// <param name="value">The string to obfuscate</param>
        /// <returns>The obfuscated string</returns>
        private static string WindowsObfuscate(string value)
        {
            #if __MonoCS__
                // This macro prevents compilation errors on Unix where ProtectedData does not exist.
                return "Should never be reached";
            #else
            try
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
                    // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                    //  only by the same current user.
                    byte[] crypt = ProtectedData.Protect(data, GetCryptoKey(), DataProtectionScope.CurrentUser);
                    return Convert.ToBase64String(crypt, Base64FormattingOptions.None);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine("Data was not encrypted. An error occurred.");
                    Console.WriteLine(e.ToString());
                    return null;
                }
            #endif
        }


        /// <summary>
        /// Deobfuscate a string on Windows.
        /// </summary>
        /// <param name="value">The string to deobfuscate</param>
        /// <returns>The clear string</returns>
        private static string WindowsDeobfuscate(string value)
        {
            #if __MonoCS__
                // This macro prevents compilation errors on Unix where ProtectedData does not exist.
                throw new ApplicationException("Should never be reached");
            #else
            try
                {
                    byte[] data = Convert.FromBase64String(value);
                    //Decrypt the data using DataProtectionScope.CurrentUser.
                    byte[] uncrypt = ProtectedData.Unprotect(data, GetCryptoKey(), DataProtectionScope.CurrentUser);
                    return System.Text.Encoding.UTF8.GetString(uncrypt);
                }
                catch (Exception e)
                {
                    if (e is CryptographicException || e is FormatException)
                    {
                        Console.WriteLine("Your password is not obfuscated yet.");
                        Console.WriteLine("Using unobfuscated value directly might be deprecated soon, so please delete your local directories and recreate them. Thank you for your understanding.");
                        return value;
                    }
                    else
                    {
                        throw;
                    }
                }
            #endif
        }


        /// <summary>
        /// Obfuscate a string on Unix.
        /// AES is used.
        /// </summary>
        /// <param name="value">The string to obfuscate</param>
        /// <returns>The obfuscated string</returns>
        private static string UnixObfuscate(string value)
        {
#if __MonoCS__
            try
            {
                using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(
                    GetCryptoKey(), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }))
                using (AesManaged myAes = new AesManaged())
                {
                    myAes.Key = pdb.GetBytes(myAes.KeySize / 8);
                    myAes.IV = pdb.GetBytes(myAes.BlockSize / 8);
                    using (ICryptoTransform encryptor = myAes.CreateEncryptor())
                    {
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
                        byte[] crypt = encryptor.TransformFinalBlock(data, 0, data.Length);
                        return Convert.ToBase64String(crypt, Base64FormattingOptions.None);
                    }
                }
            }

            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
#else
            throw new ApplicationException("Should never be reached");
#endif
        }


        /// <summary>
        /// Deobfuscate a string on UNIX.
        /// </summary>
        /// <param name="value">The string to deobfuscate</param>
        /// <returns>The clear string</returns>
        private static string UnixDeobfuscate(string value)
        {
#if __MonoCS__
            try
            {
                using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(
                    GetCryptoKey(), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }))
                using (AesManaged myAes = new AesManaged())
                {
                    myAes.Key = pdb.GetBytes(myAes.KeySize / 8);
                    myAes.IV = pdb.GetBytes(myAes.BlockSize / 8);
                    using (ICryptoTransform decryptor = myAes.CreateDecryptor())
                    {
                        byte[] data = Convert.FromBase64String(value);
                        byte[] uncrypt = decryptor.TransformFinalBlock(data, 0, data.Length);
                        return System.Text.Encoding.UTF8.GetString(uncrypt);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is CryptographicException || e is FormatException || e is ArgumentException)
                {
                    Console.WriteLine("Your password is not obfuscated yet.");
                    Console.WriteLine("Using unobfuscated value directly might be deprecated soon, so please delete your local directories and recreate them. Thank you for your understanding.");
                    return value;
                }
                else
                {
                    throw;
                }
            }
#else
            throw new ApplicationException("Should never be reached");
#endif
        }


        /// <summary>
        /// Salt for the obfuscation.
        /// </summary>
        public static byte[] GetCryptoKey()
        {
            return System.Text.Encoding.UTF8.GetBytes(
                "Thou art so farth away, I miss you my dear files❥, with CmisSync be forever by my side!");
        }

        /// <summary>
        /// Creates the hash algorithm by the given name.
        /// </summary>
        /// <returns>The hash algorithm.</returns>
        /// <param name="name">Name.</param>
        public static HashAlgorithm CreateHashAlgorithm(string name) {
            name = name.ToLower();
            if(name.Equals("sha1"))
                return SHA1.Create();
            if(name.Equals("sha256"))
                return SHA256.Create();
            if(name.Equals("sha384"))
                return SHA384.Create();
            if(name.Equals("sha512"))
                return SHA512.Create();
            if(name.Equals("md5"))
                return MD5.Create();
            if(name.Equals("ripemd160") || name.Equals("ripemd"))
                return RIPEMD160.Create();
            return HashAlgorithm.Create();
        }

        /// <summary>
        /// Calculates the checksum over the given stream.
        /// </summary>
        /// <returns>The checksum.</returns>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        /// <param name="stream">Stream.</param>
        public static byte[] CalculateChecksum(string hashAlgorithm, Stream stream) {
            using (var bs = new BufferedStream(stream))
            using (var hash = CreateHashAlgorithm(hashAlgorithm))
            {
                return hash.ComputeHash(bs);
            }
        }

        /// <summary>
        /// Calculates the checksum over the given stream with a former created hashAlgorithm
        /// </summary>
        /// <returns>The checksum.</returns>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        /// <param name="file">File.</param>
        public static byte[] CalculateChecksum(string hashAlgorithm, IFileInfo file) {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return CalculateChecksum(hashAlgorithm, stream);
            }
        }
    }
}
