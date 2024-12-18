using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer.Annot;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.UndoManger
{
    public class FreeTextAnnotHistory:AnnotHistory
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
            FreeTextParam currentParam = CurrentParam as FreeTextParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }
            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFFreeTextAnnotation textAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT) as CPDFFreeTextAnnotation;
            if (textAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;

                if (currentParam.LineColor!=null && currentParam.LineColor.Length==3)
                {
                    textAnnot.SetLineColor(currentParam.LineColor);
                }
               
               
                if(currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        textAnnot.SetBgColor(currentParam.BgColor);
                    }
                }

                textAnnot.SetTransparency((byte)currentParam.Transparency);
                textAnnot.SetLineWidth((byte)currentParam.LineWidth);

                textAnnot.SetFreetextAlignment(currentParam.Alignment);

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];

                if (currentParam.FontColor != null && currentParam.FontColor.Length == 3)
                {
                    fontColor = currentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)currentParam.FontSize;
                textAttr.FontName = currentParam.FontName;

                textAnnot.SetFreetextDa(textAttr);

                textAnnot.SetRect(currentParam.ClientRect);

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    textAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    textAnnot.SetContent(currentParam.Content);
                }

                if (currentParam.Dash != null && currentParam.Dash.Length > 0)
                {
                    textAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, currentParam.Dash);
                }

                textAnnot.SetIsLocked(currentParam.Locked);
                textAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                textAnnot.UpdateAp();
                textAnnot.ReleaseAnnot();

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
            if (CurrentParam as FreeTextParam == null || PreviousParam as FreeTextParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFFreeTextAnnotation textAnnot = Annot as CPDFFreeTextAnnotation;
                if (textAnnot == null || !textAnnot.IsValid())
                {
                    return false;
                }

                FreeTextParam updateParam = (isUndo ? PreviousParam : CurrentParam) as FreeTextParam;
                FreeTextParam checkParam = (isUndo ? CurrentParam : PreviousParam) as FreeTextParam;

                if(!CheckArrayEqual(updateParam.LineColor,checkParam.LineColor))
                {
                    if(updateParam.LineColor != null && updateParam.LineColor.Length==3)
                    {
                        textAnnot.SetLineColor(updateParam.LineColor);
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            textAnnot.SetBgColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        textAnnot.ClearBgColor();
                    }
                }

                if(updateParam.Transparency != checkParam.Transparency)
                {
                    textAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if(updateParam.LineWidth != checkParam.LineWidth)
                {
                    textAnnot.SetLineWidth((float)updateParam.LineWidth);
                }

                if(updateParam.Alignment != checkParam.Alignment)
                {
                    textAnnot.SetFreetextAlignment(updateParam.Alignment);
                }

                if(updateParam.FontName != checkParam.FontName)
                {
                    CTextAttribute textAttr = textAnnot.FreeTextDa;
                    textAttr.FontName = updateParam.FontName;
                    textAnnot.SetFreetextDa(textAttr);
                }

                if(updateParam.FontSize != checkParam.FontSize)
                {
                    CTextAttribute textAttr = textAnnot.FreeTextDa;
                    textAttr.FontSize = (float)updateParam.FontSize; 
                    textAnnot.SetFreetextDa(textAttr);
                }

                if(!CheckArrayEqual(updateParam.FontColor, checkParam.FontColor))
                {
                    if(updateParam.FontColor != null && updateParam.FontColor.Length==3)
                    {
                        CTextAttribute textAttr = textAnnot.FreeTextDa;
                        textAttr.FontColor = updateParam.FontColor;
                        textAnnot.SetFreetextDa(textAttr);
                    }
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    textAnnot.SetRect(updateParam.ClientRect);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    textAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    textAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    textAnnot.SetIsLocked(updateParam.Locked);
                }

                if (updateParam.Dash != null && updateParam.Dash.Length > 0)
                {
                    textAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, updateParam.Dash);
                }
                else
                {
                    textAnnot.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                }

                textAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                textAnnot.UpdateAp();

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
