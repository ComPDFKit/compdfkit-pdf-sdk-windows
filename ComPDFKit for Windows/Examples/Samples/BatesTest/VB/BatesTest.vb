Imports System.IO
Imports ComPDFKit.PDFDocument

Module BatesTest
    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")
    Private IntToLocationDic As New Dictionary(Of Integer, String)() From
    {
        {0, "Top Left"},
        {1, "Top Middle"},
        {2, "Top Right"},
        {3, "Bottom Left"},
        {4, "Bottom Middle"},
        {5, "Bottom Right"}
    }

    Sub Main()
#Region "Preparation work"
        Console.WriteLine("Running bates test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
#End Region

#Region "Sample 1: Add bates"
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If AddBates(document) Then
            Console.WriteLine("Add bates done.")
        End If

        document.Release()

        Console.WriteLine("--------------------")
#End Region

#Region "Samples 2: Edit bates"
        document = CPDFDocument.InitWithFilePath("Bates.pdf")

        If EditBates(document) Then
            Console.WriteLine("Edit bates done.")
        End If

        document.Release()

        Console.WriteLine("--------------------")
#End Region

#Region "Sample 3: Clear bates"
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If ClearBates(document) Then
            Console.WriteLine("Clear bates done.")
        End If

        document.Release()
        Console.WriteLine("--------------------")
#End Region

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")

        Console.ReadLine()

    End Sub

    Private Function AddBates(document As CPDFDocument) As Boolean
        Dim addBatesPath As String = outputPath & "\AddBatesTest.pdf"

        Dim bates As CPDFBates = document.GetBates()
        Dim color As Byte() = {255, 0, 0}

        bates.SetPages("0-" & (document.PageCount - 1)) ' Page numbering from 0

        For i As Integer = 0 To 5
            bates.SetText(i, "<<#3#5#Prefix-#-Suffix>>") ' 3 digits, starting from 5
            bates.SetTextColor(i, color)
            bates.SetFontSize(i, 14)

            Console.WriteLine("Text: {0}", bates.GetText(i))
            Console.WriteLine("Location: {0}" & vbCrLf, IntToLocationDic(i))
        Next

        bates.Update()

        If Not document.WriteToFilePath(addBatesPath) Then
            Return False
        End If

        Console.WriteLine("Browse the changed file in " & addBatesPath)
        Return True
    End Function

    Private Function EditBates(document As CPDFDocument) As Boolean
        Dim bates As CPDFBates = document.GetBates()

        ' Get the old Bates text from index 0
        If Not String.IsNullOrEmpty(bates.GetText(0)) Then
            Console.WriteLine("Get old Bates 0 done, text is {0}", bates.GetText(0))
        Else
            Console.WriteLine("Get Bates 0 failed, or it does not exist")
            Return False
        End If

        ' Edit the Bates text at index 0
        bates.SetText(0, "<<#3#1#ComPDFKit-#-ComPDFKit>>")

        ' Update the Bates text
        bates.Update()

        Console.WriteLine("Change Bates 0 done, new text is {0}", bates.GetText(0))

        Dim editBatesPath As String = outputPath & "\EditBatesTest.pdf"

        If document.WriteToFilePath(editBatesPath) Then
            Console.WriteLine("Browse the changed file in " & editBatesPath)
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Clear bates.
    ''' </summary>
    ''' <param name="document">Document with bates</param>
    Private Function ClearBates(document As CPDFDocument) As Boolean
        Dim bates As CPDFBates = document.GetBates()

        bates.Clear()

        Dim clearBatesPath As String = outputPath & "\ClearBatesTest.pdf"

        If document.WriteToFilePath(clearBatesPath) Then
            Console.WriteLine("Browse the changed file in " & clearBatesPath)
            Return True
        Else
            Return False
        End If
    End Function


End Module
