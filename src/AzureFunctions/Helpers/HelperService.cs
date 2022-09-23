using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions.Helpers;

public class HelperService : IHelperService
{
    private readonly CosmosClient _cosmosClient = null;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ServiceBusClient _serviceBusClient;


    public HelperService(
        ClientWrapper<CosmosClient> cosmosClientWrapper,
        ClientWrapper<BlobServiceClient> blobServiceClientWrapper,
        ClientWrapper<ServiceBusClient> serviceBusClientWrapper)
    {
        _cosmosClient = cosmosClientWrapper.Client;
        _blobServiceClient = blobServiceClientWrapper.Client;
        _serviceBusClient = serviceBusClientWrapper.Client;
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
                ItemResponse<DeliveryOrderDetailsDTO> order = await container.CreateItemAsync<DeliveryOrderDetailsDTO>(orderDetailsDto);

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
                var orderDetailJson = JsonConvert.SerializeObject(orderDetailsDto);
                var blobName = $"{DateTime.Now.ToString("s")}-DeliveryID-{orderDetailsDto.OrderId}";
            
                //init client
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(Constants._BLOB_CONTAINER_NAME);
                await containerClient.CreateIfNotExistsAsync();
             
        
                // get blob client and write
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(orderDetailJson)))
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

    public async Task<bool> SendFailureMessage(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger)
    {
        try
        {
            await using ServiceBusSender sender = _serviceBusClient.CreateSender(Constants._DELIVERY_SERVICE_FAILSAFE_QUEUE_NAME);
        
            //assemble message
            var json = JsonConvert.SerializeObject(orderDetailsDto);
            var message = new ServiceBusMessage(json);

            // Send the message to the queue.
            await sender.SendMessageAsync(message);
            
            logger.LogInformation($"SendFailureMessage: Failsafe message successfully sent!");

            return true;
        }
        catch (Exception e)
        {
            logger.LogError($"SendFailureMessage: Exception: {e.Message}");
            logger.LogWarning($"SendFailureMessage: Failed to send failure message");
            
            return false;
        }
    }
}

