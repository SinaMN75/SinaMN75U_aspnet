namespace SinaMN75U.Services;

public interface IProcessService {
	Task<UResponse<UProcessStepGet?>> Get(IdStringParams p, CancellationToken ct);
	Task<UResponse<UProcessStepGet?>> Send(UProcessStepSend p, CancellationToken ct);
}

public class ProcessService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IProcessService {
	public async Task<UResponse<UProcessStepGet?>> Get(IdStringParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (p.Id == ProcessIds.Kyc) return await AuthenticationGet(userData, ct);
		return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("ProcessNotFound"));
	}

	public async Task<UResponse<UProcessStepGet?>> Send(UProcessStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? u = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null) return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (p.Id == ProcessStepIds.UserData) {
			UProcessField? fatherName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.JsonData.FatherName));
			if (fatherName == null || (fatherName.Required && fatherName.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("FatherNameRequired"));

			UProcessField? birthdate = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.Birthdate));
			if (birthdate == null || (birthdate.Required && birthdate.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("BirthDateRequired"));

			u.JsonData.FatherName = fatherName.Value;
			if (!DateTime.TryParse(birthdate.Value!, out DateTime parsedDate)) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("InvalidDateFormat"));
			u.Birthdate = parsedDate;
		}
		else if (p.Id == ProcessStepIds.UserDocument) {
			UProcessField? nationalCardFront = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardFront));
			if (nationalCardFront == null || (nationalCardFront.Required && nationalCardFront.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("NationalCardFrontRequired"));

			UProcessField? nationalCardBack = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardBack));
			if (nationalCardBack == null || (nationalCardBack.Required && nationalCardBack.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("NationalCardBackRequired"));

			UProcessField? birthCertificateFirst = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.BirthCertificateFirst));
			if (birthCertificateFirst == null || (birthCertificateFirst.Required && birthCertificateFirst.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("BirthCertificateFirstRequired"));

			u.NationalCardFront = ImageCompressor.CompressBase64(nationalCardFront.Value!);
			u.NationalCardBack = ImageCompressor.CompressBase64(nationalCardBack.Value!);
			u.BirthCertificateFirst = ImageCompressor.CompressBase64(birthCertificateFirst.Value!);

			u.Tags.Add(TagUser.NationalCardFrontAwaitingVerification);
			u.Tags.Add(TagUser.NationalCardBackAwaitingVerification);
			u.Tags.Add(TagUser.BirthCertificateFirstAwaitingVerification);
		}
		else if (p.Id == ProcessStepIds.UserSelfieVideo) {
			UProcessField? video = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.VisualAuthentication));
			if (video == null || (video.Required && video.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("VideoRequired"));

			u.VisualAuthentication = video.Value.FromBase64();
			u.Tags.Add(TagUser.VisualAuthenticationAwaitingVerification);
		}
		else if (p.Id == ProcessStepIds.UserESignature) {
			UProcessField? signature = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.ESignature));
			if (signature == null || (signature.Required && signature.Value.IsNullOrEmpty())) return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("SignatureRequired"));

			u.ESignature = ImageCompressor.CompressBase64(signature.Value!);
			u.Tags.Add(TagUser.ESignatureAwaitingVerification);
		}
		else return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("InvalidStep"));

		await db.SaveChangesAsync(ct);

		return await AuthenticationGet(userData, ct);
	}

	private async Task<UResponse<UProcessStepGet?>> AuthenticationGet(JwtClaimData userData, CancellationToken ct) {
		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(new UserSelectorArgs {
				Wallet = new WalletSelectorArgs(),
				Merchant = new MerchantSelectorArgs { Terminal = new TerminalSelectorArgs() },
				NationalCardFront = true,
				NationalCardBack = true,
				BirthCertificateFirst = true,
				VisualAuthentication = true,
				ESignature = true
			}))
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (e == null) return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (e is { FirstName: not null, LastName: not null, JsonData.FatherName: not null, Birthdate: not null } &&
		    e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
		    e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
		    e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified) &&
		    e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified) &&
		    e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified)
		   )
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.AuthCompleted,
					Title = "فرایند احراز هویت تکمیل شده است",
					Description = "فرایند احراز هویت تکمیل شده است",
					Message = "فرایند احراز هویت تکمیل شده است"
				},
				Usc.ProcessCompleted, ls.Get("ProcessCompleted")
			);

		if (e.FirstName == null || e.LastName == null || e.JsonData.FatherName == null || e.Birthdate == null)
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.UserData,
					Title = "اطلاعات هویتی",
					Description = "اطلاعات خود را تکمیل کنید.",
					Fields = [
						new UProcessField {
							Label = "نام",
							Value = e.FirstName,
							Type = TagFieldType.Text,
							Required = true,
							Key = nameof(UserEntity.FirstName),
							TextFieldConfig = new UTextFieldConfig {
								Type = TagTextFieldType.Text,
								MaxLength = 40,
								MinLength = 2
							}
						},
						new UProcessField {
							Label = "نام خانوادگی",
							Value = e.LastName,
							Type = TagFieldType.Text,
							Required = true,
							Key = nameof(UserEntity.LastName),
							TextFieldConfig = new UTextFieldConfig {
								Type = TagTextFieldType.Text,
								MaxLength = 40,
								MinLength = 2
							}
						},
						new UProcessField {
							Label = "نام پدر",
							Value = e.JsonData.FatherName,
							Type = TagFieldType.Text,
							Required = true,
							Key = nameof(UserEntity.JsonData.FatherName),
							TextFieldConfig = new UTextFieldConfig {
								Type = TagTextFieldType.Text,
								MaxLength = 40,
								MinLength = 2
							}
						},
						new UProcessField {
							Label = "تاریخ تولد",
							Value = e.Birthdate?.ToString(),
							Type = TagFieldType.Text,
							Required = true,
							Key = nameof(UserEntity.Birthdate),
							TextFieldConfig = new UTextFieldConfig {
								Type = TagTextFieldType.PersianDate
							}
						}
					]
				}
			);

		if (
			e.NationalCardFront.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.NationalCardFrontVerified, TagUser.NationalCardFrontAwaitingVerification) ||
			e.NationalCardBack.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.NationalCardBackVerified, TagUser.NationalCardBackAwaitingVerification) ||
			e.BirthCertificateFirst.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.BirthCertificateFirstVerified, TagUser.BirthCertificateFirstAwaitingVerification)
		)
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.UserDocument,
					Title = "آپلود مدارک شناسایی",
					Description = "لطفا مدارک شناسایی خود را آپلود کنید",
					Fields = [
						new UProcessField {
							Label = "روی کارت ملی",
							Value = e.NationalCardFront,
							Type = TagFieldType.File,
							Required = true,
							Key = nameof(UserEntity.NationalCardFront),
							FileConfig = new UFileConfig {
								Type = TagFileFieldType.Image,
								IsCamera = true
							}
						},
						new UProcessField {
							Label = "پشت کارت ملی",
							Value = e.NationalCardBack,
							Type = TagFieldType.File,
							Required = true,
							Key = nameof(UserEntity.NationalCardBack),
							FileConfig = new UFileConfig {
								Type = TagFileFieldType.Image,
								IsCamera = true
							}
						},
						new UProcessField {
							Label = "صفحه اول شناسنامه",
							Value = e.BirthCertificateFirst,
							Type = TagFieldType.File,
							Required = true,
							Key = nameof(UserEntity.BirthCertificateFirst),
							FileConfig = new UFileConfig {
								Type = TagFileFieldType.Image,
								IsCamera = true
							}
						}
					]
				}
			);

		if (e.VisualAuthentication.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.VisualAuthenticationVerified, TagUser.VisualAuthenticationAwaitingVerification))
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.UserSelfieVideo,
					Title = "ویدیو احراز هویت",
					Description = "لطفا ویدیو احراز هویت خود را آپلود کنید",
					Fields = [
						new UProcessField {
							Label = "ویدیو",
							Value = e.VisualAuthentication,
							Type = TagFieldType.File,
							Required = true,
							Key = nameof(UserEntity.VisualAuthentication),
							FileConfig = new UFileConfig {
								Type = TagFileFieldType.Video,
								IsSelfieCamera = true
							}
						}
					]
				}
			);

		if (e.ESignature.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.ESignatureVerified, TagUser.ESignatureAwaitingVerification))
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.UserESignature,
					Title = "امضای دیجیتال",
					Description = "امضا",
					Fields = [
						new UProcessField {
							Label = "امضا دیجیتال",
							Value = e.ESignature,
							Type = TagFieldType.ESignature,
							Required = true,
							Key = nameof(UserEntity.ESignature)
						}
					]
				}
			);

		return new UResponse<UProcessStepGet?>(new UProcessStepGet {
			Id = ProcessStepIds.AdminApproval,
			Title = "",
			Description = "",
			Message = "اطلاعات شما در دست بررسی است."
		});
	}
}