using System;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using AzureFunctions;
using AzureFunctions.Helpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //add cosmos client to DI container
            builder.Services.AddSingleton(s =>
            {
                try
                {
                    return new ClientWrapper<CosmosClient>(new CosmosClient(Constants._COSMOS_DB_CONNECTION_STRING));
                }
                catch (Exception e)
                {
                    return new ClientWrapper<CosmosClient>();
                }
            });
            
            //add blob client to DI container
            builder.Services.AddSingleton(s =>
            {
                try
                {
                    return new ClientWrapper<BlobServiceClient>(new BlobServiceClient(Constants._BLOB_CONNECTION_STRING));
                }
                catch (Exception e)
                {
                    return new ClientWrapper<BlobServiceClient>();
                }
            });
            
            //add serviceBus client to DI container
            builder.Services.AddSingleton(s =>
            {
                try
                {
                    return new ClientWrapper<ServiceBusClient>(new ServiceBusClient(Constants._SERVICE_BUS_CONNECTION_STRING));
                }
                catch (Exception e)
                {
                    return new ClientWrapper<ServiceBusClient>();
                }
            });

            //add my helper service
            builder.Services.AddScoped<IHelperService, HelperService>();


            // var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            // if (string.IsNullOrEmpty(serviceBusConnectionString))
            // {
            //     throw new InvalidOperationException(
            //         "Please specify a valid ServiceBusConnectionString in the Azure Functions Settings or your local.settings.json file.");
            // }
            //
            // //using AMQP as transport
            // builder.Services.AddSingleton((s) => {
            //     return new ServiceBusClient(serviceBusConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }); 
            // });

        }
    }
}
