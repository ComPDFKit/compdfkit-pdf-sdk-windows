using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System; 
using System.IO; 
using System.Windows;

namespace PDFRedactTest
{
    internal class PDFRedactTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running redact test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            
            string str = document.PageAtIndex(0).GetTextPage().GetSelectText(new CPoint(300, 240), new CPoint(400, 300), new CPoint(0, 0));
            Console.WriteLine("The text need to be redact is: {0}", str);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Redact
            if (Redact(document))
            {
                Console.WriteLine("Redact done.");
            }
            else
            {
                Console.WriteLine("Redact failed.");
            }

            Console.WriteLine("--------------------");
            #endregion

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Redact an area in PDF
        /// </summary>
        /// <param name="document">Regular document with some text</param>
        /// <returns></returns>
        static private bool Redact(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFRedactAnnotation redact = page.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_REDACT) as CPDFRedactAnnotation;
            //Set radact rect: cover the title
            redact.SetRect(new CRect(300, 300, 400, 240));
            //Set overlay text: REDACTED
            redact.SetOverlayText("REDACTED");

            //Properties of cover text
            CTextAttribute textAttribute = new CTextAttribute();
            textAttribute.FontName = "Helvetica";
            textAttribute.FontSize = 12;
            byte[] fontColor = { 255, 0, 0 };
            textAttribute.FontColor = fontColor;
            redact.SetTextDa(textAttribute);
            redact.SetTextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_LEFT);

            //Properties of cover square
            byte[] fillColor = { 255, 0, 0 };
            redact.SetFillColor(fillColor);
            byte[] outlineColor = { 0, 255, 0 };
            redact.SetOutlineColor(outlineColor);

            redact.UpdateAp();
            document.ApplyRedaction();
            // Save to pointed path so you can observe the effect.
            string path = Path.Combine(outputPath, "RedactTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            CPDFDocument newDocument = CPDFDocument.InitWithFilePath(path);

            //Validation: try to get the text of the covered area
            string str = newDocument.PageAtIndex(0).GetTextPage().GetSelectText(new CPoint(60, 200), new CPoint(560, 250), new CPoint(0, 0));
            Console.WriteLine("Text in the redacted area is: {0}", str); 
            return true;
        }
    }
}
