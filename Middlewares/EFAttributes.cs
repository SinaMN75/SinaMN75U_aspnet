namespace SinaMN75U.Middlewares;

using System.Collections.Concurrent;

// [AttributeUsage(AttributeTargets.Property)]
// public abstract class UFilterAttribute(string targetProperty) : Attribute {
// 	public string TargetProperty { get; } = targetProperty;
// }
//
// public class UContainsAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UStartsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UEndsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UNotEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UGreaterThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UGreaterThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class ULessThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class ULessThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UBetweenAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UInAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UNotInAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UHasAnyAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UIsTrueAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UIsFalseAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UDateAfterAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// public class UDateBeforeAttribute(string targetProperty) : UFilterAttribute(targetProperty);
//
// [AttributeUsage(AttributeTargets.Property)]
// public class USortAttribute(string targetProperty, bool descending = false) : Attribute {
// 	public string TargetProperty { get; } = targetProperty;
// 	public bool Descending { get; } = descending;
// }
//
// [AttributeUsage(AttributeTargets.Property)]
// public class UIncludeAttribute(string navigationProperty) : Attribute {
// 	public string NavigationProperty { get; } = navigationProperty;
// }
//
// public static class UQueryableFilterExtensions {
// 	public static IQueryable<TEntity> ApplyFilters<TEntity, TParams>(this IQueryable<TEntity> query, TParams filterParams) {
// 		Type paramType = typeof(TParams);
// 		ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
//
// 		foreach (PropertyInfo prop in paramType.GetProperties()) {
// 			object? value = prop.GetValue(filterParams);
// 			if (value == null || value is string s && string.IsNullOrWhiteSpace(s)) continue;
//
// 			foreach (UFilterAttribute attr in prop.GetCustomAttributes<UFilterAttribute>()) {
// 				MemberExpression target = Expression.Property(parameter, attr.TargetProperty);
// 				ConstantExpression constant = Expression.Constant(value);
// 				Expression? predicate = attr switch {
// 					UContainsAttribute => Expression.Call(target, nameof(string.Contains), null, constant),
// 					UStartsWithAttribute => Expression.Call(target, nameof(string.StartsWith), null, constant),
// 					UEndsWithAttribute => Expression.Call(target, nameof(string.EndsWith), null, constant),
// 					UEqualAttribute => Expression.Equal(target, constant),
// 					UNotEqualAttribute => Expression.NotEqual(target, constant),
// 					UGreaterThanAttribute => Expression.GreaterThan(target, constant),
// 					UGreaterThanOrEqualAttribute => Expression.GreaterThanOrEqual(target, constant),
// 					ULessThanAttribute => Expression.LessThan(target, constant),
// 					ULessThanOrEqualAttribute => Expression.LessThanOrEqual(target, constant),
// 					UIsTrueAttribute => Expression.Equal(target, Expression.Constant(true)),
// 					UIsFalseAttribute => Expression.Equal(target, Expression.Constant(false)),
// 					UDateAfterAttribute => Expression.GreaterThan(target, constant),
// 					UDateBeforeAttribute => Expression.LessThan(target, constant),
// 					UInAttribute => BuildInExpression(target, value),
// 					UNotInAttribute => Expression.Not(BuildInExpression(target, value)),
// 					UHasAnyAttribute => BuildHasAnyExpression(target, value),
// 					_ => null
// 				};
//
// 				if (predicate == null) continue;
// 				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
// 				query = query.Where(lambda);
// 			}
//
// 			if (prop.GetCustomAttribute<UBetweenAttribute>() is not { } betweenAttr ||
// 			    value is not IList { Count: 2 } range) continue;
// 			{
// 				MemberExpression target = Expression.Property(parameter, betweenAttr.TargetProperty);
// 				ConstantExpression from = Expression.Constant(range[0]);
// 				ConstantExpression to = Expression.Constant(range[1]);
//
// 				BinaryExpression ge = Expression.GreaterThanOrEqual(target, from);
// 				BinaryExpression le = Expression.LessThanOrEqual(target, to);
// 				BinaryExpression combined = Expression.AndAlso(ge, le);
// 				Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
// 				query = query.Where(lambda);
// 			}
// 		}
//
// 		return query;
// 	}
//
// 	public static Expression BuildInExpression(MemberExpression target, object value) {
// 		ConstantExpression enumerable = Expression.Constant(value);
// 		MethodInfo containsMethod = typeof(Enumerable).GetMethods()
// 			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
// 			.MakeGenericMethod(target.Type);
// 		return Expression.Call(containsMethod, enumerable, target);
// 	}
//
// 	private static Expression BuildHasAnyExpression(MemberExpression target, object value) {
// 		Type elementType = target.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
// 		ParameterExpression param = Expression.Parameter(elementType, "e");
// 		MethodInfo containsMethod = typeof(Enumerable).GetMethods()
// 			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
// 			.MakeGenericMethod(elementType);
// 		ConstantExpression constant = Expression.Constant(value);
// 		MethodCallExpression call = Expression.Call(containsMethod, constant, param);
// 		LambdaExpression lambda = Expression.Lambda(call, param);
// 		return Expression.Call(typeof(Enumerable), "Any", new[] { elementType }, target, lambda);
// 	}
//
// 	public static IQueryable<TEntity> ApplyIncludes<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) where TEntity : class {
// 		Type paramType = typeof(TParams);
// 		foreach (PropertyInfo prop in paramType.GetProperties()) {
// 			UIncludeAttribute? attr = prop.GetCustomAttribute<UIncludeAttribute>();
// 			if (attr != null && prop.PropertyType == typeof(bool) && (bool)(prop.GetValue(param) ?? false)) {
// 				query = query.Include(attr.NavigationProperty);
// 			}
// 		}
//
// 		return query;
// 	}
//
// 	public static IQueryable<TEntity> ApplySorting<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) {
// 		IEnumerable<(PropertyInfo Property, USortAttribute? Attr)> props = typeof(TParams).GetProperties()
// 			.Select(p => (Property: p, Attr: p.GetCustomAttribute<USortAttribute>()))
// 			.Where(t => t.Attr != null && (bool)(t.Property.GetValue(param) ?? false));
//
// 		foreach ((PropertyInfo prop, USortAttribute? attr) in props) {
// 			ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
// 			MemberExpression target = Expression.Property(parameter, attr!.TargetProperty);
// 			LambdaExpression lambda = Expression.Lambda(target, parameter);
// 			string methodName = attr.Descending ? "OrderByDescending" : "OrderBy";
// 			MethodInfo method = typeof(Queryable).GetMethods()
// 				.First(m => m.Name == methodName && m.GetParameters().Length == 2)
// 				.MakeGenericMethod(typeof(TEntity), target.Type);
// 			query = (IQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
// 		}
//
// 		return query;
// 	}
// }

