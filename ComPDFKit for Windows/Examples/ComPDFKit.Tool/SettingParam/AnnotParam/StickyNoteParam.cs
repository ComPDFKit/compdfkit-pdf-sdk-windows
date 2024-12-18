using ComPDFKit.PDFAnnotation;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class StickyNoteParam:AnnotParam
    {
        public  StickyNoteParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_TEXT;
        }

        public byte[] StickyNoteColor { get; set; }
        public string IconName { get; set; } = string.Empty;

        public override bool CopyTo(AnnotParam transfer)
        {
            StickyNoteParam stickynoteTransfer = transfer as StickyNoteParam;
            if (stickynoteTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(stickynoteTransfer))
            {
                return false;
            }

            if(StickyNoteColor != null)
            {
                stickynoteTransfer.StickyNoteColor = (byte[])StickyNoteColor.Clone();
            }

            stickynoteTransfer.IconName = IconName;

            return true;
        }
    }
}
