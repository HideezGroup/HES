using HES.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace HES.Core.Models.API
{
    [Serializable]
    [DataContract]
    public class IdentityResponse
    {
        [DataMember]
        public bool Succeeded { get; protected set; }

        [DataMember]
        public bool Failed { get; protected set; }

        [DataMember]
        public string Message { get; protected set; }

        [DataMember]
        public HESCode Code { get; protected set; }
              
        public static IdentityResponse Success()
        {
            return new IdentityResponse { Succeeded = true };
        }  
        
        public static IdentityResponse Error(HESCode code)
        {
            return new IdentityResponse { Failed = true, Code = code };
        }

        public static IdentityResponse Error(string message)
        {
            return new IdentityResponse { Failed = true, Code = HESCode.None, Message = message };
        }

        public void ThrowIfFailed()
        {
            if (Failed)
            {
                if (Code == HESCode.None)
                {
                    throw new Exception(Message);
                }
                else
                {
                    throw new HESException(Code);
                }
            }
        }
    }
}