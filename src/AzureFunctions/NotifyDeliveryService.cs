using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

//using System.Text.Json;

namespace AzureFunctions;

public class NotifyDeliveryService
{
    private readonly IHelperService _helperService;

    public NotifyDeliveryService(IHelperService helperService)
    {
        _helperService = helperService;
    }
    
    [FunctionName("NotifyDeliveryService")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        try
        {
           
            string bodyString = await new StreamReader(req.Body).ReadToEndAsync();
            
            var orderDetails = _helperService.GetDeliveryOrderDetailsDtoFromJson(bodyString);

            await _helperService.WriteOrderDetailsToCosmosDbAsync(orderDetails, log);

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
    
    
}
