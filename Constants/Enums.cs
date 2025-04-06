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
	UserNotFound = 605
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

public enum TagProduct {
	Product = 101,
	New = 201,
	KindOfNew = 202,
	Used = 203,
	Released = 301,
	Expired = 302,
	InQueue = 303,
	Deleted = 304,
	NotAccepted = 305,
	AwaitingPayment = 306
}