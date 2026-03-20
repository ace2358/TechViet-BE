using System.Net;
using System.Text.Json;
using techviet_be.Common;

namespace techviet_be.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                BadHttpRequestException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = ApiResponse.Error(ex.Message, statusCode);
            var payload = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(payload);
        }
    }
}