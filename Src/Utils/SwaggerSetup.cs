namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddOpenApi();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
		});
	}

	public static void UseUSwagger(this WebApplication app) {
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
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DocumentTitle = "SinaMN75";
		});
	}
}