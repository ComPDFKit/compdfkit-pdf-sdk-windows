Imports System.IO
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage
Imports ComPDFKit.PDFAnnotation
Imports ComPDFKit.Import
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Drawing.Imaging

Module AnnotationTest
    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main()
        ' Preparation work
        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        ' Sample 1: Create annotations
        If CreateAnnots(document) Then
            Console.WriteLine("Create annots done.")
        End If
        Console.WriteLine("--------------------")

        ' Sample 2: Delete annotations
        Dim annotsDocument As CPDFDocument = CPDFDocument.InitWithFilePath("Annotations.pdf")
        If DeleteAnnotations(annotsDocument) Then
            Console.WriteLine("Delete annots done.")
        End If

        Console.WriteLine("--------------------")

        Console.WriteLine("Done")
        Console.WriteLine("--------------------")

        Console.ReadLine()


    End Sub

    Private Sub CreateFreetextAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim str As String = "ComPDFKit Samples"
        Dim freeText As CPDFFreeTextAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT), CPDFFreeTextAnnotation)
        freeText.SetContent(str)
        freeText.SetRect(New CRect(0, 100, 160, 0))
        Dim textAttribute As New CTextAttribute()
        textAttribute.FontName = "Helvetica"
        textAttribute.FontSize = 12
        Dim fontColor As Byte() = {255, 0, 0}
        textAttribute.FontColor = fontColor
        freeText.SetFreetextDa(textAttribute)
        freeText.SetFreetextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_CENTER)
        freeText.UpdateAp()
    End Sub

    Private Sub CreateFreehandAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim ink As CPDFInkAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_INK), CPDFInkAnnotation)
        ink.SetInkColor(New Byte() {255, 0, 0})
        ink.SetBorderWidth(2)
        ink.SetTransparency(128)

        Dim points As New List(Of List(Of CPoint))()

        ink.SetInkPath(points)
        ink.SetThickness(8)

        points.Clear()
        points.Add(New List(Of CPoint)() From
    {
        New CPoint(10, 100),
        New CPoint(100, 10)
    })

        ink.SetInkPath(points)
        ink.UpdateAp()
    End Sub

    Private Sub CreateShapeAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim dashArray As Single() = {2.0F, 1.0F}
        Dim lineColor As Byte() = {255, 0, 0}
        Dim bgColor As Byte() = {0, 255, 0}

        ' Square
        Dim square As CPDFSquareAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE), CPDFSquareAnnotation)
        square.SetRect(New CRect(10, 250, 200, 200))
        square.SetLineColor(lineColor)
        square.SetBgColor(bgColor)
        square.SetTransparency(120)
        square.SetLineWidth(1)
        square.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray)
        square.UpdateAp()

        ' Circle
        Dim circle As CPDFCircleAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE), CPDFCircleAnnotation)
        circle.SetRect(New CRect(10, 410, 110, 300))
        circle.SetLineColor(lineColor)
        circle.SetBgColor(bgColor)
        circle.SetTransparency(120)
        circle.SetLineWidth(1)
        circle.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray)
        circle.UpdateAp()

        ' Line
        Dim line As CPDFLineAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_LINE), CPDFLineAnnotation)
        line.SetLinePoints(New CPoint(300, 300), New CPoint(350, 350))
        line.SetLineType(C_LINE_TYPE.LINETYPE_NONE, C_LINE_TYPE.LINETYPE_CLOSEDARROW)
        line.SetLineColor(lineColor)
        line.SetTransparency(120)
        line.SetLineWidth(1)
        line.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray)
        line.UpdateAp()
    End Sub

    Private Sub CreateNoteAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim textAnnotation As CPDFTextAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_TEXT), CPDFTextAnnotation)
        textAnnotation.SetColor(New Byte() {255, 0, 0})
        textAnnotation.SetTransparency(255)
        textAnnotation.SetContent("ComPDFKit")
        textAnnotation.SetRect(New CRect(300, 650, 350, 600))
        textAnnotation.UpdateAp()
    End Sub

    Private Sub CreateSoundAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim sound As CPDFSoundAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SOUND), CPDFSoundAnnotation)
        sound.SetRect(New CRect(400, 750, 450, 700))
        Dim bitmap As New Bitmap("SoundAnnot.png")
        sound.SetSoundPath(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height, "Bird.wav")
        sound.UpdateAp()
    End Sub

    Private Sub CreateMarkupAnnotation(document As CPDFDocument)
        Dim cRectList As New List(Of CRect)()
        Dim rect As New CRect(300, 300, 400, 240)
        cRectList.Add(rect)
        Dim color As Byte() = {255, 0, 0}

        ' Highlight
        Dim page1 As CPDFPage = document.PageAtIndex(0)
        Dim highlight As CPDFHighlightAnnotation = TryCast(page1.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT), CPDFHighlightAnnotation)
        highlight.SetColor(color)
        highlight.SetTransparency(120)
        highlight.SetQuardRects(cRectList)
        highlight.UpdateAp()

        ' Underline
        Dim page2 As CPDFPage = document.PageAtIndex(1)
        Dim underline As CPDFUnderlineAnnotation = TryCast(page2.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE), CPDFUnderlineAnnotation)
        underline.SetColor(color)
        underline.SetTransparency(120)
        underline.SetQuardRects(cRectList)
        underline.UpdateAp()

        ' Strikeout
        Dim page3 As CPDFPage = document.PageAtIndex(2)
        Dim strikeout As CPDFStrikeoutAnnotation = TryCast(page3.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT), CPDFStrikeoutAnnotation)
        strikeout.SetColor(color)
        strikeout.SetTransparency(120)
        strikeout.SetQuardRects(cRectList)
        strikeout.UpdateAp()

        ' Squiggly
        Dim page4 As CPDFPage = document.PageAtIndex(3)
        Dim squiggy As CPDFSquigglyAnnotation = TryCast(page4.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY), CPDFSquigglyAnnotation)
        squiggy.SetColor(color)
        squiggy.SetTransparency(120)
        squiggy.SetQuardRects(cRectList)
        squiggy.UpdateAp()
    End Sub

    Public Function BitmapToByteArray(bitmap As Bitmap) As Byte()
        Dim bmpdata As BitmapData = Nothing

        Try
            bmpdata = bitmap.LockBits(New Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat)
            Dim numbytes As Integer = bmpdata.Stride * bitmap.Height
            Dim bytedata As Byte() = New Byte(numbytes - 1) {}
            Dim ptr As IntPtr = bmpdata.Scan0

            Marshal.Copy(ptr, bytedata, 0, numbytes)

            Return bytedata
        Finally
            If bmpdata IsNot Nothing Then
                bitmap.UnlockBits(bmpdata)
            End If
        End Try
    End Function

    Private Sub CreateStampAnnotation(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)

        ' Standard
        Dim standard As CPDFStampAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP), CPDFStampAnnotation)
        standard.SetStandardStamp("Approved")
        standard.SetRect(New CRect(300, 160, 450, 100))
        standard.UpdateAp()

        ' Text
        Dim text As CPDFStampAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP), CPDFStampAnnotation)
        text.SetTextStamp("test", "detail text", C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE, C_TEXTSTAMP_COLOR.TEXTSTAMP_RED)
        text.SetRect(New CRect(300, 300, 450, 220))
        text.UpdateAp()

        ' Image
        Dim bitmap As New Bitmap("logo.png")
        Dim image As CPDFStampAnnotation = TryCast(page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP), CPDFStampAnnotation)
        image.SetImageStamp(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height)
        image.SetRect(New CRect(300, 400, 380, 320))
        image.SetTransparency(255)
        image.UpdateAp()
    End Sub

    Private Function CreateAnnots(document As CPDFDocument) As Boolean
        CreateFreetextAnnotation(document)
        CreateFreehandAnnotation(document)
        CreateShapeAnnotation(document)
        CreateNoteAnnotation(document)
        CreateShapeAnnotation(document)
        CreateSoundAnnotation(document)
        CreateMarkupAnnotation(document)
        CreateStampAnnotation(document)
        Dim path As String = outputPath & "\CreateAnnotsTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)
        Return True
    End Function

    Private Function DeleteAnnotations(document As CPDFDocument) As Boolean
        Dim page As CPDFPage = document.PageAtIndex(0)

        Dim annotList As List(Of CPDFAnnotation) = page.GetAnnotations()
        Dim annotNum = annotList.Count

        If Not annotList(0).RemoveAnnot() Then
            Return False
        End If

        Dim path As String = outputPath & "\DeleteAnnotsTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)

        Return True
    End Function


End Module
