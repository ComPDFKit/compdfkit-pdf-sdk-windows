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
    public class LineMeasureParam : AnnotParam
    {
        public LineMeasureParam()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINE;
            Transparency = 255;
        }
        public byte[] LineColor { get; set; } = new byte[] { 255, 0, 0 };
        public float LineWidth { get; set; } = 2;
        public float[] LineDash { get; set; } = new float[] { };
        public CPoint HeadPoint { get; set; }
        public CPoint TailPoint { get; set; }
        public C_LINE_TYPE HeadLineType { get; set; } = C_LINE_TYPE.LINETYPE_ARROW;
        public C_LINE_TYPE TailLineType { get; set; } = C_LINE_TYPE.LINETYPE_ARROW;
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 14;
        public byte[] FontColor { get; set; } = new byte[] { 255, 0, 0 };
        public bool IsBold { get; set; } = false;
        public bool IsItalic { get; set; } = false;
        public C_BORDER_STYLE BorderStyle { get; set; } = C_BORDER_STYLE.BS_SOLID;
        public CPDFMeasureInfo measureInfo { get; set; }
            = new CPDFMeasureInfo
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
            LineMeasureParam polygonTransfer = transfer as LineMeasureParam;
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
                    RulerTranslate = measureInfo.RulerTranslate,
                    CaptionType = measureInfo.CaptionType,
                };
                polygonTransfer.measureInfo = cPDFMeasureInfo;
            }

            polygonTransfer.LineWidth = LineWidth;
            polygonTransfer.HeadLineType = HeadLineType;
            polygonTransfer.TailLineType = TailLineType;
            polygonTransfer.FontName = FontName;
            polygonTransfer.FontSize = FontSize;
            polygonTransfer.IsBold = IsBold;
            polygonTransfer.IsItalic = IsItalic;
            polygonTransfer.HeadPoint = HeadPoint;
            polygonTransfer.TailPoint = TailPoint;
            polygonTransfer.BorderStyle = BorderStyle;

            return true;
        }
    }
}
