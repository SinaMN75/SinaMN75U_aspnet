namespace SinaMN75U.Services;

public interface IProcessService {
	Task<UResponse<UProcessStepGetResponse?>> Get(IdStringParams p, CancellationToken ct);
	Task<UResponse<UProcessStepGetResponse?>> Send(UProcessStepSend p, CancellationToken ct);
}

public class ProcessService(DbContext db, ILocalizationService ls, ITokenService ts) : IProcessService {
	public async Task<UResponse<UProcessStepGetResponse?>> Get(IdStringParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		return p.Id switch {
			ProcessIds.Kyc => await KycGet(userData.Id, ct),
			_ => new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("ProcessNotFound"))
		};
	}

	public async Task<UResponse<UProcessStepGetResponse?>> Send(UProcessStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		

		UserEntity? u = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		UResponse<UProcessStepGetResponse?>? error = p.StepId switch {
			ProcessStepIds.UserData => ApplyUserData(u, p),
			ProcessStepIds.UserDocument => ApplyUserDocument(u, p),
			ProcessStepIds.UserSelfieVideo => ApplyUserSelfieVideo(u, p),
			ProcessStepIds.UserESignature => ApplyUserESignature(u, p),
			_ => new UResponse<UProcessStepGetResponse?>(null, Usc.BadRequest, ls.Get("InvalidStep"))
		};

		if (error != null) return error;

		await db.SaveChangesAsync(ct);
		return await KycGet(userData.Id, ct);
	}

	// ── KYC ──────────────────────────────────────────────────────────────────

	private async Task<UResponse<UProcessStepGetResponse?>> KycGet(Guid userId, CancellationToken ct) {
		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(new UserSelectorArgs {
				Wallet = new WalletSelectorArgs(),
				Merchant = new MerchantSelectorArgs { Terminal = new TerminalSelectorArgs() },
				NationalCardFront = true, 
				NationalCardBack = true,
				BirthCertificateFirst = true, 
				VisualAuthentication = true, ESignature = true
			}))
			.FirstOrDefaultAsync(x => x.Id == userId, ct);
		if (e == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		// Each step: (fields, isSubmitted, isVerified)
		// Fields and status logic live together — add a new step by adding a new entry here
		List<(string Id, string Title, string Desc, Func<List<UProcessField>> Fields, bool IsSubmitted, bool IsVerified)> steps = [
			(
				ProcessStepIds.UserData, "اطلاعات هویتی", "اطلاعات خود را تکمیل کنید.",
				Fields: () => [
					new UProcessField { Key = nameof(UserEntity.FirstName), Label = "نام", Type = TagFieldType.Text, Required = false, Value = e.FirstName, TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 } },
					new UProcessField { Key = nameof(UserEntity.LastName), Label = "نام خانوادگی", Type = TagFieldType.Text, Required = false, Value = e.LastName, TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 } },
					new UProcessField { Key = nameof(UserEntity.JsonData.FatherName), Label = "نام پدر", Type = TagFieldType.Text, Required = true, Value = e.JsonData.FatherName, TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 } },
					new UProcessField { Key = nameof(UserEntity.Birthdate), Label = "تاریخ تولد", Type = TagFieldType.Text, Required = true, Value = e.Birthdate?.ToString(), TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.PersianDate } }
				],
				IsSubmitted: e.JsonData.FatherName != null && e.Birthdate != null,
				IsVerified: e.JsonData.FatherName != null && e.Birthdate != null
			),
			(
				ProcessStepIds.UserDocument, "آپلود مدارک شناسایی", "لطفا مدارک شناسایی خود را آپلود کنید",
				Fields: () => [
					new UProcessField { Key = nameof(UserEntity.NationalCardFront), Label = "روی کارت ملی", Type = TagFieldType.File, Required = true, Value = e.NationalCardFront, FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true } },
					new UProcessField { Key = nameof(UserEntity.NationalCardBack), Label = "پشت کارت ملی", Type = TagFieldType.File, Required = true, Value = e.NationalCardBack, FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true } },
					new UProcessField { Key = nameof(UserEntity.BirthCertificateFirst), Label = "صفحه اول شناسنامه", Type = TagFieldType.File, Required = true, Value = e.BirthCertificateFirst, FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true } }
				],
				IsSubmitted:
				e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardFrontVerified, TagUser.NationalCardFrontAwaitingVerification) &&
				e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardBackVerified, TagUser.NationalCardBackAwaitingVerification) &&
				e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.BirthCertificateFirstVerified, TagUser.BirthCertificateFirstAwaitingVerification),
				IsVerified:
				e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
				e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
				e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified)
			),
			(
				ProcessStepIds.UserSelfieVideo, "ویدیو احراز هویت", "لطفا ویدیو احراز هویت خود را آپلود کنید",
				Fields: () => [
					new UProcessField { Key = nameof(UserEntity.VisualAuthentication), Label = "ویدیو", Type = TagFieldType.File, Required = true, Value = e.VisualAuthentication, FileConfig = new UFileConfig { Type = TagFileFieldType.Video, IsSelfieCamera = true } }
				],
				IsSubmitted: e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.VisualAuthenticationVerified, TagUser.VisualAuthenticationAwaitingVerification),
				IsVerified: e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified)
			),
			(
				ProcessStepIds.UserESignature, "امضای دیجیتال", "امضا",
				Fields: () => [
					new UProcessField { Key = nameof(UserEntity.ESignature), Label = "امضا دیجیتال", Type = TagFieldType.ESignature, Required = true, Value = e.ESignature }
				],
				IsSubmitted: e.ESignature.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.ESignatureVerified, TagUser.ESignatureAwaitingVerification),
				IsVerified: e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified)
			)
		];

		// Build step statuses — a step is locked until the previous one is submitted
		bool previousSubmitted = true;
		List<UProcessStepStatusResponse> stepStatuses = steps.Select(s => {
			TagProcessStepStatus status = previousSubmitted
				? s.IsVerified ? TagProcessStepStatus.Verified
				: s.IsSubmitted ? TagProcessStepStatus.AwaitingVerification
				: TagProcessStepStatus.NotStarted
				: TagProcessStepStatus.NotStarted;
			previousSubmitted = s.IsSubmitted;
			return new UProcessStepStatusResponse { Id = s.Id, Title = s.Title, Status = status };
		}).ToList();

		// First NotStarted → Current
		UProcessStepStatusResponse? current = stepStatuses.FirstOrDefault(s => s.Status == TagProcessStepStatus.NotStarted);
		if (current != null) current.Status = TagProcessStepStatus.Current;

		// All verified → completed
		if (steps.All(s => s.IsVerified))
			return new UResponse<UProcessStepGetResponse?>(
				new UProcessStepGetResponse {
					Id = ProcessStepIds.AuthCompleted, Title = "احراز هویت تکمیل شد",
					Description = "فرایند احراز هویت با موفقیت تکمیل شده است",
					Message = "فرایند احراز هویت با موفقیت تکمیل شده است", Steps = stepStatuses,
					MessageBox = new UMessageBox { Title = "تکمیل شد", Description = "فرایند احراز هویت با موفقیت انجام شد.", SvgIcon = USvgs.ShieldInfo }
				},
				Usc.ProcessCompleted, ls.Get("ProcessCompleted")
			);

		// First unsubmitted step → show its form
		var active = steps.FirstOrDefault(s => !s.IsSubmitted);

		// All submitted, not all verified → admin review
		if (active == default)
			return new UResponse<UProcessStepGetResponse?>(new UProcessStepGetResponse {
				Id = ProcessStepIds.AdminApproval, Title = "در انتظار تایید", Steps = stepStatuses,
				MessageBox = new UMessageBox { Title = "در انتظار تایید", Description = "مدارک شما دریافت شد و در حال بررسی است. نتیجه از طریق پیامک اطلاع‌رسانی خواهد شد.", SvgIcon = USvgs.ShieldInfo }
			});

		return new UResponse<UProcessStepGetResponse?>(new UProcessStepGetResponse {
			Id = active.Id, Title = active.Title, Description = active.Desc,
			Steps = stepStatuses, Fields = active.Fields()
		});
	}

	// ── Apply methods ─────────────────────────────────────────────────────────

	private UResponse<UProcessStepGetResponse?>? ApplyUserData(UserEntity u, UProcessStepSend p) {
		UProcessField? fatherName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.JsonData.FatherName));
		if (fatherName?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("FatherNameRequired"));

		UProcessField? birthdate = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.Birthdate));
		if (birthdate?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("BirthDateRequired"));

		if (!DateTime.TryParse(birthdate.Value!, out DateTime parsedDate)) return Fail(ls.Get("InvalidDateFormat"));

		u.JsonData.FatherName = fatherName.Value;
		u.Birthdate = parsedDate;

		UProcessField? firstName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.FirstName));
		if (firstName?.Value.IsNotNullOrEmpty() == true) u.FirstName = firstName.Value;

		UProcessField? lastName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.LastName));
		if (lastName?.Value.IsNotNullOrEmpty() == true) u.LastName = lastName.Value;

		return null;
	}

	private UResponse<UProcessStepGetResponse?>? ApplyUserDocument(UserEntity u, UProcessStepSend p) {
		UProcessField? front = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardFront));
		if (front?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("NationalCardFrontRequired"));

		UProcessField? back = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.NationalCardBack));
		if (back?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("NationalCardBackRequired"));

		UProcessField? cert = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.BirthCertificateFirst));
		if (cert?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("BirthCertificateFirstRequired"));

		u.NationalCardFront = ImageCompressor.CompressBase64(front.Value!);
		u.NationalCardBack = ImageCompressor.CompressBase64(back.Value!);
		u.BirthCertificateFirst = ImageCompressor.CompressBase64(cert.Value!);
		u.Tags.Add(TagUser.NationalCardFrontAwaitingVerification);
		u.Tags.Add(TagUser.NationalCardBackAwaitingVerification);
		u.Tags.Add(TagUser.BirthCertificateFirstAwaitingVerification);

		return null;
	}

	private UResponse<UProcessStepGetResponse?>? ApplyUserSelfieVideo(UserEntity u, UProcessStepSend p) {
		UProcessField? video = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.VisualAuthentication));
		if (video?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("VisualAuthenticationRequired"));

		u.VisualAuthentication = video.Value.FromBase64();
		u.Tags.Add(TagUser.VisualAuthenticationAwaitingVerification);

		return null;
	}

	private UResponse<UProcessStepGetResponse?>? ApplyUserESignature(UserEntity u, UProcessStepSend p) {
		UProcessField? signature = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.ESignature));
		if (signature?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("SignatureRequired"));

		u.ESignature = ImageCompressor.CompressBase64(signature.Value!);
		u.Tags.Add(TagUser.ESignatureAwaitingVerification);

		return null;
	}

	private static UResponse<UProcessStepGetResponse?> Fail(string message) => new(null, Usc.BadRequest, message);
}