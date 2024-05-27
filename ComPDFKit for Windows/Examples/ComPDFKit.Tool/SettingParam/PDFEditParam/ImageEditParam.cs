using ComPDFKit.Import;
using ComPDFKit.PDFPage;

namespace ComPDFKit.Tool
{
    public class ImageEditParam:PDFEditParam
    {
        public ImageEditParam() 
        {
            EditType = CPDFEditType.EditImage;
        }
        public CRect ClipRect { get; set; }
        public int Rotate { get; set; }

        public override bool CopyTo(PDFEditParam transfer)
        {
            ImageEditParam imageTransfer = transfer as ImageEditParam;
            if (imageTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(imageTransfer))
            {
                return false;
            }

           
            imageTransfer.ClipRect = ClipRect;
            imageTransfer.Rotate = Rotate;

            return true;
        }
    }
}
