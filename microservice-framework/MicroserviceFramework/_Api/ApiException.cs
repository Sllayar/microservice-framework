using System;
using RFI.MicroserviceFramework._Helpers;

namespace RFI.MicroserviceFramework._Api
{
    [Serializable]
    public class ApiException : Exception
    {
        public CodeStatus Code { get; set; }

        public string Detail { get; set; }

        public override string Message => Detail;

        public ApiException(CodeStatus code, string message = null) : base(message ?? code.GetDetail())
        {
            Code = code;
            Detail = message.NotNull() ? message : code.GetDetail();
        }

        public ApiException(string message) : base(message)
        {
            Code = CodeStatus.Warning;
            Detail = message;
        }
    }
}