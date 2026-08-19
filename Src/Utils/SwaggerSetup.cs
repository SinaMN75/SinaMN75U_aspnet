using Microsoft.OpenApi;

namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(c => {
			c.SwaggerDoc("100", new OpenApiInfo {
				Title = "SinaMN75 API",
				Version = "100",
				Description = "API version 100"
			});
			
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);

			c.AddSecurityDefinition("Locale", new OpenApiSecurityScheme {
				Name = "Locale",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Description = "The user's locale, e.g. en-US or fa-IR."
			});

			c.AddSecurityDefinition("Timezone", new OpenApiSecurityScheme {
				Name = "Timezone",
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Description = "Timezone offset in minutes (e.g. 210) or IANA ID (e.g. Asia/Tehran)."
			});

			c.AddSecurityRequirement(document => new OpenApiSecurityRequirement {
				{ new OpenApiSecuritySchemeReference("Locale", document), [] },
				{ new OpenApiSecuritySchemeReference("Timezone", document), [] }
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