using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer.Annot;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
    public class SquareAnnotHistory:AnnotHistory
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
            SquareParam currentParam = CurrentParam as SquareParam;
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFSquareAnnotation squareAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE) as CPDFSquareAnnotation;
            if (squareAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
                if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                {
                    squareAnnot.SetLineColor(currentParam.LineColor);
                }

                if (currentParam.HasBgColor)
                {
                    if (currentParam.BgColor != null && currentParam.BgColor.Length == 3)
                    {
                        squareAnnot.SetBgColor(currentParam.BgColor);
                    }
                }

                squareAnnot.SetTransparency((byte)currentParam.Transparency);
                squareAnnot.SetLineWidth((byte)currentParam.LineWidth);
                squareAnnot.SetRect(currentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (currentParam.LineDash != null)
                {
                    foreach (float num in currentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                squareAnnot.SetBorderStyle(currentParam.BorderStyle, floatArray.ToArray());

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    squareAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    squareAnnot.SetContent(currentParam.Content);
                }
                squareAnnot.SetIsLocked(currentParam.Locked);
                squareAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                squareAnnot.UpdateAp();
                squareAnnot.ReleaseAnnot();

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
            if (CurrentParam as SquareParam == null || PreviousParam as SquareParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFSquareAnnotation squareAnnot = Annot as CPDFSquareAnnotation;
                if (squareAnnot == null || !squareAnnot.IsValid())
                {
                    return false;
                }

                SquareParam updateParam = (isUndo ? PreviousParam : CurrentParam) as SquareParam;
                SquareParam checkParam = (isUndo ? CurrentParam : PreviousParam) as SquareParam;

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                    {
                        squareAnnot.SetLineColor(updateParam.LineColor);
                    }
                }

                if (!CheckArrayEqual(updateParam.BgColor, checkParam.BgColor))
                {
                    if (updateParam.HasBgColor)
                    {
                        if (updateParam.BgColor != null && updateParam.BgColor.Length == 3)
                        {
                            squareAnnot.SetBgColor(updateParam.BgColor);
                        }
                    }
                    else
                    {
                        squareAnnot.ClearBgColor();
                    }
                }

                if (updateParam.Transparency != checkParam.Transparency)
                {
                    squareAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if (updateParam.LineWidth != checkParam.LineWidth)
                {
                    squareAnnot.SetLineWidth((byte)updateParam.LineWidth);
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    squareAnnot.SetRect(updateParam.ClientRect);
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
                    squareAnnot.SetBorderStyle(updateParam.BorderStyle, floatArray.ToArray());
                }
                else
                {
                    float[] dashArray = new float[0];
                    squareAnnot.SetBorderStyle(updateParam.BorderStyle, dashArray);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    squareAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    squareAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    squareAnnot.SetIsLocked(updateParam.Locked);
                }

                squareAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                squareAnnot.UpdateAp();

                return true;
            }

            return false;
        }

        internal override bool Remove()
        {
            if(MakeAnnotValid(CurrentParam))
            {
                Annot.RemoveAnnot();
                return true;
            }
            return false;
        }
    }
}
