Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module AnnotationImportExportTest
    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")
    Private tempPath As String = Path.Combine(outputPath, "temp")

    Sub Main()
        ' Preparation work
        Console.WriteLine("Running header and footer test sample…" & Environment.NewLine)

        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
        If Not Directory.Exists(tempPath) Then
            Directory.CreateDirectory(tempPath)
        End If

        ' Sample 1: Export Annotation
        Dim annotationsDocument As CPDFDocument = CPDFDocument.InitWithFilePath("Annotations.pdf")

        If ExportAnnotation(annotationsDocument) Then
            Console.WriteLine("Export annotation done.")
        End If

        Console.WriteLine("--------------------")

        ' Sample 2: Import Annotations
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If ImportAnnotation(document) Then
            Console.WriteLine("Import annotation done.")
        End If

        Console.WriteLine("--------------------")

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Export the annotations in the document to XFDF format
    Private Function ExportAnnotation(document As CPDFDocument) As Boolean
        Dim filePath As String = Path.Combine(outputPath, "ExportAnnotationTest.xfdf")
        If Not document.ExportAnnotationToXFDFPath(filePath, tempPath) Then
            Return False
        End If
        Console.WriteLine("Xfdf file in " & filePath)
        Return True
    End Function

    ' Importing XFDF into the document
    Private Function ImportAnnotation(document As CPDFDocument) As Boolean
        Dim filePath As String = Path.Combine(outputPath, "ImportAnnotationTest.pdf")

        If Not document.ImportAnnotationFromXFDFPath("Annotations.xfdf", tempPath) Then
            Return False
        End If
        If Not document.WriteToFilePath(filePath) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & filePath)
        Return True
    End Function

End Module
