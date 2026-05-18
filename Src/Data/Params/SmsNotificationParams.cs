namespace SinaMN75U.Data.Params;

public sealed class SmsNotificationParams {
	public required string Mobile { get; set; }
	public required string Template { get; set; }
	public required string Text { get; set; }
	public string? Text2 { get; set; }
	public string? Text3 { get; set; }
}