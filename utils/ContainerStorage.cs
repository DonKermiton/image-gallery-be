using Azure.Storage.Blobs;

namespace image_gallery.utils;

public interface IAzureContainerStorageConnector : IStartupTask
{
    BlobContainerClient? ContainerClient { get; }
    Task Execute();
    Task GetCloudContainer(string containerName);
}

public class AzureContainerStorageConnector: IAzureContainerStorageConnector
{
    public BlobContainerClient? ContainerClient { get; private set; }
    private readonly String AzureConnectionString;
    public AzureContainerStorageConnector(IConfig config)
    {
        this.AzureConnectionString = config.StorageConnectionString;
    }
    
    public async Task Execute()
    {
        //todo:: add container name to .env
        await this.GetCloudContainer("images");
    }

    public async Task GetCloudContainer(string containerName)
    {
        BlobServiceClient service = new BlobServiceClient(this.AzureConnectionString);
        BlobContainerClient containerClient = service.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        this.ContainerClient = containerClient;
    }
}