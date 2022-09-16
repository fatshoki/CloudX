using System;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Newtonsoft.Json;

namespace AzureFunctions.Properties.Models;

public class OrderModel
{
    public OrderModel()
    {
    }

    public OrderModel(DeliveryOrderDetailsDTO deliveryOrderDetailsDto)
    {
        OrderId = deliveryOrderDetailsDto.Id;
        Address = deliveryOrderDetailsDto.Address;
        Items = deliveryOrderDetailsDto.Items;
        FinalPrice = deliveryOrderDetailsDto.FinalPrice;
    }

    [JsonProperty("id")] 
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public int OrderId { get; }
    public string Address { get; }
    public string Items { get;}
    public decimal FinalPrice { get; }

}
