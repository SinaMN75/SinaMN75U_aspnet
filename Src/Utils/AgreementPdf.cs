using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SinaMN75U.Utils;

public static class AgreementPdf {
	private const string FontFamily = "Vazir";
	private const string Brand = "#2f2b8f";
	private const string Ink = "#1a1a1a";
	private const string Muted = "#555555";
	private const string Line = "#c9c9d4";

	private static readonly Lazy<byte[]> Logo = new(() => ReadResource("logo.jpg"));

	private static readonly Lazy<bool> Initialized = new(() => {
		QuestPDF.Settings.License = LicenseType.Community;
		using MemoryStream font = new(ReadResource("Vazir.ttf"));
		FontManager.RegisterFontWithCustomName(FontFamily, font);
		return true;
	});

	public static byte[] Build(Dictionary<string, string> values, byte[]? signature) {
		_ = Initialized.Value;

		return Document.Create(document => {
			document.Page(page => {
				page.Size(PageSizes.A4);
				page.Margin(24);
				page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(9).LineHeight(1.6f).FontColor(Ink));
				page.Content().ContentFromRightToLeft().Column(column => {
					column.Spacing(6);
					Header(column, values);
					Body(column, values);
					Signatories(column, values, signature);
				});
				page.Footer().AlignCenter().Text(x => {
					x.DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(8).FontColor(Muted));
					x.CurrentPageNumber();
					x.Span(" / ");
					x.TotalPages();
				});
			});
		}).GeneratePdf();
	}

	private static void Header(ColumnDescriptor column, Dictionary<string, string> values) {
		column.Item().Row(row => {
			row.ConstantItem(150).Column(meta => {
				meta.Item().Text($"تاریخ : {Value(values, "day")}/{Value(values, "month")}/1405").FontSize(8).FontColor(Muted);
				meta.Item().Text($"شماره : {Value(values, "number")}/ق291").FontSize(8).FontColor(Muted);
			});
			row.RelativeItem().AlignMiddle().Text(AgreementContent.Title).FontSize(12).Bold().FontColor(Brand);
			row.ConstantItem(90).AlignLeft().Height(44).Image(Logo.Value).FitArea();
		});
		column.Item().PaddingBottom(6).LineHorizontal(2).LineColor(Brand);
	}

	private static void Body(ColumnDescriptor column, Dictionary<string, string> values) {
		foreach (AgreementBlock block in AgreementContent.Blocks) {
			string text = Fill(block.Text, values);
			switch (block.Type) {
				case AgreementBlockType.PageBreak:
					column.Item().PageBreak();
					break;
				case AgreementBlockType.Article:
					column.Item().PaddingTop(8).Text(text).FontSize(10).Bold().FontColor(Brand);
					break;
				case AgreementBlockType.ArticleCenter:
					column.Item().PaddingTop(8).AlignCenter().Text(text).FontSize(10).Bold().FontColor(Brand);
					break;
				case AgreementBlockType.Note:
					column.Item().PaddingRight(12).Text(text).FontSize(8).FontColor(Muted);
					break;
				case AgreementBlockType.NoteCenter:
					column.Item().PaddingTop(6).AlignCenter().Text(text).FontSize(8).FontColor(Muted);
					break;
				case AgreementBlockType.Sub:
					column.Item().PaddingRight(12).Text(text);
					break;
				default:
					column.Item().Text(text);
					break;
			}
		}
	}

	private static void Signatories(ColumnDescriptor column, Dictionary<string, string> values, byte[]? signature) {
		column.Item().PaddingTop(16).Row(row => {
			row.Spacing(12);
			row.RelativeItem().Border(1).BorderColor(Line).Padding(10).Column(representative => {
				representative.Item().Text("ماندگار اندیشه آوا هوشمند").Bold();
				representative.Item().PaddingTop(6).Text("مصطفی نوری");
				representative.Item().Text("مدیر عامل").FontSize(8).FontColor(Muted);
				representative.Item().PaddingTop(6).Text("حمیدرضا عرب علیدوستی");
				representative.Item().Text("رئیس هیئت مدیره").FontSize(8).FontColor(Muted);
			});
			row.RelativeItem().Border(1).BorderColor(Line).Padding(10).Column(customer => {
				customer.Item().Text("نام و نام خانوادگی پذیرنده").Bold();
				customer.Item().PaddingTop(6).Text(Value(values, "fullName"));
				customer.Item().PaddingTop(6).Text("امضا و اثر انگشت").FontSize(8).FontColor(Muted);
				if (signature == null)
					customer.Item().Height(70);
				else
					customer.Item().PaddingTop(4).Height(70).AlignCenter().Image(signature).FitArea();
			});
		});
	}

	private static string Fill(string text, Dictionary<string, string> values) =>
		Regex.Replace(text, @"\{\{([A-Za-z0-9_]+)\}\}", m => values.GetValueOrDefault(m.Groups[1].Value, "---"));

	private static string Value(Dictionary<string, string> values, string key) => values.GetValueOrDefault(key, "---");

	private static byte[] ReadResource(string endsWith) {
		Assembly assembly = typeof(AgreementPdf).Assembly;
		string name = assembly.GetManifestResourceNames().First(x => x.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase));
		using Stream stream = assembly.GetManifestResourceStream(name)!;
		using MemoryStream memory = new();
		stream.CopyTo(memory);
		return memory.ToArray();
	}
}
