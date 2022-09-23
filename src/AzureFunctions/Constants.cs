using System;

namespace AzureFunctions;

public static class Constants
{
    //CosmosDB stuff
    public static readonly string _COSMOS_DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbConnectionString");
    public static readonly string _COSMOS_DATABASE_ID = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbDatabaseId");
    public static readonly string _COSMOS_CONTAINER_ID = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbContainerId");
    public static readonly string _COSMOS_NEW_ORDERS_PARTITION = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbOrdersPartition");
    
    //Blob stuff
    public static readonly string _BLOB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DeliveryServiceBlobStorageConnectionString");
    public static readonly string _BLOB_CONTAINER_NAME = Environment.GetEnvironmentVariable("DeliveryServiceBlobStorageContainerName");
    
    //ServiceBus stuff
    public const string _SERVICE_BUS_CONN_STRING_APP_SETTING_NAME = "DeliveryServiceServiceBusConnectionString";    //name of the env variable - used in function definition
    public static readonly string _SERVICE_BUS_CONNECTION_STRING = Environment.GetEnvironmentVariable(_SERVICE_BUS_CONN_STRING_APP_SETTING_NAME);   //actual connection string
    public const string _DELIVERY_SERVICE_QUEUE_NAME = "cloudx-eshoponweb-deliveryservice";
    public const string _DELIVERY_SERVICE_FAILSAFE_QUEUE_NAME = "cloudx-eshoponweb-deliveryservice-failsafe";
}
