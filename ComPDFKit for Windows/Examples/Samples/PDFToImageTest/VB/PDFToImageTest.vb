Imports ComPDFKit.Import
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage
Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Module PDFToImageTest
    Private parentPath As String =
        Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main(args As String())
        Console.WriteLine("Running PDFToImage test sample..." & vbCrLf)

        SDKLicenseHelper.LicenseVerify()
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        PDFPageToImage(document)

        Console.WriteLine("--------------------")
        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    Private Sub SaveWriteableBitmapAsPng(writeableBitmap As WriteableBitmap, fileName As String)
        Using stream As New FileStream(fileName, FileMode.Create)
            Dim encoder As New PngBitmapEncoder()
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap))
            encoder.Save(stream)
        End Using
    End Sub

    ' Convert PDF to image
    Private Function PDFPageToImage(document As CPDFDocument) As Boolean
        For i As Integer = 0 To document.PageCount - 1
            Dim pdfPage As CPDFPage = document.PageAtIndex(i, True)
            Dim pageSize As CSize = document.GetPageSize(0)
            Dim pageRect As New CRect(0, 0, CInt(pageSize.width / 72.0 * 96), CInt(pageSize.height / 72.0 * 96))
            Dim bmpData As Byte() = New Byte(CInt(pageRect.Width * pageRect.Height * (96 / 72.0) * (96 / 72.0) * 4) - 1) {}
            pdfPage.RenderPageBitmapWithMatrix(CSng(96 / 72.0), pageRect, Convert.ToUInt32(&HFFFFFFF), bmpData, 0, True)

            Dim writeableBitmap As New WriteableBitmap(CInt(pageRect.Width), CInt(pageRect.Height), 96, 96, PixelFormats.Bgra32, Nothing)
            writeableBitmap.WritePixels(New Int32Rect(0, 0, CInt(pageRect.Width), CInt(pageRect.Height)), bmpData, writeableBitmap.BackBufferStride, 0)
            Dim filePath As String = Path.Combine(outputPath, "PDFToImageTest" & i & ".png")
            SaveWriteableBitmapAsPng(writeableBitmap, filePath)
            Console.WriteLine("Png image saved in " & filePath)

        Next

        Return False
    End Function
End Module
