Imports ComPDFKit.Import
Imports ComPDFKit.PDFAnnotation
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage
Imports System.IO
Imports System.Windows

Module TextSearchTest
    Private parentPath As String =
        Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main(args As String())
        ' Preparation work
        Console.WriteLine("Running text search test sample..." & vbCrLf)

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
        SDKLicenseHelper.LicenseVerify()

        ' Sample 1: Search text
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("Text.pdf")

        SearchText(document)
        document.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Search for keywords in the current page and record the search results.
    Private Sub SearchForPage(page As CPDFPage, searchKeywords As String, options As C_Search_Options, ByRef rects As List(Of Rect), ByRef strings As List(Of String))
        rects = New List(Of Rect)()
        strings = New List(Of String)()
        Dim findIndex As Integer = 0

        Dim textPage As CPDFTextPage = page.GetTextPage()
        Dim searcher As New CPDFTextSearcher()

        If searcher.FindStart(textPage, searchKeywords, options, 0) Then
            Dim textRect As New CRect()
            Dim textContent As String = ""
            While searcher.FindNext(page, textPage, textRect, textContent, findIndex)
                strings.Add(textContent)
                rects.Add(New Rect(textRect.left, textRect.top, textRect.width(), textRect.height()))
            End While
        End If
    End Sub

    ' Highlight the first result
    Private Function HighlightTheFirstResult(page As CPDFPage, rect As Rect) As Boolean
        Dim cRectList As New List(Of CRect)()
        cRectList.Add(New CRect(CSng(rect.Left), CSng(rect.Bottom), CSng(rect.Right), CSng(rect.Top)))

        Dim annotation As CPDFHighlightAnnotation = CType(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT), CPDFHighlightAnnotation)
        Dim color As Byte() = {0, 255, 0}
        annotation.SetColor(color)
        annotation.SetTransparency(120)
        annotation.SetQuardRects(cRectList)
        annotation.UpdateAp()
        Return True
    End Function

    ' Search PDF keywords on the first page of the article,
    ' after the search is completed,
    ' highlight the first searched keyword and save it
    Private Function SearchText(document As CPDFDocument) As Boolean
        Dim page As CPDFPage = document.PageAtIndex(0)
        'rects: The collection of locales where keywords are located.
        Dim rects As New List(Of Rect)()
        'strings: The full text of the keyword's area
        Dim strings As New List(Of String)()

        'Search for single page
        SearchForPage(page, "PDF", C_Search_Options.Search_Case_Insensitive, rects, strings)

        Console.WriteLine("The pdf have {0} results", rects.Count)

        Console.WriteLine("Search finished, now highlight the first result. ")

        'Highlight the first result
        HighlightTheFirstResult(page, rects(0))

        Dim filePath As String = Path.Combine(outputPath, "HighlightFirstTest.pdf")
        If Not document.WriteToFilePath(filePath) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & filePath)
        Return True
    End Function
End Module
