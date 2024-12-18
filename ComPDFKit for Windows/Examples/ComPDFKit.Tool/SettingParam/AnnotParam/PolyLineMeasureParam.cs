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
            Transparency = 255;
        }
        public byte[] LineColor { get; set; } = new byte[] { 255, 0, 0, };
        public float LineWidth { get; set; } = 2;
        public float[] LineDash { get; set; }
        public List<CPoint> SavePoints { get; set; }
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 14;
        public byte[] FontColor { get; set; } = new byte[] { 255, 0, 0, };
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public CPDFMeasureInfo measureInfo { get; set; } =
            new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH,
            };

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
                    RulerTranslate = measureInfo.RulerTranslate,
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
