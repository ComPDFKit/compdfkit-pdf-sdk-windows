Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module PDFATest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        ' Perparation work
        Console.WriteLine("Running PDFA test sample…" & vbCrLf)

        SDKLicenseHelper.LicenseVerify()

        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' PDF/A-1a
        If CovertToPDFA1a(document) Then
            Console.WriteLine("Convert to PDF/A-1a done.")
        Else
            Console.WriteLine("Convert to PDF/A-1a failed.")
        End If

        document.Release()

        Console.WriteLine("--------------------")

        ' PDF/A-1b
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If CovertToPDFA1b(document) Then
            Console.WriteLine("Convert to PDF/A-1b done.")
        Else
            Console.WriteLine("Convert to PDF/A-1b failed.")
        End If

        document.Release()

        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    ' Save PDF as PDFA1a
    Public Function CovertToPDFA1a(document As CPDFDocument) As Boolean
        Dim convertToPDFA1aPath As String = outputPath & "\ConvertToPDFA1aTest.pdf"
        If Not document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1a, convertToPDFA1aPath) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & convertToPDFA1aPath)
        Return True
    End Function

    ' Save PDF as PDFA1b
    Public Function CovertToPDFA1b(document As CPDFDocument) As Boolean
        Dim convertToPDFA1bPath As String = outputPath & "\ConvertToPDFA1bTest.pdf"
        If Not document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1b, convertToPDFA1bPath) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & convertToPDFA1bPath)
        Return True
    End Function
End Module
