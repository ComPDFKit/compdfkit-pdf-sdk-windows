using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument.Action;
using System.Windows;

namespace ComPDFKit.Tool
{
	public class PushButtonParam:WidgetParm
	{
		public PushButtonParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_PUSHBUTTON;
        }

		public bool IsBold {  get; set; }
		public bool IsItalic { get; set; }
		public string Text { get; set; } = string.Empty;
		public string Uri { get; set; } = string.Empty;
		public int DestinationPageIndex { get; set; }
		public CPoint DestinationPosition { get; set; }
		public C_ACTION_TYPE Action { get; set; }

		public override bool CopyTo(AnnotParam transfer)
		{
			PushButtonParam pushbuttonTransfer = transfer as PushButtonParam;
			if (pushbuttonTransfer == null)
			{
				return false;
			}

			if (!base.CopyTo(pushbuttonTransfer))
			{
				return false;
			}

			pushbuttonTransfer.IsBold = IsBold;
			pushbuttonTransfer.IsItalic = IsItalic;
			pushbuttonTransfer.Text = Text;
			pushbuttonTransfer.Uri = Uri;
			pushbuttonTransfer.DestinationPageIndex = DestinationPageIndex;
			pushbuttonTransfer.DestinationPosition = DestinationPosition;
			pushbuttonTransfer.Action = Action;

			return true;
		}
	}
}
