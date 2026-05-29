namespace SinaMN75U.Services;

public static class FlowStepIds {
	public const string UserData = "userData";
	public const string UserDocument = "userDocument";
	public const string UserSelfieVideo = "userSelfieVideo";
	public const string UserESignature = "userESignature";
}

public interface IFlowService {
	Task<UResponse<UFlowStepGet?>> AuthenticationGet(BaseParams p, CancellationToken ct);
	Task<UResponse<UFlowStepGet?>> AuthenticationSend(UFlowStepSend p, CancellationToken ct);
}

public class FlowService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IFlowService {
	public async Task<UResponse<UFlowStepGet?>> AuthenticationGet(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UFlowStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

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
		if (e == null) return new UResponse<UFlowStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (e.JsonData.FatherName == null || e.Birthdate == null) {
			return new UResponse<UFlowStepGet?>(
				new UFlowStepGet {
					Id = FlowStepIds.UserData,
					Title = "اطلاعات هویتی",
					Description = "اطلاعات خود را تکمیل کنید.",
					Fields = [
						new UFlowField { Label = "نام پدر", Value = e.JsonData.FatherName, Type = TagFieldType.Text, Required = true, Key = nameof(UserEntity.JsonData.FatherName) },
						new UFlowField { Label = "تاریخ تولد", Value = e.Birthdate.ToString(), Type = TagFieldType.PersianDate, Required = true, Key = nameof(UserEntity.Birthdate) },
					]
				}
			);
		}


		if (
			e.NationalCardFront.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.NationalCardFrontVerified) ||
			e.NationalCardBack.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.NationalCardBackVerified) ||
			e.BirthCertificateFirst.IsNullOrEmpty() ||
			!e.Tags.Contains(TagUser.BirthCertificateFirstVerified)
		)
			return new UResponse<UFlowStepGet?>(
				new UFlowStepGet {
					Id = FlowStepIds.UserDocument,
					Title = "آپلود مدارک شناسایی",
					Description = "لطفا مدارک شناسایی خود را آپلود کنید",
					Fields = [
						new UFlowField { Label = "روی کارت ملی", Value = e.NationalCardFront, Type = TagFieldType.Image, Required = true, Key = nameof(UserEntity.NationalCardFront) },
						new UFlowField { Label = "پشت کارت ملی", Value = e.NationalCardBack, Type = TagFieldType.Image, Required = true, Key = nameof(UserEntity.NationalCardBack) },
						new UFlowField { Label = "صفحه اول شناسنامه", Value = e.BirthCertificateFirst, Type = TagFieldType.Image, Required = true, Key = nameof(UserEntity.BirthCertificateFirst) }
					]
				}
			);

		if (e.VisualAuthentication.IsNullOrEmpty() || !e.Tags.Contains(TagUser.VisualAuthenticationVerified))
			return new UResponse<UFlowStepGet?>(
				new UFlowStepGet {
					Id = FlowStepIds.UserSelfieVideo,
					Title = "ویدیو احراز هویت",
					Description = "لطفا ویدیو احراز هویت خود را آپلود کنید",
					Fields = [
						new UFlowField { Label = "ویدیو", Value = e.VisualAuthentication, Type = TagFieldType.SelfieVideo, Required = true, Key = nameof(UserEntity.VisualAuthentication) }
					]
				}
			);

		if (e.ESignature.IsNullOrEmpty() || !e.Tags.Contains(TagUser.ESignatureVerified))
			return new UResponse<UFlowStepGet?>(
				new UFlowStepGet {
					Id = FlowStepIds.UserESignature,
					Title = "امضای دیجیتال",
					Description = "امضا",
					Fields = [
						new UFlowField { Label = "امضا دیجیتال", Value = e.ESignature, Type = TagFieldType.ESignature, Required = true, Key = nameof(UserEntity.ESignature) }
					]
				}
			);

		return new UResponse<UFlowStepGet?>(null);
	}

	public async Task<UResponse<UFlowStepGet?>> AuthenticationSend(UFlowStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UFlowStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? u = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null) return new UResponse<UFlowStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (p.Id == FlowStepIds.UserData) {
			UFlowField? fatherName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.JsonData.FatherName));
			if (fatherName == null || (fatherName.Required && fatherName.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("FatherNameRequired"));

			UFlowField? birthdate = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.Birthdate));
			if (birthdate == null || (birthdate.Required && birthdate.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("BirthDateRequired"));

			u.JsonData.FatherName = fatherName.Value;
			u.Birthdate = DateTime.Parse(birthdate.Value!);
		}
		else if (p.Id == FlowStepIds.UserDocument) {
			UFlowField? nationalCardFront = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardFront));
			if (nationalCardFront == null || (nationalCardFront.Required && nationalCardFront.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("NationalCardFrontRequired"));

			UFlowField? nationalCardBack = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardBack));
			if (nationalCardBack == null || (nationalCardBack.Required && nationalCardBack.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("NationalCardBackRequired"));

			UFlowField? birthCertificateFirst = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.BirthCertificateFirst));
			if (birthCertificateFirst == null || (birthCertificateFirst.Required && birthCertificateFirst.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("BirthCertificateFirstRequired"));

			u.NationalCardFront = ImageCompressor.CompressBase64(nationalCardFront.Value!);
			u.NationalCardBack = ImageCompressor.CompressBase64(nationalCardBack.Value!);
			u.BirthCertificateFirst = ImageCompressor.CompressBase64(birthCertificateFirst.Value!);

			u.Tags.Add(TagUser.NationalCardFrontVerified);
			u.Tags.Add(TagUser.NationalCardBackVerified);
			u.Tags.Add(TagUser.BirthCertificateFirstVerified);
		}
		else if (p.Id == FlowStepIds.UserSelfieVideo) {
			UFlowField? video = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.VisualAuthentication));
			if (video == null || (video.Required && video.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("VideoRequired"));

			u.VisualAuthentication = video.Value.FromBase64();
			u.Tags.Add(TagUser.VisualAuthenticationVerified);
		}
		else if (p.Id == FlowStepIds.UserESignature) {
			UFlowField? signature = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.ESignature));
			if (signature == null || (signature.Required && signature.Value.IsNullOrEmpty())) return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("SignatureRequired"));

			u.ESignature = ImageCompressor.CompressBase64(signature.Value!);
			u.Tags.Add(TagUser.ESignatureVerified);
		}
		else return new UResponse<UFlowStepGet?>(null, Usc.BadRequest, ls.Get("InvalidStep"));

		await db.SaveChangesAsync(ct);
		
		return await AuthenticationGet(new BaseParams { ApiKey = p.ApiKey, Token = p.Id }, ct);
	}
}