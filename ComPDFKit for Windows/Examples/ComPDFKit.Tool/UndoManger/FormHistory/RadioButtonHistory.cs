using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Tool.UndoManger
{
    public class RadioButtonHistory:AnnotHistory
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
			RadioButtonParam currentParam = CurrentParam as RadioButtonParam;

            if (currentParam  == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFRadioButtonWidget radioWidget=pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_RADIOBUTTON) as CPDFRadioButtonWidget;
			if (radioWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if(!string.IsNullOrEmpty(currentParam.FieldName))
				{
					radioWidget.SetFieldName(currentParam.FieldName);
				}

				radioWidget.SetWidgetCheckStyle(currentParam.CheckStyle);
				radioWidget.SetChecked(currentParam.IsChecked);

                if (currentParam.HasLineColor)
                {
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        radioWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

                if (currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        radioWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }

                radioWidget.SetBorderWidth((float)currentParam.LineWidth);
				radioWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

				if(currentParam.FontColor!=null && currentParam.FontColor.Length==3)
				{
					CTextAttribute textAttr = radioWidget.GetTextAttribute();
					textAttr.FontColor = currentParam.FontColor;
					radioWidget.SetTextAttribute(textAttr);
				}

				radioWidget.SetRect(currentParam.ClientRect);

				radioWidget.SetFlags(currentParam.Flags);
				radioWidget.SetIsLocked(currentParam.Locked);
				radioWidget.SetIsReadOnly(currentParam.IsReadOnly);
				radioWidget.SetIsHidden(currentParam.IsHidden);

				radioWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				radioWidget.UpdateFormAp();
				radioWidget.ReleaseAnnot();

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
			if (CurrentParam as RadioButtonParam == null || PreviousParam as RadioButtonParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFRadioButtonWidget radioWidget = Annot as CPDFRadioButtonWidget;
				if (radioWidget == null || !radioWidget.IsValid())
				{
					return false;
				}

				RadioButtonParam updateParam = (isUndo ? PreviousParam : CurrentParam) as RadioButtonParam;
				RadioButtonParam checkParam = (isUndo ? CurrentParam : PreviousParam) as RadioButtonParam;

				if(updateParam.FieldName != checkParam.FieldName)
				{
					radioWidget.SetFieldName(updateParam.FieldName);
				}

				if(updateParam.CheckStyle != checkParam.CheckStyle) 
				{ 
					radioWidget.SetWidgetCheckStyle(updateParam.CheckStyle);
				}

				if(updateParam.IsChecked != checkParam.IsChecked)
				{
					radioWidget.SetChecked(updateParam.IsChecked);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            radioWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        radioWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            radioWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        radioWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
				{
					radioWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if (updateParam.BorderStyle != checkParam.BorderStyle)
				{
					radioWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if(!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
				{
					if(updateParam.FontColor!=null && updateParam.FontColor.Length==3)
					{
                        CTextAttribute textAttr = radioWidget.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        radioWidget.SetTextAttribute(textAttr);
                    }
				}

				if(!updateParam.ClientRect.Equals(checkParam.ClientRect))
				{
					radioWidget.SetRect(updateParam.ClientRect);
				}

				if(updateParam.Flags!=checkParam.Flags)
				{
					radioWidget.SetFlags(updateParam.Flags);
				}

				if(updateParam.Locked!=checkParam.Locked)
				{
					radioWidget.SetIsLocked(updateParam.Locked);
				}

				if(updateParam.IsReadOnly!=checkParam.IsReadOnly)
				{
					radioWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if(updateParam.IsHidden!=checkParam.IsHidden)
				{
					radioWidget.SetIsHidden(updateParam.IsHidden);
				}

				radioWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				radioWidget.UpdateFormAp();

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
