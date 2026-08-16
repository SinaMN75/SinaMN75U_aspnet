using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
			c.OperationFilter<USwaggerHeaderFilter>();
		});
	}

	public static void UseUSwagger(this WebApplication app) {
		app.UseSwagger();

		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(128);
			c.DocumentTitle = "SinaMN75";
		});
	}
}

public sealed class USwaggerHeaderFilter : IOperationFilter {
	public void Apply(OpenApiOperation operation, OperationFilterContext context) {
		operation.Parameters ??= new List<IOpenApiParameter>();
		operation.Parameters.Add(new OpenApiParameter {
			Name = "Locale",
			In = ParameterLocation.Header,
			Required = false,
			Description = "The user's locale, e.g. en-US or fa-IR.",
			Schema = new OpenApiSchema { Type = JsonSchemaType.String }
		});
		operation.Parameters.Add(new OpenApiParameter {
			Name = "Timezone",
			In = ParameterLocation.Header,
			Required = false,
			Description = "Offset in minutes (e.g. 210) or IANA id (e.g. Asia/Tehran).",
			Schema = new OpenApiSchema { Type = JsonSchemaType.String }
		});
	}
}
