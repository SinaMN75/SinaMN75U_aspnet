namespace SinaMN75U.Services;

public interface IFlowService {
	Task<UResponse<UFlowStep?>> Authentication(BaseParams p, CancellationToken ct);
}

public class FlowService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IFlowService {
	public async Task<UResponse<UFlowStep?>> Authentication(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UFlowStep?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(new UserSelectorArgs {
				Wallet = new WalletSelectorArgs(),
				Merchant = new MerchantSelectorArgs { Terminal = new TerminalSelectorArgs() },
				NationalCardFront = true,
				NationalCardBack = true,
				BirthCertificateFirst = true,
				BirthCertificateSecond = true,
				BirthCertificateThird = true,
				BirthCertificateForth = true,
				BirthCertificateFifth = true,
				VisualAuthentication = true,
				ESignature = true
			}))
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (e == null) return new UResponse<UFlowStep?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (
			e.NationalCardFront.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.NationalCardFrontVerified) ||
			e.NationalCardBack.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.NationalCardBackVerified) ||
			e.BirthCertificateFirst.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.BirthCertificateFirstVerified)
		)
			return new UResponse<UFlowStep?>(
				new UFlowStep {
					Title = "آپلود مدارک شناسایی",
					Description = "لطفا مدارک شناسایی خود را آپلود کنید",
					Endpoint = "User/Update",
					Fields = [
						new UFlowField { Label = "روی کارت ملی", Value = e.NationalCardFront, Type = TagFieldType.Image, Required = true, Key = "NationalCardFront" },
						new UFlowField { Label = "پشت کارت ملی", Value = e.NationalCardBack, Type = TagFieldType.Image, Required = true, Key = "NationalCardBack" },
						new UFlowField { Label = "صفحه اول شناسنامه", Value = e.BirthCertificateFirst, Type = TagFieldType.Image, Required = true, Key = "BirthCertificateFirst" }
					]
				}
			);
		if (e.VisualAuthentication.IsNullOrEmpty() || e.Tags.Contains(TagUser.VisualAuthenticationVerified))
			return new UResponse<UFlowStep?>(
				new UFlowStep {
					Title = "ویدیو احراز هویت",
					Description = "لطفا مدارک شناسایی خود را آپلود کنید",
					Endpoint = "User/Update",
					Fields = [
						new UFlowField { Label = "ویدیو", Value = e.VisualAuthentication, Type = TagFieldType.SelfieVideo, Required = true, Key = "VisualAuthentication" }
					]
				}
			);
		if (e.ESignature.IsNullOrEmpty() || e.Tags.Contains(TagUser.ESignatureVerified))
			return new UResponse<UFlowStep?>(
				new UFlowStep {
					Title = "امضای دیجیتال",
					Description = "امضا",
					Endpoint = "User/Update",
					Fields = [
						new UFlowField { Label = "امضا دیجیتال", Value = e.ESignature, Type = TagFieldType.ESignature, Required = true, Key = "ESignature" }
					]
				}
			);

		return new UResponse<UFlowStep?>(null);
	}
}