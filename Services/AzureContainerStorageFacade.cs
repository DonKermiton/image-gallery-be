using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using image_gallery.utils;

namespace image_gallery.Services;
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
public interface IAzureContainerStorageFacade
{
    Task<List<ContainerFile>> Get();
    Uri GetByName(string name);
    Task<ContainerFile> Post(IFormFile image);
    Task<bool> Delete(string name);
}



public class AzureContainerStorageFacade : IAzureContainerStorageFacade
{
    private IAzureContainerStorageConnector AzureContainerStorageConnector;
    private readonly IConfig config;
    private List<string> AllowedMimes = new List<string>
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/svg+xml",
        "image/webp",
        "image/tiff",
        "image/heif"
    };

    public AzureContainerStorageFacade(IAzureContainerStorageConnector azureContainerStorageConnector, IConfig config)
    {
        this.AzureContainerStorageConnector = azureContainerStorageConnector;
        this.config = config;
    }

    public async Task<List<ContainerFile>> Get()
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;

        List<ContainerFile> results = new List<ContainerFile>();
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            BlobSasBuilder blobSasBuilder = GetImageLink(blobItem.Name);
            results.Add(new ContainerFile(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri, blobItem.Name));
        }

        return results;
    }

    public Uri GetByName(string name)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        BlobClient blobClient = containerClient.GetBlobClient(name);
        return blobClient.Uri;
    }

    public async Task<ContainerFile> Post(IFormFile image)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var contentType = image.ContentType;
        bool hasCorrectMime = this.AllowedMimes.Contains(contentType);

        if (!hasCorrectMime)
        {
            throw new BadRequestException("Invalid mime type. Check your image and try again");
        }
        
        await using (Stream file = image.OpenReadStream())
        {
            var result = await blobClient.UploadAsync(file, new BlobHttpHeaders { ContentType = contentType }); 
        }

        BlobSasBuilder blobSasBuilder = this.GetImageLink(blobClient.Name);
        
        return new ContainerFile(blobClient.GenerateSasUri(blobSasBuilder).AbsoluteUri, blobClient.Name);
    }

    public async Task<bool> Delete(string uuid)
    {
        BlobContainerClient containerClient = this.AzureContainerStorageConnector.ContainerClient!;
        BlobClient blobClient = containerClient.GetBlobClient(uuid);
        var result = await blobClient.DeleteIfExistsAsync();
        return result;
    }

    private BlobSasBuilder GetImageLink(string name)
    {
        var blobSasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = "images",
            BlobName = name,
            ExpiresOn = DateTime.UtcNow.AddMinutes(5), //Let SAS token expire after 5 minutes.
            Protocol = SasProtocol.Https
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobSasBuilder;
    }
}

public class BadRequestException: System.Exception
{
    public BadRequestException(string message): base(message)
    {
            
    }
}