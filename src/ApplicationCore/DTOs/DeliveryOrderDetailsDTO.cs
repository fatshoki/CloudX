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
        Id = o.Id;
        Address = o.ShipToAddress.ToString();
        Items = string.Join(',', o.OrderItems.Select(i => i.ToString()));
        FinalPrice = o.OrderItems.Select(item => item.Units * item.UnitPrice).Sum();
    }
    
    public int Id { get; set; }
    public string Address { get; set; }
    public string Items { get; set;}
    public decimal FinalPrice { get; set;}
    
}
