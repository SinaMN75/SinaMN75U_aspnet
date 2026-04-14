namespace SinaMN75U.Services;

public interface IAgreementService {
	Task<UResponse<AgreementResponse?>> GenerateAgreement(GenerateAgreementParams p, CancellationToken ct);
}

public class AgreementService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IAgreementService {
	public async Task<UResponse<AgreementResponse?>> GenerateAgreement(GenerateAgreementParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AgreementResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		AgreementEntity? existingAgreement = await db.Set<AgreementEntity>().FirstOrDefaultAsync(x => x.TerminalId == p.TerminalId, ct);
		if (existingAgreement != null) {
			if ((DateTime.UtcNow - existingAgreement.CreatedAt).Days > 1) db.Set<AgreementEntity>().Remove(existingAgreement);
			else return new UResponse<AgreementResponse?>(existingAgreement.MapToResponse());
		} 
		
		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking()
			.Include(x => x.Creator).ThenInclude(x => x.Extra)
			.FirstOrDefaultAsync(x => x.Id == p.TerminalId, ct);
		if (terminal == null) return new UResponse<AgreementResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFound"));
		
		UserEntity user = terminal.Creator;
		
		string agreement = await WordPdfGenerator.GenerateWithTextsAndImagesAsync(
			texts: new Dictionary<string, string> {
				{ "nationalCode", user.NationalCode! },
			},
			imagesBase64: new Dictionary<string, string> {
				{ "customerSignature", user.Extra.ESignature! },
			},
			templatePath: Path.Combine(Directory.GetCurrentDirectory(), "Templates", "atmAgreement.docx")
		);

		EntityEntry<AgreementEntity> e = await db.Set<AgreementEntity>().AddAsync(new AgreementEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData(),
			Tags = [TagAgreement.TerminalRequest, TagAgreement.Signed],
			TerminalId = terminal.Id,
			Agreement = agreement
		}, ct);
		
		await db.SaveChangesAsync(ct);
		
		return new UResponse<AgreementResponse?>(e.Entity.MapToResponse());
	}
}