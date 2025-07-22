namespace SinaMN75U.Constants;

public enum SqlDatabaseType {
	Postgres = 100,
	SqlServer = 101
}

public enum Usc {
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
	UserNotFound = 605,
	
	EmptyList = 701
}

public enum TagUser {
	Male = 100,
	Female = 101,
	SuperAdmin = 201,
	Guest = 202
}

public enum TagCategory {
	Category = 100,
	Exam = 101,
	User = 102,
	Menu = 103,
	Enabled = 201,
	Disabled = 202,
	Hidden = 203
}

public enum TagMedia {
	Image = 100
}

public enum TagProduct {
	Product = 101,
	Content = 102,
	Blog = 103,
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

public enum TagComment {
	Released = 100,
	InQueue = 101,
	Rejected = 102,
	Private = 501
}

public enum TagReaction {
	Like = 101,
	DisLike = 102
}

public enum TagExam {
	Test = 101
};