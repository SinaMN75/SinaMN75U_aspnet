namespace SinaMN75U.Routes;

public static class DataModelRoutes {
	public static void MapDataModelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapGet("Read", () => {
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
				},
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
	}
}