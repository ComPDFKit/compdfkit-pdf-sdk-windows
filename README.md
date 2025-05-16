# Introduction
[ComPDFKit PDF SDK](https://www.compdf.com) is a robust PDF library, which offers comprehensive functions for quickly viewing, annotating, editing, and signing PDFs. It is feature-rich and battle-tested, making PDF files process and manipulation easier and faster.

[ComPDFKit for Windows](https://www.compdf.com/windows) allows you to quickly add PDF functions to any Windows application, elevating your Window programs to ensure efficient development. It is available at [nuget](https://www.nuget.org/packages/ComPDFKit.NetFramework) and [github.com](https://github.com/ComPDFKit/compdfkit-pdf-sdk-windows).

# Related

- [ComPDFKit API Library for .Net](https://github.com/ComPDFKit/compdfkit-api-.net)
- Download [ComPDFKit PDF SDK for .NET](https://www.nuget.org/packages/ComPDFKit.NetFramework) in Nuget
- [How to Build a Windows PDF Viewer or Editor](https://www.compdf.com/blog/build-a-windows-pdf-viewer-or-editor)
- [Brief Introduction to ComPDFKit for Windows](https://www.compdf.com/blog/compdfkit-for-windows)

# Get Started

It is easy to embed ComPDFKit PDF SDK in your Windows application with a few lines of C# code. The following sections introduce the requirements, the structure of the installation package, and how to make a Windows PDF Reader in C# with ComPDFKit PDF SDK. Take just a few minutes and get started.


## Requirements

- Windows 7, 8, 10, and 11 (32-bit and 64-bit).
- Visual Studio 2017 or higher (Make sure the **.NET Desktop Development** is installed).
- .NET Framework 4.5 or higher.

## How to Run a Demo
[ComPDFKit PDF SDK for Windows](https://www.compdf.com/guides/pdf-sdk/windows/overview) provides multiple demos in C# for developers to learn how to call the SDK on Windows. You can find them in the ***"Examples"*** folder.

In this guide, we take ***"PDFViewer"*** as an example to show how to run it in Visual Studio 2022.

1. Copy your ***"license_key_windows.xml"*** to the ***"Examples"*** folder (The file is the license to make your project run).

2. Find ***"Examples.sln"*** in the ***"Examples"*** folder and open it in Visual Studio 2022.

   <img src="images-windows/imagev2.png" alt="2.4" width="50%" height="50%"/>

3. Select ***"PDFViewer"*** and right-click to set it as a startup project.

   <img src="images-windows/image-2v2.png" alt="2.4" width="50%" height="50%"/>

4. Run the project and then you can open the multifunctional ***"PDFViewer"*** demo.

   <img src="images-windows/image-1.png" alt="2.4" width="50%" height="50%"/>

**Note:** *This is a demo project, presenting completed [ComPDFKit PDF SDK](https://www.compdf.com/pdf-sdk) functions. The functions might be different based on the license you have purchased. Please check that the functions you choose work fine in this demo project.*



## How to Make a Windows Program in C#

### Create a New Project

1. Open Visual Studio 2022, and click **Create a new project**.

   <img src="images-windows/vs2022_1.png" alt="2.4" width="50%" height="50%"/>

2. Choose **WPF App (.NET Framework)** and click **Next**.

   <img src="images-windows/vs2022_2.png" alt="2.4" width="50%" height="50%"/>

3. Configure your project: Set your project name and choose the location to store your program. The project name is called "ComPDFKit Demo" in this example. This sample project uses .NET Framework 4.6.1 as the programming framework.

   <img src="images-windows/vs2022_3.png" alt="2.4" width="50%" height="50%"/>

4. Click the **Create** button. Then, the new project will be created.



### Add ComPDFKit to Your Project

There are two ways to add ComPDFKit to your Project: [Nuget Repository](https://www.nuget.org/packages/ComPDFKit.NetFramework) and Local Package, you can choose one or the other according to your needs.

**Nuget Repository**

1. Open your project's solution, and in the Solution Explorer, right-click on **References** and click on the menu item **Manage NuGet Packages**. This will open the NuGet Package Manager for your solution.

   <img src="images-windows/2.4.2.1.png" alt="2.4" width="35%" height="35%"/>

2. Go to [ComPDFKit.NetFramework](https://www.nuget.org/packages/ComPDFKit.NetFramework) in Nuget, and click on the **Install** button to install the package.

   <img src="images-windows/2.4.2.3v2.png" alt="2.4" width="65%" height="65%"/>

3. Once that is complete, you'll see a reference to the package in the Solution Explorer under **References**.

   <img src="images-windows/2.4.2.4.png" alt="2.4" width="35%" height="35%"/>



**Local Package**

Rather than targeting a package held at Nuget, you may set up a configuration to point to a local package. This can be useful for some situations, for example, your build machines don't have access to the internet.

1. You can find ***"ComPDFKit.NetFramework....nupkg"*** file in the [SDK Package](https://github.com/ComPDFKit/compdfkit-pdf-sdk-windows/tree/Tutorials-Blog/ComPDFKit%201.11.0%20for%20Windows/nuget)

2. Create or edit a ***"nuget.config"*** file in the same directory as your solution file (e.g. ***"ComPDFKit Demo.sln"***).

<img src="images-windows/2.4.2.5.png" alt="2.4" width="50%" height="50%"/>

- The contents of the file should contain an XML element, _packageSources_ - which describes where to find NuGet packages - as a child of a root node named _configuration_. If the file already exists, add the extra _packageSources_ entry shown below. If the file is blank, copy and paste the entirety of the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="ComPDFKitSource" value="path\to\directoryContainingNupkg" />
    </packageSources>
</configuration>
```

- Edit the _value_ of the contents to correctly refer to the location of the directory containing the ***"ComPDFKit.NetFramework....nupkg"*** package - for example, C:\Users\me\nugetPackages\. Now save the file, and close and reopen your solution for Visual Studio to force a read of the NuGet configuration.

3. Open your project's solution, and in the Solution Explorer, right-click on **References** and click on the menu item **Manage NuGet Packages…**. This will open the NuGet Package Manager for your solution.

   <img src="images-windows/2.4.2.6.png" alt="2.4" width="50%" height="50%"/>

4. On the right-hand side of the manager in the Package source dropdown window, choose the entry _ComPDFKitSource_ (or whatever you decided to name it). You should then see the entry for ***"ComPDFKit.NetFramework"***.

   <img src="images-windows/2.4.2.7.png" alt="2.4" width="50%" height="50%"/>

5. On the right side, in the panel describing the package, click on the **Install** button to install the package.

   <img src="images-windows/2.4.2.8v2.png" alt="2.4" width="50%" height="50%"/>

6. Once that's complete, you'll see a reference to the package in the Solution Explorer under **References**.

   <img src="images-windows/2.4.2.9.png" alt="2.4" width="50%" height="50%"/>

### Apply the License Key

You can [contact the ComPDFKit team](https://www.compdf.com/contact-us) to obtain a trial license. Before using any classes from the ComPDFKit PDF SDK, you need to choose the corresponding scheme from the following two options based on the license type and apply the license to your application.

**Online Authentication**

You can perform online authentication using the following approach:

```c#
public static bool LicenseVerify()
{
	if (!CPDFSDKVerifier.LoadNativeLibrary())
	{ 
		return false;
	}
	LicenseErrorCode status = CPDFSDKVerifier.OnlineLicenseVerify("Input your license here.");
	return status == LicenseErrorCode.E_LICENSE_SUCCESS;
}
```

Additionally, if you need to confirm the communication status with the server during online authentication, you can implement the `CPDFSDKVerifier.LicenseRefreshed` callback:

```C#
CPDFSDKVerifier.LicenseRefreshed += CPDFSDKVerifier_LicenseRefreshed;

private void CPDFSDKVerifier_LicenseRefreshed(object sender, ResponseModel e)
{
    if(e != null)
    {
        string message = string.Format("{0} {1}", e.Code, e.Message);
        Trace.WriteLine(message);
    }
    else
    {
        Trace.WriteLine("Network not connected."); 
    } 
}

```

**Offline Authentication** 

The following is the `LicenseVerify()` method for implementing offline authentication:

```c#
bool LicenseVerify()
{
    if (!CPDFSDKVerifier.LoadNativeLibrary())
        return false;

    LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify("Input your license here.", false);
    return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
}
```

### Display a PDF Document

We have finished all prepare steps. Let's display a PDF file.

1. Add the following code to ***"MainWindow.xaml"*** and ***"MainWindow.xaml.cs"*** to display a PDF document. Please make sure to replace "ComPDFKit_Demo" with the name of your project. Now, all you need to do is to create a `CPDFViewer` object, and then display the `CPDFViewer` object in the Grid (component) named "PDFGrid" using the `OpenPDF_Click` method.

2. Now your ***"MainWindow.xaml"*** should look like the following code.

```c#
<Window x:Class="ComPDFKit_Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:ComPDFKit_Demo"
        xmlns:compdfkitviewer="clr-namespace:ComPDFKitViewer;assembly=ComPDFKit.Viewer"
        mc:Ignorable="d"
        Focusable="True"
        Title="MainWindow" Height="600" Width="800" UseLayoutRounding="True">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="52"/>
        </Grid.RowDefinitions>
        <Grid Name="PDFGrid" Grid.Row="0">
            <ScrollViewer Focusable="False" CanContentScroll="True" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto">
                <compdfkitviewer:CPDFViewer x:Name="PDFViewer"/>
            </ScrollViewer>
        </Grid>
        <Button Content="Open PDF" Grid.Row="1" HorizontalAlignment="Left" Margin="10" Click="OpenPDF_Click"/>
    </Grid>
</Window>
```

3. Now your ***“MainWindow.xaml.cs”*** should look like the following code. Please note: You need to enter your license key. All the places that need to be modified in the code have been marked with comments in the code below. You just need to replace the string content below the comments by yourself.

```c#
using ComPDFKit.NativeMethod;
using ComPDFKit.PDFDocument;
using Microsoft.Win32;
using System.Windows;

namespace ComPDFKit_Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LicenseVerify();
        }

        bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
                return false;

            // Input your license.
            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify("Input your license here.");
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }

        private void OpenPDF_Click(object sender, RoutedEventArgs e)
        {
            // Get the path of a PDF file.
            var dlg = new OpenFileDialog();
            dlg.Filter = "PDF Files (*.pdf)|*.pdf";
            if (dlg.ShowDialog() == true)
            {
                // Use the PDF file path to open the document in CPDFViewer.
                CPDFDocument doc = CPDFDocument.InitWithFilePath(dlg.FileName);
                if (doc != null && doc.ErrorType == CPDFDocumentError.CPDFDocumentErrorSuccess)
                    PDFViewer.InitDoc(doc);
            }
        }
    }
}
```

4. Now run the project and you will see the PDF file that you want to display.  The PDF Viewer has been created successfully.

<img src="images-windows/2.4.4.png" alt="2.4" width="50%" height="50%"/>



### Troubleshooting

1. License Verification Failed
- If "System.IO.FileNotFoundException" occurred in the `LicenseVerify()` function like this:

<img src="images-windows/vs2022_4.png" alt="2.4" width="50%" height="50%"/>

- Check your WPF project and ensure that you chose **WPF App(.NET Framework)** instead of **WPF Application** when creating the project.

<img src="images-windows/vs2022_5.png" alt="2.4" width="50%" height="50%"/>

2. Other Problems

   If you meet some other problems when integrating our ComPDFKit PDF SDK for Windows, feel free to contact [our support team](https://www.compdf.com/support).


# Samples

The Samples use preset parameters and documentation to call the [API of ComPDFKit](https://api.compdf.com/) for each function without UI interaction or parameter settings. They not only demonstrate the best practices for each function but also provide detailed introductions. The impact of each function on PDF documents can be observed in the output directory. With the help of the Samples, you can quickly learn how to use the functions you need and apply them to your projects.

You can get our [code examples for Windows](https://www.compdf.com/guides/pdf-sdk/windows/examples) on our website. To learn more about the ComPDFKit API, please visit our [API Reference](https://developers.compdf.com/guides/pdf-sdk/windows/api-reference/index).

# Support

[ComPDFKit]((https://www.compdf.com)) has a professional R&D team that produces comprehensive technical documentation and guides to help developers. Also, you can get an immediate response when reporting your problems to our support team.

- For detailed information, please visit our [Guides](https://www.compdf.com/guides/pdf-sdk/windows/overview) page.

- Stay updated with the latest improvements through our [Changelog](https://www.compdf.com/pdf-sdk/changelog-windows).

- For technical assistance, please reach out to our [Technical Support](https://www.compdf.com/support).

- To get more details and an accurate quote, please contact our [Sales Team](https://compdf.com/contact-us).




# License

ComPDFKit PDF SDK supports flexible licensing options, please contact [our sales team](mailto:support@compdf.com) to know more. Each license is only valid for one application ID in development mode.  However, any documents, sample code, or source code distribution from the released package of ComPDFKit PDF SDK to any third party is prohibited.



# Note

We are glad to announce that you can register a ComPDFKit API account for a [free trial](https://api.compdf.com/api/pricing) to process 1000 documents per month for free.

