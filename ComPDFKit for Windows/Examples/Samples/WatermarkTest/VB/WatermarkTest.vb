Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFWatermark
Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices

Module WatermarkTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        ' Preparation work
        Console.WriteLine("Running Watermark test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Sample 1: Add text watermark
        If AddTextWatermark(document) Then
            Console.WriteLine("Add text watermark done.")
        Else
            Console.WriteLine("Add text watermark failed.")
        End If

        document.Release()

        Console.WriteLine("--------------------")

        ' Sample 2: Add image watermark
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If AddImageWatermark(document) Then
            Console.WriteLine("Add image watermark done.")
        Else
            Console.WriteLine("Add image watermark failed.")
        End If

        document.Release()

        Console.WriteLine("--------------------")

        ' Sample 3: Delete watermark
        Dim textWatermarkDocument As CPDFDocument = CPDFDocument.InitWithFilePath("Watermark.pdf")

        If DeleteWatermark(textWatermarkDocument) Then
            Console.WriteLine("Delete watermark done.")
        Else
            Console.WriteLine("Delete watermark failed.")
        End If

        textWatermarkDocument.Release()

        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Add text watermark
    Private Function AddTextWatermark(document As CPDFDocument) As Boolean
        Dim watermark As CPDFWatermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_TEXT)
        watermark.SetText("test")
        watermark.SetFontName("Helvetica")
        watermark.SetPages("0-3")
        Dim color As Byte() = {255, 0, 0}
        watermark.SetTextRGBColor(color)
        watermark.SetScale(2)
        watermark.SetRotation(0)
        watermark.SetOpacity(120)
        watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER)
        watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER)
        watermark.SetVertOffset(0)
        watermark.SetHorizOffset(0)
        watermark.SetFront(True)
        watermark.SetFullScreen(True)
        watermark.SetVerticalSpacing(10)
        watermark.SetHorizontalSpacing(10)
        watermark.CreateWatermark()
        Dim path As String = outputPath & "\AddTextWatermarkTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)

        Return True
    End Function

    ' Convert the bitmap to an array that can be set as an image watermark
    Private Function BitmapToByteArray(bitmap As Bitmap) As Byte()
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

    ' Add image watermark
    Private Function AddImageWatermark(document As CPDFDocument) As Boolean
        Dim watermark As CPDFWatermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_IMG)
        Dim bitmap As New Bitmap("logo.png")
        watermark.SetImage(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height)
        watermark.SetPages("0-3")
        watermark.SetScale(2)
        watermark.SetRotation(1)
        watermark.SetOpacity(128)
        watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER)
        watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER)
        watermark.SetVertOffset(0)
        watermark.SetHorizOffset(0)
        watermark.SetFront(False)
        watermark.SetFullScreen(True)
        watermark.SetVerticalSpacing(10)
        watermark.SetHorizontalSpacing(10)
        watermark.CreateWatermark()
        watermark.UpdateWatermark()

        Dim path As String = outputPath & "\AddImageWatermarkTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Delete watermark
    Private Function DeleteWatermark(watermarkDocument As CPDFDocument) As Boolean
        watermarkDocument.DeleteWatermarks()
        Dim path As String = outputPath & "\DeleteWatermarkTest.pdf"
        If Not watermarkDocument.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)

        Return True
    End Function
End Module
