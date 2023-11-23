namespace image_gallery.utils;

public interface IConfig
{
    string StorageConnectionString { get; }
    string FullImageContainerName { get; }
}

public class Config : IConfig
{
    public string StorageConnectionString {  get; private set; }

    public string FullImageContainerName { get; private set; } = String.Empty;

    public Config(IConfiguration configuration)
    {
        this.StorageConnectionString = configuration.GetSection("StorageConnectionString").Value ?? String.Empty;
            
        if (this.StorageConnectionString == String.Empty)
        {
            throw new Exception("StorageConnectionString is undefined. Check config file");
        } 
    }
}