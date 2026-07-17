namespace SinaMN75U.Constants;

public enum TagOrderBy {
	CreatedAt = 101,
	CardNumber = 102,
	ZipCode = 103,
	Order = 104,
	Title = 105,
	Capacity = 106,
	City = 107,
	IsAvailable = 108,
	StartDate = 109,
	EndDate = 110,
	DueDate = 111,
	Mcc = 112,
	Code = 113,
	Amount = 114,
	UserName = 115,
	Brand = 116,
	Balance = 117,
	DurationMs = 118,

	CreatedAtDescending = 201,
	CardNumberDescending = 202,
	ZipCodeDescending = 203,
	OrderDescending = 204,
	TitleDescending = 205,
	CapacityDescending = 206,
	CityDescending = 207,
	IsAvailableDescending = 208,
	StartDateDescending = 209,
	EndDateDescending = 210,
	DueDateDescending = 211,
	MccDescending = 212,
	CodeDescending = 213,
	AmountDescending = 214,
	UserNameDescending = 215,
	BrandDescending = 216,
	BalanceDescending = 217,
	DurationMsDescending = 218,
}

public enum TagApiLog {
	Get = 101,
	Post = 102,
	Put = 103,
	Patch = 104,
	Delete = 105,
	Other = 106,

	Success = 201,
	ClientError = 202,
	ServerError = 203,

	HasException = 301,
}

public enum Usc {
	Success = 200,
	Created = 201,
	Deleted = 211,
	ProcessCompleted = 212,

	BadRequest = 400,
	UnAuthorized = 401,
	Forbidden = 403,
	NotFound = 404,
	Conflict = 409,
	PayloadTooLarge = 413,
	MediaTypeNotSupported = 451,
	SecurityError = 452,

	InternalServerError = 500,

	ThirdPartyError = 600,
	WrongVerificationCode = 601,
	MaximumLimitReached = 602,
	UserNotFound = 603,
	ExpiredToken = 604,
	ShahkarException = 605,
	ShahkarError = 606,

	BalanceIsLow = 701,
	InquiryNotCached = 702
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
	SunUser = 205,
	SubAdmin = 206,
	AwaitingVerification = 301,
	Verified = 302,
	NationalCardFrontVerified = 401,
	NationalCardBackVerified = 402,
	BirthCertificateFirstVerified = 403,
	BirthCertificateSecondVerified = 404,
	BirthCertificateThirdVerified = 405,
	BirthCertificateForthVerified = 406,
	BirthCertificateFifthVerified = 407,
	VisualAuthenticationVerified = 408,
	ESignatureVerified = 409,

	NationalCardFrontAwaitingVerification = 501,
	NationalCardBackAwaitingVerification = 502,
	BirthCertificateFirstAwaitingVerification = 503,
	BirthCertificateSecondAwaitingVerification = 504,
	BirthCertificateThirdAwaitingVerification = 505,
	BirthCertificateForthAwaitingVerification = 506,
	BirthCertificateFifthAwaitingVerification = 507,
	VisualAuthenticationAwaitingVerification = 508,
	ESignatureAwaitingVerification = 509,

