namespace SinaMN75U.Middlewares;

[AttributeUsage(AttributeTargets.Property)]
public abstract class UUpdateAttribute(string targetProperty) : Attribute {
	public string TargetProperty { get; } = targetProperty;
}

public class UUpdateAssignIfNotNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignIfNotNullOrEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignHashedAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignDateNowAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAddRangeIfNotExistAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateRemoveMatchingAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignNestedAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedHashedAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateCustomUpdateAttribute(string methodName) : Attribute {
	public string MethodName { get; } = methodName;
}

public class UUpdateAssignIfDifferentAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateToggleBoolAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAppendStringAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdatePrependStringAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignMaxAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignMinAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignDefaultIfNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateSetNullIfEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateTrimAssignAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateClearListIfEmptyAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAddUniqueStringsAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateReplaceListAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignFromMapAttribute(string targetProperty, string key) : UUpdateAttribute(targetProperty) {
	public string Key { get; } = key;
}

public class UUpdateDateNowIfNullAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignIfMatchAttribute(string targetProperty, object matchValue) : UUpdateAttribute(targetProperty) {
	public object MatchValue { get; } = matchValue;
}

public class UUpdateMergeDictionariesAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignParsedEnumAttribute(string targetProperty, Type enumType) : UUpdateAttribute(targetProperty) {
	public Type EnumType { get; } = enumType;
}

public class UUpdateAssignIfGreaterAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignIfLesserAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

public class UUpdateAssignIfValidEmailAttribute(string targetProperty) : UUpdateAttribute(targetProperty);

// New nested attributes with UUpdate prefix
public class UUpdateAssignNestedIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfNotEmptyAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfNotNullOrEmptyAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedHashedIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedHashedIfNotEmptyAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedHashedIfNotNullOrEmptyAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedDateNowIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAddRangeNestedIfNotExistIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateRemoveNestedMatchingIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfDifferentIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateToggleNestedBoolIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAppendNestedStringIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdatePrependNestedStringIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedMaxIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedMinIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedDefaultIfNullIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateSetNestedNullIfEmptyIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateTrimNestedAssignIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateClearNestedListIfEmptyIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAddNestedUniqueStringsIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateReplaceNestedListIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignFromNestedMapIfNotNullAttribute(string targetProperty, string nestedProperty, string key) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
	public string Key { get; } = key;
}

public class UUpdateDateNowNestedIfNullIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfMatchIfNotNullAttribute(string targetProperty, string nestedProperty, object matchValue) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
	public object MatchValue { get; } = matchValue;
}

public class UUpdateMergeNestedDictionariesIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedParsedEnumIfNotNullAttribute(string targetProperty, string nestedProperty, Type enumType) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
	public Type EnumType { get; } = enumType;
}

public class UUpdateAssignNestedIfGreaterIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfLesserIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

