# Introduction
ComPDFKit PDF SDK for Windows developed and maintained by [ComPDF](https://www.compdf.com/) that allows developers to quickly add PDF Features to any Windows applications, elevating your Windows programs to ensure efficient development. It is available at [NuGet](https://www.nuget.org/packages/ComPDFKit.NetFramework) and github.com.

More Information can be found at: [https://www.compdf.com/guides/pdf-sdk/windows/overview](https://www.compdf.com/guides/pdf-sdk/windows/overview)

# Table of Content
- [Related](#related)
- [Requirements](#requirements)
- [How to Make a Windows Program in C#](#how-to-make-a-windows-program-in-c)
- [Supports](#support)
- [License ](#license)
- [Note](#note)

# Related

- [ComPDFKit PDF SDK for Windows Documentation Guide](https://www.compdf.com/guides/pdf-sdk/windows/overview)
- [ComPDF API Library for .Net](https://github.com/ComPDFKit/compdfkit-api-.net)
- Download [ComPDFKit PDF SDK for .NET](https://www.nuget.org/packages/ComPDFKit.NetFramework) in NuGet
- [How to Build a Windows PDF Viewer or Editor](https://www.compdf.com/blog/build-a-windows-pdf-viewer-or-editor)
- [Code Samples for Windows](https://www.compdf.com/guides/pdf-sdk/windows/examples) 
- [ComPDF API Reference](https://api.compdf.com/api-reference/overview)


# Requirements

Before starting, please make sure that you have already met the following prerequisites.

#### Get ComPDFKit License Key

ComPDF provides two types of license key: 30-day free trial license, and formal license.

#### How to Get Free Trial License

**Method 1: Apply Online**

If you want to get PDF SDK trials for **Web, Windows, Android, iOS, Flutter, and React Native**, simply apply for a [30-day free trial license](https://www.compdf.com/contact-sales) online.

You can check features supported by the free trial license on our [Pricing page](https://www.compdf.com/pricing).

**Method 2: Contact Sales**

For other platforms or features outside of the trial license, feel free to [contact our sales team](https://www.compdf.com/contact-sales).

#### How to Get Formal License

ComPDFKit PDF SDK is a commercial SDK that requires a license for application release. Any documents, sample code, or source code distribution from the released package of ComPDFKit to any third party is prohibited.

To get formal licenses for Windows platforms, advanced features, custom requirements, or quote inquiries, feel free to [contact our sales team](https://www.compdf.com/contact-sales).

For Windows PDF SDK, formal license must be bound to your developer device ID ([How to find the developer device ID](https://www.compdf.com/faq/how-to-find-the-device-id) ), and each license is only valid for one device ID in development mode. 

#### **Download PDF SDK**

Download the ComPDFKit PDF SDK for Windows on [GitHub](https://github.com/ComPDFKit/compdfkit-pdf-sdk-windows) or [NuGet](https://www.nuget.org/packages/ComPDFKit.NetFramework).

#### System Requirements

| System Requirements        | Windows 7, 8, 10, and 11 (32-bit and 64-bit)                 |
| -------------------------- | ------------------------------------------------------------ |
| **IDE**                    | Visual Studio 2017 or higher (Make sure the **.NET Desktop Development** is installed) |
| **Framework Requirements** | .NET Framework 4.5 or higher                                 |



# How to Make a Windows Program in C#

## Video Guide: Create a C# PDF Viewer for Windows
[<img src="images-windows/image-youtube-20250515.jpg" alt="image-youtube" width="50%" height="50%"/>](https://youtu.be/HTFMhzE1Fu4?si=obeyvxldCbVVnHTY)


## Create a New Project


1. Open Visual Studio 2022, and click **Create a new project**.

   <img src="images-windows/vs2022_1.png" alt="2.4" width="50%" height="50%"/>

2. Choose **WPF App (.NET Framework)** and click **Next**.

   <img src="images-windows/vs2022_2.png" alt="2.4" width="50%" height="50%"/>


3. Configure your project: Set your project name and choose the location to store your program. The project name is called "ComPDFKit Demo" in this example. This sample project uses .NET Framework 4.6.1 as the programming framework.

<img src="images-windows/vs2022_3.png" alt="2.4" width="50%" height="50%"/>


4. Click the **Create** button. Then, the new project will be created.



## Add ComPDFKit to Your Project

There are two ways to add ComPDFKit to your Project: [Nuget Repository](https://www.nuget.org/packages/ComPDFKit.NetFramework) and Local Package, you can choose one or the other according to your needs.

**Nuget Repository**

1. Open your project's solution, and in the Solution Explorer, right-click on **References** and click on the menu item **Manage NuGet Packages**. This will open the NuGet Package Manager for your solution.

<img src="images-windows/2.4.2.1.png" alt="2.4" width="35%" height="35%"/>

2. Go to [ComPDFKit.NetFramework](https://www.nuget.org/packages/ComPDFKit.NetFramework) in Nuget, and click on the **Install** button to install the package.


   <img src="images-windows/2.4.2.3.png" alt="2.4" width="65%" height="65%"/>

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

   <img src="images-windows/2.4.2.8.png" alt="2.4" width="50%" height="50%"/>

6. Once that's complete, you'll see a reference to the package in the Solution Explorer under **References**.

   <img src="images-windows/2.4.2.9.png" alt="2.4" width="50%" height="50%"/>
## Apply the License Key

If you haven't get a license key, please check out [how to obtain a license key](###Get ComPDFKit License Key).

ComPDFKit PDF SDK currently supports two authentication methods to verify license keys: online authentication and offline authentication.

*Learn about:* 

- [*What is the authentication mechanism of ComPDFKit's license?*](https://www.compdf.com/faq/authentication-mechanism-of-compdfkit-license)

- [*What are the differences between Online Authentication and Offline Authentication?*](https://www.compdf.com/faq/the-differences-between-online-authentication-and-offline-authentication)

#### Copy the License Key

After getting the license key, follow the steps below to apply it to your project.

1. In the email you received, locate the `XML` file containing the license key.

2. Copy the license key `XML` file to your own project.

#### Apply the License Key

You can perform authentication using the following method:

```C#
public static bool LicenseVerify()
{
	if (!CPDFSDKVerifier.LoadNativeLibrary())
	{ 
		return false;
	}
	string xmlPath = "The path to your license key XML file";
	LicenseErrorCode status = CPDFSDKVerifier.LicenseVerify(xmlPath);
	return status == LicenseErrorCode.E_LICENSE_SUCCESS;
}
```



## Display a PDF Document

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


## Troubleshooting

1. License Verification Failed
- If "System.IO.FileNotFoundException" occurred in the `LicenseVerify()` function like this:

<img src="images-windows/vs2022_4.png" alt="2.4" width="50%" height="50%"/>

- Check your WPF project and ensure that you chose **WPF App(.NET Framework)** instead of **WPF Application** when creating the project.

<img src="images-windows/vs2022_5.png" alt="2.4" width="50%" height="50%"/>

2. Other Problems

   If you meet some other problems when integrating our ComPDFKit PDF SDK for Windows, feel free to contact [our support team](https://www.compdf.com/support).



# Support

ComPDF offers professional technical support and 5*24 responsive service.

- For detailed information, please visit our [Guides](https://www.compdf.com/guides/pdf-sdk/windows/overview) page.

- Stay updated with the latest improvements through our [Changelog](https://www.compdf.com/pdf-sdk/changelog-windows).

- For technical assistance, please reach out to our [Technical Support](https://www.compdf.com/support).

- To get more details and an accurate quote, please contact our [Sales Team](https://compdf.com/contact-us) or [Send an Email](mailto:support@compdf.com) to us.




# License

ComPDF offers developers a [30-day free trial license](https://www.compdf.com/pricing) for free testing your Windows projects. Additionally, you'll have access to a fully-featured product with no limitations on file or user count.  

# Note

We are glad to announce that you can register a ComPDFKit API account for a [free trial](https://api.compdf.com/api/pricing) to process 1000 documents per month for free.

