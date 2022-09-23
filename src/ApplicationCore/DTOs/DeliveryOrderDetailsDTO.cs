using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.DTOs;

/// <summary>
/// Model for Delivery/shipping service
/// PLEASE NOTE: this is only for demo purposes 
/// </summary>
public class DeliveryOrderDetailsDTO
{
    public DeliveryOrderDetailsDTO()
    {
    }
    
    public DeliveryOrderDetailsDTO(Order o)
    {
        OrderId = o.Id;
        Address = o.ShipToAddress.ToString();
        Items = o.OrderItems.Select(i => new DeliveryItemDTO() { Id = i.ItemOrdered.CatalogItemId, Price = i.UnitPrice, Quantity = i.Units }).ToList();
    }
    
    [JsonProperty("id")] 
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int OrderId { get; set; }
    public string Address { get; set; }
    public List<DeliveryItemDTO> Items { get; set; }

    [JsonIgnore] 
    public decimal FinalPrice => Items.Sum(i => i.Quantity * i.Price);

}
