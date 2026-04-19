namespace SinaMN75U.Data;

public sealed class MediaSelectorArgs;

public sealed class ParkingSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class ParkingReportSelectorArgs {
	public VehicleSelectorArgs? Vehicle { get; set; }
	public ParkingSelectorArgs? Parking { get; set; }
}

public sealed class VasSelectorArgs {
	public WalletTxnSelectorArgs? WalletTxn { get; set; }
	public TxnSelectorArgs? Txn { get; set; }
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class VehicleSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
};

public sealed class CategorySelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Children { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class ContentSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class BankAccountSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class SimCardSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class TerminalSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class AddressSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
}

public sealed class WalletTxnSelectorArgs {
	public UserSelectorArgs? Sender { get; set; }
	public UserSelectorArgs? Receiver { get; set; }
}

public sealed class NotificationSelectorArgs {
	public UserSelectorArgs? Creator { get; set; }
	public UserSelectorArgs? User { get; set; }
}

public sealed class WalletSelectorArgs {
	public UserSelectorArgs? User { get; set; }
}

public sealed class TicketSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
	public UserSelectorArgs? User { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs? Category { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
	public TxnSelectorArgs? Txns { get; set; }
	public AddressSelectorArgs? Address { get; set; }
	public WalletSelectorArgs? Wallet { get; set; }
	public TerminalSelectorArgs? Terminal { get; set; }
	public BankAccountSelectorArgs? BankAccount { get; set; }
	public SimCardSelectorArgs? SimCard { get; set; }
	public bool Extra { get; set; }
}

public sealed class ProductSelectorArgs {
	public Guid? UserId { get; set; }
	public ProductSelectorArgs? Children { get; set; }
	public CategorySelectorArgs? Category { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public bool ChildrenCount { get; set; }
	public bool CommentsCount { get; set; }
	public bool IsFollowing { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class ContractSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class InvoiceSelectorArgs {
	public ContractSelectorArgs? Contract { get; set; }
}

public sealed class CommentSelectorArgs {
	public CommentSelectorArgs? Children { get; set; }
	public UserSelectorArgs? User { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class TxnSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class AgreementSelectorArgs {
	public TerminalSelectorArgs? Terminal { get; set; }
}

public static class Projections {
	public static Expression<Func<AddressEntity, AddressResponse>> AddressSelector(AddressSelectorArgs args) => x => new AddressResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		ZipCode = x.ZipCode,
		CreatorId = x.CreatorId, Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Bio = x.Creator.Bio,
				Birthdate = x.Creator.Birthdate,
				CreatedAt = x.Creator.CreatedAt,
				CreatorId = x.Creator.CreatorId,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList(),
				Addresses = args.Creator.Address == null ? null : x.Creator.Addresses.AsQueryable().Select(AddressSelector(args.Creator.Address)).ToList(),
				BankAccounts = args.Creator.BankAccount == null ? null : x.Creator.BankAccounts.AsQueryable().Select(BankAccountSelector(args.Creator.BankAccount)).ToList(),
				Terminals = args.Creator.Terminal == null ? null : x.Creator.Terminals.AsQueryable().Select(TerminalSelector(args.Creator.Terminal)).ToList(),
				Invoices = args.Creator.Invoice == null ? null : x.Creator.Invoices.AsQueryable().Select(InvoiceSelector(args.Creator.Invoice)).ToList(),
				Txns = args.Creator.Txns == null ? null : x.Creator.Txns.AsQueryable().Select(TxnSelector(args.Creator.Txns)).ToList(),
				SimCards = args.Creator.SimCard == null ? null : x.Creator.SimCards.AsQueryable().Select(SimCardSelector(args.Creator.SimCard)).ToList(),
				Wallets = args.Creator.Wallet == null ? null : x.Creator.Wallets.AsQueryable().Select(WalletSelector(args.Creator.Wallet)).ToList()
			}
	};

	public static Expression<Func<WalletTxnEntity, WalletTxnResponse>> WalletTxnSelector(WalletTxnSelectorArgs args) => x => new WalletTxnResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		SenderId = x.SenderId,
		ReceiverId = x.ReceiverId,
		Amount = x.Amount,
		Sender = args.Sender == null ? null : new UserResponse {
			Id = x.Sender.Id,
			JsonData = x.Sender.JsonData,
			Tags = x.Sender.Tags,
			UserName = x.Sender.UserName,
			PhoneNumber = x.Sender.PhoneNumber,
			Email = x.Sender.Email,
			FirstName = x.Sender.FirstName,
			LastName = x.Sender.LastName,
			NationalCode = x.Sender.NationalCode,
			Media = args.Sender.Media == null ? null : x.Sender.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Sender.Category == null ? null : x.Sender.Categories.AsQueryable().Select(CategorySelector(args.Sender.Category)).ToList()
		},
		Receiver = args.Receiver == null ? null : new UserResponse {
			Id = x.Receiver.Id,
			JsonData = x.Receiver.JsonData,
			Tags = x.Receiver.Tags,
			UserName = x.Receiver.UserName,
			PhoneNumber = x.Receiver.PhoneNumber,
			Email = x.Receiver.Email,
			FirstName = x.Receiver.FirstName,
			LastName = x.Receiver.LastName,
			NationalCode = x.Receiver.NationalCode,
			Media = args.Receiver.Media == null ? null : x.Receiver.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Receiver.Category == null ? null : x.Receiver.Categories.AsQueryable().Select(CategorySelector(args.Receiver.Category)).ToList()
		}
	};

	public static Expression<Func<NotificationEntity, NotificationResponse>> NotificationSelector(NotificationSelectorArgs args) => x => new NotificationResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		CreatorId = x.CreatorId,
		UserId = x.Userd,
		ZipCode = x.ZipCode,
		Creator = args.Creator == null ? null : new UserResponse {
			Id = x.Creator.Id,
			JsonData = x.Creator.JsonData,
			Tags = x.Creator.Tags,
			UserName = x.Creator.UserName,
			PhoneNumber = x.Creator.PhoneNumber,
			Email = x.Creator.Email,
			FirstName = x.Creator.FirstName,
			LastName = x.Creator.LastName,
			NationalCode = x.Creator.NationalCode,
			Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
		},
		User = args.User == null ? null : new UserResponse {
			Id = x.User.Id,
			JsonData = x.User.JsonData,
			Tags = x.User.Tags,
			UserName = x.User.UserName,
			PhoneNumber = x.User.PhoneNumber,
			Email = x.User.Email,
			FirstName = x.User.FirstName,
			LastName = x.User.LastName,
			NationalCode = x.User.NationalCode,
			Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList()
		}
	};

	public static Expression<Func<WalletEntity, WalletResponse>> WalletSelector(WalletSelectorArgs args) => x => new WalletResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Balance = x.Balance,
		CreatorId = x.CreatorId,
		Creator = args.User == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Media = args.User.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.User.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList()
			}
	};

	public static Expression<Func<BankAccountEntity, BankAccountResponse>> BankAccountSelector(BankAccountSelectorArgs args) => x => new BankAccountResponse {
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
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
			}
	};

	public static Expression<Func<SimCardEntity, SimCardResponse>> SimCardSelector(SimCardSelectorArgs args) => x => new SimCardResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UserId = x.UserId,
		JsonData = x.JsonData,
		Serial = x.Serial,
		Number = x.Number,
		Tags = x.Tags,
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				NationalCode = x.User.NationalCode,
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList()
			}
	};

	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector() => x => new MediaResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Path = x.Path
	};

	public static Expression<Func<UserEntity, UserResponse>> UserSelector(UserSelectorArgs args) => x => new UserResponse {
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
		Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
		Addresses = args.Address == null ? null : x.Addresses.AsQueryable().Select(AddressSelector(args.Address)).ToList(),
		BankAccounts = args.BankAccount == null ? null : x.BankAccounts.AsQueryable().Select(BankAccountSelector(args.BankAccount)).ToList(),
		Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().Select(InvoiceSelector(args.Invoice)).ToList(),
		SimCards = args.SimCard == null ? null : x.SimCards.AsQueryable().Select(SimCardSelector(args.SimCard)).ToList(),
		Terminals = args.Terminal == null ? null : x.Terminals.AsQueryable().Select(TerminalSelector(args.Terminal)).ToList(),
		Txns = args.Txns == null ? null : x.Txns.AsQueryable().Select(TxnSelector(args.Txns)).ToList(),
		Wallets = args.Wallet == null ? null : x.Wallets.AsQueryable().Select(WalletSelector(args.Wallet)).ToList(),
		Extra = !args.Extra
			? null
			: new UserExtraResponse {
				NationalCardFront = x.Extra.NationalCardFront,
				NationalCardBack = x.Extra.NationalCardBack,
				BirthCertificateFirst = x.Extra.BirthCertificateFirst,
				BirthCertificateSecond = x.Extra.BirthCertificateSecond,
				BirthCertificateThird = x.Extra.BirthCertificateThird,
				BirthCertificateForth = x.Extra.BirthCertificateForth,
				BirthCertificateFifth = x.Extra.BirthCertificateFifth,
				VisualAuthentication = x.Extra.VisualAuthentication,
				ESignature = x.Extra.ESignature
			}
	};

	public static Expression<Func<ParkingEntity, ParkingResponse>> ParkingSelector(ParkingSelectorArgs args) => x => new ParkingResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		CreatorId = x.CreatorId,
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Bio = x.Creator.Bio,
				Birthdate = x.Creator.Birthdate,
				CreatedAt = x.Creator.CreatedAt,
				CreatorId = x.Creator.CreatorId,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList(),
				Addresses = args.Creator.Address == null ? null : x.Creator.Addresses.AsQueryable().Select(AddressSelector(args.Creator.Address)).ToList(),
				BankAccounts = args.Creator.BankAccount == null ? null : x.Creator.BankAccounts.AsQueryable().Select(BankAccountSelector(args.Creator.BankAccount)).ToList(),
				Terminals = args.Creator.Terminal == null ? null : x.Creator.Terminals.AsQueryable().Select(TerminalSelector(args.Creator.Terminal)).ToList(),
				Invoices = args.Creator.Invoice == null ? null : x.Creator.Invoices.AsQueryable().Select(InvoiceSelector(args.Creator.Invoice)).ToList(),
				Txns = args.Creator.Txns == null ? null : x.Creator.Txns.AsQueryable().Select(TxnSelector(args.Creator.Txns)).ToList(),
				SimCards = args.Creator.SimCard == null ? null : x.Creator.SimCards.AsQueryable().Select(SimCardSelector(args.Creator.SimCard)).ToList(),
				Wallets = args.Creator.Wallet == null ? null : x.Creator.Wallets.AsQueryable().Select(WalletSelector(args.Creator.Wallet)).ToList()
			}
	};

	public static Expression<Func<ParkingReportEntity, ParkingReportResponse>> ParkingReportSelector(ParkingReportSelectorArgs args) => x => new ParkingReportResponse {
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
		Parking = args.Parking == null
			? null
			: new ParkingResponse {
				Id = x.Parking.Id,
				CreatedAt = x.Parking.CreatedAt,
				JsonData = x.Parking.JsonData,
				Tags = x.Parking.Tags,
				CreatorId = x.Parking.CreatorId,
				Title = x.Parking.Title
			},
		Vehicle = args.Vehicle == null
			? null
			: new VehicleResponse {
				Id = x.Vehicle.Id,
				CreatedAt = x.Vehicle.CreatedAt,
				JsonData = x.Vehicle.JsonData,
				Tags = x.Vehicle.Tags,
				CreatorId = x.Vehicle.CreatorId,
				LicencePlate = x.Vehicle.LicencePlate,
				Title = x.Vehicle.Title,
				Brand = x.Vehicle.Brand,
				Color = x.Vehicle.Color
			}
	};

	public static Expression<Func<VehicleEntity, VehicleResponse>> VehicleSelector(VehicleSelectorArgs args) => x => new VehicleResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		LicencePlate = x.LicencePlate,
		CreatorId = x.CreatorId,
		Title = x.Title,
		Brand = x.Brand,
		Color = x.Color,
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
			}
	};

	public static Expression<Func<AgreementEntity, AgreementResponse>> VehicleSelector(AgreementSelectorArgs args) => x => new AgreementResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		TerminalId = x.TerminalId,
		Agreement = x.Agreement,
		Terminal = args.Terminal == null
			? null
			: new TerminalResponse {
				Id = x.Terminal.Id,
				CreatedAt = x.Terminal.CreatedAt,
				JsonData = x.Terminal.JsonData,
				Tags = x.Terminal.Tags,
				Serial = x.Terminal.Serial,
				SimCardNumber = x.Terminal.SimCardNumber,
				SimCardSerial = x.Terminal.SimCardSerial,
				Imei = x.Terminal.Imei,
				CreatorId = x.Terminal.CreatorId,
				TerminalId = x.Terminal.TerminalId,
				MerchantId = x.Terminal.MerchantId,
				Creator = args.Terminal.Creator == null
					? null
					: new UserResponse {
						Id = x.Terminal.Creator.Id,
						JsonData = x.Terminal.Creator.JsonData,
						Tags = x.Terminal.Creator.Tags,
						UserName = x.Terminal.Creator.UserName,
						PhoneNumber = x.Terminal.Creator.PhoneNumber,
						Email = x.Terminal.Creator.Email,
						FirstName = x.Terminal.Creator.FirstName,
						LastName = x.Terminal.Creator.LastName,
						NationalCode = x.Terminal.Creator.NationalCode,
						Media = args.Terminal.Creator.Media == null ? null : x.Terminal.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
						Categories = args.Terminal.Creator.Category == null ? null : x.Terminal.Creator.Categories.AsQueryable().Select(CategorySelector(args.Terminal.Creator.Category)).ToList(),
						Addresses = args.Terminal.Creator.Address == null ? null : x.Terminal.Creator.Addresses.AsQueryable().Select(AddressSelector(args.Terminal.Creator.Address)).ToList(),
						BankAccounts = args.Terminal.Creator.BankAccount == null ? null : x.Terminal.Creator.BankAccounts.AsQueryable().Select(BankAccountSelector(args.Terminal.Creator.BankAccount)).ToList(),
						Terminals = args.Terminal.Creator.Terminal == null ? null : x.Terminal.Creator.Terminals.AsQueryable().Select(TerminalSelector(args.Terminal.Creator.Terminal)).ToList(),
						Invoices = args.Terminal.Creator.Invoice == null ? null : x.Terminal.Creator.Invoices.AsQueryable().Select(InvoiceSelector(args.Terminal.Creator.Invoice)).ToList(),
						Txns = args.Terminal.Creator.Txns == null ? null : x.Terminal.Creator.Txns.AsQueryable().Select(TxnSelector(args.Terminal.Creator.Txns)).ToList(),
						SimCards = args.Terminal.Creator.SimCard == null ? null : x.Terminal.Creator.SimCards.AsQueryable().Select(SimCardSelector(args.Terminal.Creator.SimCard)).ToList(),
						Wallets = args.Terminal.Creator.Wallet == null ? null : x.Terminal.Creator.Wallets.AsQueryable().Select(WalletSelector(args.Terminal.Creator.Wallet)).ToList()
					}
			}
	};

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
			Deposit = x.Deposit,
			Rent = x.Rent,
			Stock = x.Stock,
			Point = x.Point,
			Order = x.Order,
			ParentId = x.ParentId,
			CreatorId = x.CreatorId,
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().Select(childSelector!).ToList() : null,
			CommentCount = args.CommentsCount ? x.Comments.Count : null,
			ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
			IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.CreatorId == args.UserId) : null,
			Creator = args.Creator == null
				? null
				: new UserResponse {
					Id = x.Creator.Id,
					JsonData = x.Creator.JsonData,
					Tags = x.Creator.Tags,
					UserName = x.Creator.UserName,
					PhoneNumber = x.Creator.PhoneNumber,
					Email = x.Creator.Email,
					FirstName = x.Creator.FirstName,
					LastName = x.Creator.LastName,
					NationalCode = x.Creator.NationalCode,
					Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
					Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
				}
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
			Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().Select(childSelector!).ToList() : null
		};
	}

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) => x => new ContentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList()
	};

	public static Expression<Func<TerminalEntity, TerminalResponse>> TerminalSelector(TerminalSelectorArgs args) => x => new TerminalResponse {
		Id = x.Id,
		Tags = x.Tags,
		Serial = x.Serial,
		JsonData = x.JsonData,
		CreatorId = x.CreatorId,
		TerminalId = x.TerminalId,
		MerchantId = x.MerchantId,
		SimCardSerial = x.SimCardSerial,
		SimCardNumber = x.SimCardNumber,
		Imei = x.Imei,
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList(),
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList()
			}
	};

	public static Expression<Func<TxnEntity, TxnResponse>> TxnSelector(TxnSelectorArgs args) => x => new TxnResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		Tags = x.Tags,
		Amount = x.Amount,
		TrackingNumber = x.TrackingNumber,
		JsonData = x.JsonData,
		UserId = x.UserId,
		InvoiceId = x.InvoiceId,
		Invoice = args.Invoice == null
			? null
			: new InvoiceResponse {
				DebtAmount = x.Invoice!.DebtAmount,
				CreditorAmount = x.Invoice.CreditorAmount,
				PaidAmount = x.Invoice.PaidAmount,
				PenaltyAmount = x.Invoice.PenaltyAmount,
				DueDate = x.Invoice.DueDate,
				JsonData = x.Invoice.JsonData,
				Tags = x.Invoice.Tags,
				Contract = args.Invoice.Contract == null
					? null
					: new ContractResponse {
						Id = x.Invoice.Contract.Id,
						JsonData = x.Invoice.Contract.JsonData,
						Tags = x.Invoice.Contract.Tags,
						StartDate = x.Invoice.Contract.StartDate,
						EndDate = x.Invoice.Contract.EndDate,
						Deposit = x.Invoice.Contract.Deposit,
						Rent = x.Invoice.Contract.Rent,
						UserId = x.Invoice.Contract.UserId,
						CreatorId = x.Invoice.Contract.CreatorId,
						ProductId = x.Invoice.Contract.ProductId
					}
			},
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				NationalCode = x.User.NationalCode,
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector()).ToList()
			}
	};

	public static Expression<Func<TicketEntity, TicketResponse>> TicketSelector(TicketSelectorArgs args) => x => new TicketResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		CreatorId = x.CreatorId,
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
		Creator = args.User == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Categories = args.User.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
				Media = args.User.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList()
			}
	};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(CommentSelectorArgs args) => x => new CommentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		CreatorId = x.CreatorId,
		UserId = x.UserId,
		ProductId = x.ProductId,
		ParentId = x.ParentId,
		Score = x.Score,
		Description = x.Description,
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector()).ToList(),
		Children = args.Children == null ? null : x.Children.AsQueryable().Select(CommentSelector(args.Children)).ToList(),
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User!.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				NationalCode = x.User.NationalCode,
				Bio = x.User.Bio,
				Birthdate = x.User.Birthdate,
				CreatedAt = x.User.CreatedAt,
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
				Addresses = args.User.Address == null ? null : x.User.Addresses.AsQueryable().Select(AddressSelector(args.User.Address)).ToList(),
				BankAccounts = args.User.BankAccount == null ? null : x.User.BankAccounts.AsQueryable().Select(BankAccountSelector(args.User.BankAccount)).ToList(),
				Terminals = args.User.Terminal == null ? null : x.User.Terminals.AsQueryable().Select(TerminalSelector(args.User.Terminal)).ToList(),
				Invoices = args.User.Invoice == null ? null : x.User.Invoices.AsQueryable().Select(InvoiceSelector(args.User.Invoice)).ToList(),
				Txns = args.User.Txns == null ? null : x.User.Txns.AsQueryable().Select(TxnSelector(args.User.Txns)).ToList(),
				SimCards = args.User.SimCard == null ? null : x.User.SimCards.AsQueryable().Select(SimCardSelector(args.User.SimCard)).ToList(),
				Wallets = args.User.Wallet == null ? null : x.User.Wallets.AsQueryable().Select(WalletSelector(args.User.Wallet)).ToList()
			},
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Bio = x.Creator.Bio,
				Birthdate = x.Creator.Birthdate,
				CreatedAt = x.Creator.CreatedAt,
				CreatorId = x.Creator.CreatorId,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList(),
				Addresses = args.Creator.Address == null ? null : x.Creator.Addresses.AsQueryable().Select(AddressSelector(args.Creator.Address)).ToList(),
				BankAccounts = args.Creator.BankAccount == null ? null : x.Creator.BankAccounts.AsQueryable().Select(BankAccountSelector(args.Creator.BankAccount)).ToList(),
				Terminals = args.Creator.Terminal == null ? null : x.Creator.Terminals.AsQueryable().Select(TerminalSelector(args.Creator.Terminal)).ToList(),
				Invoices = args.Creator.Invoice == null ? null : x.Creator.Invoices.AsQueryable().Select(InvoiceSelector(args.Creator.Invoice)).ToList(),
				Txns = args.Creator.Txns == null ? null : x.Creator.Txns.AsQueryable().Select(TxnSelector(args.Creator.Txns)).ToList(),
				SimCards = args.Creator.SimCard == null ? null : x.Creator.SimCards.AsQueryable().Select(SimCardSelector(args.Creator.SimCard)).ToList(),
				Wallets = args.Creator.Wallet == null ? null : x.Creator.Wallets.AsQueryable().Select(WalletSelector(args.Creator.Wallet)).ToList()
			}
	};

	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector(ContractSelectorArgs args) => x => new ContractResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		StartDate = x.StartDate,
		EndDate = x.EndDate,
		Deposit = x.Deposit,
		Rent = x.Rent,
		UserId = x.UserId,
		CreatorId = x.CreatorId,
		ProductId = x.ProductId,
		Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().Select(InvoiceSelector(args.Invoice)).ToList(),
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				NationalCode = x.User.NationalCode,
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList()
			},
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
			},
		Product = args.Product == null
			? null
			: new ProductResponse {
				Id = x.Product.Id,
				JsonData = x.Product.JsonData,
				Tags = x.Product.Tags,
				Title = x.Product.Title,
				Code = x.Product.Code,
				Subtitle = x.Product.Subtitle,
				Description = x.Product.Description,
				Slug = x.Product.Slug,
				Type = x.Product.Type,
				Content = x.Product.Content,
				Latitude = x.Product.Latitude,
				Longitude = x.Product.Longitude,
				Deposit = x.Product.Deposit,
				Rent = x.Product.Rent,
				Stock = x.Product.Stock,
				Point = x.Product.Point,
				Order = x.Product.Order,
				CreatorId = x.Product.CreatorId,
				Categories = args.Product.Category == null ? null : x.Product.Categories.AsQueryable().Select(CategorySelector(args.Product.Category)).ToList(),
				Media = args.Product.Media == null ? null : x.Product.Media.AsQueryable().Select(MediaSelector()).ToList()
			}
	};

	public static Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector(InvoiceSelectorArgs args) => x => new InvoiceResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		DebtAmount = x.DebtAmount,
		CreditorAmount = x.CreditorAmount,
		PaidAmount = x.PaidAmount,
		PenaltyAmount = x.PenaltyAmount,
		DueDate = x.DueDate,
		Contract = args.Contract == null
			? null
			: new ContractResponse {
				Id = x.Contract.Id,
				JsonData = x.Contract.JsonData,
				Tags = x.Contract.Tags,
				StartDate = x.Contract.StartDate,
				EndDate = x.Contract.EndDate,
				Deposit = x.Contract.Deposit,
				Rent = x.Contract.Rent,
				UserId = x.Contract.UserId,
				CreatorId = x.Contract.CreatorId,
				ProductId = x.Contract.ProductId,
				Creator = args.Contract.Creator == null
					? null
					: new UserResponse {
						Id = x.Contract.Creator.Id,
						JsonData = x.Contract.Creator.JsonData,
						Tags = x.Contract.Creator.Tags,
						UserName = x.Contract.Creator.UserName,
						PhoneNumber = x.Contract.Creator.PhoneNumber,
						Email = x.Contract.Creator.Email,
						FirstName = x.Contract.Creator.FirstName,
						LastName = x.Contract.Creator.LastName,
						NationalCode = x.Contract.Creator.NationalCode,
						Categories = args.Contract.Creator.Category == null ? null : x.Contract.Creator.Categories.AsQueryable().Select(CategorySelector(args.Contract.Creator.Category)).ToList(),
						Media = args.Contract.Creator.Media == null ? null : x.Contract.Creator.Media.AsQueryable().Select(MediaSelector()).ToList()
					},
				User = args.Contract.User == null
					? null
					: new UserResponse {
						Id = x.Contract.User.Id,
						JsonData = x.Contract.User.JsonData,
						Tags = x.Contract.User.Tags,
						UserName = x.Contract.User.UserName,
						PhoneNumber = x.Contract.User.PhoneNumber,
						Email = x.Contract.User.Email,
						FirstName = x.Contract.User.FirstName,
						LastName = x.Contract.User.LastName,
						NationalCode = x.Contract.User.NationalCode,
						Categories = args.Contract.User.Category == null ? null : x.Contract.User.Categories.AsQueryable().Select(CategorySelector(args.Contract.User.Category)).ToList(),
						Media = args.Contract.User.Media == null ? null : x.Contract.User.Media.AsQueryable().Select(MediaSelector()).ToList()
					},
				Product = args.Contract.Product == null
					? null
					: new ProductResponse {
						Id = x.Contract.Product.Id,
						JsonData = x.Contract.Product.JsonData,
						Tags = x.Contract.Product.Tags,
						Title = x.Contract.Product.Title,
						Code = x.Contract.Product.Code,
						Deposit = x.Contract.Product.Deposit,
						Rent = x.Contract.Product.Rent,
						CreatorId = x.Contract.Product.CreatorId,
						Categories = args.Contract.Product.Category == null ? null : x.Contract.Product.Categories.AsQueryable().Select(CategorySelector(args.Contract.Product.Category)).ToList(),
						Media = args.Contract.Product.Media == null ? null : x.Contract.Product.Media.AsQueryable().Select(MediaSelector()).ToList()
					}
			}
	};

	public static Expression<Func<VasEntity, VasResponse>> VasSelector(VasSelectorArgs args) => x => new VasResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		Tags = x.Tags,
		CreatorId = x.CreatorId,
		Amount = x.Amount,
		AuthorizeCode = x.AuthorizeCode,
		OrganizationType = x.OrganizationType,
		OrganizationName = x.OrganizationName,
		BillId = x.BillId,
		PaymentId = x.PaymentId,
		TxnId = x.TxnId,
		WalletTxnId = x.WalletTxnId,
		JsonData = x.JsonData,
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				NationalCode = x.Creator.NationalCode,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList()
			},
		Txn = args.Txn == null
			? null
			: new TxnResponse {
				Id = x.Txn!.Id,
				CreatedAt = x.Txn.CreatedAt,
				JsonData = x.Txn.JsonData,
				Tags = x.Txn.Tags,
				CreatorId = x.Txn.CreatorId,
				TrackingNumber = x.Txn.TrackingNumber,
				InvoiceId = x.Txn.InvoiceId,
				UserId = x.Txn.UserId,
				Amount = x.Txn.Amount
			},
		WalletTxn = args.WalletTxn == null
			? null
			: new WalletTxnResponse {
				Id = x.WalletTxn!.Id,
				CreatedAt = x.WalletTxn.CreatedAt,
				JsonData = x.WalletTxn.JsonData,
				Tags = x.WalletTxn.Tags,
				CreatorId = x.WalletTxn.CreatorId,
				SenderId = x.WalletTxn.SenderId,
				ReceiverId = x.WalletTxn.ReceiverId,
				Amount = x.WalletTxn.Amount,
				Sender = args.WalletTxn.Sender == null ? null : new UserResponse {
					Id = x.WalletTxn.Creator.Id,
					JsonData = x.WalletTxn.Creator.JsonData,
					Tags = x.WalletTxn.Creator.Tags,
					UserName = x.WalletTxn.Creator.UserName,
					PhoneNumber = x.WalletTxn.Creator.PhoneNumber,
					Email = x.WalletTxn.Creator.Email,
					FirstName = x.WalletTxn.Creator.FirstName,
					LastName = x.WalletTxn.Creator.LastName,
					NationalCode = x.WalletTxn.Creator.NationalCode,
					Media = args.WalletTxn.Sender.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
					Categories = args.WalletTxn.Sender.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.WalletTxn.Sender.Category)).ToList()
				},
				Receiver = args.WalletTxn.Receiver == null ? null : new UserResponse {
					Id = x.WalletTxn.Receiver.Id,
					JsonData = x.WalletTxn.Receiver.JsonData,
					Tags = x.WalletTxn.Receiver.Tags,
					UserName = x.WalletTxn.Receiver.UserName,
					PhoneNumber = x.WalletTxn.Receiver.PhoneNumber,
					Email = x.WalletTxn.Receiver.Email,
					FirstName = x.WalletTxn.Receiver.FirstName,
					LastName = x.WalletTxn.Receiver.LastName,
					NationalCode = x.WalletTxn.Receiver.NationalCode,
					Media = args.WalletTxn.Receiver.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector()).ToList(),
					Categories = args.WalletTxn.Receiver.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.WalletTxn.Receiver.Category)).ToList()
				}
			}
	};
}