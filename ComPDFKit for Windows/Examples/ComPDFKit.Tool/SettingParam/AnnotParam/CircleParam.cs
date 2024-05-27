using ComPDFKit.PDFAnnotation;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public class CircleParam : AnnotParam
    {
        public CircleParam()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE;
        }

        public byte[] LineColor { get; set; }
        public byte[] BgColor { get; set; }
        public bool HasBgColor { get; set; }
        public double LineWidth { get; set; }
        public float[] LineDash { get; set; }

        public C_BORDER_STYLE BorderStyle { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            CircleParam circleTransfer = transfer as CircleParam;
            if (circleTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(circleTransfer))
            {
                return false;
            }

            if (LineColor != null)
            {
                circleTransfer.LineColor = (byte[])LineColor.Clone();
            }
            if (BgColor != null)
            {
                circleTransfer.BgColor = (byte[])BgColor.Clone();
            }

            if (LineDash != null)
            {
                circleTransfer.LineDash = (float[])LineDash.Clone();
            }
            circleTransfer.LineWidth = LineWidth;
            circleTransfer.HasBgColor = HasBgColor;
            circleTransfer.BorderStyle = BorderStyle;
            return true;
        }
    }
}
