namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		// scalar
		services.AddOpenApi();
		// swagger
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
		});
		// nSwag
		services.AddOpenApiDocument(config => {
			config.Title = "My API";
			config.Version = "v1";
			// config.GenerateExamples = true;
			config.UseControllerSummaryAsTagDescription = true;
			config.SchemaSettings.DefaultReferenceTypeNullHandling = NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
			config.SchemaSettings.GenerateEnumMappingDescription = true;
			config.SchemaSettings.FlattenInheritanceHierarchy = true;
		});
	}

	public static void UseUSwagger(this WebApplication app) {
		// scalar
		app.MapOpenApi();
		app.MapScalarApiReference(x => {
			x.WithTitle("SinaMN75");
			x.ForceDarkMode();
			x.AddPreferredSecuritySchemes("BasicAuth")
				.AddHttpAuthentication("BasicAuth", a => {
					a.Username = "sina";
					a.Password = "sina";
				});
		});
		
		// swagger
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(4);
			c.DocumentTitle = "SinaMN75";
		});
		
		// nSwag
		app.UseOpenApi(options => { options.Path = "/openapi/v1.json"; });
		app.UseSwaggerUi(options => {
			options.Path = "/swagger";
			options.DocumentPath = "/openapi/v1.json";
			options.DocExpansion = "list";
			options.TagsSorter = "alpha";
		});
	}
}