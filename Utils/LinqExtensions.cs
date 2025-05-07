namespace SinaMN75U.Utils;

public static class LinqExtensions {
	public static IQueryable<TEntity> WhereIn<TEntity, TId>(
		this IQueryable<TEntity> query,
		Expression<Func<TEntity, TId>> idSelector,
		IEnumerable<TId> ids)
		where TEntity : class {
		List<TId> distinctIds = ids.Distinct().ToList();

		if (distinctIds.Count == 1) {
			TId id = distinctIds.First();
			BinaryExpression equalExpression = Expression.Equal(idSelector.Body, Expression.Constant(id));
			Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, idSelector.Parameters);
			return query.Where(lambda);
		}

		MethodInfo containsMethod = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(typeof(TId));

		MethodCallExpression containsExpression = Expression.Call(
			containsMethod,
			Expression.Constant(distinctIds),
			idSelector.Body);

		Expression<Func<TEntity, bool>> lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(containsExpression, idSelector.Parameters);

		return query.Where(lambdaExpression);
	}

	public static IQueryable<T> Paginate<T>(
		this IQueryable<T> query,
		int pageNumber,
		int pageSize) {
		if (pageNumber < 1)
			throw new ArgumentException("Page number must be greater than or equal to 1.");

		if (pageSize < 1)
			throw new ArgumentException("Page size must be greater than or equal to 1.");

		return query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize);
	}

	//var pagedResults = context.Products.Paginate(pageNumber, pageSize);

	public static IQueryable<TResult> SelectPartial<T, TResult>(
		this IQueryable<T> query,
		Expression<Func<T, TResult>> selector) => query.Select(selector);

	// var partialResults = context.Products.SelectPartial(p => new { p.Id, p.Name });

	public static IQueryable<T> IncludeIf<T, TProperty>(
		this IQueryable<T> query,
		bool condition,
		Expression<Func<T, TProperty>> navigationPropertyPath)
		where T : class => condition ? query.Include(navigationPropertyPath) : query;

	// var results = context.Products.IncludeIf(includeCategory, p => p.Category);
}