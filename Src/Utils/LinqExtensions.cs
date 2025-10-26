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

	public static IQueryable<T> ApplyIncludeOptions<T>(
		this IQueryable<T> query,
		IncludeOptions opts,
		string childrenProperty = "Children")
		where T : class {
		// ✅ Include root navigation props
		query = opts.RootIncludes.Aggregate(query, (current, inc) => current.Include(inc));

		// ✅ Apply recursive includes only when requested
		if (opts.IncludeChildren && opts.MaxChildrenDepth > 0) {
			string basePath = childrenProperty;

			for (uint d = 1; d <= opts.MaxChildrenDepth; d++) {
				// Include: Children
				query = query.Include(basePath);

				// Include: Children.<EachRecursiveNavigation>
				query = opts.RecursiveIncludes.Aggregate(query, (current, nav) => current.Include($"{basePath}.{nav}"));

				// Next level: Children.Children
				basePath += $".{childrenProperty}";
			}
		}

		return query;
	}
}

public class IncludeOptions {
	public bool IncludeChildren { get; set; }
	public uint MaxChildrenDepth { get; set; } = 0;

	public List<string> RootIncludes { get; set; } = [];
	public List<string> RecursiveIncludes { get; set; } = [];

	public IncludeOptions Add(string include) {
		RootIncludes.Add(include);
		return this;
	}

	public IncludeOptions AddRecursive(string include) {
		RecursiveIncludes.Add(include);
		return this;
	}
}