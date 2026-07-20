namespace SinaMN75U.Utils;

public static class ModelCodeGenerator {
	private static readonly Assembly Asm = typeof(BaseParams).Assembly;

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

	private static readonly string[] Langs = ["dart", "kotlin", "java", "csharp", "typescript"];

	private sealed class Prop {
		public required string Pascal;
		public required Type Clr;
		public required bool Nullable;
	}

	private sealed class Model {
		public required string Name;
		public required string Group; // Params | Responses | Enums | Shared
		public bool IsEnum;
		public List<Prop> Props = [];
		public List<KeyValuePair<string, long>> EnumValues = [];
	}

	public static void MapUModelsPage(this WebApplication app) {
		app.MapGet("/models", () => Results.Content(BuildHtml(), "text/html; charset=utf-8"));
		app.MapGet("/models/json", () => Results.Content(JsonSerializer.Serialize(BuildPayload()), "application/json; charset=utf-8"));
	}

	private static bool DerivesFromBaseParams(Type t) => typeof(BaseParams).IsAssignableFrom(t) && t != typeof(BaseParams);

	private static bool DerivesFromBaseResponse(Type t) {
		for (Type? b = t.BaseType; b != null; b = b.BaseType)
			if (b.IsGenericType && b.GetGenericTypeDefinition() == typeof(BaseResponse<,>))
				return true;
		return false;
	}

	private static bool ShouldInclude(Type t) =>
		t.Assembly == Asm && t != typeof(string) && (t.IsEnum || (t.IsClass && !t.IsGenericTypeDefinition));

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

	private static List<Model> Collect() {
		List<Type> roots = Asm.GetTypes()
			.Where(t => t.IsClass && !t.IsGenericTypeDefinition && (DerivesFromBaseParams(t) || DerivesFromBaseResponse(t)))
			.ToList();

		NullabilityInfoContext nic = new();
		Dictionary<Type, Model> result = new();
		HashSet<Type> seen = [];
		Queue<Type> queue = new();
		foreach (Type r in roots) {
			if (seen.Add(r)) queue.Enqueue(r);
		}

		while (queue.Count > 0) {
			Type t = queue.Dequeue();

			if (t.IsEnum) {
				Model em = new() { Name = t.Name, Group = "Enums", IsEnum = true };
				string[] names = t.GetEnumNames();
				Array values = t.GetEnumValues();
				for (int i = 0; i < names.Length; i++)
					em.EnumValues.Add(new KeyValuePair<string, long>(names[i], Convert.ToInt64(values.GetValue(i))));
				result[t] = em;
				continue;
			}

			string group = DerivesFromBaseParams(t) ? "Params" : DerivesFromBaseResponse(t) ? "Responses" : "Shared";
			Model m = new() { Name = t.Name, Group = group };
			HashSet<string> propSeen = [];

			foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
				if (p.GetIndexParameters().Length > 0) continue;
				if (!propSeen.Add(p.Name)) continue;

				bool nullable = Nullable.GetUnderlyingType(p.PropertyType) != null || nic.Create(p).ReadState == NullabilityState.Nullable;
				m.Props.Add(new Prop { Pascal = p.Name, Clr = p.PropertyType, Nullable = nullable });

				foreach (Type leaf in LeafTypes(p.PropertyType))
					if (ShouldInclude(leaf) && seen.Add(leaf))
						queue.Enqueue(leaf);
			}

			result[t] = m;
		}

