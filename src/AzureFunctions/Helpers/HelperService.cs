using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
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
    private readonly BlobServiceClient _blobServiceClient;


    public HelperService(CosmosClientWrapper cosmosClientWrapper, BlobServiceClientWrapper blobServiceClientWrapper)
    {
        _cosmosClient = cosmosClientWrapper.CosmosClient;
        _blobServiceClient = blobServiceClientWrapper.BlobServiceClient;
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
    
    
    public async Task<bool> WriteOrderDetailsToBlobStorageAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                var orderModel = new OrderModel(orderDetailsDto);
                var orderModelJson = JsonConvert.SerializeObject(orderModel);
                var blobName = $"{DateTime.Now.ToString("s")}-DeliveryID-{orderModel.OrderId}";
            
                //init client
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(Constants._BLOB_CONTAINER_NAME);
                await containerClient.CreateIfNotExistsAsync();
             
        
                // get blob client and write
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(orderModelJson)))
                {
                    await blobClient.UploadAsync(stream);    
                }
                
                logger.LogInformation($"WriteOrderDetailsToBlobStorageAsync: success!");

                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"WriteOrderDetailsToBlobStorageAsync: exception: {e}");
                logger.LogError($"WriteOrderDetailsToBlobStorageAsync: Retries left: {retries - 1 - i}");
            }
        }

        logger.LogError($"WriteOrderDetailsToBlobStorageAsync: Failed writing to Blob after {retries} retries");
        
        return false;
        
    }
    
    
    
}

