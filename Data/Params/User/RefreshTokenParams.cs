namespace SinaMN75U.Data.Params.User;

public class RefreshTokenParams: BaseParam {
	[Required]
	public required string RefreshToken { get; set; }
}