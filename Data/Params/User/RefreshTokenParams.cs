namespace SinaMN75U.Data.Params.User;

public class RefreshTokenParams: BaseParams {
	[Required]
	public required string RefreshToken { get; set; }
}