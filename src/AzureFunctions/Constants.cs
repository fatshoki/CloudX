using System;

namespace AzureFunctions;

public static class Constants
{
    //CosmosDB stuff
    public static string _COSMOS_DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbConnectionString");//"AccountEndpoint=https://shoki-cloudx-eshoponweb.documents.azure.com:443/;AccountKey=b7BxgT3cYPF3XmSrNbepJVi4WsVBqdBAhovwMNYu6uREz2cndwWQTd5QXt02WoVJoXBCodeT8y7eUJK6UIvhBw==;";
    public static string _COSMOS_DATABASE_ID = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbDatabaseId");//"DeliveryService";
    public static string _COSMOS_CONTAINER_ID = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbContainerId");//"Orders";
    public static string _COSMOS_NEW_ORDERS_PARTITION = Environment.GetEnvironmentVariable("DeliveryServiceCosmosDbOrdersPartition");//"/Unfulfilled";
    
    //Blob stuff
    public static string _BLOB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DeliveryServiceBlobStorageConnectionString");
    public static string _BLOB_CONTAINER_NAME = Environment.GetEnvironmentVariable("DeliveryServiceBlobStorageContainerName");
    

    //ServiceBus stuff
    public const string _SERVICE_BUS_CONN_STRING_APP_SETTING_NAME = "DeliveryServiceServiceBusConnectionString";
    public const string _QUEUE_NAME = "cloudx-eshoponweb-deliveryservice";
}
