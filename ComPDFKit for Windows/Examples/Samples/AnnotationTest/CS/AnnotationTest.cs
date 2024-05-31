using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick;

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
            ink.UpdateAp();
        }

        /// <summary>
        /// Create Shape annotations
        /// Include:
        /// Square, Circle, Line
        /// </summary>
        /// <param name="document"></param>
        static private void CreateShapeAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            float[] dashArray = { 2, 1 };
            byte[] lineColor = { 255, 0, 0 };
            byte[] bgColor = { 0, 255, 0 };

            // Square
            CPDFSquareAnnotation square = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE) as CPDFSquareAnnotation;
            square.SetRect(new CRect(10, 250, 200, 200));
            square.SetLineColor(lineColor);
            square.SetBgColor(bgColor);
            square.SetTransparency(120);
            square.SetLineWidth(1);
            square.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray);
            square.UpdateAp();

            // Circle
            CPDFCircleAnnotation circle = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE) as CPDFCircleAnnotation;
            circle.SetRect(new CRect(10, 410, 110, 300));
            circle.SetLineColor(lineColor);
            circle.SetBgColor(bgColor);
            circle.SetTransparency(120);
            circle.SetLineWidth(1);
            circle.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, dashArray);
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
            CPDFPage page = document.PageAtIndex(1);
            CPDFSoundAnnotation sound = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SOUND) as CPDFSoundAnnotation;
            sound.SetRect(new CRect(400, 750, 450, 700));

            using (var image = new MagickImage("SoundAnnot.png"))
            {
                byte[] byteArray = image.ToByteArray(MagickFormat.Bgra);
                sound.SetSoundPath(byteArray, image.Width, image.Height, "Bird.wav");
            }
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
        

        /// <summary>
        /// Create stamp annotation
        /// </summary>
        /// <param name="document"></param>
        static private void CreateStampAnnotation(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            // Standard
            CPDFStampAnnotation standard = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            standard.SetStandardStamp("Approved");
            standard.SetRect(new CRect(300, 160, 450, 100));
            standard.UpdateAp();

            // Text
            CPDFStampAnnotation text = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            text.SetTextStamp("test", "detail text", C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE, C_TEXTSTAMP_COLOR.TEXTSTAMP_RED);
            text.SetRect(new CRect(300, 300, 450, 220));

            text.UpdateAp();

            // Image
            CPDFStampAnnotation stampAnnotation = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            
            using (var image = new MagickImage("ComPDFKit_Logo.ico"))
            {
                byte[] byteArray = image.ToByteArray(MagickFormat.Bgra);
                stampAnnotation.SetImageStamp(byteArray, image.Width, image.Height);
            }
            
            stampAnnotation.SetRect(new CRect(300, 400, 380, 320));
            stampAnnotation.SetTransparency(255);
            stampAnnotation.UpdateAp();
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

            string path = Path.Combine(outputPath , "DeleteAnnotsTest.pdf"); 
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }
    }
}
