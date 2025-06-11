namespace SinaMN75U.Middlewares;

[AttributeUsage(AttributeTargets.Property)]
public abstract class FilterAttribute(string targetProperty) : Attribute {
	public string TargetProperty { get; } = targetProperty;
}

public class ContainsAttribute(string targetProperty) : FilterAttribute(targetProperty);

public class EqualAttribute(string targetProperty) : FilterAttribute(targetProperty);

public class GreaterThanOrEqualAttribute(string targetProperty) : FilterAttribute(targetProperty);

public class LessThanOrEqualAttribute(string targetProperty) : FilterAttribute(targetProperty);

public static class QueryableFilterExtensions {
	public static IQueryable<TEntity> ApplyFilters<TEntity, TParams>(this IQueryable<TEntity> query, TParams filterParams) {
		Type paramType = typeof(TParams);
		Type entityType = typeof(TEntity);
		ParameterExpression parameter = Expression.Parameter(entityType, "x");

		foreach (PropertyInfo prop in paramType.GetProperties()) {
			object? value = prop.GetValue(filterParams);
			if (value == null || value is string s && string.IsNullOrWhiteSpace(s)) continue;

			foreach (FilterAttribute attr in prop.GetCustomAttributes<FilterAttribute>()) {
				MemberExpression targetProperty = Expression.Property(parameter, attr.TargetProperty);
				ConstantExpression constant = Expression.Constant(value);

				Expression? predicate = attr switch {
					ContainsAttribute => Expression.Call(targetProperty, typeof(string).GetMethod("Contains", [typeof(string)])!, constant),
					EqualAttribute => Expression.Equal(targetProperty, constant),
					GreaterThanOrEqualAttribute => Expression.GreaterThanOrEqual(targetProperty, constant),
					LessThanOrEqualAttribute => Expression.LessThanOrEqual(targetProperty, constant),

					_ => null
				};

				if (predicate == null) continue;
				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
				query = query.Where(lambda);
			}
		}

		return query;
	}
}