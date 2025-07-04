namespace SinaMN75U.Middlewares;

[AttributeUsage(AttributeTargets.Property)]
public abstract class UUpdateAttribute(string targetProperty) : Attribute {
    public string TargetProperty { get; } = targetProperty;
}

public class UAssignIfNotNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignIfNotEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignHashedAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignDateNowAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAddRangeIfNotExistAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class URemoveMatchingAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignNestedAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
    public string NestedProperty { get; } = nestedProperty;
}
public class UAssignNestedHashedAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
    public string NestedProperty { get; } = nestedProperty;
}
public class UCustomUpdateAttribute(string methodName) : Attribute {
    public string MethodName { get; } = methodName;
}
public class UAssignIfDifferentAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UToggleBoolAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAppendStringAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UPrependStringAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignMaxAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignMinAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignDefaultIfNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class USetNullIfEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UTrimAssignAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UClearListIfEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAddUniqueStringsAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UReplaceListAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignFromMapAttribute(string targetProperty, string key) : UUpdateAttribute(targetProperty) {
    public string Key { get; } = key;
}
public class UDateNowIfNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignIfMatchAttribute(string targetProperty, object matchValue) : UUpdateAttribute(targetProperty) {
    public object MatchValue { get; } = matchValue;
}
public class UMergeDictionariesAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignParsedEnumAttribute(string targetProperty, Type enumType) : UUpdateAttribute(targetProperty) {
    public Type EnumType { get; } = enumType;
}
public class UAssignIfGreaterAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignIfLesserAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }
public class UAssignIfValidEmailAttribute(string targetProperty) : UUpdateAttribute(targetProperty) { }