	// ---- Granular admin-panel permissions (only enforced for non-full-admins, e.g. SubAdmin) ----
	PermissionManageHotels = 601, // create/update Hotel + HotelRoom
	PermissionDeleteHotels = 602, // delete Hotel + HotelRoom
	PermissionManageDorms = 603, // create/update Dorm + DormRoom + DormBed
	PermissionDeleteDorms = 604, // delete Dorm + DormRoom + DormBed
	PermissionManageContracts = 605, // create/update DormBedContract
	PermissionDeleteContracts = 606,
	PermissionManageInvoices = 607, // create/update DormBedInvoice
	PermissionDeleteInvoices = 608,
	PermissionPayInvoices = 609,
	PermissionManageUsers = 610, // update other users' profiles
	PermissionDeleteUsers = 611,
	PermissionManageReservations = 612, // create/update HotelReservation + HotelInvoice
	PermissionDeleteReservations = 613, // delete HotelReservation
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

public enum TagBlog {
	Draft = 101,
	Published = 102,
	Archived = 103,
	Featured = 201,
	Pinned = 202
}

public enum TagComment {
	Released = 101,
	InQueue = 102,
	Rejected = 103,
	Private = 201
}

public enum TagReaction {
	Like = 101,
	Dislike = 102
}

public enum TagFollow {
	User = 101,
	Product = 102,
	Category = 103,
	Blog = 104
}

public enum TagContent {
	AboutUs = 101,
	Terms = 102,
	ContactUs = 103,
	HomeSlider1 = 201,
	HomeSlider2 = 202,
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
	ChargeWallet = 301,
	MerchantCreationFee = 302
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
	FreewayTolls = 208,
	MerchantCreationFee = 209,

	ChargeSimPin = 301,
	ChargeSimTopup = 302,
	InternetSim = 303
}

public enum TagTerminal {
	Atm = 101,
	WallCashless = 102,
	DeskCashless = 103
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

public enum TagNotification {
	Test = 999
}

public enum TagTxnErrorCodes {
	LowBalance = 101,
	Unauthorized = 102,
	SenderWalletNotFound = 103,
	ReceiverWalletNotFound = 104,
	SecurityError = 105,
	Ok = 201
}

public enum TagVas {
	Water = 101,
	ChargeTopup = 201,
	ChargePin = 202,
	InternetPackage = 203
}

public enum TagSimOperator {
	IranCell = 1,
	HamrahAvval = 2,
	Rigthel = 3,
	Shatel = 5
}

public enum TagMerchant {
	Normal = 101
}

public enum TagMoadi {
	Pending = 101,
	Approved = 102,
	Rejected = 103
}

public enum TagFieldType {
	Text = 101,
	DropDown = 102,
	File = 103,
	ESignature = 105
}

public enum TagTextFieldType {
	Text = 101,
	MultilineText = 102,
	NumberDecimal = 201,
	PhoneNumber = 301,
	PhoneNumberWithCountryCode = 302,
	Date = 401,
	DateTime = 402,
	PersianDate = 403,
	PersianDateTime = 404
}

public enum TagFileFieldType {
	Image = 101,
	Video = 102,
	Pdf = 103,
	Text = 104
}

public enum TagProcessStepStatus {
	NotStarted = 101,
	Current = 102,
	AwaitingVerification = 103,
	Verified = 104
}

public enum TagDormBedContract {
	Daily = 101,
	Weekly = 102,
	Monthly = 103,
	Yearly = 104,
	SingleInvoice = 201
}

public enum TagDormBedInvoice {
	Deposit = 101,
	Rent = 102,
	Paid = 201,
	PaidOnline = 202,
	PaidManual = 203,
	NotPaid = 204
}

public enum TagBed {
	Test = 999
}

public enum TagHotel {
	Hotel = 101,
	Featured = 201,
	Active = 202,
	Inactive = 203
}

public enum TagHotelReservation {
	Pending = 101,
	Confirmed = 102,
	CheckedIn = 103,
	CheckedOut = 104,
	Cancelled = 201,
	NoShow = 202
}

public enum TagHotelInvoice {
	Full = 101,
	Paid = 201,
	PaidOnline = 202,
	PaidManual = 203,
	NotPaid = 204
}

public enum TagDorm {
	Girls = 101,
	Boys = 102
}

public enum TagRoom {
	Single = 101,
	Double = 102,
	Triple = 103,
	Twin = 104,
	Suite = 105,
	Family = 106,
	Deluxe = 107,
	Available = 201,
	OutOfService = 202
}

public enum TagDormRoom {
	Single = 101,
	Double = 102,
	Dorm = 103
}

public enum TagDormBed {
	Single = 101,
	Double = 102
}