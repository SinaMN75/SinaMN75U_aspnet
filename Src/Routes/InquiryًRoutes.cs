namespace SinaMN75U.Routes;

public static class InquiryRoutes {
	public static void MapInquiryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("PostalCodeToAddressDetail", async (PostalCodeToAddressDetailParams p, IInquiryService s, CancellationToken c) => (await s.PostalCodeToAddressDetail(p, c)).ToResult()).Produces<UResponse<PostalCodeToAddressDetailResponse>>();
		r.MapPost("VehicleViolationDetail", async (VehicleViolationDetailParams p, IInquiryService s, CancellationToken c) => (await s.GetVehicleViolationsDetail(p, c)).ToResult()).Produces<UResponse<PostalCodeToAddressDetailResponse>>();
		r.MapPost("DrivingLicenceStatus", async (DrivingLicenceStatusParams p, IInquiryService s, CancellationToken c) => (await s.GetDrivingLicenceStatus(p, c)).ToResult()).Produces<UResponse<PostalCodeToAddressDetailResponse>>();
		r.MapPost("LicencePlateInquiry", async (LicencePlateInquiryParams p, IInquiryService s, CancellationToken c) => (await s.InquiryLicencePlate(p, c)).ToResult()).Produces<UResponse<LicencePlateInquiryResponse>>();
		r.MapPost("DrivingLicenceNegativePoint", async (DrivingLicenceNegativePointParams p, IInquiryService s, CancellationToken c) => (await s.DrivingLicenceNegativePoint(p, c)).ToResult()).Produces<UResponse<DrivingLicenceNegativePointResponse>>();
		r.MapPost("IBanToBankAccountDetail", async (IBanToBankAccountDetailParams p, IInquiryService s, CancellationToken c) => (await s.IBanToBankAccountDetail(p, c)).ToResult()).Produces<UResponse<IBanToBankAccountDetailResponse>>();
	}
}