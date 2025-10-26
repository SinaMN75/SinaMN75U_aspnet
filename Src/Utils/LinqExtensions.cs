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

	public static IQueryable<T> IncludeRecursive<T>(
		this IQueryable<T> source,
		uint maxDepth,
		string childrenProperty = "Children",
		string mediaProperty = "Media"
	)
		where T : class {
		if (maxDepth < 1)
			return source;

		// Always include media for the root
		source = source.Include(mediaProperty);

		string includePath = childrenProperty;

		for (int depth = 1; depth <= maxDepth; depth++) {
			// Include: Children
			source = source.Include(includePath);

			// Include: Children.Media
			source = source.Include($"{includePath}.{mediaProperty}");

			// Build next level: Children.Children
			includePath += $".{childrenProperty}";
		}

		return source;
	}

	private static IQueryable<T> IncludeChildren<T>(
		IQueryable<T> source,
		uint maxDepth,
		Expression<Func<T, IEnumerable<T>>> childrenSelector,
		Expression<Func<T, object>> mediaSelector,
		uint currentDepth
	) where T : class {
		while (true) {
			if (currentDepth >= maxDepth) return source;

			// Include Children for the current level
			IIncludableQueryable<T, IEnumerable<T>> include = source.Include(childrenSelector);

			// Include Media for the current level of Children
			IIncludableQueryable<T, object> withMedia = include.ThenInclude(mediaSelector);

			// Recursively include deeper levels of Children
			if (currentDepth + 1 < maxDepth) {
				// Create a new queryable for the next level
				source = withMedia;
				currentDepth += 1;
				continue;
			}

			return withMedia;
		}
	}
}