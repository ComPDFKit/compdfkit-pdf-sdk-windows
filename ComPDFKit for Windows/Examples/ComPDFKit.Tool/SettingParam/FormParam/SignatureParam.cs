using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;

namespace ComPDFKit.Tool
{
	public class SignatureParam:WidgetParm
	{
		public  SignatureParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS;
        }

		public override bool CopyTo(AnnotParam transfer)
		{
			SignatureParam signTransfer = transfer as SignatureParam;
			if (signTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(signTransfer))
			{
				return false;
			}

			return true;
		}
	}
}
