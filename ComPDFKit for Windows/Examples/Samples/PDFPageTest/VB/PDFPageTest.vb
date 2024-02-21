Imports ComPDFKit.PDFDocument
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.IO

Module PDFPageTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        ' Preparation work
        Console.WriteLine("Running PDFPage test sample…" & vbCrLf)

        SDKLicenseHelper.LicenseVerify()
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Sample 1: Insert blank page
        If InsertBlankPage(document) Then
            Console.WriteLine("Insert blank page done.")
        Else
            Console.WriteLine("Insert blank page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 2: Insert PDF page
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If InsertPDFPPage(document) Then
            Console.WriteLine("Insert PDF page done.")
        Else
            Console.WriteLine("Insert PDF page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 3: Split pages
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If SplitPages(document) Then
            Console.WriteLine("Split page done.")
        Else
            Console.WriteLine("Split failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 4: Remove pages
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If RemovePages(document) Then
            Console.WriteLine("Delete even page done.")
        Else
            Console.WriteLine("Delete even page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 5: Rotate page
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If RotatePage(document) Then
            Console.WriteLine("Rotate page done.")
        Else
            Console.WriteLine("Rotate page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 6: Replace pages
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If RepalcePages(document) Then
            Console.WriteLine("Repalce page done.")
        Else
            Console.WriteLine("Repalce page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        ' Sample 7: Extract pages
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If ExtractPages(document) Then
            Console.WriteLine("Extract page done.")
        Else
            Console.WriteLine("Extract page failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    ' Insert a new page of A4 size at the second page
    Private Function InsertBlankPage(document As CPDFDocument) As Boolean
        Dim pageIndex As Integer = 1
        Dim pageWidth As Integer = 595
        Dim pageHeight As Integer = 842
        document.InsertPage(pageIndex, pageWidth, pageHeight, "")
        Console.WriteLine("Insert PageIndex: {0}", pageIndex)
        Console.WriteLine("Size: {0}*{1}", pageWidth, pageHeight)

        Dim path As String = outputPath & "\InsertBlankPageTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Select pages from other PDF files and insert them into the current document
    Private Function InsertPDFPPage(document As CPDFDocument) As Boolean
        Dim documentForInsert As CPDFDocument = CPDFDocument.InitWithFilePath("Text.pdf")
        document.ImportPagesAtIndex(documentForInsert, "1", 1)

        Dim path As String = outputPath & "\InsertPDFPPageTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Split the current document into two documents according to the first 2 pages and the last 3 pages
    Private Function SplitPages(document As CPDFDocument) As Boolean
        ' Split 1-2 pages
        Dim documentPart1 As CPDFDocument = CPDFDocument.CreateDocument()
        documentPart1.ImportPagesAtIndex(document, "1-2", 0)

        Dim pathPart1 As String = outputPath & "\SplitPart1Test.pdf"
        If Not documentPart1.WriteToFilePath(pathPart1) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & pathPart1)

        ' Split 3-5 pages
        Dim documentPart2 As CPDFDocument = CPDFDocument.CreateDocument()
        documentPart2.ImportPagesAtIndex(document, "3-5", 0)

        Dim pathPart2 As String = outputPath & "\SplitPart2Test.pdf"
        If Not documentPart2.WriteToFilePath(pathPart2) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & pathPart2)
        Return True
    End Function

    ' Remove even-numbered pages from a document
    Private Function RemovePages(document As CPDFDocument) As Boolean
        Dim arr As New ArrayList()
        For i As Integer = 1 To document.PageCount - 1 Step 2
            arr.Add(i)
        Next
        document.RemovePages(DirectCast(arr.ToArray(GetType(Integer)), Integer()))

        Dim path As String = outputPath & "\RemoveEvenPagesTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Rotate the first page 90 degrees clockwise
    Private Function RotatePage(document As CPDFDocument) As Boolean
        document.RotatePage(0, 1) ' Rotation: Rotate 90 degrees per unit
        Dim path As String = outputPath & "\RotatePageTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Replace the first page of the current document with a page from another document
    ' Delete the pages that need to be replaced first
    ' Insert the required pages into the document
    Private Function RepalcePages(document As CPDFDocument) As Boolean
        Dim pageArr(0) As Integer
        pageArr(0) = 0
        document.RemovePages(pageArr)
        Dim documentForInsert As CPDFDocument = CPDFDocument.InitWithFilePath("Text.pdf")
        document.ImportPagesAtIndex(documentForInsert, "1", 0)

        Dim path As String = outputPath & "\RepalcePagesTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    ' Extract pages from a document
    ' Create a new document
    ' Insert the required pages into a new document
    Private Function ExtractPages(document As CPDFDocument) As Boolean
        Dim extractDocument As CPDFDocument = CPDFDocument.CreateDocument()
        extractDocument.ImportPagesAtIndex(document, "1", 0)
        Dim path As String = outputPath & "\ExtractPagesTest.pdf"
        If Not extractDocument.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function
End Module

