namespace SinaMN75U.Data.Params;

public sealed class NotificationCreateParams : BaseCreateParams<TagNotification> {
	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }
}

public sealed class NotificationUpdateParams : BaseUpdateParams<TagNotification>;

public sealed class NotificationReadParams : BaseReadParams<TagNotification> {
	public Guid? UserId { get; set; }
	public NotificationSelectorArgs SelectorArgs { get; set; } = new();
}