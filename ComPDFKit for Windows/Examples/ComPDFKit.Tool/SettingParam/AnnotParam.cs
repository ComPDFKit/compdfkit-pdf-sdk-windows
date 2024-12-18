using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Windows;

namespace ComPDFKit.Tool
{
    public class AnnotParam
    {
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CreateTime { get; set; } = string.Empty;
        public string UpdateTime { get; set; } = string.Empty;
        public C_ANNOTATION_TYPE CurrentType { get; set; }
        public bool Locked { get; set; }

        /// <summary>
        /// The rectangle of the annotation within the PDF (PDF DPI)
        /// </summary>
        public CRect ClientRect { get; set; }
        public int PageIndex { get; set; }
        public int AnnotIndex { get; set; }
        public byte Transparency
        {
            get;
            set;
        } = 255;
        public virtual bool CopyTo(AnnotParam transfer)
        {
            if (transfer == null)
            {
                return false;
            }

            transfer.Content = Content;
            transfer.Author = Author;
            transfer.CreateTime = CreateTime;
            transfer.UpdateTime = UpdateTime;
            transfer.CurrentType = CurrentType;
            transfer.Locked = Locked;
            transfer.ClientRect = ClientRect;
            transfer.CurrentType = CurrentType;
            transfer.PageIndex = PageIndex;
            transfer.Transparency = Transparency;
            transfer.AnnotIndex = AnnotIndex;

            return true;
        }
    }
}
