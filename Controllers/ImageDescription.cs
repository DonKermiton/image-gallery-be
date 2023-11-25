using image_gallery.Models;
using image_gallery.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Controllers;

public class CreateImageDescriptionRequestBody
{
    public string ImageId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
}


[ApiController]
[Route("/image-description")]
public class ImageDescriptionController: ControllerBase
{
    private IAzureCosmosConnector AzureCosmosConnector { get; set; }

    public ImageDescriptionController(IAzureCosmosConnector azureCosmosConnector)
    {
        this.AzureCosmosConnector = azureCosmosConnector;
    }

    [HttpPost]
    public async Task GetImageDescriptionByName([FromBody] CreateImageDescriptionRequestBody imageDescription)
    {
        ImageDescription newImageDescription = new(
            category: "/someImage",
            title: imageDescription.Title,
            description: imageDescription.Description,
            created: DateTime.Now
        );

        ItemResponse<ImageDescription> response =
            await this.AzureCosmosConnector.Container.UpsertItemAsync<ImageDescription>(item: newImageDescription);
    }
    
}