using ComPDFKitViewer.Annot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComPDFKit.Tool.DrawTool
{
    internal class DashedBorder : Border
    {
        private bool DrawDash {  get; set; }
        private double DashThickness {  get; set; }
        private double DashWidth { get; set; }
        private DoubleCollection DashArray { get; set; }
        protected override void OnRender(DrawingContext dc)
        {
            if (DrawDash)
            {
                Pen dashPen = new Pen(BorderBrush, DashThickness);
                dashPen.DashCap = PenLineCap.Flat;
                DashStyle dash = new DashStyle();
                foreach (double offset in DashArray)
                {
                    dash.Dashes.Add(offset / DashWidth);
                }
                dashPen.DashStyle = dash;
                dc.DrawRectangle(null, dashPen, new Rect(0, 0, ActualWidth, ActualHeight));
                return;
            }
            base.OnRender(dc);
        }
        public void DrawDashBorder(bool isDash,double dashWidth,double rawWidth, DoubleCollection dashArray)
        {
            DrawDash = isDash;
            DashArray = dashArray;
            DashThickness=dashWidth;
            DashWidth=rawWidth;
            InvalidateVisual();
        }
    }
}
