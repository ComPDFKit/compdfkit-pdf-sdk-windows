using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument.Action;
using System.Windows;

namespace ComPDFKit.Tool
{
	public class LinkParam:AnnotParam
    {
        public  LinkParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINK;
        }

        public string Uri { get; set; } = string.Empty;
        public int DestinationPageIndex {  get; set; }
        public CPoint DestinationPosition { get; set; }
        public C_ACTION_TYPE Action { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            LinkParam linkTransfer = transfer as LinkParam;
            if (linkTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(linkTransfer))
            {
                return false;
            }

            linkTransfer.Uri = Uri;
            linkTransfer.DestinationPageIndex = DestinationPageIndex;
            linkTransfer.DestinationPosition = DestinationPosition;
            linkTransfer.Action = Action;

            return true;
        }
    }
}
