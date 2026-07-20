namespace SinaMN75U.Utils;

public static partial class ModelCodeGenerator {
	private static readonly Assembly Asm = typeof(BaseParams).Assembly;
	private const StringComparison Oic = StringComparison.OrdinalIgnoreCase;

	private static readonly Dictionary<string, string[]> Prim = new() {
		["string"] = ["String", "String", "String", "string", "string"],
		["guid"] = ["String", "String", "String", "Guid", "string"],
		["bool"] = ["bool", "Boolean", "Boolean", "bool", "boolean"],
		["int"] = ["int", "Int", "Integer", "int", "number"],
		["long"] = ["int", "Long", "Long", "long", "number"],
		["double"] = ["double", "Double", "Double", "double", "number"],
		["datetime"] = ["DateTime", "String", "String", "DateTime", "string"],
		["object"] = ["dynamic", "Any", "Object", "object", "any"]
	};

	private sealed class Prop {
		public required string pascal;
		public required Type clr;
		public required bool nullable;
	}

	private sealed class Model {
		public required string name;
		public required string group;
		public bool isEnum;
		public readonly List<Prop> props = [];
		public readonly List<KeyValuePair<string, long>> enumValues = [];
	}

	private sealed class Section {
		public required string name;
		public required string group;
		public List<string> items = [];
	}

	private sealed class SrcFile {
		public required string name;
		public required string folder;
		public required List<string> types;
	}

	public static void MapUModelsPage(this WebApplication app) {
		app.MapGet("/models", (EndpointDataSource eds) => Results.Content(BuildHtml(eds), "text/html; charset=utf-8"));
		app.MapGet("/models/json", (EndpointDataSource eds) => Results.Content(JsonSerializer.Serialize(BuildPayload(eds)), "application/json; charset=utf-8"));
	}

	private static bool DerivesFromBaseParams(Type t) => typeof(BaseParams).IsAssignableFrom(t) && t != typeof(BaseParams);

	private static bool DerivesFromBaseResponse(Type t) {
		for (Type? b = t.BaseType; b != null; b = b.BaseType)
			if (b.IsGenericType && b.GetGenericTypeDefinition() == typeof(BaseResponse<,>))
				return true;
		return false;
	}

	private static bool IsJsonData(Type t) => typeof(BaseJson).IsAssignableFrom(t) && t != typeof(BaseJson);

	private static bool IsCustomClass(Type t) => t.Assembly == Asm && t.IsClass && t != typeof(string) && t is { IsGenericType: false, IsArray: false };

	private static bool ShouldInclude(Type t) => t.Assembly == Asm && t != typeof(string) && (t.IsEnum || t is { IsClass: true, IsGenericTypeDefinition: false });

	private static Type? ElementType(Type u) {
		if (u == typeof(string) || u == typeof(byte[])) return null;
		if (u.IsArray) return u.GetElementType();
		if (u.IsGenericType && u.GetGenericArguments().Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return u.GetGenericArguments()[0];
		return null;
	}

	private static IEnumerable<Type> LeafTypes(Type t) {
		Type u = Nullable.GetUnderlyingType(t) ?? t;
		if (u == typeof(byte[])) yield break;
		if (u.IsArray) {
			foreach (Type x in LeafTypes(u.GetElementType()!)) yield return x;
			yield break;
		}

		if (u.IsGenericType) {
			foreach (Type arg in u.GetGenericArguments())
			foreach (Type x in LeafTypes(arg))
				yield return x;
			yield break;
		}

		yield return u;
	}

	private static Dictionary<Type, Model> Collect(IEnumerable<Type> roots) {
		NullabilityInfoContext nic = new();
		Dictionary<Type, Model> result = new();
		HashSet<Type> seen = [];
		Queue<Type> queue = new();
		foreach (Type r in roots)
			if (seen.Add(r))
				queue.Enqueue(r);

		while (queue.Count > 0) {
			Type t = queue.Dequeue();

			if (t.IsEnum) {
				Model em = new() { name = t.Name, group = "Enums", isEnum = true };
				string[] names = t.GetEnumNames();
				Array values = t.GetEnumValues();
				for (int i = 0; i < names.Length; i++)
					em.enumValues.Add(new KeyValuePair<string, long>(names[i], Convert.ToInt64(values.GetValue(i))));
				result[t] = em;
				continue;
			}

			string group = DerivesFromBaseParams(t) ? "Params" : DerivesFromBaseResponse(t) ? "Responses" : "Shared";
			Model m = new() { name = t.Name, group = group };
			HashSet<string> propSeen = [];

			foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
				if (p.Name is "ApiKey" or "Token") continue;
				if (p.GetIndexParameters().Length > 0 || !propSeen.Add(p.Name)) continue;
				bool nullable = Nullable.GetUnderlyingType(p.PropertyType) != null || nic.Create(p).ReadState == NullabilityState.Nullable;
				m.props.Add(new Prop { pascal = p.Name, clr = p.PropertyType, nullable = nullable });
				foreach (Type leaf in LeafTypes(p.PropertyType))
					if (ShouldInclude(leaf) && seen.Add(leaf))
						queue.Enqueue(leaf);
			}

			result[t] = m;
		}

		return result;
	}

