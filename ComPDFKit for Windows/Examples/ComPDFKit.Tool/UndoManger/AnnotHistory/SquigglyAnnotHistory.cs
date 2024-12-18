using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class SquigglyAnnotHistory:AnnotHistory
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
            SquigglyParam currentParam = CurrentParam as SquigglyParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}

			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFSquigglyAnnotation squigglyAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY) as CPDFSquigglyAnnotation;
			if (squigglyAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				squigglyAnnot.SetTransparency((byte)currentParam.Transparency);
				squigglyAnnot.SetRect(currentParam.ClientRect);
				
				if (currentParam.QuardRects != null)
				{
                    squigglyAnnot.SetQuardRects(currentParam.QuardRects);
                }

                if (currentParam.SquigglyColor != null && currentParam.SquigglyColor.Length == 3)
                {
                    squigglyAnnot.SetColor(currentParam.SquigglyColor);
                }


                if (!string.IsNullOrEmpty(currentParam.Author))
				{
					squigglyAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					squigglyAnnot.SetContent(currentParam.Content);
				}
				squigglyAnnot.SetIsLocked(currentParam.Locked);
				squigglyAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				squigglyAnnot.UpdateAp();
				squigglyAnnot.ReleaseAnnot();

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
			if (CurrentParam as SquigglyParam == null || PreviousParam as SquigglyParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFSquigglyAnnotation squigglyAnnot = Annot as CPDFSquigglyAnnotation;
				if (squigglyAnnot == null || !squigglyAnnot.IsValid())
				{
					return false;
				}

				SquigglyParam updateParam = (isUndo ? PreviousParam : CurrentParam) as SquigglyParam;
				SquigglyParam checkParam = (isUndo ? CurrentParam : PreviousParam) as SquigglyParam;

                if (!CheckArrayEqual(updateParam.SquigglyColor, checkParam.SquigglyColor))
                {
                    if (updateParam.SquigglyColor != null && updateParam.SquigglyColor.Length == 3)
                    {
                        squigglyAnnot.SetColor(updateParam.SquigglyColor);
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
				{
					squigglyAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if (updateParam.Author != checkParam.Author)
				{
					squigglyAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					squigglyAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					squigglyAnnot.SetIsLocked(updateParam.Locked);
				}

				squigglyAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				squigglyAnnot.UpdateAp();

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
