using System.Collections.Generic;
using System.Text.Json.Serialization;
using RFI.MicroserviceFramework._Helpers;

namespace RFI.MicroserviceFramework._Api
{
    public enum CodeStatus
    {
        Ok = 0,
        UnhandledException = 1,
        Warning = 2,
        PermissionDenied = 3,
        NotAllowedMethod = 4,
        DuplicateRequest = 5,
        SignatureNotValid = 6,
        TooFrequentRequests = 7,
        TokenExpired = 8,
        DataNotFound = 9,
        InProcess = 10
    }

    public static class ApiResponseStatuses
    {
        private static readonly Dictionary<CodeStatus, string> Dict = new Dictionary<CodeStatus, string>(10)
        {
            { CodeStatus.Ok, "Ok" },
            { CodeStatus.UnhandledException, "Unhandled exception" },
            { CodeStatus.Warning, "Warning" },
            { CodeStatus.PermissionDenied, "Permission denied" },
            { CodeStatus.NotAllowedMethod, "Not allowed method" },
            { CodeStatus.DuplicateRequest, "Duplicate request" },
            { CodeStatus.SignatureNotValid, "Signature is not valid" },
            { CodeStatus.TooFrequentRequests, "Too frequent requests" },
            { CodeStatus.TokenExpired, "Token is expired" },
            { CodeStatus.DataNotFound, "Data not found" },
            { CodeStatus.InProcess, "In Process" }
        };

        public static string GetDetail(this CodeStatus code) => Dict[code];
    }

    public class ApiResponseStatus
    {
        [JsonPropertyName("code")]
        public CodeStatus Code { get; set; } = CodeStatus.Ok;

        [JsonPropertyName("detail")]
        public string Detail { get; set; } = CodeStatus.Ok.GetDetail();
    }

    public class ApiResponse : ApiSafeData
    {
        [JsonPropertyName("status")]
        public ApiResponseStatus Status { get; } = new ApiResponseStatus();

        public void SetStatus(CodeStatus code, string description = null)
        {
            Status.Code = code;
            Status.Detail = description.NotNull() ? description : code.GetDetail();
        }

        public void SetSafeData(ApiSafeData safeData)
        {
            Data = safeData.Data;
            Signature = safeData.Signature;
            Des = safeData.Des;
        }
    }
}