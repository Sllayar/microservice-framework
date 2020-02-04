using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace RFI.MicroserviceFramework._Api.Controllers.service
{
    [Route("/service")]
    public class ServiceController : ApiControllerBase<ServiceRequest>
    {
        private readonly IHostApplicationLifetime ApplicationLifetime;
        public ServiceController(IHostApplicationLifetime applicationLifetime) => ApplicationLifetime = applicationLifetime;


        protected override object GetData()
        {
            if(ApiUser.User_ID != 1) throw new ApiException(CodeStatus.NotAllowedMethod);
            var method = GetType().GetMethod(ApiRequest.Command) ?? throw new ApiException(CodeStatus.NotAllowedMethod);
            return method.Invoke(this, new object[] { });
        }

        public object Info() => ApiInfo.Get();
        

        public void RestartApi() => ApplicationLifetime.StopApplication();
    }
}
