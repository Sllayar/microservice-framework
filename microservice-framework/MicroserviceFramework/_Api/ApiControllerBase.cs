using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RFI.MicroserviceFramework._Cryptography;
using RFI.MicroserviceFramework._Environment;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Loggers;
using RFI.MicroserviceFramework._Metrics;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Api
{
    [Produces("application/json")]
    public class ApiControllerBase<TRequest> : ControllerBase where TRequest : IRequest, new()
    {
        public ApiUser ApiUser;

        public TRequest ApiRequest = new TRequest();

        public readonly ApiResponse ApiResponse = new ApiResponse();

        private string responseDataJson;

        private object responseData;


        [HttpPost]
        public ApiResponse EntryPoint([FromBody] ApiSafeData safeData)
        {
            try
            {
                if(ClearDbCache) ApiDbController.ClearCaches();

                DecryptData(safeData);

                POST();
            }
            catch(Exception ex)
            {
                var exception = ex.InnerException ?? ex;

                exception.Log(ApiRequest?.Request_ID.ToString());

                if(exception is ApiException apiException) ApiResponse.SetStatus(apiException.Code, apiException.Detail);
                else ApiResponse.SetStatus(CodeStatus.UnhandledException);
            }
            finally
            {
                if(SaveResponse) ResponseSave();

                Logger.Log(ApiResponse.Status.Code == CodeStatus.Ok, null, new
                {
                    host = SEnv.Hostname,
                    path = ApiRequest.Path,
                    request_id = ApiRequest.Request_ID,
                    request = HJsonMasker.JsonAsSimpleString(ApiRequest.ToMaskedJson()),
                    response = HJsonMasker.JsonAsSimpleString(responseData.ToMaskedJson()),
                    user = ApiUser?.Username,
                    status = ApiResponse.Status.Detail
                });

                SMetrics.CounterApiRequests.Inc(ApiUser?.Username ?? "", Request.Path, ApiResponse.Status.Code, ApiResponse.Status.Detail);
            }

            return ApiResponse;
        }


        [HttpGet, HttpDelete, HttpPatch, HttpPut, HttpHead]
        public ApiResponse Others()
        {
            try
            {
                Logger.Log(false, "Not post method");
                HttpContext.Response.StatusCode = 404;
                ApiResponse.SetStatus(CodeStatus.NotAllowedMethod, "Only POST method allowed");
                return ApiResponse;
            }
            catch
            {
                return null;
            }
        }


        private void DecryptData(ApiSafeData safeData)
        {
            ApiRequest.Path = Request.Path.Value;
            ApiRequest.Method = Request.Method;
            ApiRequest.Version = Request.GetApiVersion();
            ApiRequest.Date = DateTime.Now;

            var username = ApiSsoToken.GetUsernameFromToken(Request.GetHeaderValue("Authorization"));
            ApiUser = new ApiUser(username);
            ApiRequest.User_ID = ApiUser.User_ID;

            if(safeData?.Data == null)
            {
                if(ApiRequest.Path == "/check") return;
                throw new ApiException("Data decoding error");
            }

            ApiRequest.IsTestRequest = SEnv.IsDevelopment && safeData.Signature.IsNullOrEmpty() && safeData.Des.IsNullOrEmpty();

            string decryptedData;

            if(ApiRequest.IsTestRequest)
            {
                decryptedData = safeData.Data.ToString();
            }
            else
            {
                var desDecrypted = Rsa.Decrypt(safeData.Des, SEnv.RfiKeyPrivate);
                if(Rsa.Verify(desDecrypted, safeData.Signature, ApiUser.PublicKey).Not()) throw new ApiException(CodeStatus.SignatureNotValid);
                decryptedData = TripleDes.Decrypt(safeData.Data.ToString(), desDecrypted);
            }

            ApiRequest.PopulateFromJson(decryptedData);

            if(ApiRequest.IsTestRequest && ApiRequest.Request_ID == 0) ApiRequest.Request_ID = Convert.ToInt32(DateTime.Now.ToString("ddhhmmssf"));


            ApiRequest.ValidateAndFormat();

            if(ApiRequest.Path != "/check")
            {
                ExecStoredProcedureNonQuery("api_request_save", new { ApiRequest.User_ID, ApiRequest.Partner_ID, ApiRequest.Request_ID, ApiRequest.Path, ApiRequest.Method, ApiRequest.Version, ApiRequest.Date });
            }
        }

        private ApiSafeData EncryptData(object decrytedData)
        {
            responseData = decrytedData;

            responseDataJson = decrytedData.JsonSerialize(true);

            object encryptedStr = TripleDes.Encrypt(responseDataJson, out var desParameters);

            var desEncrypted = Rsa.Encrypt(desParameters, ApiUser.PublicKey);

            var signStr = Rsa.Sign(desParameters, SEnv.RfiKeyPrivate);

            if(Rsa.Verify(desParameters, signStr, SEnv.RfiKeyPublic).Not()) throw new ApiException(CodeStatus.SignatureNotValid);

            var safeData = new ApiSafeData
            {
                Data = ApiRequest.IsTestRequest ? decrytedData : encryptedStr,
                Signature = ApiRequest.IsTestRequest ? null : signStr,
                Des = ApiRequest.IsTestRequest ? null : desEncrypted
            };

            return safeData;
        }

        private void ResponseSave()
        {
            try
            {
                if(ApiResponse.IsNull() || ApiResponse.Status.IsNull() || ApiResponse.Status.Code.IsNull() || ApiResponse.Status.Code == CodeStatus.DuplicateRequest) return;

                var inResponseJson = responseDataJson.NotEmpty() ? responseDataJson.Substring(0, responseDataJson.Length > 4000 ? 4000 : responseDataJson.Length) : "";

                var inStatusJson = ApiResponse.Status.JsonSerialize();

                ExecStoredProcedureNonQuery("api_response_save", new { ApiRequest.User_ID, ApiRequest.Partner_ID, ApiRequest.Request_ID, inResponseJson, ApiRequest.Path, ApiRequest.Method, ApiRequest.Version, inStatusJson });
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }


        protected virtual bool ClearDbCache => false;

        protected virtual bool SaveResponse => false;

        public virtual void POST() => ApiResponse.SetSafeData(EncryptData(GetData()));

        protected virtual object GetData() => throw new ApiException(CodeStatus.NotAllowedMethod);


        protected T ExecStoredProcedure<T>(string procedureName, object request = null) where T : new() => ApiDbController.ExecStoredProcedure<T>(procedureName, request ?? ApiRequest);

        protected void ExecStoredProcedureNonQuery(string procedureName, object request = null) => ApiDbController.ExecStoredProcedure(procedureName, request ?? ApiRequest);

        protected List<T> ExecSelect<T>(string tableName, bool usePackage = true, object request = null) where T : new() => ApiDbController.ExecSelect<T>(tableName, usePackage, request ?? ApiRequest);
    }
}