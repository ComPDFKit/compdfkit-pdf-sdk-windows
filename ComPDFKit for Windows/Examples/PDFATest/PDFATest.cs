using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFATest
{
    internal class PDFATest
    {
        private static string outputPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) + "\\Output\\PDFA";
        public static bool CovertToPDFA(CPDFDocument document)
        {
            string convertToPDFAPath = outputPath + "\\ConvertToPDFATest.pdf";
            document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1a, convertToPDFAPath);
            return true;
        }

        public static bool CovertToPDFA1b(CPDFDocument document)
        {
            string convertToPDFA1bPath = outputPath + "\\ConvertToPDFA1bTest.pdf";
            document.WritePDFAToFilePath(CPDFType.CPDFTypePDFA1a, convertToPDFA1bPath);
            return false; 
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Running PDFA test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();
             
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            CovertToPDFA(document);
            CovertToPDFA1b(document);
        }
    }
}
