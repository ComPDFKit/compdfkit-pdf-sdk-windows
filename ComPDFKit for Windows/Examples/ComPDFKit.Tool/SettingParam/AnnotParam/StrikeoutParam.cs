using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class StrikeoutParam:AnnotParam
    {
		public  StrikeoutParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT;
        }
		public byte[] StrikeoutColor { get; set; }

		public List<CRect> QuardRects { get; set; }

		public override bool CopyTo(AnnotParam transfer)
		{
			StrikeoutParam strikeoutTransfer = transfer as StrikeoutParam;
			if (strikeoutTransfer == null)
			{
				return false;
			}

			if (!base.CopyTo(strikeoutTransfer))
			{
				return false;
			}

			if(StrikeoutColor != null)
			{
                strikeoutTransfer.StrikeoutColor = (byte[])StrikeoutColor.Clone();
            }

			if (QuardRects != null)
			{
				List<CRect> rectList = new List<CRect>();
				foreach (CRect saveRect in QuardRects)
				{
					rectList.Add(saveRect);
				}

				strikeoutTransfer.QuardRects = rectList;
			}

			return true;
		}
	}
}
