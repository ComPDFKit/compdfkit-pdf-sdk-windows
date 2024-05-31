using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.IO; 
using System.Windows;

namespace TextSearchTest
{
    internal class TextSearch
    {
        private static string parentPath =
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())));
        private static string outputPath = Path.Combine(parentPath, "Output", "CS");
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running text search test sample…\r\n");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            SDKLicenseHelper.LicenseVerify();

            #endregion

            #region Sample 1: Search text

            CPDFDocument document = CPDFDocument.InitWithFilePath("Text.pdf");

            SearchText(document);
            document.Release();
            Console.WriteLine("--------------------");

            #endregion
            
            Console.WriteLine("Done");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        /// Search for keywords in the current page and record the search results.
        /// </summary> 
        static private void SearchForPage(CPDFPage page, string searchKeywords, C_Search_Options option, ref List<CRect> rects, ref List<string> strings)
        {
            rects = new List<CRect>();
            strings = new List<string>();
            int findIndex = 0;

            CPDFTextPage textPage = page.GetTextPage();
            CPDFTextSearcher searcher = new CPDFTextSearcher();

            if (searcher.FindStart(textPage, searchKeywords, option, 0))
            {
                CRect textRect = new CRect();
                string textContent = "";
                while (searcher.FindNext(page, textPage, ref textRect, ref textContent, ref findIndex))
                {
                    strings.Add(textContent);
                    rects.Add(new CRect(textRect.left, textRect.bottom, textRect.right, textRect.top));
                }
            }
        }

        /// <summary>
        ///  Highlight the first result
        /// </summary> 
        static private bool HighlightTheFirstResult(CPDFPage page, CRect rect)
        {
            List<CRect> cRectList = new List<CRect>();
            cRectList.Add(new CRect(rect.left, rect.bottom, rect.right, rect.top));

            CPDFHighlightAnnotation annotation = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT) as CPDFHighlightAnnotation;
            byte[] color = { 0, 255, 0 };
            annotation.SetColor(color);
            annotation.SetTransparency(120);
            annotation.SetQuardRects(cRectList);
            annotation.UpdateAp();
            return true;
        }

        /// <summary>
        /// Search PDF keywords on the first page of the article, 
        /// after the search is completed, 
        /// highlight the first searched keyword and save it
        /// </summary>
        /// <param name="document">PDF with text</param> 
        static private bool SearchText(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            //rects: The collection of locales where keywords are located.
            List<CRect> rects = new List<CRect>();
            //strings: The full text of the keyword's area
            List<string> strings = new List<string>();

            //Search for single page
            SearchForPage(page, "PDF", C_Search_Options.Search_Case_Insensitive, ref rects, ref strings);

            Console.WriteLine("The pdf have {0} results", rects.Count);

            Console.WriteLine("Search finished, now highlight the first result. ");

            //Highlight the first result
            HighlightTheFirstResult(page, rects[0]);

            string path = Path.Combine(outputPath, "HighlightFirstTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

    }
}
