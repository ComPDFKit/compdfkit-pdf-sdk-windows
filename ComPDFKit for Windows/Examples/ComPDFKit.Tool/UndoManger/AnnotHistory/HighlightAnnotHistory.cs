using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
    public class HighlightAnnotHistory:AnnotHistory
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
            HighlightParam currentParam = CurrentParam as HighlightParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFHighlightAnnotation highlightAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT) as CPDFHighlightAnnotation;
			if (highlightAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;
				
				highlightAnnot.SetTransparency((byte)currentParam.Transparency);
				
				if (currentParam.QuardRects != null)
				{
                    highlightAnnot.SetQuardRects(currentParam.QuardRects);
                }

                if (currentParam.HighlightColor != null && currentParam.HighlightColor.Length==3)
				{
                    highlightAnnot.SetColor(currentParam.HighlightColor);
                }

				if (!string.IsNullOrEmpty(currentParam.Author))
				{
					highlightAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					highlightAnnot.SetContent(currentParam.Content);
				}
                highlightAnnot.SetRect(currentParam.ClientRect);
                highlightAnnot.SetIsLocked(currentParam.Locked);
				highlightAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				highlightAnnot.UpdateAp();
				highlightAnnot.ReleaseAnnot();

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
			if (CurrentParam as HighlightParam == null || PreviousParam as HighlightParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFHighlightAnnotation highlightAnnot = Annot as CPDFHighlightAnnotation;
				if (highlightAnnot == null || !highlightAnnot.IsValid())
				{
					return false;
				}

				HighlightParam updateParam = (isUndo ? PreviousParam : CurrentParam) as HighlightParam;
				HighlightParam checkParam = (isUndo ? CurrentParam : PreviousParam) as HighlightParam;

                if (!CheckArrayEqual(updateParam.HighlightColor, checkParam.HighlightColor))
                {
					if(updateParam.HighlightColor != null && updateParam.HighlightColor.Length==3)
					{
                        highlightAnnot.SetColor(updateParam.HighlightColor);
                    }
				}

				if (updateParam.Transparency != checkParam.Transparency)
				{
					highlightAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if (updateParam.Author != checkParam.Author)
				{
					highlightAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					highlightAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					highlightAnnot.SetIsLocked(updateParam.Locked);
				}

				highlightAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				highlightAnnot.UpdateAp();

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
