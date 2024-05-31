using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ImageMagick;

namespace WatermarkTest
{
    internal class WatermarkTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running Watermark test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
             
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Sample 1: Add text watermark.

            if (AddTextWatermark(document))
            {
                Console.WriteLine("Add text watermark done.");
            }
            else
            {
                Console.WriteLine("Add text watermark failed.");
            }

            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 2: Add image watermark

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (AddImageWatermark(document))
            {
                Console.WriteLine("Add image watermark done.");
            }
            else
            {
                Console.WriteLine("Add image watermark failed.");
            }

            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Delete watermark
             
            CPDFDocument textWatermarkDocument = CPDFDocument.InitWithFilePath("Watermark.pdf");

            if (DeleteWatermark(textWatermarkDocument))
            {
                Console.WriteLine("Delete watermark done.");
            }
            else
            {
                Console.WriteLine("Delete watermark failed.");
            }

            textWatermarkDocument.Release();

            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        /// Add text watermark
        /// </summary>
        /// <param name="document">Regular document without watermark.</param> 
        static private bool AddTextWatermark(CPDFDocument document)
        {
            CPDFWatermark watermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_TEXT);
            watermark.SetText("test");
            watermark.SetFontName("Helvetica"); 
            watermark.SetPages("0-3");
            byte[] color = { 255, 0, 0 };
            watermark.SetTextRGBColor(color);
            watermark.SetScale(2);
            watermark.SetRotation(0);
            watermark.SetOpacity(120);
            watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER);
            watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER);
            watermark.SetVertOffset(0);
            watermark.SetHorizOffset(0);
            watermark.SetFront(true);
            watermark.SetFullScreen(true);
            watermark.SetVerticalSpacing(10);
            watermark.SetHorizontalSpacing(10);
            watermark.CreateWatermark();
            string path = Path.Combine(outputPath, "AddTextWatermarkTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }
        

        /// <summary>
        /// Add image watermark
        /// </summary>
        /// <param name="document">Image source to be used as a image watermark.</param> 
        static private bool AddImageWatermark(CPDFDocument document)
        {
            CPDFWatermark watermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_IMG);
            
            using (var image = new MagickImage("ComPDFKit_Logo.ico"))
            {
                byte[] byteArray = image.ToByteArray(MagickFormat.Bgra);
                watermark.SetImage(byteArray, image.Width, image.Height);
            }
            watermark.SetPages("0-3");
            watermark.SetScale(2);
            watermark.SetRotation(1);
            watermark.SetOpacity(128);
            watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER);
            watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER);
            watermark.SetVertOffset(0);
            watermark.SetHorizOffset(0);
            watermark.SetFront(false);
            watermark.SetFullScreen(true);
            watermark.SetVerticalSpacing(10);
            watermark.SetHorizontalSpacing(10);
            watermark.CreateWatermark();

            string path = Path.Combine(outputPath, "AddImageWatermarkTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true; 
        }

        /// <summary>
        /// Delete watermark
        /// </summary>
        /// <param name="watermarkDocument">Documents with text watermarks</param> 
        static private bool DeleteWatermark(CPDFDocument watermarkDocument)
        {
            watermarkDocument.DeleteWatermarks();
            string path = Path.Combine(outputPath, "DeleteWatermarkTest.pdf");
            if (!watermarkDocument.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }
    }
}
