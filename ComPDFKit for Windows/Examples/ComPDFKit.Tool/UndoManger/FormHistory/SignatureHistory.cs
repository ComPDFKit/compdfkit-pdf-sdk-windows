using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class SignatureHistory:AnnotHistory
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
            SignatureParam currentParam = CurrentParam as SignatureParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFSignatureWidget signWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS) as CPDFSignatureWidget;
			if (signWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					signWidget.SetFieldName(currentParam.FieldName);
				}

				if(currentParam.HasLineColor)
				{
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        signWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

                if (currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        signWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }

                signWidget.SetBorderWidth((float)currentParam.LineWidth);
				signWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

				signWidget.SetRect(currentParam.ClientRect);

				signWidget.SetFlags(currentParam.Flags);
				signWidget.SetIsLocked(currentParam.Locked);
				signWidget.SetIsReadOnly(currentParam.IsReadOnly);
				signWidget.SetIsHidden(currentParam.IsHidden);

				signWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				signWidget.UpdateFormAp();
				signWidget.ReleaseAnnot();

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
			if (CurrentParam as SignatureParam == null || PreviousParam as SignatureParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFSignatureWidget signWidget = Annot as CPDFSignatureWidget;
				if (signWidget == null || !signWidget.IsValid())
				{
					return false;
				}

				SignatureParam updateParam =( isUndo ? PreviousParam : CurrentParam) as SignatureParam;
				SignatureParam checkParam = (isUndo ? CurrentParam : PreviousParam) as SignatureParam;

				if (updateParam.FieldName != checkParam.FieldName)
				{
					signWidget.SetFieldName(updateParam.FieldName);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            signWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        signWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            signWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        signWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
				{
					signWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if(updateParam.BorderStyle != checkParam.BorderStyle)
				{
					signWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if(!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    signWidget.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Flags != checkParam.Flags)
				{
					signWidget.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					signWidget.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					signWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					signWidget.SetIsHidden(updateParam.IsHidden);
				}

				signWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				signWidget.UpdateFormAp();

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
