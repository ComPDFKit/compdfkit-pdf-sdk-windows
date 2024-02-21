using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices; 

namespace WatermarkTest
{
    internal class WatermarkTest
    {
        static private string outputPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) + "\\Output\\CS";
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
            string path = outputPath + "\\AddTextWatermarkTest.pdf";
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }

        /// <summary>
        ///  Convert the bitmap to an array that can be set as an image watermark
        /// </summary>
        /// <param name="bitmap">Image source to be used as a image watermark.</param>
        /// <returns>An array for setting image</returns>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        /// <summary>
        /// Add image watermark
        /// </summary>
        /// <param name="document">Image source to be used as a image watermark.</param> 
        static private bool AddImageWatermark(CPDFDocument document)
        {
            CPDFWatermark watermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_IMG);
            Bitmap bitmap = new Bitmap("logo.png");
            watermark.SetImage(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height);
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

            string path = outputPath + "\\AddImageWatermarkTest.pdf";
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
            string path = outputPath + "\\DeleteWatermarkTest.pdf";
            if (!watermarkDocument.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }
    }
}
