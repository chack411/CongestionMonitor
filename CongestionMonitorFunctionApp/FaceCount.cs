using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace CongestionMonitorFunctionApp
{
    public static class FaceCount
    {
        [FunctionName("FaceCount")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "FaceCount",
            collectionName: "Result1",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true,
            ConnectionStringSetting = "AzureCosmosDBFaceCountConnectionString")]
            IReadOnlyList<Document> input,
            [SignalR(
            HubName = "congestion",
            ConnectionStringSetting = "AzureSignalRConnectionString")]
            IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
                log.LogInformation("Document: " + input[0].ToString());

                // Send message to SignalR Services.
                foreach (var item in input)
                {
                    await signalRMessages.AddAsync(
                        new SignalRMessage
                        {
                            Target = "faceCountUpdated",
                            Arguments = new[] { item }
                        });
                }
            }
        }
    }
}
