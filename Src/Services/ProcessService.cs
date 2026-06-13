namespace SinaMN75U.Services;


// ─────────────────────────────────────────────────────────────────────────────
// DTOs & Models
// ─────────────────────────────────────────────────────────────────────────────

public enum TagProcessStepStatus {
	NotStarted,
	Current,
	AwaitingVerification,
	Verified
}

public class UProcessStepStatus {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public TagProcessStepStatus Status { get; set; }
}

public class UProcessField {
	public string Key { get; set; } = "";
	public string Label { get; set; } = "";
	public string? Value { get; set; }
	public bool Required { get; set; }
	public TagFieldType Type { get; set; }
	public UTextFieldConfig? TextFieldConfig { get; set; }
	public UFileConfig? FileConfig { get; set; }
}

public class UTextFieldConfig {
	public TagTextFieldType Type { get; set; }
	public int? MaxLength { get; set; }
	public int? MinLength { get; set; }
}

public class UFileConfig {
	public TagFileFieldType Type { get; set; }
	public bool IsCamera { get; set; }
	public bool IsSelfieCamera { get; set; }
}

public class UProcessStepGet {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public string Description { get; set; } = "";
	public string? Message { get; set; }
	public List<UProcessField> Fields { get; set; } = [];
	public List<UProcessStepStatus> Steps { get; set; } = [];
}

public class UProcessStepSend : BaseParams {
	public string Id { get; set; } = "";
	public List<UProcessField> Fields { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────
// Process handler abstraction — implement one per process type
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Implement this interface for each process (KYC, merchant onboarding, etc).
/// ProcessService discovers handlers by ProcessId and delegates all logic here.
/// </summary>
public interface IProcessHandler {
	/// <summary>The process ID this handler owns, e.g. ProcessIds.Kyc</summary>
	string ProcessId { get; }

	/// <summary>Returns the current step the user should act on, plus full step statuses.</summary>
	Task<UResponse<UProcessStepGet?>> GetAsync(JwtClaimData userData, CancellationToken ct);

	/// <summary>Validates and persists one step's submitted fields, then returns the new current step.</summary>
	Task<UResponse<UProcessStepGet?>> SendAsync(JwtClaimData userData, UProcessStepSend p, CancellationToken ct);
}

// ─────────────────────────────────────────────────────────────────────────────
// Generic ProcessService — just a router, zero business logic
// ─────────────────────────────────────────────────────────────────────────────

public interface IProcessService {
	Task<UResponse<UProcessStepGet?>> Get(IdStringParams p, CancellationToken ct);
	Task<UResponse<UProcessStepGet?>> Send(UProcessStepSend p, CancellationToken ct);
}

public class ProcessService(
	IEnumerable<IProcessHandler> handlers,
	ILocalizationService ls,
	ITokenService ts
) : IProcessService {
	// Build a lookup once at startup: processId -> handler
	private readonly Dictionary<string, IProcessHandler> _handlers =
		handlers.ToDictionary(h => h.ProcessId, h => h);

	public async Task<UResponse<UProcessStepGet?>> Get(IdStringParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<UProcessStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (!_handlers.TryGetValue(p.Id, out IProcessHandler? handler))
			return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("ProcessNotFound"));

		return await handler.GetAsync(userData, ct);
	}

	public async Task<UResponse<UProcessStepGet?>> Send(UProcessStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<UProcessStepGet?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		// p.Id here is a step ID (e.g. ProcessStepIds.UserData), not a process ID.
		// We find the handler that owns this step.
		IProcessHandler? handler = _handlers.Values.FirstOrDefault(h => h.OwnsStep(p.Id));
		if (handler == null)
			return new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("InvalidStep"));

		return await handler.SendAsync(userData, p, ct);
	}
}

// ─────────────────────────────────────────────────────────────────────────────
// IProcessHandler extension — step ownership
// ─────────────────────────────────────────────────────────────────────────────

public static class ProcessHandlerExtensions {
	/// <summary>
	/// Handlers declare which step IDs they own so the router can match Send calls.
	/// Default implementation returns false; override in each handler.
	/// </summary>
	public static bool OwnsStep(this IProcessHandler handler, string stepId) =>
		handler is IStepOwner owner && owner.StepIds.Contains(stepId);
}

public interface IStepOwner {
	IReadOnlySet<string> StepIds { get; }
}

// ─────────────────────────────────────────────────────────────────────────────
// KYC process handler
// ─────────────────────────────────────────────────────────────────────────────

