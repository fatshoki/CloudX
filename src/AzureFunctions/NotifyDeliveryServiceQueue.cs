using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public static class NotifyDeliveryServiceQueue
{
    private const string _CONNECTION_APP_SETTING_NAME = "DeliveryServiceServiceBusConnectionString";
    private const string _QUEUE_NAME = "cloudx-eshoponweb-deliveryservice";
    
    [FunctionName("NotifyDeliveryServiceQueue")]
    public static async Task RunAsync(
        [ServiceBusTrigger(_QUEUE_NAME, Connection = _CONNECTION_APP_SETTING_NAME)] 
        string orderItem, 
        ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {orderItem}");
        
    }
}
