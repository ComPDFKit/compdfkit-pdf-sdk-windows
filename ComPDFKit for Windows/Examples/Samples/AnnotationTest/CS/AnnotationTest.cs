using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AnnotationTest
{
    internal class AnnotationTest
    {
        static private string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        static private string outputPath = Path.Combine(parentPath, "Output", "CS");

        static void Main(string[] args)
        {
            #region Preparation work

            Console.WriteLine("Running Annotation test sample…" + Environment.NewLine);
            SDKLicenseHelper.LicenseVerify();
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #endregion

            #region Sample 1: Create annotations

            if (CreateAnnots(document))
            {
                Console.WriteLine("Create annots done.");
            }
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 2: Delete annotations

            CPDFDocument annotsDocument = CPDFDocument.InitWithFilePath("Annotations.pdf");
            if (DeleteAnnotations(annotsDocument))
            {
                Console.WriteLine("Create annots done.");
            }

            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Create freetext annotation
        /// </summary>
        static private void CreateFreetextAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFFreeTextAnnotation freeText = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT) as CPDFFreeTextAnnotation;
            string str = "ComPDFKit Samples";
            freeText.SetContent(str);
            freeText.SetRect(new CRect(0, 100, 160, 0));
            CTextAttribute textAttribute = new CTextAttribute();
            textAttribute.FontName = "Helvetica";
            textAttribute.FontSize = 12;
            byte[] fontColor = { 255, 0, 0 };
            textAttribute.FontColor = fontColor;
            freeText.SetFreetextDa(textAttribute);
            freeText.SetFreetextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_CENTER);
            freeText.UpdateAp();
        }

        /// <summary>
        /// Create freehand annotations
        /// </summary>
        /// <param name="document"></param>
        static private void CreateFreehandAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFInkAnnotation ink = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_INK) as CPDFInkAnnotation;
            ink.SetInkColor(new byte[] { 255, 0, 0 });
            ink.SetBorderWidth(2);
            ink.SetTransparency(128);
            List<List<CPoint>> points = new List<List<CPoint>>();
            ink.SetInkPath(points);
            ink.SetThickness(8);
            points.Clear();
            points.Add(new List<CPoint>()
            {
                new CPoint(10,100),
                new CPoint(100,10),
            });
            ink.SetInkPath(points);
            ink.SetRect(new CRect(10, 200, 200, 20));

            ink.UpdateAp();
        }

        /// <summary>
        /// Create Shape annotations
        /// Include:
        /// Square, Circle, Line
        /// </summary>
        /// <param name="document"></param>7
        static private void CreateShapeAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            float[] dashArray = { 2, 1 };
            byte[] lineColor = { 255, 0, 0 };
            byte[] bgColor = { 0, 255, 0 };

            CPDFBorderEffector borderEffect = new CPDFBorderEffector( C_BORDER_TYPE.C_BORDER_TYPE_Cloud, C_BORDER_INTENSITY.C_INTENSITY_TWO);
            
            // Square
            CPDFSquareAnnotation square = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE) as CPDFSquareAnnotation;
            square.SetSourceRect(new CRect(10, 250, 200, 200)); 
            square.SetLineColor(lineColor);
            square.SetBgColor(bgColor);
            square.SetTransparency(120);
            square.SetLineWidth(1);
            square.SetBorderWidth(1);
            square.SetAnnotBorderEffector(borderEffect); 
            square.AnnotationRotator.SetRotation(45); 
            square.UpdateAp();

            // Circle
            CPDFCircleAnnotation circle = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE) as CPDFCircleAnnotation;
            circle.SetRect(new CRect(10, 410, 110, 300));
            circle.SetLineColor(lineColor);
            circle.SetBgColor(bgColor);
            circle.SetTransparency(120);
            circle.SetLineWidth(1);
            circle.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray);
            circle.SetAnnotBorderEffect(borderEffect);
            square.SetBorderWidth(1);
            circle.UpdateAp();

            // Line
            CPDFLineAnnotation line = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_LINE) as CPDFLineAnnotation;
            line.SetLinePoints(new CPoint(300, 300), new CPoint(350, 350));
            line.SetLineType(C_LINE_TYPE.LINETYPE_NONE, C_LINE_TYPE.LINETYPE_CLOSEDARROW);
            line.SetLineColor(lineColor);
            line.SetTransparency(120);
            line.SetLineWidth(1);
            line.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray);
            line.UpdateAp();
        }

        /// <summary>
        /// Create note annotations
        /// </summary>
        /// <param name="document"></param>
        static private void CreateNoteAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFTextAnnotation textAnnotation = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_TEXT) as CPDFTextAnnotation;
            textAnnotation.SetColor(new byte[] { 255, 0, 0 });
            textAnnotation.SetTransparency(255);
            textAnnotation.SetContent("ComPDFKit");
            textAnnotation.SetRect(new CRect(300, 650, 350, 600));
            textAnnotation.UpdateAp();
        }

        /// <summary>
        /// Create sound annotations
        /// </summary>
        /// <param name="document"></param>
        static private void CreateSoundAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFSoundAnnotation sound = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SOUND) as CPDFSoundAnnotation;
            sound.SetRect(new CRect(400, 750, 450, 700));
            sound.SetSoundPath("", "Bird.wav");
            sound.UpdateAp();
        }

        /// <summary>
        /// Create Markup annotations
        /// </summary>
        /// <param name="document"></param>
        static private void CreateMarkupAnnotation(CPDFDocument document)
        {
            List<CRect> cRectList = new List<CRect>();
            CRect rect = new CRect(300, 300, 400, 240);
            cRectList.Add(rect);
            byte[] color = { 255, 0, 0 };

            //highlight
            CPDFPage page1 = document.PageAtIndex(0);

            CPDFHighlightAnnotation highlight = page1.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT) as CPDFHighlightAnnotation;
            highlight.SetColor(color);
            highlight.SetTransparency(120);
            highlight.SetQuardRects(cRectList);
            highlight.UpdateAp();

            //underline
            CPDFPage page2 = document.PageAtIndex(1);

            CPDFUnderlineAnnotation underline = page2.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE) as CPDFUnderlineAnnotation;
            underline.SetColor(color);
            underline.SetTransparency(120);
            underline.SetQuardRects(cRectList);
            underline.UpdateAp();

            //strikeout
            CPDFPage page3 = document.PageAtIndex(2);

            CPDFStrikeoutAnnotation strikeout = page3.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT) as CPDFStrikeoutAnnotation;
            strikeout.SetColor(color);
            strikeout.SetTransparency(120);
            strikeout.SetQuardRects(cRectList);
            strikeout.UpdateAp();

            //squiggly
            CPDFPage page4 = document.PageAtIndex(3);

            CPDFSquigglyAnnotation squiggy = page4.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY) as CPDFSquigglyAnnotation;
            squiggy.SetColor(color);
            squiggy.SetTransparency(120);
            squiggy.SetQuardRects(cRectList);
            squiggy.UpdateAp();

        }

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
        /// Create stamp annotation
        /// </summary>
        /// <param name="document"></param>
        static private void CreateStampAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFStampAnnotation standard = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            standard.SetStandardStamp("Approved");
             
            standard.SetSourceRect(new CRect(100, 150, 250, 100));
            standard.AnnotationRotator.SetRotation(45);

            standard.UpdateAp();

            // Text
            CPDFStampAnnotation text = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            text.SetTextStamp("test", "detail text", C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE, C_TEXTSTAMP_COLOR.TEXTSTAMP_RED);
            text.SetRect(new CRect(300, 300, 450, 220));
            text.UpdateAp();

            // Image
            Bitmap bitmap = new Bitmap("logo.png");
            CPDFStampAnnotation image = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            image.SetImageStamp(BitmapToByteArray(bitmap), bitmap.Width, bitmap.Height);
            image.SetRect(new CRect(300, 400, 380, 320));
            image.SetTransparency(255);
            standard.AnnotationRotator.SetRotation(45);
            image.UpdateAp();
        }


        private static void CreateLinkAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFDestination dest = new CPDFDestination();
            dest.PageIndex = 1;
            CPDFLinkAnnotation link = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_LINK) as CPDFLinkAnnotation;
            link.SetRect(new CRect(0, 50, 50, 0));
            link.SetDestination(document, dest);
        }

        /// <summary>
        /// Create annotations
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        static private bool CreateAnnots(CPDFDocument document)
        {
            CreateFreetextAnnotation(document);
            CreateFreehandAnnotation(document);
            CreateShapeAnnotation(document);
            CreateNoteAnnotation(document);
            CreateShapeAnnotation(document);
            CreateSoundAnnotation(document);
            CreateMarkupAnnotation(document);
            CreateStampAnnotation(document);
            CreateLinkAnnotation(document);
            string path = Path.Combine(outputPath, "CreateAnnotsTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }

            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }


        /// <summary>
        /// Delete the first annotation
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        static private bool DeleteAnnotations(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);

            List<CPDFAnnotation> annotList = page.GetAnnotations();
            if (!annotList[0].RemoveAnnot())
            {
                return false;
            }

            string path = Path.Combine(outputPath, "DeleteAnnotsTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }
    }
}
