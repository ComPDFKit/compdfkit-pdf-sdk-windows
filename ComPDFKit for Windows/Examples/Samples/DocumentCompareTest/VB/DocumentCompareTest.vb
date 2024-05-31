Imports ComPDFKit.Compare
Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module DocumentCompareTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main(args As String())
        Console.WriteLine("Running PDFPage test sample..." & vbCrLf)

        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        Dim document1 As CPDFDocument = CPDFDocument.InitWithFilePath("ElectricalDiagram.pdf")
        Dim document2 As CPDFDocument = CPDFDocument.InitWithFilePath("ElectricalDiagram_New.pdf")
        If OverlayCompareDocument(document1, document2) Then
            Console.WriteLine("Compare document done.")
        Else
            Console.WriteLine("Compare document failed.")
        End If

        document1.Release()
        document2.Release()
        Console.WriteLine("--------------------")

        Dim document3 As CPDFDocument = CPDFDocument.InitWithFilePath("Text.pdf")
        Dim document4 As CPDFDocument = CPDFDocument.InitWithFilePath("TextChanged.pdf")
        If ContentCompareDocument(document3, document4) Then
            Console.WriteLine("Compare document done.")
        Else
            Console.WriteLine("Compare document failed.")
        End If

        document3.Release()
        document4.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    Private Function OverlayCompareDocument(document1 As CPDFDocument, document2 As CPDFDocument) As Boolean
        Dim compareOverlay As New CPDFCompareOverlay(document1, "1-5", document2, "1-5")
        compareOverlay.Compare()
        Dim comparisonDocument As CPDFDocument = compareOverlay.ComparisonDocument()
        Dim path As String = outputPath & "\CompareDocumentTest.pdf"
        If Not comparisonDocument.WriteToFilePath(path) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    Private Function ContentCompareDocument(document As CPDFDocument, documentNew As CPDFDocument) As Boolean
        Dim compareContent As New CPDFCompareContent(document, documentNew)
        Dim pageCount As Integer = Math.Min(document.PageCount, documentNew.PageCount)
        For i As Integer = 0 To pageCount - 1
            Console.WriteLine("Page: {0}", i)

            Dim compareResults As CPDFCompareResults = compareContent.Compare(i, i, CPDFCompareType.CPDFCompareTypeAll, True)
            Console.WriteLine("Replace count: {0}", compareResults.ReplaceCount)
            Console.WriteLine("TextResults count: {0}", compareResults.TextResults.Count)
            Console.WriteLine("Delete count: {0}", compareResults.DeleteCount)
            Console.WriteLine("Insert count: {0}", compareResults.InsertCount)
        Next

        Return True
    End Function
End Module
