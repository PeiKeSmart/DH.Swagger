using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace DH.Swagger;

public class ApiUIMiddleware {

    private readonly SwaggerUIOptions _options;
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ApiUIMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory,
        SwaggerUIOptions options)
    {
        _next = next;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var httpMethod = httpContext.Request.Method;
        var path = httpContext.Request.Path.Value;
        if (httpMethod == "GET" && Regex.IsMatch(path, $"^/api-config$"))
        {
            await RespondWithConfig(httpContext.Response).ConfigureAwait(false);
            return;
        }

        // 或请求管道中调用下一个中间件
        await _next(httpContext).ConfigureAwait(false);
    }

    private async Task RespondWithConfig(HttpResponse response)
    {
        await response.WriteAsync(JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions)).ConfigureAwait(false);
    }
}
