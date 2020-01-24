using System;
using Newtonsoft.Json;
using RFI.MicroserviceFramework._Loggers;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Api
{
    public interface IRequest
    {
        int User_ID { get; set; }

        int Partner_ID { get; set; }

        string Path { get; set; }

        string Method { get; set; }

        decimal Version { get; set; }

        DateTime Date { get; set; }

        bool IsTestRequest { get; set; }

        int Request_ID { get; set; }

        void ValidateAndFormat();
    }

    public class ApiRequest : IRequest
    {
        [DBIgnoreParam]
        public int Request_ID { get; set; }

        [DBIncludeParam]
        public int User_ID { get; set; }

        [DBIncludeParam]
        public int Partner_ID { get; set; }

        [JsonIgnore]
        [DBIgnoreParam]
        public string Path { get; set; }

        [JsonIgnore]
        [DBIgnoreParam]
        public string Method { get; set; }

        [JsonIgnore]
        [DBIgnoreParam]
        public decimal Version { get; set; }

        [JsonIgnore]
        [DBIgnoreParam]
        public DateTime Date { get; set; }

        [JsonIgnore]
        [DBIgnoreParam]
        public bool IsTestRequest { get; set; }

        public void ValidateAndFormat()
        {
            try
            {
                ThrowValidationApiExceptionIf(Request_ID <= 0, "Request_ID must be greater than 0");
                ThrowValidationApiExceptionIf(Partner_ID <= 0, "Partner_ID must be greater than 0");

                ValidateAndFormatFields();
            }
            catch(ApiException)
            {
                throw;
            }
            catch(Exception ex)
            {
                ex.Log();
                throw new ApiException(CodeStatus.Warning, "Validation error");
            }
        }

        protected virtual void ValidateAndFormatFields()
        {
            // for override
        }

        protected static void ThrowValidationApiExceptionIf(bool isNotValid, string message = "")
        {
            if(isNotValid)
            {
                throw new ApiException(CodeStatus.Warning, $"Request validation error: {message}");
            }
        }
    }
}