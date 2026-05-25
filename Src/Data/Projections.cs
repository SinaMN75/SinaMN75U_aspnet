using LinqKit;

namespace SinaMN75U.Data;

public class BaseSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class MediaSelectorArgs : BaseSelectorArgs;

public sealed class ParkingSelectorArgs : BaseSelectorArgs { }

public sealed class ParkingReportSelectorArgs : BaseSelectorArgs {
	public VehicleSelectorArgs? Vehicle { get; set; }
	public ParkingSelectorArgs? Parking { get; set; }
}

public sealed class VasSelectorArgs : BaseSelectorArgs {
	public WalletTxnSelectorArgs? WalletTxn { get; set; }
	public TxnSelectorArgs? Txn { get; set; }
}

public sealed class VehicleSelectorArgs : BaseSelectorArgs { }

public sealed class CategorySelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Children { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class ContentSelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class BankAccountSelectorArgs : BaseSelectorArgs { }

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

public sealed class AddressSelectorArgs : BaseSelectorArgs { }

public sealed class WalletTxnSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? Sender { get; set; }
	public UserSelectorArgs? Receiver { get; set; }
}

public sealed class NotificationSelectorArgs : BaseSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class WalletSelectorArgs : BaseSelectorArgs { }

public sealed class TicketSelectorArgs : BaseSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class UserSelectorArgs : BaseSelectorArgs {
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

public static class Projections {
	public static Expression<Func<AddressEntity, AddressResponse>> AddressSelector(AddressSelectorArgs args) {
		Expression<Func<AddressEntity, AddressResponse>> selector = x => new AddressResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			ZipCode = x.ZipCode,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator),
			Sender = args.Sender == null ? null : UserSelector(args.Sender).Invoke(x.Sender),
			Receiver = args.Receiver == null ? null : UserSelector(args.Receiver).Invoke(x.Receiver)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator),
			User = args.User == null ? null : UserSelector(args.User).Invoke(x.User)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			User = args.User == null ? null : UserSelector(args.User).Invoke(x.User),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			CreatorId = x.CreatorId,
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	private static Expression<Func<UserEntity, UserResponse>> UserSelector(UserSelectorArgs args) {
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
			NationalCardBack = args.NationalCardFront ? x.NationalCardBack.ToBase64() : null,
			BirthCertificateFirst = args.NationalCardFront ? x.BirthCertificateFirst.ToBase64() : null,
			BirthCertificateSecond = args.NationalCardFront ? x.BirthCertificateSecond.ToBase64() : null,
			BirthCertificateThird = args.NationalCardFront ? x.BirthCertificateThird.ToBase64() : null,
			BirthCertificateForth = args.NationalCardFront ? x.BirthCertificateForth.ToBase64() : null,
			BirthCertificateFifth = args.NationalCardFront ? x.BirthCertificateFifth.ToBase64() : null,
			VisualAuthentication = args.NationalCardFront ? x.VisualAuthentication.ToBase64() : null,
			ESignature = args.NationalCardFront ? x.ESignature.ToBase64() : null,
			Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Addresses = args.Address == null ? null : x.Addresses.AsQueryable().Select(AddressSelector(args.Address)).ToList(),
			BankAccounts = args.BankAccount == null ? null : x.BankAccounts.AsQueryable().Select(BankAccountSelector(args.BankAccount)).ToList(),
			SimCards = args.SimCard == null ? null : x.SimCards.AsQueryable().Select(SimCardSelector(args.SimCard)).ToList(),
			Merchants = args.Merchant == null ? null : x.Merchants.AsQueryable().Select(MerchantSelector(args.Merchant)).ToList(),
			Txns = args.Txns == null ? null : x.Txns.AsQueryable().Select(TxnSelector(args.Txns)).ToList(),
			Wallets = args.Wallet == null ? null : x.Wallets.AsQueryable().Select(WalletSelector(args.Wallet)).ToList(),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			User = args.User == null ? null : UserSelector(args.User).Invoke(x.User),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Parking = args.Parking == null ? null : ParkingSelector(args.Parking).Invoke(x.Parking),
			Vehicle = args.Vehicle == null ? null : VehicleSelector(args.Vehicle).Invoke(x.Vehicle),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
		};
		return selector.Expand();
	}

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(ProductSelectorArgs args) {
		Expression<Func<ProductEntity, ProductResponse>>? childSelector = null;
		if (args is { Children: not null, ChildrenDebt: > 0 and < 10 })
			childSelector = ProductSelector(new ProductSelectorArgs {
				UserId = args.UserId,
				Media = args.Media,
				ChildrenCount = args.ChildrenCount,
				CommentsCount = args.CommentsCount,
				IsFollowing = args.IsFollowing,
				Children = args.Children,
				Category = args.Category,
				Creator = args.Creator,
				ChildrenDebt = args.ChildrenDebt - 1
			});
		return x => new ProductResponse {
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
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().Select(childSelector!).ToList() : null,
			CommentCount = args.CommentsCount ? x.Comments.Count : null,
			ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
			IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.CreatorId == args.UserId) : null,
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
		};
	}

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategorySelectorArgs args) {
		Expression<Func<CategoryEntity, CategoryResponse>>? childSelector = null;
		if (args is { Children: not null, ChildrenDebt: > 0 and < 10 })
			childSelector = CategorySelector(new CategorySelectorArgs {
					Media = args.Media,
					Children = args.Children,
					ChildrenDebt = args.ChildrenDebt - 1
				}
			);
		return x => new CategoryResponse {
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
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().Select(childSelector!).ToList() : null,
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
		};
	}

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) {
		Expression<Func<ContentEntity, ContentResponse>> selector = x => new ContentResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatorId = x.CreatorId,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			TerminalId =  x.TerminalId,
			CreatedAt =  x.CreatedAt,
			Agreement = args.Agreement ? x.Agreement.ToBase64() : null,
			Merchant = args.Merchant == null ? null : MerchantSelector(args.Merchant)!.Invoke(x.Merchant),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			User = args.User == null ? null : UserSelector(args.User).Invoke(x.User),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			ParentId = x.ParentId,
			Score = x.Score,
			Description = x.Description,
			CreatedAt = x.CreatedAt,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Children = args.Children == null ? null : x.Children.AsQueryable().Select(CommentSelector(args.Children)).ToList(),
			User = args.User == null ? null : UserSelector(args.User)!.Invoke(x.User),
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator)
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
			Creator = args.Creator == null ? null : UserSelector(args.Creator).Invoke(x.Creator),
			Txn = args.Txn == null ? null : TxnSelector(args.Txn)!.Invoke(x.Txn),
			WalletTxn = args.WalletTxn == null ? null : WalletTxnSelector(args.WalletTxn)!.Invoke(x.WalletTxn)
		};
		return selector.Expand();
	}
}