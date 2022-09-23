using Microsoft.Azure.Cosmos;

namespace AzureFunctions.Helpers;

/// <summary>
/// helper wrapper, so that client can be added to DI
/// </summary>
public class CosmosClientWrapper
{
    public CosmosClient CosmosClient { get; set; }
    public bool IsValid => CosmosClient != null;
}
