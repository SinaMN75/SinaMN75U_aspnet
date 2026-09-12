namespace SinaMN75U.Services;

public interface IBrokerService {
	Task<UResponse<Guid?>> CreateBroker(BrokerCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<BrokerResponse>?>> ReadBroker(BrokerReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBroker(BrokerUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateAgreementTemplate(AgreementTemplateCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AgreementTemplateResponse>?>> ReadAgreementTemplate(AgreementTemplateReadParams p, CancellationToken ct);
	Task<UResponse> UpdateAgreementTemplate(AgreementTemplateUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteAgreementTemplate(IdParams p, CancellationToken ct);
}

public class BrokerService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IBrokerService {
	public async Task<UResponse<Guid?>> CreateBroker(BrokerCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<BrokerEntity>().AnyAsync(x => x.Code == p.Code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("codeAlreadyExists"));

		BrokerEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Code = p.Code,
			AgreementTemplateId = p.AgreementTemplateId,
			JsonData = new BrokerJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				LegalName = p.LegalName,
				RegistrationNumber = p.RegistrationNumber,
				NationalId = p.NationalId,
				Address = p.Address,
				PostalCode = p.PostalCode,
				PhoneNumber = p.PhoneNumber,
				SupportPhoneNumber = p.SupportPhoneNumber,
				CallCenterPhoneNumber = p.CallCenterPhoneNumber,
				RepresentativeName = p.RepresentativeName,
				RepresentativeRole = p.RepresentativeRole,
				LogoBase64 = p.LogoBase64,
				ThemeColor = p.ThemeColor,
				ContractNumberSuffix = p.ContractNumberSuffix,
				Provider = p.Provider ?? TagBrokerProvider.Avreen,
				ProviderBaseUrl = p.ProviderBaseUrl,
				ProviderAuthHeader = p.ProviderAuthHeader,
				ProviderProject = p.ProviderProject,
				ProviderDefinitionTemplate = p.ProviderDefinitionTemplate ?? 1,
				Signatories = p.Signatories
			}
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<BrokerResponse>?>> ReadBroker(BrokerReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<BrokerResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<BrokerResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<IEnumerable<BrokerResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		IQueryable<BrokerEntity> q = db.Set<BrokerEntity>().ApplyReadParams(p);

		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code == p.Code);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));

		IQueryable<BrokerResponse> projected = q.Select(Projections.BrokerSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBroker(BrokerUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		BrokerEntity? e = await db.Set<BrokerEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("brokerNotFound"));
		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code && await db.Set<BrokerEntity>().AnyAsync(x => x.Code == p.Code, ct))
			return new UResponse(Usc.Conflict, ls.Get("codeAlreadyExists"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code!;
		if (p.AgreementTemplateId.IsNotNullOrEmpty()) e.AgreementTemplateId = p.AgreementTemplateId;

		if (p.LegalName != null) e.JsonData.LegalName = p.LegalName;
		if (p.RegistrationNumber != null) e.JsonData.RegistrationNumber = p.RegistrationNumber;
		if (p.NationalId != null) e.JsonData.NationalId = p.NationalId;
		if (p.Address != null) e.JsonData.Address = p.Address;
		if (p.PostalCode != null) e.JsonData.PostalCode = p.PostalCode;
		if (p.PhoneNumber != null) e.JsonData.PhoneNumber = p.PhoneNumber;
		if (p.SupportPhoneNumber != null) e.JsonData.SupportPhoneNumber = p.SupportPhoneNumber;
		if (p.CallCenterPhoneNumber != null) e.JsonData.CallCenterPhoneNumber = p.CallCenterPhoneNumber;
		if (p.RepresentativeName != null) e.JsonData.RepresentativeName = p.RepresentativeName;
		if (p.RepresentativeRole != null) e.JsonData.RepresentativeRole = p.RepresentativeRole;
		if (p.LogoBase64 != null) e.JsonData.LogoBase64 = p.LogoBase64;
		if (p.ThemeColor != null) e.JsonData.ThemeColor = p.ThemeColor;
		if (p.ContractNumberSuffix != null) e.JsonData.ContractNumberSuffix = p.ContractNumberSuffix;
		if (p.Provider != null) e.JsonData.Provider = p.Provider.Value;
		if (p.ProviderBaseUrl != null) e.JsonData.ProviderBaseUrl = p.ProviderBaseUrl;
		if (p.ProviderAuthHeader != null) e.JsonData.ProviderAuthHeader = p.ProviderAuthHeader;
		if (p.ProviderProject != null) e.JsonData.ProviderProject = p.ProviderProject;
		if (p.ProviderDefinitionTemplate != null) e.JsonData.ProviderDefinitionTemplate = p.ProviderDefinitionTemplate.Value;
		if (p.Signatories != null) e.JsonData.Signatories = p.Signatories;

		e.ApplyUpdateParam<BrokerEntity, TagBroker, BrokerJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		BrokerEntity? e = await db.Set<BrokerEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("brokerNotFound"));
		if (await db.Set<TerminalEntity>().AnyAsync(x => x.BrokerId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisBrokerIsUsedByTerminalsAndCannotBeDeleted"));
		if (await db.Set<TerminalBrandEntity>().AnyAsync(x => x.BrokerId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisBrokerHasBrandsAndCannotBeDeleted"));

		db.Set<BrokerEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Deleted, ls.Get("brokerDeletedSuccessfully"));
	}

	public async Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!await db.Set<BrokerEntity>().AnyAsync(x => x.Id == p.BrokerId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("brokerNotFound"));
		if (await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Code == p.Code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("codeAlreadyExists"));

		TerminalBrandEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Code = p.Code,
			BrokerId = p.BrokerId,
			JsonData = new TerminalBrandJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				RequiresSimCardSerial = p.RequiresSimCardSerial ?? true,
				RequiresImei = p.RequiresImei ?? false,
				ImageBase64 = p.ImageBase64,
				Order = p.Order,
				AgreementTemplateId = p.AgreementTemplateId,
				LegacyTag = p.LegacyTag
			}
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<TerminalBrandResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<TerminalBrandResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<TerminalBrandEntity> q = db.Set<TerminalBrandEntity>().ApplyReadParams(p);

		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code == p.Code);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.BrokerId.IsNotNullOrEmpty()) q = q.Where(x => x.BrokerId == p.BrokerId);
		if (!userData.IsAdmin) q = q.Where(x => x.Tags.Contains(TagTerminalBrand.Active));

		IQueryable<TerminalBrandResponse> projected = q.Select(Projections.TerminalBrandSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrandEntity? e = await db.Set<TerminalBrandEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));
		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code && await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Code == p.Code, ct))
			return new UResponse(Usc.Conflict, ls.Get("codeAlreadyExists"));
		if (p.BrokerId.IsNotNullOrEmpty() && !await db.Set<BrokerEntity>().AnyAsync(x => x.Id == p.BrokerId, ct))
			return new UResponse(Usc.NotFound, ls.Get("brokerNotFound"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code!;
		if (p.BrokerId.IsNotNullOrEmpty()) e.BrokerId = p.BrokerId!.Value;

		if (p.RequiresSimCardSerial != null) e.JsonData.RequiresSimCardSerial = p.RequiresSimCardSerial.Value;
		if (p.RequiresImei != null) e.JsonData.RequiresImei = p.RequiresImei.Value;
		if (p.ImageBase64 != null) e.JsonData.ImageBase64 = p.ImageBase64;
		if (p.Order != null) e.JsonData.Order = p.Order;
		if (p.AgreementTemplateId.IsNotNullOrEmpty()) e.JsonData.AgreementTemplateId = p.AgreementTemplateId;
		if (p.LegacyTag != null) e.JsonData.LegacyTag = p.LegacyTag;

		e.ApplyUpdateParam<TerminalBrandEntity, TagTerminalBrand, TerminalBrandJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrandEntity? e = await db.Set<TerminalBrandEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));
		if (await db.Set<TerminalEntity>().AnyAsync(x => x.BrandId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisBrandIsUsedByTerminalsAndCannotBeDeleted"));

		db.Set<TerminalBrandEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Deleted, ls.Get("terminalBrandDeletedSuccessfully"));
	}

