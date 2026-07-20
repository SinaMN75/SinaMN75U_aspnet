namespace SinaMN75U.Utils;

public static partial class ModelCodeGenerator {
	private static readonly Assembly Asm = typeof(BaseParams).Assembly;
	private const StringComparison Oic = StringComparison.OrdinalIgnoreCase;

	private static readonly Dictionary<string, string[]> Prim = new() {
		// order: dart, kotlin, java, csharp, typescript
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
		public required string Pascal;
		public required Type Clr;
		public required bool Nullable;
	}

	private sealed class Model {
		public required string Name;
		public required string Group;
		public Type? Clr;
		public bool IsEnum;
		public List<Prop> Props = [];
		public List<KeyValuePair<string, long>> EnumValues = [];
	}

	private sealed class Section {
		public required string Name;
		public required string Group;
		public List<string> Items = [];
	}

	private sealed class SrcFile {
		public required string Name;
		public required string Folder;
		public required List<string> Types;
	}

	public static void MapUModelsPage(this WebApplication app) {
		app.MapGet("/models", () => Results.Content(BuildHtml(), "text/html; charset=utf-8"));
		app.MapGet("/models/json", () => Results.Content(JsonSerializer.Serialize(BuildPayload()), "application/json; charset=utf-8"));
	}

	// ---------- discovery ----------

	private static bool DerivesFromBaseParams(Type t) => typeof(BaseParams).IsAssignableFrom(t) && t != typeof(BaseParams);

	private static bool DerivesFromBaseResponse(Type t) {
		for (Type? b = t.BaseType; b != null; b = b.BaseType)
			if (b.IsGenericType && b.GetGenericTypeDefinition() == typeof(BaseResponse<,>))
				return true;
		return false;
	}

	private static bool IsJsonData(Type t) => typeof(BaseJson).IsAssignableFrom(t) && t != typeof(BaseJson);

	private static bool IsCustomClass(Type t) => t.Assembly == Asm && t.IsClass && t != typeof(string) && !t.IsGenericType && !t.IsArray;

	private static bool ShouldInclude(Type t) => t.Assembly == Asm && t != typeof(string) && (t.IsEnum || (t.IsClass && !t.IsGenericTypeDefinition));

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
			if (seen.Add(r)) queue.Enqueue(r);

		while (queue.Count > 0) {
			Type t = queue.Dequeue();

			if (t.IsEnum) {
				Model em = new() { Name = t.Name, Group = "Enums", IsEnum = true, Clr = t };
				string[] names = t.GetEnumNames();
				Array values = t.GetEnumValues();
				for (int i = 0; i < names.Length; i++)
					em.EnumValues.Add(new KeyValuePair<string, long>(names[i], Convert.ToInt64(values.GetValue(i))));
				result[t] = em;
				continue;
			}

			string group = DerivesFromBaseParams(t) ? "Params" : DerivesFromBaseResponse(t) ? "Responses" : "Shared";
			Model m = new() { Name = t.Name, Group = group, Clr = t };
			HashSet<string> propSeen = [];

			foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
				if (p.Name is "ApiKey" or "Token") continue;
				if (p.GetIndexParameters().Length > 0 || !propSeen.Add(p.Name)) continue;
				bool nullable = Nullable.GetUnderlyingType(p.PropertyType) != null || nic.Create(p).ReadState == NullabilityState.Nullable;
				m.Props.Add(new Prop { Pascal = p.Name, Clr = p.PropertyType, Nullable = nullable });
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
		foreach (Prop p in m.Props)
			foreach (Type leaf in LeafTypes(p.Clr))
				if (models.ContainsKey(leaf))
					set.Add(leaf);
		return set;
	}

	// ---------- type rendering (non-dart) ----------

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
		Type u = Nullable.GetUnderlyingType(t) ?? t;
		if (u == typeof(byte[])) return Prim["string"][lang];
		if (u.IsArray) return Wrap(RenderType(u.GetElementType()!, lang), lang);
		if (u.IsGenericType) {
			Type[] a = u.GetGenericArguments();
			if (a.Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return Wrap(RenderType(a[0], lang), lang);
			if (a.Length == 2 && typeof(IEnumerable).IsAssignableFrom(u)) {
				string k = RenderType(a[0], lang), v = RenderType(a[1], lang);
				return lang == 3 ? $"Dictionary<{k}, {v}>" : lang == 4 ? $"Record<{k}, {v}>" : $"Map<{k}, {v}>";
			}
			return RenderType(a[0], lang);
		}
		if (u.IsEnum) return u.Name;
		if (IsCustomClass(u)) return "U" + u.Name;
		return PrimName(u, lang);
	}

