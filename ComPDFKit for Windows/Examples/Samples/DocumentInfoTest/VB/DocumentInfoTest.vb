Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module DocumentInfoTest
    Sub Main(args As String())
        Console.WriteLine("Running DocumentInfo test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()

        ' Sample 1: Print information
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        PrintDocumentInfo(document)
        Console.WriteLine("--------------------")

        Console.WriteLine("Done.")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    Public Function GetFileSize(filePath As String) As String
        Dim fileInfo As FileInfo = Nothing
        Try
            fileInfo = New FileInfo(filePath)
        Catch
            Return "0B"
        End Try

        If fileInfo IsNot Nothing AndAlso fileInfo.Exists Then
            Dim fileSize As Double = fileInfo.Length
            If fileSize > 1024 Then
                fileSize = Math.Round(fileSize / 1024, 2)
                If fileSize > 1024 Then
                    fileSize = Math.Round(fileSize / 1024, 2)
                    If fileSize > 1024 Then
                        fileSize = Math.Round(fileSize / 1024, 2)
                        Return fileSize & " GB"
                    Else
                        Return fileSize & " MB"
                    End If
                Else
                    Return fileSize & " KB"
                End If
            Else
                Return fileSize & " B"
            End If
        End If

        Return "0B"
    End Function

    Private Sub PrintDocumentInfo(document As CPDFDocument)
        Console.WriteLine("File Name: {0}", document.FileName)
        Console.WriteLine("File Size: {0}", GetFileSize(document.FilePath))
        Console.WriteLine("Title: {0}", document.GetInfo().Title)
        Console.WriteLine("Author: {0}", document.GetInfo().Author)
        Console.WriteLine("Subject: {0}", document.GetInfo().Subject)
        Console.WriteLine("Keywords: {0}", document.GetInfo().Keywords)
        Console.WriteLine("Version: {0}", document.GetInfo().Version)
        Console.WriteLine("Page Count: {0}", document.PageCount)
        Console.WriteLine("Creator: {0}", document.GetInfo().Creator)
        Console.WriteLine("Creation Data: {0}", document.GetInfo().CreationDate)
        Console.WriteLine("Allows Printing: {0}", document.GetPermissionsInfo().AllowsPrinting)
        Console.WriteLine("Allows Copying: {0}", document.GetPermissionsInfo().AllowsCopying)
        Console.WriteLine("Allows Document Changes: {0}", document.GetPermissionsInfo().AllowsDocumentChanges)
        Console.WriteLine("Allows Document Assembly: {0}", document.GetPermissionsInfo().AllowsDocumentAssembly)
        Console.WriteLine("Allows Commenting: {0}", document.GetPermissionsInfo().AllowsCommenting)
        Console.WriteLine("Allows FormField Entry: {0}", document.GetPermissionsInfo().AllowsFormFieldEntry)
    End Sub
End Module
