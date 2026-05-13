### Syncfusion&reg; .NET Metafile Renderer library

The Metafile Renderer is a .NET library used by [Syncfusion&reg; Document Processing Libraries](https://www.syncfusion.com/document-processing-libraries?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget) for converting the Metafile (EMF and WMF) to SKBitmap image in any .NET applications. 

### Key Features
* Supports converting Metafiles to image format supported by SkiaSharp.

### System Requirements

*	[System Requirements](https://help.syncfusion.com/document-processing/system-requirements?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget).

### Getting Started
You can fetch the Syncfusion&reg; .NET MetafileRenderer library NuGet by simply running `Install-Package Syncfusion.MetafileRenderer.NET` from the Package Manager Console in Visual Studio.

Try the following code example to convert the Metafile to SKBitmap image.

```csharp
//Load the Metafile.
FileStream docStream = new FileStream(Path.GetFullPath("Input.emf"), FileMode.Open, FileAccess.Read);
//Create a new instance for the MetafileRenderer
MetafileRenderer renderer = new MetafileRenderer();
//Convert the Metafile to SKBitmap Image.
SKBitmap skBitmap = renderer.ConvertToImage(docStream);
//Save the image as stream
using (SKImage skImage = SKImage.FromBitmap(skBitmap))
using (SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100))
using (FileStream stream = new FileStream(Path.GetFullPath("Output.png"), FileMode.Create, FileAccess.ReadWrite))
{
    data.SaveTo(stream);
}
```

### License

This is a commercial product and requires a paid license for possession or use. Syncfusion’s licensed software, including this component, is subject to the terms and conditions of [Syncfusion's EULA](https://www.syncfusion.com/eula/es/?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget). You can purchase a license [here](https://www.syncfusion.com/sales/products?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget) or start a free 30-day trial [here](https://www.syncfusion.com/account/manage-trials/start-trials?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget).

### About Syncfusion&reg;

Founded in 2001 and headquartered in Research Triangle Park, N.C., Syncfusion&reg; has more than 27,000+ customers and more than 1 million users, including large financial institutions, Fortune 500 companies, and global IT consultancies.
Today, we provide 1700+ components and frameworks for web ([Blazor](https://www.syncfusion.com/blazor-components?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [ASP.NET Core](https://www.syncfusion.com/aspnet-core-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [ASP.NET MVC](https://www.syncfusion.com/aspnet-mvc-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [ASP.NET Web Forms](https://www.syncfusion.com/jquery/aspnet-webforms-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Angular](https://www.syncfusion.com/angular-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [React](https://www.syncfusion.com/react-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Vue](https://www.syncfusion.com/vue-ui-components?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), and [jQuery](https://www.syncfusion.com/jquery-ui-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget)), mobile ([.NET MAUI (Preview)](https://www.syncfusion.com/maui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Xamarin](https://www.syncfusion.com/xamarin-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), and [JavaScript](https://www.syncfusion.com/javascript-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget)), and desktop development ([WinForms](https://www.syncfusion.com/winforms-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [WPF](https://www.syncfusion.com/wpf-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [WinUI](https://www.syncfusion.com/winui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [.NET MAUI (Preview)](https://www.syncfusion.com/maui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), [Flutter](https://www.syncfusion.com/flutter-widgets?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget),[Xamarin](https://www.syncfusion.com/xamarin-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget), and [UWP](https://www.syncfusion.com/uwp-ui-controls?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget)). We provide ready-to-deploy enterprise software for dashboards, reports, data integration, and big data processing. Many customers have saved millions in licensing fees by deploying our software.

[sales@syncfusion.com](mailto:sales@syncfusion.com?Subject=Syncfusion%20ASPNET%20Core%20MetafileRenderer%20-%20NuGet) | [www.syncfusion.com](https://www.syncfusion.com?utm_source=nuget&utm_medium=listing&utm_campaign=net-metafile-renderer-nuget) | Toll Free: 1-888-9 DOTNET
