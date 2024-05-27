using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ComPDFKit.Import;
using ImageMagick;

namespace PDFToImageTest
{
    internal class PDFToImage
    {
        private static string parentPath =
                    Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        private static string outputPath = parentPath + @"\Output\CS";
        static void Main(string[] args)
        {
            Console.WriteLine("Running PDFToImage test sample…" + Environment.NewLine);

            SDKLicenseHelper.LicenseVerify();
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            PDFPageToImage(document);
            PDFToImage2(document);
            Console.WriteLine("--------------------");
            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        private static void PDFToImage2(CPDFDocument document)
        {
            document.PdfToImage("1", outputPath);
        }

        /// <summary>
        /// Convert PDF to image
        /// </summary>
        static private bool PDFPageToImage(CPDFDocument document)
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                CPDFPage pdfPage = document.PageAtIndex(i, true);
                CSize cSize = document.GetPageSize(0);
                System.Drawing.Size pageSize = new System.Drawing.Size((int)cSize.width, (int)cSize.height);
                CRect pageRect = new CRect(0, (int)(pageSize.Height / 72.0 * 96), (int)(pageSize.Width / 72.0 * 96), 0);
                byte[] bmpData = new byte[(int)(pageRect.width() * pageRect.height() * (96 / 72.0) * (96 / 72.0) * 4)];
                pdfPage.RenderPageBitmapWithMatrix((float)(96 / 72.0), pageRect, 0xFFFFFFFF, bmpData, 0, true);
                var path = Path.Combine(outputPath, "PDFToImageTest" + i + ".png");

                var settings = new MagickReadSettings()
                {
                    Format = MagickFormat.Bgra,
                    Width = (int)pageRect.width(),
                    Height = (int)pageRect.height()
                };
                using (var image = new MagickImage(bmpData,settings))
                {
                    image.Format = MagickFormat.Png;
                    image.Write(path);
                }
                Console.WriteLine("Png image saved in {0}", path);
            }
            return false;
        }
    }
}
