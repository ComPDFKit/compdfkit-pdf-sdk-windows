using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class ComboBoxHistory:AnnotHistory
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
            ComboBoxParam currentParam = CurrentParam as ComboBoxParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFComboBoxWidget comboboxWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_COMBOBOX) as CPDFComboBoxWidget;
			if (comboboxWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					comboboxWidget.SetFieldName(currentParam.FieldName);
				}
				
				if(currentParam.HasLineColor)
				{
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        comboboxWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

				if(currentParam.HasBgColor)
				{
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
					{
                        comboboxWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }
				
				comboboxWidget.SetBorderWidth((float)currentParam.LineWidth);
				comboboxWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

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
					currentParam.IsBold,
					currentParam.IsItalic);

				comboboxWidget.SetTextAttribute(textAttr);

				if(currentParam.OptionItems!=null && currentParam.OptionItems.Count > 0)
				{
					int addIndex = 0;
					foreach (string key in currentParam.OptionItems.Keys)
					{
						comboboxWidget.AddOptionItem(addIndex, currentParam.OptionItems[key], key);
						addIndex++;
					}
				}

				if(currentParam.SelectItemsIndex!=null && currentParam.SelectItemsIndex.Count > 0)
				{
					comboboxWidget.SelectItem(currentParam.SelectItemsIndex[0]);
				}

				comboboxWidget.SetRect(currentParam.ClientRect);

				comboboxWidget.SetFlags(currentParam.Flags);
				comboboxWidget.SetIsLocked(currentParam.Locked);
				comboboxWidget.SetIsReadOnly(currentParam.IsReadOnly);
				comboboxWidget.SetIsHidden(currentParam.IsHidden);

				comboboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				comboboxWidget.UpdateFormAp();
				comboboxWidget.ReleaseAnnot();

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
			if (CurrentParam as ComboBoxParam == null || PreviousParam as ComboBoxParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFComboBoxWidget comboboxWidget = Annot as CPDFComboBoxWidget;
				if (comboboxWidget == null || !comboboxWidget.IsValid())
				{
					return false;
				}

				ComboBoxParam updateParam = (isUndo ? PreviousParam : CurrentParam) as ComboBoxParam;
				ComboBoxParam checkParam = (isUndo ? CurrentParam : PreviousParam) as ComboBoxParam;

				if (updateParam.FieldName != checkParam.FieldName)
				{
					comboboxWidget.SetFieldName(updateParam.FieldName);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            comboboxWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        comboboxWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            comboboxWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        comboboxWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth!=checkParam.LineWidth)
				{
					comboboxWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if(updateParam.BorderStyle!=checkParam.BorderStyle)
				{
					comboboxWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if (updateParam.FontName != checkParam.FontName)
				{
					CTextAttribute textAttr = comboboxWidget.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(updateParam.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, isItalic);
					comboboxWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.FontSize != checkParam.FontSize)
				{
					CTextAttribute textAttr = comboboxWidget.GetTextAttribute();
					textAttr.FontSize = (float)updateParam.FontSize;
					comboboxWidget.SetTextAttribute(textAttr);
				}

				if (!CheckArrayEqual(updateParam.FontColor,checkParam.FontColor))
				{
                    if (updateParam.FontColor != null && updateParam.FontColor.Length == 3)
					{
                        CTextAttribute textAttr = comboboxWidget.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        comboboxWidget.SetTextAttribute(textAttr);
                    }
				}

				if (updateParam.IsBold != checkParam.IsBold)
				{
					CTextAttribute textAttr = comboboxWidget.GetTextAttribute();
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, updateParam.IsBold, isItalic);
					comboboxWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.IsItalic != checkParam.IsItalic)
				{
					CTextAttribute textAttr = comboboxWidget.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, updateParam.IsItalic);
					comboboxWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.OptionItems != null)
				{
					int optionsCount = comboboxWidget.GetItemsCount();
					for (int i = 0; i < optionsCount; i++)
					{
						comboboxWidget.RemoveOptionItem(0);
					}

					int addIndex = 0;
					foreach (string key in updateParam.OptionItems.Keys)
					{
						comboboxWidget.AddOptionItem(addIndex, updateParam.OptionItems[key], key);
						addIndex++;
					}
				}

                if (updateParam.SelectItemsIndex != null)
                {
                    if (updateParam.SelectItemsIndex.Count > 0)
                    {
                        comboboxWidget.SelectItem(updateParam.SelectItemsIndex[0]);
                    }
                }


                if (updateParam.SelectItemsIndex != null && updateParam.SelectItemsIndex.Count > 0)
				{
					comboboxWidget.SelectItem(updateParam.SelectItemsIndex[0]);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    comboboxWidget.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Flags != checkParam.Flags)
				{
					comboboxWidget.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					comboboxWidget.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					comboboxWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					comboboxWidget.SetIsHidden(updateParam.IsHidden);
				}

				comboboxWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				comboboxWidget.UpdateFormAp();

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
