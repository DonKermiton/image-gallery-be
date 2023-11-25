using image_gallery.Services;

namespace image_gallery.middlewares;

public class ErrorHandlingMiddleware: IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (BadRequestException e)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(e.Message);
        }
        catch(System.Exception e)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(e.Message);
        }
    }
}