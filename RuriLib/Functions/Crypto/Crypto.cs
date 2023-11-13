﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using CryptSharp.Utility;
using HashLib;
using Scrypt;

namespace RuriLib.Functions.Crypto
{
    /// <summary>
    /// The available hashing functions.
    /// </summary>
    public enum Hash
    {
        /// <summary>The MD2 hashing function (128 bits digest).</summary>
        MD2,

        /// <summary>The MD4 hashing function (128 bits digest).</summary>
        MD4,

        /// <summary>The MD5 hashing function (128 bits digest).</summary>
        MD5,

        /// <summary>The SHA-1 hashing function (160 bits digest).</summary>
        SHA1,

        /// <summary>The SHA-256 hashing function (256 bits digest).</summary>
        SHA256,

        /// <summary>The SHA-384 hashing function (384 bits digest).</summary>
        SHA384,

        /// <summary>The SHA-512 hashing function (512 bits digest).</summary>
        SHA512,

        /// <summary>The SHA3-224 hashing function (224 bits digest).</summary>
        SHA3_224,

        /// <summary>The SHA3-256 hashing function (256 bits digest).</summary>
        SHA3_256,

        /// <summary>The SHA3-384 hashing function (384 bits digest).</summary>
        SHA3_384,

        /// <summary>The SHA3-512 hashing function (512 bits digest).</summary>
        SHA3_512
    }

    /// <summary>
    /// Provides methods for encrypting, decrypting and generating signatures.
    /// </summary>
    public static class Crypto
    {
        #region Hash and Hmac
        /// <summary>
        /// Hashes a byte array through MD2.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The MD2 digest.</returns>
        public static byte[] MD2(byte[] input)
        {
            IHash hash = HashFactory.Crypto.CreateMD2();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }


        /// <summary>
        /// Hashes a byte array through MD4.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The MD4 digest.</returns>
        public static byte[] MD4(byte[] input)
        {
            IHash hash = HashFactory.Crypto.CreateMD4();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }


        /// <summary>
        /// Hashes a byte array through MD5.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hsh</param>
        /// <returns>The MD5 digest.</returns>
        public static byte[] MD5(byte[] input)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                return md5.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates an MD5 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACMD5(byte[] input, byte[] key)
        {
            using (HMACMD5 hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(input);
            }
        }

        /// <summary>
        /// Hashes a byte array through SHA-1.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-1 digest.</returns>
        public static byte[] SHA1(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates a SHA-1 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA1(byte[] input, byte[] key)
        {
            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                return hmac.ComputeHash(input);
            }
        }

        /// <summary>
        /// Hashes a byte array through SHA-256.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-256 digest.</returns>
        public static byte[] SHA256(byte[] input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates a SHA-256 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA256(byte[] input, byte[] key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(input);
            }
        }

        /// <summary>
        /// Hashes a byte array through SHA-384.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-384 digest.</returns>
        public static byte[] SHA384(byte[] input)
        {
            using (SHA384Managed sha384 = new SHA384Managed())
            {
                return sha384.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates a SHA-384 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA384(byte[] input, byte[] key)
        {
            using (HMACSHA384 hmac = new HMACSHA384(key))
            {
                return hmac.ComputeHash(input);
            }
        }

        /// <summary>
        /// Hashes a byte array through SHA-512.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA-512 digest.</returns>
        public static byte[] SHA512(byte[] input)
        {
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                return sha512.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates a SHA-512 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <returns>The HMAC signature.</returns>
        public static byte[] HMACSHA512(byte[] input, byte[] key)
        {
            using (HMACSHA512 hmac = new HMACSHA512(key))
            {
                return hmac.ComputeHash(input);
            }
        }

        /// <summary>
        /// Calculates a SHA3-224 hash signature.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA3-224 digest.</returns>
        public static byte[] SHA3_224(byte[] input)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak224();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }

        /// <summary>
        /// Calculates a SHA3-256 hash signature.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA3-256 digest.</returns>
        public static byte[] SHA3_256(byte[] input)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak256();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }

        /// <summary>
        /// Calculates a SHA3-384 hash signature.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA3-384 digest.</returns>
        public static byte[] SHA3_384(byte[] input)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak384();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }

