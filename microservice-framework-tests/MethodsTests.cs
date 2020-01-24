using System;
using RFI._Helpers;
using RFI.MicroserviceFramework._Api;
using RFI.MicroserviceFramework._Api.Controllers.service;
using RFI.MicroserviceFramework._Helpers;
using Xunit;
using Xunit.Abstractions;

namespace RFI
{
    public class MethodsTests
    {
        private const bool FullTest = false;

        public MethodsTests(ITestOutputHelper outputHelper)
        {
            Helpers.LoadEnvironment();

            TestsLogger.Initialize(outputHelper);

            TestsLogger.WriteLine("-----------------------------------------------");
            TestsLogger.WriteLine("BEGIN TEST");

            //apiUser = new ApiUser(ChainLogger, "rfi_api_test"); // slow
            apiUser = new ApiUser{ User_ID = 1, Username = "rfi_api_test", PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzUD6oowGBe+IFX3qq9qH+go+y/zj3PxOTL6aHPtrKdfGPrXrUlSej60GyBeQFPclG+Su8i162X8d67PqQUVFEiwjOu3mS5qcNeJhH3aCqDXScmKYEbQpilelxZ1nuowXKHmA9KdkrVLvHvOzyrV2RhvwH80yOtJNOYf/7ecKg2FnirMQfOFiquSudBckVd6meyew0ylX0PdSpFeQ7bD92/L+OdMrOLxNq8tbwReEGKo/PYPNU/t1+PMrfEUpOYVShQNsMjHJ+tCkMB2Ji8W7DZRdWxIMagbgKS6Zyup7k6azpJyHYfB4oB/Bmhie3l7pw8anCVHaF+9xVU1jAeHQvQIDAQAB" };
        }

        
        private readonly ApiUser apiUser;

        private ApiResponse GetSafeResponse<TRequest>(ApiControllerBase<TRequest> controller, TRequest safeApiRequest) where TRequest : class, IRequest, new()
        {
            controller.ApiUser = apiUser;

            controller.ApiRequest = safeApiRequest;
            controller.ApiRequest.Request_ID = HRnd.Next(99999999, 999999999);
            controller.ApiRequest.User_ID = 1;
            controller.ApiRequest.Partner_ID = 1;
            controller.ApiRequest.IsTestRequest = true;

            controller.ApiRequest.ValidateAndFormat();

            TestsLogger.WriteLine("-----------------------------------------------");
            TestsLogger.WriteLine("REQUEST:");
            TestsLogger.WriteLine(controller.ApiRequest.JsonSerialize(true));

            var startDateTime = DateTime.Now;
            try
            {
                var method = controller.GetType().GetMethod("POST");
                if(method == null) throw new Exception("Method not found");
                method.Invoke(controller, new object[] { });
            }
            catch(Exception ex)
            {
                if(ex.InnerException is ApiException exInner)
                {
                    controller.ApiResponse.SetStatus(exInner.Code, exInner.Detail);
                    TestsLogger.WriteLine($"Warning: {ex.InnerException.Message}");
                }
                else
                {
                    controller.ApiResponse.SetStatus(CodeStatus.UnhandledException);
                    TestsLogger.WriteLine($"Exception: {ex.InnerException.Message}");
                    TestsLogger.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
                }
            }

            var totalMs = Math.Round((DateTime.Now - startDateTime).TotalMilliseconds);

            TestsLogger.WriteLine("-----------------------------------------------");
            TestsLogger.WriteLine("RESPONSE:");
            TestsLogger.WriteLine(controller.ApiResponse.JsonSerialize(true));
            TestsLogger.WriteLine("-----------------------------------------------");
            TestsLogger.WriteLine($"TEST DONE IN {totalMs}ms");
            TestsLogger.WriteLine("-----------------------------------------------");

            return controller.ApiResponse;
        }

        private bool TestControllerStatusCode<TRequest>(ApiControllerBase<TRequest> controller, TRequest safeApiRequest, bool onlyInFullTest = false) where TRequest : class, IRequest, new()
        {
            if(onlyInFullTest && FullTest.Not())
            {
                TestsLogger.WriteLine("TEST SKIPPED");
                Assert.True(true);
                return true;
            }

            var safeResponse = GetSafeResponse(controller, safeApiRequest);
            return safeResponse.Status.Code == (int)CodeStatus.Ok;
        }
        
        [Fact(DisplayName = "/card/cvc2")]
        public void ServiceInfo() => Assert.True(TestControllerStatusCode(new ServiceController(null), new ServiceRequest
        {
            Command = "Info"
        }, true));

    }
}