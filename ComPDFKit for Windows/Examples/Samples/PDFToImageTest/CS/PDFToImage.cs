using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        static private void SaveWriteableBitmapAsPng(WriteableBitmap writeableBitmap, string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(stream);
            }
        }

        /// <summary>
        /// Convert PDF to image
        /// </summary>
        static private bool PDFPageToImage(CPDFDocument document)
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                CPDFPage pdfPage = document.PageAtIndex(i, true);
                CSize pageSize = document.GetPageSize(0);
                CRect pageRect = new CRect(0, 0, (int)(pageSize.width / 72.0 * 96), (int)(pageSize.height / 72.0 * 96));
                byte[] bmpData = new byte[(int)(pageRect.width() * pageRect.width() * (96 / 72.0) * (96 / 72.0) * 4)];
                pdfPage.RenderPageBitmapWithMatrix((float)(96 / 72.0), pageRect, 0xFFFFFFFF, bmpData, 0, true);

                WriteableBitmap writeableBitmap = new WriteableBitmap((int)pageRect.width(), (int)pageRect.height(), 96, 96, PixelFormats.Bgra32, null);

                writeableBitmap.WritePixels(new Int32Rect(0, 0, (int)pageRect.width(), (int)pageRect.height()), bmpData, writeableBitmap.BackBufferStride, 0);
                var path = outputPath + @"\PDFToImageTest" + i + ".png";
                SaveWriteableBitmapAsPng(writeableBitmap, path);
                Console.WriteLine("Png image saved in {0}", path);
            }
            return false;
        }
    }
}
