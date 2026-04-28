using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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

	// private static void ConvertToPdf(string inputDocx, string outputPdf) {
	// 	using WordDocument wordDoc = new(inputDocx, FormatType.Docx);
	// 	using DocIORenderer renderer = new();
	// 	PdfDocument? pdf = renderer.ConvertToPDF(wordDoc);
	// 	using FileStream stream = new(outputPdf, FileMode.Create);
	// 	pdf.Save(stream);
	// 	pdf.Close(true);
	// }

// Install-Package PdfSharp

	private static void ConvertToPdf(string inputDocx, string outputPdf) {
		// Extract text from DOCX
		string text = ExtractTextFromDocx(inputDocx);

		// Create PDF document
		using (PdfDocument document = new()) {
			// Add a page - CORRECTED API usage
			PdfPage page = document.AddPage();

			// Set page size (optional - defaults to A4/Letter)
			// page.Size = PageSize.;

			// Create graphics object for drawing
			using (XGraphics gfx = XGraphics.FromPdfPage(page)) {
				// Define font and formatting
				XFont font = new("Arial", 12);
				XBrush brush = XBrushes.Black;

				// Create rectangle for text positioning
				// FIXED: Use .Point property to get double value (modern PDFsharp)
				XRect rect = new(
					40, // X position
					40, // Y position  
					page.Width.Point - 80, // Width with margins
					page.Height.Point - 80 // Height with margins
				);

				// Draw the text
				gfx.DrawString(text, font, brush, rect);
			}

			// Save the document
			document.Save(outputPdf);
		}
	}

	private static string ExtractTextFromDocx(string docxPath) {
		// Install-Package DocumentFormat.OpenXml
		using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(docxPath, false)) {
			return wordDoc.MainDocumentPart.Document.Body.InnerText;
		}
	}
}