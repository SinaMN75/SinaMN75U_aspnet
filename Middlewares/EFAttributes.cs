namespace SinaMN75U.Middlewares;

using System.Collections.Concurrent;

[AttributeUsage(AttributeTargets.Property)]
public abstract class UFilterAttribute(string targetProperty) : Attribute {
	public string TargetProperty { get; } = targetProperty;
}

public class UFilterContainsAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterStartsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterEndsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterNotEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterGreaterThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterGreaterThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterLessThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterLessThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterBetweenAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterInAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterNotInAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterHasAnyAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterHasAllAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterOverlapsAttribute(string targetProperty) : UFilterAttribute(targetProperty); // New: any overlap

public class UFilterIsTrueAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterIsFalseAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterDateAfterAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterDateBeforeAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterIsNullAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterIsNotNullAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterLengthEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterLengthGreaterAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UFilterLengthLessAttribute(string targetProperty) : UFilterAttribute(targetProperty);

[AttributeUsage(AttributeTargets.Property)]
public class USortAttribute(string targetProperty, bool descending = false) : Attribute {
	public string TargetProperty { get; } = targetProperty;
	public bool Descending { get; } = descending;
}

public static class UQueryableFilterExtensions {
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

	public static IQueryable<TEntity> ApplyFilters<TEntity, TParams>(this IQueryable<TEntity> query, TParams filterParams) {
		Type paramType = typeof(TParams);
		Type entityType = typeof(TEntity);
		ParameterExpression parameter = Expression.Parameter(entityType, "x");
		PropertyInfo[] props = PropertyCache.GetOrAdd(paramType, t => t.GetProperties());

		foreach (PropertyInfo prop in props) {
			object? value = prop.GetValue(filterParams);
			if (value == null || value is string s && string.IsNullOrWhiteSpace(s)) continue;

			foreach (UFilterAttribute attr in prop.GetCustomAttributes<UFilterAttribute>()) {
				try {
					MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
					Expression? predicate = BuildPredicateExpression(attr, target, value);

					if (predicate == null) continue;

					Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
					query = query.Where(lambda);
				}
				catch (Exception ex) {
					throw new InvalidOperationException($"Failed to build filter for property {prop.Name} with attribute {attr.GetType().Name}. Target: {attr.TargetProperty}", ex);
				}
			}

			if (prop.GetCustomAttribute<UFilterBetweenAttribute>() is not { } betweenAttr || value is not IList { Count: 2 } range) continue;

			try {
				MemberExpression target = BuildNestedProperty(parameter, betweenAttr.TargetProperty);
				ConstantExpression from = Expression.Constant(range[0], target.Type);
				ConstantExpression to = Expression.Constant(range[1], target.Type);
				BinaryExpression ge = Expression.GreaterThanOrEqual(target, from);
				BinaryExpression le = Expression.LessThanOrEqual(target, to);
				BinaryExpression combined = Expression.AndAlso(ge, le);
				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
				query = query.Where(lambda);
			}
			catch (Exception ex) {
				throw new InvalidOperationException($"Failed to build between filter for property {prop.Name}. Target: {betweenAttr.TargetProperty}", ex);
			}
		}

		return query;
	}

	private static Expression? BuildPredicateExpression(UFilterAttribute attr, MemberExpression target, object value) {
		try {
			return attr switch {
				UFilterContainsAttribute => Expression.Call(target, nameof(string.Contains), null, Expression.Constant(value)),
				UFilterStartsWithAttribute => Expression.Call(target, nameof(string.StartsWith), null, Expression.Constant(value)),
				UFilterEndsWithAttribute => Expression.Call(target, nameof(string.EndsWith), null, Expression.Constant(value)),
				UFilterEqualAttribute => Expression.Equal(target, Expression.Constant(value, target.Type)),
				UFilterNotEqualAttribute => Expression.NotEqual(target, Expression.Constant(value, target.Type)),
				UFilterGreaterThanAttribute => Expression.GreaterThan(target, Expression.Constant(value, target.Type)),
				UFilterGreaterThanOrEqualAttribute => Expression.GreaterThanOrEqual(target, Expression.Constant(value, target.Type)),
				UFilterLessThanAttribute => Expression.LessThan(target, Expression.Constant(value, target.Type)),
				UFilterLessThanOrEqualAttribute => Expression.LessThanOrEqual(target, Expression.Constant(value, target.Type)),
				UFilterIsTrueAttribute => Expression.Equal(target, Expression.Constant(true)),
				UFilterIsFalseAttribute => Expression.Equal(target, Expression.Constant(false)),
				UFilterDateAfterAttribute => Expression.GreaterThan(target, Expression.Constant(value, target.Type)),
				UFilterDateBeforeAttribute => Expression.LessThan(target, Expression.Constant(value, target.Type)),
				UFilterInAttribute => BuildContainsExpression(target, value),
				UFilterNotInAttribute => Expression.Not(BuildContainsExpression(target, value)),
				UFilterHasAnyAttribute => BuildHasAnyExpression(target, value),
				UFilterHasAllAttribute => BuildHasAllExpression(target, value),
				UFilterOverlapsAttribute => BuildOverlapExpression(target, value),
				UFilterIsNullAttribute => Expression.Equal(target, Expression.Constant(null)),
				UFilterIsNotNullAttribute => Expression.NotEqual(target, Expression.Constant(null)),
				UFilterLengthEqualAttribute => Expression.Equal(Expression.Property(target, "Length"), Expression.Constant(value)),
				UFilterLengthGreaterAttribute => Expression.GreaterThan(Expression.Property(target, "Length"), Expression.Constant(value)),
				UFilterLengthLessAttribute => Expression.LessThan(Expression.Property(target, "Length"), Expression.Constant(value)),
				_ => null
			};
		}
		catch (Exception ex) {
			throw new InvalidOperationException($"Failed to build predicate for attribute {attr.GetType().Name}. " +
			                                    $"Target type: {target.Type}, Value type: {value.GetType()}", ex);
		}
	}

