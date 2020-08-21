using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosDBLib
{
    public class FaceCountDB
    {
        private readonly string Endpoint;
        private readonly string Key;
        private readonly string DatabaseId;
        private readonly string ContainerId;

        private Container container = null;

        public FaceCountDB(string endpoint, string key, string databaseId, string containerId)
        {
            Endpoint = endpoint;
            Key = key;
            DatabaseId = databaseId;
            ContainerId = containerId;

            this.Initialize().Wait();
        }

        private async Task Initialize()
        {
            if (container != null)
                return;

            try
            {
                CosmosClient cosmos = new CosmosClient(Endpoint, Key);

                // Create the database - this can also be done through the portal
                Database database = await cosmos.CreateDatabaseIfNotExistsAsync(DatabaseId);

                // Create the container - make sure to specify the RUs - has pricing implications
                // This can also be done through the portal
                container = await database.CreateContainerIfNotExistsAsync(
                    ContainerId,
                    "/_partitionKey",
                    400);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public bool IsInitialized()
        {
            return (container != null);
        }

        public async Task<List<FaceItem>> GetResultItemsByPlace(string placeName)
        {
            var faces = new List<FaceItem>();

            if (container == null)
                return faces;

            QueryDefinition query = new QueryDefinition("SELECT * FROM p WHERE p.placeName = @place")
                .WithParameter("@place", placeName);

            using (FeedIterator<FaceItem> resultSetIterator = container.GetItemQueryIterator<FaceItem>(query))
            {
                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<FaceItem> response = await resultSetIterator.ReadNextAsync();
                    faces.AddRange(response);
                }
            }

            return faces;
        }

        public async Task CreateFaceItem(FaceItem item)
        {
            if (container == null)
                return;

            await container.CreateItemAsync<FaceItem>(item);
        }

        public async Task ReplaceFaceItem(FaceItem item)
        {
            if (container == null)
                return;

            await container.ReplaceItemAsync<FaceItem>(item, item.Id);
        }

        //public async Task DeleteResultItem(FaceItem item)
        //{
        //    if (container == null)
        //        return;

        //    await container.DeleteItemAsync<FaceItem>(item.Id, new PartitionKey(item.PlaceName));
        //}

        public async void UpdateFaceCount(long faceCount, long maskCount, string placeName)
        {
            List<FaceItem> items = await GetResultItemsByPlace(placeName);
            if (items.Count == 0)
            {
                var item = new FaceItem()
                {
                    Id = string.Format("{0:10}_{1}", DateTime.Now.Ticks, Guid.NewGuid()),
                    FaceCount = faceCount,
                    MaskCount = maskCount,
                    PlaceName = placeName,
                    RecordDateTime = DateTime.Now.ToString()
                };
                await CreateFaceItem(item);
            }
            else
            {
                items[0].FaceCount = faceCount;
                items[0].MaskCount = maskCount;
                items[0].RecordDateTime = DateTime.Now.ToString();
                await ReplaceFaceItem(items[0]);
            }
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FaceItem
    {
        public string Id { get; set; }

        public long FaceCount { get; set; }

        public long MaskCount { get; set; }

        public string PlaceName { get; set; }

        public string RecordDateTime { get; set; }
    }
}
