namespace SinaMN75U.Services;

public interface IFlowService {
	Task<UResponse<UFlowStep?>> AuthenticationGet(BaseParams p, CancellationToken ct);
	Task<UResponse> AuthenticationSend(UFlowStepSend p, CancellationToken ct);
}

public class FlowService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IFlowService {
	public async Task<UResponse<UFlowStep?>> AuthenticationGet(BaseParams p, CancellationToken ct) {
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
					Id = "userDocument",
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
		if (e.VisualAuthentication.IsNullOrEmpty() || !e.Tags.Contains(TagUser.VisualAuthenticationVerified))
			return new UResponse<UFlowStep?>(
				new UFlowStep {
					Id = "",
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
					Id = "Guid.NewGuid()",
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

	public async Task<UResponse> AuthenticationSend(UFlowStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UFlowStep?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		UserEntity u = (await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct))!;
		
		if (p.Id == "userDocument") {
			
			UFlowField? nationalCardFront = p.Fields.FirstOrDefault(x => x.Key == "NationalCardFront");
			if (nationalCardFront == null || nationalCardFront.Required && nationalCardFront.Value.IsNullOrEmpty())
				return new UResponse(Usc.BadRequest);
			
			u.NationalCardFront = nationalCardFront.Value.FromBase64();
		}


		await db.SaveChangesAsync(ct);
		return new UResponse<UFlowStep?>(null, Usc.Success, ls.Get("Success"));
	}
}