	private static IEnumerable<Type> ReferencedTypes(Model m, Dictionary<Type, Model> models) {
		HashSet<Type> set = [];
		foreach (Type leaf in from p in m.props from leaf in LeafTypes(p.clr) where models.ContainsKey(leaf) select leaf)
			set.Add(leaf);
		return set;
	}

	private static string PrimName(Type u, int lang) {
		string cat;
		if (u == typeof(string) || u == typeof(char)) cat = "string";
		else if (u == typeof(Guid)) cat = "guid";
		else if (u == typeof(bool)) cat = "bool";
		else if (u == typeof(long) || u == typeof(ulong)) cat = "long";
		else if (u == typeof(byte) || u == typeof(sbyte) || u == typeof(short) || u == typeof(ushort) || u == typeof(int) || u == typeof(uint)) cat = "int";
		else if (u == typeof(float) || u == typeof(double) || u == typeof(decimal)) cat = "double";
		else if (u == typeof(DateTime) || u == typeof(DateTimeOffset) || u == typeof(TimeSpan) || u == typeof(DateOnly) || u == typeof(TimeOnly)) cat = "datetime";
		else cat = "object";
		return Prim[cat][lang];
	}

	private static string RenderType(Type t, int lang) {
		while (true) {
			Type u = Nullable.GetUnderlyingType(t) ?? t;
			if (u == typeof(byte[])) return Prim["string"][lang];
			if (u.IsArray) return Wrap(RenderType(u.GetElementType()!, lang), lang);
			if (u.IsGenericType) {
				Type[] a = u.GetGenericArguments();
				if (a.Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return Wrap(RenderType(a[0], lang), lang);
				if (a.Length == 2 && typeof(IEnumerable).IsAssignableFrom(u)) {
					string k = RenderType(a[0], lang), v = RenderType(a[1], lang);
					return lang switch {
						3 => $"Dictionary<{k}, {v}>",
						4 => $"Record<{k}, {v}>",
						_ => $"Map<{k}, {v}>"
					};
				}

				t = a[0];
				continue;
			}

			if (u.IsEnum) return u.Name;
			if (IsCustomClass(u)) return "U" + u.Name;
			return PrimName(u, lang);
		}
	}

	private static string Wrap(string inner, int lang) => lang == 4 ? $"{inner}[]" : $"List<{inner}>";

	private static string Camel(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];

	private static string Out(Model m) => m.isEnum ? m.name : "U" + m.name;

	private static string DartType(Type t) {
		while (true) {
			Type u = Nullable.GetUnderlyingType(t) ?? t;
			if (u == typeof(byte[])) return "String";
			if (u.IsArray) return $"List<{DartType(u.GetElementType()!)}>";
			if (u.IsGenericType) {
				Type[] a = u.GetGenericArguments();
				if (a.Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return $"List<{DartType(a[0])}>";
				if (a.Length == 2 && typeof(IEnumerable).IsAssignableFrom(u)) return $"Map<{DartType(a[0])}, {DartType(a[1])}>";
				t = a[0];
				continue;
			}

			if (u.IsEnum) return "int";
			if (IsCustomClass(u)) return "U" + u.Name;
			if (u == typeof(string) || u == typeof(char) || u == typeof(Guid)) return "String";
			if (u == typeof(bool)) return "bool";
			if (u == typeof(float) || u == typeof(double) || u == typeof(decimal)) return "double";
			if (u == typeof(DateTime) || u == typeof(DateTimeOffset)) return "DateTime";
			if (u == typeof(byte) || u == typeof(sbyte) || u == typeof(short) || u == typeof(ushort) || u == typeof(int) || u == typeof(uint) || u == typeof(long) || u == typeof(ulong)) return "int";
			return "dynamic";
		}
	}

	private static (string read, string write) DartSer(string name, Type t, bool nullable) {
		string j = $"json[\"{name}\"]";
		Type u = Nullable.GetUnderlyingType(t) ?? t;

		Type? elem = ElementType(u);
		if (elem != null) {
			string et = DartType(elem);
			bool custom = IsCustomClass(elem);
			string read = $"{j} == null ? <{et}>[] : List<{et}>.from({j}!.map((dynamic x) => {(custom ? $"{et}.fromMap(x)" : "x")}))";
			string body = $"List<dynamic>.from({name}{(nullable ? "!" : "")}.map(({et} x) => {(custom ? "x.toMap()" : "x")}))";
			string write = nullable ? $"{name} == null ? {(custom ? "null" : "<dynamic>[]")} : {body}" : body;
			return (read, write);
		}

		if (u == typeof(DateTime) || u == typeof(DateTimeOffset))
			return (nullable ? $"{j} == null ? null : DateTime.parse({j})" : $"DateTime.parse({j})",
				nullable ? $"{name}?.toIso8601String()" : $"{name}.toIso8601String()");

		if (IsCustomClass(u))
			return (nullable ? $"{j} == null ? null : U{u.Name}.fromMap({j})" : $"U{u.Name}.fromMap({j})",
				nullable ? $"{name}?.toMap()" : $"{name}.toMap()");

		if (u == typeof(float) || u == typeof(double) || u == typeof(decimal))
			return (nullable ? $"{j}?.toDouble()" : $"({j} as num).toDouble()", name);

		return (j, name);
	}

	private static string GenDart(Model m) {
		string tn = Out(m);
		if (m.isEnum) {
			string members = string.Join("\n", m.enumValues.Select(e => $"  {Camel(e.Key)}({e.Value}),"));
			return $"enum {tn} {{\n{members}\n  ;\n\n  final int number;\n  const {tn}(this.number);\n}}";
		}

		StringBuilder sb = new();
		sb.Append($"class {tn} {{\n");
		foreach (Prop p in m.props) sb.Append($"  final {DartType(p.clr)}{(p.nullable ? "?" : "")} {Camel(p.pascal)};\n");
		sb.Append($"\n  {tn}({{\n");
		foreach (Prop p in m.props.Where(x => !x.nullable).Concat(m.props.Where(x => x.nullable)))
			sb.Append($"    {(p.nullable ? "" : "required ")}this.{Camel(p.pascal)},\n");
		sb.Append("  });\n\n");
		sb.Append($"  factory {tn}.fromJson(String str) => {tn}.fromMap(json.decode(str));\n\n");
		sb.Append("  String toJson() => json.encode(toMap());\n\n");
		sb.Append($"  factory {tn}.fromMap(Map<String, dynamic> json) => {tn}(\n");
		foreach (Prop p in m.props) sb.Append($"    {Camel(p.pascal)}: {DartSer(Camel(p.pascal), p.clr, p.nullable).read},\n");
		sb.Append("  );\n\n");
		sb.Append("  Map<String, dynamic> toMap() => <String, dynamic>{\n");
		foreach (Prop p in m.props) sb.Append($"    \"{Camel(p.pascal)}\": {DartSer(Camel(p.pascal), p.clr, p.nullable).write},\n");
		sb.Append("  };\n}");
		return sb.ToString();
	}

	private static string GenKotlin(Model m) {
		string tn = Out(m);
		return m.isEnum ? $"enum class {tn}(val value: Int) {{\n{string.Join("\n", m.enumValues.Select(e => $"    {e.Key}({e.Value}),"))}\n}}" : $"data class {tn}(\n{string.Join("\n", m.props.Select(p => $"    val {Camel(p.pascal)}: {RenderType(p.clr, 1)}{(p.nullable ? "? = null" : "")},"))}\n)";
	}

	private static string GenJava(Model m) {
		string tn = Out(m);
		return m.isEnum ? $"public enum {tn} {{\n{string.Join("\n", m.enumValues.Select(e => $"    {e.Key}({e.Value}),"))}\n    ;\n\n    public final int value;\n    {tn}(int value) {{ this.value = value; }}\n}}" : $"public record {tn}(\n{string.Join(",\n", m.props.Select(p => $"    {RenderType(p.clr, 2)} {Camel(p.pascal)}"))}\n) {{}}";
	}

	private static string GenCsharp(Model m) {
		string tn = Out(m);
		if (m.isEnum)
			return $"public enum {tn} {{\n{string.Join("\n", m.enumValues.Select(e => $"    {e.Key} = {e.Value},"))}\n}}";
		StringBuilder sb = new();
		sb.Append($"public class {tn} {{\n");
		foreach (Prop p in m.props) sb.Append($"    public {RenderType(p.clr, 3)}{(p.nullable ? "?" : "")} {p.pascal} {{ get; set; }}\n");
		sb.Append('}');
		return sb.ToString();
	}

	private static string GenTs(Model m) {
		string tn = Out(m);
		return m.isEnum ? $"export enum {tn} {{\n{string.Join("\n", m.enumValues.Select(e => $"  {e.Key} = {e.Value},"))}\n}}" : $"export interface {tn} {{\n{string.Join("\n", m.props.Select(p => $"  {Camel(p.pascal)}{(p.nullable ? "?" : "")}: {RenderType(p.clr, 4)};"))}\n}}";
	}

	private static string? FindSrcRoot() {
		foreach (string start in new[] { SelfFilePath(), AppContext.BaseDirectory, Directory.GetCurrentDirectory() }) {
			if (string.IsNullOrEmpty(start)) continue;
			try {
				DirectoryInfo? d = new(File.Exists(start) ? Path.GetDirectoryName(start)! : start);
				for (int i = 0; i < 12 && d != null; i++, d = d.Parent)
					if (Directory.Exists(Path.Combine(d.FullName, "Src", "Data", "Params")))
						return d.FullName;
			}
			catch {
				// ignored
			}
		}

		return null;
	}

	private static string SelfFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

	private static List<SrcFile> ParseSource(string root) {
		List<SrcFile> files = [];
		files.AddRange(from path in Directory.EnumerateFiles(Path.Combine(root, "Src"), "*.cs", SearchOption.AllDirectories)
			let names = TypeDeclRegex().Matches(File.ReadAllText(path)).Select(x => x.Groups[1].Value).Distinct().ToList()
			where names.Count != 0
			select new SrcFile { name = Path.GetFileNameWithoutExtension(path), folder = Path.GetFileName(Path.GetDirectoryName(path)!), types = names });
		return files;
	}

	private static Dictionary<string, Type> NameToType() {
		Dictionary<string, Type> map = new();
		foreach (Type t in Asm.GetTypes()) {
			if (t.IsGenericTypeDefinition) continue;
			if (!map.TryGetValue(t.Name, out Type? existing) || Prefer(t) && !Prefer(existing)) map[t.Name] = t;
		}

		return map;

		static bool Prefer(Type t) => t.Namespace != null && (t.Namespace.Contains("Data") || t.Namespace.Contains("Constants"));
	}

	private static List<Section> BuildFileSections(List<SrcFile> files, Dictionary<Type, Model> models, Dictionary<string, Model> byName) {
		HashSet<string> placed = [];
		List<Section> sections = [];

		IEnumerable<SrcFile> ordered = files.Where(f => f.folder.Equals("Params", Oic)).OrderBy(f => f.name)
			.Concat(files.Where(f => f.folder.Equals("Responses", Oic)).OrderBy(f => f.name))
			.Concat(files.Where(f => f.folder.Equals("Data", Oic)).OrderBy(f => f.name));

		foreach (SrcFile f in ordered) {
			string grp = f.folder.Equals("Params", Oic) ? "Params" : f.folder.Equals("Responses", Oic) ? "Responses" : "Models";
			Section sec = new() { name = f.name, group = grp };
			foreach (string n in f.types)
				if (byName.TryGetValue(n, out Model? m) && !m.isEnum && placed.Add(n))
					sec.items.Add(n);
			if (sec.items.Count > 0) sections.Add(sec);
		}

		foreach (Section sec in sections.Where(s => s.group == "Responses")) {
			for (int i = 0; i < sec.items.Count; i++) {
				Model m = byName[sec.items[i]];
				foreach (Type refT in ReferencedTypes(m, models))
					if (IsJsonData(refT) && placed.Add(refT.Name))
						sec.items.Add(refT.Name);
			}
		}

		List<string> leftover = models.Values.Where(m => !m.isEnum && !placed.Contains(m.name)).Select(m => m.name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (leftover.Count > 0) sections.Add(new Section { name = "Shared", group = "Models", items = leftover });

		List<string> enums = models.Values.Where(m => m.isEnum).Select(m => m.name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (enums.Count > 0) sections.Add(new Section { name = "Enums", group = "Enums", items = enums });

		return sections;
	}

	private static List<Section> BuildFallbackSections(Dictionary<Type, Model> models) {
		HashSet<string> placed = [];
		List<Section> sections = [];

		List<string> pars = models.Values.Where(m => !m.isEnum && m.group == "Params").Select(m => m.name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		foreach (string n in pars) placed.Add(n);
		if (pars.Count > 0) sections.Add(new Section { name = "Params", group = "Params", items = pars });

		Section resp = new() { name = "Responses", group = "Responses" };
		foreach (string n in models.Values.Where(m => !m.isEnum && m.group == "Responses").Select(m => m.name).OrderBy(x => x, StringComparer.Ordinal)) {
			resp.items.Add(n);
			placed.Add(n);
		}

		for (int i = 0; i < resp.items.Count; i++) {
			Model m = models.Values.First(x => x.name == resp.items[i]);
			foreach (Type refT in ReferencedTypes(m, models))
				if (!refT.IsEnum && placed.Add(refT.Name))
					resp.items.Add(refT.Name);
		}

		if (resp.items.Count > 0) sections.Add(resp);

		List<string> enums = models.Values.Where(m => m.isEnum).Select(m => m.name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (enums.Count > 0) sections.Add(new Section { name = "Enums", group = "Enums", items = enums });

		return sections;
	}

	private static object BuildPayload(EndpointDataSource eds) {
		List<SrcFile>? files = null;
		string? root = FindSrcRoot();
		if (root != null)
			try {
				files = ParseSource(root);
			}
			catch {
				files = null;
			}

		Dictionary<string, Type> nameToType = NameToType();
		List<Type> roots;
		if (files != null) {
			HashSet<string> dtoFolders = new(StringComparer.OrdinalIgnoreCase) { "Params", "Responses", "Data" };
			roots = files.Where(f => dtoFolders.Contains(f.folder))
				.SelectMany(f => f.types)
				.Distinct()
				.Select(nameToType.GetValueOrDefault)
				.Where(t => t is { IsClass: true, IsGenericTypeDefinition: false } && !(t.IsAbstract && t.IsSealed))
				.Select(t => t!)
				.ToList();
		}
		else {
			roots = Asm.GetTypes().Where(t => t.IsClass && !t.IsGenericTypeDefinition && (DerivesFromBaseParams(t) || DerivesFromBaseResponse(t))).ToList();
		}

		Dictionary<Type, Model> models = Collect(roots);
		Dictionary<string, Model> byName = new();
		foreach (Model m in models.Values) byName[m.name] = m;

		Dictionary<string, object> types = new();
		foreach (Model m in models.Values)
			types[Out(m)] = new {
				dart = GenDart(m),
				kotlin = GenKotlin(m),
				java = GenJava(m),
				csharp = GenCsharp(m),
				typescript = GenTs(m)
			};

		List<Section> sections = files != null ? BuildFileSections(files, models, byName) : BuildFallbackSections(models);
		var outSections = sections.Where(s => s.items.Count > 0)
			.Select(s => new { name = s.name, group = s.group, items = s.items.Select(n => Out(byName[n])).ToList() })
			.ToList();
		return new { sections = outSections, types, apis = BuildApis(eds, models) };
	}

	// ---------- apis ----------

	private static readonly JsonSerializerOptions ExampleOpts = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	private static string ExampleJson(Type? t) => t == null ? "null" : JsonSerializer.Serialize(BuildExample(t, []), ExampleOpts);

	private static object? BuildExample(Type t, HashSet<Type> stack) {
		Type u = Nullable.GetUnderlyingType(t) ?? t;
		if (u == typeof(byte[])) return "string";
		if (u.IsEnum) {
			Array vals = u.GetEnumValues();
			return vals.Length > 0 ? Convert.ToInt64(vals.GetValue(0)) : 0L;
		}

		if (u == typeof(string) || u == typeof(char)) return "string";
		if (u == typeof(Guid)) return "00000000-0000-0000-0000-000000000000";
		if (u == typeof(bool)) return true;
		if (u == typeof(DateTime) || u == typeof(DateTimeOffset)) return "2024-01-01T00:00:00Z";
		if (u == typeof(float) || u == typeof(double) || u == typeof(decimal)) return 0.0;
		if (u == typeof(byte) || u == typeof(sbyte) || u == typeof(short) || u == typeof(ushort) || u == typeof(int) || u == typeof(uint) || u == typeof(long) || u == typeof(ulong)) return 0L;

		Type? elem = ElementType(u);
		if (elem != null) {
			Type ek = Nullable.GetUnderlyingType(elem) ?? elem;
			if (ek.Assembly == Asm && ek.IsClass && stack.Contains(ek)) return new List<object?>();
			return new List<object?> { BuildExample(elem, stack) };
		}

		if (u.Assembly == Asm && u.IsClass) {
			if (!stack.Add(u)) return null;
			Dictionary<string, object?> dict = new();
			foreach (PropertyInfo p in u.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
				if (p.Name is "ApiKey" or "Token" || p.GetIndexParameters().Length > 0) continue;
				dict[Camel(p.Name)] = BuildExample(p.PropertyType, stack);
			}

			stack.Remove(u);
			return dict;
		}

		return null;
	}

	private static List<object> BuildApis(EndpointDataSource eds, Dictionary<Type, Model> models) {
		List<(string tag, string method, string path, object code)> rows = [];

		foreach (RouteEndpoint ep in eds.Endpoints.OfType<RouteEndpoint>()) {
			string? raw = ep.RoutePattern.RawText;
			if (string.IsNullOrEmpty(raw)) continue;
			string path = "/" + raw.TrimStart('/');
			if (path.StartsWith("/models", Oic) || path.StartsWith("/swagger", Oic)) continue;

			string method = ep.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault(x => x is not ("OPTIONS" or "HEAD")) ?? "POST";
			string tag = ep.Metadata.GetMetadata<Microsoft.AspNetCore.Http.Metadata.ITagsMetadata>()?.Tags.FirstOrDefault() ?? "default";
			Type? reqType = ep.Metadata.GetMetadata<Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata>()?.RequestType;
			Type? respType = ep.Metadata.OfType<Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata>()
				.OrderBy(x => x.StatusCode == 200 ? 0 : 1)
				.Select(x => x.Type)
				.FirstOrDefault();

			List<Model> req = Closure(reqType, models);
			List<Model> resp = Closure(respType, models);

			object code = new {
				json = new { request = reqType != null ? ExampleJson(reqType) : "// (no request body)", response = respType != null ? ExampleJson(respType) : "// (no declared response)" },
				dart = new { request = Block(req, GenDart), response = Block(resp, GenDart) },
				kotlin = new { request = Block(req, GenKotlin), response = Block(resp, GenKotlin) },
				java = new { request = Block(req, GenJava), response = Block(resp, GenJava) },
				csharp = new { request = Block(req, GenCsharp), response = Block(resp, GenCsharp) },
				typescript = new { request = Block(req, GenTs), response = Block(resp, GenTs) }
			};
			rows.Add((tag, method, path, code));
		}

		return rows
			.OrderBy(r => r.tag, StringComparer.Ordinal).ThenBy(r => r.path, StringComparer.Ordinal).ThenBy(r => r.method, StringComparer.Ordinal)
			.Select(r => (object)new { method = r.method, path = r.path, tag = r.tag, code = r.code })
			.ToList();
	}

	private static string Block(List<Model> list, Func<Model, string> gen) => list.Count == 0 ? "// (none)" : string.Join("\n\n", list.Select(gen));

	// Transitive set of generated models reachable from a type (unwraps the UResponse<T> envelope, collections, nullables).
	private static List<Model> Closure(Type? start, Dictionary<Type, Model> models) {
		List<Model> order = [];
		if (start == null) return order;
		HashSet<Type> seen = [];
		Queue<Type> queue = new();
		foreach (Type lt in LeafTypes(start)) queue.Enqueue(lt);
		while (queue.Count > 0) {
			Type t = queue.Dequeue();
			if (!models.TryGetValue(t, out Model? m) || !seen.Add(t)) continue;
			order.Add(m);
			foreach (Prop p in m.props)
			foreach (Type lt in LeafTypes(p.clr))
				queue.Enqueue(lt);
		}

		return order;
	}

	private static string BuildHtml(EndpointDataSource eds) => HtmlTemplate.Replace("/*__DATA__*/", JsonSerializer.Serialize(BuildPayload(eds)).Replace("<", "\\u003c"));

	[GeneratedRegex(@"\b(?:class|record|struct|enum)\s+([A-Za-z_]\w*)")]
	private static partial Regex TypeDeclRegex();

	private const string HtmlTemplate = """
	                                    <!DOCTYPE html>
	                                    <html lang="en">
	                                    <head>
	                                    <meta charset="utf-8">
	                                    <meta name="viewport" content="width=device-width, initial-scale=1">
	                                    <title>SinaMN75 API</title>
	                                    <style>
	                                      :root { --bg:#0f1419; --panel:#1a1f29; --panel2:#141922; --border:#2a3140; --text:#e6e9ef; --muted:#8b93a3; --accent:#4fa3ff; --accent2:#2d3648; }
	                                      * { box-sizing:border-box; }
	                                      body { margin:0; font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif; background:var(--bg); color:var(--text); height:100vh; display:flex; flex-direction:column; }
	                                      header { padding:12px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; gap:16px; background:var(--panel2); }
	                                      header h1 { font-size:16px; margin:0; font-weight:600; }
	                                      .nav { display:flex; gap:4px; }
	                                      .navbtn { padding:6px 16px; font-size:13px; border-radius:6px; border:1px solid var(--border); background:transparent; color:var(--muted); cursor:pointer; }
	                                      .navbtn.active { background:var(--accent); border-color:var(--accent); color:#fff; }
	                                      .wrap { flex:1; display:flex; min-height:0; }
	                                      aside { width:300px; border-right:1px solid var(--border); overflow-y:auto; background:var(--panel2); }
	                                      aside .search { padding:10px; position:sticky; top:0; background:var(--panel2); border-bottom:1px solid var(--border); }
	                                      aside input { width:100%; padding:8px 10px; border-radius:6px; border:1px solid var(--border); background:var(--bg); color:var(--text); font-size:13px; outline:none; }
	                                      .group-title { padding:9px 14px; font-size:11px; text-transform:uppercase; letter-spacing:.06em; color:var(--muted); cursor:pointer; user-select:none; display:flex; align-items:center; gap:7px; }
	                                      .group-title:hover { color:var(--text); }
	                                      .chev { display:inline-block; width:9px; font-size:9px; }
	                                      .cnt { margin-left:auto; font-size:10px; color:var(--muted); background:var(--panel); padding:0 7px; border-radius:9px; }
	                                      .item { padding:6px 14px; font-size:13px; cursor:pointer; color:var(--text); border-left:3px solid transparent; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
	                                      .item:hover { background:var(--panel); }
	                                      .item.active { background:var(--accent2); border-left-color:var(--accent); color:#fff; }
	                                      .m { display:inline-block; min-width:42px; font-size:10px; font-weight:700; padding:1px 5px; border-radius:4px; margin-right:6px; text-align:center; }
	                                      .m-post { background:#1c3a2e; color:#4ade80; }
	                                      .m-get { background:#1c2f45; color:#60a5fa; }
	                                      .m-put { background:#3a331c; color:#fbbf24; }
	                                      .m-delete { background:#3a1c1c; color:#f87171; }
	                                      main { flex:1; display:flex; flex-direction:column; min-width:0; }
	                                      .tabs { display:flex; gap:2px; padding:12px 20px 0; border-bottom:1px solid var(--border); flex-wrap:wrap; }
	                                      .tab { padding:8px 16px; font-size:13px; cursor:pointer; color:var(--muted); border:1px solid transparent; border-bottom:none; border-radius:6px 6px 0 0; }
	                                      .tab:hover { color:var(--text); }
	                                      .tab.active { color:#fff; background:var(--panel); border-color:var(--border); }
	                                      .codebar { display:flex; align-items:center; justify-content:space-between; padding:12px 20px 0; }
	                                      .codebar .name { font-size:15px; font-weight:600; }
	                                      .copy { padding:6px 12px; font-size:12px; border-radius:6px; border:1px solid var(--border); background:var(--panel); color:var(--text); cursor:pointer; }
	                                      .copy:hover { border-color:var(--accent); color:var(--accent); }
	                                      .codewrap { flex:1; overflow:auto; padding:0 0 24px; }
	                                      pre { margin:0 20px 6px; background:var(--panel); border:1px solid var(--border); border-radius:8px; padding:16px; font-size:13px; line-height:1.55; font-family:"SF Mono",Menlo,Consolas,monospace; white-space:pre; overflow:auto; }
	                                      .empty { color:var(--muted); padding:40px 20px; }
	                                      .apihead { padding:16px 20px 2px; }
	                                      .apipath { font-size:15px; font-weight:600; margin-left:4px; }
	                                      .blockhead { display:flex; align-items:center; justify-content:space-between; padding:16px 20px 6px; }
	                                      .btitle { font-size:12px; text-transform:uppercase; letter-spacing:.06em; color:var(--muted); }
	                                    </style>
	                                    </head>
	                                    <body>
	                                    <header>
	                                      <h1>SinaMN75</h1>
	                                      <div class="nav">
	                                        <button class="navbtn active" data-view="models">Models</button>
	                                        <button class="navbtn" data-view="apis">APIs</button>
	                                      </div>
	                                    </header>
	                                    <div class="wrap">
	                                      <aside>
	                                        <div class="search"><input id="search" placeholder="Filter..." autocomplete="off"></div>
	                                        <div id="list"></div>
	                                      </aside>
	                                      <main>
	                                        <div class="tabs" id="tabs"></div>
	                                        <div class="codewrap" id="panel"><div class="empty">Pick an item from the left.</div></div>
	                                      </main>
	                                    </div>
	                                    <script>
	                                      const DATA = /*__DATA__*/;
	                                      const MODEL_LANGS = [["dart","Dart"],["kotlin","Kotlin"],["java","Java"],["csharp","C#"],["typescript","TypeScript"]];
	                                      const API_LANGS = [["json","JSON"],["dart","Dart"],["kotlin","Kotlin"],["java","Java"],["csharp","C#"],["typescript","TypeScript"]];
	                                      const GROUP_ORDER = ["Params","Responses","Models","Enums"];
	                                      const byName = {};
	                                      for (const s of DATA.sections) byName[s.name] = s;

	                                      let view = "models";
	                                      let modelLang = "dart";
	                                      let apiLang = "json";
	                                      let currentSection = null;
	                                      let currentApi = -1;
	                                      const expanded = new Set();

	                                      const listEl = document.getElementById("list");
	                                      const tabsEl = document.getElementById("tabs");
	                                      const panelEl = document.getElementById("panel");
	                                      const searchEl = document.getElementById("search");

	                                      const langs = () => view === "models" ? MODEL_LANGS : API_LANGS;
	                                      const curLang = () => view === "models" ? modelLang : apiLang;
	                                      const setLang = l => { if (view === "models") modelLang = l; else apiLang = l; };

	                                      function groupHeader(key, label, count, searching) {
	                                        const open = searching || expanded.has(key);
	                                        const t = document.createElement("div");
	                                        t.className = "group-title";
	                                        t.innerHTML = '<span class="chev">' + (open ? "▾" : "▸") + '</span><span>' + label + '</span><span class="cnt">' + count + '</span>';
	                                        t.onclick = () => { if (expanded.has(key)) expanded.delete(key); else expanded.add(key); renderList(); };
	                                        listEl.appendChild(t);
	                                        return open;
	                                      }

	                                      function renderNav() {
	                                        document.querySelectorAll(".navbtn").forEach(b => b.classList.toggle("active", b.dataset.view === view));
	                                      }

	                                      function renderTabs() {
	                                        tabsEl.innerHTML = "";
	                                        for (const [id, label] of langs()) {
	                                          const t = document.createElement("div");
	                                          t.className = "tab" + (id === curLang() ? " active" : "");
	                                          t.textContent = label;
	                                          t.onclick = () => { setLang(id); renderTabs(); renderPanel(); };
	                                          tabsEl.appendChild(t);
	                                        }
	                                      }

	                                      function renderList() {
	                                        const f = searchEl.value.toLowerCase();
	                                        const searching = f.trim() !== "";
	                                        listEl.innerHTML = "";
	                                        if (view === "models") {
	                                          for (const g of GROUP_ORDER) {
	                                            const secs = DATA.sections.filter(s => s.group === g && (s.name.toLowerCase().includes(f) || s.items.some(n => n.toLowerCase().includes(f))));
	                                            if (!secs.length) continue;
	                                            if (!groupHeader("m:" + g, g, secs.length, searching)) continue;
	                                            for (const s of secs) {
	                                              const d = document.createElement("div");
	                                              d.className = "item" + (s.name === currentSection ? " active" : "");
	                                              d.textContent = s.name;
	                                              d.onclick = () => { currentSection = s.name; renderList(); renderPanel(); };
	                                              listEl.appendChild(d);
	                                            }
	                                          }
	                                        } else {
	                                          const tags = [];
	                                          for (const a of DATA.apis) if (!tags.includes(a.tag)) tags.push(a.tag);
	                                          for (const tag of tags) {
	                                            const items = DATA.apis.map((a, i) => ({ a, i })).filter(o => o.a.tag === tag && (o.a.path.toLowerCase().includes(f) || o.a.method.toLowerCase().includes(f)));
	                                            if (!items.length) continue;
	                                            if (!groupHeader("a:" + tag, tag, items.length, searching)) continue;
	                                            for (const o of items) {
	                                              const d = document.createElement("div");
	                                              d.className = "item" + (o.i === currentApi ? " active" : "");
	                                              d.innerHTML = '<span class="m m-' + o.a.method.toLowerCase() + '">' + o.a.method + '</span>' + o.a.path;
	                                              d.onclick = () => { currentApi = o.i; renderList(); renderPanel(); };
	                                              listEl.appendChild(d);
	                                            }
	                                          }
	                                        }
	                                      }

	                                      function makeBlock(title, code) {
	                                        const head = document.createElement("div");
	                                        head.className = "blockhead";
	                                        const t = document.createElement("span");
	                                        t.className = "btitle";
	                                        t.textContent = title;
	                                        const btn = document.createElement("button");
	                                        btn.className = "copy";
	                                        btn.textContent = "Copy";
	                                        btn.onclick = () => { navigator.clipboard.writeText(code); btn.textContent = "Copied"; setTimeout(() => btn.textContent = "Copy", 1200); };
	                                        head.appendChild(t);
	                                        head.appendChild(btn);
	                                        panelEl.appendChild(head);
	                                        const pre = document.createElement("pre");
	                                        pre.textContent = code;
	                                        panelEl.appendChild(pre);
	                                      }

	                                      function renderPanel() {
	                                        panelEl.innerHTML = "";
	                                        if (view === "models") {
	                                          if (!currentSection) { panelEl.innerHTML = '<div class="empty">Pick a file from the left.</div>'; return; }
	                                          const s = byName[currentSection];
	                                          makeBlock(currentSection, s.items.map(n => DATA.types[n][modelLang]).join("\n\n"));
	                                        } else {
	                                          if (currentApi < 0) { panelEl.innerHTML = '<div class="empty">Pick an endpoint from the left.</div>'; return; }
	                                          const a = DATA.apis[currentApi];
	                                          const h = document.createElement("div");
	                                          h.className = "apihead";
	                                          h.innerHTML = '<span class="m m-' + a.method.toLowerCase() + '">' + a.method + '</span> <span class="apipath">' + a.path + '</span>';
	                                          panelEl.appendChild(h);
	                                          const c = a.code[apiLang];
	                                          makeBlock("Request", c.request);
	                                          makeBlock("Response", c.response);
	                                        }
	                                      }

	                                      function switchView(v) {
	                                        if (v === view) return;
	                                        view = v;
	                                        searchEl.value = "";
	                                        renderNav();
	                                        renderTabs();
	                                        renderList();
	                                        renderPanel();
	                                      }

	                                      document.querySelectorAll(".navbtn").forEach(b => b.onclick = () => switchView(b.dataset.view));
	                                      searchEl.oninput = renderList;

	                                      renderNav();
	                                      renderTabs();
	                                      renderList();
	                                      renderPanel();
	                                    </script>
	                                    </body>
	                                    </html>
	                                    """;
}