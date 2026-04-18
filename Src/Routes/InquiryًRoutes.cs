namespace SinaMN75U.Routes;

public static class InquiryRoutes {
	public static void MapInquiryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ZipCodeToAddressDetail", async (ZipCodeToAddressDetailParams p, IInquiryService s, CancellationToken c) => (await s.ZipCodeToAddressDetail(p, c)).ToResult()).Produces<UResponse<ZipCodeToAddressDetailResponse>>();
		r.MapPost("VehicleViolationDetail", async (VehicleViolationDetailParams p, IInquiryService s, CancellationToken c) => (await s.VehicleViolationsDetail(p, c)).ToResult()).Produces<UResponse<ZipCodeToAddressDetailResponse>>();
		r.MapPost("DrivingLicenceStatus", async (DrivingLicenceStatusParams p, IInquiryService s, CancellationToken c) => (await s.DrivingLicenceStatus(p, c)).ToResult()).Produces<UResponse<ZipCodeToAddressDetailResponse>>();
		r.MapPost("LicencePlateInquiry", async (LicencePlateInquiryParams p, IInquiryService s, CancellationToken c) => (await s.LicencePlateDetail(p, c)).ToResult()).Produces<UResponse<LicencePlateDetailResponse>>();
		r.MapPost("DrivingLicenceNegativePoint", async (DrivingLicenceNegativePointParams p, IInquiryService s, CancellationToken c) => (await s.DrivingLicenceNegativePoint(p, c)).ToResult()).Produces<UResponse<DrivingLicenceNegativePointResponse>>();
		r.MapPost("IBanToBankAccountDetail", async (IBanToBankAccountDetailParams p, IInquiryService s, CancellationToken c) => (await s.IBanToBankAccountDetail(p, c)).ToResult()).Produces<UResponse<IBanToBankAccountDetailResponse>>();
	}
}