namespace SinaMN75U.Data.Responses;

public sealed class SimCardResponse : BaseResponse<TagSimCard, BaseJsonData> {
	public required string Number { get; set; }
	public string? Serial { get; set; }

	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
}