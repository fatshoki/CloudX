using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;

    //todo: move this to appsettings
    private readonly string _DELIVERY_SERVICE_FUNCTION_URL = "https://shoki-cloudx-eshoponweb.azurewebsites.net/api/NotifyDeliveryService";

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer,
        IHttpClientFactory httpClientFactory)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _httpClientFactory = httpClientFactory;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        Guard.Against.NullBasket(basketId, basket);
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);
         
        await _orderRepository.AddAsync(order);
        
        await PostOrderDetailsAsync(order);
}

    private async Task<bool> PostOrderDetailsAsync(Order order)
    {
        //get client
        var httpClient = _httpClientFactory.CreateClient();
        
        //assemble message
        var postData = new DeliveryOrderDetailsDTO(order);
        var json = JsonSerializer.Serialize(postData);
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _DELIVERY_SERVICE_FUNCTION_URL)
        {
            Content = new StringContent(json) 
        };

        //send
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        
        return httpResponseMessage.IsSuccessStatusCode;

    }
}
