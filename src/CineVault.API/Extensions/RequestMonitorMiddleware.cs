using System.Diagnostics;

namespace CineVault.API.Extensions;

public class RequestMonitorMiddleware : IMiddleware
{
    private readonly ILogger<RequestMonitorMiddleware> _logger;

    public RequestMonitorMiddleware(ILogger<RequestMonitorMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var timer = Stopwatch.StartNew();
        _logger.LogInformation("Req start: {verb} {url}", context.Request.Method, context.Request.Path);
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Req error: {verb} {url}", context.Request.Method, context.Request.Path);
            throw;
        }
        finally
        {
            timer.Stop();
            _logger.LogInformation("Req done: {verb} {url} {status} in {ms} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                timer.ElapsedMilliseconds);
        }
    }
}