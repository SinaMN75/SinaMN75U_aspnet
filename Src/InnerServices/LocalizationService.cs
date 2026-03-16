namespace SinaMN75U.InnerServices;

public interface ILocalizationService {
	string Get(string key, string locale = "en");
}

public class LocalizationService : ILocalizationService {
	private readonly Dictionary<string, string> _en = new() {
		{ "Required", "Required" },
		{ "InvalidCredentials", "Login Information is Wrong" },
		{ "PhoneNumberNotValid", "Phone number is not valid." },
		{ "PhoneNumberRequired", "Phone number is required." },
		{ "CommentRequired", "Comment is required." },
		{ "CommentNotFound", "Comment Not Found." },
		{ "ContentNotFound", "Content Not Found." },
		{ "ExamNotFound", "Exam Not Found." },
		{ "EmailRequired", "Email is required." },
		{ "UserNameRequired", "UserName is required." },
		{ "UserNameMinLenght", "UserName is not valid." },
		{ "TagsRequired", "Tags is Required" },
		{ "IdRequired", "Id is Required" },
		{ "OtpRequired", "Otp is Required" },
		{ "TitleRequired", "Title is Required" },
		{ "DescriptionRequired", "Description is Required" },
		{ "SubTitleRequired", "Subtitle is Required" },
		{ "FirstNameRequired", "First Name is Required" },
		{ "LastNameRequired", "Last Name is Required" },
		{ "NationalCodeRequired", "National Code is Required" },
		{ "StartDateRequired", "Start Date is Required" },
		{ "EndDateRequired", "End Date is Required" },
		{ "DateRequired", "Date is Required" },
		{ "PriceRequired", "Price is Required" },
		{ "UserIdRequired", "User is Required" },
		{ "ContractIdRequired", "Contract is Required" },
		{ "UserNotFound", "Account not found. Please check your details." },
		{ "EmailInvalid", "Please enter a valid email." },
		{ "UserNameInvalid", "Username is invalid. Please try another." },
		{ "FirstNameInvalid", "First Name is Invalid" },
		{ "LastNameInvalid", "Last Name is Invalid" },
		{ "NationalCodeInvalid", "National Code is Invalid" },
		{ "NationalCodeNotMatchWithPhoneNumberOwner", "National Code is not Match with Phone Number Owner." },
		{ "PasswordMinLength", "Password must be at least 6 characters." },
		{ "PasswordRequired", "Please enter a password." },
		{ "PasswordInvalid", "Password must be 6-100 characters." },
		{ "UserAlreadyExist", "Account already exists. Would you like to log in?" },
		{ "CategoryNotFound", "Category not found. Please try another." },
		{ "InvoiceNotFound", "Invoice not found. Please try another." },
		{ "ProductNotFound", "Product not found. Please check details." },
		{ "ProductDeleted", "Product removed successfully." },
		{ "ProductDeleteFailed", "Could not delete product. Please try later." },
		{ "PhoneNumberInvalid", "Please enter a valid phone number." },
		{ "AuthorizationRequired", "Please sign in to continue." },
		{ "MaxOtpReached", "Too many OTP requests. Please wait and try again." },
		{ "UserDeleted", "Account deleted successfully." },
		{ "AtLeastOneUserRequired", "At Least One User Required" },
		{ "CannotFollowYourself", "Cannot Follow Yourself" },
		{ "AlreadyFollowingUser", "Already Following User" },
		{ "AlreadyBookmarked", "Already Bookmarked" },
		{ "FollowSuccess", "Your now Following, " },
		{ "FollowRelationshipNotFound", "Error Finding the relation" },
		{ "FutureDateSelected", "Future Date Selected" },
		{ "BeforeDateSelected", "Wrong Date Selected" },
		{ "UnfollowSuccess", "You no Longer following, " },
		{ "ProductHasActiveContract", "Product Has Active Contract." },
		{ "ShahkarIsNotAvailableAtThisTime", "Shahkar is not Available at this Time, Please try again Later." },
		{ "TokenExpired", "Auth Token is Expired." },
		{ "InvalidAPIKey", "Invalid API key" },
		{ "InvalidJsonBody", "Invalid JSON body" },
		{ "InvalidBase64RequestBody", "Invalid base64 request body" },
		{ "RequestTooLarge", "Request too large" },
		{ "InvalidRequestFormat", "Invalid request format" },
		{ "InternalServerError", "Internal server error" },
		{ "YourDetailSubmittedSuccessfully", "Your Detail Submitted Successfully." },
		{ "SystemError", "System Error" },
		{ "ZipCodeMustBe10CharactersLong", "Zip Code must be 10 Characters Long." },
		{ "AddressWithThisZipCodeAlreadyExists", "Address with this ZipCode already exists." },
		{ "BalanceIsLow", "Your Balance is Not Enough." },
		{ "SenderWalletNotFound", "Sender Wallet Not Found" },
		{ "ReceiverWalletNotFound", "Receiver Wallet Not Found" },
		{ "TransferMoneyDone", "Transfer Money Successful" },
		{ "WalletForThisUserAlreadyExists", "User Already has Wallet." },
		{ "AddressNotFound", "Address Not Found" },
		{ "AddressUpdatedSuccessfully", "Address Updated Successfully" },
		{ "AddressDeletedSuccessfully", "Address Deleted Successfully" },
	};

	public string Get(string key, string locale = "en") {
		try {
			return _en[key];
		}
		catch (Exception) {
			return string.Empty;
		}
	}
}