public class UUpdateAssignNestedIfValidEmailIfNotNullAttribute(string targetProperty, string nestedProperty) : UUpdateAttribute(targetProperty) {
	public string NestedProperty { get; } = nestedProperty;
}

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
					case UUpdateAssignIfNotNullAttribute:
						entityProp.SetValue(entity, value);
						break;

					case UUpdateAssignIfNotNullOrEmptyAttribute when value is string s && !string.IsNullOrWhiteSpace(s):
						entityProp.SetValue(entity, s);
						break;

					case UUpdateAssignHashedAttribute when value is string pw && !string.IsNullOrWhiteSpace(pw):
						entityProp.SetValue(entity, PasswordHasher.Hash(pw));
						break;

					case UUpdateAssignDateNowAttribute:
						entityProp.SetValue(entity, DateTime.UtcNow);
						break;

					case UUpdateDateNowIfNullAttribute:
						if (entityProp.GetValue(entity) == null)
							entityProp.SetValue(entity, DateTime.UtcNow);
						break;

					case UUpdateAddRangeIfNotExistAttribute when value is IEnumerable list1 && entityProp.GetValue(entity) is IList target1:
						foreach (object? item in list1)
							if (!target1.Contains(item))
								target1.Add(item);
						break;

					case UUpdateRemoveMatchingAttribute when value is IEnumerable list2 && entityProp.GetValue(entity) is IList target2:
						for (int i = target2.Count - 1; i >= 0; i--)
							if (list2.Cast<object>().Contains(target2[i]))
								target2.RemoveAt(i);
						break;

					case UUpdateAssignNestedAttribute a:
						object? nest = entityProp.GetValue(entity);
						PropertyInfo? nestProp = nest?.GetType().GetProperty(a.NestedProperty);
						if (nestProp?.CanWrite == true)
							nestProp.SetValue(nest, value);
						break;

					case UUpdateAssignNestedHashedAttribute h:
						if (value is string pass) {
							object? obj = entityProp.GetValue(entity);
							PropertyInfo? prop = obj?.GetType().GetProperty(h.NestedProperty);
							if (prop?.CanWrite == true)
								prop.SetValue(obj, PasswordHasher.Hash(pass));
						}

						break;

					case UUpdateAssignIfDifferentAttribute:
						if (!Equals(entityProp.GetValue(entity), value))
							entityProp.SetValue(entity, value);
						break;

					case UUpdateToggleBoolAttribute:
						if (entityProp.PropertyType == typeof(bool))
							entityProp.SetValue(entity, !(bool)entityProp.GetValue(entity)!);
						break;

					case UUpdateAppendStringAttribute when value is string strA:
						entityProp.SetValue(entity, (entityProp.GetValue(entity) as string ?? "") + strA);
						break;

					case UUpdatePrependStringAttribute when value is string strP:
						entityProp.SetValue(entity, strP + (entityProp.GetValue(entity) as string ?? ""));
						break;

					case UUpdateAssignIfGreaterAttribute when value is IComparable nv1 && entityProp.GetValue(entity) is IComparable ov1 && nv1.CompareTo(ov1) > 0:
					case UUpdateAssignIfLesserAttribute when value is IComparable nv2 && entityProp.GetValue(entity) is IComparable ov2 && nv2.CompareTo(ov2) < 0:
						entityProp.SetValue(entity, value);
						break;

					case UUpdateTrimAssignAttribute when value is string ts:
						entityProp.SetValue(entity, ts.Trim());
						break;

					case UUpdateAssignIfValidEmailAttribute when value is string email && email.Contains("@"):
						entityProp.SetValue(entity, email);
						break;

					case UUpdateAssignDefaultIfNullAttribute:
						if (entityProp.GetValue(entity) == null)
							entityProp.SetValue(entity, value);
						break;

					case UUpdateSetNullIfEmptyAttribute when value is string empty && string.IsNullOrWhiteSpace(empty):
						entityProp.SetValue(entity, null);
						break;

					case UUpdateClearListIfEmptyAttribute when value is IEnumerable e && !e.Cast<object>().Any():
						if (entityProp.GetValue(entity) is IList list)
							list.Clear();
						break;

					case UUpdateAddUniqueStringsAttribute when value is IEnumerable<string> newItems && entityProp.GetValue(entity) is IList<string> existingList:
						foreach (string item in newItems)
							if (!existingList.Contains(item))
								existingList.Add(item);
						break;

					case UUpdateReplaceListAttribute when value is IEnumerable newList:
						if (entityProp.GetValue(entity) is IList listToReplace) {
							listToReplace.Clear();
							foreach (object? item in newList)
								listToReplace.Add(item);
						}

						break;

					case UUpdateAssignFromMapAttribute mapAttr when value is IDictionary dict && dict.Contains(mapAttr.Key):
						entityProp.SetValue(entity, dict[mapAttr.Key]);
						break;

					case UUpdateAssignIfMatchAttribute matchAttr:
						if (Equals(value, matchAttr.MatchValue))
							entityProp.SetValue(entity, value);
						break;

					case UUpdateMergeDictionariesAttribute when value is IDictionary src && entityProp.GetValue(entity) is IDictionary dest:
						foreach (object? key in src.Keys)
							dest[key] = src[key];
						break;

					case UUpdateAssignParsedEnumAttribute enumAttr when value is string enumStr:
						object parsed = Enum.Parse(enumAttr.EnumType, enumStr, ignoreCase: true);
						entityProp.SetValue(entity, parsed);
						break;

					case UUpdateAssignNestedIfNotNullAttribute a when value != null:
						object? nestNotNull = entityProp.GetValue(entity);
						PropertyInfo? nestPropNotNull = nestNotNull?.GetType().GetProperty(a.NestedProperty);
						if (nestPropNotNull?.CanWrite == true)
							nestPropNotNull.SetValue(nestNotNull, value);
						break;

					case UUpdateAssignNestedIfNotEmptyAttribute a when value is string s && !string.IsNullOrWhiteSpace(s):
						object? nestNotEmpty = entityProp.GetValue(entity);
						PropertyInfo? nestPropNotEmpty = nestNotEmpty?.GetType().GetProperty(a.NestedProperty);
						if (nestPropNotEmpty?.CanWrite == true)
							nestPropNotEmpty.SetValue(nestNotEmpty, s);
						break;

					case UUpdateAssignNestedIfNotNullOrEmptyAttribute a when value is string str && !string.IsNullOrEmpty(str):
						object? nestNotNullOrEmpty = entityProp.GetValue(entity);
						PropertyInfo? nestPropNotNullOrEmpty = nestNotNullOrEmpty?.GetType().GetProperty(a.NestedProperty);
						if (nestPropNotNullOrEmpty?.CanWrite == true)
							nestPropNotNullOrEmpty.SetValue(nestNotNullOrEmpty, str);
						break;

					case UUpdateAssignNestedHashedIfNotNullAttribute h when value != null && value is string pass0:
						object? objNotNull = entityProp.GetValue(entity);
						PropertyInfo? propNotNull = objNotNull?.GetType().GetProperty(h.NestedProperty);
						if (propNotNull?.CanWrite == true)
							propNotNull.SetValue(objNotNull, PasswordHasher.Hash(pass0));
						break;

					case UUpdateAssignNestedHashedIfNotEmptyAttribute h when value is string pass34 && !string.IsNullOrWhiteSpace(pass34):
						object? objNotEmpty = entityProp.GetValue(entity);
						PropertyInfo? propNotEmpty = objNotEmpty?.GetType().GetProperty(h.NestedProperty);
						if (propNotEmpty?.CanWrite == true)
							propNotEmpty.SetValue(objNotEmpty, PasswordHasher.Hash(pass34));
						break;

					case UUpdateAssignNestedHashedIfNotNullOrEmptyAttribute h when value is string pass4 && !string.IsNullOrEmpty(pass4):
						object? objNotNullOrEmpty = entityProp.GetValue(entity);
						PropertyInfo? propNotNullOrEmpty = objNotNullOrEmpty?.GetType().GetProperty(h.NestedProperty);
						if (propNotNullOrEmpty?.CanWrite == true)
							propNotNullOrEmpty.SetValue(objNotNullOrEmpty, PasswordHasher.Hash(pass4));
						break;

					case UUpdateAssignNestedDateNowIfNotNullAttribute a when value != null:
						object? nestDateNow = entityProp.GetValue(entity);
						PropertyInfo? nestPropDateNow = nestDateNow?.GetType().GetProperty(a.NestedProperty);
						if (nestPropDateNow?.CanWrite == true)
							nestPropDateNow.SetValue(nestDateNow, DateTime.UtcNow);
						break;

					case UUpdateAddRangeNestedIfNotExistIfNotNullAttribute a when value != null && value is IEnumerable list1:
						object? nestRange = entityProp.GetValue(entity);
						PropertyInfo? nestPropRange = nestRange?.GetType().GetProperty(a.NestedProperty);
						if (nestPropRange?.CanWrite == true && nestPropRange.GetValue(nestRange) is IList target8) {
							foreach (object? item in list1)
								if (!target8.Contains(item))
									target8.Add(item);
						}

						break;

					case UUpdateRemoveNestedMatchingIfNotNullAttribute a when value != null && value is IEnumerable list2:
						object? nestRemove = entityProp.GetValue(entity);
						PropertyInfo? nestPropRemove = nestRemove?.GetType().GetProperty(a.NestedProperty);
						if (nestPropRemove?.CanWrite == true && nestPropRemove.GetValue(nestRemove) is IList target7) {
							for (int i = target7.Count - 1; i >= 0; i--)
								if (list2.Cast<object>().Contains(target7[i]))
									target7.RemoveAt(i);
						}

						break;

					case UUpdateAssignNestedIfDifferentIfNotNullAttribute a when value != null:
						object? nestDiff = entityProp.GetValue(entity);
						PropertyInfo? nestPropDiff = nestDiff?.GetType().GetProperty(a.NestedProperty);
						if (nestPropDiff?.CanWrite == true && !Equals(nestPropDiff.GetValue(nestDiff), value))
							nestPropDiff.SetValue(nestDiff, value);
						break;

					case UUpdateToggleNestedBoolIfNotNullAttribute a when value != null:
						object? nestToggle = entityProp.GetValue(entity);
						PropertyInfo? nestPropToggle = nestToggle?.GetType().GetProperty(a.NestedProperty);
						if (nestPropToggle?.CanWrite == true && nestPropToggle.PropertyType == typeof(bool))
							nestPropToggle.SetValue(nestToggle, !(bool)nestPropToggle.GetValue(nestToggle)!);
						break;

					case UUpdateAppendNestedStringIfNotNullAttribute a when value != null && value is string strA:
						object? nestAppend = entityProp.GetValue(entity);
						PropertyInfo? nestPropAppend = nestAppend?.GetType().GetProperty(a.NestedProperty);
						if (nestPropAppend?.CanWrite == true)
							nestPropAppend.SetValue(nestAppend, (nestPropAppend.GetValue(nestAppend) as string ?? "") + strA);
						break;

					case UUpdatePrependNestedStringIfNotNullAttribute a when value != null && value is string strP:
						object? nestPrepend = entityProp.GetValue(entity);
						PropertyInfo? nestPropPrepend = nestPrepend?.GetType().GetProperty(a.NestedProperty);
						if (nestPropPrepend?.CanWrite == true)
							nestPropPrepend.SetValue(nestPrepend, strP + (nestPropPrepend.GetValue(nestPrepend) as string ?? ""));
						break;

					case UUpdateAssignNestedMaxIfNotNullAttribute a when value != null:
						object? nestMax = entityProp.GetValue(entity);
						PropertyInfo? nestPropMax = nestMax?.GetType().GetProperty(a.NestedProperty);
						if (nestPropMax?.CanWrite == true && value is IComparable nv9 && nestPropMax.GetValue(nestMax) is IComparable ov9 && nv9.CompareTo(ov9) > 0)
							nestPropMax.SetValue(nestMax, value);
						break;

					case UUpdateAssignNestedMinIfNotNullAttribute a when value != null:
						object? nestMin = entityProp.GetValue(entity);
						PropertyInfo? nestPropMin = nestMin?.GetType().GetProperty(a.NestedProperty);
						if (nestPropMin?.CanWrite == true && value is IComparable nv5 && nestPropMin.GetValue(nestMin) is IComparable ov8 && nv5.CompareTo(ov8) < 0)
							nestPropMin.SetValue(nestMin, value);
						break;

					case UUpdateAssignNestedDefaultIfNullIfNotNullAttribute a when value != null:
						object? nestDefault = entityProp.GetValue(entity);
						PropertyInfo? nestPropDefault = nestDefault?.GetType().GetProperty(a.NestedProperty);
						if (nestPropDefault?.CanWrite == true && nestPropDefault.GetValue(nestDefault) == null)
							nestPropDefault.SetValue(nestDefault, value);
						break;

					case UUpdateSetNestedNullIfEmptyIfNotNullAttribute a when value != null && value is string empty && string.IsNullOrWhiteSpace(empty):
						object? nestNull = entityProp.GetValue(entity);
						PropertyInfo? nestPropNull = nestNull?.GetType().GetProperty(a.NestedProperty);
						if (nestPropNull?.CanWrite == true)
							nestPropNull.SetValue(nestNull, null);
						break;

					case UUpdateTrimNestedAssignIfNotNullAttribute a when value != null && value is string ts:
						object? nestTrim = entityProp.GetValue(entity);
						PropertyInfo? nestPropTrim = nestTrim?.GetType().GetProperty(a.NestedProperty);
						if (nestPropTrim?.CanWrite == true)
							nestPropTrim.SetValue(nestTrim, ts.Trim());
						break;

					case UUpdateClearNestedListIfEmptyIfNotNullAttribute a when value != null && value is IEnumerable e && !e.Cast<object>().Any():
						object? nestClear = entityProp.GetValue(entity);
						PropertyInfo? nestPropClear = nestClear?.GetType().GetProperty(a.NestedProperty);
						if (nestPropClear?.CanWrite == true && nestPropClear.GetValue(nestClear) is IList nestedList)
							nestedList.Clear();
						break;

					case UUpdateAddNestedUniqueStringsIfNotNullAttribute a when value != null && value is IEnumerable<string> newItems:
						object? nestUnique = entityProp.GetValue(entity);
						PropertyInfo? nestPropUnique = nestUnique?.GetType().GetProperty(a.NestedProperty);
						if (nestPropUnique?.CanWrite == true && nestPropUnique.GetValue(nestUnique) is IList<string> nestedExistingList) {
							foreach (string item in newItems)
								if (!nestedExistingList.Contains(item))
									nestedExistingList.Add(item);
						}

						break;

					case UUpdateReplaceNestedListIfNotNullAttribute a when value != null && value is IEnumerable newList:
						object? nestReplace = entityProp.GetValue(entity);
						PropertyInfo? nestPropReplace = nestReplace?.GetType().GetProperty(a.NestedProperty);
						if (nestPropReplace?.CanWrite == true && nestPropReplace.GetValue(nestReplace) is IList nestedListToReplace) {
							nestedListToReplace.Clear();
							foreach (object? item in newList)
								nestedListToReplace.Add(item);
						}

						break;

					case UUpdateAssignFromNestedMapIfNotNullAttribute mapAttr when value != null && value is IDictionary dict && dict.Contains(mapAttr.Key):
						object? nestMap = entityProp.GetValue(entity);
						PropertyInfo? nestPropMap = nestMap?.GetType().GetProperty(mapAttr.NestedProperty);
						if (nestPropMap?.CanWrite == true)
							nestPropMap.SetValue(nestMap, dict[mapAttr.Key]);
						break;

					case UUpdateAssignNestedIfMatchIfNotNullAttribute matchAttr when value != null:
						object? nestMatch = entityProp.GetValue(entity);
						PropertyInfo? nestPropMatch = nestMatch?.GetType().GetProperty(matchAttr.NestedProperty);
						if (nestPropMatch?.CanWrite == true && Equals(value, matchAttr.MatchValue))
							nestPropMatch.SetValue(nestMatch, value);
						break;

					case UUpdateMergeNestedDictionariesIfNotNullAttribute a when value != null && value is IDictionary src:
						object? nestMerge = entityProp.GetValue(entity);
						PropertyInfo? nestPropMerge = nestMerge?.GetType().GetProperty(a.NestedProperty);
						if (nestPropMerge?.CanWrite == true && nestPropMerge.GetValue(nestMerge) is IDictionary _dest) {
							foreach (object? key in src.Keys)
								_dest[key] = src[key];
						}

						break;

					case UUpdateAssignNestedParsedEnumIfNotNullAttribute enumAttr when value != null && value is string enumStr:
						object? nestEnum = entityProp.GetValue(entity);
						PropertyInfo? nestPropEnum = nestEnum?.GetType().GetProperty(enumAttr.NestedProperty);
						if (nestPropEnum?.CanWrite == true) {
							object _parsed = Enum.Parse(enumAttr.EnumType, enumStr, ignoreCase: true);
							nestPropEnum.SetValue(nestEnum, _parsed);
						}

						break;

					case UUpdateAssignNestedIfGreaterIfNotNullAttribute a when value != null && value is IComparable _nv1:
						object? nestGreater = entityProp.GetValue(entity);
						PropertyInfo? nestPropGreater = nestGreater?.GetType().GetProperty(a.NestedProperty);
						if (nestPropGreater?.CanWrite == true && nestPropGreater.GetValue(nestGreater) is IComparable _ov1 && _nv1.CompareTo(_ov1) > 0)
							nestPropGreater.SetValue(nestGreater, value);
						break;

					case UUpdateAssignNestedIfLesserIfNotNullAttribute a when value != null && value is IComparable _nv2:
						object? nestLesser = entityProp.GetValue(entity);
						PropertyInfo? nestPropLesser = nestLesser?.GetType().GetProperty(a.NestedProperty);
						if (nestPropLesser?.CanWrite == true && nestPropLesser.GetValue(nestLesser) is IComparable _ov2 && _nv2.CompareTo(_ov2) < 0)
							nestPropLesser.SetValue(nestLesser, value);
						break;

					case UUpdateAssignNestedIfValidEmailIfNotNullAttribute a when value != null && value is string email && email.Contains("@"):
						object? nestEmail = entityProp.GetValue(entity);
						PropertyInfo? nestPropEmail = nestEmail?.GetType().GetProperty(a.NestedProperty);
						if (nestPropEmail?.CanWrite == true)
							nestPropEmail.SetValue(nestEmail, email);
						break;
				}
			}

			UUpdateCustomUpdateAttribute? customAttr = paramProp.GetCustomAttribute<UUpdateCustomUpdateAttribute>();
			if (customAttr == null) continue;
			MethodInfo? method = entityType.GetMethod(customAttr.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(entity, [value]);
		}
	}
}