	private static string Wrap(string inner, int lang) => lang == 4 ? $"{inner}[]" : $"List<{inner}>";

	private static string Camel(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];

	// Generated classes are prefixed with "U"; enums keep their original name.
	private static string Out(Model m) => m.IsEnum ? m.Name : "U" + m.Name;

	// ---------- dart ----------

	private static string DartType(Type t) {
		Type u = Nullable.GetUnderlyingType(t) ?? t;
		if (u == typeof(byte[])) return "String";
		if (u.IsArray) return $"List<{DartType(u.GetElementType()!)}>";
		if (u.IsGenericType) {
			Type[] a = u.GetGenericArguments();
			if (a.Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return $"List<{DartType(a[0])}>";
			if (a.Length == 2 && typeof(IEnumerable).IsAssignableFrom(u)) return $"Map<{DartType(a[0])}, {DartType(a[1])}>";
			return DartType(a[0]);
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
		if (m.IsEnum) {
			string members = string.Join("\n", m.EnumValues.Select(e => $"  {Camel(e.Key)}({e.Value}),"));
			return $"enum {tn} {{\n{members}\n  ;\n\n  final int number;\n  const {tn}(this.number);\n}}";
		}

		StringBuilder sb = new();
		sb.Append($"class {tn} {{\n");
		foreach (Prop p in m.Props) sb.Append($"  final {DartType(p.Clr)}{(p.Nullable ? "?" : "")} {Camel(p.Pascal)};\n");
		sb.Append($"\n  {tn}({{\n");
		foreach (Prop p in m.Props.Where(x => !x.Nullable).Concat(m.Props.Where(x => x.Nullable)))
			sb.Append($"    {(p.Nullable ? "" : "required ")}this.{Camel(p.Pascal)},\n");
		sb.Append("  });\n\n");
		sb.Append($"  factory {tn}.fromJson(String str) => {tn}.fromMap(json.decode(str));\n\n");
		sb.Append("  String toJson() => json.encode(toMap());\n\n");
		sb.Append($"  factory {tn}.fromMap(Map<String, dynamic> json) => {tn}(\n");
		foreach (Prop p in m.Props) sb.Append($"    {Camel(p.Pascal)}: {DartSer(Camel(p.Pascal), p.Clr, p.Nullable).read},\n");
		sb.Append("  );\n\n");
		sb.Append("  Map<String, dynamic> toMap() => <String, dynamic>{\n");
		foreach (Prop p in m.Props) sb.Append($"    \"{Camel(p.Pascal)}\": {DartSer(Camel(p.Pascal), p.Clr, p.Nullable).write},\n");
		sb.Append("  };\n}");
		return sb.ToString();
	}

	// ---------- kotlin / java / csharp / typescript ----------

	private static string GenKotlin(Model m) {
		string tn = Out(m);
		if (m.IsEnum)
			return $"enum class {tn}(val value: Int) {{\n{string.Join("\n", m.EnumValues.Select(e => $"    {e.Key}({e.Value}),"))}\n}}";
		return $"data class {tn}(\n{string.Join("\n", m.Props.Select(p => $"    val {Camel(p.Pascal)}: {RenderType(p.Clr, 1)}{(p.Nullable ? "? = null" : "")},"))}\n)";
	}

	private static string GenJava(Model m) {
		string tn = Out(m);
		if (m.IsEnum)
			return $"public enum {tn} {{\n{string.Join("\n", m.EnumValues.Select(e => $"    {e.Key}({e.Value}),"))}\n    ;\n\n    public final int value;\n    {tn}(int value) {{ this.value = value; }}\n}}";
		return $"public record {tn}(\n{string.Join(",\n", m.Props.Select(p => $"    {RenderType(p.Clr, 2)} {Camel(p.Pascal)}"))}\n) {{}}";
	}

	private static string GenCsharp(Model m) {
		string tn = Out(m);
		if (m.IsEnum)
			return $"public enum {tn} {{\n{string.Join("\n", m.EnumValues.Select(e => $"    {e.Key} = {e.Value},"))}\n}}";
		StringBuilder sb = new();
		sb.Append($"public class {tn} {{\n");
		foreach (Prop p in m.Props) sb.Append($"    public {RenderType(p.Clr, 3)}{(p.Nullable ? "?" : "")} {p.Pascal} {{ get; set; }}\n");
		sb.Append('}');
		return sb.ToString();
	}

	private static string GenTs(Model m) {
		string tn = Out(m);
		if (m.IsEnum)
			return $"export enum {tn} {{\n{string.Join("\n", m.EnumValues.Select(e => $"  {e.Key} = {e.Value},"))}\n}}";
		return $"export interface {tn} {{\n{string.Join("\n", m.Props.Select(p => $"  {Camel(p.Pascal)}{(p.Nullable ? "?" : "")}: {RenderType(p.Clr, 4)};"))}\n}}";
	}

	// ---------- source parsing ----------

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
				// ignore and try next candidate
			}
		}
		return null;
	}

	// Resolves to this file's absolute path as of compile time, which lets us locate
	// the Src tree even when the package is running inside a different host project.
	private static string SelfFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

	private static List<SrcFile> ParseSource(string root) {
		List<SrcFile> files = [];
		foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Src"), "*.cs", SearchOption.AllDirectories)) {
			List<string> names = TypeDeclRegex().Matches(File.ReadAllText(path)).Select(x => x.Groups[1].Value).Distinct().ToList();
			if (names.Count == 0) continue;
			files.Add(new SrcFile {
				Name = Path.GetFileNameWithoutExtension(path),
				Folder = Path.GetFileName(Path.GetDirectoryName(path)!),
				Types = names
			});
		}
		return files;
	}

	private static Dictionary<string, Type> NameToType() {
		Dictionary<string, Type> map = new();
		foreach (Type t in Asm.GetTypes()) {
			if (t.IsGenericTypeDefinition) continue;
			if (!map.TryGetValue(t.Name, out Type? existing)) map[t.Name] = t;
			else if (Prefer(t) && !Prefer(existing)) map[t.Name] = t;
		}
		return map;

		static bool Prefer(Type t) => t.Namespace != null && (t.Namespace.Contains("Data") || t.Namespace.Contains("Constants"));
	}

	// ---------- sections ----------

	private static List<Section> BuildFileSections(List<SrcFile> files, Dictionary<Type, Model> models, Dictionary<string, Model> byName) {
		HashSet<string> placed = [];
		List<Section> sections = [];

		IEnumerable<SrcFile> ordered = files.Where(f => f.Folder.Equals("Params", Oic)).OrderBy(f => f.Name)
			.Concat(files.Where(f => f.Folder.Equals("Responses", Oic)).OrderBy(f => f.Name))
			.Concat(files.Where(f => f.Folder.Equals("Data", Oic)).OrderBy(f => f.Name));

		foreach (SrcFile f in ordered) {
			string grp = f.Folder.Equals("Params", Oic) ? "Params" : f.Folder.Equals("Responses", Oic) ? "Responses" : "Models";
			Section sec = new() { Name = f.Name, Group = grp };
			foreach (string n in f.Types)
				if (byName.TryGetValue(n, out Model? m) && !m.IsEnum && placed.Add(n))
					sec.Items.Add(n);
			if (sec.Items.Count > 0) sections.Add(sec);
		}

		// fold JsonData classes into the response section that references them
		foreach (Section sec in sections.Where(s => s.Group == "Responses")) {
			for (int i = 0; i < sec.Items.Count; i++) {
				Model m = byName[sec.Items[i]];
				foreach (Type refT in ReferencedTypes(m, models))
					if (IsJsonData(refT) && placed.Add(refT.Name))
						sec.Items.Add(refT.Name);
			}
		}

		List<string> leftover = models.Values.Where(m => !m.IsEnum && !placed.Contains(m.Name)).Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (leftover.Count > 0) sections.Add(new Section { Name = "Shared", Group = "Models", Items = leftover });

		List<string> enums = models.Values.Where(m => m.IsEnum).Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (enums.Count > 0) sections.Add(new Section { Name = "Enums", Group = "Enums", Items = enums });

		return sections;
	}

	private static List<Section> BuildFallbackSections(Dictionary<Type, Model> models) {
		HashSet<string> placed = [];
		List<Section> sections = [];

		List<string> pars = models.Values.Where(m => !m.IsEnum && m.Group == "Params").Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		foreach (string n in pars) placed.Add(n);
		if (pars.Count > 0) sections.Add(new Section { Name = "Params", Group = "Params", Items = pars });

		Section resp = new() { Name = "Responses", Group = "Responses" };
		foreach (string n in models.Values.Where(m => !m.IsEnum && m.Group == "Responses").Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal)) {
			resp.Items.Add(n);
			placed.Add(n);
		}
		for (int i = 0; i < resp.Items.Count; i++) {
			Model m = models.Values.First(x => x.Name == resp.Items[i]);
			foreach (Type refT in ReferencedTypes(m, models))
				if (!refT.IsEnum && placed.Add(refT.Name))
					resp.Items.Add(refT.Name);
		}
		if (resp.Items.Count > 0) sections.Add(resp);

		List<string> enums = models.Values.Where(m => m.IsEnum).Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal).ToList();
		if (enums.Count > 0) sections.Add(new Section { Name = "Enums", Group = "Enums", Items = enums });

		return sections;
	}

	// ---------- payload / html ----------

	private static object BuildPayload() {
		List<SrcFile>? files = null;
		string? root = FindSrcRoot();
		if (root != null)
			try { files = ParseSource(root); }
			catch { files = null; }

		Dictionary<string, Type> nameToType = NameToType();
		List<Type> roots;
		if (files != null) {
			HashSet<string> dtoFolders = new(StringComparer.OrdinalIgnoreCase) { "Params", "Responses", "Data" };
			roots = files.Where(f => dtoFolders.Contains(f.Folder))
				.SelectMany(f => f.Types)
				.Distinct()
				.Select(n => nameToType.GetValueOrDefault(n))
				.Where(t => t is { IsClass: true, IsGenericTypeDefinition: false } && !(t.IsAbstract && t.IsSealed))
				.Select(t => t!)
				.ToList();
		}
		else {
			roots = Asm.GetTypes().Where(t => t.IsClass && !t.IsGenericTypeDefinition && (DerivesFromBaseParams(t) || DerivesFromBaseResponse(t))).ToList();
		}

		Dictionary<Type, Model> models = Collect(roots);
		Dictionary<string, Model> byName = new();
		foreach (Model m in models.Values) byName[m.Name] = m;

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
		var outSections = sections.Where(s => s.Items.Count > 0)
			.Select(s => new { name = s.Name, group = s.Group, items = s.Items.Select(n => Out(byName[n])).ToList() })
			.ToList();
		return new { sections = outSections, types };
	}

	private static string BuildHtml() => HtmlTemplate.Replace("/*__DATA__*/", JsonSerializer.Serialize(BuildPayload()).Replace("<", "\\u003c"));

	[GeneratedRegex(@"\b(?:class|record|struct|enum)\s+([A-Za-z_]\w*)")]
	private static partial Regex TypeDeclRegex();

	private const string HtmlTemplate = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>SinaMN75 Models</title>
