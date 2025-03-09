namespace SinaMN75U.Constants;

public enum USC {
	Success = 200,
	Created = 201,
	Deleted = 211,

	BadRequest = 400,
	UnAuthorized = 401,
	Forbidden = 403,
	NotFound = 404,
	Conflict = 409,

	WrongVerificationCode = 601,
	MaximumLimitReached = 602,
	UserAlreadyExist = 603,
	UserSuspended = 604,
	UserNotFound = 605,
	MultipleSeller = 607,
	OrderPayed = 608,
	OutOfStock = 610,
	NotEnoughMoney = 611,
	UserRecieverBlocked = 612,
	UserSenderBlocked = 613,
	MoreThan2UserIsInPrivateChat = 614,
	Overused = 615,
	MoreThanAllowedMoney = 616,
	WrongPassword = 617,
	InvalidDiscountCode = 618,
	S3Error = 619,

	Unhandled = 999
}

public enum TagUser {
	Male = 100,
	Female = 101
}

public enum TagCategory {
	Category = 100
}

public enum TagMedia {
	Image = 100
}