namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddOpenApi();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
			c.AddSecurityDefinition("locale", new OpenApiSecurityScheme {
				Description = "Locale",
				Name = "Locale",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "locale" } }, [] }
			});
		});
	}

	public static void UseUSwagger(this IApplicationBuilder app) {
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(2);
		});
	}
}