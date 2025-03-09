namespace SinaMN75U.Data.Responses;

public class UResponse<T> : UResponse {
	public UResponse(T result, USC status = USC.Success, string message = "") {
		Result = result;
		Status = status;
		Message = message;
	}

	public T? Result { get; }
}

public class UResponse(USC status = USC.Success, string message = "") {
	public USC Status { get; protected set; } = status;
	public int? PageSize { get; set; }
	public int? PageCount { get; set; }
	public int? TotalCount { get; set; }
	public string Message { get; set; } = message;
}