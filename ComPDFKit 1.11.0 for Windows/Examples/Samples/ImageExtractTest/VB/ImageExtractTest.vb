Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module ImageExtractTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        ' Perparation work
        Console.WriteLine("Running Bookmark test sample..." & vbCrLf)

        SDKLicenseHelper.LicenseVerify()

        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Sample 1: Extract image
        ExtractImage(document)
        document.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Extract all images from document
    Private Sub ExtractImage(document As CPDFDocument)
        document.ExtractImage("1-5", outputPath)
    End Sub
End Module
