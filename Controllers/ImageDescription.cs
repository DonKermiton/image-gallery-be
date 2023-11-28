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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await this.AzureCosmosDbFacade.GetAll());
    }

    [HttpGet]
    [Route("{uuid}")]
    public async Task<IActionResult> GetById(string uuid)
    {
        return Ok(await this.AzureCosmosDbFacade.GetById(uuid));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateImageDescriptionRequestBody imageDescription)
    {
        try
        {
            PostRecord response = await this.AzureCosmosDbFacade.Create(imageDescription); 
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
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await this.AzureCosmosDbFacade.Delete(id);
            return Ok();
        }
        catch (CosmosException ex)
        {
            return BadRequest("Something went wrong");
        }
    }
}