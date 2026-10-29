using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MMSA.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public const string SchemeId = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any())
            return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, context.Document)] = []
        });

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized: no or invalid token" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden: insufficient role" });
    }
}