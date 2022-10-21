# 1 Overview

ComPDFKit, a software development kit (SDK), consists of PDF SDK and PDF Conversion SDK. With ComPDFKit, even developers with limited knowledge of PDF can quickly integrate professional PDF functions with just a few lines of code on multiple platforms. And we will introduce ComPDFKit PDF SDK for Windows here.

ComPDFKit PDF SDK for Windows is a powerful PDF library that ships with an easy-to-use C# interface. Developers can seamlessly integrate PDF rendering, navigation, creation, searching, annotation, PDF text extract, form data collection, and editing capabilities into their applications and services running.

## 1.1 ComPDFKit PDF SDK

ComPDFKit PDF SDK consists of two elements as shown in the following picture.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/ComPDFKit.png)

The two elements for ComPDFKit:

- **PDF Core API**

The ComPDFKit PDF SDK.Desk can be used independently for document rendering, analysis, text extraction, text search, form filling, annotation creation and manipulation, and much more.
- **PDF View**

The ComPDFKit PDF SDK.Viewer is a utility class that provides the functionality for developers to interact with rendering PDF documents per their requirements. The View Control provides fast and high-quality rendering, zooming, scrolling, and page navigation features.
## 1.2 Key Features

**Viewer** component offers:

- Standard page display modes, including Single page, Double page, Scrolling, and Cover mode.
- Navigation with thumbnails, outlines, and bookmarks.
- Text search & selection.
- Zoom in and out & Fit-page.
- Switch between different themes, including Dark mode, Sepia mode, Reseda mode, and Custom color mode.
- Text reflow.

**Annotations** component offers:

- Create, edit and remove annotations, including Note, Link, Freetext, Line, Square, Circle, Highlight, Underline, Squiggly, Strikeout, Stamp, Ink, and Sound.
- Support for annotation appearances.
- Import and export annotations to/from XFDF.
- Support for annotation flattening.

**Forms** component offers:

- Create, edit and remove form fields, including Push Button, Check Box, Radio Button, Text Field, Combo Box, List Box, and Signature.
- Fill PDF Forms.
- Support for PDF form flattening.

**Document editor** component offers:

- PDF manipulation, including Split pages, Extract pages, and Merge pages.
- Page edit, include: Delete pages, Insert pages, Move pages, Rotate pages, Replace pages, and Exchange pages.
- Document information setting.
- Extract images.

**Security** component offers:

- Encrypt and decrypt PDFs, including Permission setting and Password protected.
- Create and remove watermark.
- Redact content including images, text, and vector graphics.
- Create, edit, and remove header & footer, including dates, page numbers, document name, author name, and chapter name.
- Create, edit, and remove bates numbers.
- Create, edit, and remove background that can be a solid color or an image.

**Conversion** component offers:

- PDF to PDF/A.

## 1.3 License

ComPDFKit PDF SDK is a commercial SDK, which requires a license to grant developer permission to release their apps. Each license is only valid for one device ID in development mode. Other flexible licensing options are also supported, please contact [our marketing team](mailto:support@compdf.com) to know more. However, any documents, sample code, or source code distribution from the released package of ComPDFKit PDF SDK to any third party is prohibited.

# 2 Get Started

It is easy to embed ComPDFKit PDF SDK in your Windows program with a few lines of C# code. Takes just a few minutes and gets started. 

The following sections introduce the structure of the installation package, how to run a demo, and how to make a Windows program in C# with ComPDFKit PDF SDK. 

## 2.1 Requirements

Please make sure that the .NET Desktop Development and .NET Framework 4.6.1+ development tools workload is part of your installation.  

- Windows 7,8,10, and 11 (32-bit, 64-bit)  .
- Visual Studio 2017 or higher.
- .NET Framework 4.6.1 or higher.

## 2.2 Windows Package Structure

The package of ComPDFKit PDF SDK for Windows includes the following files as shown in Figure 2-1:

- **lib** - Include the ComPDFKit dynamic library (x86, x64).
- **ComPDFKit.Demo** - A folder containing Windows sample projects.
- **api_reference_windows** - API reference.
- **developer_guide_windows.pdf** - Developer guide.
- **release_notes.txt** - Release information.
- **legal.txt** - Legal and copyright information.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220218141621062.png)

