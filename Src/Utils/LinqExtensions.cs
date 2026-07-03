namespace SinaMN75U.Utils;

public static class LinqExtensions {
	public static IQueryable<TEntity> WhereIn<TEntity, TId>(
		this IQueryable<TEntity> query,
		Expression<Func<TEntity, TId>> idSelector,
		IEnumerable<TId> ids
	) where TEntity : class {
		List<TId> distinctIds = ids.Distinct().ToList();

		if (distinctIds.Count == 1) {
			TId id = distinctIds.First();
			BinaryExpression equalExpression = Expression.Equal(idSelector.Body, Expression.Constant(id));
			Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, idSelector.Parameters);
			return query.Where(lambda);
		}

		MethodInfo containsMethod = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(typeof(TId));

		MethodCallExpression containsExpression = Expression.Call(containsMethod, Expression.Constant(distinctIds), idSelector.Body);
		Expression<Func<TEntity, bool>> lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(containsExpression, idSelector.Parameters);
		return query.Where(lambdaExpression);
	}

	public static IQueryable<TEntity> ApplyReadParams<TEntity, TTag>(this IQueryable<TEntity> q, BaseReadParams<TTag> p)
		where TEntity : BaseEntity<TTag>
		where TTag : Enum {
		if (p.Ids.Count != 0) q = q.Where(x => p.Ids.Contains(x.Id));
		if (p.CreatorId != null) q = q.Where(x => x.CreatorId == p.CreatorId);
		if (p.FromCreatedAt != null) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt != null) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt);
		if (p.Tags != null && p.Tags.Count != 0) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));

		string name = p.OrderBy.ToString();
		bool desc = name.EndsWith("Descending");
		if (desc) name = name[..^"Descending".Length];
		PropertyInfo prop = typeof(TEntity).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) ?? typeof(TEntity).GetProperty(nameof(BaseEntity<TTag>.CreatedAt))!;

		ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
		LambdaExpression key = Expression.Lambda(Expression.Property(param, prop), param);
		MethodCallExpression call = Expression.Call(typeof(Queryable), desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy), [typeof(TEntity), prop.PropertyType], q.Expression, Expression.Quote(key));

		return q.Provider.CreateQuery<TEntity>(call);
	}
	
	public static IQueryable<TEntity> ApplyAdminScope<TEntity, TTag>(this IQueryable<TEntity> q, JwtClaimData? userData) where TEntity : BaseEntity<TTag> where TTag : Enum {
		if (userData is { IsSuperAdmin: true }) return q;
		Guid uid = userData?.Id ?? Guid.Empty;
		return q.Where(x => x.CreatorId == uid || x.AdminUserIds.Count == 0 || x.AdminUserIds.Contains(uid));
	}

	public static TEntity ApplyUpdateParam<TEntity, TTag, TJson>(this TEntity e, BaseUpdateParams<TTag> p)
		where TEntity : BaseEntity<TTag, TJson>
		where TTag : Enum
		where TJson : BaseJson {
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) foreach (TTag i in p.RemoveTags) e.Tags.Remove(i);
		if (p.AdminUserIds.IsNotNullOrEmpty()) e.AdminUserIds = p.AdminUserIds;
		if (p.AddAdminUserIds.IsNotNullOrEmpty()) e.AdminUserIds.AddRangeIfNotExist(p.AddAdminUserIds);
		if (p.RemoveAdminUserIds.IsNotNullOrEmpty()) foreach (Guid i in p.RemoveAdminUserIds) e.AdminUserIds.Remove(i);
		if (p.Detail1.IsNotNullOrEmpty()) e.JsonData.Detail1 = p.Detail1;
		if (p.Detail2.IsNotNullOrEmpty()) e.JsonData.Detail2 = p.Detail2;
		return e;
	}
}