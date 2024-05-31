using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class LineParam:AnnotParam
    {
        public LineParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINE;
        }

        public byte[] LineColor {  get; set; }
        public byte[] BgColor { get; set; }
        public bool HasBgColor {  get; set; }
        public double LineWidth { get; set; }
        public float[] LineDash { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public C_LINE_TYPE HeadLineType {  get; set; }
        public C_LINE_TYPE TailLineType {  get; set; }
        public CPoint HeadPoint { get; set; }
        public CPoint TailPoint { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            LineParam lineTransfer = transfer as LineParam;
            if (lineTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(lineTransfer))
            {
                return false;
            }

            if (LineColor!=null)
            {
                lineTransfer.LineColor = (byte[])LineColor.Clone();
            }
          
            if(BgColor!=null)
            {
                lineTransfer.BgColor = (byte[])BgColor.Clone();
            }
            
            if(LineDash!=null)
            {
                lineTransfer.LineDash = (float[])LineDash.Clone();
            }

            lineTransfer.HasBgColor = HasBgColor;
            lineTransfer.LineWidth = LineWidth;
            lineTransfer.BorderStyle = BorderStyle;
            lineTransfer.HeadLineType = HeadLineType;
            lineTransfer.TailLineType = TailLineType;
            lineTransfer.HeadPoint = HeadPoint;
            lineTransfer.TailPoint = TailPoint;

            return true;
        }
    }
}
