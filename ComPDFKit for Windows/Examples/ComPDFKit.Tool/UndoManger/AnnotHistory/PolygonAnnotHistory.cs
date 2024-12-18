using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComPDFKit.Tool.UndoManger
{
    public class PolygonAnnotHistory : AnnotHistory
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
            PolygonMeasureParam currentParam = CurrentParam as PolygonMeasureParam;
            if (CurrentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFPolygonAnnotation polygonAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON) as CPDFPolygonAnnotation;

            if (polygonAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;

                if (currentParam.HasFillColor)
                {
                    if (currentParam.FillColor != null && currentParam.FillColor.Length == 3)
                    {
                        polygonAnnot.SetBgColor(currentParam.FillColor);
                    }
                }
                if (currentParam.LineColor != null && currentParam.LineColor.Length == 3)
                {
                    polygonAnnot.SetLineColor(currentParam.LineColor);
                }

                polygonAnnot.SetTransparency((byte)currentParam.Transparency);
                polygonAnnot.SetLineWidth(currentParam.LineWidth);

                polygonAnnot.SetPoints(currentParam.SavePoints);
                polygonAnnot.SetRect(currentParam.ClientRect);

                if (currentParam.LineDash != null)
                {
                    if (currentParam.LineDash.Length == 0)
                    {
                        polygonAnnot.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                    }
                    else
                    {
                        List<float> floatArray = new List<float>();
                        foreach (float num in currentParam.LineDash)
                        {
                            floatArray.Add(num);
                        }
                        polygonAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, floatArray.ToArray());
                    }
                }

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    polygonAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    polygonAnnot.SetContent(currentParam.Content);
                }

                polygonAnnot.SetIsLocked(currentParam.Locked);
                polygonAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                polygonAnnot.UpdateAp();
                polygonAnnot.ReleaseAnnot();

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
            return true;
        }

        internal override bool Update(bool isUndo)
        {
            if (CurrentParam as PolygonMeasureParam == null || PreviousParam as PolygonMeasureParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFPolygonAnnotation polygonAnnot = Annot as CPDFPolygonAnnotation;
                if (polygonAnnot == null || !polygonAnnot.IsValid() || !polygonAnnot.IsMeasured())
                {
                    return false;
                }


                PolygonMeasureParam updateParam = (isUndo ? PreviousParam : CurrentParam) as PolygonMeasureParam;
                PolygonMeasureParam checkParam = (isUndo ? CurrentParam : PreviousParam) as PolygonMeasureParam;

                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                    {
                        polygonAnnot.SetLineColor(updateParam.LineColor);
                    }
                }

                if (!CheckArrayEqual(updateParam.FillColor, checkParam.FillColor))
                {
                    if (updateParam.HasFillColor)
                    {
                        if (updateParam.FillColor != null && updateParam.FillColor.Length == 3)
                        {
                            polygonAnnot.SetBgColor(updateParam.FillColor);
                        }
                    }
                    else
                    {
                        polygonAnnot.ClearBgColor();
                    }

                    if (updateParam.Transparency != checkParam.Transparency)
                    {
                        polygonAnnot.SetTransparency((byte)updateParam.Transparency);
                    }

                    if (updateParam.LineWidth != checkParam.LineWidth)
                    {
                        polygonAnnot.SetLineWidth((byte)updateParam.LineWidth);
                    }

                    if (!CheckArrayEqual(updateParam.LineDash, checkParam.LineDash))
                    {
                        if (updateParam.LineDash != null)
                        {
                            if (updateParam.LineDash.Length == 0)
                            {
                                polygonAnnot.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                            }
                            else
                            {
                                List<float> floatArray = new List<float>();
                                foreach (float num in updateParam.LineDash)
                                {
                                    floatArray.Add(num);
                                }
                                polygonAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, floatArray.ToArray());
                            }
                        }
                    }


                    if (!updateParam.SavePoints.SequenceEqual(checkParam.SavePoints))
                    {
                        polygonAnnot.SetPoints(updateParam.SavePoints);
                    }

                    if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                    {
                        polygonAnnot.SetRect(updateParam.ClientRect);
                    }


                    if (updateParam.Author != checkParam.Author)
                    {
                        polygonAnnot.SetAuthor(updateParam.Author);
                    }

                    if (updateParam.Content != checkParam.Content)
                    {
                        polygonAnnot.SetContent(updateParam.Content);
                    }


                    if (updateParam.Locked != checkParam.Locked)
                    {
                        polygonAnnot.SetIsLocked(updateParam.Locked);
                    }
                }
                polygonAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());

                polygonAnnot.UpdateAp();

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