		return result.Values.ToList();
	}

	private static string PrimName(Type u, int langIdx) {
		string cat;
		if (u == typeof(string) || u == typeof(char)) cat = "string";
		else if (u == typeof(Guid)) cat = "guid";
		else if (u == typeof(bool)) cat = "bool";
		else if (u == typeof(long) || u == typeof(ulong)) cat = "long";
		else if (u == typeof(byte) || u == typeof(sbyte) || u == typeof(short) || u == typeof(ushort) || u == typeof(int) || u == typeof(uint)) cat = "int";
		else if (u == typeof(float) || u == typeof(double) || u == typeof(decimal)) cat = "double";
		else if (u == typeof(DateTime) || u == typeof(DateTimeOffset) || u == typeof(TimeSpan) || u == typeof(DateOnly) || u == typeof(TimeOnly)) cat = "datetime";
		else cat = "object";
		return Prim[cat][langIdx];
	}

	private static string Collection(string inner, int langIdx) => langIdx == 4 ? $"{inner}[]" : $"List<{inner}>";

	private static string RenderType(Type t, int langIdx) {
		Type u = Nullable.GetUnderlyingType(t) ?? t;
		if (u == typeof(byte[])) return Prim["string"][langIdx];
		if (u.IsArray) return Collection(RenderType(u.GetElementType()!, langIdx), langIdx);
		if (u.IsGenericType) {
			Type[] args = u.GetGenericArguments();
			if (args.Length == 1 && typeof(IEnumerable).IsAssignableFrom(u)) return Collection(RenderType(args[0], langIdx), langIdx);
			if (args.Length == 2 && typeof(IEnumerable).IsAssignableFrom(u)) return RenderMap(args[0], args[1], langIdx);
			return RenderType(args[0], langIdx);
		}
		if (u.IsEnum) return u.Name;
		return PrimName(u, langIdx);
	}

	private static string RenderMap(Type k, Type v, int langIdx) {
		string kk = RenderType(k, langIdx);
		string vv = RenderType(v, langIdx);
		return langIdx switch {
			0 => $"Map<{kk}, {vv}>",
			1 => $"Map<{kk}, {vv}>",
			2 => $"Map<{kk}, {vv}>",
			3 => $"Dictionary<{kk}, {vv}>",
			_ => $"Record<{kk}, {vv}>"
		};
	}

	private static string Camel(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];

	private static string GenDart(Model m) {
		if (m.IsEnum) {
			string body = string.Join("\n", m.EnumValues.Select(e => $"  {Camel(e.Key)}({e.Value}),"));
			return $"enum {m.Name} {{\n{body}\n  ;\n\n  final int value;\n  const {m.Name}(this.value);\n}}";
		}
		StringBuilder sb = new();
		sb.Append($"class {m.Name} {{\n");
		foreach (Prop p in m.Props) sb.Append($"  final {RenderType(p.Clr, 0)}{(p.Nullable ? "?" : "")} {Camel(p.Pascal)};\n");
		sb.Append($"\n  const {m.Name}({{\n");
		foreach (Prop p in m.Props) sb.Append($"    {(p.Nullable ? "" : "required ")}this.{Camel(p.Pascal)},\n");
		sb.Append("  });\n}");
		return sb.ToString();
	}

	private static string GenKotlin(Model m) {
		if (m.IsEnum) {
			string body = string.Join("\n", m.EnumValues.Select(e => $"    {e.Key}({e.Value}),"));
			return $"enum class {m.Name}(val value: Int) {{\n{body}\n}}";
		}
		string fields = string.Join("\n", m.Props.Select(p => $"    val {Camel(p.Pascal)}: {RenderType(p.Clr, 1)}{(p.Nullable ? "? = null" : "")},"));
		return $"data class {m.Name}(\n{fields}\n)";
	}

	private static string GenJava(Model m) {
		if (m.IsEnum) {
			string body = string.Join("\n", m.EnumValues.Select(e => $"    {e.Key}({e.Value}),"));
			return $"public enum {m.Name} {{\n{body}\n    ;\n\n    public final int value;\n    {m.Name}(int value) {{ this.value = value; }}\n}}";
		}
		string fields = string.Join(",\n", m.Props.Select(p => $"    {RenderType(p.Clr, 2)} {Camel(p.Pascal)}"));
		return $"public record {m.Name}(\n{fields}\n) {{}}";
	}

	private static string GenCsharp(Model m) {
		if (m.IsEnum) {
			string body = string.Join("\n", m.EnumValues.Select(e => $"    {e.Key} = {e.Value},"));
			return $"public enum {m.Name} {{\n{body}\n}}";
		}
		StringBuilder sb = new();
		sb.Append($"public class {m.Name} {{\n");
		foreach (Prop p in m.Props) sb.Append($"    public {RenderType(p.Clr, 3)}{(p.Nullable ? "?" : "")} {p.Pascal} {{ get; set; }}\n");
		sb.Append('}');
		return sb.ToString();
	}

	private static string GenTs(Model m) {
		if (m.IsEnum) {
			string body = string.Join("\n", m.EnumValues.Select(e => $"  {e.Key} = {e.Value},"));
			return $"export enum {m.Name} {{\n{body}\n}}";
		}
		string fields = string.Join("\n", m.Props.Select(p => $"  {Camel(p.Pascal)}{(p.Nullable ? "?" : "")}: {RenderType(p.Clr, 4)};"));
		return $"export interface {m.Name} {{\n{fields}\n}}";
	}

	private static object BuildPayload() {
		List<Model> models = Collect();
		Dictionary<string, Dictionary<string, string>> types = new();
		foreach (Model m in models)
			types[m.Name] = new Dictionary<string, string> {
				["dart"] = GenDart(m),
				["kotlin"] = GenKotlin(m),
				["java"] = GenJava(m),
				["csharp"] = GenCsharp(m),
				["typescript"] = GenTs(m)
			};

		string[] order = ["Params", "Responses", "Shared", "Enums"];
		List<object> groups = order
			.Select(g => new {
				name = g,
				items = models.Where(m => m.Group == g).Select(m => m.Name).OrderBy(x => x, StringComparer.Ordinal).ToList()
			})
			.Where(g => g.items.Count > 0)
			.Cast<object>()
			.ToList();

		return new { groups, types };
	}

	private static string BuildHtml() {
		string json = JsonSerializer.Serialize(BuildPayload()).Replace("<", "\\u003c");
		return HtmlTemplate.Replace("/*__DATA__*/", json);
	}

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
  aside { width:300px; border-right:1px solid var(--border); overflow-y:auto; background:var(--panel2); }
  aside .search { padding:10px; position:sticky; top:0; background:var(--panel2); border-bottom:1px solid var(--border); }
  aside input { width:100%; padding:8px 10px; border-radius:6px; border:1px solid var(--border); background:var(--bg); color:var(--text); font-size:13px; outline:none; }
  .group-title { padding:10px 14px 4px; font-size:11px; text-transform:uppercase; letter-spacing:.06em; color:var(--muted); }
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
    <div class="search"><input id="search" placeholder="Filter types..." autocomplete="off"></div>
    <div id="list"></div>
  </aside>
  <main>
    <div class="codebar"><span class="name" id="typeName">Select a type</span><button class="copy" id="copyBtn" style="display:none">Copy</button></div>
    <div class="tabs" id="tabs"></div>
    <div class="codewrap"><pre id="code" class="empty">Pick a type from the left to see its models.</pre></div>
  </main>
