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
					Id = "userSelfieVideo",
					Title = "ویدیو احراز هویت",
					Description = "لطفا ویدیو احراز هویت خود را آپلود کنید",
					Endpoint = "User/Update",
					Fields = [
						new UFlowField { Label = "ویدیو", Value = e.VisualAuthentication, Type = TagFieldType.SelfieVideo, Required = true, Key = "VisualAuthentication" }
					]
				}
			);
		
		if (e.ESignature.IsNullOrEmpty() || !e.Tags.Contains(TagUser.ESignatureVerified))
			return new UResponse<UFlowStep?>(
				new UFlowStep {
					Id = "userESignature",
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
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		UserEntity? u = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null) return new UResponse(Usc.NotFound, ls.Get("UserNotFound"));
		
		// Step order validation
		if (p.Id == "userSelfieVideo" && !CanProceedToSelfieVideo(u))
			return new UResponse(Usc.BadRequest, ls.Get("CompleteDocumentStepFirst"));
		
		if (p.Id == "userESignature" && !CanProceedToESignature(u))
			return new UResponse(Usc.BadRequest, ls.Get("CompleteVideoStepFirst"));
		
		if (p.Id == "userDocument") {
			UFlowField? nationalCardFront = p.Fields.FirstOrDefault(x => x.Key == "NationalCardFront");
			if (nationalCardFront == null || (nationalCardFront.Required && nationalCardFront.Value.IsNullOrEmpty()))
				return new UResponse(Usc.BadRequest, ls.Get("NationalCardFrontRequired"));
			
			UFlowField? nationalCardBack = p.Fields.FirstOrDefault(x => x.Key == "NationalCardBack");
			if (nationalCardBack == null || (nationalCardBack.Required && nationalCardBack.Value.IsNullOrEmpty()))
				return new UResponse(Usc.BadRequest, ls.Get("NationalCardBackRequired"));
			
			UFlowField? birthCertificateFirst = p.Fields.FirstOrDefault(x => x.Key == "BirthCertificateFirst");
			if (birthCertificateFirst == null || (birthCertificateFirst.Required && birthCertificateFirst.Value.IsNullOrEmpty()))
				return new UResponse(Usc.BadRequest, ls.Get("BirthCertificateFirstRequired"));
			
			u.NationalCardFront = nationalCardFront.Value.FromBase64();
			u.NationalCardBack = nationalCardBack.Value.FromBase64();
			u.BirthCertificateFirst = birthCertificateFirst.Value.FromBase64();
			
			u.Tags.Add(TagUser.NationalCardFrontVerified);
			u.Tags.Add(TagUser.NationalCardBackVerified);
			u.Tags.Add(TagUser.BirthCertificateFirstVerified);
		}
		else if (p.Id == "userSelfieVideo") {
			UFlowField? video = p.Fields.FirstOrDefault(x => x.Key == "VisualAuthentication");
			if (video == null || (video.Required && video.Value.IsNullOrEmpty()))
				return new UResponse(Usc.BadRequest, ls.Get("VideoRequired"));
			
			u.VisualAuthentication = video.Value.FromBase64();
			u.Tags.Add(TagUser.VisualAuthenticationVerified);
		}
		else if (p.Id == "userESignature") {
			UFlowField? signature = p.Fields.FirstOrDefault(x => x.Key == "ESignature");
			if (signature == null || (signature.Required && signature.Value.IsNullOrEmpty()))
				return new UResponse(Usc.BadRequest, ls.Get("SignatureRequired"));
			
			u.ESignature = signature.Value.FromBase64();
			u.Tags.Add(TagUser.ESignatureVerified);
		}
		else {
			return new UResponse(Usc.BadRequest, ls.Get("InvalidStep"));
		}

		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("Success"));
	}
	
	private bool CanProceedToSelfieVideo(UserEntity u) {
		return !u.NationalCardFront.IsNullOrEmpty() && 
		       u.Tags.Contains(TagUser.NationalCardFrontVerified) &&
		       !u.NationalCardBack.IsNullOrEmpty() && 
		       u.Tags.Contains(TagUser.NationalCardBackVerified) &&
		       !u.BirthCertificateFirst.IsNullOrEmpty() && 
		       u.Tags.Contains(TagUser.BirthCertificateFirstVerified);
	}
	
	private bool CanProceedToESignature(UserEntity u) {
		return !u.VisualAuthentication.IsNullOrEmpty() && 
		       u.Tags.Contains(TagUser.VisualAuthenticationVerified);
	}
}