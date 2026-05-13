### Syncfusion&reg; .NET Core PDF Imaging library

Syncfusion&reg; [.NET Core PDF Imaging](https://www.syncfusion.com/pdf-framework/net-core/pdf-library?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) is a .NET standard PDF library used to draw the image files to PDF in any .NET Core applications.

![.NET Core PDF Imaging](https://cdn.syncfusion.com/nuget-readme/fileformats/net-image-to-pdf.png)

[Features overview](https://www.syncfusion.com/pdf-framework/net-core/pdf-library?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [Documentation](https://help.syncfusion.com/file-formats/pdf/overview?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [API Reference](https://help.syncfusion.com/cr/file-formats/Syncfusion.Pdf.html?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [Blogs](https://www.syncfusion.com/blogs/?s=pdf?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [Support](https://www.syncfusion.com/support/directtrac/incidents/newincident?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [Forums](https://www.syncfusion.com/forums?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | [Feedback](https://www.syncfusion.com/feedback/pdf?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget).

### Key Features
* Supports drawing JPEG, PNG images to PDF document.
* Support to compress existing PDF files.
* Support to extract images from existing PDF files.
* Support to convert [PDF to PDF/A-1B](https://help.syncfusion.com/file-formats/pdf/working-with-pdf-conformance#converting-pdf-to-pdfa-1b?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) conformance.
* Support for [PDF/X1-A,PDF/A1-B,PDF/A3-B](https://help.syncfusion.com/file-formats/pdf/working-with-pdf-conformance#adding-support-for-pdfa-1b-conformance?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) and [PDF/A1-B](https://help.syncfusion.com/file-formats/pdf/working-with-pdf-conformance#adding-support-for-pdfa-3b-conformance?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) conformances.
* Support to run the PDF applications in multi-thread and its thread safe. 

### System Requirements

*	[System Requirements](https://help.syncfusion.com/file-formats/installation-and-upgrade/system-requirements?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget).

### Getting Started
You can fetch the Syncfusion&reg; .NET Core PDF imaging library NuGet by simply running the command Install-Package [Syncfusion.Pdf.Imaging.Net.Core](https://www.nuget.org/packages/Syncfusion.Pdf.Imaging.Net.Core//?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) from the Package Manager Console in Visual Studio.

Try the following code example to draw image to PDF document

```csharp
//Create a new PDF document
PdfDocument doc = new PdfDocument();
//Add a page to the document
PdfPage page = doc.Pages.Add();
//Create PDF graphics for the page
PdfGraphics graphics = page.Graphics;
//Load the image from the disk
FileStream imageStream = new FileStream("Autumn Leaves.jpg", FileMode.Open, FileAccess.Read);
PdfBitmap image = new PdfBitmap(imageStream);
//Draw the image
graphics.DrawImage(image, 0, 0);
//Creating the stream object
MemoryStream stream = new MemoryStream();
//Save the document as stream
doc.Save(stream);
//Close the document
doc.Close(true);
```

### License
This is a commercial product and requires a paid license for possession or use. Syncfusion’s licensed software, including this component, is subject to the terms and conditions of [Syncfusion's EULA](https://www.syncfusion.com/eula/es?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget). You can purchase a license [here](https://www.syncfusion.com/sales/products?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) or start a free 30-day trial [here](https://www.syncfusion.com/account/manage-trials/start-trials?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget).

### About Syncfusion&reg;
Founded in 2001 and headquartered in Research Triangle Park, N.C., Syncfusion&reg; has more than 26,000+ customers and more than 1 million users, including large financial institutions, Fortune 500 companies, and global IT consultancies. 

Today, we provide 1600+ components and frameworks for web ([Blazor](https://www.syncfusion.com/blazor-components?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [ASP.NET Core](https://www.syncfusion.com/aspnet-core-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [ASP.NET MVC](https://www.syncfusion.com/aspnet-mvc-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [ASP.NET WebForms](https://www.syncfusion.com/jquery/aspnet-webforms-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [Angular](https://www.syncfusion.com/angular-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [React](https://www.syncfusion.com/react-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [Vue](https://www.syncfusion.com/vue-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), and [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget)), mobile ([Xamarin](https://www.syncfusion.com/xamarin-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), and [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget)), and desktop development ([WinForms](https://www.syncfusion.com/winforms-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [WPF](https://www.syncfusion.com/wpf-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [WinUI(Preview)](https://www.syncfusion.com/winui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) and [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget)). We provide ready-to-deploy enterprise software for dashboards, reports, data integration, and big data processing. Many customers have saved millions in licensing fees by deploying our software.

[sales@syncfusion.com](mailto:sales@syncfusion.com?Subject=Syncfusion%20ASPNET%20Core%20DocIO%20-%20NuGet) | [www.syncfusion.com](https://www.syncfusion.com?utm_source=nuget&utm_medium=listing&utm_campaign=aspnetcore-pdf-nuget) | Toll Free: 1-888-9 DOTNET
