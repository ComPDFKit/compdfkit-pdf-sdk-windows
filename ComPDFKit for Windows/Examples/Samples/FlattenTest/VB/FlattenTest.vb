Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage
Imports System
Imports System.IO

Module FlattenTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"
    Sub Main(args As String())
        Console.WriteLine("Running Flatten test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()
        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Flatten
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("Annotations.pdf")

        If Flatten(document) Then
            Console.WriteLine("Flatten done.")
        Else
            Console.WriteLine("Flatten failed.")
        End If

        Console.WriteLine("--------------------")
        document.Release()

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Flatten documentation with comments
    Private Function Flatten(document As CPDFDocument) As Boolean
        Dim annotationCount As Integer = 0
        For i As Integer = 0 To document.PageCount - 1
            Dim page As CPDFPage = document.PageAtIndex(i)
            annotationCount += page.GetAnnotCount()
        Next

        Console.Write("{0} annotations in the file. ", annotationCount)
        Dim flattenPath As String = outputPath & "\FlattenTest.pdf"

        If Not document.WriteFlattenToFilePath(flattenPath) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & flattenPath)

        ' Verify: Check if the number of comments in the new document is 0
        Dim flattenDocument As CPDFDocument = CPDFDocument.InitWithFilePath(flattenPath)
        Dim newCount As Integer = 0
        For i As Integer = 0 To flattenDocument.PageCount - 1
            Dim page As CPDFPage = flattenDocument.PageAtIndex(i)
            newCount += page.GetAnnotCount()
        Next

        Console.WriteLine("{0} annotations in the new file. ", newCount)

        If Not (newCount = 0) Then
            Return False
        End If

        Return True
    End Function
End Module
