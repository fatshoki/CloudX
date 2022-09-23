using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;

namespace AzureFunctions.Helpers;

/// <summary>
/// helper wrapper, so that client can be added to DI
/// </summary>
public class BlobServiceClientWrapper
{
    public BlobServiceClient BlobServiceClient { get; set; }
    public bool IsValid => BlobServiceClient != null;
}
