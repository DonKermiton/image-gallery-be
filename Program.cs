using System.Runtime.InteropServices.JavaScript;
using image_gallery.utils;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSingleton<IConfig, Config>();
builder.Services.AddSingleton<IAzureContainerStorageConnector, AzureContainerStorageConnector>();
builder.Services.AddSingleton<IAzureContainerStorageCache, AzureContainerStorageFacade>();
builder.Services.AddControllers();



var app = builder.Build();
var startupTasks = app.Services.GetServices<IAzureContainerStorageConnector>();
foreach(var startupTask in startupTasks)
{
    await startupTask.Execute();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();


