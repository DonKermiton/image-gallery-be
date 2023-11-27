using Microsoft.Azure.Cosmos;


namespace image_gallery.utils;

public interface IAzureCosmosConnector : IStartupTask
{
    CosmosClient CosmosClient { get; }
    IConfig Config { get; set; }
    public Container PostContainer { get; set; } 
    public Container ImagesContainer { get; set; }
    Task<Database> ConnectToDatabase();
}

public class AzureCosmosConnector : IAzureCosmosConnector
{
    public Database Database { get; private set; }
    public CosmosClient CosmosClient { get; private set; }
    public IConfig Config { get; set; }
    public Container PostContainer { get; set; }


    public Container ImagesContainer { get; set; }

    public AzureCosmosConnector(IConfig config)
    {
        this.Config = config;
    }

    public async Task Execute()
    {
        this.Database = await this.ConnectToDatabase();
        this.PostContainer = await this.ConnectToPostContainer();
        this.ImagesContainer = await this.ConnectToPostImagesContainer();
    }

    public async Task<Database> ConnectToDatabase()
    {
        Console.WriteLine("Establishing connection to CosmosDB...");
        this.CosmosClient = new CosmosClient(this.Config.CosmosDb!.Value.EndpointUrl,
            this.Config.CosmosDb.Value.PrimaryKey, new CosmosClientOptions() {AllowBulkExecution = true});
        return (await this.CosmosClient.CreateDatabaseIfNotExistsAsync(this.Config.CosmosDb.Value.DatabaseName))
            .Database;
    }

    public async Task<Container> ConnectToPostContainer()
    {
        Console.WriteLine("Creating Post Container...");
        return (await this.Database
            .DefineContainer(this.Config.CosmosDb!.Value.PostContainerName, this.Config.CosmosDb.Value.PostPartitionKey)
            .WithIndexingPolicy()
            .WithIndexingMode(IndexingMode.Consistent)
            .WithIncludedPaths()
            .Path("/created/?")
            .Attach()
            .WithExcludedPaths()
            .Path("/*")
            .Attach()
            .Attach()
            .CreateIfNotExistsAsync()).Container;
    }

    public async Task<Container> ConnectToPostImagesContainer()
    {
        Console.WriteLine("Creating Post Images Container... \t" + this.Config.CosmosDb!.Value.ImagesPartitionKey );
        return (await this.Database
            .DefineContainer(this.Config.CosmosDb!.Value.ImagesContainerName, this.Config.CosmosDb.Value.ImagesPartitionKey)
            .WithIndexingPolicy()
            .WithIndexingMode(IndexingMode.Consistent)
            .WithIncludedPaths()
            .Attach()
            .WithExcludedPaths()
            .Path("/*")
            .Attach()
            .Attach()
            .CreateIfNotExistsAsync()).Container; 
        
    }


}