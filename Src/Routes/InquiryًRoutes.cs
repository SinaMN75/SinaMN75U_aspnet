namespace SinaMN75U.Routes;

public static class InquiryRoutes {
	public static void MapInquiryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("PostalCodeToAddressDetail", async (PostalCodeToAddressDetailParams d, IITHubServiceService s, CancellationToken c) => (await s.PostalCodeToAddressDetail(d, c)).ToResult()).Produces<UResponse<ItHubPostalCodeToAddressDetailResponse>>();
	}
}