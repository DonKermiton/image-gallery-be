using image_gallery.utils;
using Microsoft.AspNetCore.Mvc;

namespace image_gallery.Controllers;

[ApiController]
[Route("/images")]
public class ImageController : ControllerBase
{
    private readonly IAzureContainerStorageFacade AzureContainerStorageCache;

    
    public ImageController(IAzureContainerStorageFacade azureContainerStorageCache)
    {
        this.AzureContainerStorageCache = azureContainerStorageCache;
    }


    [HttpGet]
    public Task<List<ContainerFile>> GetImages()
    {
        return this.AzureContainerStorageCache.Get();
    }

    [HttpGet("{name}")]
    public ActionResult<Uri> GetImage(string name)
    {
        return Ok(this.AzureContainerStorageCache.GetByUuid(name));
    }

    [HttpDelete("{name}")]
    public OkObjectResult Delete(string name)
    {
        return Ok(this.AzureContainerStorageCache.Delete(name));
    }

    [HttpPost]
    public async Task<IActionResult> Post(IFormFile image)
    {
        try
        {
            var result = await this.AzureContainerStorageCache.Post(image);
            return Ok(result);
        }
        catch (BadRequestException err)
        {
            return BadRequest(err.Message);
        }
    }
}