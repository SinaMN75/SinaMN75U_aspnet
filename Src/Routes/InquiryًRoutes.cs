namespace SinaMN75U.Routes;

public static class InquiryRoutes {
	public static void MapInquiryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("PostalCodeToAddressDetail", async (PostalCodeToAddressDetailParams p, IInquiryService s, CancellationToken c) => (await s.PostalCodeToAddressDetail(p, c)).ToResult()).Produces<UResponse<PostalCodeToAddressDetailResponse>>();
		r.MapPost("VehicleViolationDetail", async (VehicleViolationDetailParams p, IInquiryService s, CancellationToken c) => (await s.GetVehicleViolationsDetail(p, c)).ToResult()).Produces<UResponse<PostalCodeToAddressDetailResponse>>();
		r.MapPost("DrivingLicenceStatus", async (DrivingLicenceStatusParams p, IInquiryService s, CancellationToken c) => (await s.GetDrivingLicenceStatus(p, c)).ToResult()).Produces<UResponse<DrivingLicenceStatusResponse>>();
	}
}