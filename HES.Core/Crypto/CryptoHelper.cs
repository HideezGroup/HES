using System;
using System.Security.Cryptography;
using System.Text;

namespace HES.Core.Crypto
{
    public static class CryptoHelper
    {
        const string MAGIC = "HES";

        public static byte[] Encrypt(byte[] data, byte[] key, out byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.GenerateIV();

                using (var cryptoTransform = aes.CreateEncryptor())
                {
                    iv = aes.IV;
                    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;

                using (var cryptoTransform = aes.CreateEncryptor())
                {
                    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;

                using (var cryptoTransform = aes.CreateDecryptor())
                {
                    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public static string Encrypt(int keyId, byte[] key, string plainText)
        {
            if (plainText == null)
                return null;

            var plainTextBuffer = Encoding.UTF8.GetBytes(MAGIC + plainText);
            var encryptedBuffer = Encrypt(plainTextBuffer, key, out byte[] iv);

            var resultBuffer = new byte[sizeof(int) * 2 + encryptedBuffer.Length + iv.Length];

            // KEY ID (4 bytes) + IV LEN (4 bytes) + IV (iv len) + ENCRYPTED(MAGIC + plain text)
            Array.Copy(BitConverter.GetBytes(keyId), 0, resultBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(iv.Length), 0, resultBuffer, sizeof(int), sizeof(int));
            Array.Copy(iv, 0, resultBuffer, sizeof(int) * 2, iv.Length);
            Array.Copy(encryptedBuffer, 0, resultBuffer, sizeof(int) * 2 + iv.Length, encryptedBuffer.Length);

            return Convert.ToBase64String(resultBuffer);
        }

        public static string Decrypt(int keyId, byte[] key, string cipherText)
        {
            if (cipherText == null)
                return null;

            var cipherTextBuffer = Convert.FromBase64String(cipherText);

            // KEY ID
            var storedKeyId = BitConverter.ToInt32(cipherTextBuffer, 0);
            if (storedKeyId != keyId)
                throw new DataProtectionWrongCryptoKeyIdException("Trying to decrypt with a wrong key ID");

            // IV
            var ivLength = BitConverter.ToInt32(cipherTextBuffer, sizeof(int));
            var iv = new byte[ivLength];
            Array.Copy(cipherTextBuffer, sizeof(int) * 2, iv, 0, ivLength);

            var encryptedBuffer = new byte[cipherTextBuffer.Length - sizeof(int) * 2 - ivLength];
            Array.Copy(cipherTextBuffer, sizeof(int) * 2 + ivLength, encryptedBuffer, 0, encryptedBuffer.Length);

            var decrypted = Decrypt(encryptedBuffer, key, iv);

            var decryptedString = Encoding.UTF8.GetString(decrypted);

            if (!decryptedString.StartsWith(MAGIC))
                throw new CryptographicException("Decrypted data is not valid");

            return decryptedString.Substring(MAGIC.Length);
        }
    }
}
