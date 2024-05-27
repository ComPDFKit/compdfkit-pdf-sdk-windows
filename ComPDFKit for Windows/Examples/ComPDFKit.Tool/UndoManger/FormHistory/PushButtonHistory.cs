using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class PushButtonHistory:AnnotHistory
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
            PushButtonParam currentParam = CurrentParam as PushButtonParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}
			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFPushButtonWidget pushbuttonWidget = pdfPage.CreateWidget(C_WIDGET_TYPE.WIDGET_PUSHBUTTON) as CPDFPushButtonWidget;
			if (pushbuttonWidget != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;

				if (!string.IsNullOrEmpty(currentParam.FieldName))
				{
					pushbuttonWidget.SetFieldName(currentParam.FieldName);
				}

                if (currentParam.HasLineColor)
                {
                    if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                    {
                        pushbuttonWidget.SetWidgetBorderRGBColor(currentParam.LineColor);
                    }
                }

				if(currentParam.HasBgColor)
				{
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        pushbuttonWidget.SetWidgetBgRGBColor(currentParam.BgColor);
                    }
                }

				if(!string.IsNullOrEmpty(currentParam.Text))
				{
					pushbuttonWidget.SetButtonTitle(currentParam.Text);
				}

				pushbuttonWidget.SetBorderWidth((float)currentParam.LineWidth);
				pushbuttonWidget.SetWidgetBorderStyle(currentParam.BorderStyle);

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

				pushbuttonWidget.SetTextAttribute(textAttr);

				switch (currentParam.Action)
				{
					case C_ACTION_TYPE.ACTION_TYPE_GOTO:
						{
							CPDFGoToAction gotoAction = new CPDFGoToAction();
							CPDFDestination destination = new CPDFDestination();
							destination.Position_X = currentParam.DestinationPosition.x;
							destination.Position_Y = currentParam.DestinationPosition.y;
							destination.PageIndex = currentParam.DestinationPageIndex;
							gotoAction.SetDestination(PDFDoc, destination);
							pushbuttonWidget.SetButtonAction(gotoAction);
						}
						break;
					case C_ACTION_TYPE.ACTION_TYPE_URI:
						{
							CPDFUriAction uriAction = new CPDFUriAction();
							uriAction.SetUri(currentParam.Uri);
							pushbuttonWidget.SetButtonAction(uriAction);
						}
						break;
					default:
						break;
				}

				pushbuttonWidget.SetRect(currentParam.ClientRect);
				pushbuttonWidget.SetFlags(currentParam.Flags);
				pushbuttonWidget.SetIsLocked(currentParam.Locked);
				pushbuttonWidget.SetIsReadOnly(currentParam.IsReadOnly);
				pushbuttonWidget.SetIsHidden(currentParam.IsHidden);

				pushbuttonWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				pushbuttonWidget.UpdateFormAp();
				pushbuttonWidget.ReleaseAnnot();

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
			if (CurrentParam as PushButtonParam == null || PreviousParam as PushButtonParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFPushButtonWidget pushbutton = Annot as CPDFPushButtonWidget;
				if (pushbutton == null || !pushbutton.IsValid())
				{
					return false;
				}

				PushButtonParam updateParam = (isUndo ? PreviousParam : CurrentParam) as PushButtonParam;
				PushButtonParam checkParam = (isUndo ? CurrentParam : PreviousParam) as PushButtonParam;

				if (updateParam.FieldName != checkParam.FieldName)
				{
					pushbutton.SetFieldName(updateParam.FieldName);
				}

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.HasLineColor)
                    {
                        if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                        {
                            pushbutton.SetWidgetBorderRGBColor(updateParam.LineColor);
                        }
                    }
                    else
                    {
                        pushbutton.ClearWidgetBorderRGBColor();
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            pushbutton.SetWidgetBgRGBColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        pushbutton.ClearWidgetBgRGBColor();
                    }
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
				{
					pushbutton.SetBorderWidth((float)updateParam.LineWidth);
				}

				if (updateParam.BorderStyle != checkParam.BorderStyle)
				{
					pushbutton.SetWidgetBorderStyle(updateParam.BorderStyle);
				}

				if (updateParam.FontName != checkParam.FontName)
				{
					CTextAttribute textAttr = pushbutton.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(updateParam.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, isItalic);
					pushbutton.SetTextAttribute(textAttr);
				}

				if (updateParam.FontSize != checkParam.FontSize)
				{
					CTextAttribute textAttr = pushbutton.GetTextAttribute();
					textAttr.FontSize = (float)updateParam.FontSize;
					pushbutton.SetTextAttribute(textAttr);
				}

                if (!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
                {
                    if (updateParam.FontColor != null && updateParam.FontColor.Length == 3)
                    {
                        CTextAttribute textAttr = pushbutton.GetTextAttribute();
                        textAttr.FontColor = updateParam.FontColor;
                        pushbutton.SetTextAttribute(textAttr);
                    }
                }

                if (updateParam.IsBold != checkParam.IsBold)
				{
					CTextAttribute textAttr = pushbutton.GetTextAttribute();
					bool isItalic = IsItalic(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, updateParam.IsBold, isItalic);
					pushbutton.SetTextAttribute(textAttr);
				}

				if (updateParam.IsItalic != checkParam.IsItalic)
				{
					CTextAttribute textAttr = pushbutton.GetTextAttribute();
					bool isBold = IsBold(textAttr.FontName);
					FontType fontType = GetFontType(textAttr.FontName);
					textAttr.FontName = ObtainFontName(fontType, isBold, updateParam.IsItalic);
					pushbutton.SetTextAttribute(textAttr);
				}

				switch (updateParam.Action)
				{
					case C_ACTION_TYPE.ACTION_TYPE_GOTO:
						{
							CPDFGoToAction gotoAction = new CPDFGoToAction();
							CPDFDestination destination = new CPDFDestination();
							destination.Position_X = updateParam.DestinationPosition.x;
							destination.Position_Y = updateParam.DestinationPosition.y;
							destination.PageIndex = updateParam.DestinationPageIndex;
							gotoAction.SetDestination(PDFDoc, destination);
							pushbutton.SetButtonAction(gotoAction);
						}
						break;
					case C_ACTION_TYPE.ACTION_TYPE_URI:
						{
							CPDFUriAction uriAction = new CPDFUriAction();
							uriAction.SetUri(updateParam.Uri);
							pushbutton.SetButtonAction(uriAction);
						}
						break;
					default:
						break;
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
				{
					pushbutton.SetRect(updateParam.ClientRect);
				}

				if (updateParam.Flags != checkParam.Flags)
				{
					pushbutton.SetFlags(updateParam.Flags);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					pushbutton.SetIsLocked(updateParam.Locked);
				}

				if (updateParam.IsReadOnly != checkParam.IsReadOnly)
				{
					pushbutton.SetIsReadOnly(updateParam.IsReadOnly);
				}

				if (updateParam.IsHidden != checkParam.IsHidden)
				{
					pushbutton.SetIsHidden(updateParam.IsHidden);
				}

				pushbutton.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				pushbutton.UpdateFormAp();

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
