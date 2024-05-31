Imports ComPDFKit.Import
Imports ComPDFKit.PDFAnnotation
Imports ComPDFKit.PDFAnnotation.Form
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFDocument.Action
Imports ComPDFKit.PDFPage
Imports System.IO

Module InteractiveFormsTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"

    Sub Main()
        Console.WriteLine("Running interactive forms test sample…" & vbCrLf)
        SDKLicenseHelper.LicenseVerify()
        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        If CreateForms(document) Then
            Console.WriteLine("Create forms done.")
        Else
            Console.WriteLine("Create forms failed.")
        End If
        Console.WriteLine("--------------------")
        Console.WriteLine("Done")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    ' Create text field.
    ' Text field: used to input text
    Private Sub CreateTextField(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim textField As CPDFTextWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_TEXTFIELD), CPDFTextWidget)
        textField.SetRect(New CRect(28, 75, 235, 32))
        textField.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        textField.SetWidgetBgRGBColor(New Byte() {240, 255, 240})
    End Sub

    ' Create push button.
    ' Push button: Click to perform some actions, such as jumping to a page or website.
    Private Sub CreatePushButton(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim pushButton1 As CPDFPushButtonWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_PUSHBUTTON), CPDFPushButtonWidget)
        pushButton1.SetRect(New CRect(28, 150, 150, 100))
        pushButton1.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        pushButton1.SetWidgetBgRGBColor(New Byte() {180, 180, 220})
        pushButton1.SetButtonTitle("Go To Page 2")
        Dim attribute As New CTextAttribute()
        attribute.FontColor = New Byte() {0, 0, 0}
        attribute.FontSize = 14
        attribute.FontName = "Helvetica"
        pushButton1.SetTextAttribute(attribute)
        Dim gotoAction As New CPDFGoToAction()
        Dim dest As New CPDFDestination()
        dest.PageIndex = 1
        gotoAction.SetDestination(document, dest)
        pushButton1.SetButtonAction(gotoAction)
        pushButton1.UpdateFormAp()

        Dim pushButton2 As CPDFPushButtonWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_PUSHBUTTON), CPDFPushButtonWidget)
        pushButton2.SetRect(New CRect(168, 150, 290, 100))
        pushButton2.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        pushButton2.SetWidgetBgRGBColor(New Byte() {180, 180, 220})
        pushButton2.SetButtonTitle("Go To ComPDFKit")
        Dim attribute2 As New CTextAttribute()
        attribute2.FontColor = New Byte() {0, 0, 0}
        attribute2.FontSize = 14
        attribute2.FontName = "Helvetica"
        pushButton2.SetTextAttribute(attribute)
        Dim uriAction As New CPDFUriAction()
        uriAction.SetUri("https://www.compdf.com/")
        pushButton2.SetButtonAction(uriAction)
        pushButton2.UpdateFormAp()
    End Sub

    ' Create ListBox
    Private Sub CreateListBox(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim listbox As CPDFListBoxWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_LISTBOX), CPDFListBoxWidget)
        listbox.SetRect(New CRect(28, 330, 150, 230))
        listbox.AddOptionItem(0, "1", "ComPDFKit1")
        listbox.AddOptionItem(1, "2", "ComPDFKit2")
        listbox.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        listbox.SetWidgetBgRGBColor(New Byte() {200, 180, 180})
    End Sub

    ' Create SignatureField
    ' Provide an area for electronic signatures.
    Private Sub CreateSignatureField(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim signatureField As CPDFSignatureWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS), CPDFSignatureWidget)
        signatureField.SetRect(New CRect(28, 420, 150, 370))
        signatureField.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        signatureField.SetWidgetBgRGBColor(New Byte() {150, 180, 210})
    End Sub

    ' Create CheckBox
    Private Sub CreateCheckBox(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim checkBox1 As CPDFCheckBoxWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX), CPDFCheckBoxWidget)
        checkBox1.SetRect(New CRect(28, 470, 48, 450))
        checkBox1.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        checkBox1.SetWidgetBgRGBColor(New Byte() {150, 180, 210})

        Dim checkBox2 As CPDFCheckBoxWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX), CPDFCheckBoxWidget)
        checkBox2.SetRect(New CRect(58, 470, 78, 450))
        checkBox2.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        checkBox2.SetWidgetBgRGBColor(New Byte() {150, 180, 210})

        Dim checkBox3 As CPDFCheckBoxWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX), CPDFCheckBoxWidget)
        checkBox3.SetRect(New CRect(88, 470, 108, 450))
        checkBox3.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        checkBox3.SetWidgetBgRGBColor(New Byte() {150, 180, 210})
    End Sub

    ' Create RadioButton
    Private Sub CreateRadioButton(document As CPDFDocument)
        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim radioButton1 As CPDFRadioButtonWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON), CPDFRadioButtonWidget)
        radioButton1.SetRect(New CRect(28, 500, 48, 480))
        radioButton1.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        radioButton1.SetWidgetBgRGBColor(New Byte() {210, 180, 150})
        radioButton1.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE)

        Dim radioButton2 As CPDFRadioButtonWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON), CPDFRadioButtonWidget)
        radioButton2.SetRect(New CRect(58, 500, 78, 480))
        radioButton2.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        radioButton2.SetWidgetBgRGBColor(New Byte() {210, 180, 150})
        radioButton2.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE)

        Dim radioButton3 As CPDFRadioButtonWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON), CPDFRadioButtonWidget)
        radioButton3.SetRect(New CRect(88, 500, 108, 480))
        radioButton3.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        radioButton3.SetWidgetBgRGBColor(New Byte() {210, 180, 150})
        radioButton3.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE)
    End Sub

    ' Generate various signatures.
    Private Function CreateForms(document As CPDFDocument) As Boolean
        CreateTextField(document)
        CreatePushButton(document)
        CreateListBox(document)
        CreateSignatureField(document)
        CreateCheckBox(document)
        CreateRadioButton(document)

        ' Save to the specified path so you can observe the effect.
        Dim path As String = outputPath & "\CreateFormsTest.pdf"

        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Console.WriteLine("Browse the changed file in " & path)

        Return True
    End Function
End Module
