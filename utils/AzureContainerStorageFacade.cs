using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace image_gallery.utils;
public class ContainerFile
{
    public string Filename { set; get; }
    public string Uri { set; get; }
    public ContainerFile(string uri, string filename)
    {
        this.Uri = uri;
        this.Filename = filename;
    }


}
public interface IAzureContainerStorageCache
{
    Task<List<ContainerFile>> Get();
    Uri GetByUuid(string uuid);
    Task<Response<BlobContentInfo>> Post(Stream image);
    Task<bool> Delete(string uuid);
}



public class AzureContainerStorageFacade : IAzureContainerStorageCache
{
    private IAzureContainerStorageConnector AzureContainerStorageConnector;
    private readonly IConfig config;

    public AzureContainerStorageFacade(IAzureContainerStorageConnector azureContainerStorageConnector, IConfig config)
    {
        this.AzureContainerStorageConnector = azureContainerStorageConnector;
        this.config = config;
    }

    public async Task<List<ContainerFile>> Get()
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        BlobServiceClient blobServiceClient = new BlobServiceClient(this.config.StorageConnectionString);

        List<ContainerFile> results = new List<ContainerFile>();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = "images",
                BlobName = blobItem.Name,
                ExpiresOn = DateTime.UtcNow.AddMinutes(5), //Let SAS token expire after 5 minutes.
                Protocol = SasProtocol.Https
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            results.Add(new ContainerFile(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri, blobItem.Name));
        }

        return results;
    }

    public Uri GetByUuid(string uuid)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        BlobClient blobClient = containerClient.GetBlobClient(uuid);
        return blobClient.Uri;
    }

    public async Task<Response<BlobContentInfo>> Post(Stream image)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var result = await blobClient.UploadAsync(image);
        return result;
    }

    public async Task<bool> Delete(string uuid)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        BlobClient blobClient = containerClient.GetBlobClient(uuid);
        var result = await blobClient.DeleteIfExistsAsync();
        return result;
    }
}