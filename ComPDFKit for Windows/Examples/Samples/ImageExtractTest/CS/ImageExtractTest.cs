using ComPDFKit.PDFDocument;
using System;
using System.IO;

namespace ImageExtractTest
{
    internal class ImageExtractTest
    {
        static private string outputPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) + "\\Output\\CS";

        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running Bookmark test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #endregion

            #region Sample 1: Extract image

            ExtractImage(document);
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        /// Extract all images from document
        /// </summary>
        /// <param name="document">Regular documet with some picture</param>
        static private void ExtractImage(CPDFDocument document)
        { 
            document.ExtractImage("1-5", outputPath);
        }
    }
}
