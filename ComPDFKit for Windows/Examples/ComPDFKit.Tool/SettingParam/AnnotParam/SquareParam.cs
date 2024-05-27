using ComPDFKit.PDFAnnotation;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class SquareParam:AnnotParam
    {
        public SquareParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE;
        }

        public byte[] LineColor { get; set; }
        public byte[] BgColor { get; set; }
        public bool HasBgColor {  get; set; }
        public double LineWidth { get; set; }
        public float[] LineDash { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public override bool CopyTo(AnnotParam transfer)
        {
            SquareParam squareTransfer = transfer as SquareParam;
            if (squareTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(squareTransfer))
            {
                return false;
            }

            if(LineColor!=null)
            {
                squareTransfer.LineColor = (byte[])LineColor.Clone();
            }

            if(BgColor!=null)
            {
                squareTransfer.BgColor = (byte[])BgColor.Clone();
            }
            
            if(LineDash!=null)
            {
                squareTransfer.LineDash = (float[])LineDash.Clone();
            }
            squareTransfer.LineWidth = LineWidth;
            squareTransfer.HasBgColor = HasBgColor;
            squareTransfer.BorderStyle = BorderStyle;

            return true;
        }
    }
}
