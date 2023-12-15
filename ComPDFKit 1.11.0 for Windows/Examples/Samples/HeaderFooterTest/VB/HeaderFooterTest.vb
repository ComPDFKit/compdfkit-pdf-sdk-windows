Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO
Imports System.Collections.Generic

Module HeaderFooterTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Private IntToLocationDic As New Dictionary(Of Integer, String) From {
        {0, "Top Left"},
        {1, "Top Middle"},
        {2, "Top Right"},
        {3, "Bottom Left"},
        {4, "Bottom Middle"},
        {5, "Bottom Right"}
    }

    Sub Main(args As String())
        Console.WriteLine("Running header and footer test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()

        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Add common header and footer
        If AddCommonHeaderFooter(document) Then
            Console.WriteLine("Add common header and footer done.")
        Else
            Console.WriteLine("Add common header and footer failed.")
        End If
        Console.WriteLine("--------------------")

        ' Add page header and footer
        If AddPageHeaderFooter(document) Then
            Console.WriteLine("Add page header and footer done.")
        Else
            Console.WriteLine("Add page header and footer failed.")
        End If

        Console.WriteLine("--------------------")

        ' Edit header and footer
        If EditHeaderFooter(document) Then
            Console.WriteLine("Edit header and footer done.")
        Else
            Console.WriteLine("Edit header and footer failed.")
        End If

        Console.WriteLine("--------------------")

        ' Delete header and footer
        If ClearHeaderFooter(document) Then
            Console.WriteLine("Delete header and footer done." & vbCrLf)
        Else
            Console.WriteLine("Delete header and footer failed" & vbCrLf)
        End If

        Console.WriteLine("--------------------")
        Console.WriteLine("Done")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    ' Add common header and footer
    Private Function AddCommonHeaderFooter(document As CPDFDocument) As Boolean
        Dim headerFooter As CPDFHeaderFooter = document.GetHeaderFooter()
        Dim color As Byte() = {255, 0, 0}
        headerFooter.SetPages("0-" & (document.PageCount - 1))

        For i As Integer = 0 To 2
            headerFooter.SetText(i, "ComPDFKit")
            headerFooter.SetTextColor(i, color)
            headerFooter.SetFontSize(i, 14)

            Console.WriteLine("Text: {0}", headerFooter.GetText(i))
            Console.WriteLine("Location: {0}" & vbCrLf, IntToLocationDic(i))
        Next

        headerFooter.Update()

        Dim addHeaderFooterPath As String = outputPath & "\AddCommonHeaderFooterTest.pdf"

        If Not document.WriteToFilePath(addHeaderFooterPath) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & addHeaderFooterPath)
        Return True
    End Function

    ' Add page header and footer
    Private Function AddPageHeaderFooter(document As CPDFDocument) As Boolean
        Dim headerFooter As CPDFHeaderFooter = document.GetHeaderFooter()
        Dim color As Byte() = {255, 0, 0}
        headerFooter.SetPages("0-" & (document.PageCount - 1))

        For i As Integer = 3 To 5
            headerFooter.SetText(i, "<<1,2>>")
            headerFooter.SetTextColor(i, color)
            headerFooter.SetFontSize(i, 14)

            Console.WriteLine("Text: {0}", headerFooter.GetText(i))
            Console.WriteLine("Location: {0}" & vbCrLf, IntToLocationDic(i))
        Next

        headerFooter.Update()

        Dim addHeaderFooterPath As String = outputPath & "\AddPageHeaderFooterTest.pdf"

        If document.WriteToFilePath(addHeaderFooterPath) Then
            Console.WriteLine("Browse the changed file in " & addHeaderFooterPath)
            Return True
        Else
            Return False
        End If
    End Function

    ' Edit header and footer
    Private Function EditHeaderFooter(document As CPDFDocument) As Boolean
        Dim headerFooter As CPDFHeaderFooter = document.GetHeaderFooter()

        If headerFooter.GetText(0) <> String.Empty Then
            Console.WriteLine("Get old head and footer 0 succeeded, text is {0}", headerFooter.GetText(0))
        Else
            Console.WriteLine("Get head and footer 0 failed, or it does not exist")
            Return False
        End If

        headerFooter.SetText(0, "ComPDFKit Samples")

        headerFooter.Update()

        Console.WriteLine("Change head and footer 0 succeeded, new text is {0}", headerFooter.GetText(0))

        Dim editHeaderFooterPath As String = outputPath & "\EditHeaderFooterTest.pdf"

        If document.WriteToFilePath(editHeaderFooterPath) Then
            Console.WriteLine("Browse the changed file in " & editHeaderFooterPath)
            Return True
        Else
            Return False
        End If
    End Function

    ' Delete header and footer
    Private Function ClearHeaderFooter(document As CPDFDocument) As Boolean
        Dim headerFooter As CPDFHeaderFooter = document.GetHeaderFooter()

        headerFooter.Clear()

        Dim clearHeaderFooterPath As String = outputPath & "\ClearHeaderFooterTest.pdf"

        If document.WriteToFilePath(clearHeaderFooterPath) Then
            Console.WriteLine("Browse the changed file in " & clearHeaderFooterPath)
            Return True
        Else
            Return False
        End If
    End Function
End Module