	public static IQueryable<TEntity> ApplySorting<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) {
		PropertyInfo[] props = PropertyCache.GetOrAdd(typeof(TParams), t => t.GetProperties());

		foreach (PropertyInfo prop in props) {
			if (prop.GetCustomAttribute<USortAttribute>() is not { } attr) continue;
			if (prop.GetValue(param) is not true) continue;

			try {
				ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
				MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
				LambdaExpression lambda = Expression.Lambda(target, parameter);
				string methodName = attr.Descending ? "OrderByDescending" : "OrderBy";

				MethodInfo method = typeof(Queryable).GetMethods()
					.First(m => m.Name == methodName && m.GetParameters().Length == 2)
					.MakeGenericMethod(typeof(TEntity), target.Type);

				query = (IQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
			}
			catch (Exception ex) {
				throw new InvalidOperationException($"Failed to apply sorting for property {prop.Name}. Target: {attr.TargetProperty}", ex);
			}
		}

		return query;
	}

	private static Expression BuildContainsExpression(Expression target, object value) {
		if (value is IEnumerable enumerable and not string) {
			Type elementType = target.Type;
			MethodInfo containsMethod = typeof(Enumerable).GetMethods()
				.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
				.MakeGenericMethod(elementType);

			// Convert the enumerable to the right type
			object? typedEnumerable = typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
				?.MakeGenericMethod(elementType)
				.Invoke(null, [enumerable]);

			return Expression.Call(containsMethod,
				Expression.Constant(typedEnumerable),
				target);
		}

		// Handle case where target is a collection and value is a single item
		if (target.Type.IsGenericType && target.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
			Type elementType = target.Type.GetGenericArguments()[0];
			MethodInfo containsMethod = typeof(Enumerable).GetMethods()
				.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
				.MakeGenericMethod(elementType);

			return Expression.Call(containsMethod, target, Expression.Constant(value, elementType));
		}

		throw new InvalidOperationException($"Cannot build Contains expression between types {target.Type} and {value.GetType()}");
	}

	private static Expression BuildHasAnyExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments()[0];
		ParameterExpression param = Expression.Parameter(elementType, "e");

		MethodInfo contains = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(elementType);

		MethodCallExpression call = Expression.Call(contains, Expression.Constant(value), param);
		LambdaExpression lambda = Expression.Lambda(call, param);

		return Expression.Call(typeof(Enumerable), "Any", [elementType], target, lambda);
	}

	private static Expression BuildHasAllExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments()[0];
		ParameterExpression param = Expression.Parameter(elementType, "e");

		MethodInfo contains = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(elementType);

		MethodCallExpression call = Expression.Call(contains, target, param);
		LambdaExpression lambda = Expression.Lambda(call, param);

		return Expression.Call(typeof(Enumerable), "All", [elementType], Expression.Constant(value), lambda);
	}

	private static Expression BuildOverlapExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments()[0];
		ParameterExpression param = Expression.Parameter(elementType, "e");

		MethodInfo contains = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(elementType);

		MethodCallExpression call = Expression.Call(contains, target, param);
		LambdaExpression lambda = Expression.Lambda(call, param);

		return Expression.Call(typeof(Enumerable), "Any", [elementType], Expression.Constant(value), lambda);
	}

	private static MemberExpression BuildNestedProperty(Expression parameter, string propertyPath) {
		Expression current = parameter;

		foreach (string propertyName in propertyPath.Split('.')) {
			try {
				PropertyInfo? property = current.Type.GetProperty(propertyName);
				if (property == null) {
					throw new InvalidOperationException($"Property '{propertyName}' not found on type '{current.Type.Name}'");
				}

				current = Expression.Property(current, property);
			}
			catch (ArgumentException ex) {
				throw new InvalidOperationException($"Failed to access property '{propertyName}' in path '{propertyPath}'. " +
				                                    $"Make sure the property exists and is accessible.", ex);
			}
		}

		if (current is not MemberExpression memberExpression) {
			throw new InvalidOperationException($"The property path '{propertyPath}' did not resolve to a member access expression.");
		}

		return memberExpression;
	}

	public static IQueryable<TEntity> ApplyQuery<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) {
		query = query.ApplyFilters(param);
		query = query.ApplySorting(param);
		return query;
	}
}