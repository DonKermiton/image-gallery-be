using System.Runtime.InteropServices.JavaScript;
using image_gallery.utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndClient", policy =>
    {
        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin();
    });
});

builder.Services.AddSingleton<IConfig, Config>();
builder.Services.AddSingleton<IAzureContainerStorageConnector, AzureContainerStorageConnector>();
builder.Services.AddSingleton<IAzureContainerStorageFacade, AzureContainerStorageFacade>();
builder.Services.AddControllers();



var app = builder.Build();
var startupTasks = app.Services.GetServices<IAzureContainerStorageConnector>();
foreach(var startupTask in startupTasks)
{
    await startupTask.Execute();
}


app.UseHttpsRedirection();
app.UseCors("FrontEndClient");
app.UseAuthorization();

app.MapControllers();
app.Run();