public static class UUpdateExtensions {
    public static void ApplyUpdates<TParams, TEntity>(this TEntity entity, TParams updateParams) {
        Type paramType = typeof(TParams);
        Type entityType = typeof(TEntity);

        foreach (PropertyInfo paramProp in paramType.GetProperties()) {
            object? value = paramProp.GetValue(updateParams);
            if (value is null) continue;

            foreach (UUpdateAttribute attr in paramProp.GetCustomAttributes<UUpdateAttribute>()) {
                PropertyInfo? entityProp = entityType.GetProperty(attr.TargetProperty);
                if (entityProp == null || !entityProp.CanWrite) continue;

                switch (attr) {
                    case UAssignIfNotNullAttribute:
                        entityProp.SetValue(entity, value);
                        break;

                    case UAssignIfNotEmptyAttribute when value is string s && !string.IsNullOrWhiteSpace(s):
                        entityProp.SetValue(entity, s);
                        break;

                    case UAssignHashedAttribute when value is string pw && !string.IsNullOrWhiteSpace(pw):
                        entityProp.SetValue(entity, PasswordHasher.Hash(pw));
                        break;

                    case UAssignDateNowAttribute:
                        entityProp.SetValue(entity, DateTime.UtcNow);
                        break;

                    case UDateNowIfNullAttribute:
                        if (entityProp.GetValue(entity) == null)
                            entityProp.SetValue(entity, DateTime.UtcNow);
                        break;

                    case UAddRangeIfNotExistAttribute when value is IEnumerable list1 && entityProp.GetValue(entity) is IList target1:
                        foreach (object? item in list1)
                            if (!target1.Contains(item))
                                target1.Add(item);
                        break;

                    case URemoveMatchingAttribute when value is IEnumerable list2 && entityProp.GetValue(entity) is IList target2:
                        for (int i = target2.Count - 1; i >= 0; i--)
                            if (list2.Cast<object>().Contains(target2[i]))
                                target2.RemoveAt(i);
                        break;

                    case UAssignNestedAttribute a:
                        object? nest = entityProp.GetValue(entity);
                        PropertyInfo? nestProp = nest?.GetType().GetProperty(a.NestedProperty);
                        if (nestProp?.CanWrite == true)
                            nestProp.SetValue(nest, value);
                        break;

                    case UAssignNestedHashedAttribute h:
                        if (value is string pass) {
                            object? obj = entityProp.GetValue(entity);
                            PropertyInfo? prop = obj?.GetType().GetProperty(h.NestedProperty);
                            if (prop?.CanWrite == true)
                                prop.SetValue(obj, PasswordHasher.Hash(pass));
                        }
                        break;

                    case UAssignIfDifferentAttribute:
                        if (!Equals(entityProp.GetValue(entity), value))
                            entityProp.SetValue(entity, value);
                        break;

                    case UToggleBoolAttribute:
                        if (entityProp.PropertyType == typeof(bool))
                            entityProp.SetValue(entity, !(bool)entityProp.GetValue(entity)!);
                        break;

                    case UAppendStringAttribute when value is string strA:
                        entityProp.SetValue(entity, (entityProp.GetValue(entity) as string ?? "") + strA);
                        break;

                    case UPrependStringAttribute when value is string strP:
                        entityProp.SetValue(entity, strP + (entityProp.GetValue(entity) as string ?? ""));
                        break;

                    case UAssignIfGreaterAttribute when value is IComparable nv1 && entityProp.GetValue(entity) is IComparable ov1 && nv1.CompareTo(ov1) > 0:
                    case UAssignIfLesserAttribute when value is IComparable nv2 && entityProp.GetValue(entity) is IComparable ov2 && nv2.CompareTo(ov2) < 0:
                        entityProp.SetValue(entity, value);
                        break;

                    case UTrimAssignAttribute when value is string ts:
                        entityProp.SetValue(entity, ts.Trim());
                        break;

                    case UAssignIfValidEmailAttribute when value is string email && email.Contains("@"):
                        entityProp.SetValue(entity, email);
                        break;

                    case UAssignDefaultIfNullAttribute:
                        if (entityProp.GetValue(entity) == null)
                            entityProp.SetValue(entity, value);
                        break;

                    case USetNullIfEmptyAttribute when value is string empty && string.IsNullOrWhiteSpace(empty):
                        entityProp.SetValue(entity, null);
                        break;

                    case UClearListIfEmptyAttribute when value is IEnumerable e && !e.Cast<object>().Any():
                        if (entityProp.GetValue(entity) is IList list)
                            list.Clear();
                        break;

                    case UAddUniqueStringsAttribute when value is IEnumerable<string> newItems && entityProp.GetValue(entity) is IList<string> existingList:
                        foreach (string item in newItems)
                            if (!existingList.Contains(item))
                                existingList.Add(item);
                        break;

                    case UReplaceListAttribute when value is IEnumerable newList:
                        if (entityProp.GetValue(entity) is IList listToReplace) {
                            listToReplace.Clear();
                            foreach (object? item in newList)
                                listToReplace.Add(item);
                        }
                        break;

                    case UAssignFromMapAttribute mapAttr when value is IDictionary dict && dict.Contains(mapAttr.Key):
                        entityProp.SetValue(entity, dict[mapAttr.Key]);
                        break;

                    case UAssignIfMatchAttribute matchAttr:
                        if (Equals(value, matchAttr.MatchValue))
                            entityProp.SetValue(entity, value);
                        break;

                    case UMergeDictionariesAttribute when value is IDictionary src && entityProp.GetValue(entity) is IDictionary dest:
                        foreach (object? key in src.Keys)
                            dest[key] = src[key];
                        break;

                    case UAssignParsedEnumAttribute enumAttr when value is string enumStr:
                        object parsed = Enum.Parse(enumAttr.EnumType, enumStr, ignoreCase: true);
                        entityProp.SetValue(entity, parsed);
                        break;
                }
            }

            UCustomUpdateAttribute? customAttr = paramProp.GetCustomAttribute<UCustomUpdateAttribute>();
            if (customAttr == null) continue;
            MethodInfo? method = entityType.GetMethod(customAttr.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(entity, [value]);
        }
    }
}

// [UAssignIfNotEmpty(nameof(UserEntity.UserName))]
// public string? UserName { get; set; }
//
// [UAssignIfNotEmpty(nameof(UserEntity.Email))]
// public string? Email { get; set; }
//
// [UAssignIfNotEmpty(nameof(UserEntity.PhoneNumber))]
// public string? PhoneNumber { get; set; }
//
// [UAssignHashed(nameof(UserEntity.Password))]
// public string? Password { get; set; }
//
// [UAssignDateNow(nameof(UserEntity.UpdatedAt))]
// public bool SetUpdatedNow { get; set; } = true;
//
// [UAssignNested(nameof(UserEntity.JsonData), nameof(JsonData.FcmToken))]
// public string? FcmToken { get; set; }
//
// [UAssignNested(nameof(UserEntity.JsonData), nameof(JsonData.FatherName))]
// public string? FatherName { get; set; }
//
// [UAddRangeIfNotExist(nameof(UserEntity.Tags))]
// public List<string>? AddTags { get; set; }
//
// [URemoveMatching(nameof(UserEntity.Tags))]
// public List<string>? RemoveTags { get; set; }
//
// [UAddRangeIfNotExistNested(nameof(UserEntity.JsonData), nameof(JsonData.HealthIssues))]
// public List<string>? AddHealthIssues { get; set; }
//
// [URemoveMatchingNested(nameof(UserEntity.JsonData), nameof(JsonData.HealthIssues))]
// public List<string>? RemoveHealthIssues { get; set; }
//
// [UCustomUpdate(nameof(UserEntity.ReplaceCategories))]
// public List<CategoryEntity>? Categories { get; set; }

// UserEntity? entity = await dbContext.Set<UserEntity>()
// 	.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
//
// if (entity == null)
// 	return new UResponse<UserEntity?>(null, Usc.NotFound);
//
// entity.ApplyUpdates(p);
//
// dbContext.Update(entity);
// await dbContext.SaveChangesAsync(ct);
//
// return new UResponse<UserEntity?>(entity);