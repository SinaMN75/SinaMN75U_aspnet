using Microsoft.OpenApi;
using ReferenceType = Syncfusion.DocIO.ReferenceType;

namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();

			c.OrderActionsBy(s => s.RelativePath);

			c.AddSecurityDefinition("Locale", new OpenApiSecurityScheme {
				Name = "Locale",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Description = "The user's locale, e.g. en-US or fa-IR.",
			});

			c.AddSecurityDefinition("Timezone", new OpenApiSecurityScheme {
				Name = "Timezone",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Description = "The user's timezone, e.g. Asia/Tehran or America/New_York.",
			});
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