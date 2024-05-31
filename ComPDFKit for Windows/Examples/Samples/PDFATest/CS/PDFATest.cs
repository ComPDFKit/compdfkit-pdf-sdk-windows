using ComPDFKit.PDFDocument;
using System;
using System.IO; 

namespace PDFATest
{
    internal class PDFATest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");

        static void Main(string[] args)
        {
            #region Perparation work
             
            Console.WriteLine("Running PDFA test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion
             
            #region PDF/A-1a

            if (CovertToPDFA1a(document))
            {
                Console.WriteLine("Convert to PDF/A-1a done.");
            }
            else
            {
                Console.WriteLine("Convert to PDF/A-1a failed.");
            }

            document.Release();
            
            Console.WriteLine("--------------------");

            #endregion
             
            #region PDF/A-1b

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (CovertToPDFA1b(document))
            {
                Console.WriteLine("Convert to PDF/A-1b done.");
            }
            else
            {
                Console.WriteLine("Convert to PDF/A-1b failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");
            #endregion

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Save PDF as PDFA1a
        /// </summary>
        /// <param name="document">Regular document</param> 
        static public bool CovertToPDFA1a(CPDFDocument document)
        {
            string convertToPDFA1aPath = Path.Combine(outputPath, "ConvertToPDFA1aTest.pdf");
            if (!document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1a, convertToPDFA1aPath))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + convertToPDFA1aPath);
            return true;
        }

        /// <summary>
        /// Save PDF as PDFA1b
        /// </summary>
        /// <param name="document">Regular document</param> 
        static public bool CovertToPDFA1b(CPDFDocument document)
        {
            string convertToPDFA1bPath = Path.Combine(outputPath, "ConvertToPDFA1bTest.pdf");
            if (!document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1b, convertToPDFA1bPath))
            {
                return false;
            }

            Console.WriteLine("Browse the changed file in " + convertToPDFA1bPath);
            return true;
        }
    }
}