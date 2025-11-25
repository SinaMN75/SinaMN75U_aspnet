namespace SinaMN75U.Utils;

public static class SwaggerSetup {
	public static void AddUSwagger(this IServiceCollection services) {
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
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