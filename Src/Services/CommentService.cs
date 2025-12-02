namespace SinaMN75U.Services;

public interface ICommentService {
	public Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct);
}

public class CommentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : ICommentService {
	public async Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CommentResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<CommentEntity> e = await db.Set<CommentEntity>().AddAsync(p.MapToEntity(), ct);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse<CommentResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct) {
		IQueryable<CommentEntity> q = db.Set<CommentEntity>();
		if (p.ProductId.HasValue()) q = q.Where(x => x.ProductId == p.ProductId);
		if (p.UserId.HasValue()) q = q.Where(x => x.UserId == p.UserId);
		if (p.TargetUserId.HasValue()) q = q.Where(x => x.TargetUserId == p.TargetUserId);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		IQueryable<CommentResponse> list = q.Select(x => new CommentResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Score = x.Score,
			Description = x.Description,
			ParentId = x.ParentId,
			UserId = x.UserId,
			TargetUserId = x.TargetUserId,
			ProductId = x.ProductId,
			JsonData = x.JsonData,
			Tags = x.Tags,
			User = p.ShowUser
				? new UserResponse {
					Id = x.User.Id,
					JsonData = x.User.JsonData,
					Tags = x.User.Tags,
					UserName = x.User.UserName,
					PhoneNumber = x.User.PhoneNumber,
					Email = x.User.Email,
					FirstName = x.User.FirstName,
					LastName = x.User.LastName,
				}
				: null,
			TargetUser = p.ShowTargetUser && x.TargetUserId != null
				? new UserResponse {
					Id = x.TargetUser!.Id,
					JsonData = x.TargetUser.JsonData,
					Tags = x.TargetUser.Tags,
					UserName = x.TargetUser.UserName,
					PhoneNumber = x.TargetUser.PhoneNumber,
					Email = x.TargetUser.Email,
					FirstName = x.TargetUser.FirstName,
					LastName = x.TargetUser.LastName,
				}
				: null,
			Product = p.ShowProduct && x.ProductId != null
				? new ProductResponse {
					Id = x.Product!.Id,
					JsonData = x.Product!.JsonData,
					Tags = x.Product.Tags,
					Title = x.Product.Title,
					Code = x.Product.Code,
					Subtitle = x.Product.Subtitle,
					Description = x.Product.Description,
				}
				: null,
			Media = p.ShowMedia
				? x.Media.Select(y => new MediaResponse {
					Path = y.Path,
					JsonData = y.JsonData,
					Tags = y.Tags
				}).ToList()
				: null,
			Children = p.ShowChildren
				? x.Children.Select(y => new CommentResponse {
					Id = y.Id,
					CreatedAt = y.CreatedAt,
					UpdatedAt = y.UpdatedAt,
					DeletedAt = y.DeletedAt,
					Score = y.Score,
					Description = y.Description,
					ParentId = y.ParentId,
					UserId = y.UserId,
					TargetUserId = y.TargetUserId,
					ProductId = y.ProductId,
					JsonData = y.JsonData,
					Tags = y.Tags,
					User = p.ShowUser
						? new UserResponse {
							Id = y.User.Id,
							JsonData = y.User.JsonData,
							Tags = y.User.Tags,
							UserName = y.User.UserName,
							PhoneNumber = y.User.PhoneNumber,
							Email = y.User.Email,
							FirstName = y.User.FirstName,
							LastName = y.User.LastName,
						}
						: null,
					TargetUser = p.ShowTargetUser && y.TargetUserId != null
						? new UserResponse {
							Id = y.TargetUser!.Id,
							JsonData = y.TargetUser.JsonData,
							Tags = y.TargetUser.Tags,
							UserName = y.TargetUser.UserName,
							PhoneNumber = y.TargetUser.PhoneNumber,
							Email = y.TargetUser.Email,
							FirstName = y.TargetUser.FirstName,
							LastName = y.TargetUser.LastName,
						}
						: null,
					Product = p.ShowProduct && y.ProductId != null
						? new ProductResponse {
							Id = y.Product!.Id,
							JsonData = y.Product!.JsonData,
							Tags = y.Product.Tags,
							Title = y.Product.Title,
							Code = y.Product.Code,
							Subtitle = y.Product.Subtitle,
							Description = y.Product.Description,
						}
						: null,
					Media = p.ShowMedia
						? y.Media.Select(z => new MediaResponse {
							Path = z.Path,
							JsonData = z.JsonData,
							Tags = z.Tags
						}).ToList()
						: null,
				}).ToList()
				: null,
		});


		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct) {
		CommentResponse? e = await db.Set<CommentEntity>()
			.Select(x => new CommentResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Score = x.Score,
			Description = x.Description,
			ParentId = x.ParentId,
			UserId = x.UserId,
			TargetUserId = x.TargetUserId,
			ProductId = x.ProductId,
			JsonData = x.JsonData,
			Tags = x.Tags,
			User = new UserResponse {
					Id = x.User.Id,
					JsonData = x.User.JsonData,
					Tags = x.User.Tags,
					UserName = x.User.UserName,
					PhoneNumber = x.User.PhoneNumber,
					Email = x.User.Email,
					FirstName = x.User.FirstName,
					LastName = x.User.LastName,
				},
			TargetUser = x.TargetUserId == null ? null : new UserResponse {
					Id = x.TargetUser!.Id,
					JsonData = x.TargetUser.JsonData,
					Tags = x.TargetUser.Tags,
					UserName = x.TargetUser.UserName,
					PhoneNumber = x.TargetUser.PhoneNumber,
					Email = x.TargetUser.Email,
					FirstName = x.TargetUser.FirstName,
					LastName = x.TargetUser.LastName,
				},
			Product = x.ProductId == null 
				? null : new ProductResponse {
					Id = x.Product!.Id,
					JsonData = x.Product!.JsonData,
					Tags = x.Product.Tags,
					Title = x.Product.Title,
					Code = x.Product.Code,
					Subtitle = x.Product.Subtitle,
					Description = x.Product.Description,
				},
			Media = x.Media.Select(y => new MediaResponse {
					Path = y.Path,
					JsonData = y.JsonData,
					Tags = y.Tags
				}).ToList(),
			Children = x.Children.Select(y => new CommentResponse {
					Id = y.Id,
					CreatedAt = y.CreatedAt,
					UpdatedAt = y.UpdatedAt,
					DeletedAt = y.DeletedAt,
					Score = y.Score,
					Description = y.Description,
					ParentId = y.ParentId,
					UserId = y.UserId,
					TargetUserId = y.TargetUserId,
					ProductId = y.ProductId,
					JsonData = y.JsonData,
					Tags = y.Tags,
					User = new UserResponse {
							Id = y.User.Id,
							JsonData = y.User.JsonData,
							Tags = y.User.Tags,
							UserName = y.User.UserName,
							PhoneNumber = y.User.PhoneNumber,
							Email = y.User.Email,
							FirstName = y.User.FirstName,
							LastName = y.User.LastName,
						},
					Media = y.Media.Select(z => new MediaResponse {
							Path = z.Path,
							JsonData = z.JsonData,
							Tags = z.Tags
						}).ToList(),
				}).ToList(),
		})
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		
		return e == null ? new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("CommentNotFound")) : new UResponse<CommentResponse?>(e);
	}

	public async Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct) {
		CommentEntity? e = await db.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("CommentNotFound"));
		if (p.Score.IsNotNull()) e.Score = p.Score.Value;
		if (p.Description.HasValue()) e.Description = p.Description;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		db.Set<CommentEntity>().Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse<CommentResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		await db.Set<CommentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse();
	}

	public async Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.ProductId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}

	public async Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.TargetUserId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}
}