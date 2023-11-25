using image_gallery.middlewares;
using image_gallery.Services;
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
builder.Services.AddSingleton<IAzureCosmosConnector, AzureCosmosConnector>();
builder.Services.AddSingleton<IAzureContainerStorageFacade, AzureContainerStorageFacade>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddControllers();



var app = builder.Build();


List<IStartupTask> asyncStartupTasks = new List<IStartupTask>
{
    app.Services.GetService<IAzureContainerStorageConnector>()!,
    app.Services.GetService<IAzureCosmosConnector>()!,
};

foreach(var startupTask in asyncStartupTasks)
{
    await startupTask.Execute();
}


app.UseHttpsRedirection();
app.UseCors("FrontEndClient");
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();
app.Run();


