using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO;

namespace HeaderFooterTest
{
    internal class HeaderFooterTest
    {
        private static string outputPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");

        private static Dictionary<int, string> IntToLocationDic = new Dictionary<int, string>()
        {
            {0, "Top Left" },
            {1, "Top Middle" },
            {2, "Top Right" },
            {3, "Bottom Left" },
            {4, "Bottom Middle" },
            {5, "Bottom Right" }
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Running header and footer test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #region Add common header and footer
            if (AddCommonHeaderFooter(document))
            {
                Console.WriteLine("Add common header and footer done.");
            }
            else
            {
                Console.WriteLine("Add common header and footer failed.");
            }
            Console.WriteLine("--------------------");

            #endregion
            #region Add page header and footer 
            if (AddPageHeaderFooter(document))
            {
                Console.WriteLine("Add page header and footer done.");
            }
            else
            {
                Console.WriteLine("Add page header and footer failed.");
            }
            #endregion
            Console.WriteLine("--------------------");

            #region Edit header and footer 
            if (EditHeaderFooter(document))
            {
                Console.WriteLine("Edit header and footer done.");
            }
            else
            {
                Console.WriteLine("Edit header and footer failed.");
            }
            #endregion
            Console.WriteLine("--------------------");

            #region Delete header and footer 
            if (ClearHeaderFooter(document))
            {
                Console.WriteLine("Delete header and footer done.\n");
            }
            else
            {
                Console.WriteLine("delete header and footer failed\n");
            }
            #endregion
            Console.WriteLine("--------------------");
            Console.WriteLine("Done");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Follow these steps to add a header and footer in a blank pages file.  
        /// </summary>
        /// <param name="document">Regular document</param>
        static private bool AddCommonHeaderFooter(CPDFDocument document)
        {

            CPDFHeaderFooter headerFooter = document.GetHeaderFooter();
            byte[] color = { 255, 0, 0 };
            headerFooter.SetPages("0-" + (document.PageCount - 1));

            for (int i = 0; i <= 2; i++)
            {
                headerFooter.SetText(i, "ComPDFKit");
                headerFooter.SetTextColor(i, color);
                headerFooter.SetFontSize(i, 14);

                Console.WriteLine("Text: {0}", headerFooter.GetText(i));
                Console.WriteLine("Location: {0}\n", IntToLocationDic[i]);
            }

            headerFooter.Update();

            string addHeaderFooterPath = Path.Combine(outputPath, "AddCommonHeaderFooterTest.pdf");

            if (!document.WriteToFilePath(addHeaderFooterPath))
            {
                return false; 
            }
            Console.WriteLine("Browse the changed file in " + addHeaderFooterPath);
            return true;
        }

        /// <summary>
        /// Add headers and footers that automatically display page numbers
        /// </summary>
        /// <param name="document">Regular document</param>
        static private bool AddPageHeaderFooter(CPDFDocument document)
        {
            CPDFHeaderFooter headerFooter = document.GetHeaderFooter();
            byte[] color = { 255, 0, 0 };
            headerFooter.SetPages("0-" + (document.PageCount - 1));
            for (int i = 3; i <= 5; i++)
            {
                headerFooter.SetText(i, "<<1,2>>");
                headerFooter.SetTextColor(i, color);
                headerFooter.SetFontSize(i, 14);

                Console.WriteLine("Text: {0}", headerFooter.GetText(i));
                Console.WriteLine("Location: {0}\n", IntToLocationDic[i]);
            }

            headerFooter.Update();

            string addHeaderFooterPath = Path.Combine(outputPath, "AddPageHeaderFooterTest.pdf");

            if (document.WriteToFilePath(addHeaderFooterPath))
            {
                Console.WriteLine("Browse the changed file in " + addHeaderFooterPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Follow these steps to delete a header and footer in a blank pages file.  
        /// </summary>
        /// <param name="document">Documents that require manipulation</param>
        static private bool EditHeaderFooter(CPDFDocument document)
        {
            CPDFHeaderFooter headerFooter = document.GetHeaderFooter();

            if (headerFooter.GetText(0) != string.Empty)
            {
                Console.WriteLine("Get old head and footer 0 succeeded, text is {0}", headerFooter.GetText(0));
            }
            else
            {
                Console.WriteLine("Get head and footer 0 failed, or it does not exist");
                return false;
            }

            headerFooter.SetText(0, "ComPDFKit Samples");

            headerFooter.Update();

            Console.WriteLine("Change head and footer 0 succeeded, new text is {0}", headerFooter.GetText(0));

            string editHeaderFooterPath = Path.Combine(outputPath, "EditHeaderFooterTest.pdf");

            if (document.WriteToFilePath(editHeaderFooterPath))
            {
                Console.WriteLine("Browse the changed file in " + editHeaderFooterPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        static private bool ClearHeaderFooter(CPDFDocument document)
        {
            CPDFHeaderFooter headerFooter = document.GetHeaderFooter();

            headerFooter.Clear();

            string clearHeaderFooterPath = Path.Combine(outputPath, "ClearHeaderFooterTest.pdf");

            if (document.WriteToFilePath(clearHeaderFooterPath))
            {
                Console.WriteLine("Browse the changed file in " + clearHeaderFooterPath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
