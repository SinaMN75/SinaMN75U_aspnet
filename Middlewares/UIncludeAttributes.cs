namespace SinaMN75U.Middlewares;

[AttributeUsage(AttributeTargets.Property)]
public class NotIncludedAttribute : Attribute { }

public static class ProjectionExtensions {
	public static IQueryable<TResult> ProjectExcludingNotIncluded<TEntity, TResult>(this IQueryable<TEntity> source)
		where TResult : new() {
		Type entityType = typeof(TEntity);
		Type resultType = typeof(TResult);
		ParameterExpression parameter = Expression.Parameter(entityType, "x");

		List<MemberAssignment> bindings = entityType.GetProperties()
			.Where(p => !p.IsDefined(typeof(NotIncludedAttribute)))
			.Where(p => resultType.GetProperty(p.Name) != null)
			.Select(p =>
				Expression.Bind(resultType.GetProperty(p.Name)!,
					Expression.Property(parameter, p)))
			.ToList();

		MemberInitExpression body = Expression.MemberInit(Expression.New(resultType), bindings);
		Expression<Func<TEntity, TResult>> selector = Expression.Lambda<Func<TEntity, TResult>>(body, parameter);

		return source.Select(selector);
	}

	public static IQueryable<T> ProjectExcludingMarked<T>(this IQueryable<T> source) {
		Type type = typeof(T);
		ParameterExpression parameter = Expression.Parameter(type, "x");

		MemberAssignment[] bindings = type.GetProperties()
			.Where(p => !p.IsDefined(typeof(NotIncludedAttribute)))
			.Where(p => p.CanRead && p.CanWrite)
			.Select(p => Expression.Bind(p, Expression.Property(parameter, p)))
			.ToArray();

		MemberInitExpression body = Expression.MemberInit(Expression.New(type), bindings);
		Expression<Func<T, T>> lambda = Expression.Lambda<Func<T, T>>(body, parameter);

		return source.Select(lambda);
	}

	public static ICollection<T> ThenTransform<T>(this ICollection<T> source, Func<ICollection<T>, ICollection<T>> transformer) {
		return transformer(source);
	}
}