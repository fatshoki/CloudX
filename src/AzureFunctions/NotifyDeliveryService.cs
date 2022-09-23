using System;
using System.Threading.Tasks;
using AzureFunctions.Helpers;
using EShopOnWebAzureFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class NotifyDeliveryService
{
    private readonly IHelperService _helperService;

    public NotifyDeliveryService(IHelperService helperService)
    {
        _helperService = helperService;
    }
    
    [FunctionName("NotifyDeliveryService")]
    public async Task RunAsync(
        [ServiceBusTrigger(Constants._DELIVERY_SERVICE_QUEUE_NAME, Connection = Constants._SERVICE_BUS_CONN_STRING_APP_SETTING_NAME)] 
        string orderItem, 
        ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {orderItem}");
        try
        {
            var orderDetails = _helperService.GetDeliveryOrderDetailsDtoFromJson(orderItem);

            //writing to CosmosDB
            if (await _helperService.WriteOrderDetailsToBlobStorageAsync(orderDetails, log, 3))
            {
                log.LogInformation($"NotifyDeliveryServiceQueue: Posting order details: Success!");
                return;
            }

            // //writing to CosmosDB
            // if (await _helperService.WriteOrderDetailsToCosmosDbAsync(orderDetails, log, 3))
            // {
            //     log.LogInformation($"NotifyDeliveryServiceQueue: Posting order details: Success!");
            //     return;
            // }
            //
            
            
            //if we came to here, notify failsafe queue
            log.LogWarning($"NotifyDeliveryServiceQueue: Error while posting Order details - running failsafe");
            _helperService.SendFailureMessage(orderDetails, log);

        }
        catch (Exception e)
        {
            log.LogError($"NotifyDeliveryServiceQueue: Exception: {e}");
        }
        
    }
}
