using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class StrikeoutAnnotHistory:AnnotHistory
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
            StrikeoutParam currentParam = CurrentParam as StrikeoutParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFStrikeoutAnnotation strikeoutAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT) as CPDFStrikeoutAnnotation;
			if (strikeoutAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				strikeoutAnnot.SetTransparency((byte)currentParam.Transparency);
				strikeoutAnnot.SetRect(currentParam.ClientRect);
				
				if (currentParam.QuardRects != null)
				{
                    strikeoutAnnot.SetQuardRects(currentParam.QuardRects);
                }

                if (currentParam.StrikeoutColor != null && currentParam.StrikeoutColor.Length == 3)
                {
                    strikeoutAnnot.SetColor(currentParam.StrikeoutColor);
                }

                if (!string.IsNullOrEmpty(currentParam.Author))
				{
					strikeoutAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					strikeoutAnnot.SetContent(currentParam.Content);
				}
				strikeoutAnnot.SetIsLocked(currentParam.Locked);
				strikeoutAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				strikeoutAnnot.UpdateAp();
				strikeoutAnnot.ReleaseAnnot();

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
			if (CurrentParam as StrikeoutParam == null || PreviousParam as StrikeoutParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFStrikeoutAnnotation strikeoutAnnot = Annot as CPDFStrikeoutAnnotation;
				if (strikeoutAnnot == null || !strikeoutAnnot.IsValid())
				{
					return false;
				}

				StrikeoutParam updateParam = (isUndo ? PreviousParam : CurrentParam) as StrikeoutParam;
				StrikeoutParam checkParam = (isUndo ? CurrentParam : PreviousParam) as StrikeoutParam;

                if (!CheckArrayEqual(updateParam.StrikeoutColor, checkParam.StrikeoutColor))
                {
                    if (updateParam.StrikeoutColor != null && updateParam.StrikeoutColor.Length == 3)
                    {
                        strikeoutAnnot.SetColor(updateParam.StrikeoutColor);
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
				{
					strikeoutAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if (updateParam.Author != checkParam.Author)
				{
					strikeoutAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					strikeoutAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					strikeoutAnnot.SetIsLocked(updateParam.Locked);
				}

				strikeoutAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				strikeoutAnnot.UpdateAp();

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
