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
        public bool Passwordless { get; protected set; }

        [DataMember]
        public string Message { get; protected set; }

        public static AuthorizationResponse Success => new AuthorizationResponse { Succeeded = true };

        public static AuthorizationResponse SuccessAndPasswordless => new AuthorizationResponse { Succeeded = true, Passwordless = true };
    }
}
