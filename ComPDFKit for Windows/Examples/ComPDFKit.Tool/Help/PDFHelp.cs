using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Viewer.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace ComPDFKit.Tool.Help
{
    public class PDFHelp
    {
        public static List<TextDrawRect> GetSelectTextRect(CPDFDocument document, int pageIndex, Point startPoint, Point endPoint, Point tolerance)
        {
            CPDFPage page = document.PageAtIndex(pageIndex);
            if (page != null)
            {
                CPDFTextPage textPage = page.GetTextPage();
                if (textPage != null)
                {
                    List<CRect> selectList = textPage.GetCharsRectAtPos(
                       DataConversionForWPF.PointConversionForCPoint(startPoint),
                        DataConversionForWPF.PointConversionForCPoint(endPoint),
                        new CPoint(10, 10)
                        );
                    List<TextDrawRect> drawList = new List<TextDrawRect>();
                    foreach (CRect rawRect in selectList)
                    {
                        drawList.Add(new TextDrawRect() { DrawRect = DataConversionForWPF.CRectConversionForRect( rawRect), Text = "" });
                    }
                    return drawList;
                }
            }
            return new List<TextDrawRect>();
        }
     
        public static string GetSelectText(CPDFDocument document, int pageIndex, Point startPoint, Point endPoint,Point tolerance)
        {
            if (document != null)
            {
                CPDFPage page = document.PageAtIndex(pageIndex);
                if (page != null && page.IsValid())
                {
                    CPDFTextPage textPage = page.GetTextPage();
                    if (textPage != null && textPage.IsValid())
                    {
                        return textPage.GetSelectText(
                             DataConversionForWPF.PointConversionForCPoint(startPoint),
                             DataConversionForWPF.PointConversionForCPoint(endPoint),
                             DataConversionForWPF.PointConversionForCPoint(tolerance)
                             );
                    }
                }
            }
            return string.Empty;
        }

        public static string GetDoubleClickText(CPDFDocument document, int pageIndex, Point startPoint, ref Rect uiRect)
        {
            CPDFPage page = document.PageAtIndex(pageIndex);
            if (page != null)
            {
                CPDFTextPage textPage = page.GetTextPage();
                if (textPage != null)
                {
                    CRect wordRect = new CRect();
                   string Word =  textPage.GetSelectionForWordAtPos(
                         DataConversionForWPF.PointConversionForCPoint(startPoint), 
                        new CPoint(5, 5), ref wordRect);
                    uiRect= DataConversionForWPF.CRectConversionForRect(wordRect);
                    return Word;
                }
            }
            return string.Empty;
        }

        public static string GetCurrentPdfTime()
        {
            DateTime localTime = DateTime.Now;
            DateTime utcTime = DateTime.UtcNow;
            int checkHour = (localTime - utcTime).Hours;
            if (checkHour > 0)
            {
                return "D:" + localTime.ToString("yyyyMMddHHmmss") + "+" + checkHour.ToString("D2") + "'00'";
            }
            if (checkHour < 0)
            {
                return "D:" + localTime.ToString("yyyyMMddHHmmss") + "-" + Math.Abs(checkHour).ToString("D2") + "'00'";
            }
            return "D:" + localTime.ToString("yyyyMMddHHmmss") + "Z00'00'";
        }

        public static bool IsTextAtPos(CPDFDocument document, int pageIndex, Point pagePoint)
        {

            if (document==null)
            {
                return false;
            }
            CPDFPage page = document.PageAtIndex(pageIndex);
            if (page == null)
            {
                return false;
            }
            CPDFTextPage textPage = page.GetTextPage();
            if (textPage == null)
            {
                return false;
            }
            var x = textPage.GetCharIndexAtPos(
                DataConversionForWPF.PointConversionForCPoint(pagePoint),
                new CPoint(10, 10)
                );
            return x != -1;
        }

        public static void ImagePathToByte(string imagePath, ref byte[] imageData, ref int imageWidth, ref int imageHeight)
        {
            if (!File.Exists(imagePath))
                return;

            imagePath = Path.GetFullPath(imagePath);
            BitmapFrame frame = null;
            BitmapDecoder decoder = BitmapDecoder.Create(new Uri(imagePath), BitmapCreateOptions.None, BitmapCacheOption.Default);
            if (decoder != null && decoder.Frames.Count > 0)
            {
                frame = decoder.Frames[0];
            }
            if (frame != null)
            {
                imageData = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                if (frame.Format != PixelFormats.Bgra32)
                {
                    FormatConvertedBitmap covert = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, frame.Palette, 0);
                    covert.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }
                else
                {
                    frame.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }

                imageWidth = frame.PixelWidth;
                imageHeight = frame.PixelHeight;
            }
        }

        public static void ImageStreamToByte(Stream imageStream, ref byte[] imageData, ref int imageWidth, ref int imageHeight)
        {
            imageStream.Seek(0, SeekOrigin.Begin);
            BitmapDecoder decoder = BitmapDecoder.Create(imageStream,BitmapCreateOptions.None,BitmapCacheOption.Default);
            BitmapFrame frame = null;
            if (decoder != null && decoder.Frames.Count > 0)
            {
                frame = decoder.Frames[0];
            }

            if (frame != null)
            {
                imageData = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                if (frame.Format != PixelFormats.Bgra32)
                {
                    FormatConvertedBitmap covert = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, frame.Palette, 0);
                    covert.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }
                else
                {
                    frame.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }

                imageWidth = frame.PixelWidth;
                imageHeight = frame.PixelHeight;
            }
        }
    }

    public static class PDFDateHelp
    {
        /// <summary>
        /// Gets image for the appearance stream of the text stamp.
        /// </summary>
        /// <param name="stampText">Text</param>
        /// <param name="date">Date</param>
        /// <param name="shape">Shape</param>
        /// <param name="color">Color</param>
        /// <param name="stampWidth">Width of stamp</param>
        /// <param name="stampHeight">Height of stamp</param>
        /// <param name="imageWidth">Width of image</param>
        /// <param name="imageHeight">Height of image</param>
        /// <param name="rotation">Rotation angle</param>
        /// <returns>Image data</returns>
        public static byte[] GetTempTextStampImage(string stampText, string date, C_TEXTSTAMP_SHAPE shape, C_TEXTSTAMP_COLOR color, out int stampWidth, out int stampHeight, out int imageWidth, out int imageHeight, int rotation = 0)
        {
            CPDFDocument doc = CPDFDocument.CreateDocument();
            string Path = null;
            doc.InsertPage(0, 800, 1200, Path);
            CPDFPage page = doc.PageAtIndex(0, false);
            CPDFStampAnnotation stampAnnot = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;

            stampAnnot.SetTextStamp(stampText, date, shape, color, rotation);
            stampAnnot.UpdateAp();

            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var dpiProperty = typeof(SystemParameters).GetProperty("Dpi", flags);
            int dpi = (int)dpiProperty.GetValue(null, null);

            CRect rect = stampAnnot.GetRect();
            stampWidth = (int)rect.width();
            stampHeight = (int)rect.height();
            imageWidth = (int)(rect.width() * dpi / 72D * 2);
            imageHeight = (int)(rect.height() * dpi / 72D * 2);

            byte[] imageData = new byte[imageWidth * imageHeight * 4];
            stampAnnot.RenderAnnot(imageWidth, imageHeight, imageData);

            stampAnnot.ReleaseAnnot();
            doc.ReleasePages(0);
            doc.Release();

            return imageData;
        }
    }
}
