using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class StickyNoteAnnotHistory:AnnotHistory
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
            StickyNoteParam currentParam = CurrentParam as StickyNoteParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFTextAnnotation stickynoteAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_TEXT) as CPDFTextAnnotation;
            if(stickynoteAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
              
                if(currentParam.StickyNoteColor!=null && currentParam.StickyNoteColor.Length==3)
                {
                    stickynoteAnnot.SetColor(currentParam.StickyNoteColor);
                }
               
                stickynoteAnnot.SetTransparency((byte)currentParam.Transparency);
                stickynoteAnnot.SetRect(currentParam.ClientRect);

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    stickynoteAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    stickynoteAnnot.SetContent(currentParam.Content);
                }
                stickynoteAnnot.SetIsLocked(currentParam.Locked);
                stickynoteAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                stickynoteAnnot.UpdateAp();
                stickynoteAnnot.ReleaseAnnot();

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
            if (CurrentParam as StickyNoteParam == null || PreviousParam as StickyNoteParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFTextAnnotation stickynoteAnnot = Annot as CPDFTextAnnotation;
                if (stickynoteAnnot == null || !stickynoteAnnot.IsValid())
                {
                    return false;
                }

                StickyNoteParam updateParam = (isUndo ? PreviousParam : CurrentParam) as StickyNoteParam;
                StickyNoteParam checkParam = (isUndo ? CurrentParam : PreviousParam) as StickyNoteParam;

                if (!CheckArrayEqual(updateParam.StickyNoteColor, checkParam.StickyNoteColor))
                {
                    if (updateParam.StickyNoteColor != null && updateParam.StickyNoteColor.Length == 3)
                    {
                        stickynoteAnnot.SetColor(updateParam.StickyNoteColor);
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
                {
                    stickynoteAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    stickynoteAnnot.SetRect(updateParam.ClientRect);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    stickynoteAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    stickynoteAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    stickynoteAnnot.SetIsLocked(updateParam.Locked);
                }

                stickynoteAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                stickynoteAnnot.UpdateAp();

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
