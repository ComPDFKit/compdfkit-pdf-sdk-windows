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
        }
        public byte[] LineColor { get; set; }
        public float LineWidth { get; set; }
        public float[] LineDash { get; set; }
        public CPoint HeadPoint { get; set; }
        public CPoint TailPoint { get; set; }
        public C_LINE_TYPE HeadLineType { get; set; }
        public C_LINE_TYPE TailLineType { get; set; }
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public byte[] FontColor { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public CPDFMeasureInfo measureInfo { get; set; }

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
                    RulerTranslate= measureInfo.RulerTranslate,
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
