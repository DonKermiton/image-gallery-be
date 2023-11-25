using Microsoft.Azure.Cosmos;


namespace image_gallery.utils;

public interface IAzureCosmosConnector : IStartupTask
{
    CosmosClient CosmosClient { get; }
    IConfig Config { get; set; }
    public Container Container { get; set; } 
    Task<Database> ConnectToDatabase();
}

public class AzureCosmosConnector : IAzureCosmosConnector
{
    public Database Database { get; private set; }
    public CosmosClient CosmosClient { get; private set; }
    public IConfig Config { get; set; }
    public Container Container { get; set; } 


    public AzureCosmosConnector(IConfig config)
    {
        this.Config = config;
    }

    public async Task Execute()
    {
        this.Database = await this.ConnectToDatabase();
        this.Container =  await this.ConnectToContainer();
    }

    public async Task<Database> ConnectToDatabase()
    {
        Console.WriteLine("Establishing connection to CosmosDB...");
        this.CosmosClient = new CosmosClient(this.Config.CosmosDb!.Value.EndpointUrl,
            this.Config.CosmosDb.Value.PrimaryKey, new CosmosClientOptions() {AllowBulkExecution = true});
        return (await this.CosmosClient.CreateDatabaseIfNotExistsAsync(this.Config.CosmosDb.Value.DatabaseName))
            .Database;
    }

    public async Task<Container> ConnectToContainer()
    {
        Console.WriteLine("Creating Container...");
        return (await this.Database
            .DefineContainer(this.Config.CosmosDb!.Value.ContainerName, this.Config.CosmosDb.Value.PartitionKey)
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