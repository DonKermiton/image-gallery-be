using image_gallery.Models;
using image_gallery.utils;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Services;

public interface IPostImagesCosmos
{
    IAzureContainerStorageFacade AzureContainerStorageFacade { get; set; }
    IAzureCosmosConnector AzureCosmosConnector { get; set; }
    Task<PostImages> GetImageById(string id);
    Task<List<PostImages>> GetImagesByPostId(string postId);
    Task<PostImages> GetImageByQuery(QueryDefinition query);
    Task<PostImages> SaveImage(string postId, string filename);
}

public class PostImagesCosmos : IPostImagesCosmos
{
    public IAzureContainerStorageFacade AzureContainerStorageFacade { get; set; }

    public IAzureCosmosConnector AzureCosmosConnector { get; set; }

    public PostImagesCosmos(IAzureCosmosConnector azureCosmosConnector, IAzureContainerStorageFacade azureContainerStorageFacade)
    {
        this.AzureCosmosConnector = azureCosmosConnector;
        this.AzureContainerStorageFacade = azureContainerStorageFacade;
    }

    public async Task<PostImages> GetImageById(string id)
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM postImages i WHERE i.id = @uuid"
        ).WithParameter("@uuid", id);

        return await this.GetImageByQuery(query);
    }

    public async Task<List<PostImages>> GetImagesByPostId(string postId)
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM postImages i WHERE i.postId = @postId"
        ).WithParameter("@postId", postId);

        FeedIterator<PostImages> feedIterator =
            this.AzureCosmosConnector.ImagesContainer.GetItemQueryIterator<PostImages>(queryDefinition: query);

        List<PostImages> results = new List<PostImages>();

        while (feedIterator.HasMoreResults)
        {
            FeedResponse<PostImages> next = await feedIterator.ReadNextAsync();
            results.AddRange(next);
        }

        return results;
    }

    public async Task<PostImages> GetImageByQuery(QueryDefinition query)
    {
        FeedResponse<PostImages> imageDescription =
            (await this.AzureCosmosConnector.PostContainer.GetItemQueryIterator<PostImages>(queryDefinition: query)
                .ReadNextAsync());

        if (imageDescription.Count == 0)
        {
            throw new BadRequestException($"Content not found");
        }

        return imageDescription.First();
    }
    
    public async Task<PostImages> SaveImage(string postId, string filename)
    {
        PostImages postImages = new PostImages(
            id: Guid.NewGuid().ToString(),
            postId: postId,
            filename: filename
        );

        return (await this.AzureCosmosConnector.ImagesContainer.UpsertItemAsync<PostImages>(postImages)).Resource;
    }
}