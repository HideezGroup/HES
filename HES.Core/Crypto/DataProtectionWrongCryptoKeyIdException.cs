using System;
using System.Security.Cryptography;

namespace HES.Core.Crypto
{
    public class DataProtectionWrongCryptoKeyIdException : CryptographicException
    {
        public DataProtectionWrongCryptoKeyIdException()
        {
        }

        public DataProtectionWrongCryptoKeyIdException(string message) 
            : base(message)
        {
        }

        public DataProtectionWrongCryptoKeyIdException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}