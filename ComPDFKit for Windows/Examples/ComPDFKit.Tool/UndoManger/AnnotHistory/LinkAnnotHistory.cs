using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class LinkAnnotHistory:AnnotHistory
    {
        public override int GetAnnotIndex()
        {
            if (CurrentParam != null)
            {
                return CurrentParam.AnnotIndex;
            }
            return base.GetAnnotIndex();
        }

        public override int GetPageIndex()
        {
            if (CurrentParam != null)
            {
                return CurrentParam.PageIndex;
            }
            return base.GetPageIndex();
        }

        public override void SetAnnotIndex(int newIndex)
        {
            if (CurrentParam != null)
            {
                CurrentParam.AnnotIndex = newIndex;
            }

            if (PreviousParam != null)
            {
                PreviousParam.AnnotIndex = newIndex;
            }
        }

        internal override bool Add()
        {
            LinkParam currentParam = CurrentParam as LinkParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFLinkAnnotation linkAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_LINK) as CPDFLinkAnnotation;
            if (linkAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;

                switch(currentParam.Action)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        {
                            CPDFGoToAction gotoAction = new CPDFGoToAction();
                            CPDFDestination destination = new CPDFDestination();
                            destination.Position_X = currentParam.DestinationPosition.x;
                            destination.Position_Y = currentParam.DestinationPosition.y;
                            destination.PageIndex = currentParam.DestinationPageIndex;
                            gotoAction.SetDestination(PDFDoc,destination);
                            linkAnnot.SetLinkAction(gotoAction);
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction uriAction = new CPDFUriAction();
                            if(!string.IsNullOrEmpty(currentParam.Uri))
                            {
                                uriAction.SetUri(currentParam.Uri);
                            }
                            linkAnnot.SetLinkAction(uriAction);
                        }
                        break;
                    default:
                        break;
                }

                linkAnnot.SetRect(currentParam.ClientRect);

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    linkAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    linkAnnot.SetContent(currentParam.Content);
                }

                linkAnnot.SetIsLocked(currentParam.Locked);
                linkAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                linkAnnot.UpdateAp();
                linkAnnot.ReleaseAnnot();

                if (currentParam != null)
                {
                    currentParam.AnnotIndex = annotIndex;
                }

                if (PreviousParam != null)
                {
                    PreviousParam.AnnotIndex = annotIndex;
                }

                return true;
            }

            return false;
        }

        internal override bool Update(bool isUndo)
        {
            if (CurrentParam as LinkParam == null || PreviousParam as LinkParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFLinkAnnotation linkAnnot = Annot as CPDFLinkAnnotation;
                if (linkAnnot == null || !linkAnnot.IsValid())
                {
                    return false;
                }

                LinkParam updateParam = (isUndo ? PreviousParam : CurrentParam) as LinkParam;
                LinkParam checkParam = (isUndo ? CurrentParam : PreviousParam) as LinkParam;

                switch (updateParam.Action)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        {
                            CPDFGoToAction gotoAction = new CPDFGoToAction();
                            CPDFDestination destination = new CPDFDestination();
                            destination.Position_X = updateParam.DestinationPosition.x;
                            destination.Position_Y = updateParam.DestinationPosition.y;
                            destination.PageIndex = updateParam.DestinationPageIndex;
                            gotoAction.SetDestination(PDFDoc, destination);
                            linkAnnot.SetLinkAction(gotoAction);
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction uriAction = new CPDFUriAction();
                            if(!string.IsNullOrEmpty(updateParam.Uri))
                            {
                                uriAction.SetUri(updateParam.Uri);
                            }
                            linkAnnot.SetLinkAction(uriAction);
                        }
                        break;
                    default:
                        break;
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    linkAnnot.SetRect(updateParam.ClientRect);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    linkAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    linkAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    linkAnnot.SetIsLocked(updateParam.Locked);
                }

                linkAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                linkAnnot.UpdateAp();

                return true;
            }

            return false;
        }

        internal override bool Remove()
        {
            if (MakeAnnotValid(CurrentParam))
            {
                Annot.RemoveAnnot();
                return true;
            }
            return false;
        }
    }
}
