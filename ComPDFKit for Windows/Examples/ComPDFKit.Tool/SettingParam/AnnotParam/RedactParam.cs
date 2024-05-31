using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class RedactParam:AnnotParam
	{
		public RedactParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_REDACT;
        }

		public byte[] LineColor { get; set; }
		public byte[] BgColor { get; set; }
		public byte[] FontColor { get; set; }
		public string FontName { get; set; } = string.Empty;
		public string OverlayText { get; set; } = string.Empty;
		public double FontSize { get; set; }
		public C_TEXT_ALIGNMENT Alignment { get; set; }
		public List<CRect> QuardRects { get; set; }

		public override bool CopyTo(AnnotParam transfer)
		{
			RedactParam redactTransfer = transfer as RedactParam;
			if (redactTransfer == null)
			{
				return false;
			}

			if (!base.CopyTo(redactTransfer))
			{
				return false;
			}

			if(LineColor!=null)
			{
				redactTransfer.LineColor= (byte[])LineColor.Clone();
			}
			
			if(BgColor!=null)
			{
				redactTransfer.BgColor= (byte[])BgColor.Clone();
			}
			
			if(FontColor!=null)
			{ 
				redactTransfer.FontColor= (byte[])FontColor.Clone();
			}
			
			redactTransfer.FontName = FontName;
			redactTransfer.FontSize = FontSize;
			redactTransfer.Alignment = Alignment;
			redactTransfer.OverlayText = OverlayText;

			if (QuardRects != null)
			{
				List<CRect> rectList = new List<CRect>();
				foreach (CRect saveRect in QuardRects)
				{
					rectList.Add(saveRect);
				}

				redactTransfer.QuardRects = rectList;
			}

			return true;
		}
	}
}
