using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Helpers;

public interface IHelperService
{
    /// <summary>
    /// Helper. Deserializes DeliveryOrderDetailDTO object from JSON
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public DeliveryOrderDetailsDTO GetDeliveryOrderDetailsDtoFromJson(string json);
    
    /// <summary>
    /// Write order details to CosmosDB 
    /// </summary>
    /// <param name="orderDetailsDto"></param>
    /// <param name="logger"></param>
    /// <param name="retries"></param>
    /// <returns></returns>
    public Task<bool> WriteOrderDetailsToCosmosDbAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1);
    
    /// <summary>
    /// Write order details to Blob storage
    /// </summary>
    /// <param name="orderDetailsDto"></param>
    /// <param name="logger"></param>
    /// <param name="retries"></param>
    /// <returns></returns>
    public Task<bool> WriteOrderDetailsToBlobStorageAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1);
    
    /// <summary>
    /// Failsafe mechanism, posts order details to failsafe queue, to be handled by different function
    /// </summary>
    /// <param name="orderDetailsDto"></param>
    /// <returns></returns>
    public Task<bool> SendFailureMessage(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger);
}
