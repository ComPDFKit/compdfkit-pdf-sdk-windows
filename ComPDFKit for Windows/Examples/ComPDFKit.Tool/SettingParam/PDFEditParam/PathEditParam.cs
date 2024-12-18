using ComPDFKit.Import;
using ComPDFKit.PDFPage;

namespace ComPDFKit.Tool
{
    public class PathEditParam:PDFEditParam
    {
        public PathEditParam() 
        {
            EditType = CPDFEditType.EditPath;
        }

        public byte[] StrokeColor { get; set; }
        public byte[] FillColor { get; set; }
        public CRect ClipRect { get; set; }
        public int Rotate { get; set; }

        public override bool CopyTo(PDFEditParam transfer)
        {
            PathEditParam pathTransfer = transfer as PathEditParam;
            if (pathTransfer == null || !base.CopyTo(pathTransfer))
                return false;

            pathTransfer.StrokeColor = StrokeColor;
            pathTransfer.FillColor = FillColor;
            pathTransfer.ClipRect = ClipRect;
            pathTransfer.Rotate = Rotate;
            return true;
        }
    }
}
