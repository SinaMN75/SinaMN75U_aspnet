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
	PayloadTooLarge = 413,
	InternalServerError = 500,

	ThirdPartyError = 600,
	WrongVerificationCode = 601,
	MaximumLimitReached = 602,
	UserNotFound = 603,
	ExpiredToken = 604,
	ShahkarException = 605,
	ShahkarError = 606,
	BalanceIsLow = 607,
	
	SecurityError = 701,
	
	MediaTypeNotSupported = 802
}

public enum TagSmsPanel {
	NikSms = 101,
	Ghasedak = 102,
	Kavenegar = 103
}

public enum TagUser {
	Male = 101,
	Female = 102,
	Unspecified = 103,
	SuperAdmin = 201,
	Guest = 202,
	SystemAdmin = 203,
	SystemUser = 204,
	ESignatureVerified = 301,
	BirthCertificate1Verified = 302,
	BirthCertificate2Verified = 303,
	BirthCertificate3Verified = 304,
	BirthCertificate4Verified = 305,
	NationalCardFrontVerified = 306,
	NationalCardBackVerified = 307,
	VisualAuthenticationVerified = 308
}

public enum TagCategory {
	Category = 101,
	Exam = 102,
	User = 103,
	Menu = 104,
	Speciality = 105,
	Dorm = 106,
	Room = 107,
	Bed = 108,
	Enabled = 201,
	Disabled = 202,
	Hidden = 203
}

public enum TagMedia {
	Image = 101,
	Profile = 102
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
	Room10 = 410
}

public enum TagComment {
	Released = 101,
	InQueue = 102,
	Rejected = 103,
	Private = 201
}

public enum TagReaction {
	Like = 101,
	DisLike = 102
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

public enum TagContract {
	Daily = 101,
	Weekly = 102,
	Monthly = 103,
	Yearly = 104,
	SingleInvoice = 201
}

public enum TagInvoice {
	Deposit = 101,
	Rent = 102,
	Paid = 201,
	PaidOnline = 202,
	PaidManual = 203,
	NotPaid = 204
}

public enum TagTicket {
	SuperAdmin = 101,
	Admin = 102
}

public enum TagTxn {
	CreditCard = 101,
	Cash = 102,
	Pending = 201,
	Paid = 202,
	Failed = 203,
	Refunded = 204,
	ChargeWallet = 301
}

public enum TagParking {
	Test = 999
}

public enum TagVehicle {
	Test = 999
}

public enum TagParkingReport {
	Test = 999
}

public enum TagAddress {
	Verified = 101
}

public enum TagWallet {
	Primary = 101
}

public enum TagWalletTxn {
	Charge = 101,
	Transfer = 102,
	
	MobileAndNationalCodeVerification = 201,
	ZipCodeToAddressDetail = 202,
	VehicleViolationsDetail = 203,
	DrivingLicenceStatus = 204,
	LicencePlateDetail = 205,
	DrivingLicenceNegativePoint = 206,
	IBanToBankAccountDetail = 207,
	FreewayTolls = 208
}

public enum TagUserExtra {
	Verified = 101
}

public enum TagTerminal {
	Atm = 101,
	WallCashless = 102,
	DeskCashless = 103,
	Verified = 104,
	Suspended = 105
}

public enum TagBankAccount {
	Verified = 101
}

public enum TagIpg {
	Pn = 101
}

public enum TagMpg {
	Pn = 101
}

public enum TagPayment {
	ChargeWallet = 101
}

public enum TagInquiryHistory {
	ValidateNationalCodeAndPhoneNumber = 101,
	ZipCodeToAddressDetail = 201,
	VehicleViolationsDetail = 301,
	DrivingLicenceDetail = 302,
	LicencePlateDetail = 303,
	DrivingLicenceNegativePoint = 304,
	FreewayTolls = 305,
	IBanToBankAccountDetail = 501,
	Verified = 601,
	NotVerified = 602,
	Error = 603,
	ItHub = 701
}

public enum TagAgreement {
	TerminalRequest = 101,
	Signed = 201,
	Verified = 202
}

public enum TagNotification {
	Test = 999
}

public enum TagTxnErrorCodes {
	LowBalance = 101,
	UnAuthorized = 102,
	SenderWalletNotFound = 103,
	ReceiverWalletNotFound = 104,
	SecurityError = 105,
	Ok = 201
}

public enum TagVas {
	Water = 101
}

public enum TagSimCard {
	IranCell = 1,
	HamrahAvval = 2,
	Rigthel = 3,
	Shatel = 5
}

public enum TagSimChargeType {
	Normal = 0,
	Javanan = 2,
	Banovan = 3,
	Delkhah = 5
}

public enum TagIrancelChargeType {
	Normal = 0,
	Shegeftangiz = 1,
	Bill = 2
}

public enum TagRightelChargeType {
	Normal = 0,
	ShoorAngiz = 1
}

public enum TagShatelChargeType {
	Normal = 0
}

public enum TagMerchant {
	Normal = 101
}