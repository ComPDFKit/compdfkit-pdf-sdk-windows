using ComPDFKit.PDFDocument;
using System.IO;
using System;

namespace UrlLoadTest
{
    internal class UrlLoadTest
    {
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running Stream Load test sample…\r\n");
            bool statue = SDKLicenseHelper.LicenseVerify();
            #endregion

            #region Sample 1: Open Local File With Stream
            if (statue && File.Exists("PDF32000_2008.pdf"))
            {
                FileStream fs = File.OpenRead("PDF32000_2008.pdf");
                CPDFDocument pdfDoc = CPDFDocument.InitWithStream(fs);
                if (pdfDoc != null)
                {
                    Console.WriteLine("Load File With Stream Done.");
                    pdfDoc.Release();
                    fs.Close();
                }
            }

            #endregion

            Console.WriteLine("Done.");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }
    }
}
