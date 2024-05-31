using ComPDFKit.PDFDocument;
using System;
using System.IO;

namespace TextExtractTest
{
    internal class TextExtractTest
    {
        private static string parentPath =
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())));
        private static string outputPath = Path.Combine(parentPath, "Output", "CS");
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

            if (PDFToText(document))
            {
                Console.WriteLine("PDF to text done.");
            }
            else
            {
                Console.WriteLine("PDF to text failed.");
            }
            Console.WriteLine("--------------------");
            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        //
        static private bool PDFToText(CPDFDocument document)
        {
            string path = Path.Combine(outputPath, "PDFToText.txt");
            if (!document.PdfToText("1-" + document.PageCount.ToString(), path))//Page ranges are counted from 1
            {
                return false;
            }
            Console.WriteLine("Browse the generated file in " + path);
            return true;
        }
    }
}
