namespace SinaMN75U.Data.Params;

public sealed class AccountingReportParams : BaseParams {
	public Guid? UserId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class OrganizationSettlementRequestParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("amountRequired")]
	public decimal Amount { get; set; }

	[UValidationRequired("iBanIsRequired")]
	public string Iban { get; set; } = "";
}

public sealed class OrganizationSettlementProcessParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("idIsRequired")]
	public Guid SettlementId { get; set; }

	public bool Approve { get; set; }
	public string? Note { get; set; }
}

public sealed class AccountCreateParams : BaseCreateParams<TagAccount> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("numberRequired")]
	public string Code { get; set; } = null!;

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;
}

public sealed class AccountUpdateParams : BaseUpdateParams<TagAccount> {
	public string? Code { get; set; }
	public string? Title { get; set; }
}

public sealed class AccountReadParams : BaseReadParams<TagAccount> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public Guid? PlaceId { get; set; }
}

public sealed class VoucherLineParams {
	public Guid AccountId { get; set; }
	public decimal Debit { get; set; }
	public decimal Credit { get; set; }
	public Guid? PersonId { get; set; }
	public string? Description { get; set; }
}

public sealed class VoucherCreateParams : BaseCreateParams<TagVoucher> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public DateTime? Date { get; set; }
	public Guid? PlaceId { get; set; }

	[UValidationMinCollectionLength(2, "amountIsNotValid")]
	public List<VoucherLineParams> Lines { get; set; } = [];
}

public sealed class VoucherReadParams : BaseReadParams<TagVoucher> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? PersonId { get; set; }
	public Guid? AccountId { get; set; }
	public Guid? SourceId { get; set; }
}

public sealed class LedgerReadParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? AccountId { get; set; }
	public ICollection<TagAccount>? AccountTags { get; set; }
	public Guid? PersonId { get; set; }
	public Guid? PlaceId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class LedgerReportParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class CheckCreateParams : BaseCreateParams<TagCheck> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("amountRequired")]
	public decimal Amount { get; set; }

	[UValidationRequired("dateIsRequired")]
	public DateTime DueDate { get; set; }

	[UValidationRequired("numberRequired")]
	public string Number { get; set; } = null!;

	public string? Bank { get; set; }
	public string? SayadId { get; set; }
	public string? Drawer { get; set; }
	public Guid? PersonId { get; set; }
	public Guid? ContractId { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AccountId { get; set; }
}

public sealed class CheckReadParams : BaseReadParams<TagCheck> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? PersonId { get; set; }
	public Guid? ContractId { get; set; }
	public DateTime? FromDueDate { get; set; }
	public DateTime? ToDueDate { get; set; }
}

public sealed class CheckStatusParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public TagCheck Status { get; set; }
	public Guid? AccountId { get; set; }
	public DateTime? Date { get; set; }
}

public sealed class InvoiceReceiveCheckParams {
	public string Number { get; set; } = "";
	public DateTime DueDate { get; set; }
	public string? Bank { get; set; }
	public string? SayadId { get; set; }
	public string? Drawer { get; set; }
}

public sealed class InvoiceReceiveParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public decimal? Amount { get; set; }
	public Guid? AccountId { get; set; }
	public InvoiceReceiveCheckParams? Check { get; set; }
}
