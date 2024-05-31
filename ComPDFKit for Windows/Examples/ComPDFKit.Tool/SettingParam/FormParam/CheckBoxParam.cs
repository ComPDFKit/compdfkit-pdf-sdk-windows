using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;

namespace ComPDFKit.Tool
{
	public class CheckBoxParam:WidgetParm
	{
		public  CheckBoxParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
			WidgetType = C_WIDGET_TYPE.WIDGET_CHECKBOX;
        }

		public C_CHECK_STYLE CheckStyle { get; set; }
		public bool IsChecked { get; set; }
		public override bool CopyTo(AnnotParam transfer)
		{
			CheckBoxParam checkboxTransfer = transfer as CheckBoxParam;
			if (checkboxTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(checkboxTransfer))
			{
				return false;
			}

			checkboxTransfer.CheckStyle = CheckStyle;
			checkboxTransfer.IsChecked = IsChecked;

			return true;
		}
	}
}
