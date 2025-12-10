namespace SinaMN75U.Constants;

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

	EmptyList = 701,

	MediaTypeNotEntered = 801,
	MediaTypeNotSupported = 802,
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
	Speciality = 104,
	Dorm = 105,
	Room = 106,
	Bed = 107, 
	Enabled = 201,
	Disabled = 202,
	Hidden = 203
}

public enum TagMedia {
	Image = 100,
	Profile = 101,
	Test = 999
}

public enum TagProduct {
	Product = 101,
	Content = 102,
	Blog = 103,
	Case = 104,
	Dorm = 105,
	Room = 106,
	Bed = 107,
	New = 201,
	KindOfNew = 202,
	Used = 203,
	Released = 301,
	Expired = 302,
	InQueue = 303,
	Deleted = 304,
	NotAccepted = 305,
	AwaitingPayment = 306,
	Room1 = 401,
	Room2 = 402,
	Room3 = 403,
	Room4 = 404,
	Room5 = 405,
	Room6 = 406,
	Room7 = 407,
	Room8 = 408,
	Room9 = 409,
	Room10 = 410,
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
	Test = 999
}

public enum TagFollow {
	User = 101,
	Product = 102,
	Category = 103
}

public enum TagContent {
	AboutUs = 101,
	Terms = 102,
	HomeSlider1 = 103
}

public enum TagBadge {
	Test = 999
}

public enum TagContract {
	Test = 999
}

public enum TagInvoice {
	Deposit = 101,
	Rent = 102,
	
	PaidOnline = 201,
	PaidManual = 202,
	NotPaid = 203,
	NotPaidDue = 204,
	
	Test = 999
}

public enum TagChatBot {
	DrHana = 101,
	Test = 999
}

public enum TagTicket {
	SuperAdmin = 101,
	Admin = 102,
	Test = 999
}