public class KycProcessHandler(
	DbContext db,
	ILocalizationService ls
) : IProcessHandler, IStepOwner {
	public string ProcessId => ProcessIds.Kyc;

	public IReadOnlySet<string> StepIds { get; } = new HashSet<string> {
		ProcessStepIds.UserData,
		ProcessStepIds.UserDocument,
		ProcessStepIds.UserSelfieVideo,
		ProcessStepIds.UserESignature
	};

	// ── Get ──────────────────────────────────────────────────────────────────

	public async Task<UResponse<UProcessStepGet?>> GetAsync(JwtClaimData userData, CancellationToken ct) =>
		await ResolveCurrentStep(userData.Id, ct);

	// ── Send ─────────────────────────────────────────────────────────────────

	public async Task<UResponse<UProcessStepGet?>> SendAsync(JwtClaimData userData, UProcessStepSend p, CancellationToken ct) {
		UserEntity? u = await db.Set<UserEntity>()
			.AsTracking()
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (u == null)
			return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		UResponse<UProcessStepGet?>? validationError = p.Id switch {
			ProcessStepIds.UserData => ApplyUserData(u, p),
			ProcessStepIds.UserDocument => ApplyUserDocument(u, p),
			ProcessStepIds.UserSelfieVideo => ApplyUserSelfieVideo(u, p),
			ProcessStepIds.UserESignature => ApplyUserESignature(u, p),
			_ => new UResponse<UProcessStepGet?>(null, Usc.BadRequest, ls.Get("InvalidStep"))
		};

		if (validationError != null) return validationError;

		await db.SaveChangesAsync(ct);
		return await ResolveCurrentStep(userData.Id, ct);
	}

	// ── Step apply methods (return null = success, non-null = early error) ───

	private UResponse<UProcessStepGet?>? ApplyUserData(UserEntity u, UProcessStepSend p) {
		UProcessField? fatherName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.JsonData.FatherName));
		if (fatherName?.Value.IsNullOrEmpty() != false)
			return Fail(ls.Get("FatherNameRequired"));

		UProcessField? birthdate = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.Birthdate));
		if (birthdate?.Value.IsNullOrEmpty() != false)
			return Fail(ls.Get("BirthDateRequired"));

		if (!DateTime.TryParse(birthdate.Value!, out DateTime parsedDate))
			return Fail(ls.Get("InvalidDateFormat"));

		u.JsonData.FatherName = fatherName.Value;
		u.Birthdate = parsedDate;

		// Optional fields — only update if client sent a non-empty value
		UProcessField? firstName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.FirstName));
		if (firstName?.Value.IsNotNullOrEmpty() == true) u.FirstName = firstName.Value;

		UProcessField? lastName = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.LastName));
		if (lastName?.Value.IsNotNullOrEmpty() == true) u.LastName = lastName.Value;

		return null;
	}

	private UResponse<UProcessStepGet?>? ApplyUserDocument(UserEntity u, UProcessStepSend p) {
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

	private UResponse<UProcessStepGet?>? ApplyUserSelfieVideo(UserEntity u, UProcessStepSend p) {
		UProcessField? video = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.VisualAuthentication));
		if (video?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("VideoRequired"));

		u.VisualAuthentication = video.Value.FromBase64();
		u.Tags.Add(TagUser.VisualAuthenticationAwaitingVerification);

		return null;
	}

	private UResponse<UProcessStepGet?>? ApplyUserESignature(UserEntity u, UProcessStepSend p) {
		UProcessField? signature = p.Fields.FirstOrDefault(x => x.Key == nameof(UserEntity.ESignature));
		if (signature?.Value.IsNullOrEmpty() != false) return Fail(ls.Get("SignatureRequired"));

		u.ESignature = ImageCompressor.CompressBase64(signature.Value!);
		u.Tags.Add(TagUser.ESignatureAwaitingVerification);

		return null;
	}

	// ── Step resolution ───────────────────────────────────────────────────────

	private async Task<UResponse<UProcessStepGet?>> ResolveCurrentStep(Guid userId, CancellationToken ct) {
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

		if (e == null)
			return new UResponse<UProcessStepGet?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		List<UProcessStepStatus> steps = BuildStepStatuses(e);

		// Completed
		if (e is { JsonData.FatherName: not null, Birthdate: not null } &&
		    e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
		    e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
		    e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified) &&
		    e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified) &&
		    e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified))
			return new UResponse<UProcessStepGet?>(
				new UProcessStepGet {
					Id = ProcessStepIds.AuthCompleted,
					Title = "فرایند احراز هویت تکمیل شده است",
					Description = "فرایند احراز هویت تکمیل شده است",
					Message = "فرایند احراز هویت تکمیل شده است",
					Steps = steps
				},
				Usc.ProcessCompleted, ls.Get("ProcessCompleted")
			);

		// Step 1 — personal data
		if (e.JsonData.FatherName == null || e.Birthdate == null)
			return Ok(new UProcessStepGet {
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

		// Step 2 — documents
		if (e.NationalCardFront.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.NationalCardFrontVerified, TagUser.NationalCardFrontAwaitingVerification) ||
		    e.NationalCardBack.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.NationalCardBackVerified, TagUser.NationalCardBackAwaitingVerification) ||
		    e.BirthCertificateFirst.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.BirthCertificateFirstVerified, TagUser.BirthCertificateFirstAwaitingVerification))
			return Ok(new UProcessStepGet {
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

		// Step 3 — selfie video
		if (e.VisualAuthentication.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.VisualAuthenticationVerified, TagUser.VisualAuthenticationAwaitingVerification))
			return Ok(new UProcessStepGet {
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

		// Step 4 — e-signature
		if (e.ESignature.IsNullOrEmpty() || !e.Tags.ContainsAny(TagUser.ESignatureVerified, TagUser.ESignatureAwaitingVerification))
			return Ok(new UProcessStepGet {
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

		// All submitted, awaiting admin review
		return Ok(new UProcessStepGet {
			Id = ProcessStepIds.AdminApproval,
			Title = "",
			Description = "",
			Message = "اطلاعات شما در دست بررسی است.",
			Steps = steps
		});
	}

	// ── Step status computation ───────────────────────────────────────────────

	/// <summary>
	/// Builds the full ordered status list for all KYC steps.
	/// Steps are evaluated in sequence — a step is NotStarted until the previous one is Verified.
	/// AwaitingVerification means submitted but not yet reviewed by admin.
	/// </summary>
	private static List<UProcessStepStatus> BuildStepStatuses(UserResponse e) {
		TagProcessStepStatus userData = ResolveUserDataStatus(e);
		TagProcessStepStatus documents = userData == TagProcessStepStatus.Verified ? ResolveDocumentStatus(e) : TagProcessStepStatus.NotStarted;
		TagProcessStepStatus selfie = documents == TagProcessStepStatus.Verified ? ResolveSelfieVideoStatus(e) : TagProcessStepStatus.NotStarted;
		TagProcessStepStatus signature = selfie == TagProcessStepStatus.Verified ? ResolveESignatureStatus(e) : TagProcessStepStatus.NotStarted;

		MarkCurrentStep(ref userData, ref documents, ref selfie, ref signature);

		return [
			new UProcessStepStatus { Id = ProcessStepIds.UserData, Title = "اطلاعات هویتی", Status = userData },
			new UProcessStepStatus { Id = ProcessStepIds.UserDocument, Title = "مدارک شناسایی", Status = documents },
			new UProcessStepStatus { Id = ProcessStepIds.UserSelfieVideo, Title = "ویدیو احراز هویت", Status = selfie },
			new UProcessStepStatus { Id = ProcessStepIds.UserESignature, Title = "امضای دیجیتال", Status = signature }
		];
	}

	private static TagProcessStepStatus ResolveUserDataStatus(UserResponse e) =>
		e.JsonData.FatherName != null && e.Birthdate != null
			? TagProcessStepStatus.Verified
			: TagProcessStepStatus.NotStarted;
	// Note: personal data has no AwaitingVerification — it's trusted immediately

	private static TagProcessStepStatus ResolveDocumentStatus(UserResponse e) {
		if (e.NationalCardFront.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardFrontVerified) &&
		    e.NationalCardBack.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.NationalCardBackVerified) &&
		    e.BirthCertificateFirst.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.BirthCertificateFirstVerified))
			return TagProcessStepStatus.Verified;

		if (e.NationalCardFront.IsNotNullOrEmpty() || e.NationalCardBack.IsNotNullOrEmpty() || e.BirthCertificateFirst.IsNotNullOrEmpty())
			return TagProcessStepStatus.AwaitingVerification;

		return TagProcessStepStatus.NotStarted;
	}

	private static TagProcessStepStatus ResolveSelfieVideoStatus(UserResponse e) {
		if (e.VisualAuthentication.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.VisualAuthenticationVerified)) return TagProcessStepStatus.Verified;
		if (e.VisualAuthentication.IsNotNullOrEmpty()) return TagProcessStepStatus.AwaitingVerification;
		return TagProcessStepStatus.NotStarted;
	}

	private static TagProcessStepStatus ResolveESignatureStatus(UserResponse e) {
		if (e.ESignature.IsNotNullOrEmpty() && e.Tags.Contains(TagUser.ESignatureVerified)) return TagProcessStepStatus.Verified;
		if (e.ESignature.IsNotNullOrEmpty()) return TagProcessStepStatus.AwaitingVerification;
		return TagProcessStepStatus.NotStarted;
	}

	/// <summary>
	/// Marks the first NotStarted step as Current.
	/// AwaitingVerification steps are left alone — user is waiting on admin.
	/// </summary>
	private static void MarkCurrentStep(
		ref TagProcessStepStatus userData,
		ref TagProcessStepStatus documents,
		ref TagProcessStepStatus selfie,
		ref TagProcessStepStatus signature
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

		if (signature == TagProcessStepStatus.NotStarted) signature = TagProcessStepStatus.Current;
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	private static UResponse<UProcessStepGet?> Ok(UProcessStepGet step) =>
		new(step);

	private static UResponse<UProcessStepGet?> Fail(string message) =>
		new(null, Usc.BadRequest, message);
}

// ─────────────────────────────────────────────────────────────────────────────
// DI Registration — in Program.cs / Startup.cs
// ─────────────────────────────────────────────────────────────────────────────
//
// builder.Services.AddScoped<IProcessService, ProcessService>();
// builder.Services.AddScoped<IProcessHandler, KycProcessHandler>();
//
// To add a new process later, just implement IProcessHandler + IStepOwner
// and register it — ProcessService picks it up automatically.
//
// Example:
//   builder.Services.AddScoped<IProcessHandler, MerchantOnboardingProcessHandler>();