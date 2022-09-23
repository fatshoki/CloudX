using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Helpers;

public interface IHelperService
{
    public DeliveryOrderDetailsDTO GetDeliveryOrderDetailsDtoFromJson(string json);
    public Task<bool> WriteOrderDetailsToCosmosDbAsync(DeliveryOrderDetailsDTO orderDetailsDto, ILogger logger, int retries = 1);
}
