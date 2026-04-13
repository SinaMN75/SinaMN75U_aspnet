namespace SinaMN75U.Utils;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;

public class WordPdfGeneratorService {
	// =========================
	// 1. TEXT ONLY
	// =========================
	public async Task<string> GenerateWithTextsAsync(
		Dictionary<string, string> texts,
		string templatePath) {
		var (docx, pdf) = CreateTempFiles(templatePath);

		FillTexts(docx, texts);
		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	// =========================
	// 2. IMAGE ONLY
	// =========================
	public async Task<string> GenerateWithImagesAsync(
		Dictionary<string, string> imagesBase64,
		string templatePath) {
		var (docx, pdf) = CreateTempFiles(templatePath);

		InsertImages(docx, imagesBase64);
		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	// =========================
	// 3. TEXT + IMAGE
	// =========================
	public async Task<string> GenerateWithTextsAndImagesAsync(
		Dictionary<string, string> texts,
		Dictionary<string, string> imagesBase64,
		string templatePath) {
		var (docx, pdf) = CreateTempFiles(templatePath);

		FillTexts(docx, texts);
		InsertImages(docx, imagesBase64);

		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	// =========================
	// INTERNALS
	// =========================

	private (string docx, string pdf) CreateTempFiles(string templatePath) {
		var tempDocx = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");
		var tempPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

		File.Copy(templatePath, tempDocx, true);

		return (tempDocx, tempPdf);
	}

	private async Task<string> ToBase64AndCleanup(string docx, string pdf) {
		var bytes = await File.ReadAllBytesAsync(pdf);
		var base64 = Convert.ToBase64String(bytes);

		File.Delete(docx);
		File.Delete(pdf);

		return base64;
	}

	// =========================
	// TEXT (SDT)
	// =========================
	private void FillTexts(string filePath, Dictionary<string, string> values) {
		using var doc = WordprocessingDocument.Open(filePath, true);

		var sdtElements = doc.MainDocumentPart.Document
			.Descendants<SdtElement>();

		foreach (var sdt in sdtElements) {
			var tag = sdt.SdtProperties?
				.GetFirstChild<Tag>()?.Val?.Value;

			if (tag == null || !values.ContainsKey(tag))
				continue;

			var textElement = sdt.Descendants<Text>().FirstOrDefault();
			if (textElement != null) {
				textElement.Text = values[tag];
			}
		}

		doc.MainDocumentPart.Document.Save();
	}

	// =========================
	// IMAGE (SDT)
	// =========================
	private void InsertImages(string filePath, Dictionary<string, string> images) {
		using var doc = WordprocessingDocument.Open(filePath, true);
		var mainPart = doc.MainDocumentPart;

		foreach (var pair in images) {
			var tagName = pair.Key;
			var base64 = pair.Value;

			var sdt = doc.MainDocumentPart.Document
				.Descendants<SdtElement>()
				.FirstOrDefault(s =>
					s.SdtProperties?.GetFirstChild<Tag>()?.Val == tagName);

			if (sdt == null) continue;

			var imageBytes = Convert.FromBase64String(base64);

			var imagePart = mainPart.AddImagePart(ImagePartType.Png);
			using var stream = new MemoryStream(imageBytes);
			imagePart.FeedData(stream);

			var drawing = CreateImage(mainPart.GetIdOfPart(imagePart));

			sdt.RemoveAllChildren<SdtContentRun>();
			sdt.AppendChild(new SdtContentRun(new Run(drawing)));
		}

		doc.MainDocumentPart.Document.Save();
	}

	private Drawing CreateImage(string relId) {
		return new Drawing(
			new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
				new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent()
					{ Cx = 990000L, Cy = 792000L },
				new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties()
					{ Id = 1U, Name = "Image" },
				new DocumentFormat.OpenXml.Drawing.Graphic(
					new DocumentFormat.OpenXml.Drawing.GraphicData(
						new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
							new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
								new DocumentFormat.OpenXml.Drawing.Blip()
									{ Embed = relId }
							)
						)
					)
				)
			)
		);
	}

	// =========================
	// PDF
	// =========================
	private void ConvertToPdf(string inputDocx, string outputPdf) {
		using var wordDoc = new WordDocument(inputDocx, FormatType.Docx);
		using var renderer = new DocIORenderer();

		var pdf = renderer.ConvertToPDF(wordDoc);

		using var stream = new FileStream(outputPdf, FileMode.Create);
		pdf.Save(stream);
		pdf.Close(true);
	}
}