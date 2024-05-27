using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO;

namespace BookmarkTest
{
    internal class BookmarkTest
    {
        static private string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        static private string outputPath = Path.Combine(parentPath, "Output", "CS");

        static void Main(string[] args)
        {
            #region Preparation work
            Console.WriteLine("Running Bookmark test sample…" + Environment.NewLine);
            SDKLicenseHelper.LicenseVerify();
            CPDFDocument document = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Sample 1: Access bookmark

            if (AccessBookmark(document))
            {
                Console.WriteLine("Check bookmark list done.");
            }
            else
            {
                Console.WriteLine("Check bookmark list failed.");
                Console.WriteLine("--------------------");
                Console.WriteLine("Stop.");
                Console.WriteLine("--------------------");
                Console.ReadLine();
                return;
            }
            document.Release();
            Console.WriteLine("--------------------");
             
            #endregion

            #region Sample 2: Create bookmark

            document = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf");

            if (CreateBookmark(document))
            {
                Console.WriteLine("Add bookmark done.");
            }
            else
            {
                Console.WriteLine("Add bookmark failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Remove bookmark

            document = CPDFDocument.InitWithFilePath("ThreeBookmark.pdf");

            if (RemoveBookmark(document))
            {
                Console.WriteLine("Remove bookmark done.");
            }
            else
            {
                Console.WriteLine("Remove bookmark done.");
            }

            document.Release();
            Console.WriteLine("--------------------");

            #endregion
            
            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        /// Access bookmark
        /// </summary> 
        static private bool AccessBookmark(CPDFDocument document)
        {
            List<CPDFBookmark> bookmarkList = document.GetBookmarkList();
            if (bookmarkList.Count == 3)
            {
                Console.WriteLine("Access bookmark list done.");
            }
            else
            {
                return false;
            }
            if (document.BookmarkForPageIndex(0).Title == "Bookmark1")
            {
                Console.WriteLine("Access bookmark for a page done.");
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create bookmark
        /// </summary> 
        static private bool CreateBookmark(CPDFDocument document)
        {
            var bookmarkCount = document.GetBookmarkList().Count;
            CPDFBookmark bookmark = new CPDFBookmark();
            bookmark.Title = "new bookmark";
            bookmark.PageIndex = 4;
            document.AddBookmark(bookmark);
            if (!(document.GetBookmarkList().Count - bookmarkCount == 1))
            {
                return false;
            }
            Console.WriteLine("Add bookmark in page {0}. ", bookmark.PageIndex + 1);
            string addBookmarkPath = Path.Combine(outputPath, "AddBookmarkTest.pdf");
            if (document.WriteToFilePath(addBookmarkPath))
            {
                Console.WriteLine("Browse the changed file in " + addBookmarkPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove bookmark
        /// </summary> 
        static private bool RemoveBookmark(CPDFDocument document)
        {
            var bookmarkCount = document.GetBookmarkList().Count;
            document.RemoveBookmark(0);
            if (!(bookmarkCount - document.GetBookmarkList().Count == 1))
            {
                return false;
            }
            string removeBookmarkPath = Path.Combine(outputPath, "RemoveBookmarkTest.pdf");
            if (document.WriteToFilePath(removeBookmarkPath))
            {
                Console.WriteLine("Browse the changed file in " + removeBookmarkPath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
