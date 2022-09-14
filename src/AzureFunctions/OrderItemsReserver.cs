using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AzureFunctions.Properties;

public static class OrderItemsReserver
{
    private static string _CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=shokistorage1;AccountKey=Q9cGwxBDSZ6T8H8EmJEPQIO9SFkejrdqxejfeWtJv/oZdG2OUrPaZIabbdSfRjgbFrx9BE+zfG/6+AStYMCflQ==;EndpointSuffix=core.windows.net";
    private static string _CONTAINER_NAME = "cloudxcontainer";
    
    [FunctionName("OrderItemsReserver")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        try
        {
           
            string bodyString = await new StreamReader(req.Body).ReadToEndAsync();
            
            //this won't work because i failed to deserialize nested objects. need to annotate model, which i'll skip for now.
            //Order order = JsonConvert.DeserializeObject<Order>(bodyString);
            
            var order = (JObject)JsonConvert.DeserializeObject(bodyString);
            
            var id = order.GetValue(nameof(Order.Id)).ToString();
            var buyerId = order.GetValue(nameof(Order.BuyerId)).ToString();
            
            
            var message = $"OrderItemsReserver: order received: {id} by user {buyerId}";

            var blobName = $"{buyerId}.{id}.{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}.json";
            await WriteDataToBlob(blobName, bodyString, log);
            
            
            log.LogInformation(message);
            return new OkObjectResult(message);
        }
        catch (Exception e)
        {
            log.LogError($"OrderItemsReserver: order not sent: {e}");
            return new BadRequestObjectResult("OrderItemsReserver: error");
        }

    }

    private static async Task WriteDataToBlob(string blobName, string data, ILogger logger)
    {
        try
        {
            //init client
            BlobServiceClient blobServiceClient = new BlobServiceClient(_CONNECTION_STRING);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_CONTAINER_NAME);
            await containerClient.CreateIfNotExistsAsync();
        
        
            // get blob client and write
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
            {
                await blobClient.UploadAsync(stream);    
            }
            logger.LogInformation($"WriteDataToBlob: success");
        }
        catch (Exception e)
        {
            logger.LogError($"WriteDataToBlob: whoopsie: {e}");
            throw;
        }
        
        
    }
}
