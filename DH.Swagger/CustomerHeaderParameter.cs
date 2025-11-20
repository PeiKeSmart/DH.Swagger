using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace DH.Swagger;

public class CustomerHeaderParameter : IOperationFilter
{
    private readonly IConfiguration _configuration;
    public CustomerHeaderParameter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
        {
            operation.Parameters = new List<IOpenApiParameter>();
        }

        var headers = _configuration.GetSection("SwaggerOption:Headers").Get<List<OpenApiParameter>>();
        if (headers == null) return;
        foreach (var header in headers)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = header.Name,
                In = ParameterLocation.Header,
                Description = header.Description,
            });
        }
    }
}
