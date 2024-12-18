using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class ListBoxHistory:AnnotHistory
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
            ListBoxParam currentParam = CurrentParam as ListBoxParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFListBoxWidget listboxWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_LISTBOX) as CPDFListBoxWidget;
			if (listboxWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					listboxWidget.SetFieldName(currentParam.FieldName);
				}

				if(currentParam.HasLineColor)
				{
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        listboxWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

				if(currentParam.HasBgColor)
				{
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
					{
                        listboxWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }
				
				listboxWidget.SetBorderWidth((float)currentParam.LineWidth);
				listboxWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

				CTextAttribute textAttr = new CTextAttribute();
				byte[] fontColor = new byte[3];
                if (currentParam.FontColor != null && currentParam.FontColor.Length == 3)
                {
                    fontColor = currentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
				textAttr.FontSize = (float)currentParam.FontSize;
				textAttr.FontName = currentParam.FontName;

                listboxWidget.SetTextAttribute(textAttr);

				if (currentParam.OptionItems != null && currentParam.OptionItems.Count > 0)
				{
					int addIndex = 0;
					foreach (string key in currentParam.OptionItems.Keys)
					{
						listboxWidget.AddOptionItem(addIndex, currentParam.OptionItems[key], key);
						addIndex++;
					}
				}

				if (currentParam.SelectItemsIndex != null && currentParam.SelectItemsIndex.Count > 0)
				{
					listboxWidget.SelectItem(currentParam.SelectItemsIndex[0]);
				}

				listboxWidget.SetRect(currentParam.ClientRect);

				listboxWidget.SetFlags(currentParam.Flags);
				listboxWidget.SetIsLocked(currentParam.Locked);
				listboxWidget.SetIsReadOnly(currentParam.IsReadOnly);
				listboxWidget.SetIsHidden(currentParam.IsHidden);

				listboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				listboxWidget.UpdateFormAp();
				listboxWidget.ReleaseAnnot();

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
			if (CurrentParam as ListBoxParam == null || PreviousParam as ListBoxParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFListBoxWidget listboxWidget = Annot as CPDFListBoxWidget;
				if (listboxWidget == null || !listboxWidget.IsValid())
				{
					return false;
				}

				ListBoxParam updateParam = (isUndo ? PreviousParam : CurrentParam) as ListBoxParam;
				ListBoxParam checkParam = (isUndo ? CurrentParam : PreviousParam) as ListBoxParam;

				if (updateParam.FieldName != checkParam.FieldName)
				{
					listboxWidget.SetFieldName(updateParam.FieldName);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            listboxWidget.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        listboxWidget.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            listboxWidget.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        listboxWidget.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
				{
					listboxWidget.SetBorderWidth((float)updateParam.LineWidth);
				}

				if (updateParam.BorderStyle != checkParam.BorderStyle)
				{
					listboxWidget.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if (updateParam.FontName != checkParam.FontName)
				{
					CTextAttribute textAttr = listboxWidget.GetTextAttribute();
					textAttr.FontName = updateParam.FontName;
                    listboxWidget.SetTextAttribute(textAttr);
				}

				if (updateParam.FontSize != checkParam.FontSize)
				{
					CTextAttribute textAttr = listboxWidget.GetTextAttribute();
					textAttr.FontSize = (float)updateParam.FontSize;
					listboxWidget.SetTextAttribute(textAttr);
				}

                if (!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
                {
                    if (updateParam.FontColor != null && updateParam.FontColor.Length == 3)
                    {
                        CTextAttribute textAttr = listboxWidget.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        listboxWidget.SetTextAttribute(textAttr);
                    }
                }

				if (updateParam.OptionItems != null)
				{
					int optionsCount = listboxWidget.GetItemsCount();
					for (int i = 0; i < optionsCount; i++)
					{
						listboxWidget.RemoveOptionItem(0);
					}

					int addIndex = 0;
					foreach (string key in updateParam.OptionItems.Keys)
					{
						listboxWidget.AddOptionItem(addIndex, updateParam.OptionItems[key], key);
						addIndex++;
					}
				}

				if (updateParam.SelectItemsIndex != null)
				{
					if (updateParam.SelectItemsIndex.Count > 0)
					{
						listboxWidget.SelectItem(updateParam.SelectItemsIndex[0]);
					}
				}


				if (updateParam.SelectItemsIndex != null && updateParam.SelectItemsIndex.Count > 0)
				{
					listboxWidget.SelectItem(updateParam.SelectItemsIndex[0]);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
				{
					listboxWidget.SetRect(updateParam.ClientRect);
				}

				if (updateParam.Flags != checkParam.Flags)
				{
					listboxWidget.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					listboxWidget.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					listboxWidget.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					listboxWidget.SetIsHidden(updateParam.IsHidden);
				}

				listboxWidget.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				listboxWidget.UpdateFormAp();

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
