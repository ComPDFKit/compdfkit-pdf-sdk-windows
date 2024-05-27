using ComPDFKit.Import;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComPDFKit.Tool.DrawTool
{
    /// <summary>
    /// Use to draw the cursor and selected effect of text editing
    /// </summary>
    internal class CaretVisual : DrawingVisual
    {
        #region Attributes

        private Timer caretTimer;

        protected DefaultDrawParam drawParam = new DefaultDrawParam();

        private bool lastShow = true;

        private Point caretHeight = new Point(0, 0);

        private Point cursorPoint = new Point(0, 0);

        private double currentZoom = 1;

        private Rect paintOffset { get; set; }

        /// <summary>
        /// Selected text's collection of drawn rectangles.
        /// </summary>
        List<Rect> selectRects { get; set; } = new List<Rect>();

        #endregion

        #region  Timer

        public void StartTimer()
        {
            if (caretTimer.Enabled == false)
            {
                caretTimer.Start();
            }
        }

        public void StopTimer()
        {
            caretTimer.Stop();
            cursorPoint = new Point(0, 0);
            caretHeight = new Point(0, 0);
            currentZoom = 0;
            paintOffset = new Rect();
        }

        private void CaretTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if ((cursorPoint - caretHeight).Length > 0)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Draw(lastShow);
                });
            }
        }

        #endregion

        /// <summary>
        /// Re-locate the child elements
        /// </summary>
        public void Arrange()
        {
            foreach (Visual child in Children)
            {
                if (!(child is UIElement))
                {
                    continue;
                }
                UIElement checkChild = child as UIElement;
                try
                {
                    double left = Canvas.GetLeft(checkChild);
                    double top = Canvas.GetTop(checkChild);
                    double width = (double)checkChild.GetValue(FrameworkElement.WidthProperty);
                    double height = (double)checkChild.GetValue(FrameworkElement.HeightProperty);
                    checkChild.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    checkChild.Arrange(new Rect(
                        double.IsNaN(left) ? 0 : left,
                        double.IsNaN(top) ? 0 : top,
                        double.IsNaN(width) ? checkChild.DesiredSize.Width : width,
                        double.IsNaN(height) ? checkChild.DesiredSize.Height : height));
                }
                catch (Exception ex)
                {

                }
            }
        }

        public CaretVisual(DefaultDrawParam defaultDrawParam)
        {
            caretTimer = new Timer(500);
            caretTimer.Elapsed += CaretTimerElapsed;
            caretTimer.AutoReset = true;
            drawParam = defaultDrawParam;
        }

        /// <summary>
        /// Set the selected text effect area in text editing
        /// </summary>
        /// <param name="SelectLineRects">
        /// Rect of PDF
        /// </param>
        public void SetSelectRect(List<Rect> SelectLineRects)
        {
            selectRects.Clear();
            for (int i = 0; i < SelectLineRects.Count; i++)
            {
                selectRects.Add(DpiHelper.PDFRectToStandardRect(SelectLineRects[i]));
            }
        }

        public void SetZoom(double Zoom)
        {
            currentZoom = Zoom;
        }

        /// <summary>
        /// Set the current page drawing area (standard DPI)
        /// </summary>
        /// <param name="paintOffset">
        /// The current page drawing area (standard DPI)
        /// </param>
        public void SetPaintOffset(Rect paintOffset)
        {
            this.paintOffset = paintOffset;
        }

        /// <summary>
        /// Set the current cursor position
        /// </summary>
        /// <param name="EditArea">
        /// Data object being edited
        /// </param>
        /// <param name="Zoom">
        /// Current zoom factor
        /// </param>
        /// <param name="paintRect">
        /// Current text box drawing area (standard DPI)
        /// </param>
        /// <param name="paintOffset">
        /// Current page drawing area (standard DPI)
        /// </param>
        /// <returns>
        /// Set error return false
        /// </returns>
        public bool SetCaretVisualArea(CPDFEditArea EditArea, double Zoom, Rect paintOffset, Point mousePoint)
        {
            if (EditArea.Type == CPDFEditType.EditText)
            {
                cursorPoint = new Point(0, 0);
                // Mouse coordinates on the current page (PDF).
                Point pagePoint = new Point(((mousePoint.X - paintOffset.X) / Zoom),
                    ((mousePoint.Y - paintOffset.Y) / Zoom));

                // Call the SDK interface to set the coordinates and get the current cursor position and height
                pagePoint = DpiHelper.StandardPointToPDFPoint(pagePoint);
                (EditArea as CPDFEditTextArea).SelectCharItemAtPos(DataConversionForWPF.PointConversionForCPoint(pagePoint));

                CPoint caretCPoint = new CPoint(0, 0);
                CPoint HighCpoint = new CPoint(0, 0);
                (EditArea as CPDFEditTextArea).GetTextCursorPoints(ref caretCPoint, ref HighCpoint);

                Point caretPoint = DataConversionForWPF.CPointConversionForPoint(caretCPoint);
                Point pointHigh = DataConversionForWPF.CPointConversionForPoint(HighCpoint); 
                // Converting SDK return values into data required for drawing
                //CaretHeight = DpiHelper.PDFNumToStandardNum((caretPoint - pointHigh).Length);
                //Rect caretRect = new Rect(caretPoint, pointHigh);
                caretHeight = DpiHelper.PDFPointToStandardPoint(pointHigh);
                cursorPoint = DpiHelper.PDFPointToStandardPoint(caretPoint);
                currentZoom = Zoom;
                this.paintOffset = paintOffset;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the current cursor position
        /// </summary>
        /// <param name="paintOffset">
        /// The lowPoint obtained from the SDK 
        /// </param>
        /// <param name="mousePoint">
        ///The lowPoint obtained from the SDK.
        /// </param>
        /// <returns>
        /// Set error return false
        /// </returns>
        public bool SetCaretVisualArea(Point lowPoint, Point highPoint)
        {
            caretHeight = DpiHelper.PDFPointToStandardPoint(lowPoint);
            cursorPoint = DpiHelper.PDFPointToStandardPoint(highPoint);
            return true;
        }

        /// <summary>
        /// The low point of the cursor (96 DPI). 
        /// </summary>
        /// <returns>
        /// The low point of the cursor (96 DPI).
        /// </returns>
        public Point GetCaretLowPoint()
        {
            return cursorPoint;
        }

        /// <summary> 
        /// The high point of the cursor (96 DPI).
        /// </summary>
        /// <returns>
        /// The high point of the cursor (96 DPI).
        /// </returns>
        public Point GetCaretHighPoint()
        {
            return caretHeight;
        }

        /// <summary>
        /// Draw method.
        /// </summary>
        /// <param name="isLastShow">
        ///  Indicates whether to immediately refresh
        /// </param>
        /// <param name="isShowCaret"> 
        /// Indicates whether to draw the cursor.
        /// </param>
        public void Draw(bool isLastShow, bool isShowCaret = true)
        {
            using (DrawingContext dc = RenderOpen())
            {
                RectangleGeometry clipGeometry = new RectangleGeometry();

                Point CaretPos = new Point(
                   cursorPoint.X * currentZoom + paintOffset.X,
                    cursorPoint.Y * currentZoom + paintOffset.Y);

                Point CaretHeight = new Point(
                   caretHeight.X * currentZoom + paintOffset.X,
                    caretHeight.Y * currentZoom + paintOffset.Y);
                if (isLastShow)
                {
                    if (isShowCaret)
                    {
                        dc.DrawLine(drawParam.CaretPen, CaretPos, CaretHeight);
                    }
                }
                foreach (Rect selectRect in selectRects)
                {
                    Rect paintRect = new Rect(
                        (int)(selectRect.X * currentZoom + paintOffset.X),
                        (int)(selectRect.Y * currentZoom + paintOffset.Y),
                        (int)(selectRect.Width * currentZoom),
                        (int)(selectRect.Height * currentZoom));

                    dc?.DrawRectangle(drawParam.CaretBrush, null, paintRect);
                }
                dc.Close();
            }
            lastShow = !isLastShow;
        }

        public void CleanDraw()
        {
            DrawingContext dc = RenderOpen();
            dc.Close();
        }

        public void CleanSelectRectDraw()
        {
            selectRects.Clear();
            Draw(true);
        }

        public void StopCaret()
        {
            caretTimer.Stop();
        }
    }
}
