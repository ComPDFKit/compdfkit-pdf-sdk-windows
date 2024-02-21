Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module TextExtractTest
    Private parentPath As String =
        Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main(args As String())
        ' Preparation work
        Console.WriteLine("Running PDFPage test sample..." & vbCrLf)

        SDKLicenseHelper.LicenseVerify()
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        If PDFToText(document) Then
            Console.WriteLine("PDF to text done.")
        Else
            Console.WriteLine("PDF to text failed.")
        End If

        Console.WriteLine("--------------------")
        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Convert PDF to text
    Private Function PDFToText(document As CPDFDocument) As Boolean
        Dim filePath As String = Path.Combine(outputPath, "PDFToText.txt")
        If Not document.PdfToText("1-" & document.PageCount.ToString(), filePath) Then ' Page ranges are counted from 1
            Return False
        End If
        Console.WriteLine("Browse the generated file in " & filePath)
        Return True
    End Function
End Module
