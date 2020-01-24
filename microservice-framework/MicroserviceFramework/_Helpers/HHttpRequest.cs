using System;
using Microsoft.AspNetCore.Http;

namespace RFI.MicroserviceFramework._Helpers
{
    public static class HHttpRequest
    {
        public static string GetHeaderValue(this HttpRequest request, string header)
        {
            if(request.Headers[header].ToString().IsNullOrEmpty()) throw new Exception($"Missing header {header}");
            return request.Headers[header];
        }

        public static decimal GetApiVersion(this HttpRequest httpRequest) => httpRequest.GetHeaderValue("api-version").ToDecimal();
    }
}