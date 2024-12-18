using ComPDFKit.Import;
using ComPDFKit.PDFPage;

namespace ComPDFKit.Tool
{
    public class PDFEditParam
    {
        public int PageIndex {  get; set; }
        public int EditIndex {  get; set; }
        public CPDFEditType EditType { get; set; }
        public byte Transparency { get; set; }
        public CRect ClientRect { get; set; }
        public virtual bool CopyTo(PDFEditParam transfer)
        {
            if (transfer == null)
                return false;

            transfer.PageIndex = PageIndex;
            transfer.EditIndex = EditIndex;
            transfer.EditType = EditType;
            transfer.Transparency = Transparency;
            transfer.ClientRect = ClientRect;
            return true;
        }
    }
}
