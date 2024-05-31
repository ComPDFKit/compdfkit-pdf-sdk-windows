using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;

namespace ComPDFKit.Tool
{
	public class RadioButtonParam:WidgetParm
	{
		public  RadioButtonParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_RADIOBUTTON;
        }
		public C_CHECK_STYLE CheckStyle {  get; set; }
		public bool IsChecked {  get; set; }
		public override bool CopyTo(AnnotParam transfer)
		{
			RadioButtonParam radioTransfer = transfer as RadioButtonParam;
			if (radioTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(radioTransfer))
			{
				return false;
			}

			radioTransfer.CheckStyle = CheckStyle;
			radioTransfer.IsChecked = IsChecked;

			return true;
		}
	}
}
