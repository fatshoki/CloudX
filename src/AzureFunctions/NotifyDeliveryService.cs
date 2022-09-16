using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFunctions.Properties.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//using System.Text.Json;

namespace AzureFunctions.Properties;

public static class NotifyDeliveryService
{
    //these things would NOT be hardcoded here :D
    private const string _COSMOS_DB_CONNECTION_STRING = "AccountEndpoint=https://shoki-cloudx-eshoponweb.documents.azure.com:443/;AccountKey=b7BxgT3cYPF3XmSrNbepJVi4WsVBqdBAhovwMNYu6uREz2cndwWQTd5QXt02WoVJoXBCodeT8y7eUJK6UIvhBw==;";
    
    private const string _COSMOS_DATABASE_ID = "DeliveryService";
    private const string _COSMOS_CONTAINER_ID = "Orders";
    private const string _COSMOS_NEW_ORDERS_PARTITION = "/Unfulfilled";
    
    
    
    [FunctionName("NotifyDeliveryService")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        try
        {
           
            string bodyString = await new StreamReader(req.Body).ReadToEndAsync();
            
            //sanitize json string
            var jsonString = Regex.Replace(bodyString, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
            
            var orderDetails = JsonConvert.DeserializeObject<DeliveryOrderDetailsDTO>(jsonString);

            await WriteOrderDetailsToCosmosDbAsync(orderDetails, log);

            var msg = $"NotifyDeliveryService: Order with id {orderDetails.Id} processed successfully";
            log.LogInformation(msg);
            return new OkObjectResult(msg);
        }
        catch (Exception e)
        {
            log.LogError($"NotifyDeliveryService: error: {e}");
            return new BadRequestObjectResult($"NotifyDeliveryService: error: {e}");
        }
    }
    
    private static async Task WriteOrderDetailsToCosmosDbAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger)
    {
        try
        {
            //create client
            CosmosClient client = new CosmosClient(_COSMOS_DB_CONNECTION_STRING);
            
            //get database, create if not exist
            Database database = await client.CreateDatabaseIfNotExistsAsync(_COSMOS_DATABASE_ID);
            
            //get container, create if doesn't exist
            Container container = await database.CreateContainerIfNotExistsAsync(_COSMOS_CONTAINER_ID, _COSMOS_NEW_ORDERS_PARTITION);
            
            //finally, add item to container
            var orderModel = new OrderModel(orderDetailsDto);
            ItemResponse<OrderModel> order = await container.CreateItemAsync<OrderModel>(orderModel);
            
            
            logger.LogInformation($"WriteOrderDetailsToCosmosDbAsync: success: new itm: {order.Resource.Id}");
        }
        catch (Exception e)
        {
            logger.LogError($"WriteOrderDetailsToCosmosDbAsync: exception: {e}");
            throw;
        }
        
        
    }
}
