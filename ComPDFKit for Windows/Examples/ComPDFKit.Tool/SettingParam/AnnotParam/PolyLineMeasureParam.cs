using ComPDFKit.Import;
using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public class PolyLineMeasureParam : AnnotParam
    {
        public PolyLineMeasureParam()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE;
        }
        public byte[] LineColor { get; set; }
        public float LineWidth { get; set; }
        public float[] LineDash { get; set; }
        public List<CPoint> SavePoints { get; set; }
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public byte[] FontColor { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public CPDFMeasureInfo measureInfo { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            PolyLineMeasureParam polygonTransfer = transfer as PolyLineMeasureParam;
            if (polygonTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(polygonTransfer))
            {
                return false;
            }

            if (LineColor != null)
            {
                polygonTransfer.LineColor = (byte[])LineColor.Clone();
            }

            if (LineDash != null)
            {
                polygonTransfer.LineDash = (float[])LineDash.Clone();
            }

            if (SavePoints != null)
            {
                polygonTransfer.SavePoints.CopyTo(SavePoints.ToArray());
            }

            if (FontColor != null)
            {
                polygonTransfer.FontColor = (byte[])FontColor.Clone();
            }

            if (measureInfo != null)
            {
                CPDFMeasureInfo cPDFMeasureInfo = new CPDFMeasureInfo()
                {
                    Factor = measureInfo.Factor,
                    Unit = measureInfo.Unit,
                    DecimalSymbol = measureInfo.DecimalSymbol,
                    ThousandSymbol = measureInfo.ThousandSymbol,
                    Display = measureInfo.Display,
                    Precision = measureInfo.Precision,
                    UnitPrefix = measureInfo.UnitPrefix,
                    UnitSuffix = measureInfo.UnitSuffix,
                    UnitPosition = measureInfo.UnitPosition,
                    RulerBase = measureInfo.RulerBase,
                    RulerBaseUnit = measureInfo.RulerBaseUnit,
                    RulerTranslateUnit = measureInfo.RulerTranslateUnit,
                    CaptionType = measureInfo.CaptionType,
                    RulerTranslate= measureInfo.RulerTranslate,
                };
                polygonTransfer.measureInfo = cPDFMeasureInfo;
            }

            polygonTransfer.LineWidth = LineWidth;
            polygonTransfer.FontName = FontName;
            polygonTransfer.FontSize = FontSize;
            polygonTransfer.IsBold = IsBold;
            polygonTransfer.IsItalic = IsItalic;
            polygonTransfer.BorderStyle = BorderStyle;

            return true;
        }
    }
}
