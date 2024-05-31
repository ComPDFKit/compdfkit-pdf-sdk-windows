using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class UnderlineAnnotHistory:AnnotHistory
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
            UnderlineParam currentParam = CurrentParam as UnderlineParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFUnderlineAnnotation underlineAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE) as CPDFUnderlineAnnotation;
			if (underlineAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				underlineAnnot.SetTransparency((byte)currentParam.Transparency);
				underlineAnnot.SetRect(currentParam.ClientRect);
				
				if (currentParam.QuardRects != null)
				{
                    underlineAnnot.SetQuardRects(currentParam.QuardRects);
                }

                if (currentParam.UnderlineColor != null && currentParam.UnderlineColor.Length == 3)
                {
                    underlineAnnot.SetColor(currentParam.UnderlineColor);
                }

                if (!string.IsNullOrEmpty(currentParam.Author))
				{
					underlineAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					underlineAnnot.SetContent(currentParam.Content);
				}
				underlineAnnot.SetIsLocked(currentParam.Locked);
				underlineAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				underlineAnnot.UpdateAp();
				underlineAnnot.ReleaseAnnot();

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
			if (CurrentParam as UnderlineParam == null || PreviousParam as UnderlineParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFUnderlineAnnotation underlineAnnot = Annot as CPDFUnderlineAnnotation;
				if (underlineAnnot == null || !underlineAnnot.IsValid())
				{
					return false;
				}

				UnderlineParam updateParam = (isUndo ? PreviousParam : CurrentParam) as UnderlineParam;
				UnderlineParam checkParam = (isUndo ? CurrentParam : PreviousParam) as UnderlineParam;

                if (!CheckArrayEqual(updateParam.UnderlineColor, checkParam.UnderlineColor))
                {
                    if (updateParam.UnderlineColor != null && updateParam.UnderlineColor.Length == 3)
                    {
                        underlineAnnot.SetColor(updateParam.UnderlineColor);
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
				{
					underlineAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if (updateParam.Author != checkParam.Author)
				{
					underlineAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					underlineAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					underlineAnnot.SetIsLocked(updateParam.Locked);
				}

				underlineAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				underlineAnnot.UpdateAp();

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
