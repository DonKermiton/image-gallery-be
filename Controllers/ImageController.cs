using System.Text.Json;
using System.Text.Json.Serialization;
using image_gallery.Services;
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
    public async Task<ActionResult<Uri>> GetImage(string name)
    {
        return Ok(this.AzureContainerStorageCache.GetByName(name));
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        try
        {
            bool result = await this.AzureContainerStorageCache.Delete(name);
            return Ok(new {success = result, message = result ? "Delete successful." : "Delete failed."});
        }  catch (BadRequestException err)
        {
            return BadRequest(err.Message);
        } 
    }

    [HttpPost]
    public async Task<IActionResult> Post(List<IFormFile> images)
    {
        try
        {
            List<ContainerFile> result = new List<ContainerFile>();
            foreach (var image in images)
            {
                var e = await this.AzureContainerStorageCache.Post(image);
                result.Add(e);
            }
            
            return Ok(result);
        }
        catch (BadRequestException err)
        {
            Console.WriteLine(err);
            return BadRequest(err.Message);
        }
    }
}