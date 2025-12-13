namespace SinaMN75U.Data;

public sealed class DartModelGenerator {
	private readonly string _paramsNs;
	private readonly string _responsesNs;

	private readonly string _paramsOut;
	private readonly string _responsesOut;

	public DartModelGenerator(
		string paramsNs,
		string responsesNs,
		string paramsOut,
		string responsesOut) {
		_paramsNs = paramsNs;
		_responsesNs = responsesNs;
		_paramsOut = paramsOut;
		_responsesOut = responsesOut;
	}

	public int Generate(Assembly assembly) {
		int count = 0;

		count += GenerateGroup(assembly, _paramsNs, _paramsOut);
		count += GenerateGroup(assembly, _responsesNs, _responsesOut);

		return count;
	}

	private int GenerateGroup(
		Assembly assembly,
		string targetNamespace,
		string outputRoot) {
		IEnumerable<Type> types = assembly.GetTypes()
			.Where(t =>
				t.IsClass &&
				t.Namespace == targetNamespace &&
				!t.IsAbstract);

		Directory.CreateDirectory(outputRoot);

		int written = 0;

		foreach (Type type in types) {
			string dart = GenerateClass(type);
			string file = Path.Combine(outputRoot, ToSnake(type.Name) + ".dart");
			File.WriteAllText(file, dart);
			written++;
		}

		return written;
	}

	// ---------------- Dart ----------------

	private string GenerateClass(Type type) {
		StringBuilder sb = new StringBuilder();

		sb.AppendLine(@"part of ""../data.dart"";");
		sb.AppendLine();
		sb.AppendLine($"class {type.Name} {{");

		// constructor
		sb.AppendLine($"  {type.Name}({{");
		foreach (PropertyInfo p in Props(type))
			sb.AppendLine($"    this.{Camel(p.Name)},");
		sb.AppendLine("  });\n");

		// fromJson / fromMap
		sb.AppendLine($"  factory {type.Name}.fromJson(String str) =>");
		sb.AppendLine($"      {type.Name}.fromMap(json.decode(str));\n");

		sb.AppendLine($"  factory {type.Name}.fromMap(Map<String, dynamic> json) =>");
		sb.AppendLine($"      {type.Name}(");
		foreach (PropertyInfo p in Props(type))
			sb.AppendLine($"        {Camel(p.Name)}: {FromMap(p)},");
		sb.AppendLine("      );\n");

		// fields
		foreach (PropertyInfo p in Props(type))
			sb.AppendLine($"  final {DartType(p)} {Camel(p.Name)};");

		sb.AppendLine();
		sb.AppendLine("  String toJson() => json.encode(toMap());\n");

		sb.AppendLine("  Map<String, dynamic> toMap() => {");
		foreach (PropertyInfo p in Props(type))
			sb.AppendLine($"        \"{Camel(p.Name)}\": {ToMap(p)},");
		sb.AppendLine("      };");

		sb.AppendLine("}");
		return sb.ToString();
	}

	// ---------------- Helpers ----------------

	private static IEnumerable<PropertyInfo> Props(Type t) =>
		t.GetProperties()
			.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null);

	private static Type Unwrap(Type t) => Nullable.GetUnderlyingType(t) ?? t;

	private static bool IsList(Type t) =>
		t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);

	private static string Camel(string s) =>
		char.ToLowerInvariant(s[0]) + s[1..];

	private static string ToSnake(string s) =>
		string.Concat(s.Select((c, i) =>
			i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()));

	private static string DartType(PropertyInfo p) {
		Type t = Unwrap(p.PropertyType);

		if (t == typeof(string)) return "String?";
		if (t == typeof(int)) return "int?";
		if (t == typeof(bool)) return "bool?";
		if (t == typeof(double) || t == typeof(float)) return "double?";

		if (IsList(t))
			return $"List<{t.GetGenericArguments()[0].Name}>?";

		return $"{t.Name}?";
	}

	private static string FromMap(PropertyInfo p) {
		string name = Camel(p.Name);
		Type t = Unwrap(p.PropertyType);

		if (t == typeof(string) || t.IsPrimitive)
			return $"json[\"{name}\"]";

		if (IsList(t)) {
			string arg = t.GetGenericArguments()[0].Name;
			return
				$"json[\"{name}\"] == null ? <{arg}>[] : " +
				$"List<{arg}>.from(json[\"{name}\"].map((x) => {arg}.fromMap(x)))";
		}

		return $"json[\"{name}\"] == null ? null : {t.Name}.fromMap(json[\"{name}\"])";
	}

	private static string ToMap(PropertyInfo p) {
		string name = Camel(p.Name);
		Type t = Unwrap(p.PropertyType);

		if (t == typeof(string) || t.IsPrimitive)
			return name;

		if (IsList(t))
			return $"{name}?.map((x) => x.toMap()).toList()";

		return $"{name}?.toMap()";
	}
}