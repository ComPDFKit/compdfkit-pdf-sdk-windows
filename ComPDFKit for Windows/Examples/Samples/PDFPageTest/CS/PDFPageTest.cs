using ComPDFKit.PDFDocument;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFPageTest
{
    internal class PDFPageTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running PDFPage test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Sample 1: Insert blank page

            if (InsertBlankPage(document))
            {
                Console.WriteLine("Insert blank page done.");
            }
            else
            {
                Console.WriteLine("Insert blank page failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 2: Insert PDF page

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (InsertPDFPPage(document))
            {
                Console.WriteLine("Insert PDF page done.");
            }
            else
            {
                Console.WriteLine("Insert PDF page failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Split pages

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (SplitPages(document))
            {
                Console.WriteLine("Split page done.");
            }
            else
            {
                Console.WriteLine("Split failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 4: Remove pages

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (RemovePages(document))
            {
                Console.WriteLine("Delete even page done.");
            }
            else
            {
                Console.WriteLine("Delete even page failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 5: Rotate page

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (RotatePage(document))
            {
                Console.WriteLine("Rotate page done.");
            }
            else
            {
                Console.WriteLine("Rotate page failed.");
            }

            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 6: Repalce pages

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (RepalcePages(document))
            {
                Console.WriteLine("Repalce page done.");
            }
            else
            {
                Console.WriteLine("Repalce page failed.");
            }

            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 7: Extract pages
            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (ExtractPages(document))
            {
                Console.WriteLine("Extract page done.");
            }
            else
            {
                Console.WriteLine("Extract page failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Insert a new page of A4 size at the second page
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool InsertBlankPage(CPDFDocument document)
        {
            int pageIndex = 1;
            int pageWidth = 595;
            int pageHeight = 842;
            document.InsertPage(pageIndex, pageWidth, pageHeight, "");
            Console.WriteLine("Insert PageIndex: {0}", pageIndex);
            Console.WriteLine("Size: {0}*{1}", pageWidth, pageHeight);

            string path = Path.Combine(outputPath, "InsertBlankPageTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        /// <summary>
        /// Select pages from other PDF files and insert them into the current document
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool InsertPDFPPage(CPDFDocument document)
        {
            CPDFDocument documentForInsert = CPDFDocument.InitWithFilePath("Text.pdf");
            document.ImportPagesAtIndex(documentForInsert, "1", 1);

            string path = Path.Combine(outputPath, "InsertPDFPPageTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        /// <summary>
        /// Split the current document into two documents according to the first 2 pages and the last 3 pages
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool SplitPages(CPDFDocument document)
        {
            //Split 1-2 pages
            CPDFDocument documentPart1 = CPDFDocument.CreateDocument();
            documentPart1.ImportPagesAtIndex(document, "1-2", 0);

            string pathPart1 = Path.Combine(outputPath, "SplitPart1Test.pdf");
            if (!documentPart1.WriteToFilePath(pathPart1))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + pathPart1);

            //Split 3-5 pages
            CPDFDocument documentPart2 = CPDFDocument.CreateDocument();
            documentPart2.ImportPagesAtIndex(document, "3-5", 0);

            string pathPart2 = Path.Combine(outputPath, "SplitPart2Test.pdf");
            if (!documentPart2.WriteToFilePath(pathPart2))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + pathPart2);
            return true;
        }

        /// <summary>
        /// Remove even-numbered pages from a document
        /// </summary>
        /// <param name="document">Regular five page document</param> 
        static private bool RemovePages(CPDFDocument document)
        {
            List<int> pageNumbersToRemove = new List<int>();

            for (int i = 1; i < document.PageCount; i += 2)
            {
                pageNumbersToRemove.Add(i);
            }

            document.RemovePages(pageNumbersToRemove.ToArray());
             

            string path = Path.Combine(outputPath, "RemoveEvenPagesTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        /// <summary>
        ///  Rotate the first page 90 degrees clockwise
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool RotatePage(CPDFDocument document)
        {
            document.RotatePage(0, 1);//Rotation: Rotate 90 degrees per unit
            string path = Path.Combine(outputPath, "RotatePageTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        /// <summary>
        /// Replace the first page of the current document with a page from another document
        /// Delete the pages that need to be replaced first
        /// Insert the required pages into the document
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool RepalcePages(CPDFDocument document)
        {
            List<int> pageList = new List<int>() { 0 }; 
            document.RemovePages(pageList.ToArray());
            CPDFDocument documentForInsert = CPDFDocument.InitWithFilePath("Text.pdf");
            document.ImportPagesAtIndex(documentForInsert, "1", 0);
            string path = Path.Combine(outputPath, "RepalcePagesTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        /// <summary>
        /// Extract pages from a document
        /// Create a new document
        /// Insert the required pages into a new document
        /// </summary>
        /// <param name="document">Regular five page document</param>
        static private bool ExtractPages(CPDFDocument document)
        {
            CPDFDocument extractDocument = CPDFDocument.CreateDocument();
            extractDocument.ImportPagesAtIndex(document, "1", 0);
            string path = Path.Combine(outputPath, "ExtractPagesTest.pdf");
            if (!extractDocument.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }
    }
}
