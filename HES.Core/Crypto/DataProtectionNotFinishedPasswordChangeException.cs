using System;

namespace HES.Core.Crypto
{
    public class DataProtectionNotFinishedPasswordChangeException : Exception
    {
        public DataProtectionNotFinishedPasswordChangeException(string message) : base(message)
        {

        }
    }
}