<p align="center">
Figure 2-1
</p>

## 2.3 How to Run a Demo

ComPDFKit PDF SDK for Windows provides one demo in C# for developers to learn how to call the SDK on Windows. You can find them in the ***"ComPDFKit.Demo"*** folder. In this guide, it takes the "C#" demo as an example to show how to run it in Visual Studio 2017.

1. Double-click the ***"ComPDFKit.Demo.sln"*** found in the ***"ComPDFKit.Demo"*** folder to open the demo in Visual Studio 2017.

2. Click on ***"Start"*** to run the demo on a Windows device. In this guide, a Windows10 device will be used as an example. After building the demo successfully, click the ***"Open"*** button,and select a PDF Document, then it will be opened and displayed.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220218101311614.png)

**Note:** *This is a demo project, presenting completed ComPDFKit PDF SDKfunctions. The functions might be different based on the license you have purchased. Please check that the functions you choose work fine in this demo project.*

## 2.4 How to Make a Windows Program in C# with ComPDFKit PDF SDK

This section will help you to quickly get started with ComPDFKit PDF SDK to make a Windows project in C# with step-by-step instructions, which include the following steps:

1. Create a new Windows project in C#.
2. Integrate ComPDFKit into your project.
3. Apply the license key.
4. Display a PDF document.

### 2.4.1 Create a New Windows Project in C#

In this guide, we use Visual Studio 2017 to create a new Windows project.

Fire up Visual Studio 2017, choose **File** -> **New** -> **Project...**, and then select **Visual C#**->**Windows Desktop** -> **WPF App(.NET Framework)** as shown in Figure 2-2.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220117173740405.png)

<p align="center">
Figure 2-2
</p>

Choose the options for your new project as shown in Figure 2-3. Please make sure to choose .NET Framework 4.6.1 as the programming framework.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220117173900713.png)

<p align="center">
Figure 2-3
</p>

Place the project to the location as desired. Then, click **OK**.

### 2.4.2 Integrate ComPDFKit PDF SDK into your projects

1. Copy the ***"ComPDFKit.Desk.dll"*** and ***"ComPDFKit.Viewer.dll"*** DLL files and ***"x64"*** or ***"x86"*** folder depending on your build configurations in the ***"lib"*** folder to the project ***"PdfViewer"*** folder.

2. Add ComPDFKit PDF SDK dynamic library to **References**. In order to use ComPDFKit PDF SDK APIs in the project, you must first add a reference to it.

- In Solution Explorer, right-click the ***"PdfViewer"*** project and click **Add -> Reference…**

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220117181014260.png)

- In the Add Reference dialog, click Browse tab, navigate to the ***"PdfViewer"*** folder, select ***"ComPDFKit.Desk.dll"*** and ***"ComPDFKit.Viewer.dll"*** dynamic library, and then click **OK**.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220207135940239.png)

3. Add ***"ComPDFKit.dll"*** to the project. Include the ***"x64"*** or ***"x86"*** folder into the project. Please make sure to set the property ***"Copy to Output Directory"*** of ***"ComPDFKit.dll"*** to ***"Copy if newer"***. Otherwise, you should copy it to the same folder with the executable file manually before running the project.

![Alt text](https://github.com/ComPDFKit/PDF-SDK-Windows/blob/main/images-windows/image-20220207140432182.png)

### 2.4.3 Apply the License Key

It is important that you set the license key before using any ComPDFKit PDF SDK classes.

```C#
bool LicenseVerify()
{
    bool result = CPDFSDKVerifier.LoadNativeLibrary();
    if (!result)
        return false;

    string devKey = "***";
    string devSecret = "***";
    string userKey = "***";
    string userSecret = "***";
    CPDFSDKVerifier.LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(devKey, devSecret, userKey, userSecret);
    if (verifyResult != CPDFSDKVerifier.LicenseErrorCode.LICENSE_ERR_SUCCESS)
        return false;

    return true;
}
```

### 2.4.4 Display a PDF Document
