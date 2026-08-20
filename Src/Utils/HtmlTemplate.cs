namespace SinaMN75U.Utils;

public class HtmlTemplate {
	private readonly string _content;
	private readonly Dictionary<string, string> _map = new();

	private HtmlTemplate(string content) => _content = content;

	public static HtmlTemplate FromString(string html) => new(html);
	public static async Task<HtmlTemplate> FromFile(string path, CancellationToken ct = default) => new(await File.ReadAllTextAsync(path, ct));

	public HtmlTemplate Set(string token, string? value) {
		_map[Key(token)] = WebUtility.HtmlEncode(value ?? "");
		return this;
	}

	public HtmlTemplate Set(IEnumerable<KeyValuePair<string, string>> values) {
		foreach (KeyValuePair<string, string> v in values) Set(v.Key, v.Value);
		return this;
	}

	public HtmlTemplate SetHtml(string token, string? html) {
		_map[Key(token)] = html ?? "";
		return this;
	}

	public HtmlTemplate SetImageBase64(string token, string? base64, string mime = "image/png", string? attributes = null) {
		_map[Key(token)] = base64.IsNullOrEmpty() ? "" : Img($"data:{mime};base64,{StripDataUri(base64)}", attributes);
		return this;
	}

	public HtmlTemplate SetImageBytes(string token, byte[]? bytes, string mime = "image/png", string? attributes = null) =>
		SetImageBase64(token, bytes == null ? null : Convert.ToBase64String(bytes), mime, attributes);
	
	public async Task<HtmlTemplate> SetImageFile(string token, string path, string? attributes = null, CancellationToken ct = default) =>
		SetImageBase64(token, Convert.ToBase64String(await File.ReadAllBytesAsync(path, ct)), MimeFromExtension(path), attributes);

	public HtmlTemplate SetImageUrl(string token, string? url, string? attributes = null) {
		_map[Key(token)] = url.IsNullOrEmpty() ? "" : Img(url, attributes);
		return this;
	}

	public HtmlTemplate SetFileLink(string token, byte[] bytes, string mime, string fileName, string? text = null) {
		_map[Key(token)] = $"<a href=\"data:{mime};base64,{Convert.ToBase64String(bytes)}\" download=\"{WebUtility.HtmlEncode(fileName)}\">{WebUtility.HtmlEncode(text ?? fileName)}</a>";
		return this;
	}

	public HtmlTemplate Clear(string token) {
		_map[Key(token)] = "";
		return this;
	}

	public bool RemoveUnmatchedTokens { get; set; }

	public string Render() {
		string result = _map.Aggregate(_content, (current, kv) => current.Replace(kv.Key, kv.Value));
		return RemoveUnmatchedTokens ? Regex.Replace(result, @"\{\{[A-Za-z0-9_]+\}\}", "") : result;
	}

	public byte[] RenderBytes() => Encoding.UTF8.GetBytes(Render());
	public string RenderBase64() => Convert.ToBase64String(RenderBytes());
	public async Task SaveAsync(string path, CancellationToken ct = default) => await File.WriteAllTextAsync(path, Render(), Encoding.UTF8, ct);

	private static string Key(string token) => token.StartsWith("{{") ? token : "{{" + token + "}}";

	private static string Img(string src, string? attributes) => $"<img src=\"{src}\"{(attributes.IsNullOrEmpty() ? "" : " " + attributes)}>";

	private static string StripDataUri(string s) {
		int i = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
		return i >= 0 ? s[(i + 7)..] : s;
	}

	private static string MimeFromExtension(string path) => Path.GetExtension(path).ToLowerInvariant() switch {
		".png" => "image/png",
		".jpg" or ".jpeg" => "image/jpeg",
		".gif" => "image/gif",
		".webp" => "image/webp",
		".svg" => "image/svg+xml",
		".bmp" => "image/bmp",
		_ => "application/octet-stream"
	};
}
