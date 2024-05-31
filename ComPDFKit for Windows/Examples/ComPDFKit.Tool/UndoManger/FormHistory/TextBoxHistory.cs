using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class TextBoxHistory:AnnotHistory
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
            TextBoxParam currentParam = CurrentParam as TextBoxParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFTextWidget textWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_TEXTFIELD) as CPDFTextWidget;
			if(textWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					textWidget.SetFieldName(currentParam.FieldName);
				}

                if (currentParam.HasLineColor)
                {
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        textWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

				if(currentParam.HasBgColor)
				{
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        textWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }

				if(!string.IsNullOrEmpty(currentParam.Text))
				{
					textWidget.SetText(currentParam.Text);
				}

				CTextAttribute textAttr = new CTextAttribute();
				byte[] fontColor = new byte[3];
				if(currentParam.FontColor != null && currentParam.FontColor.Length==3)
				{
					fontColor = currentParam.FontColor;
                }
				textAttr.FontColor = fontColor;
				textAttr.FontSize = (float)currentParam.FontSize;
				textAttr.FontName = ObtainFontName(
					GetFontType(currentParam.FontName),
					currentParam.IsBold,
					currentParam.IsItalic);

				textWidget.SetTextAttribute(textAttr);
                textWidget.SetJustification(currentParam.Alignment);

				textWidget.SetBorderWidth((float)currentParam.LineWidth);
				textWidget.SetWidgetBorderStyle(currentParam.BorderStyle);
				textWidget.SetMultiLine(currentParam.IsMultiLine);

				textWidget.SetRect(currentParam.ClientRect);

				textWidget.SetFlags(currentParam.Flags);
				textWidget.SetIsLocked(currentParam.Locked);
				textWidget.SetIsReadOnly(currentParam.IsReadOnly);
				textWidget.SetIsHidden(currentParam.IsHidden);

				textWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				textWidget.UpdateFormAp();
				textWidget.ReleaseAnnot();

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
			if (CurrentParam as TextBoxParam == null || PreviousParam as TextBoxParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFTextWidget textWidget = Annot as CPDFTextWidget;
				if (textWidget == null || !textWidget.IsValid())
				{
					return false;
				}

				TextBoxParam updateParam = (isUndo ? PreviousParam : CurrentParam) as TextBoxParam;
				TextBoxParam checkParam = (isUndo ? CurrentParam : PreviousParam) as TextBoxParam;

				if(updateParam.FieldName!=checkParam.FieldName)
				{
					textWidget.SetFieldName(updateParam.FieldName);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            textWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        textWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            textWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        textWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.Text!=checkParam.Text)
				{
					textWidget.SetText(updateParam.Text);
				}

				if (updateParam.FontName != checkParam.FontName)
				{
					CTextAttribute textAttr = textWidget.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(updateParam.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, isItalic);
					textWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.FontSize != checkParam.FontSize)
				{
					CTextAttribute textAttr = textWidget.GetTextAttribute();
					textAttr.FontSize = (float)updateParam.FontSize;
					textWidget.SetTextAttribute(textAttr);
				}

                if (!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
				{
					if(updateParam.FontColor != null && updateParam.FontColor.Length==3)
					{
                        CTextAttribute textAttr = textWidget.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        textWidget.SetTextAttribute(textAttr);
                    }
				}

				if (updateParam.IsBold != checkParam.IsBold)
				{
					CTextAttribute textAttr = textWidget.GetTextAttribute();
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, updateParam.IsBold, isItalic);
					textWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.IsItalic != checkParam.IsItalic)
				{
					CTextAttribute textAttr = textWidget.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, updateParam.IsItalic);
					textWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.Alignment != checkParam.Alignment)
				{
                    textWidget.SetJustification(updateParam.Alignment);
                }

				if(updateParam.LineWidth != checkParam.LineWidth)
				{
					textWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if(updateParam.BorderStyle != checkParam.BorderStyle)
				{
					textWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if(updateParam.IsMultiLine!=checkParam.IsMultiLine)
				{
					textWidget.SetMultiLine(updateParam.IsMultiLine);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    textWidget.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Flags != checkParam.Flags)
				{
					textWidget.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					textWidget.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					textWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					textWidget.SetIsHidden(updateParam.IsHidden);
				}

				textWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				textWidget.UpdateFormAp();

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
