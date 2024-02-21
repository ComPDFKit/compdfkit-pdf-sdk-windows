Imports ComPDFKit.PDFDocument
Imports System
Imports System.Collections.Generic
Imports System.IO

Module OutlineTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"
    Private outlineCounter As Integer = 0
    Private outlineNumber As Integer = 0

    Sub Main()
        ' Perparation work
        Console.WriteLine("Running Outline test sample…" & vbCrLf)

        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Sample 1: Print outline
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("FourOutline.pdf")

        If PrintOutline(document) Then
            Console.WriteLine("Print outline done.")
        Else
            Console.WriteLine("Print outline failed.")
        End If
        document.Release()

        Console.WriteLine("--------------------")

        ' Sample 2: Create outline
        document = CPDFDocument.InitWithFilePath("FourOutline.pdf")

        If CreateOutline(document) Then
            Console.WriteLine("Create outline done.")
        Else
            Console.WriteLine("Create outline failed.")
        End If
        document.Release()

        Console.WriteLine("--------------------")

        ' Sample 3: Move outline
        document = CPDFDocument.InitWithFilePath("FourOutline.pdf")
        If MoveOutline(document) Then
            Console.WriteLine("Move outline done.")
        Else
            Console.WriteLine("Move outline failed.")
        End If
        document.Release()

        Console.WriteLine("--------------------")

        ' Sample 4: Remove outline
        document = CPDFDocument.InitWithFilePath("FourOutline.pdf")
        If RemoveOutline(document) Then
            Console.WriteLine("Remove outline done.")
        Else
            Console.WriteLine("Remove outline failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done.")
        Console.WriteLine("--------------------")

        Console.ReadLine()
    End Sub

    ' Traverse outline and print it as a list
    Private Sub TraverseOutline(outlineList As List(Of CPDFOutline))
        For Each outline In outlineList
            For i As Integer = 0 To outlineCounter - 1
                Console.Write("    ")
            Next
            Console.Write("-> " & outline.Title & vbCrLf)
            outlineNumber += 1
            Dim childList = outline.ChildList
            If childList IsNot Nothing AndAlso childList.Count <> 0 Then
                outlineCounter += 1
                TraverseOutline(childList)
            Else
                Dim i = outlineList.IndexOf(outline)
                If outlineList.IndexOf(outline) + 1 = outlineList.Count Then
                    outlineCounter -= 1
                End If
            End If
        Next
    End Sub

    ' Print all outlines in the file
    Private Function PrintOutline(document As CPDFDocument) As Boolean
        Dim outlineList As List(Of CPDFOutline) = document.GetOutlineList()
        outlineNumber = 0
        outlineCounter = 0
        TraverseOutline(outlineList)
        Console.WriteLine("Outline number: {0}", outlineNumber)
        Return True
    End Function

    ' Create an outline at the top of the first page
    Private Function CreateOutline(document As CPDFDocument) As Boolean
        Dim outline As CPDFOutline = document.GetOutlineRoot()
        Dim childOutline As CPDFOutline = Nothing
        outline.InsertChildAtIndex(document, 0, childOutline)
        childOutline.SetTitle("New outline")

        PrintOutline(document)
        Return True
    End Function

    ' Move outline
    Private Function MoveOutline(document As CPDFDocument) As Boolean
        Dim outline As CPDFOutline = document.GetOutlineRoot()
        outline.InsertChildAtIndex(document, outline.ChildList.Count, outline)
        outline.SetTitle("new outline")
        Dim targetOutline As CPDFOutline = document.GetOutlineList()(1)
        targetOutline.MoveChildAtIndex(document, outline, targetOutline.ChildList.Count)

        Dim path As String = outputPath & "\MoveOutlineTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Dim newDocument As CPDFDocument = CPDFDocument.InitWithFilePath(path)
        PrintOutline(newDocument)
        Return True
    End Function

    ' Remove outline
    Private Function RemoveOutline(document As CPDFDocument) As Boolean
        document.GetOutlineList()(0).RemoveFromParent(document)

        Dim path As String = outputPath & "\RemoveOutlineTest.pdf"
        If Not document.WriteToFilePath(path) Then
            Return False
        End If
        Dim newDocument As CPDFDocument = CPDFDocument.InitWithFilePath(path)
        PrintOutline(newDocument)
        Return True
    End Function
End Module