        /// <summary>
        /// Calculates a SHA3-512 hash signature.
        /// </summary>
        /// <param name="input">The byte array for which to calculate the hash</param>
        /// <returns>The SHA3-512 digest.</returns>
        public static byte[] SHA3_512(byte[] input)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak512();
            HashResult res = hash.ComputeBytes(input);
            return res.GetBytes();
        }


        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>An uppercase hexadecimal string.</returns>
        public static string ToHex(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="input">The hex string</param>
        /// <returns>A byte array</returns>
        public static byte[] FromHex(this string input)
        {
            var resultantArray = new byte[input.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }

        /// <summary>
        /// Converts from the Hash enum to the HashAlgorithmName default struct.
        /// </summary>
        /// <param name="type">The hash type as a Hash enum</param>
        /// <returns>The HashAlgorithmName equivalent.</returns>
        public static HashAlgorithmName ToHashAlgorithmName(this Hash type)
        {
            switch (type)
            {
                case Hash.MD5:
                    return HashAlgorithmName.MD5;

                case Hash.SHA1:
                    return HashAlgorithmName.SHA1;

                case Hash.SHA256:
                    return HashAlgorithmName.SHA256;

                case Hash.SHA384:
                    return HashAlgorithmName.SHA384;

                case Hash.SHA512:
                    return HashAlgorithmName.SHA512;

                default:
                    throw new NotSupportedException("No such algorithm name");
            }
        }

        #endregion

        #region RSA

        private static string RSAEncrypt(string dataToEncrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            RSA.ImportParameters(RSAKeyInfo);

            return Convert.ToBase64String(RSA.Encrypt(Convert.FromBase64String(dataToEncrypt), doOAEPPadding));
        }

        private static string RSADecrypt(string dataToDecrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            RSA.ImportParameters(RSAKeyInfo);

            return Convert.ToBase64String(RSA.Decrypt(Convert.FromBase64String(dataToDecrypt), doOAEPPadding));
        }

        /// <summary>
        /// Encrypts a string using RSA.
        /// </summary>
        /// <param name="data">The data to encrypt as a base64 string</param>
        /// <param name="n">The public key's modulus as a base64 string</param>
        /// <param name="e">The public key's exponent as a base64 string</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS1</param>
        /// <returns>The encrypted data encoded as base64.</returns>
        public static string RSAEncrypt(string data, string n, string e, bool oaep)
        {
            return RSAEncrypt(
                data,
                new RSAParameters
                {
                    Modulus = Encoding.UTF8.GetBytes(n),
                    Exponent = Encoding.UTF8.GetBytes(e)
                },
                oaep
            );
        }

        /// <summary>
        /// Decrypts a string using RSA.
        /// </summary>
        /// <param name="data">The data to decrypt as a base64 string</param>
        /// <param name="n">The public key's modulus as a base64 string</param>
        /// <param name="d">The private key's exponent as a base64 string</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS v1.5</param>
        /// <returns>The decrypted data encoded as base64.</returns>
        public static string RSADecrypt(string data, string n, string d, bool oaep)
        {
            return RSADecrypt(
                data,
                new RSAParameters
                {
                    D = Encoding.UTF8.GetBytes(d),
                    Modulus = Encoding.UTF8.GetBytes(n)
                },
                oaep
            );
        }

        /// <summary>
        /// Encrypts a message using RSA with PKCS1PAD2 padding.
        /// </summary>
        /// <param name="message">The message as a UTF-8 string</param>
        /// <param name="modulus">The public key's modulus as a HEX string</param>
        /// <param name="exponent">The public key's exponent as a HEX string</param>
        /// <returns>The encrypted message.</returns>
        // Thanks to TheLittleTrain for this implementation.
        public static string RSAPkcs1Pad2(string message, string modulus, string exponent)
        {
            // Convert the public key components to numbers
            var n = HexToBigInteger(modulus);
            var e = HexToBigInteger(exponent);

            // (modulus.ToByteArray().Length - 1) * 8
            //modulus has 256 bytes multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(message, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = BigInteger.ModPow(encryptedNumber, e, n);

            //Reverse number and convert to base64
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }

        private static BigInteger HexToBigInteger(string hex)
        {
            return BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }

        private static BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new BigInteger(buffer);
        }
        #endregion

        #region KDF
        /// <summary>
        /// Generates a PKCS v5 #2.0 key using a Password-Based Key Derivation Function.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">The salt to use encoded as base64. If empty, a random salt will be generated</param>
        /// <param name="saltSize">The random salt size that gets generated in case no salt is provided</param>
        /// <param name="iterations">The number of times the algorithm should be executed</param>
        /// <param name="type">The hashing algorithm to use</param>
        /// <param name="keyLength">The generated key length in bytes</param>
        /// <returns>The generated key as a base64 string.</returns>
        public static string PBKDF2PKCS5(string password, string salt, int saltSize = 8, int iterations = 1, int keyLength = 16, Hash type = Hash.SHA1)
        {
            if (salt != string.Empty)
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), iterations, type.ToHashAlgorithmName()))
                {
                    return Convert.ToBase64String(deriveBytes.GetBytes(keyLength));
                }
            }
            else
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, saltSize, iterations, type.ToHashAlgorithmName()))
                {
                    return Convert.ToBase64String(deriveBytes.GetBytes(keyLength));
                }
            }
        }
        #endregion

        #region AES
        /// <summary>
        /// Encrypts a string with AES.
        /// </summary>
        /// <param name="data">The AES-encrypted data</param>
        /// <param name="key">The decryption key as base64</param>
        /// <param name="iv">The initial value as base64</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        /// <returns>The AES-encrypted string encoded as base64</returns>
        public static string AESEncrypt(string data, string key, string iv = "", CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, bool hexKeys = false)
        {
            string encData = null;
            byte[][] keys = ConvertKeys(key, iv, hexKeys);

            encData = EncryptStringToBytes_Aes(data, keys[0], keys[1], mode, padding);

            return encData;
        }

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="data">The AES-encrypted data encoded as base64</param>
        /// <param name="key">The decryption key as base64</param>
        /// <param name="iv">The initial value as base64</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        /// <returns>The plaintext string</returns>
        public static string AESDecrypt(string data, string key, string iv = "", CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None, bool hexKeys = false)
        {
            string decData = null;
            byte[][] keys = ConvertKeys(key, iv, hexKeys);

            decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1], mode, padding);

            return decData;
        }

        private static byte[][] ConvertKeys(string key, string iv, bool hexKeys)
        {
            byte[][] result = new byte[2][];

            if (hexKeys)
            {
                result[0] = key.FromHex();

                if (string.IsNullOrEmpty(iv))
                {
                    result[1] = key.FromHex();
                    Array.Resize(ref result[1], 16);
                }
                else
                {
                    result[1] = iv.FromHex();
                }

                return result;
            }

            result[0] = Convert.FromBase64String(key);

            if (string.IsNullOrEmpty(iv))
            {
                result[1] = Convert.FromBase64String(key);
                Array.Resize(ref result[1], 16);
            }
            else
            {
                result[1] = Convert.FromBase64String(iv);
            }

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV, CipherMode mode, PaddingMode padding)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (RijndaelManaged aesAlg = new RijndaelManaged())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = mode;
                aesAlg.Padding = padding;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV, CipherMode mode, PaddingMode padding)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = mode;
                aesAlg.Padding = padding;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        #endregion

        #region Scrypt

        private static readonly RandomNumberGenerator _saltGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ScryptEncoder(string input, string salt = "", int cost = 1024, int blockSize = 1, int parallel = 1, int outputLength = 16, bool base64Output = false)
        {
            byte[] sBytes = null;
            if (string.IsNullOrWhiteSpace(salt))
            {
                byte[] array = new byte[32];
                _saltGenerator.GetBytes(array);
                sBytes = array;
            }

            byte[] iBytes = Encoding.UTF8.GetBytes(input);
            if (sBytes == null)
            {
                sBytes = Encoding.UTF8.GetBytes(salt);
            }
            byte[] output = SCrypt.ComputeDerivedKey(iBytes, sBytes, cost, blockSize, parallel, null, outputLength);

            if (!base64Output)
            {
                return BitConverter.ToString(output).Replace("-", "").ToLower();
            }
            return Convert.ToBase64String(output);
        }

        public static bool ScryptCompare(string input, string hashedPassword)
        {
            try
            {
                return new ScryptEncoder().Compare(input, hashedPassword);
            }
            catch { return false; }
        }

        public static bool ScryptIsValid(string hashedPassword)
        {
            return new ScryptEncoder().IsValid(hashedPassword);
        }

        #endregion

        #region Bcrypt

        public static string BcryptEncoder(string input, int? workFactor, string salt = "")
        {
            if (workFactor.HasValue && string.IsNullOrEmpty(salt))
                return BCrypt.Net.BCrypt.HashPassword(input, workFactor.Value);
            else if (!workFactor.HasValue && !string.IsNullOrEmpty(salt))
                return BCrypt.Net.BCrypt.HashPassword(input, salt);
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        public static bool BcryptVerify(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }

        public static string BcryptGenerateSalt(int? workFactor)
        {
            if (!workFactor.HasValue)
                return BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.GenerateSalt(workFactor.Value);
        }

        #endregion

    }
}
