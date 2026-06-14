namespace SinaMN75U.Services;

public class KycProcessService(
	DbContext db,
	ILocalizationService ls
) : IProcessHandlerService, IStepOwner {
	public string ProcessId => ProcessIds.Kyc;

	public IReadOnlySet<string> StepIds { get; } = new HashSet<string> {
		ProcessStepIds.UserData,
		ProcessStepIds.UserDocument,
		ProcessStepIds.UserSelfieVideo,
		ProcessStepIds.UserESignature,
		ProcessStepIds.AwaitingVerification
	};

	public async Task<UResponse<UProcessStepGetResponse?>> Get(JwtClaimData userData, CancellationToken ct) => await ResolveCurrentStep(userData.Id, ct);

	public async Task<UResponse<UProcessStepGetResponse?>> Send(JwtClaimData userData, UProcessStepSend p, CancellationToken ct) {
		UserEntity? u = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		UResponse<UProcessStepGetResponse?>? error = p.Id switch {
			ProcessStepIds.UserData => ApplyUserData(u, p),
			ProcessStepIds.UserDocument => ApplyUserDocument(u, p),
			ProcessStepIds.UserSelfieVideo => ApplyUserSelfieVideo(u, p),
			ProcessStepIds.UserESignature => ApplyUserESignature(u, p),
			_ => Fail(ls.Get("InvalidStep"))
		};

		if (error != null) return error;

		await db.SaveChangesAsync(ct);
		return await ResolveCurrentStep(userData.Id, ct);
	}

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

	// ── Step resolution ───────────────────────────────────────────────────────

	private async Task<UResponse<UProcessStepGetResponse?>> ResolveCurrentStep(Guid userId, CancellationToken ct) {
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
			.FirstOrDefaultAsync(x => x.Id == userId, ct);

		if (e == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		List<UProcessStepStatusResponse> steps = BuildStepStatuses(e);

		// ── Fully verified by admin ───────────────────────────────────────────
		if (e is { JsonData.FatherName: not null, Birthdate: not null } &&
		    e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
		    e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
		    e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified) &&
		    e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified) &&
		    e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified))
			return new UResponse<UProcessStepGetResponse?>(
				new UProcessStepGetResponse {
					Id = ProcessStepIds.AuthCompleted,
					Title = "احراز هویت تکمیل شد",
					Description = "فرایند احراز هویت با موفقیت تکمیل شده است",
					Message = "فرایند احراز هویت با موفقیت تکمیل شده است",
					Steps = steps,
					MessageBox = new UMessageBox {
						Title = "در انتظار تایید",
						Description = "مدارک شما دریافت شد و در حال بررسی است. نتیجه از طریق پیامک اطلاع‌رسانی خواهد شد.",
						SvgIcon = USvgs.ShieldInfo
					}
				},
				Usc.ProcessCompleted, ls.Get("ProcessCompleted")
			);

		// ── Step 1: personal data ─────────────────────────────────────────────
		if (e.JsonData.FatherName == null || e.Birthdate == null)
			return Ok(new UProcessStepGetResponse {
				Id = ProcessStepIds.UserData,
				Title = "اطلاعات هویتی",
				Description = "اطلاعات خود را تکمیل کنید.",
				Steps = steps,
				Fields = [
					new UProcessField {
						Label = "نام", Value = e.FirstName, Type = TagFieldType.Text, Required = false,
						Key = nameof(UserEntity.FirstName),
						TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 }
					},
					new UProcessField {
						Label = "نام خانوادگی", Value = e.LastName, Type = TagFieldType.Text, Required = false,
						Key = nameof(UserEntity.LastName),
						TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 }
					},
					new UProcessField {
						Label = "نام پدر", Value = e.JsonData.FatherName, Type = TagFieldType.Text, Required = true,
						Key = nameof(UserEntity.JsonData.FatherName),
						TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.Text, MaxLength = 40, MinLength = 2 }
					},
					new UProcessField {
						Label = "تاریخ تولد", Value = e.Birthdate?.ToString(), Type = TagFieldType.Text, Required = true,
						Key = nameof(UserEntity.Birthdate),
						TextFieldConfig = new UTextFieldConfig { Type = TagTextFieldType.PersianDate }
					}
				]
			});

		// ── Step 2: documents ─────────────────────────────────────────────────
		bool documentsSubmitted =
			e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardFrontVerified, TagUser.NationalCardFrontAwaitingVerification) &&
			e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardBackVerified, TagUser.NationalCardBackAwaitingVerification) &&
			e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.BirthCertificateFirstVerified, TagUser.BirthCertificateFirstAwaitingVerification);

		if (!documentsSubmitted)
			return Ok(new UProcessStepGetResponse {
				Id = ProcessStepIds.UserDocument,
				Title = "آپلود مدارک شناسایی",
				Description = "لطفا مدارک شناسایی خود را آپلود کنید",
				Steps = steps,
				Fields = [
					new UProcessField {
						Label = "روی کارت ملی", Value = e.NationalCardFront, Type = TagFieldType.File, Required = true,
						Key = nameof(UserEntity.NationalCardFront),
						FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true }
					},
					new UProcessField {
						Label = "پشت کارت ملی", Value = e.NationalCardBack, Type = TagFieldType.File, Required = true,
						Key = nameof(UserEntity.NationalCardBack),
						FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true }
					},
					new UProcessField {
						Label = "صفحه اول شناسنامه", Value = e.BirthCertificateFirst, Type = TagFieldType.File, Required = true,
						Key = nameof(UserEntity.BirthCertificateFirst),
						FileConfig = new UFileConfig { Type = TagFileFieldType.Image, IsCamera = true }
					}
				]
			});

		// ── Step 3: selfie video ──────────────────────────────────────────────
		bool selfieSubmitted =
			e.VisualAuthentication.IsNotNullOrEmpty() &&
			e.Tags.ContainsAny(TagUser.VisualAuthenticationVerified, TagUser.VisualAuthenticationAwaitingVerification);

		if (!selfieSubmitted)
			return Ok(new UProcessStepGetResponse {
				Id = ProcessStepIds.UserSelfieVideo,
				Title = "ویدیو احراز هویت",
				Description = "لطفا ویدیو احراز هویت خود را آپلود کنید",
				Steps = steps,
				Fields = [
					new UProcessField {
						Label = "ویدیو", Value = e.VisualAuthentication, Type = TagFieldType.File, Required = true,
						Key = nameof(UserEntity.VisualAuthentication),
						FileConfig = new UFileConfig { Type = TagFileFieldType.Video, IsSelfieCamera = true }
					}
				]
			});

		// ── Step 4: e-signature ───────────────────────────────────────────────
		bool signatureSubmitted = e.ESignature.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.ESignatureVerified, TagUser.ESignatureAwaitingVerification);

		if (!signatureSubmitted)
			return Ok(new UProcessStepGetResponse {
				Id = ProcessStepIds.UserESignature,
				Title = "امضای دیجیتال",
				Description = "امضا",
				Steps = steps,
				Fields = [
					new UProcessField {
						Label = "امضا دیجیتال", Value = e.ESignature, Type = TagFieldType.ESignature, Required = true,
						Key = nameof(UserEntity.ESignature)
					}
				]
			});

		// ── All submitted — waiting for admin review ───────────────────────────
		return Ok(new UProcessStepGetResponse {
			Id = ProcessStepIds.AdminApproval,
			Steps = steps,
			MessageBox = new UMessageBox {
				Title = "در انتظار تایید",
				Description = "مدارک شما دریافت شد و در حال بررسی است. نتیجه از طریق پیامک اطلاع‌رسانی خواهد شد.",
				SvgIcon = USvgs.ShieldInfo
			}
		});
	}

	// ── Step status computation ───────────────────────────────────────────────

	/// <summary>
	/// Computes status for the 4 user-action steps only.
	/// AdminApproval / AuthCompleted are process-level states, not steps.
	///
	/// Sequencing rule: a step is unlocked (not NotStarted) only after the
	/// previous step is Verified OR AwaitingVerification — i.e. the user has
	/// nothing left to do for it, regardless of admin review.
	/// </summary>
	private static List<UProcessStepStatusResponse> BuildStepStatuses(UserResponse e) {
		TagProcessStepStatus userData = ResolveUserDataStatus(e);

		// Each step unlocks when the previous is done (Verified or AwaitingVerification)
		bool userDataDone = userData is TagProcessStepStatus.Verified; // UserData has no AwaitingVerification
		TagProcessStepStatus documents = userDataDone ? ResolveDocumentStatus(e) : TagProcessStepStatus.NotStarted;

		bool documentsDone = documents is TagProcessStepStatus.Verified or TagProcessStepStatus.AwaitingVerification;
		TagProcessStepStatus selfie = documentsDone ? ResolveSelfieVideoStatus(e) : TagProcessStepStatus.NotStarted;

		bool selfieDone = selfie is TagProcessStepStatus.Verified or TagProcessStepStatus.AwaitingVerification;
		TagProcessStepStatus signature = selfieDone ? ResolveESignatureStatus(e) : TagProcessStepStatus.NotStarted;

		bool signatureDone = selfie is TagProcessStepStatus.Verified or TagProcessStepStatus.AwaitingVerification;
		TagProcessStepStatus awaitingVerification = signatureDone ? ResolveESignatureStatus(e) : TagProcessStepStatus.NotStarted;

		MarkCurrentStep(ref userData, ref documents, ref selfie, ref signature, ref awaitingVerification);

		return [
			new UProcessStepStatusResponse { Id = ProcessStepIds.UserData, Title = "اطلاعات هویتی", Status = userData },
			new UProcessStepStatusResponse { Id = ProcessStepIds.UserDocument, Title = "مدارک شناسایی", Status = documents },
			new UProcessStepStatusResponse { Id = ProcessStepIds.UserSelfieVideo, Title = "ویدیو احراز هویت", Status = selfie },
			new UProcessStepStatusResponse { Id = ProcessStepIds.UserESignature, Title = "امضای دیجیتال", Status = signature },
			new UProcessStepStatusResponse { Id = ProcessStepIds.AwaitingVerification, Title = "در انتظار تایید ادمین", Status = awaitingVerification }
		];
	}

	/// UserData: only FatherName + Birthdate are required (FirstName/LastName are optional)
	private static TagProcessStepStatus ResolveUserDataStatus(UserResponse e) =>
		e.JsonData.FatherName != null && e.Birthdate != null
			? TagProcessStepStatus.Verified
			: TagProcessStepStatus.NotStarted;
	// No AwaitingVerification for UserData — personal text fields are trusted immediately

	/// Documents: ALL three must be uploaded. AwaitingVerification once all three are submitted.
	private static TagProcessStepStatus ResolveDocumentStatus(UserResponse e) {
		bool allVerified =
			e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
			e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
			e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified);
		if (allVerified) return TagProcessStepStatus.Verified;

		// All three uploaded (even if only awaiting) = AwaitingVerification
		bool allSubmitted =
			e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardFrontVerified, TagUser.NationalCardFrontAwaitingVerification) &&
			e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.NationalCardBackVerified, TagUser.NationalCardBackAwaitingVerification) &&
			e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.ContainsAny(TagUser.BirthCertificateFirstVerified, TagUser.BirthCertificateFirstAwaitingVerification);
		if (allSubmitted) return TagProcessStepStatus.AwaitingVerification;

		return TagProcessStepStatus.NotStarted;
	}

	private static TagProcessStepStatus ResolveSelfieVideoStatus(UserResponse e) {
		if (e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified)) return TagProcessStepStatus.Verified;
		if (e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationAwaitingVerification)) return TagProcessStepStatus.AwaitingVerification;
		return TagProcessStepStatus.NotStarted;
	}

	private static TagProcessStepStatus ResolveESignatureStatus(UserResponse e) {
		if (e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified)) return TagProcessStepStatus.Verified;
		if (e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureAwaitingVerification)) return TagProcessStepStatus.AwaitingVerification;
		return TagProcessStepStatus.NotStarted;
	}

	/// <summary>
	/// Marks the first NotStarted step as Current.
	/// If nothing is NotStarted (all Verified or AwaitingVerification),
	/// nothing gets marked Current — correct for AdminApproval state.
	/// </summary>
	private static void MarkCurrentStep(
		ref TagProcessStepStatus userData,
		ref TagProcessStepStatus documents,
		ref TagProcessStepStatus selfie,
		ref TagProcessStepStatus signature,
		ref TagProcessStepStatus awaitingVerification
	) {
		if (userData == TagProcessStepStatus.NotStarted) {
			userData = TagProcessStepStatus.Current;
			return;
		}

		if (documents == TagProcessStepStatus.NotStarted) {
			documents = TagProcessStepStatus.Current;
			return;
		}

		if (selfie == TagProcessStepStatus.NotStarted) {
			selfie = TagProcessStepStatus.Current;
			return;
		}

		if (signature == TagProcessStepStatus.NotStarted) {
			signature = TagProcessStepStatus.Current;
			return;
		}

		if (awaitingVerification == TagProcessStepStatus.NotStarted) {
			awaitingVerification = TagProcessStepStatus.Current;
			return;
		}

		if (awaitingVerification == TagProcessStepStatus.NotStarted) awaitingVerification = TagProcessStepStatus.Current;
	}

	private static UResponse<UProcessStepGetResponse?> Ok(UProcessStepGetResponse step) => new(step);
	private static UResponse<UProcessStepGetResponse?> Fail(string message) => new(null, Usc.BadRequest, message);
}