namespace SinaMN75U.Data;

public class BaseSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class HotelSelectorArgs : BaseSelectorArgs {
	public HotelRoomSelectorArgs? Rooms { get; set; }
	public HotelReservationSelectorArgs? Reservations { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class HotelRoomSelectorArgs : BaseSelectorArgs {
	public HotelSelectorArgs? Hotel { get; set; }
	public HotelReservationSelectorArgs? Reservations { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class HotelReservationSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public HotelRoomSelectorArgs? Room { get; set; }
	public HotelSelectorArgs? Hotel { get; set; }
	public HotelInvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class HotelInvoiceSelectorArgs : BaseSelectorArgs {
	public HotelReservationSelectorArgs? Reservation { get; set; }
}

public sealed class DormSelectorArgs : BaseSelectorArgs {
	public DormRoomSelectorArgs? Rooms { get; set; }
	public DormBedSelectorArgs? Beds { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class DormRoomSelectorArgs : BaseSelectorArgs {
	public DormSelectorArgs? Dorm { get; set; }
	public DormBedSelectorArgs? Beds { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class DormBedSelectorArgs : BaseSelectorArgs {
	public DormRoomSelectorArgs? Room { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public DormBedContractSelectorArgs? Contract { get; set; }
}

public sealed class DormBedContractSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public DormBedSelectorArgs? Bed { get; set; }
	public DormBedInvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class DormBedInvoiceSelectorArgs : BaseSelectorArgs {
	public DormBedContractSelectorArgs? Contract { get; set; }
}

public sealed class ParkingReportSelectorArgs : BaseSelectorArgs {
	public VehicleSelectorArgs? Vehicle { get; set; }
	public ParkingSelectorArgs? Parking { get; set; }
}

public sealed class VasSelectorArgs : BaseSelectorArgs {
	public WalletTxnSelectorArgs? WalletTxn { get; set; }
	public TxnSelectorArgs? Txn { get; set; }
}

public sealed class CategorySelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Children { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class ContentSelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class SimCardSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class TerminalSelectorArgs : BaseSelectorArgs {
	public MerchantSelectorArgs? Merchant { get; set; }
	public bool Agreement { get; set; }
}

public sealed class MerchantSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public TerminalSelectorArgs? Terminal { get; set; }
}

public sealed class WalletTxnSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? Sender { get; set; }
	public UserSelectorArgs? Receiver { get; set; }
}

public sealed class NotificationSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class TicketSelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs? Category { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public TxnSelectorArgs? Txns { get; set; }
	public AddressSelectorArgs? Address { get; set; }
	public WalletSelectorArgs? Wallet { get; set; }
	public MerchantSelectorArgs? Merchant { get; set; }
	public BankAccountSelectorArgs? BankAccount { get; set; }
	public SimCardSelectorArgs? SimCard { get; set; }
	public bool NationalCardFront { get; set; }
	public bool NationalCardBack { get; set; }
	public bool BirthCertificateFirst { get; set; }
	public bool BirthCertificateSecond { get; set; }
	public bool BirthCertificateThird { get; set; }
	public bool BirthCertificateForth { get; set; }
	public bool BirthCertificateFifth { get; set; }
	public bool VisualAuthentication { get; set; }
	public bool ESignature { get; set; }
}

public sealed class ProductSelectorArgs : BaseSelectorArgs {
	public Guid? UserId { get; set; }
	public ProductSelectorArgs? Children { get; set; }
	public CategorySelectorArgs? Category { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public bool ChildrenCount { get; set; }
	public bool CommentsCount { get; set; }
	public bool IsFollowing { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class CommentSelectorArgs : BaseSelectorArgs {
	public CommentSelectorArgs? Children { get; set; }
	public UserSelectorArgs? User { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class TxnSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class MediaSelectorArgs : BaseSelectorArgs;

public sealed class ParkingSelectorArgs : BaseSelectorArgs;

public sealed class VehicleSelectorArgs : BaseSelectorArgs;

public sealed class BankAccountSelectorArgs : BaseSelectorArgs;

public sealed class AddressSelectorArgs : BaseSelectorArgs;

public sealed class WalletSelectorArgs : BaseSelectorArgs;

public sealed class ApiLogSelectorArgs : BaseSelectorArgs;

public sealed class BlogSelectorArgs : BaseSelectorArgs {
	public Guid? UserId { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Category { get; set; }
	public CommentSelectorArgs? Comments { get; set; }
	public BlogSelectorArgs? Children { get; set; }
	public bool CommentsCount { get; set; }
	public bool ChildrenCount { get; set; }
	public int ChildrenDebt { get; set; }
}

public static class Projections {
	public static Expression<Func<ApiLogEntity, ApiLogResponse>> ApiLogSelector(ApiLogSelectorArgs args) {
		Expression<Func<ApiLogEntity, ApiLogResponse>> selector = x => new ApiLogResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Path = x.Path,
			StatusCode = x.StatusCode,
			DurationMs = x.DurationMs,
			UserId = x.UserId,
			IpAddress = x.IpAddress,
			TraceId = x.TraceId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	public static Expression<Func<AddressEntity, AddressResponse>> AddressSelector(AddressSelectorArgs args) {
		Expression<Func<AddressEntity, AddressResponse>> selector = x => new AddressResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			ZipCode = x.ZipCode,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	public static Expression<Func<WalletTxnEntity, WalletTxnResponse>> WalletTxnSelector(WalletTxnSelectorArgs args) {
		Expression<Func<WalletTxnEntity, WalletTxnResponse>> selector = x => new WalletTxnResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			SenderId = x.SenderId,
			ReceiverId = x.ReceiverId,
			Amount = x.Amount,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
			Sender = x.Sender == null ? null : (args.Sender != null ? UserSelector(args.Sender) : u => null!).Invoke(x.Sender),
			Receiver = x.Receiver == null ? null : (args.Receiver != null ? UserSelector(args.Receiver) : u => null!).Invoke(x.Receiver)
		};
		return selector.Expand();
	}

	public static Expression<Func<NotificationEntity, NotificationResponse>> NotificationSelector(NotificationSelectorArgs args) {
		Expression<Func<NotificationEntity, NotificationResponse>> selector = x => new NotificationResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			UserId = x.UserId,
			ZipCode = x.ZipCode,
			CreatedAt = x.CreatedAt,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : u => null!).Invoke(x.User),
		};
		return selector.Expand();
	}

	public static Expression<Func<WalletEntity, WalletResponse>> WalletSelector(WalletSelectorArgs args) {
		Expression<Func<WalletEntity, WalletResponse>> selector = x => new WalletResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Balance = x.Balance,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<BankAccountEntity, BankAccountResponse>> BankAccountSelector(BankAccountSelectorArgs args) {
		Expression<Func<BankAccountEntity, BankAccountResponse>> selector = x => new BankAccountResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			CardNumber = x.CardNumber,
			AccountNumber = x.AccountNumber,
			IBanNumber = x.IBanNumber,
			BankName = x.BankName,
			OwnerName = x.OwnerName,
			CreatorId = x.CreatorId,
			JsonData = x.JsonData,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<SimCardEntity, SimCardResponse>> SimCardSelector(SimCardSelectorArgs args) {
		Expression<Func<SimCardEntity, SimCardResponse>> selector = x => new SimCardResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UserId = x.UserId,
			JsonData = x.JsonData,
			Serial = x.Serial,
			Number = x.Number,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : u => null!).Invoke(x.User),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector() {
		Expression<Func<MediaEntity, MediaResponse>> selector = x => new MediaResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Path = x.Path
		};
		return selector.Expand();
	}

	public static Expression<Func<ParkingEntity, ParkingResponse>> ParkingSelector(ParkingSelectorArgs args) {
		Expression<Func<ParkingEntity, ParkingResponse>> selector = x => new ParkingResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			EntrancePrice = x.EntrancePrice,
			HourlyPrice = x.HourlyPrice,
			DailyPrice = x.DailyPrice,
			AdminUserIds = x.AdminUserIds,
			CreatorId = x.CreatorId,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<UserEntity, UserResponse>> UserSelector(UserSelectorArgs args) {
		return x => new UserResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			FirstName = x.FirstName,
			LastName = x.LastName,
			Bio = x.Bio,
			Birthdate = x.Birthdate,
			NationalCode = x.NationalCode,
			CreatedAt = x.CreatedAt,
			LandLine = x.LandLine,
			CreatorId = x.CreatorId,
			NationalCardFront = args.NationalCardFront ? x.NationalCardFront.ToBase64() : null,
			NationalCardBack = args.NationalCardBack ? x.NationalCardBack.ToBase64() : null,
			BirthCertificateFirst = args.BirthCertificateFirst ? x.BirthCertificateFirst.ToBase64() : null,
			BirthCertificateSecond = args.BirthCertificateSecond ? x.BirthCertificateSecond.ToBase64() : null,
			BirthCertificateThird = args.BirthCertificateThird ? x.BirthCertificateThird.ToBase64() : null,
			BirthCertificateForth = args.BirthCertificateForth ? x.BirthCertificateForth.ToBase64() : null,
			BirthCertificateFifth = args.BirthCertificateFifth ? x.BirthCertificateFifth.ToBase64() : null,
			VisualAuthentication = args.VisualAuthentication ? x.VisualAuthentication.ToBase64() : null,
			ESignature = args.ESignature ? x.ESignature.ToBase64() : null,
			Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Addresses = args.Address == null ? null : x.Addresses.AsQueryable().Select(AddressSelector(args.Address)).ToList(),
			BankAccounts = args.BankAccount == null ? null : x.BankAccounts.AsQueryable().Select(BankAccountSelector(args.BankAccount)).ToList(),
			SimCards = args.SimCard == null ? null : x.SimCards.AsQueryable().Select(SimCardSelector(args.SimCard)).ToList(),
			Merchants = args.Merchant == null ? null : x.Merchants.AsQueryable().Select(MerchantSelector(args.Merchant)).ToList(),
			Txns = args.Txns == null ? null : x.Txns.AsQueryable().Select(TxnSelector(args.Txns)).ToList(),
			Wallets = args.Wallet == null ? null : x.Wallets.AsQueryable().Select(WalletSelector(args.Wallet)).ToList(),
		};
	}

	public static Expression<Func<MerchantEntity, MerchantResponse>> MerchantSelector(MerchantSelectorArgs args) {
		Expression<Func<MerchantEntity, MerchantResponse>> selector = x => new MerchantResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			UserId = x.UserId,
			ZipCode = x.ZipCode,
			BankAccountId = x.BankAccountId,
			NationalCode = x.NationalCode,
			Title = x.Title,
			CityCode = x.CityCode,
			InsId = x.InsId,
			Landline = x.Landline,
			Mcc = x.Mcc,
			MerchantId = x.MerchantId,
			PhoneNumber = x.PhoneNumber,
			Terminals = args.Terminal == null ? null : x.Terminals.AsQueryable().Select(TerminalSelector(args.Terminal)).ToList(),
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : u => null!).Invoke(x.User),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<ParkingReportEntity, ParkingReportResponse>> ParkingReportSelector(ParkingReportSelectorArgs args) {
		Expression<Func<ParkingReportEntity, ParkingReportResponse>> selector = x => new ParkingReportResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			StartDate = x.StartDate,
			VehicleId = x.VehicleId,
			ParkingId = x.ParkingId,
			Amount = x.Amount,
			EndDate = x.EndDate,
			Parking = x.Parking == null ? null : (args.Parking != null ? ParkingSelector(args.Parking) : p => null!).Invoke(x.Parking),
			Vehicle = x.Vehicle == null ? null : (args.Vehicle != null ? VehicleSelector(args.Vehicle) : v => null!).Invoke(x.Vehicle),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<VehicleEntity, VehicleResponse>> VehicleSelector(VehicleSelectorArgs args) {
		Expression<Func<VehicleEntity, VehicleResponse>> selector = x => new VehicleResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			LicencePlate = x.LicencePlate,
			CreatorId = x.CreatorId,
			Title = x.Title,
			Brand = x.Brand,
			Color = x.Color,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(ProductSelectorArgs args) {
		bool hasChildren = args is { Children: not null, ChildrenDebt: > 0 };

		Expression<Func<ProductEntity, ProductResponse>> selector = x => new ProductResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Code = x.Code,
			Subtitle = x.Subtitle,
			Description = x.Description,
			Slug = x.Slug,
			Type = x.Type,
			Content = x.Content,
			Latitude = x.Latitude,
			Longitude = x.Longitude,
			Stock = x.Stock,
			Point = x.Point,
			Order = x.Order,
			ParentId = x.ParentId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
			Children = !hasChildren
				? null
				: x.Children.AsQueryable().Select(ProductSelector(new ProductSelectorArgs {
					UserId = args.UserId,
					Media = args.Media,
					ChildrenCount = args.ChildrenCount,
					CommentsCount = args.CommentsCount,
					IsFollowing = args.IsFollowing,
					Children = args.Children,
					Category = args.Category,
					Creator = args.Creator,
					ChildrenDebt = args.ChildrenDebt - 1
				})).ToList(),
			CommentCount = args.CommentsCount ? x.Comments.Count : null,
			ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
			IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.CreatorId == args.UserId) : null,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};

		return selector.Expand();
	}

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategorySelectorArgs args) {
		bool hasChildren = args is { Children: not null, ChildrenDebt: > 0 };

		Expression<Func<CategoryEntity, CategoryResponse>> selector = x => new CategoryResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Order = x.Order,
			Code = x.Code,
			ParentId = x.ParentId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Children = !hasChildren
				? null
				: x.Children.AsQueryable().Select(CategorySelector(new CategorySelectorArgs {
					Media = args.Media,
					Creator = args.Creator,
					Children = args.Children,
					ChildrenDebt = args.ChildrenDebt - 1
				})).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) {
		Expression<Func<ContentEntity, ContentResponse>> selector = x => new ContentResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<TerminalEntity, TerminalResponse>> TerminalSelector(TerminalSelectorArgs args) {
		Expression<Func<TerminalEntity, TerminalResponse>> selector = x => new TerminalResponse {
			Id = x.Id,
			Tags = x.Tags,
			Serial = x.Serial,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			MerchantId = x.MerchantId,
			SimCardSerial = x.SimCardSerial,
			SimCardNumber = x.SimCardNumber,
			Imei = x.Imei,
			TerminalId = x.TerminalId,
			CreatedAt = x.CreatedAt,
			Agreement = args.Agreement ? x.Agreement.ToBase64() : null,
			Merchant = x.Merchant == null ? null : (args.Merchant != null ? MerchantSelector(args.Merchant) : m => null!).Invoke(x.Merchant),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<TxnEntity, TxnResponse>> TxnSelector(TxnSelectorArgs args) {
		Expression<Func<TxnEntity, TxnResponse>> selector = x => new TxnResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			Amount = x.Amount,
			TrackingNumber = x.TrackingNumber,
			JsonData = x.JsonData,
			UserId = x.UserId,
			CreatorId = x.CreatorId,
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : u => null!).Invoke(x.User),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<TicketEntity, TicketResponse>> TicketSelector(TicketSelectorArgs args) {
		Expression<Func<TicketEntity, TicketResponse>> selector = x => new TicketResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(CommentSelectorArgs args) {
		Expression<Func<CommentEntity, CommentResponse>> selector = x => new CommentResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			UserId = x.UserId,
			ProductId = x.ProductId,
			BlogId = x.BlogId,
			ParentId = x.ParentId,
			Score = x.Score,
			Description = x.Description,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Children = args.Children == null ? null : x.Children.AsQueryable().Select(CommentSelector(args.Children)).ToList(),
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : u => null!).Invoke(x.User),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<VasEntity, VasResponse>> VasSelector(VasSelectorArgs args) {
		Expression<Func<VasEntity, VasResponse>> selector = x => new VasResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			Amount = x.Amount,
			AuthorizeCode = x.AuthorizeCode,
			BillId = x.BillId,
			PaymentId = x.PaymentId,
			TxnId = x.TxnId,
			WalletTxnId = x.WalletTxnId,
			JsonData = x.JsonData,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
			Txn = x.Txn == null ? null : (args.Txn != null ? TxnSelector(args.Txn) : t => null!).Invoke(x.Txn),
			WalletTxn = x.WalletTxn == null ? null : (args.WalletTxn != null ? WalletTxnSelector(args.WalletTxn) : w => null!).Invoke(x.WalletTxn)
		};
		return selector.Expand();
	}

	public static Expression<Func<DormBedContractEntity, DormBedContractResponse>> DormBedContractSelector(DormBedContractSelectorArgs args) {
		Expression<Func<DormBedContractEntity, DormBedContractResponse>> selector = x => new DormBedContractResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			StartDate = x.StartDate,
			EndDate = x.EndDate,
			Deposit = x.Deposit,
			Rent = x.Rent,
			UserId = x.UserId,
			CreatorId = x.CreatorId,
			BedId = x.BedId,
			CreatedAt = x.CreatedAt,
			IsActive = x.EndDate < DateTime.UtcNow,
			Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().Select(DormBedInvoiceSelector(args.Invoice)).ToList(),
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : t => null!).Invoke(x.User),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : t => null!).Invoke(x.Creator),
			Bed = x.Bed == null ? null : (args.Bed != null ? DormBedSelector(args.Bed) : t => null!).Invoke(x.Bed)
		};
		return selector.Expand();
	}

	public static Expression<Func<DormBedInvoiceEntity, DormBedInvoiceResponse>> DormBedInvoiceSelector(DormBedInvoiceSelectorArgs args) {
		Expression<Func<DormBedInvoiceEntity, DormBedInvoiceResponse>> selector = x => new DormBedInvoiceResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			CreditorAmount = x.CreditorAmount,
			PaidAmount = x.PaidAmount,
			PenaltyAmount = x.PenaltyAmount,
			DueDate = x.DueDate,
			DebtAmount = x.DebtAmount,
			Contract = x.Contract == null ? null : (args.Contract != null ? DormBedContractSelector(args.Contract) : t => null!).Invoke(x.Contract),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : t => null!).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	public static Expression<Func<HotelEntity, HotelResponse>> HotelSelector(HotelSelectorArgs args) {
		Expression<Func<HotelEntity, HotelResponse>> selector = x => new HotelResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			CityCode = x.CityCode,
			Stars = x.Stars,
			Address = x.Address,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			AdminUserIds = x.AdminUserIds,
			Rooms = args.Rooms == null ? null : x.Rooms.AsQueryable().Select(HotelRoomSelector(args.Rooms)).ToList(),
			Reservations = args.Reservations == null ? null : x.Reservations.AsQueryable().Select(HotelReservationSelector(args.Reservations)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<HotelRoomEntity, HotelRoomResponse>> HotelRoomSelector(HotelRoomSelectorArgs args) {
		Expression<Func<HotelRoomEntity, HotelRoomResponse>> selector = x => new HotelRoomResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Capacity = x.Capacity,
			PricePerNight = x.PricePerNight,
			RoomNumber = x.RoomNumber,
			Quantity = x.Quantity,
			IsAvailable = x.IsAvailable,
			HotelId = x.HotelId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Hotel = x.Hotel == null ? null : (args.Hotel != null ? HotelSelector(args.Hotel) : h => null!).Invoke(x.Hotel),
			Reservations = args.Reservations == null ? null : x.Reservations.AsQueryable().Select(HotelReservationSelector(args.Reservations)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<HotelReservationEntity, HotelReservationResponse>> HotelReservationSelector(HotelReservationSelectorArgs args) {
		Expression<Func<HotelReservationEntity, HotelReservationResponse>> selector = x => new HotelReservationResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CheckInDate = x.CheckInDate,
			CheckOutDate = x.CheckOutDate,
			GuestCount = x.GuestCount,
			TotalPrice = x.TotalPrice,
			UserId = x.UserId,
			RoomId = x.RoomId,
			HotelId = x.HotelId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			AdminUserIds = x.AdminUserIds,
			IsActive = x.CheckInDate <= DateTime.UtcNow && x.CheckOutDate >= DateTime.UtcNow,
			User = x.User == null ? null : (args.User != null ? UserSelector(args.User) : t => null!).Invoke(x.User),
			Room = x.Room == null ? null : (args.Room != null ? HotelRoomSelector(args.Room) : t => null!).Invoke(x.Room),
			Hotel = x.Hotel == null ? null : (args.Hotel != null ? HotelSelector(args.Hotel) : t => null!).Invoke(x.Hotel),
			Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().Select(HotelInvoiceSelector(args.Invoice)).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : t => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<HotelInvoiceEntity, HotelInvoiceResponse>> HotelInvoiceSelector(HotelInvoiceSelectorArgs args) {
		Expression<Func<HotelInvoiceEntity, HotelInvoiceResponse>> selector = x => new HotelInvoiceResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			AdminUserIds = x.AdminUserIds,
			DueDate = x.DueDate,
			DebtAmount = x.DebtAmount,
			CreditorAmount = x.CreditorAmount,
			PaidAmount = x.PaidAmount,
			PenaltyAmount = x.PenaltyAmount,
			ReservationId = x.ReservationId,
			Reservation = x.Reservation == null ? null : (args.Reservation != null ? HotelReservationSelector(args.Reservation) : t => null!).Invoke(x.Reservation),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : t => null!).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	public static Expression<Func<DormEntity, DormResponse>> DormSelector(DormSelectorArgs args) {
		Expression<Func<DormEntity, DormResponse>> selector = x => new DormResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			CityCode = x.CityCode,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			AdminUserIds = x.AdminUserIds,
			Rooms = args.Rooms == null ? null : x.Rooms.AsQueryable().Select(DormRoomSelector(args.Rooms)).ToList(),
			Beds = args.Beds == null ? null : x.Rooms.SelectMany(r => r.Beds).AsQueryable().Select(DormBedSelector(args.Beds)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<DormRoomEntity, DormRoomResponse>> DormRoomSelector(DormRoomSelectorArgs args) {
		Expression<Func<DormRoomEntity, DormRoomResponse>> selector = x => new DormRoomResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			DormId = x.DormId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Dorm = x.Dorm == null ? null : (args.Dorm != null ? DormSelector(args.Dorm) : d => null!).Invoke(x.Dorm),
			Beds = (args.Beds == null ? null : x.Beds.AsQueryable().Select(DormBedSelector(args.Beds)).ToList())!,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}

	public static Expression<Func<DormBedEntity, DormBedResponse>> DormBedSelector(DormBedSelectorArgs args) {
		Expression<Func<DormBedEntity, DormBedResponse>> selector = x => new DormBedResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Deposit = x.Deposit,
			MonthlyRent = x.MonthlyRent,
			RoomId = x.RoomId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Room = x.Room == null ? null : (args.Room != null ? DormRoomSelector(args.Room) : r => null!).Invoke(x.Room),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
			Contracts = args.Contract == null ? null : x.Contracts.AsQueryable().Select(DormBedContractSelector(args.Contract)).ToList(),
		};
		return selector.Expand();
	}

	public static Expression<Func<BlogEntity, BlogResponse>> BlogSelector(BlogSelectorArgs args) {
		bool hasChildren = args is { Children: not null, ChildrenDebt: > 0 };

		Expression<Func<BlogEntity, BlogResponse>> selector = x => new BlogResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Subtitle = x.Subtitle,
			Code = x.Code,
			Description = x.Description,
			Slug = x.Slug,
			Type = x.Type,
			Content = x.Content,
			Latitude = x.Latitude,
			Longitude = x.Longitude,
			ParentId = x.ParentId,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			AdminUserIds = x.AdminUserIds,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
			Comments = args.Comments == null ? null : x.Comments.AsQueryable().Select(CommentSelector(args.Comments)).ToList(),
			Children = !hasChildren
				? null
				: x.Children.AsQueryable().Select(BlogSelector(new BlogSelectorArgs {
					UserId = args.UserId,
					Media = args.Media,
					Category = args.Category,
					Comments = args.Comments,
					Children = args.Children,
					CommentsCount = args.CommentsCount,
					ChildrenCount = args.ChildrenCount,
					Creator = args.Creator,
					ChildrenDebt = args.ChildrenDebt - 1
				})).ToList(),
			CommentCount = args.CommentsCount ? x.Comments.Count : null,
			ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
			Creator = x.Creator == null ? null : (args.Creator != null ? UserSelector(args.Creator) : u => null!).Invoke(x.Creator),
		};
		return selector.Expand();
	}
}