using image_gallery.utils;
using Microsoft.AspNetCore.Mvc;

namespace image_gallery.Controllers;

[ApiController]
[Route("/images")]
public class ImageController : ControllerBase
{
    private readonly IAzureContainerStorageCache AzureContainerStorageCache;

    
    public ImageController(IAzureContainerStorageCache azureContainerStorageCache)
    {
        this.AzureContainerStorageCache = azureContainerStorageCache;
    }


    [HttpGet]
    public Task<List<ContainerFile>> GetImages()
    {
        return this.AzureContainerStorageCache.Get();
    }

    [HttpGet("{uuid}")]
    public Uri GetImage(string uuid)
    {
        return this.AzureContainerStorageCache.GetByUuid(uuid);
    }

    [HttpDelete("{name}")]
    public Task<bool> Delete(string name)
    {
        return this.AzureContainerStorageCache.Delete(name);
    }
}