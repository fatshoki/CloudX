using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EShopOnWebAzureFunctions;
using EShopOnWebAzureFunctions.Properties.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions.Helpers;

public class HelperService : IHelperService
{
    private readonly CosmosClient _cosmosClient = null;

    public HelperService(CosmosClientWrapper cosmosClientWrapper)
    {
        _cosmosClient = cosmosClientWrapper.CosmosClient;
    }
    public DeliveryOrderDetailsDTO GetDeliveryOrderDetailsDtoFromJson(string json)
    {
        //sanitize json string
        var jsonString = Regex.Replace(json, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
        var orderDetailsDto = JsonConvert.DeserializeObject<DeliveryOrderDetailsDTO>(jsonString);
        return orderDetailsDto;
    }


    public async Task<bool> WriteOrderDetailsToCosmosDbAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                //create client
                //CosmosClient client = new CosmosClient(Constants._COSMOS_DB_CONNECTION_STRING);
                
                //get database, create if not exist
                //Database database = await client.CreateDatabaseIfNotExistsAsync(Constants._COSMOS_DATABASE_ID);
                Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(Constants._COSMOS_DATABASE_ID);

                //get container, create if doesn't exist
                Container container = await database.CreateContainerIfNotExistsAsync(Constants._COSMOS_CONTAINER_ID, Constants._COSMOS_NEW_ORDERS_PARTITION);

                //finally, add item to container
                var orderModel = new OrderModel(orderDetailsDto);
                ItemResponse<OrderModel> order = await container.CreateItemAsync<OrderModel>(orderModel);

                logger.LogInformation($"WriteOrderDetailsToCosmosDbAsync: success: new itm: {order.Resource.Id}");

                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"WriteOrderDetailsToCosmosDbAsync: exception: {e}");
                logger.LogError($"WriteOrderDetailsToCosmosDbAsync: Retries left: {retries - 1 - i}");
            }
        }

        logger.LogError($"WriteOrderDetailsToCosmosDbAsync: Failed writing to DB after {retries} retries");
        
        return false;
    }
    
    public static async Task<bool> WriteOrderDetailsToBlobStorageAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                
                
                
                
                //create client
                CosmosClient client = new CosmosClient(Constants._COSMOS_DB_CONNECTION_STRING);

                //get database, create if not exist
                Database database = await client.CreateDatabaseIfNotExistsAsync(Constants._COSMOS_DATABASE_ID);

                //get container, create if doesn't exist
                Container container = await database.CreateContainerIfNotExistsAsync(Constants._COSMOS_CONTAINER_ID, Constants._COSMOS_NEW_ORDERS_PARTITION);

                //finally, add item to container
                var orderModel = new OrderModel(orderDetailsDto);
                ItemResponse<OrderModel> order = await container.CreateItemAsync<OrderModel>(orderModel);

                logger.LogInformation($"WriteOrderDetailsToCosmosDbAsync: success: new itm: {order.Resource.Id}");

                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"WriteOrderDetailsToCosmosDbAsync: exception: {e}");
                logger.LogError($"WriteOrderDetailsToCosmosDbAsync: Retries left: {retries - 1 - i}");
            }
        }

        logger.LogError($"WriteOrderDetailsToCosmosDbAsync: Failed writing to DB after {retries} retries");
        
        return false;
    }
}