[AttributeUsage(AttributeTargets.Property)]
public abstract class UFilterAttribute(string targetProperty) : Attribute {
	public string TargetProperty { get; } = targetProperty;
}

public class UContainsAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UStartsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UEndsWithAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UNotEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UGreaterThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UGreaterThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class ULessThanAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class ULessThanOrEqualAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UBetweenAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UInAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UNotInAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UHasAnyAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UIsTrueAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UIsFalseAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UDateAfterAttribute(string targetProperty) : UFilterAttribute(targetProperty);

public class UDateBeforeAttribute(string targetProperty) : UFilterAttribute(targetProperty);

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
	private static readonly ConcurrentDictionary<Type, List<(PropertyInfo Property, UFilterAttribute Attr)>> FilterCache = new();

	public static IQueryable<TEntity> ApplyFilters<TEntity, TParams>(this IQueryable<TEntity> query, TParams filterParams) {
		Type paramType = typeof(TParams);
		Type entityType = typeof(TEntity);
		ParameterExpression parameter = Expression.Parameter(entityType, "x");

		if (!FilterCache.TryGetValue(paramType, out List<(PropertyInfo Property, UFilterAttribute Attr)>? cachedFilters)) {
			cachedFilters = (from prop in paramType.GetProperties()
				from attr in prop.GetCustomAttributes<UFilterAttribute>()
				select (prop, attr)).ToList();
			FilterCache[paramType] = cachedFilters;
		}

		foreach ((PropertyInfo prop, UFilterAttribute attr) in cachedFilters) {
			object? value = prop.GetValue(filterParams);
			if (value == null || value is string s && string.IsNullOrWhiteSpace(s)) continue;

			MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
			ConstantExpression constant = Expression.Constant(value);
			Expression? predicate = attr switch {
				UContainsAttribute => Expression.Call(target, nameof(string.Contains), null, constant),
				UStartsWithAttribute => Expression.Call(target, nameof(string.StartsWith), null, constant),
				UEndsWithAttribute => Expression.Call(target, nameof(string.EndsWith), null, constant),
				UEqualAttribute => Expression.Equal(target, constant),
				UNotEqualAttribute => Expression.NotEqual(target, constant),
				UGreaterThanAttribute => Expression.GreaterThan(target, constant),
				UGreaterThanOrEqualAttribute => Expression.GreaterThanOrEqual(target, constant),
				ULessThanAttribute => Expression.LessThan(target, constant),
				ULessThanOrEqualAttribute => Expression.LessThanOrEqual(target, constant),
				UIsTrueAttribute => Expression.Equal(target, Expression.Constant(true)),
				UIsFalseAttribute => Expression.Equal(target, Expression.Constant(false)),
				UDateAfterAttribute => Expression.GreaterThan(target, constant),
				UDateBeforeAttribute => Expression.LessThan(target, constant),
				UInAttribute => BuildInExpression(target, value),
				UNotInAttribute => Expression.Not(BuildInExpression(target, value)),
				UHasAnyAttribute => BuildHasAnyExpression(target, value),
				_ => null
			};

			if (predicate == null) continue;
			Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
			query = query.Where(lambda);
		}

		foreach (PropertyInfo prop in paramType.GetProperties()) {
			UBetweenAttribute? attr = prop.GetCustomAttribute<UBetweenAttribute>();
			if (attr == null || prop.GetValue(filterParams) is not IList { Count: 2 } range) continue;

			MemberExpression target = BuildNestedProperty(parameter, attr.TargetProperty);
			ConstantExpression from = Expression.Constant(range[0]);
			ConstantExpression to = Expression.Constant(range[1]);
			BinaryExpression ge = Expression.GreaterThanOrEqual(target, from);
			BinaryExpression le = Expression.LessThanOrEqual(target, to);
			BinaryExpression between = Expression.AndAlso(ge, le);
			Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(between, parameter);
			query = query.Where(lambda);
		}

		return query;
	}

	private static MemberExpression BuildNestedProperty(Expression param, string propertyPath) {
		return propertyPath.Split('.')
			       .Aggregate((Expression)param, Expression.Property) as MemberExpression
		       ?? throw new InvalidOperationException($"Invalid property path: {propertyPath}");
	}

	private static Expression BuildInExpression(MemberExpression target, object value) {
		ConstantExpression enumerable = Expression.Constant(value);
		MethodInfo method = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(target.Type);
		return Expression.Call(method, enumerable, target);
	}

	private static Expression BuildHasAnyExpression(MemberExpression target, object value) {
		Type elementType = target.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
		ParameterExpression param = Expression.Parameter(elementType, "e");
		ConstantExpression constant = Expression.Constant(value);
		MethodInfo containsMethod = typeof(Enumerable).GetMethods()
			.First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
			.MakeGenericMethod(elementType);
		MethodCallExpression body = Expression.Call(containsMethod, constant, param);
		LambdaExpression lambda = Expression.Lambda(body, param);
		return Expression.Call(typeof(Enumerable), "Any", [elementType], target, lambda);
	}

	public static IQueryable<TEntity> ApplyIncludes<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) where TEntity : class {
		Type paramType = typeof(TParams);
		foreach (PropertyInfo prop in paramType.GetProperties()) {
			UIncludeAttribute? attr = prop.GetCustomAttribute<UIncludeAttribute>();
			if (attr != null && prop.PropertyType == typeof(bool) && (bool)(prop.GetValue(param) ?? false)) {
				query = query.Include(attr.NavigationProperty);
			}
		}

		return query;
	}

	public static IQueryable<TEntity> ApplySorting<TEntity, TParams>(this IQueryable<TEntity> query, TParams param) {
		IEnumerable<(PropertyInfo Property, USortAttribute? Attr)> props = typeof(TParams).GetProperties()
			.Select(p => (Property: p, Attr: p.GetCustomAttribute<USortAttribute>()))
			.Where(t => t.Attr != null && (bool)(t.Property.GetValue(param) ?? false));

		foreach ((PropertyInfo _, USortAttribute? attr) in props) {
			ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
			MemberExpression target = BuildNestedProperty(parameter, attr!.TargetProperty);
			LambdaExpression lambda = Expression.Lambda(target, parameter);
			string methodName = attr.Descending ? "OrderByDescending" : "OrderBy";
			MethodInfo method = typeof(Queryable).GetMethods()
				.First(m => m.Name == methodName && m.GetParameters().Length == 2)
				.MakeGenericMethod(typeof(TEntity), target.Type);
			query = (IQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
		}

		return query;
	}
}