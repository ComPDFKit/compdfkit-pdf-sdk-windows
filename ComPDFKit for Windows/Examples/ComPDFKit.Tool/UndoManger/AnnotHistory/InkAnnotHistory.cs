using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
    public class InkAnnotHistory:AnnotHistory
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
            InkParam currentParam = CurrentParam as InkParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFInkAnnotation inkAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_INK) as CPDFInkAnnotation;
            if(inkAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
                if(currentParam.InkColor!=null && currentParam.InkColor.Length==3)
                {
                    inkAnnot.SetInkColor(currentParam.InkColor);
                }
               
                inkAnnot.SetThickness((float)currentParam.Thickness);
                inkAnnot.SetInkPath(currentParam.InkPath);
                inkAnnot.SetTransparency(currentParam.Transparency);
                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    inkAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    inkAnnot.SetContent(currentParam.Content);
                }
                inkAnnot.SetIsLocked(currentParam.Locked);
                inkAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                inkAnnot.UpdateAp();
                inkAnnot.ReleaseAnnot();

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
            if (CurrentParam as InkParam == null || PreviousParam as InkParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFInkAnnotation inkAnnot = Annot as CPDFInkAnnotation;
                if (inkAnnot == null || !inkAnnot.IsValid())
                {
                    return false;
                }

                InkParam updateParam = (isUndo ? PreviousParam : CurrentParam) as InkParam;
                InkParam checkParam = (isUndo ? CurrentParam : PreviousParam)as InkParam;

                if (!CheckArrayEqual(updateParam.InkColor,checkParam.InkColor))
                {
                    if(updateParam.InkColor != null && updateParam.InkColor.Length==3)
                    {
                        inkAnnot.SetInkColor(updateParam.InkColor);
                    }
                }

                if (updateParam.Thickness != checkParam.Thickness)
                {
                    inkAnnot.SetThickness((byte)updateParam.Thickness);
                }

                if (updateParam.InkPath!=null)
                {
                    inkAnnot.SetInkPath(updateParam.InkPath);
                }

                if (updateParam.Transparency != checkParam.Transparency)
                {
                    inkAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    inkAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    inkAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    inkAnnot.SetIsLocked(updateParam.Locked);
                }

                inkAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                inkAnnot.UpdateAp();
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
