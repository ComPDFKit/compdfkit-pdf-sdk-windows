using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class SquigglyParam:AnnotParam
    {
        public  SquigglyParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY;
        }
        public byte[] SquigglyColor { get; set; }

        public List<CRect> QuardRects { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            SquigglyParam squigglyTransfer = transfer as SquigglyParam;
            if (squigglyTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(squigglyTransfer))
            {
                return false;
            }

            squigglyTransfer.SquigglyColor = SquigglyColor;

            if(SquigglyColor!=null)
            {
                squigglyTransfer.SquigglyColor= (byte[])SquigglyColor.Clone();
            }

            if (QuardRects != null)
            {
                List<CRect> rectList = new List<CRect>();
                foreach (CRect saveRect in QuardRects)
                {
                    rectList.Add(saveRect);
                }

                squigglyTransfer.QuardRects = rectList;
            }

            return true;
        }

    }
}
