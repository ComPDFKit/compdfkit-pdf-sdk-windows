Imports System.IO
Imports ComPDFKit.PDFDocument
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Drawing.Imaging

Module BackgroundTest
    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main()
        ' Preparation work
        Console.WriteLine("Running Background test sample..." & Environment.NewLine)
        SDKLicenseHelper.LicenseVerify()
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Sample 1: Add color background
        If AddColorBackground(document) Then
            Console.WriteLine("Add color background done.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 2: Add image background
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If AddImageBackground(document) Then
            Console.WriteLine("Add image background done.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 3: Remove background
        Dim colorBgDocument As CPDFDocument = CPDFDocument.InitWithFilePath("ColorBackground.pdf")
        Dim imageBgDocument As CPDFDocument = CPDFDocument.InitWithFilePath("ImageBackground.pdf")

        If RemoveBackground(colorBgDocument, imageBgDocument) Then
            Console.WriteLine("Remove background done.")
        End If
        colorBgDocument.Release()
        imageBgDocument.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub


    ' Add color background
    ' Parameters:
    ' - document: Regular document
    Private Function AddColorBackground(document As CPDFDocument) As Boolean
        Dim background As CPDFBackground = document.GetBackground()
        background.SetBackgroundType(C_Background_Type.BG_TYPE_COLOR)
        background.SetColor(New Byte() {255, 0, 0})
        background.SetOpacity(255) ' 0-255
        background.SetScale(1) ' 1 == 100%
        background.SetRotation(0) ' Use radians
        background.SetHorizalign(C_Background_Horizalign.BG_HORIZALIGN_CENTER)
        background.SetVertalign(C_Background_Vertalign.BG_VERTALIGN_CENTER)
        background.SetXOffset(0)
        background.SetYOffset(0)
        background.SetPages("0-2") ' Page numbering from 0
        background.Update() ' Note: update after setup is complete

        Dim path As String = outputPath & "\AddColorBackgroundTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)

        Return True
    End Function

    ' Convert the bitmap to an array that can be set as an image watermark
    ' Parameters:
    ' - bitmap: Image source to be used as an image watermark.
    ' Returns: An array for setting image
    Public Function BitmapToByteArray(bitmap As Bitmap) As Byte()
        Dim bmpdata As BitmapData = Nothing

        Try
            bmpdata = bitmap.LockBits(New Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat)
            Dim numbytes As Integer = bmpdata.Stride * bitmap.Height
            Dim bytedata(numbytes - 1) As Byte
            Dim ptr As IntPtr = bmpdata.Scan0

            Marshal.Copy(ptr, bytedata, 0, numbytes)

            Return bytedata
        Finally
            If bmpdata IsNot Nothing Then
                bitmap.UnlockBits(bmpdata)
            End If
        End Try
    End Function

    Private Function AddImageBackground(document As CPDFDocument) As Boolean
        Dim background As CPDFBackground = document.GetBackground()
        background.SetBackgroundType(C_Background_Type.BG_TYPE_IMAGE)
        Dim bitmap As New Bitmap("logo.png")
        background.SetImage(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height, ComPDFKit.Import.C_Scale_Type.fitCenter)
        background.SetOpacity(128) ' 0-255
        background.SetScale(1) ' 1 == 100%
        background.SetRotation(1.0F) ' Use radians
        background.SetHorizalign(C_Background_Horizalign.BG_HORIZALIGN_CENTER)
        background.SetVertalign(C_Background_Vertalign.BG_VERTALIGN_CENTER)
        background.SetXOffset(0)
        background.SetYOffset(0)
        background.SetPages("0-2") ' Page numbering from 0
        background.Update() ' Note: update after setup is complete
        Dim filePath As String = Path.Combine(outputPath, "AddImageBackgroundTest.pdf")
        If Not document.WriteToFilePath(filePath) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & filePath)
        Return True
    End Function

    Private Function RemoveBackground(ByVal colorBgDocument As CPDFDocument, ByVal imageBgDocument As CPDFDocument) As Boolean
        Dim colorBackground As CPDFBackground = colorBgDocument.GetBackground()
        If colorBackground.GetBackgroundType() <> C_Background_Type.BG_TYPE_COLOR Then
            Return False
        End If

        colorBackground.Clear()
        Dim path1 As String = outputPath & "\ClearColorBgTest.pdf"
        If Not colorBgDocument.WriteToFilePath(path1) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path1)

        Dim imageBackground As CPDFBackground = imageBgDocument.GetBackground()
        imageBackground.Clear()
        Dim path2 As String = outputPath & "\ClearImageBgTest.pdf"
        If Not imageBgDocument.WriteToFilePath(path2) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path2)

        Return True
    End Function

End Module
