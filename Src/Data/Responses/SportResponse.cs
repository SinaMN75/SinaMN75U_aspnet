namespace SinaMN75U.Data.Responses;

public sealed class SportResponse : BaseResponse<TagSport, SportJson> {
	public required string Title { get; set; }
	public int Order { get; set; }
	public decimal MinLevel { get; set; }
	public decimal MaxLevel { get; set; }
}

public sealed class PlayerSportProfileResponse : BaseResponse<TagPlayerSportProfile, PlayerSportProfileJson> {
	public required decimal Level { get; set; }
	public required Guid UserId { get; set; }
	public required Guid SportId { get; set; }

	public UserResponse? User { get; set; }
	public SportResponse? Sport { get; set; }
}
