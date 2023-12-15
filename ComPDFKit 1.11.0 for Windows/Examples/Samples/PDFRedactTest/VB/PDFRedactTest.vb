Imports ComPDFKit.Import
Imports ComPDFKit.PDFAnnotation
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage
Imports System
Imports System.IO
Imports System.Windows

Module PDFRedactTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        ' Perparation work
        Console.WriteLine("Running redact test sample…" & vbCrLf)
        SDKLicenseHelper.LicenseVerify()

        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        Dim str As String = document.PageAtIndex(0).GetTextPage().GetSelectText(New Point(300, 240), New Point(400, 300), New Point(0, 0))
        Console.WriteLine("The text need to be redact is: {0}", str)

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Redact
        If Redact(document) Then
            Console.WriteLine("Redact done.")
        Else
            Console.WriteLine("Redact failed.")
        End If

        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    ' Redact an area in PDF
    Private Function Redact(document As CPDFDocument) As Boolean
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim redactAnnot As CPDFRedactAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_REDACT), CPDFRedactAnnotation)
        ' Set redact rect: cover the title
        redactAnnot.SetRect(New CRect(300, 240, 400, 300))
        ' Set overlay text: REDACTED
        redactAnnot.SetOverlayText("REDACTED")

        ' Properties of cover text
        Dim textAttribute As New CTextAttribute()
        textAttribute.FontName = "Helvetica"
        textAttribute.FontSize = 12
        Dim fontColor As Byte() = {255, 0, 0}
        textAttribute.FontColor = fontColor
        redactAnnot.SetTextDa(textAttribute)
        redactAnnot.SetTextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_LEFT)

        ' Properties of cover square
        Dim fillColor As Byte() = {255, 0, 0}
        redactAnnot.SetFillColor(fillColor)
        Dim outlineColor As Byte() = {0, 255, 0}
        redactAnnot.SetOutlineColor(outlineColor)

        redactAnnot.UpdateAp()
        document.ApplyRedaction()
        ' Save to pointed path so you can observe the effect.
        Dim path As String = outputPath & "\RedactTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Dim newDocument As CPDFDocument = CPDFDocument.InitWithFilePath(path)

        ' Validation: try to get the text of the covered area
        Dim str As String = newDocument.PageAtIndex(0).GetTextPage().GetSelectText(New Point(60, 200), New Point(560, 250), New Point(0, 0))
        Console.WriteLine("Text in the redacted area is: {0}", str)
        Return True
    End Function
End Module
