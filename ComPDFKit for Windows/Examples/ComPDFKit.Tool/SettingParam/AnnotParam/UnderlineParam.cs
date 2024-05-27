using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class UnderlineParam:AnnotParam
	{
		public UnderlineParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE;
        }
		public byte[] UnderlineColor { get; set; }

		public List<CRect> QuardRects { get; set; }

		public override bool CopyTo(AnnotParam transfer)
		{
			UnderlineParam underlineTransfer = transfer as UnderlineParam;
			if (underlineTransfer == null)
			{
				return false;
			}

			if (!base.CopyTo(underlineTransfer))
			{
				return false;
			}

			if(UnderlineColor != null)
			{
                underlineTransfer.UnderlineColor = (byte[])UnderlineColor.Clone();
            }

			if (QuardRects != null)
			{
				List<CRect> rectList = new List<CRect>();
				foreach (CRect saveRect in QuardRects)
				{
					rectList.Add(saveRect);
				}

				underlineTransfer.QuardRects = rectList;
			}

			return true;
		}
	}
}
