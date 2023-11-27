using image_gallery.Controllers;
using image_gallery.Models;
using image_gallery.utils;
using Microsoft.Azure.Cosmos;

namespace image_gallery.Services;

public interface IAzureCosmosDbFacade
{
    IAzureCosmosConnector AzureCosmosConnector { get; set; }
    Task<List<ImageDescriptionWithImages>> GetAll();
    Task<ImageDescriptionWithImages> GetById(string id);
    Task<ItemResponse<ImageDescription>> Delete(string id);
    Task<ImageDescription> Create(CreateImageDescriptionRequestBody data);
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

    public async Task<List<ImageDescriptionWithImages>> GetAll()
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i order by i.created DESC"
        );

        FeedIterator<ImageDescription> feedIterator =
            this.AzureCosmosConnector.PostContainer.GetItemQueryIterator<ImageDescription>(queryDefinition: query);

        List<ImageDescriptionWithImages> results = new List<ImageDescriptionWithImages>();


        while (feedIterator.HasMoreResults)
        {
            FeedResponse<ImageDescription> next = await feedIterator.ReadNextAsync();
            foreach (var post in next)
            {
                results.Add(this.GetModel(post, await this.GetImagesByPostId(post.id)));
            } 

        }

        return results;
    }

    private ImageDescriptionWithImages GetModel(ImageDescription post, List<ContainerFile> images)
    {
        return new ImageDescriptionWithImages(
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

    public async Task<ImageDescriptionWithImages> GetById(string id)
    {
        QueryDefinition query = new QueryDefinition(
            query: "SELECT * FROM items i WHERE i.id = @uuid"
        ).WithParameter("@uuid", id);

        FeedResponse<ImageDescription> imageDescription =
            (await this.AzureCosmosConnector.PostContainer.GetItemQueryIterator<ImageDescription>(queryDefinition: query)
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


        return new ImageDescriptionWithImages(
            id: post.id,
            collection: post.collection,
            title: post.title,
            description: post.description,
            created: post.created,
            images: files
        );
    }

    public async Task<ImageDescription> Create(CreateImageDescriptionRequestBody data)
    {
        ImageDescription newImageDescription = new(
            id: Guid.NewGuid().ToString(),
            collection: data.Collection ?? "all",
            title: data.Title ,
            description: data.Description,
            created: DateTime.Now
        );

        ItemResponse<ImageDescription> response =
            await this.AzureCosmosConnector.PostContainer.UpsertItemAsync<ImageDescription>(item: newImageDescription,
                new PartitionKey(data.Collection ?? "all"));

        foreach (var imageId in data.ImageIds)
        {
            await this.PostImagesCosmos.SaveImage(newImageDescription.id, imageId);
        }
        
        return response.Resource;
    } 
    
    public async Task<ItemResponse<ImageDescription>> Delete(string id)
    {
        ImageDescription imageDescription = (await this.GetById(id));

        // foreach (var imageId in imageDescription.imageIds)
        // {
        //     try
        //     {
        //         await this.AzureContainerStorageFacade.Delete(imageId);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        // }
        //
     
        return this.AzureCosmosConnector.PostContainer.DeleteItemAsync<ImageDescription>(id: imageDescription.id,
            new PartitionKey(imageDescription.collection)).Result;
    }
}