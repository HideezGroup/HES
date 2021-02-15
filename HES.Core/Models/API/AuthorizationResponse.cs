using HES.Core.Entities;
using HES.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace HES.Core.Models.API
{
    [Serializable]
    [DataContract]
    public class AuthorizationResponse
    {
        [DataMember]
        public bool Succeeded { get; protected set; }

        [DataMember]
        public bool RequiresTwoFactor { get; protected set; }
        
        [DataMember]
        public bool IsLockedOut { get; protected set; }

        [DataMember]
        public bool Failed { get; protected set; }

        [DataMember]
        public string Message { get; protected set; }

        [DataMember]
        public HESCode Code { get; protected set; }

        [DataMember]
        public ApplicationUser User { get; protected set; }

        public static AuthorizationResponse Success(ApplicationUser user)
        {
            return new AuthorizationResponse { Succeeded = true, User = user };
        }  
        
        public static AuthorizationResponse TwoFactorRequired(ApplicationUser user)
        {
            return new AuthorizationResponse { RequiresTwoFactor = true, User = user };
        }

         public static AuthorizationResponse LockedOut(ApplicationUser user)
        {
            return new AuthorizationResponse { IsLockedOut = true, User = user };
        }

        public static AuthorizationResponse Error(HESCode code)
        {
            return new AuthorizationResponse { Failed = true, Code = code };
        }

        public static AuthorizationResponse Error(string message)
        {
            return new AuthorizationResponse { Failed = true, Code = HESCode.None, Message = message };
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