 using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveFormsTest
{
    internal class InteractiveFormsTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
          
        static void Main(string[] args)
        {
            Console.WriteLine("Running interactive forms test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            if (CreateForms(document))
            {
                Console.WriteLine("Create forms done.");
            }
            else
            {
                Console.WriteLine("Create forms failed."); 
            }
            Console.WriteLine("--------------------");
            Console.WriteLine("Done");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        /// Create text field.
        /// Text field: used to input text
        /// </summary> 
        static private void CreateTextField(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFTextWidget textField = page.CreateWidget(C_WIDGET_TYPE.WIDGET_TEXTFIELD) as CPDFTextWidget;
            textField.SetRect(new CRect( 28, 75, 235, 32));
            textField.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            textField.SetWidgetBgRGBColor(new byte[] {240,255,240});
        }

        /// <summary>
        ///  Create push button.
        ///  Push button:  Click to perform some actions, such as jumping to a page or website.
        /// </summary> 
        static private void CreatePushButton(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFPushButtonWidget pushButton1 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_PUSHBUTTON) as CPDFPushButtonWidget;
            pushButton1.SetRect(new CRect(28, 150, 150, 100));
            pushButton1.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            pushButton1.SetWidgetBgRGBColor(new byte[] { 180, 180, 220 });
            pushButton1.SetButtonTitle("Go To Page 2");
            CTextAttribute attribute = new CTextAttribute();
            attribute.FontColor = new byte[] { 0, 0, 0 };
            attribute.FontSize = 14;
            attribute.FontName = "Helvetica";
            pushButton1.SetTextAttribute(attribute);
            CPDFGoToAction gotoAction = new CPDFGoToAction();
            CPDFDestination dest = new CPDFDestination();
            dest.PageIndex = 1;
            gotoAction.SetDestination(document, dest);
            pushButton1.SetButtonAction(gotoAction);
            pushButton1.UpdateFormAp();

            CPDFPushButtonWidget pushButton2 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_PUSHBUTTON) as CPDFPushButtonWidget;
            pushButton2.SetRect(new CRect(168, 150, 290, 100));
            pushButton2.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            pushButton2.SetWidgetBgRGBColor(new byte[] { 180, 180, 220 });
            pushButton2.SetButtonTitle("Go To ComPDFKit");
            CTextAttribute attribute2 = new CTextAttribute();
            attribute2.FontColor = new byte[] { 0, 0, 0 };
            attribute2.FontSize = 14;
            attribute2.FontName = "Helvetica";
            pushButton2.SetTextAttribute(attribute);

            CPDFUriAction uriAction = new CPDFUriAction();
            uriAction.SetUri("https://www.compdf.com/");

            pushButton2.SetButtonAction(uriAction);
            pushButton2.UpdateFormAp();
        }

        /// <summary>
        /// Create ListBox
        /// </summary> 
        static private void CreateListBox(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFListBoxWidget listbox = page.CreateWidget(C_WIDGET_TYPE.WIDGET_LISTBOX) as CPDFListBoxWidget; 
            listbox.SetRect(new CRect(28, 330, 150, 230));
            listbox.AddOptionItem(0, "1", "ComPDFKit1");
            listbox.AddOptionItem(1, "2", "ComPDFKit2");
            listbox.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            listbox.SetWidgetBgRGBColor(new byte[] { 200, 180, 180 });
        }

        /// <summary>
        /// Create SignatureField
        /// Provide an area for electronic signatures.
        /// </summary> 
        static private void CreateSignatureField(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFSignatureWidget signatureField = page.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS) as CPDFSignatureWidget;
            signatureField.SetRect(new CRect(28, 420, 150, 370));

            signatureField.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            signatureField.SetWidgetBgRGBColor(new byte[] { 150, 180, 210 });
        }

        /// <summary>
        /// Create CheckBox
        /// </summary>
        static private void CreateCheckBox(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFCheckBoxWidget checkBox1 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX) as CPDFCheckBoxWidget;
            checkBox1.SetRect(new CRect(28, 470, 48, 450));
            checkBox1.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            checkBox1.SetWidgetBgRGBColor(new byte[] { 150, 180, 210 });

            CPDFCheckBoxWidget checkBox2 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX) as CPDFCheckBoxWidget;
            checkBox2.SetRect(new CRect(58, 470, 78, 450));
            checkBox2.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            checkBox2.SetWidgetBgRGBColor(new byte[] { 150, 180, 210 });

            CPDFCheckBoxWidget checkBox3 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX) as CPDFCheckBoxWidget;
            checkBox3.SetRect(new CRect(88, 470, 108, 450));
            checkBox3.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            checkBox3.SetWidgetBgRGBColor(new byte[] { 150, 180, 210 });
        }

        /// <summary>
        /// Create RadioButton
        /// </summary>
        static private void CreateRadioButton(CPDFDocument document)
        {
            CPDFPage page = document.PageAtIndex(0);
            CPDFRadioButtonWidget radioButton1 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON) as CPDFRadioButtonWidget;
            radioButton1.SetRect(new CRect(28, 500, 48, 480));
            radioButton1.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            radioButton1.SetWidgetBgRGBColor(new byte[] { 210, 180, 150 });
            radioButton1.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE);

            CPDFRadioButtonWidget radioButton2 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON) as CPDFRadioButtonWidget;
            radioButton2.SetRect(new CRect(58, 500, 78, 480));
            radioButton2.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            radioButton2.SetWidgetBgRGBColor(new byte[] { 210, 180, 150 });
            radioButton2.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE);

            CPDFRadioButtonWidget radioButton3 = page.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON) as CPDFRadioButtonWidget;
            radioButton3.SetRect(new CRect(88, 500, 108, 480));
            radioButton3.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            radioButton3.SetWidgetBgRGBColor(new byte[] { 210, 180, 150 });
            radioButton3.SetWidgetCheckStyle(C_CHECK_STYLE.CK_CIRCLE);
        }

        /// <summary>
        /// Generate various signatures.
        /// </summary>
        static private bool CreateForms(CPDFDocument document)
        {
            CreateTextField(document);
            CreatePushButton(document);
            CreateListBox(document);
            CreateSignatureField(document);
            CreateCheckBox(document);
            CreateRadioButton(document);

            // Save to pointed path so you can observe the effect.
            string path = Path.Combine(outputPath, "CreateFormsTest.pdf");

            if (!document.WriteToFilePath(path)) 
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);

            return true;
        }

    }
}
