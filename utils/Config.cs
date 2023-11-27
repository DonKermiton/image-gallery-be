using System.Reflection;

namespace image_gallery.utils;

public interface IConfig
{
    string StorageConnectionString { get; }
    string FullImageContainerName { get; }
    CosmosDbConnectionInfo? CosmosDb { get; }
}

public struct CosmosDbConnectionInfo
{
    public String EndpointUrl;
    public String PrimaryKey;
    public String DatabaseName;
    public String PostContainerName;
    public String PostPartitionKey;
    public String ImagesContainerName;
    public String ImagesPartitionKey;
}

public class Config : IConfig
{
    public string StorageConnectionString { get; private set; }
    public string FullImageContainerName { get; private set; } = String.Empty;
    private IConfiguration Configuration { get; set; }
    public CosmosDbConnectionInfo? CosmosDb { get; private set; }

    public Config(IConfiguration configuration)
    {
        this.Configuration = configuration;
        this.InitStorageData();
        this.InitCosmosDb();
    }


    private void InitStorageData()
    {
        this.StorageConnectionString =
            this.Configuration.GetSection("StorageConnectionString").Value ?? String.Empty;

        if (this.StorageConnectionString == String.Empty)
        {
            throw new Exception("StorageConnectionString is undefined. Check config file");
        }
    }

    private void InitCosmosDb()
    {
        this.CosmosDb = new CosmosDbConnectionInfo
        {
            PostContainerName = this.Configuration["CosmosDB:PostContainerName"] ?? String.Empty,
            DatabaseName = this.Configuration["CosmosDB:DatabaseName"] ?? String.Empty,
            PostPartitionKey = this.Configuration["CosmosDB:PostPartitionKey"] ?? String.Empty,
            PrimaryKey = this.Configuration["CosmosDB:PrimaryKey"] ?? String.Empty,
            EndpointUrl = this.Configuration["CosmosDB:EndpointUrl"] ?? String.Empty,
            ImagesContainerName = this.Configuration["CosmosDB:ImagesContainerName"] ?? String.Empty,
            ImagesPartitionKey = this.Configuration["CosmosDB:ImagesPartitionKey"] ?? String.Empty,
        };

        foreach (FieldInfo field in this.CosmosDb.GetType().GetFields())
        {
            string fieldValue = field.GetValue(this.CosmosDb)?.ToString();

            if (String.IsNullOrEmpty(fieldValue))
            {
                Console.WriteLine($"{field.Name} is empty, but required to set connection to Cosmos DB");
                throw new Exception($"{field.Name} is empty");
            }
        }
    }
}