using ComPDFKit.Import;
using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public class PolygonMeasureParam : AnnotParam
    {
        public PolygonMeasureParam() 
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON;
        }    
        public byte[] FillColor { get; set; }
        public bool HasFillColor { get; set; }
        public byte[] LineColor { get; set; }
        public float LineWidth { get; set; }
        public float[] LineDash { get; set; }
        public List<CPoint> SavePoints { get; set; } 
        public byte[] EndLineColor { get; set; }
        public double EndLineWidth { get; set; }
        public double EndTransparency { get; set; }
        public DashStyle EndLineDash { get; set; }
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public byte[] FontColor { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public C_BORDER_STYLE BorderStyle { get; set; }
        public CPDFMeasureInfo measureInfo { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            PolygonMeasureParam polygonTransfer = transfer as PolygonMeasureParam;
            if (polygonTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(polygonTransfer))
            {
                return false;
            }

            if (FillColor != null)
            {
                polygonTransfer.FillColor = (byte[])FillColor.Clone();
            }

            if (SavePoints != null)
            {
                polygonTransfer.SavePoints.CopyTo(SavePoints.ToArray());
            }

            if (LineColor != null)
            {
                polygonTransfer.LineColor = (byte[])LineColor.Clone();
            }

            if (LineDash != null)
            {
                polygonTransfer.LineDash = (float[])LineDash.Clone(); 
            }

            if (EndLineColor != null)
            {
                polygonTransfer.EndLineColor = (byte[])EndLineColor.Clone();
            }

            if (EndLineDash != null)
            {
                polygonTransfer.EndLineDash = EndLineDash.Clone();
            }

            if (FontColor != null)
            {
                polygonTransfer.FontColor = (byte[])FontColor.Clone();
            }

            if (measureInfo != null)
            {
                CPDFMeasureInfo cPDFMeasureInfo =new CPDFMeasureInfo() 
                {
                    Factor=measureInfo.Factor,
                    Unit=measureInfo.Unit,
                    DecimalSymbol=measureInfo.DecimalSymbol,
                    ThousandSymbol=measureInfo.ThousandSymbol,
                    Display=measureInfo.Display,
                    Precision=measureInfo.Precision,
                    UnitPrefix=measureInfo.UnitPrefix,
                    UnitSuffix=measureInfo.UnitSuffix,
                    UnitPosition=measureInfo.UnitPosition,
                    RulerBase=measureInfo.RulerBase,
                    RulerBaseUnit=measureInfo.RulerBaseUnit,
                    RulerTranslateUnit=measureInfo.RulerTranslateUnit,
                    CaptionType=measureInfo.CaptionType,
                    RulerTranslate=measureInfo.RulerTranslate,
                };
                polygonTransfer.measureInfo = cPDFMeasureInfo;
            }

            polygonTransfer.HasFillColor = HasFillColor;
            polygonTransfer.LineWidth = LineWidth;
            polygonTransfer.EndLineWidth = EndLineWidth;
            polygonTransfer.EndTransparency = EndTransparency;
            polygonTransfer.FontName = FontName;
            polygonTransfer.FontSize = FontSize;
            polygonTransfer.IsBold = IsBold;
            polygonTransfer.IsItalic = IsItalic;
            polygonTransfer.BorderStyle = BorderStyle;

            return true;
        }
    }
}
