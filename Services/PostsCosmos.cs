using image_gallery.Controllers;
using image_gallery.Models;
using image_gallery.utils;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Services;

public interface IAzureCosmosDbFacade
{
    IAzureCosmosConnector AzureCosmosConnector { get; set; }
    Task<List<PostRecordWithImages>> GetAll();
    Task<PostRecordWithImages> GetById(string id);
    Task<ItemResponse<PostRecord>> Delete(string id);
    Task<PostRecord> Create(CreateImageDescriptionRequestBody data);
}

public class PostsCosmos : IAzureCosmosDbFacade
{
    public IAzureCosmosConnector AzureCosmosConnector { get; set; }

    public IAzureContainerStorageFacade AzureContainerStorageFacade { get; set; }

    public IPostImagesCosmos PostImagesCosmos { get; set; }

    public PostsCosmos(IAzureCosmosConnector azureCosmosConnector, IAzureContainerStorageFacade azureContainerStorageFacade, IPostImagesCosmos postImagesCosmos)
    {
        this.AzureCosmosConnector = azureCosmosConnector;
        this.AzureContainerStorageFacade = azureContainerStorageFacade;
        this.PostImagesCosmos = postImagesCosmos;
    }

    public async Task<List<PostRecordWithImages>> GetAll()
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i order by i.created DESC"
        );

        FeedIterator<PostRecord> feedIterator =
            this.AzureCosmosConnector.PostContainer.GetItemQueryIterator<PostRecord>(queryDefinition: query);

        List<PostRecordWithImages> results = new List<PostRecordWithImages>();


        while (feedIterator.HasMoreResults)
        {
            FeedResponse<PostRecord> next = await feedIterator.ReadNextAsync();
            foreach (var post in next)
            {
                results.Add(this.GetModel(post, await this.GetImagesByPostId(post.id)));
            } 

        }

        return results;
    }

    private PostRecordWithImages GetModel(PostRecord post, List<ContainerFile> images)
    {
        return new PostRecordWithImages(
            id: post.id,
            collection: post.collection,
            title: post.title,
            description: post.description,
            created: post.created,
            images: images
        ); 
    } 

    private async Task<List<ContainerFile>> GetImagesByPostId(string postId)
    {
        List<ContainerFile> files = new List<ContainerFile>();
        foreach (var postImages in await this.PostImagesCosmos.GetImagesByPostId(postId))
        {
            files.Add(new ContainerFile(
                this.AzureContainerStorageFacade.GetByName(postImages.filename),
                postImages.filename));
        }

        return files;
    } 

    public async Task<PostRecordWithImages> GetById(string id)
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i WHERE i.id = @uuid"
        ).WithParameter("@uuid", id);

        FeedResponse<PostRecord> imageDescription =
            (await this.AzureCosmosConnector.PostContainer.GetItemQueryIterator<PostRecord>(queryDefinition: query)
                .ReadNextAsync());

        if (imageDescription.Count == 0)
        {
            throw new BadRequestException($"Content with id {id} not found");
        }


        List<ContainerFile> files = new List<ContainerFile>();
        foreach (var postImages in await this.PostImagesCosmos.GetImagesByPostId(id))
        {
            files.Add(new ContainerFile(
                this.AzureContainerStorageFacade.GetByName(postImages.filename),
                postImages.filename));
        }

        var post = imageDescription.First();


        return new PostRecordWithImages(
            id: post.id,
            collection: post.collection,
            title: post.title,
            description: post.description,
            created: post.created,
            images: files
        );
    }

    public async Task<PostRecord> Create(CreateImageDescriptionRequestBody data)
    {
        PostRecord newPostRecord = new(
            id: Guid.NewGuid().ToString(),
            collection: data.Collection ?? "all",
            title: data.Title ,
            description: data.Description,
            created: DateTime.Now
        );

        ItemResponse<PostRecord> response =
            await this.AzureCosmosConnector.PostContainer.UpsertItemAsync<PostRecord>(item: newPostRecord,
                new PartitionKey(data.Collection ?? "all"));

        foreach (var imageId in data.ImageIds)
        {
            await this.PostImagesCosmos.SaveImage(newPostRecord.id, imageId);
        }
        
        return response.Resource;
    } 
    
    public async Task<ItemResponse<PostRecord>> Delete(string id)
    {
        PostRecord postRecord = (await this.GetById(id));

        return this.AzureCosmosConnector.PostContainer.DeleteItemAsync<PostRecord>(id: postRecord.id,
            new PartitionKey(postRecord.collection)).Result;
    }
}