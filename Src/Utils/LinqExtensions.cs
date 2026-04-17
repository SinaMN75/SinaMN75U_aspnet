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

	public static IQueryable<TEntity> ApplyReadParams<TEntity, TTag, TJson>(this IQueryable<TEntity> q, BaseReadParams<TTag> p)
		where TEntity : BaseEntity<TTag, TJson>
		where TTag : Enum
		where TJson : BaseJsonData {
		if (p.Ids.Any()) q = q.Where(x => p.Ids.Contains(x.Id));
		if (p.CreatorId != null) q = q.Where(x => x.CreatorId == p.CreatorId);
		if (p.FromCreatedAt != null) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt != null) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt);
		if (p.Tags?.Any() == true) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));

		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		else if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);

		return q;
	}

	public static TEntity ApplyUpdateParam<TEntity, TTag, TJson>(this TEntity e, BaseUpdateParams<TTag> p)
		where TEntity : BaseEntity<TTag, TJson>
		where TTag : Enum
		where TJson : BaseJsonData {
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Detail1.IsNotNullOrEmpty()) e.JsonData.Detail1 = p.Detail1;
		if (p.Detail2.IsNotNullOrEmpty()) e.JsonData.Detail2 = p.Detail2;
		return e;
	}
}