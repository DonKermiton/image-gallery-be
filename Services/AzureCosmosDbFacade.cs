using image_gallery.Controllers;
using image_gallery.Models;
using image_gallery.utils;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Services;

public interface IAzureCosmosDbFacade
{
    IAzureCosmosConnector AzureCosmosConnector { get; set; }
    Task<List<ImageDescription>> GetAll();
    Task<ImageDescription> GetById(string id);
    Task<ItemResponse<ImageDescription>> Delete(DeleteImageRequestBody data);
    Task<ImageDescription> Create(CreateImageDescriptionRequestBody data);
}

public class AzureCosmosDbFacade : IAzureCosmosDbFacade
{
    public IAzureCosmosConnector AzureCosmosConnector { get; set; }

    public AzureCosmosDbFacade(IAzureCosmosConnector azureCosmosConnector)
    {
        this.AzureCosmosConnector = azureCosmosConnector;
    }

    public async Task<List<ImageDescription>> GetAll()
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i"
        );

        FeedIterator<ImageDescription> feedIterator =
            this.AzureCosmosConnector.Container.GetItemQueryIterator<ImageDescription>(queryDefinition: query);

        List<ImageDescription> results = new List<ImageDescription>();


        while (feedIterator.HasMoreResults)
        {
            FeedResponse<ImageDescription> next = await feedIterator.ReadNextAsync();
            results.AddRange(next);
        }

        return results;
    }

    public async Task<ImageDescription> GetById(string id)
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i WHERE i.id = @uuid"
        ).WithParameter("@uuid", id);

        FeedResponse<ImageDescription> imageDescription =
            (await this.AzureCosmosConnector.Container.GetItemQueryIterator<ImageDescription>(queryDefinition: query)
                .ReadNextAsync());

        if (imageDescription.Count == 0)
        {
            // return NotFound($"Content with id {uuid} not found");
        }

        return imageDescription.First();
    }

    public async Task<ImageDescription> Create(CreateImageDescriptionRequestBody data)
    {
        ImageDescription newImageDescription = new(
            id: Guid.NewGuid().ToString(),
            imageIds: data.ImageIds,
            collection: data.Collection ?? "all",
            title: data.Title ,
            description: data.Description,
            created: DateTime.Now
        );

        ItemResponse<ImageDescription> response =
            await this.AzureCosmosConnector.Container.UpsertItemAsync<ImageDescription>(item: newImageDescription,
                new PartitionKey(data.Collection ?? "all"));

        return response.Resource;
    } 
    
    public async Task<ItemResponse<ImageDescription>> Delete(DeleteImageRequestBody data)
    {
        if (String.IsNullOrEmpty(data.collection))
        {
            data.collection = (await this.GetById(data.id)).collection;
        }


        return this.AzureCosmosConnector.Container.DeleteItemAsync<ImageDescription>(id: data.id,
            new PartitionKey(data.collection)).Result;
    }
}