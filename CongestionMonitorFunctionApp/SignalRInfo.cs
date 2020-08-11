using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace CongestionMonitorFunctionApp
{
    public static class SignalRInfo
    {
        [FunctionName("GetSignalRInfo")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "congestions")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}
