namespace SinaMN75U.Data.Params;

// Optional date-range filter for dashboard aggregate reads. Null dates = server default window.
public sealed class DashboardRangeParams : BaseParams {
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}
