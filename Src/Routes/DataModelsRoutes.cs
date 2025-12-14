namespace SinaMN75U.Routes;

public static class DataModelRoutes {
	public static void MapDataModelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		Guid guid = Guid.NewGuid();
		DateTime dateTime = DateTime.UtcNow;
		const decimal dec = 10.0m;
		const int integer = 10;

		MediaResponse mediaResponse = new() {
			Path = "",
			Tags = [TagMedia.Image],
			JsonData = new MediaJson {
				Description = "",
				Title = ""
			},
		};

		CategoryResponse categoryResponse = new() {
			Parent = null,
			Children = null,
			Users = null,
			Products = null,
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			Tags = [TagCategory.Bed],
			Title = "",
			Order = integer,
			Code = "",
			ParentId = Guid.Empty,
			Media = [mediaResponse],
			JsonData = new CategoryJson {
				Subtitle = "",
				Link = "",
				Location = "",
				Type = "",
				Address = "",
				PhoneNumber = "",
				RelatedProducts = [guid]
			}
		};

		ChatBotResponse chatBotResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			Tags = [TagChatBot.DrHana],
			CreatorId = guid,
			JsonData = new ChatBotJsonData { History = [new ChatBotHistoryItem { User = "", Bot = "" }] }
		};

		CommentResponse commentResponse = new() {
			Parent = null,
			Creator = null,
			User = null,
			Product = null,
			Children = null,
			Category = null,
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			Tags = [TagComment.InQueue],
			Score = integer,
			Description = "",
			ParentId = guid,
			CreatorId = guid,
			UserId = guid,
			ProductId = guid,
			CategoryId = guid,
			Media = [mediaResponse],
			JsonData = new CommentJson {
				Reacts = [
					new CommentReacts {
						Tag = TagReaction.DisLike,
						UserId = guid
					}
				]
			}
			
		};

		ContentResponse contentResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			Tags = [TagContent.AboutUs],
			Media = [mediaResponse],
			JsonData = new ContentJson {
				Title = "",
				SubTitle = "",
				Description = "",
				Instagram = "",
				Telegram = "",
				Whatsapp = "",
				Phone = ""
			},
		};

		ContractResponse contractResponse = new() {
			User = null,
			Creator = null,
			Product = null,
			Invoices = null,
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new ContractJson {
				Description = ""
			},
			Tags = [TagContract.Daily],
			StartDate = dateTime,
			EndDate = dateTime,
			Deposit = dec,
			Rent = dec,
			UserId = guid,
			CreatorId = guid,
			ProductId = guid,
		};

		UserResponse userResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new UserJson {
				FcmToken = "",
				Address = "",
				FatherName = "",
				Weight = integer,
				Height = integer,
				Health1 = [""],
				Health2 = [""],
				FoodAllergies = [],
				DrugAllergies = [""],
				Sickness = [""],
				UserAnswerJson = [
					new UserAnswerJson {
						Date = dateTime,
						TotalScore = integer,
						Results = [
							new UserAnswerResultJson {
								Question = "",
								Answer = new QuestionOptionJson {
									Title = "",
									Hint = "",
									Score = integer
								}
							}
						],
						Label = "",
						Description = ""
					}
				],
				VisitCounts = [
					new VisitCount {
						UserId = guid,
						Count = integer
					}
				]
			},
			Tags = [TagUser.Female],
			UserName = "",
			PhoneNumber = "",
			Email = "",
			FirstName = "",
			LastName = "",
			Bio = "",
			Country = "",
			State = "",
			City = "",
			Birthdate = dateTime,
			Categories = [categoryResponse],
			Media = [mediaResponse],
			Contracts = [contractResponse],
		};

		FollowerFollowingCountResponse followerFollowingCountResponse = new() {
			Followers = integer,
			FollowedUsers = integer,
			FollowedProducts = integer,
			FollowedCategories = integer
		};

		InvoiceResponse invoiceResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new InvoiceJson {
				Description = "",
				PenaltyPrecentEveryDate = integer
			},
			Tags = [TagInvoice.Deposit],
			DebtAmount = dec,
			CreditorAmount = dec,
			PaidAmount = dec,
			PenaltyAmount = dec,
			DueDate = dateTime,
			Contract = contractResponse,
		};

		ProductResponse productResponse = new() {
			Children = null,
			Parent = null,
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new ProductJson {
				ActionType = "",
				ActionTitle = "",
				ActionUri = "",
				Details = "",
				PhoneNumber = "",
				Address = "",
				VisitCounts = [
					new VisitCount {
						UserId = guid,
						Count = integer
					}
				],
				PointCounts = [
					new PointCount {
						UserId = guid,
						Point = integer
					}
				],
				RelatedProducts = [guid]
			},
			Tags = [TagProduct.AwaitingPayment],
			Title = "",
			Code = "",
			Subtitle = "",
			Description = "",
			Slug = "",
			Type = "",
			Content = "",
			Latitude = dec,
			Longitude = dec,
			Deposit = dec,
			Rent = dec,
			Stock = integer,
			Point = integer,
			Order = integer,
			ParentId = guid,
			Creator = userResponse,
			CreatorId = guid,
			Categories = [categoryResponse],
			Media = [mediaResponse],
			CommentCount = integer,
			IsFollowing = false,
			ChildrenCount = integer,
		};

		TicketResponse ticketResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new TicketJson {
				Title = "",
				Description = "",
				Instagram = "",
				Telegram = "",
				Whatsapp = "",
				Phone = ""
			},
			Tags = [TagTicket.Admin],
			Media = [mediaResponse],
			Creator = userResponse,
			CreatorId = guid,
		};

		TxnResponse txnResponse = new() {
			Id = guid,
			CreatedAt = dateTime,
			UpdatedAt = dateTime,
			DeletedAt = dateTime,
			JsonData = new TxnJson {
				GatewayName = ""
			},
			Tags = [TagTxn.Cash],
			Amount = dec,
			TrackingNumber = "",
			PaidAt = dateTime,
			InvoiceId = guid,
			Invoice = invoiceResponse,
			UserId = guid,
			User = userResponse
		};


		categoryResponse.Parent = categoryResponse;
		categoryResponse.Children = [categoryResponse];
		categoryResponse.Users = [userResponse];
		categoryResponse.Products = [productResponse];

		commentResponse.Parent = commentResponse;
		commentResponse.Creator = userResponse;
		commentResponse.Children = [commentResponse];
		commentResponse.Product = productResponse;
		commentResponse.User = userResponse;
		commentResponse.Category = categoryResponse;

		contractResponse.Creator = userResponse;
		contractResponse.User = userResponse;
		contractResponse.Product = productResponse;
		contractResponse.Invoices = [invoiceResponse];
		
		productResponse.Parent = productResponse;
		productResponse.Children = [productResponse];

		r.MapPost("Responses", () => {
			Results.Ok(new DataModelResponse {
				Category = categoryResponse,
				ChatBot = chatBotResponse,
				Comment = commentResponse,
				Content = contentResponse,
				Contract = contractResponse,
				FollowerFollowingCount = followerFollowingCountResponse,
				Invoice = invoiceResponse,
				Media = mediaResponse,
				Product = productResponse,
				Ticket = ticketResponse,
				Txn = txnResponse,
				User = userResponse
			});
		});
	}
}