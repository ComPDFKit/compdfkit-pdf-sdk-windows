using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class CheckBoxHistory:AnnotHistory
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
            CheckBoxParam currentParam = CurrentParam as CheckBoxParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFCheckBoxWidget checkboxWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_CHECKBOX) as CPDFCheckBoxWidget;
			if (checkboxWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					checkboxWidget.SetFieldName(currentParam.FieldName);
				}

                checkboxWidget.SetWidgetCheckStyle(currentParam.CheckStyle);
				checkboxWidget.SetChecked(currentParam.IsChecked);

				if(currentParam.HasLineColor)
				{
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        checkboxWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

                if (currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        checkboxWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }

                checkboxWidget.SetBorderWidth((float)currentParam.LineWidth);
				checkboxWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

                if (currentParam.FontColor != null && currentParam.FontColor.Length == 3)
                {
                    CTextAttribute textAttr = checkboxWidget.GetTextAttribute();
					textAttr.FontColor = currentParam.FontColor;
                    checkboxWidget.SetTextAttribute(textAttr);
                }

                checkboxWidget.SetRect(currentParam.ClientRect);

				checkboxWidget.SetFlags(currentParam.Flags);
				checkboxWidget.SetIsLocked(currentParam.Locked);
				checkboxWidget.SetIsReadOnly(currentParam.IsReadOnly);
				checkboxWidget.SetIsHidden(currentParam.IsHidden);

				checkboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				checkboxWidget.UpdateFormAp();
				checkboxWidget.ReleaseAnnot();

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
			if (CurrentParam as CheckBoxParam == null || PreviousParam as CheckBoxParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFCheckBoxWidget checkboxWidget = Annot as CPDFCheckBoxWidget;
				if (checkboxWidget == null || !checkboxWidget.IsValid())
				{
					return false;
				}

				CheckBoxParam updateParam = (isUndo ? PreviousParam : CurrentParam) as CheckBoxParam;
				CheckBoxParam checkParam = (isUndo ? CurrentParam : PreviousParam) as CheckBoxParam;

				if (updateParam.FieldName != checkParam.FieldName)
				{
					checkboxWidget.SetFieldName(updateParam.FieldName);
				}

				if (updateParam.CheckStyle != checkParam.CheckStyle)
				{
					checkboxWidget.SetWidgetCheckStyle(updateParam.CheckStyle);
				}

				if (updateParam.IsChecked != checkParam.IsChecked)
				{
					checkboxWidget.SetChecked(updateParam.IsChecked);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            checkboxWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        checkboxWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            checkboxWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        checkboxWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
				{
					checkboxWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if (updateParam.BorderStyle != checkParam.BorderStyle)
				{
					checkboxWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

                if (!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
				{
					CTextAttribute textAttr = checkboxWidget.GetTextAttribute();
					textAttr.FontColor = updateParam.FontColor;
                    checkboxWidget.SetTextAttribute(textAttr);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    checkboxWidget.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Flags != checkParam.Flags)
				{
					checkboxWidget.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					checkboxWidget.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					checkboxWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					checkboxWidget.SetIsHidden(updateParam.IsHidden);
				}

				checkboxWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				checkboxWidget.UpdateFormAp();

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
