using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class InkParam:AnnotParam
    {
        public InkParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_INK;
        }

        public byte[] InkColor { get; set; }
        public double Thickness { get; set; }
        public List<List<CPoint>> InkPath { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            InkParam inkTransfer = transfer as InkParam;
            if (inkTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(inkTransfer))
            {
                return false;
            }

            if(InkColor != null)
            {
                inkTransfer.InkColor = (byte[])InkColor.Clone();
            }
            
            inkTransfer.Thickness = Thickness;

            if(InkPath != null)
            {
                List<List<CPoint>> inkPoints = new List<List<CPoint>>();
                foreach (List<CPoint> points in InkPath)
                {
                    List<CPoint> pointList = new List<CPoint>();
                    foreach (CPoint point in points)
                    {
                        pointList.Add(point);
                    }
                    inkPoints.Add(pointList);
                }

                inkTransfer.InkPath = inkPoints;
            }

            return true;
        }
    }
}
