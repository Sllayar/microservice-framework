using Microsoft.AspNetCore.Mvc;

namespace RFI.MicroserviceFramework._Api.Controllers.check
{
    [Route("check")]
    public class CheckController : ApiControllerBase<CheckRequest>
    {
        public override void POST()
        {
            // do nothing
        }
    }
}