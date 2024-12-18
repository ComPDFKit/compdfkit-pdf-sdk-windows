using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ComPDFKit.PDFAnnotation.CTextAttribute;

namespace ComPDFKit.Tool.UndoManger
{
    public class PolyLineMeasureAnnotHistory : AnnotHistory
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
            PolyLineMeasureParam currentParam = CurrentParam as PolyLineMeasureParam;
            if (CurrentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFPolylineAnnotation polygonAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE) as CPDFPolylineAnnotation;
            if (polygonAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;

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

                CTextAttribute textAttribute = new CTextAttribute();
                textAttribute.FontColor = currentParam.FontColor;
                textAttribute.FontSize = (float)currentParam.FontSize;
                textAttribute.FontName = CFontNameHelper.ObtainFontName(CFontNameHelper.GetFontType(currentParam.FontName),
                            currentParam.IsBold,
                            currentParam.IsItalic);
                polygonAnnot.SetTextAttribute(textAttribute);
                if (currentParam.measureInfo != null)
                {
                    CPDFPerimeterMeasure polygonMeasure = polygonAnnot.GetPerimeterMeasure();
                    if (polygonMeasure != null)
                    {
                        polygonMeasure.SetMeasureInfo(currentParam.measureInfo);
                        polygonMeasure.SetMeasureScale(currentParam.measureInfo.RulerBase, currentParam.measureInfo.RulerBaseUnit,
                                                       currentParam.measureInfo.RulerTranslate, currentParam.measureInfo.RulerTranslateUnit);
                        polygonMeasure.UpdateAnnotMeasure();
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
            return false;
        }
        internal override bool Update(bool isUndo)
        {
            if (CurrentParam as PolyLineMeasureParam == null || PreviousParam as PolyLineMeasureParam == null)
            {
                return false;
            }
            if (MakeAnnotValid(CurrentParam))
            {
                CPDFPolylineAnnotation polygonAnnot = Annot as CPDFPolylineAnnotation;
                if (polygonAnnot == null || !polygonAnnot.IsValid() || !polygonAnnot.IsMeasured())
                {
                    return false;
                }

                PolyLineMeasureParam updateParam = (isUndo ? PreviousParam : CurrentParam) as PolyLineMeasureParam;
                PolyLineMeasureParam checkParam = (isUndo ? CurrentParam : PreviousParam) as PolyLineMeasureParam;
                if (!CheckArrayEqual(updateParam.LineColor, checkParam.LineColor))
                {
                    if (updateParam.LineColor != null && updateParam.LineColor.Length == 3)
                    {
                        polygonAnnot.SetLineColor(updateParam.LineColor);
                    }
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

                if (updateParam.measureInfo != checkParam.measureInfo)
                {
                    CPDFPerimeterMeasure polygonMeasure = polygonAnnot.GetPerimeterMeasure();
                    if (polygonMeasure != null)
                    {
                        polygonMeasure.SetMeasureInfo(updateParam.measureInfo);
                        polygonMeasure.SetMeasureScale(updateParam.measureInfo.RulerBase, updateParam.measureInfo.RulerBaseUnit,
                                                       updateParam.measureInfo.RulerTranslate, updateParam.measureInfo.RulerTranslateUnit);
                        polygonMeasure.UpdateAnnotMeasure();
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
