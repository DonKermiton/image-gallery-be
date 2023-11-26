using image_gallery.Models;
using image_gallery.Services;
using image_gallery.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Controllers;

public class CreateImageDescriptionRequestBody
{
    public List<string> ImageIds { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Collection { get; set; }
}

[ApiController]
[Route("/image-description")]
public class ImageDescriptionController : ControllerBase
{
    private IAzureCosmosDbFacade AzureCosmosDbFacade { get; set; }
    private IAzureCosmosConnector AzureCosmosConnector { get; set; }


    public ImageDescriptionController(IAzureCosmosDbFacade azureCosmosDbFacade, IAzureCosmosConnector azureCosmosConnector)
    {
        this.AzureCosmosDbFacade = azureCosmosDbFacade;
        AzureCosmosConnector = azureCosmosConnector;
    }

    [HttpGet]
    public async Task<IActionResult> GetSelectedDescription()
    {
        return Ok(await this.AzureCosmosDbFacade.GetAll());
    }

    [HttpGet]
    [Route("{uuid}")]
    public async Task<IActionResult> GetSelectedDescription(string uuid)
    {
        return Ok(await this.AzureCosmosDbFacade.GetById(uuid));
    }

    [HttpPost]
    public async Task<IActionResult> GetImageDescriptionByName(
        [FromBody] CreateImageDescriptionRequestBody imageDescription)
    {
        try
        {
            ImageDescription response = await this.AzureCosmosDbFacade.Create(imageDescription); 
            return Created(new Uri(
                    $"image-description/{response.id}",
                    UriKind.Relative),
                response
            );
        }
        catch (CosmosException ex)
        {
            return BadRequest("Something went wrong");
        }
    }


    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteImageRequestBody deleteImageRequestBody)
    {
        try
        {
            await this.AzureCosmosDbFacade.Delete(deleteImageRequestBody);
            return Ok("Deleted");
        }
        catch (CosmosException ex)
        {
            return BadRequest("Something went wrong");
        }
    }
}