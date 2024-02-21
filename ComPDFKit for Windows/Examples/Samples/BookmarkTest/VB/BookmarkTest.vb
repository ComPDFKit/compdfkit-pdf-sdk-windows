Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module BookmarkTest
    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main()
        ' Preparation work
        Console.WriteLine("Running Bookmark test sample…" + Environment.NewLine)
        SDKLicenseHelper.LicenseVerify()
        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf")

#Region "Sample 1: Access bookmark"

        If AccessBookmark(document) Then
            Console.WriteLine("Check bookmark list done.")
        Else
            Console.WriteLine("Check bookmark list failed.")
            Console.WriteLine("--------------------")
            Console.WriteLine("Stop.")
            Console.WriteLine("--------------------")
            Console.ReadLine()
            Return
        End If
        document.Release()
        Console.WriteLine("--------------------")

#End Region

#Region "Sample 2: Create bookmark"

        document = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf")

        If CreateBookmark(document) Then
            Console.WriteLine("Add bookmark done.")
        Else
            Console.WriteLine("Add bookmark failed.")
        End If
        document.Release()
        Console.WriteLine("--------------------")

#End Region

#Region "Sample 3: Remove bookmark"

        document = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf")

        If RemoveBookmark(document) Then
            Console.WriteLine("Remove bookmark done.")
        Else
            Console.WriteLine("Remove bookmark failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

#End Region

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()



    End Sub

    ''' <summary>
    ''' Access bookmark
    ''' </summary> 
    Private Function AccessBookmark(document As CPDFDocument) As Boolean
        Dim bookmarkList As List(Of CPDFBookmark) = document.GetBookmarkList()

        ' Check if there are 3 bookmarks in the document
        If bookmarkList.Count = 3 Then
            Console.WriteLine("Access bookmark list done.")
        Else
            Return False
        End If

        ' Check the title of the bookmark for page index 0
        If document.BookmarkForPageIndex(0).Title = "Bookmark1" Then
            Console.WriteLine("Access bookmark for a page done.")
        Else
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Create bookmark
    ''' </summary> 
    Private Function CreateBookmark(document As CPDFDocument) As Boolean
        Dim bookmarkCount As Integer = document.GetBookmarkList().Count

        Dim bookmark As New CPDFBookmark()
        bookmark.Title = "new bookmark"
        bookmark.PageIndex = 4

        ' Add the new bookmark to the document
        document.AddBookmark(bookmark)

        ' Check if the number of bookmarks has increased by 1
        If document.GetBookmarkList().Count - bookmarkCount = 1 Then
            Console.WriteLine("Add bookmark in page {0}. ", bookmark.PageIndex + 1)
        Else
            Return False
        End If

        ' Save the modified document
        Dim addBookmarkPath As String = outputPath & "//AddBookmarkTest.pdf"
        If document.WriteToFilePath(addBookmarkPath) Then
            Console.WriteLine("Browse the changed file in " & addBookmarkPath)
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Remove bookmark
    ''' </summary> 
    Private Function RemoveBookmark(document As CPDFDocument) As Boolean
        Dim bookmarkCount As Integer = document.GetBookmarkList().Count

        ' Remove the first bookmark
        document.RemoveBookmark(0)

        ' Check if the number of bookmarks has decreased by 1
        If bookmarkCount - document.GetBookmarkList().Count = 1 Then
            Console.WriteLine("Bookmark removed.")
        Else
            Return False
        End If

        ' Save the modified document
        Dim removeBookmarkPath As String = outputPath & "//RemoveBookmarkTest.pdf"
        If document.WriteToFilePath(removeBookmarkPath) Then
            Console.WriteLine("Browse the changed file in " & removeBookmarkPath)
            Return True
        Else
            Return False
        End If
    End Function

End Module
