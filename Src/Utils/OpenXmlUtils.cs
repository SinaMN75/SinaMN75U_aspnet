using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;

namespace SinaMN75U.Utils;

public class WordPdfGenerator {
	public static async Task<string> GenerateWithTextsAsync(Dictionary<string, string> texts, string templatePath) {
		(string docx, string pdf) = CreateTempFiles(templatePath);

		FillTexts(docx, texts);
		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	public static async Task<string> GenerateWithImagesAsync(Dictionary<string, string> imagesBase64, string templatePath) {
		(string docx, string pdf) = CreateTempFiles(templatePath);

		InsertImages(docx, imagesBase64);
		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	public static async Task<string> GenerateWithTextsAndImagesAsync(Dictionary<string, string> texts, Dictionary<string, string> imagesBase64,
		string templatePath) {
		(string docx, string pdf) = CreateTempFiles(templatePath);

		FillTexts(docx, texts);
		InsertImages(docx, imagesBase64);

		ConvertToPdf(docx, pdf);

		return await ToBase64AndCleanup(docx, pdf);
	}

	private static (string docx, string pdf) CreateTempFiles(string templatePath) {
		string tempDocx = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", $"{Guid.NewGuid()}.docx");
		string tempPdf = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", $"{Guid.NewGuid()}.pdf");

		File.Copy(templatePath, tempDocx, true);

		return (tempDocx, tempPdf);
	}

	private static async Task<string> ToBase64AndCleanup(string docx, string pdf) {
		byte[] bytes = await File.ReadAllBytesAsync(pdf);
		string base64 = Convert.ToBase64String(bytes);

		File.Delete(docx);
		File.Delete(pdf);

		return base64;
	}

	private static void FillTexts(string filePath, Dictionary<string, string> values) {
		using WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true);
		IEnumerable<SdtElement> sdtElements = doc.MainDocumentPart!.Document!.Descendants<SdtElement>();

		foreach (SdtElement sdt in sdtElements) {
			string? tag = sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;

			if (tag == null || !values.ContainsKey(tag)) continue;

			Text? textElement = sdt.Descendants<Text>().FirstOrDefault();
			textElement?.Text = values[tag];
		}

		doc.MainDocumentPart.Document.Save();
	}

	private static void InsertImages(string filePath, Dictionary<string, string> images) {
		using WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true);
		MainDocumentPart? mainPart = doc.MainDocumentPart;

		foreach (KeyValuePair<string, string> pair in images) {
			SdtElement? sdt = doc.MainDocumentPart!.Document!.Descendants<SdtElement>().FirstOrDefault(s => s.SdtProperties?.GetFirstChild<Tag>()?.Val == pair.Key);

			if (sdt == null) continue;

			byte[] imageBytes = Convert.FromBase64String(pair.Value);

			ImagePart imagePart = mainPart!.AddImagePart(ImagePartType.Png);
			using MemoryStream stream = new(imageBytes);
			imagePart.FeedData(stream);

			Drawing drawing = CreateImage(mainPart!.GetIdOfPart(imagePart));

			sdt.RemoveAllChildren<SdtContentRun>();
			sdt.AppendChild(new SdtContentRun(new Run(drawing)));
		}

		doc.MainDocumentPart!.Document!.Save();
	}

	private static Drawing CreateImage(string relId) => new(
		new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
			new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent { Cx = 990000L, Cy = 792000L },
			new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties { Id = 1U, Name = "Image" },
			new DocumentFormat.OpenXml.Drawing.Graphic(
				new DocumentFormat.OpenXml.Drawing.GraphicData(
					new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
						new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
							new DocumentFormat.OpenXml.Drawing.Blip { Embed = relId }
						)
					)
				)
			)
		)
	);

	private static void ConvertToPdf(string inputDocx, string outputPdf) {
		using WordDocument wordDoc = new(inputDocx, FormatType.Docx);
		using DocIORenderer renderer = new();
		PdfDocument? pdf = renderer.ConvertToPDF(wordDoc);
		using FileStream stream = new(outputPdf, FileMode.Create);
		pdf.Save(stream);
		pdf.Close(true);
	}
}