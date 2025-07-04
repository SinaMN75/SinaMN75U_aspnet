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

[AttributeUsage(AttributeTargets.Property)]
public class UIncludeAttribute(string navigationProperty) : Attribute {
	public string NavigationProperty { get; } = navigationProperty;
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
				MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
				ConstantExpression constant = Expression.Constant(value);
				Expression? predicate = attr switch {
					UFilterContainsAttribute => Expression.Call(target, nameof(string.Contains), null, constant),
					UFilterStartsWithAttribute => Expression.Call(target, nameof(string.StartsWith), null, constant),
					UFilterEndsWithAttribute => Expression.Call(target, nameof(string.EndsWith), null, constant),
					UFilterEqualAttribute => Expression.Equal(target, constant),
					UFilterNotEqualAttribute => Expression.NotEqual(target, constant),
					UFilterGreaterThanAttribute => Expression.GreaterThan(target, constant),
					UFilterGreaterThanOrEqualAttribute => Expression.GreaterThanOrEqual(target, constant),
					UFilterLessThanAttribute => Expression.LessThan(target, constant),
					UFilterLessThanOrEqualAttribute => Expression.LessThanOrEqual(target, constant),
					UFilterIsTrueAttribute => Expression.Equal(target, Expression.Constant(true)),
					UFilterIsFalseAttribute => Expression.Equal(target, Expression.Constant(false)),
					UFilterDateAfterAttribute => Expression.GreaterThan(target, constant),
					UFilterDateBeforeAttribute => Expression.LessThan(target, constant),
					UFilterInAttribute => BuildContainsExpression(target, value),
					UFilterNotInAttribute => Expression.Not(BuildContainsExpression(target, value)),
					UFilterHasAnyAttribute => BuildHasAnyExpression(target, value),
					UFilterHasAllAttribute => BuildHasAllExpression(target, value),
					UFilterOverlapsAttribute => BuildOverlapExpression(target, value),
					UFilterIsNullAttribute => Expression.Equal(target, Expression.Constant(null)),
					UFilterIsNotNullAttribute => Expression.NotEqual(target, Expression.Constant(null)),
					UFilterLengthEqualAttribute => Expression.Equal(Expression.Property(target, "Length"), constant),
					UFilterLengthGreaterAttribute => Expression.GreaterThan(Expression.Property(target, "Length"), constant),
					UFilterLengthLessAttribute => Expression.LessThan(Expression.Property(target, "Length"), constant),
					_ => null
				};
				if (predicate == null) continue;
				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
				query = query.Where(lambda);
			}

			if (prop.GetCustomAttribute<UFilterBetweenAttribute>() is not { } betweenAttr || value is not IList { Count: 2 } range) continue;
			{
				MemberExpression target = BuildNestedProperty(parameter, betweenAttr.TargetProperty);
				ConstantExpression from = Expression.Constant(range[0]);
				ConstantExpression to = Expression.Constant(range[1]);
				BinaryExpression ge = Expression.GreaterThanOrEqual(target, from);
				BinaryExpression le = Expression.LessThanOrEqual(target, to);
				BinaryExpression combined = Expression.AndAlso(ge, le);
				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
				query = query.Where(lambda);
			}
		}

		return query;
	}

	public static IQueryable<TEntity> ApplyIncludes<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) where TEntity : class {
		PropertyInfo[] props = PropertyCache.GetOrAdd(typeof(TParams), t => t.GetProperties());
		foreach (PropertyInfo prop in props) {
			if (prop.GetCustomAttribute<UIncludeAttribute>() is { } attr && prop.PropertyType == typeof(bool) && (bool)(prop.GetValue(param) ?? false))
				query = query.Include(attr.NavigationProperty);
		}

		return query;
	}

	public static IQueryable<TEntity> ApplySorting<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) {
		PropertyInfo[] props = PropertyCache.GetOrAdd(typeof(TParams), t => t.GetProperties());
		foreach (PropertyInfo prop in props) {
			if (prop.GetCustomAttribute<USortAttribute>() is not { } attr) continue;
			if (prop.GetValue(param) is not true) continue;
			ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
			MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
			LambdaExpression lambda = Expression.Lambda(target, parameter);
			string methodName = attr.Descending ? "OrderByDescending" : "OrderBy";
			MethodInfo method = typeof(Queryable).GetMethods().First(m => m.Name == methodName && m.GetParameters().Length == 2).MakeGenericMethod(typeof(TEntity), target.Type);
			query = (IQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
		}

		return query;
	}

	private static Expression BuildContainsExpression(Expression target, object value) {
		MethodInfo method = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(target.Type);
		return Expression.Call(method, Expression.Constant(value), target);
	}

	private static Expression BuildHasAnyExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
		ParameterExpression param = Expression.Parameter(elementType, "e");
		MethodInfo contains = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(elementType);
		MethodCallExpression call = Expression.Call(contains, Expression.Constant(value), param);
		LambdaExpression lambda = Expression.Lambda(call, param);
		return Expression.Call(typeof(Enumerable), "Any", [elementType], target, lambda);
	}

	private static Expression BuildHasAllExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
		ParameterExpression param = Expression.Parameter(elementType, "e");
		MethodInfo contains = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(target.Type);
		MethodCallExpression call = Expression.Call(contains, target, param);
		LambdaExpression lambda = Expression.Lambda(call, param);
		return Expression.Call(typeof(Enumerable), "All", [elementType], Expression.Constant(value), lambda);
	}

	private static Expression BuildOverlapExpression(Expression target, object value) {
		Type elementType = target.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
		ParameterExpression param = Expression.Parameter(elementType, "e");
		MethodInfo contains = typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(elementType);
		MethodCallExpression call = Expression.Call(contains, target, param);
		LambdaExpression lambda = Expression.Lambda(call, param);
		return Expression.Call(typeof(Enumerable), "Any", [elementType], Expression.Constant(value), lambda);
	}

	private static MemberExpression BuildNestedProperty(Expression parameter, string propertyPath) {
		return propertyPath.Split('.').Aggregate((MemberExpression)parameter, Expression.Property);
	}
}