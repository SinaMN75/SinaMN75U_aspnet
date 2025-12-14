namespace SinaMN75U.Utils;

public static class LinqExtensions {
	public static IQueryable<TEntity> WhereIn<TEntity, TId>(
		this IQueryable<TEntity> query,
		Expression<Func<TEntity, TId>> idSelector,
		IEnumerable<TId> ids
	)
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
}