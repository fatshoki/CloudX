using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Queries;
using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.eShopWeb.Infrastructure.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderItemsReserver, OrderItemsReserver>();
        services.AddScoped<IBasketQueryService, BasketQueryService>();
        services.AddSingleton<IUriComposer>(new UriComposer(configuration.Get<CatalogSettings>()));
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        services.AddTransient<IEmailSender, EmailSender>();
        
        //add azure services
        services.AddAzureClients(clientsBuilder =>
        {
            clientsBuilder.AddServiceBusClient(configuration["DeliveryServiceServiceBusConnectionString"])
                .WithName(configuration["DeliveryServiceServiceBusClientName"])
                .ConfigureOptions(options =>
                {
                    options.RetryOptions.Delay = TimeSpan.FromMilliseconds(50);
                    options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(5);
                    options.RetryOptions.MaxRetries = 3;
                });
        });

        return services;
    }
}