</div>
<script>
  const DATA = /*__DATA__*/;
  const LANGS = [["dart","Dart"],["kotlin","Kotlin"],["java","Java"],["csharp","C#"],["typescript","TypeScript"]];
  let current = null;
  let lang = "dart";

  const listEl = document.getElementById("list");
  const tabsEl = document.getElementById("tabs");
  const codeEl = document.getElementById("code");
  const nameEl = document.getElementById("typeName");
  const copyBtn = document.getElementById("copyBtn");
  const searchEl = document.getElementById("search");

  function renderList(filter) {
    listEl.innerHTML = "";
    const f = (filter || "").toLowerCase();
    for (const g of DATA.groups) {
      const items = g.items.filter(n => n.toLowerCase().includes(f));
      if (!items.length) continue;
      const t = document.createElement("div");
      t.className = "group-title";
      t.textContent = g.name;
      listEl.appendChild(t);
      for (const n of items) {
        const d = document.createElement("div");
        d.className = "item" + (n === current ? " active" : "");
        d.textContent = n;
        d.onclick = () => select(n);
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
    codeEl.textContent = DATA.types[current][lang];
  }

  function select(n) {
    current = n;
    nameEl.textContent = n;
    copyBtn.style.display = "";
    renderList(searchEl.value);
    renderCode();
  }

  copyBtn.onclick = () => {
    if (!current) return;
    navigator.clipboard.writeText(DATA.types[current][lang]);
    copyBtn.textContent = "Copied";
    setTimeout(() => copyBtn.textContent = "Copy", 1200);
  };

  searchEl.oninput = () => renderList(searchEl.value);

  renderTabs();
  renderList("");
</script>
</body>
</html>
""";
}