<style>
  :root { --bg:#0f1419; --panel:#1a1f29; --panel2:#141922; --border:#2a3140; --text:#e6e9ef; --muted:#8b93a3; --accent:#4fa3ff; --accent2:#2d3648; }
  * { box-sizing:border-box; }
  body { margin:0; font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif; background:var(--bg); color:var(--text); height:100vh; display:flex; flex-direction:column; }
  header { padding:14px 20px; border-bottom:1px solid var(--border); display:flex; align-items:center; gap:14px; background:var(--panel2); }
  header h1 { font-size:16px; margin:0; font-weight:600; }
  header .sub { color:var(--muted); font-size:12px; }
  .wrap { flex:1; display:flex; min-height:0; }
  aside { width:280px; border-right:1px solid var(--border); overflow-y:auto; background:var(--panel2); }
  aside .search { padding:10px; position:sticky; top:0; background:var(--panel2); border-bottom:1px solid var(--border); }
  aside input { width:100%; padding:8px 10px; border-radius:6px; border:1px solid var(--border); background:var(--bg); color:var(--text); font-size:13px; outline:none; }
  .group-title { padding:12px 14px 4px; font-size:11px; text-transform:uppercase; letter-spacing:.06em; color:var(--muted); }
  .item { padding:6px 14px; font-size:13px; cursor:pointer; color:var(--text); border-left:3px solid transparent; }
  .item:hover { background:var(--panel); }
  .item.active { background:var(--accent2); border-left-color:var(--accent); color:#fff; }
  main { flex:1; display:flex; flex-direction:column; min-width:0; }
  .tabs { display:flex; gap:2px; padding:12px 20px 0; border-bottom:1px solid var(--border); flex-wrap:wrap; }
  .tab { padding:8px 16px; font-size:13px; cursor:pointer; color:var(--muted); border:1px solid transparent; border-bottom:none; border-radius:6px 6px 0 0; }
  .tab:hover { color:var(--text); }
  .tab.active { color:#fff; background:var(--panel); border-color:var(--border); }
  .codebar { display:flex; align-items:center; justify-content:space-between; padding:12px 20px 0; }
  .codebar .name { font-size:15px; font-weight:600; }
  .copy { padding:6px 12px; font-size:12px; border-radius:6px; border:1px solid var(--border); background:var(--panel); color:var(--text); cursor:pointer; }
  .copy:hover { border-color:var(--accent); color:var(--accent); }
  .codewrap { flex:1; overflow:auto; padding:12px 20px 24px; }
  pre { margin:0; background:var(--panel); border:1px solid var(--border); border-radius:8px; padding:16px; font-size:13px; line-height:1.55; font-family:"SF Mono",Menlo,Consolas,monospace; white-space:pre; overflow:auto; }
  .empty { color:var(--muted); padding:40px 20px; }
</style>
</head>
<body>
<header>
  <h1>SinaMN75 Models</h1>
  <span class="sub">Dart · Kotlin · Java · C# · TypeScript</span>
</header>
<div class="wrap">
  <aside>
    <div class="search"><input id="search" placeholder="Filter files..." autocomplete="off"></div>
    <div id="list"></div>
  </aside>
  <main>
    <div class="codebar"><span class="name" id="typeName">Select a file</span><button class="copy" id="copyBtn" style="display:none">Copy</button></div>
    <div class="tabs" id="tabs"></div>
    <div class="codewrap"><pre id="code" class="empty">Pick a file from the left to see its models.</pre></div>
  </main>
</div>
<script>
  const DATA = /*__DATA__*/;
  const LANGS = [["dart","Dart"],["kotlin","Kotlin"],["java","Java"],["csharp","C#"],["typescript","TypeScript"]];
  const GROUP_ORDER = ["Params","Responses","Models","Enums"];
  const byName = {};
  for (const s of DATA.sections) byName[s.name] = s;
  let current = null;
  let lang = "dart";

  const listEl = document.getElementById("list");
  const tabsEl = document.getElementById("tabs");
  const codeEl = document.getElementById("code");
  const nameEl = document.getElementById("typeName");
  const copyBtn = document.getElementById("copyBtn");
  const searchEl = document.getElementById("search");

  function sectionCode(name) {
    const s = byName[name];
    return s.items.map(n => DATA.types[n][lang]).join("\n\n");
  }

  function matches(s, f) {
    if (!f) return true;
    if (s.name.toLowerCase().includes(f)) return true;
    return s.items.some(n => n.toLowerCase().includes(f));
  }

  function renderList() {
    const f = searchEl.value.toLowerCase();
    listEl.innerHTML = "";
    for (const g of GROUP_ORDER) {
      const secs = DATA.sections.filter(s => s.group === g && matches(s, f));
      if (!secs.length) continue;
      const t = document.createElement("div");
      t.className = "group-title";
      t.textContent = g;
      listEl.appendChild(t);
      for (const s of secs) {
        const d = document.createElement("div");
        d.className = "item" + (s.name === current ? " active" : "");
        d.textContent = s.name;
        d.onclick = () => select(s.name);
        listEl.appendChild(d);
      }
    }
  }

  function renderTabs() {
    tabsEl.innerHTML = "";
    for (const [id, label] of LANGS) {
      const t = document.createElement("div");
      t.className = "tab" + (id === lang ? " active" : "");
      t.textContent = label;
      t.onclick = () => { lang = id; renderTabs(); renderCode(); };
      tabsEl.appendChild(t);
    }
  }

  function renderCode() {
    if (!current) return;
    codeEl.className = "";
    codeEl.textContent = sectionCode(current);
  }

  function select(name) {
    current = name;
    nameEl.textContent = name;
    copyBtn.style.display = "";
    renderList();
    renderCode();
  }

  copyBtn.onclick = () => {
    if (!current) return;
    navigator.clipboard.writeText(sectionCode(current));
    copyBtn.textContent = "Copied";
    setTimeout(() => copyBtn.textContent = "Copy", 1200);
  };

  searchEl.oninput = renderList;

  renderTabs();
  renderList();
</script>
</body>
</html>
""";
}
