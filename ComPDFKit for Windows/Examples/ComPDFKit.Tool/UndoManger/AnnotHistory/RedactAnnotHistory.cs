using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class RedactAnnotHistory:AnnotHistory
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
            RedactParam currentParam = CurrentParam as RedactParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}

			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFRedactAnnotation redactAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_REDACT) as CPDFRedactAnnotation;
			if (redactAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

                if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                {
                    redactAnnot.SetOutlineColor(currentParam.LineColor);
                }

                if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                {
                    redactAnnot.SetFillColor(currentParam.BgColor);
                }

                redactAnnot.SetTextAlignment(currentParam.Alignment);

				CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];

                if (currentParam.FontColor != null && currentParam.FontColor.Length == 3)
                {
                    fontColor = currentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
				textAttr.FontSize = (float)currentParam.FontSize;
				textAttr.FontName = ObtainFontName(
					GetFontType(currentParam.FontName),
					false,
					false);

				redactAnnot.SetTextAttribute(textAttr);
				
				if(currentParam.QuardRects != null)
				{
                    redactAnnot.SetQuardRects(currentParam.QuardRects);
                }
				
				redactAnnot.SetRect(currentParam.ClientRect);

				redactAnnot.SetBorderWidth(1);
				redactAnnot.SetTransparency((byte)currentParam.Transparency);

				if(!string.IsNullOrEmpty(currentParam.OverlayText))
				{
					redactAnnot.SetOverlayText(currentParam.OverlayText);
				}

				if (!string.IsNullOrEmpty(currentParam.Author))
				{
					redactAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					redactAnnot.SetContent(currentParam.Content);
				}

				redactAnnot.SetIsLocked(currentParam.Locked);
				redactAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				redactAnnot.UpdateAp();
				redactAnnot.ReleaseAnnot();

				if (CurrentParam != null)
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
			if (CurrentParam as RedactParam == null || PreviousParam as RedactParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFRedactAnnotation redactAnnot = Annot as CPDFRedactAnnotation;
				if (redactAnnot == null || !redactAnnot.IsValid())
				{
					return false;
				}

				RedactParam updateParam = (isUndo ? PreviousParam : CurrentParam) as RedactParam;
				RedactParam checkParam = (isUndo ? CurrentParam : PreviousParam) as RedactParam;

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                    {
                        redactAnnot.SetOutlineColor(updateParam.LineColor);
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                    {
                        redactAnnot.SetFillColor(updateParam.BgColor);
                    }
                }

                if (updateParam.Alignment != checkParam.Alignment)
				{
                    redactAnnot.SetTextAlignment(updateParam.Alignment);
                }

                if (updateParam.FontName != checkParam.FontName)
				{
					CTextAttribute textAttr = redactAnnot.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(updateParam.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, isItalic);
					redactAnnot.SetTextAttribute(textAttr);
				}

				if (updateParam.FontSize != checkParam.FontSize)
				{
					CTextAttribute textAttr = redactAnnot.GetTextAttribute();
					textAttr.FontSize = (float)updateParam.FontSize;
					redactAnnot.SetTextAttribute(textAttr);
				}

                if (!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
                {
                    if (updateParam.FontColor != null && updateParam.FontColor.Length == 3)
                    {
                        CTextAttribute textAttr = redactAnnot.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        redactAnnot.SetTextAttribute(textAttr);
                    }
                }

                if (updateParam.QuardRects != null)
				{
					redactAnnot.SetQuardRects(updateParam.QuardRects);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    redactAnnot.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Transparency != checkParam.Transparency)
				{
					redactAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if(updateParam.OverlayText != checkParam.OverlayText)
				{
					redactAnnot.SetOverlayText(updateParam.OverlayText);
				}

				if (updateParam.Author != checkParam.Author)
				{
					redactAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					redactAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					redactAnnot.SetIsLocked(updateParam.Locked);
				}

				redactAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				redactAnnot.UpdateAp();

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