	public async Task<UResponse<Guid?>> CreateAgreementTemplate(AgreementTemplateCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<AgreementTemplateEntity>().AnyAsync(x => x.Code == p.Code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("codeAlreadyExists"));

		AgreementTemplateEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Code = p.Code,
			JsonData = new AgreementTemplateJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				HeaderTitle = p.HeaderTitle,
				Blocks = Ordered(p.Blocks)
			}
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<AgreementTemplateResponse>?>> ReadAgreementTemplate(AgreementTemplateReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<AgreementTemplateResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<AgreementTemplateResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<IEnumerable<AgreementTemplateResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		IQueryable<AgreementTemplateEntity> q = db.Set<AgreementTemplateEntity>().ApplyReadParams(p);

		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code == p.Code);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));

		IQueryable<AgreementTemplateResponse> projected = q.Select(Projections.AgreementTemplateSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateAgreementTemplate(AgreementTemplateUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		AgreementTemplateEntity? e = await db.Set<AgreementTemplateEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("agreementTemplateNotFound"));
		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code && await db.Set<AgreementTemplateEntity>().AnyAsync(x => x.Code == p.Code, ct))
			return new UResponse(Usc.Conflict, ls.Get("codeAlreadyExists"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code!;
		if (p.HeaderTitle != null) e.JsonData.HeaderTitle = p.HeaderTitle;
		if (p.Blocks != null) e.JsonData.Blocks = Ordered(p.Blocks);

		e.ApplyUpdateParam<AgreementTemplateEntity, TagAgreementTemplate, AgreementTemplateJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteAgreementTemplate(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		AgreementTemplateEntity? e = await db.Set<AgreementTemplateEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("agreementTemplateNotFound"));
		if (await db.Set<BrokerEntity>().AnyAsync(x => x.AgreementTemplateId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisAgreementTemplateIsInUseAndCannotBeDeleted"));

		List<TerminalBrandEntity> brands = await db.Set<TerminalBrandEntity>().ToListAsync(ct);
		if (brands.Any(x => x.JsonData.AgreementTemplateId == p.Id)) return new UResponse(Usc.Conflict, ls.Get("thisAgreementTemplateIsInUseAndCannotBeDeleted"));

		db.Set<AgreementTemplateEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Deleted, ls.Get("agreementTemplateDeletedSuccessfully"));
	}

	private static List<AgreementTemplateBlock> Ordered(List<AgreementTemplateBlock> blocks) {
		int order = 0;
		foreach (AgreementTemplateBlock block in blocks.OrderBy(x => x.Order).ToList()) block.Order = order++;
		return blocks.OrderBy(x => x.Order).ToList();
	}
}
