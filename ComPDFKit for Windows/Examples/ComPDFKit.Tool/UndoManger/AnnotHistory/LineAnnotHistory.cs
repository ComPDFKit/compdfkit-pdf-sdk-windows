using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
    public class LineAnnotHistory:AnnotHistory
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
            LineParam currentParam = CurrentParam as LineParam;
            if (currentParam==null || PDFDoc==null || !PDFDoc.IsValid())
            {
               return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFLineAnnotation lineAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_LINE) as CPDFLineAnnotation;
            if (lineAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
                if(currentParam.HeadLineType!=C_LINE_TYPE.LINETYPE_NONE || currentParam.TailLineType!=C_LINE_TYPE.LINETYPE_NONE)
                {
                    lineAnnot.SetLineType(currentParam.HeadLineType, currentParam.TailLineType);
                }

                if(currentParam.LineColor!=null && currentParam.LineColor.Length==3)
                {
                    lineAnnot.SetLineColor(currentParam.LineColor);
                }

                if(currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        lineAnnot.SetBgColor(currentParam.BgColor);
                    }
                }

                lineAnnot.SetTransparency((byte)currentParam.Transparency);
                lineAnnot.SetLineWidth((byte)currentParam.LineWidth);
                lineAnnot.SetLinePoints(currentParam.HeadPoint, currentParam.TailPoint);
                lineAnnot.SetRect(currentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (currentParam.LineDash != null)
                {
                    foreach (float num in currentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                lineAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, floatArray.ToArray());

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    lineAnnot.SetAuthor(currentParam.Author);
                }

                if(!string.IsNullOrEmpty(currentParam.Content))
                {
                    lineAnnot.SetContent(currentParam.Content);
                }
                lineAnnot.SetIsLocked(currentParam.Locked);
                lineAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                lineAnnot.UpdateAp();
                lineAnnot.ReleaseAnnot();

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
            if (CurrentParam as LineParam == null || PreviousParam as LineParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFLineAnnotation lineAnnot = Annot as CPDFLineAnnotation;
                if (lineAnnot == null || !lineAnnot.IsValid())
                {
                    return false;
                }

                LineParam updateParam = (isUndo ? PreviousParam : CurrentParam)as LineParam;
                LineParam checkParam = (isUndo ? CurrentParam : PreviousParam) as LineParam;

                if(updateParam.HeadLineType!=checkParam.HeadLineType || updateParam.TailLineType!=checkParam.TailLineType)
                {
                    lineAnnot.SetLineType(updateParam.HeadLineType, updateParam.TailLineType);
                }

                if (!CheckArrayEqual(updateParam.LineColor,checkParam.LineColor))
                {
                    if(updateParam.LineColor != null && updateParam.LineColor.Length==3)
                    {
                        lineAnnot.SetLineColor(updateParam.LineColor);
                    }
                }

                if(!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            lineAnnot.SetBgColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        lineAnnot.ClearBgColor();
                    }
                }

                if(updateParam.Transparency!=checkParam.Transparency)
                {
                    lineAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if(updateParam.LineWidth!=checkParam.LineWidth)
                {
                    lineAnnot.SetLineWidth((byte)updateParam.LineWidth);
                }

                if(!updateParam.HeadPoint.Equals(checkParam.HeadPoint) || !updateParam.TailPoint.Equals(checkParam.TailPoint))
                {
                    lineAnnot.SetLinePoints(updateParam.HeadPoint,updateParam.TailPoint);
                }

                if(!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    lineAnnot.SetRect(updateParam.ClientRect);
                }

                if (updateParam.LineDash != null)
                {
                    List<float> floatArray = new List<float>();
                    if (updateParam.LineDash != null && updateParam.LineDash.Length > 0)
                    {
                        foreach (double num in updateParam.LineDash)
                        {
                            floatArray.Add((float)num);
                        }
                    }
                    lineAnnot.SetBorderStyle(updateParam.BorderStyle, floatArray.ToArray());
                }
                else
                {
                    float[] dashArray = new float[0];
                    lineAnnot.SetBorderStyle(updateParam.BorderStyle, dashArray);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    lineAnnot.SetAuthor(updateParam.Author);
                }

                if(updateParam.Content != checkParam.Content)
                {
                    lineAnnot.SetContent(updateParam.Content);
                }

                if(updateParam.Locked != checkParam.Locked) 
                {
                    lineAnnot.SetIsLocked(updateParam.Locked);
                }

                lineAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                lineAnnot.UpdateAp();

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
