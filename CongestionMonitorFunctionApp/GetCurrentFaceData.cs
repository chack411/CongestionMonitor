using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace CongestionMonitorFunctionApp
{
    public static class GetCurrentFaceData
    {
        [FunctionName("GetCurrentFaceData")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [CosmosDB("FaceCount", "Result1", ConnectionStringSetting = "AzureCosmosDBFaceCountConnectionString")]
                IEnumerable<object> faceResults)
        {
            return new OkObjectResult(faceResults);
        }
    }
}
