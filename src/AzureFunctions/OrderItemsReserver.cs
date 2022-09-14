using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
    [FunctionName("OrderItemsReserver")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, 
                                                     ILogger log)
    {
        try
        {
            //test if im sending the correct json
            // req.Body.Position = 0;
            // var reader = new StreamReader(req.Body, Encoding.UTF8);
            // var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            // req.Body.Position = 0;
            // log.LogInformation($"*** {body}");
            
            string bodyString = await new StreamReader(req.Body).ReadToEndAsync();
            
            //this won't work because i failed to deserialize nested objects. need to annotate model, which i'll skip for now.
            //Order order = JsonConvert.DeserializeObject<Order>(bodyString);
            
            var order = (JObject)JsonConvert.DeserializeObject(bodyString);
            
            var id = order.GetValue(nameof(Order.Id)).ToString();
            var buyerId = order.GetValue(nameof(Order.BuyerId)).ToString();
            
            
            var message = $"OrderItemsReserver: order received: {id} by user {buyerId}";
            
            
            log.LogInformation(message);
            return new OkObjectResult(message);
        }
        catch (Exception e)
        {
            log.LogError("OrderItemsReserver: order not sent");
            return new BadRequestObjectResult($"OrderItemsReserver: bad order: {e}");
        }
        

        
        
        string name = req.Query["name"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name = name ?? data?.name;

        return name != null
            ? (ActionResult)new OkObjectResult($"Hello, {name}")
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        
    }
}
