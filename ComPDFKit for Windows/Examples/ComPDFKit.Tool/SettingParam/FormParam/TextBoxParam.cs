using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using System.Windows;

namespace ComPDFKit.Tool
{
	public class TextBoxParam:WidgetParm
	{
		public  TextBoxParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_TEXTFIELD;
        }

		public string Text { get; set; } = string.Empty;
		public bool IsMultiLine {  get; set; }
		public bool IsPassword { get; set; }
		public bool IsItalic {  get; set; }
		public bool IsBold { get; set; }
		public C_TEXT_ALIGNMENT Alignment { get; set; }

		public override bool CopyTo(AnnotParam transfer)
		{
			TextBoxParam textTransfer = transfer as TextBoxParam;
			if (textTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(textTransfer))
			{
				return false;
			}

			textTransfer.Text = Text;
			textTransfer.IsMultiLine = IsMultiLine;
			textTransfer.IsPassword = IsPassword;
			textTransfer.IsItalic = IsItalic;
			textTransfer.IsBold = IsBold;
			textTransfer.Alignment = Alignment;

			return true;
		}
	}
}
