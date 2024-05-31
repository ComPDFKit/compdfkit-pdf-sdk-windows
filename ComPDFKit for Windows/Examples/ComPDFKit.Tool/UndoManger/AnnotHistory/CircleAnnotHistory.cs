using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
    public class CircleAnnotHistory:AnnotHistory
    {
        public override int GetAnnotIndex()
        {
            if(CurrentParam!=null)
            {
                return CurrentParam.AnnotIndex;
            }
            return base.GetAnnotIndex();
        }

        public override int GetPageIndex()
        {
            if(CurrentParam!=null)
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
            CircleParam currentParam = CurrentParam as CircleParam;
            
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFCircleAnnotation circleAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE) as CPDFCircleAnnotation;
            if (circleAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
                
                if(currentParam.LineColor!=null && currentParam.LineColor.Length==3)
                {
                    circleAnnot.SetLineColor(currentParam.LineColor);
                }

                if (currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        circleAnnot.SetBgColor(currentParam.BgColor);
                    }
                }

                circleAnnot.SetTransparency((byte)currentParam.Transparency);
                circleAnnot.SetLineWidth((byte)currentParam.LineWidth);
                circleAnnot.SetRect(currentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (currentParam.LineDash != null)
                {
                    foreach (float num in currentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                circleAnnot.SetBorderStyle(currentParam.BorderStyle, floatArray.ToArray());

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    circleAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    circleAnnot.SetContent(currentParam.Content);
                }
                circleAnnot.SetIsLocked(currentParam.Locked);
                circleAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                circleAnnot.UpdateAp();
                circleAnnot.ReleaseAnnot();
                
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
            if (CurrentParam as CircleParam == null || PreviousParam as CircleParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFCircleAnnotation circleAnnot = Annot as CPDFCircleAnnotation;
                if (circleAnnot == null || !circleAnnot.IsValid())
                {
                    return false;
                }

                CircleParam updateParam = (isUndo ? PreviousParam : CurrentParam) as CircleParam;
                CircleParam checkParam = (isUndo ? CurrentParam : PreviousParam) as CircleParam;

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if(updateParam.LineColor != null && updateParam.LineColor.Length==3)
                    {
                        circleAnnot.SetLineColor(updateParam.LineColor);
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor ==false)
                    {
                        circleAnnot.ClearBgColor();
                    }
                    else
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            circleAnnot.SetBgColor(updateParam.BgColor);
                        }
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
                {
                    circleAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
                {
                    circleAnnot.SetLineWidth((byte)updateParam.LineWidth);
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    circleAnnot.SetRect(updateParam.ClientRect);
                }

                if(updateParam.LineDash!=null)
                {
                    List<float> floatArray = new List<float>();
                    if (updateParam.LineDash != null && updateParam.LineDash.Length > 0)
                    {
                        foreach (double num in updateParam.LineDash)
                        {
                            floatArray.Add((float)num);
                        }
                    }
                    circleAnnot.SetBorderStyle(updateParam.BorderStyle, floatArray.ToArray());
                }
                else
                {
                    float[] dashArray = new float[0];
                    circleAnnot.SetBorderStyle(updateParam.BorderStyle, dashArray);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    circleAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    circleAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    circleAnnot.SetIsLocked(updateParam.Locked);
                }

                circleAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                circleAnnot.UpdateAp();

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
