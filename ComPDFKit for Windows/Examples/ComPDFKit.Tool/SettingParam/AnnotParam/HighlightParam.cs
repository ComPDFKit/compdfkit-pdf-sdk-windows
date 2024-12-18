using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public class HighlightParam : AnnotParam
    {

        public HighlightParam()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT;
        }

        public byte[] HighlightColor { get; set; } = new byte[3] { 255, 0, 0 };

        public List<CRect> QuardRects { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            HighlightParam highlightTransfer = transfer as HighlightParam;
            if (highlightTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(highlightTransfer))
            {
                return false;
            }

            if (HighlightColor != null)
            {
                highlightTransfer.HighlightColor = (byte[])HighlightColor.Clone();
            }

            if (QuardRects != null)
            {
                List<CRect> rectList = new List<CRect>();
                foreach (CRect saveRect in QuardRects)
                {
                    rectList.Add(saveRect);
                }

                highlightTransfer.QuardRects = rectList;
            }

            return true;
        }
    }
}
