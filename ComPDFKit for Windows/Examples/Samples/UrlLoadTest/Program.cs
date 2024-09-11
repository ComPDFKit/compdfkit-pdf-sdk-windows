using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComPDFKit.PDFDocument;

namespace UrlLoadTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #region Perparation work

            Console.WriteLine("Running Url Load test sample…\r\n");
            bool statue = SDKLicenseHelper.LicenseVerify();
            #endregion

            #region Sample 1: Open File With Url
            if (statue)
            {
                string url = "https://www.compdf.com/webviewer/example/developer_guide_web.pdf";
                CPDFDocument pdfDoc = CPDFDocument.InitWithUrl(url);
                if (pdfDoc != null)
                {
                    Console.WriteLine("Load File With Url Done.");
                    pdfDoc.Release();
                }
            }
            #endregion

            Console.WriteLine("Done.");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }
    }
}
