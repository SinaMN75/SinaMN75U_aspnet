namespace SinaMN75U.Data.Params;

public sealed class AccountingReportParams : BaseParams {
	public Guid? UserId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}
