using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HES.Core.Crypto
{
    public class DataProtectionKey
    {
        const string MAGIC = "HIDEEZ";
        const int KEY_LEN = 32;
        const int VERIFY_CODE_LEN = 4;

        readonly DataProtectionParams _prms;

        byte[] _key;

        public int KeyId { get; }
        public bool Validated => _key != null;

        public DataProtectionKey(int keyId, DataProtectionParams prms)
        {
            KeyId = keyId;
            _prms = prms;
        }

        public static DataProtectionParams CreateParams(string password)
        {
            var salt = new byte[16];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(salt);
            }

            var prms = new DataProtectionParams()
            {
                Salt = Convert.ToBase64String(salt)
            };

            var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                Convert.FromBase64String(prms.Salt),
                prms.IterationCount,
                new HashAlgorithmName(prms.HashAlgorithmName));

            byte[] key = pbkdf2.GetBytes(KEY_LEN);

            var encrypted = CryptoHelper.Encrypt(Encoding.UTF8.GetBytes(MAGIC), key, out byte[] iv);
            var verificationCode = new byte[iv.Length + VERIFY_CODE_LEN];
            Array.Copy(encrypted, 0, verificationCode, 0, VERIFY_CODE_LEN);
            Array.Copy(iv, 0, verificationCode, VERIFY_CODE_LEN, iv.Length);

            prms.VerificationCode = Convert.ToBase64String(verificationCode);
            return prms;
        }

        public bool ValidatePassword(string password)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                Convert.FromBase64String(_prms.Salt),
                _prms.IterationCount,
                new HashAlgorithmName(_prms.HashAlgorithmName));

            var key = pbkdf2.GetBytes(KEY_LEN);

            var verificationCode = Convert.FromBase64String(_prms.VerificationCode);
            var iv = new byte[verificationCode.Length - VERIFY_CODE_LEN];
            Array.Copy(verificationCode, VERIFY_CODE_LEN, iv, 0, iv.Length);

            var encrypted = CryptoHelper.Encrypt(Encoding.UTF8.GetBytes(MAGIC), key, iv);

            if (CryptographicOperations.FixedTimeEquals(
                verificationCode.Take(VERIFY_CODE_LEN).ToArray(),
                encrypted.Take(VERIFY_CODE_LEN).ToArray()))
            {
                _key = key;
            }

            return Validated;
        }

        public string Encrypt(string plainText)
        {
            return CryptoHelper.Encrypt(KeyId, _key, plainText);
        }

        public string Decrypt(string cipherText)
        {
            return CryptoHelper.Decrypt(KeyId, _key, cipherText);
        }

        internal bool TryDecrypt(string cipherText, out string plainText)
        {
            plainText = null;
            try
            {
                plainText = CryptoHelper.Decrypt(KeyId, _key, cipherText);
                return true;
            }
            catch (DataProtectionWrongCryptoKeyIdException)
            {
                return false;
            }
        }
    }
}