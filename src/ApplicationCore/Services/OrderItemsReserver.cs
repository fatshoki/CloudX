using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderItemsReserver : IOrderItemsReserver
{
    //todo: move this to appsettings
    private readonly string _DELIVERY_SERVICE_FUNCTION_URL = "https://shoki-cloudx-eshoponweb.azurewebsites.net/api/NotifyDeliveryService";

    private readonly IAppLogger<OrderItemsReserver> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ServiceBusClient _serviceBusClient;

    public OrderItemsReserver(IAppLogger<OrderItemsReserver> logger, 
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _serviceBusClient = serviceBusClientFactory.CreateClient(_configuration["DeliveryServiceServiceBusClientName"]);
    }
    
    //public entry poing
    public async Task<bool> PostOrderDetailsAsync(Order order)
    {
        //return await PostOrderDetailsAzureFunctionAsync(order);
        return await PostOrderDetailsToQueueAsync(order);
    }
    
    /// <summary>
    /// ServiceBus implementation - send message to service bus
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private async Task<bool> PostOrderDetailsToQueueAsync(Order order)
    {
        try
        {
            await using ServiceBusSender sender = _serviceBusClient.CreateSender(_configuration["DeliveryServiceQueueName"]);
        
            //assemble message
            var json = GetDeliveryOrderDtoJson(order);
            var message = new ServiceBusMessage(json);

            // Send the message to the queue.
            await sender.SendMessageAsync(message);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Exception: {e.Message}");
            return false;
        }
        
    }
    
    /// <summary>
    /// AzureFunction implementation - directly call Azure function
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    [Obsolete]
    private async Task<bool> PostOrderDetailsAzureFunctionAsync(Order order)
    {
        //get client
        var httpClient = _httpClientFactory.CreateClient();
        
        //assemble message
        var json = GetDeliveryOrderDtoJson(order);
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _DELIVERY_SERVICE_FUNCTION_URL)
        {
            Content = new StringContent(json) 
        };

        //send
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        
        return httpResponseMessage.IsSuccessStatusCode;
    }

    /// <summary>
    /// private helper. convert Order to OrderDTO and serialize it to JSON
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private string GetDeliveryOrderDtoJson(Order order)
    {
        //assemble message
        var postData = new DeliveryOrderDetailsDTO(order);
        var json = JsonSerializer.Serialize(postData);
        return json;
    }
}
