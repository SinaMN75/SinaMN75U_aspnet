namespace SinaMN75U.Routes;

public static class DataModelRoutes {
	public static void MapDataModelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapGet("Responses", () => {
			Guid guid = Guid.CreateVersion7();
			DateTime dateTime = DateTime.UtcNow;
			MediaEntity media = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Tags = [TagMedia.Image],
				Path = "",
				JsonData = new MediaJson {
					Title = "",
					Description = ""
				}
			};

			CategoryEntity category = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Title = "",
				ParentId = guid,
				Order = 0,
				Media = [media],
				Tags = [0],
				JsonData = new CategoryJson {
					Subtitle = "",
					Link = "",
					Location = "",
					Type = "",
					RelatedProducts = [guid, guid]
				},
				Children = [
					new CategoryEntity {
						Id = guid,
						CreatedAt = dateTime,
						UpdatedAt = dateTime,
						Title = "",
						ParentId = guid,
						Order = 0,
						Children = [],
						Media = [media, media],
						Tags = [TagCategory.Category],
						JsonData = new CategoryJson {
							Subtitle = "",
							Link = "",
							Location = "",
							Type = "",
							RelatedProducts = [guid, guid]
						}
					}
				]
			};

			UserEntity user = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Tags = [TagUser.Male],
				UserName = "",
				Password = "",
				RefreshToken = "",
				PhoneNumber = "",
				Email = "",
				FirstName = "",
				LastName = "",
				Bio = "",
				Country = "",
				State = "",
				City = "",
				Birthdate = dateTime,
				Categories = [category],
				Media = [media],
				JsonData = new UserJson {
					FcmToken = "",
					Health1 = [""],
					FoodAllergies = [""],
					DrugAllergies = [""],
					Sickness = [""],
					Weight = 0,
					Height = 0,
					Address = "",
					FatherName = "",
					UserAnswerJson = [
						new UserAnswerJson {
							Date = dateTime,
							TotalScore = 0,
							Results = [
								new UserAnswerResultJson {
									Question = "",
									Answer = new QuestionOptionJson {
										Title = "",
										Hint = "",
										Score = 0
									}
								}
							],
							Label = "",
							Description = ""
						}
					]
				}
			};

			ProductEntity product = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Title = "",
				Code = "",
				Subtitle = "",
				Description = "",
				Slug = "",
				Type = "",
				Content = "",
				Latitude = 1.5,
				Longitude = 1.5,
				Stock = 1,
				Price = 1.0,
				Parent = null,
				ParentId = guid,
				UserId = guid,
				User = user,
				Children = [],
				Media = [media],
				Categories = [category],
				Tags = [TagProduct.AwaitingPayment],
				JsonData = new ProductJson {
					ActionType = "",
					ActionTitle = "",
					ActionUri = "",
					Details = "",
					VisitCounts = [new VisitCount { UserId = guid, Count = 1 }],
					RelatedProducts = [guid]
				}
			};
			CommentEntity comment = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Tags = [TagComment.Private],
				Score = 0,
				Description = "",
				ParentId = guid,
				User = user,
				UserId = guid,
				TargetUser = user,
				TargetUserId = guid,
				Product = product,
				ProductId = guid,
				Media = [media],
				JsonData = new CommentJson { Reacts = [new CommentReacts { Tag = TagReaction.Like, UserId = guid }] },
				Children = [
					new CommentEntity {
						Id = guid,
						CreatedAt = dateTime,
						UpdatedAt = dateTime,
						JsonData = new CommentJson { Reacts = [new CommentReacts { Tag = TagReaction.Like, UserId = guid }] },
						Tags = [TagComment.Private],
						Score = 0,
						Description = "",
						ParentId = guid,
						User = user,
						UserId = guid,
						TargetUser = user,
						TargetUserId = guid,
						Product = product,
						ProductId = guid,
						Children = [],
						Media = [media]
					}
				]
			};

			ContentEntity content = new() {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Tags = [1, 2, 3],
				Media = [media],
				JsonData = new ContentJson {
					Title = "",
					SubTitle = "",
					Description = "",
					Instagram = ""
				}
			};

			ExamEntity exam = new ExamEntity {
				Id = guid,
				CreatedAt = dateTime,
				UpdatedAt = dateTime,
				Tags = [TagExam.Test],
				Title = "",
				Description = "",
				CategoryId = guid,
				Category = category,
				JsonData = new ExamJson {
					Questions = [
						new QuestionJson {
							Order = 0,
							Title = "",
							Description = "",
							Options = [new QuestionOptionJson { Title = "", Hint = "", Score = 0 }]
						}
					],
					ScoreDetails = [
						new ExamScoreDetail {
							MinScore = 0,
							MaxScore = 0,
							Label = "",
							Description = ""
						}
					]
				}
			};

			DataModelResponse response = new() {
				Category = category,
				Comment = comment,
				Content = content,
				Exam = exam,
				Media = media,
				Product = product,
				User = user
			};
			return Results.Ok(response);
		}).Cache(60).Produces<DataModelResponse>();

		r.MapGet("Params", () => {
			Guid guid = Guid.CreateVersion7();
			DateTime dateTime = DateTime.UtcNow;

			ParamsResponse response = new() {
				RefreshTokenParams = new RefreshTokenParams {
					ApiKey = "",
					Token = "",
					RefreshToken = ""
				},
				GetMobileVerificationCodeForLoginParams = new GetMobileVerificationCodeForLoginParams {
					PhoneNumber = "",
					ApiKey = "",
					Token = ""
				},
				LoginWithEmailPasswordParams = new LoginWithEmailPasswordParams {
					Email = "",
					Password = "",
					ApiKey = "",
					Token = ""
				},
				LoginWithUserNamePasswordParams = new LoginWithUserNamePasswordParams {
					UserName = "",
					Password = "",
					ApiKey = "",
					Token = ""
				},
				RegisterParams = new RegisterParams {
					UserName = "",
					Email = "",
					PhoneNumber = "",
					Password = "",
					FirstName = "",
					LastName = "",
					Tags = [TagUser.Male],
					ApiKey = "",
					Token = ""
				},
				VerifyMobileForLoginParams = new VerifyMobileForLoginParams {
					PhoneNumber = "",
					Otp = "",
					FirstName = "",
					LastName = "",
					ApiKey = "",
					Token = ""
				},
				CategoryCreateParams = new CategoryCreateParams {
					Title = "",
					Subtitle = "",
					Tags = [TagCategory.Category],
					ParentId = guid,
					Order = 1,
					Location = "",
					Type = "",
					Link = "",
					RelatedProducts = [guid, guid],
					ApiKey = "",
					Token = ""
				},
				CategoryUpdateParams = new CategoryUpdateParams {
					Title = "",
					Subtitle = "",
					Link = "",
					Location = "",
					Type = "",
					Order = 2,
					ParentId = guid,
					RelatedProducts = [guid],
					AddRelatedProducts = [guid],
					RemoveRelatedProducts = [guid],
					ApiKey = "",
					Token = "",
					Id = guid,
					AddTags = [TagCategory.Category],
					RemoveTags = [TagCategory.Category],
					Tags = [TagCategory.Category]
				},
				CategoryReadParams = new CategoryReadParams {
					Ids = [guid, guid],
					ShowMedia = true,
					ApiKey = "",
					Token = ""
				},
				CommentCreateParams = new CommentCreateParams {
					Description = "",
					Score = 5,
					Reaction = TagReaction.Like,
					ParentId = guid,
					ProductId = guid,
					TargetUserId = guid,
					UserId = guid,
					Tags = [TagComment.InQueue],
					ApiKey = "",
					Token = ""
				},
				CommentUpdateParams = new CommentUpdateParams {
					Description = "",
					Score = 4,
					ApiKey = "",
					Token = "",
					Id = guid,
					AddTags = [TagComment.InQueue],
					RemoveTags = [TagComment.InQueue],
					Tags = [TagComment.InQueue]
				},
				CommentReadParams = new CommentReadParams {
					UserId = guid,
					ProductId = guid,
					TargetUserId = guid,
					ShowMedia = true,
					ApiKey = "",
					Token = "",
					PageSize = 0,
					PageNumber = 0,
					FromCreatedAt = dateTime,
					ToCreatedAt = dateTime,
					OrderByCreatedAt = false,
					OrderByCreatedAtDesc = false,
					OrderByUpdatedAt = false,
					OrderByUpdatedAtDesc = false,
					Tags = [TagComment.InQueue]
				},
				ContentCreateParams = new ContentCreateParams {
					Title = "",
					Description = "",
					SubTitle = "",
					Instagram = "",
					Tags = [1, 2, 3],
					ApiKey = "",
					Token = ""
				},
				ContentUpdateParams = new ContentUpdateParams {
					Id = guid,
					Title = "",
					SubTitle = "",
					Description = "",
					Instagram = "",
					AddTags = [1],
					RemoveTags = [2],
					ApiKey = "",
					Token = ""
				},
				ContentReadParams = new ContentReadParams {
					ShowMedia = true,
					ApiKey = "",
					Token = "",
					PageSize = 0,
					PageNumber = 0,
					FromCreatedAt = dateTime,
					ToCreatedAt = dateTime,
					OrderByCreatedAt = false,
					OrderByCreatedAtDesc = false,
					OrderByUpdatedAt = false,
					OrderByUpdatedAtDesc = false,
					Tags = [1]
				},
				ExamCreateParams = new ExamCreateParams {
					Title = "",
					Description = "",
					ApiKey = "",
					Token = "",
					Questions = [
						new QuestionJson {
							Order = 1,
							Title = "",
							Description = "",
							Options = [new QuestionOptionJson { Title = "", Hint = "", Score = 1 }]
						}
					],
					ScoreDetails = [
						new ExamScoreDetail {
							MinScore = 0,
							MaxScore = 10,
							Label = "",
							Description = ""
						}
					],
					CategoryId = guid,
					Tags = [TagExam.Test]
				},
				ExamReadParams = new ExamReadParams {
					CategoryId = guid,
					ApiKey = "",
					Token = "",
					PageSize = 0,
					PageNumber = 0,
					FromCreatedAt = dateTime,
					ToCreatedAt = dateTime,
					OrderByCreatedAt = false,
					OrderByCreatedAtDesc = false,
					OrderByUpdatedAt = false,
					OrderByUpdatedAtDesc = false,
					Tags = [TagExam.Test]
				},
				SubmitAnswersParams = new SubmitAnswersParams {
					Answers = [
						new UserAnswerResultJson {
							Question = "",
							Answer = new QuestionOptionJson { Title = "", Hint = "", Score = 1 }
						}
					],
					UserId = guid,
					ExamId = guid
				},
				MediaUpdateParams = new MediaUpdateParams {
					Id = guid,
					AddTags = (List<TagMedia>) [TagMedia.Image],
					RemoveTags = (List<TagMedia>) [TagMedia.Image],
					Title = "",
					Description = "",
					UserId = guid,
					ContentId = guid,
					CommentId = guid,
					CategoryId = guid,
					ProductId = guid,
					ApiKey = "",
					Token = ""
				},
				ProductCreateParams = new ProductCreateParams {
					Title = "",
					Code = "",
					Subtitle = "",
					Description = "",
					ActionType = "",
					ActionTitle = "",
					ActionUri = "",
					Slug = "",
					Type = "",
					Content = "",
					Latitude = 35.6895,
					Longitude = 51.3890,
					Stock = 100,
					Price = 99.99,
					Details = "",
					Tags = [TagProduct.Blog],
					Categories = [guid],
					RelatedProducts = [guid],
					ParentId = guid,
					UserId = guid,
					ApiKey = "",
					Token = ""
				},
				ProductUpdateParams = new ProductUpdateParams {
					Title = "",
					Code = "",
					Subtitle = "",
					Description = "",
					Slug = "",
					Type = "",
					Content = "",
					Latitude = 35.6895,
					Longitude = 51.3890,
					Stock = 50,
					Price = 149.99,
					ParentId = guid,
					UserId = guid,
					ActionType = "",
					ActionTitle = "",
					ActionUri = "",
					Details = "",
					RelatedProducts = [guid],
					AddRelatedProducts = [guid],
					RemoveRelatedProducts = [guid],
					AddCategories = [guid],
					RemoveCategories = [guid],
					ApiKey = "",
					Token = "",
					Id = guid,
					AddTags = [TagProduct.AwaitingPayment],
					RemoveTags = [TagProduct.AwaitingPayment],
					Tags = [TagProduct.AwaitingPayment]
				},
				ProductReadParams = new ProductReadParams {
					Query = "",
					Title = "",
					Code = "",
					ParentId = guid,
					UserId = guid,
					MinStock = 10,
					MaxStock = 100,
					MinPrice = 50,
					MaxPrice = 200,
					ShowCategories = true,
					ShowCategoriesMedia = true,
					ShowMedia = true,
					ShowUser = true,
					ShowUserMedia = true,
					ShowUserCategory = true,
					ShowChildren = true,
					ShowChildrenDepth = true,
					Ids = [guid],
					ApiKey = "",
					Token = "",
					PageSize = 0,
					PageNumber = 0,
					FromCreatedAt = dateTime,
					ToCreatedAt = dateTime,
					OrderByCreatedAt = false,
					OrderByCreatedAtDesc = false,
					OrderByUpdatedAt = false,
					OrderByUpdatedAtDesc = false,
					Tags = [TagProduct.AwaitingPayment]
				},
				UserCreateParams = new UserCreateParams {
					UserName = "",
					Password = "",
					PhoneNumber = "",
					Email = "",
					FirstName = "",
					LastName = "",
					Bio = "",
					Country = "",
					State = "",
					City = "",
					Birthdate = dateTime,
					Weight = 70,
					Height = 175,
					Address = "",
					FatherName = "",
					FcmToken = "",
					Health1 = [""],
					FoodAllergies = [""],
					DrugAllergies = [""],
					Sickness = [""],
					Tags = [TagUser.Male],
					Categories = [guid],
					ApiKey = "",
					Token = ""
				},
				UserReadParams = new UserReadParams {
					UserName = "",
					PhoneNumber = "",
					Email = "",
					Bio = "",
					StartBirthDate = dateTime.AddYears(-30),
					EndBirthDate = dateTime.AddYears(-20),
					Categories = [guid],
					ShowCategories = true,
					ShowMedia = true,
					OrderByLastName = true,
					OrderByLastNameDesc = false,
					ApiKey = "",
					Token = "",
					PageSize = 0,
					PageNumber = 0,
					FromCreatedAt = dateTime,
					ToCreatedAt = dateTime,
					OrderByCreatedAt = false,
					OrderByCreatedAtDesc = false,
					OrderByUpdatedAt = false,
					OrderByUpdatedAtDesc = false,
					Tags = [TagUser.Female]
				},
				UserUpdateParams = new UserUpdateParams {
					Password = "",
					FirstName = "",
					LastName = "",
					Country = "",
					State = "",
					City = "",
					UserName = "",
					PhoneNumber = "",
					Email = "",
					Bio = "",
					Birthdate = dateTime,
					FcmToken = "",
					Address = "",
					FatherName = "",
					Weight = 75,
					Height = 180,
					AddHealth1 = [""],
					RemoveHealth1 = [""],
					FoodAllergies = [""],
					DrugAllergies = [""],
					Sickness = [""],
					Health1 = [""],
					Categories = [guid],
					ApiKey = "",
					Token = "",
					Id = guid,
					AddTags = [TagUser.Female],
					RemoveTags = [TagUser.Female],
					Tags = [TagUser.Female]
				}
			};
			return Results.Ok(response);
		}).Cache(60).Produces<DataModelResponse>();
	}
}