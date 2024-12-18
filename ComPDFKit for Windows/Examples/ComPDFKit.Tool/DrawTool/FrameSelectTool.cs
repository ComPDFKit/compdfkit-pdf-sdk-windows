using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer.Helper;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    internal class FrameSelectTool : CustomizeLayer
    {

        #region Attributes

        /// <summary>
        /// Indicates whether proportional scaling is required
        /// </summary>
        protected bool isProportionalScaling { get; set; } = false;

        /// <summary>
        /// Mouse start point
        /// </summary>
        protected Point mouseStartPoint { get; set; }

        /// <summary>
        /// Mouse end point
        /// </summary>
        protected Point mouseEndPoint { get; set; }

        /// <summary>
        /// Crop point
        /// </summary>
        protected Point cropPoint { get; set; }

        /// <summary>
        ///Is drawing annotation
        /// </summary>
        protected bool isFrameSelect { get; set; }

        /// <summary>   
        /// Current zoom factor
        /// </summary>
        private double zoomFactor { get; set; } = 1;

        /// <summary>
        /// Draw rectangle
        /// </summary>
        protected Rect drawRect { get; set; } = new Rect();

        /// <summary> 
        /// The rectangle representing the maximum drawing area
        /// </summary>
        protected Rect maxRect { get; set; }

        /// <summary> 
        /// The rectangle representing the original page range (calculated offset in continuous mode)
        /// </summary>
        protected Rect pageBound { get; set; }

        /// <summary>
        /// The rectangle at standard DPI (without subtracting half of the pen thickness)
        /// </summary>
        protected Rect DPIRect { get; set; }

        /// <summary>
        /// The offset value during movement
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// The offset value during movement
        /// </summary>
        protected int pageIndex { get; set; } = -1;

        protected DrawingContext drawDC { get; set; }

        /// <summary>
        /// The collection of points measured for annotation drawing
        /// </summary>
        protected PointCollection drawPoints { get; set; } = new PointCollection();

        protected double textPadding { get; set; } = 10;

        protected Border lastTextBorder;

        #endregion

        public FrameSelectTool()
        {
        }

        public Point GetStartPoint()
        {
            return DpiHelper.StandardPointToPDFPoint(new Point((mouseStartPoint.X - pageBound.X + (cropPoint.X * zoomFactor)) / zoomFactor, (mouseStartPoint.Y - pageBound.Y + (cropPoint.Y * zoomFactor)) / zoomFactor));
        }

        public Point GetEndPoint()
        {
            if (moveOffset == new Point())
            {
                return new Point(-1, -1);
            }
            else
            {
                return DpiHelper.StandardPointToPDFPoint(new Point((mouseEndPoint.X - pageBound.X + (cropPoint.X * zoomFactor)) / zoomFactor, (mouseEndPoint.Y - pageBound.Y + (cropPoint.Y * zoomFactor)) / zoomFactor));
            }
        }

        public double GetMoveLength()
        {
            if (mouseEndPoint == new Point())
            {
                return 0;
            }
            Point checkPoint = mouseEndPoint;
            checkPoint.X = Math.Max(pageBound.Left, checkPoint.X);
            checkPoint.X = Math.Min(pageBound.Right, checkPoint.X);
            checkPoint.Y = Math.Max(pageBound.Top, checkPoint.Y);
            checkPoint.Y = Math.Min(pageBound.Bottom, checkPoint.Y);

            Vector moveOffset = checkPoint - mouseStartPoint;
            return moveOffset.Length;
        }

        public void SetIsProportionalScaling(bool isOpen)
        {
            isProportionalScaling = isOpen;
        }

        #region Draw
        public void StartDraw(Point downPoint, Rect maxRect, Rect pageBound, double zoom, int pageindex)
        {
            this.pageBound = pageBound;
            pageIndex = pageindex;
            isFrameSelect = true;
            drawRect = new Rect();
            DPIRect = new Rect();
            mouseStartPoint = downPoint;
            isFrameSelect = true;
            this.maxRect = maxRect;
            zoomFactor = zoom;
            moveOffset = new Point();
            DPIRect = new Rect();
        }
        bool noDraw=false;
        public void MoveDraw(Point downPoint, double zoom)
        {
            if (isFrameSelect)
            {
                noDraw = true;
                moveOffset = new Point(
                    mouseEndPoint.X - downPoint.X,
                    mouseEndPoint.Y - downPoint.Y
                    );
                mouseEndPoint = downPoint;
                zoomFactor = zoom;
                Draw();
            }
            noDraw = false;
        }

        public Rect EndDraw(ref int index)
        {
            if (noDraw && isFrameSelect)
            {
                new Rect();
            }
            if (!DPIRect.Equals(new Rect()))
            {
                Rect StandardRect = new Rect(
                    (DPIRect.Left - pageBound.X + (cropPoint.X * zoomFactor)) / zoomFactor, (DPIRect.Top - pageBound.Y + (cropPoint.Y * zoomFactor)) / zoomFactor,
                    DPIRect.Width / zoomFactor, DPIRect.Height / zoomFactor);
                isFrameSelect = false;
                noDraw = false;
                mouseStartPoint = new Point();
                mouseEndPoint = new Point();
                moveOffset = new Point();
                pageBound = new Rect();
                DPIRect = new Rect();
                drawPoints.Clear();
                ClearDraw();
                index = pageIndex;
                return DpiHelper.StandardRectToPDFRect(StandardRect);
            }
            isFrameSelect = false;
            noDraw = false;
            return new Rect();
        }

        public override void Draw()
        {
            if (noDraw&& isFrameSelect)
            {
                Dispatcher.Invoke(() =>
            {

                drawDC = Open();
                DrawSquare(drawDC);
                Present();
            });
            }
        }

        public virtual void ClearDraw()
        {
            Open();
            Present();
        }

        private void DrawSquare(DrawingContext drawingContext)
        {
            try
            {
                Pen DrawPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);

                SolidColorBrush FillBrush = new SolidColorBrush(Colors.Transparent);
                FillBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

                if (isProportionalScaling)
                {
                    Point mouseOffset = (Point)(mouseStartPoint - mouseEndPoint);
                    if (mouseOffset.X < 0)
                    {
                        if (mouseOffset.Y > 0)
                        {
                            mouseEndPoint = new Point(mouseEndPoint.X, mouseStartPoint.Y + mouseStartPoint.X - mouseEndPoint.X);
                        }
                        else
                        {
                            mouseEndPoint = new Point(mouseEndPoint.X, mouseStartPoint.Y + mouseEndPoint.X - mouseStartPoint.X);
                        }
                    }
                    else
                    {
                        if (mouseOffset.Y > 0)
                        {
                            mouseEndPoint = new Point(mouseEndPoint.X, mouseStartPoint.Y + mouseEndPoint.X - mouseStartPoint.X);
                        }
                        else
                        {
                            mouseEndPoint = new Point(mouseEndPoint.X, mouseStartPoint.Y + mouseStartPoint.X - mouseEndPoint.X);
                        }
                    }
                }

                Rect rect = new Rect(mouseStartPoint, mouseEndPoint);

                double mLeft = rect.Left;
                double mRight = rect.Right;
                double mUp = rect.Top;
                double mDown = rect.Bottom;
                if (rect.Left < maxRect.Left)
                {
                    mLeft = maxRect.Left;
                }
                if (rect.Right > maxRect.Right)
                {
                    mRight = maxRect.Right;
                }
                if (rect.Top < maxRect.Top)
                {
                    mUp = maxRect.Top;
                }
                if (rect.Bottom > maxRect.Bottom)
                {
                    mDown = maxRect.Bottom;
                }
                DPIRect = new Rect(mLeft, mUp, mRight - mLeft, mDown - mUp);
                int halfPenWidth = (int)Math.Ceiling(DrawPen.Thickness / 2);
                double drawWidth = DPIRect.Width - halfPenWidth * 2;
                double drawHeight = DPIRect.Height - halfPenWidth * 2;
                if (drawWidth > 0 && drawHeight > 0)
                {
                    drawRect = new Rect(
                        (int)DPIRect.Left + halfPenWidth,
                        (int)DPIRect.Top + halfPenWidth,
                        (int)DPIRect.Width - halfPenWidth * 2,
                        (int)DPIRect.Height - halfPenWidth * 2);
                    drawingContext?.DrawRectangle(null, DrawPen, drawRect);
                    halfPenWidth = (int)Math.Floor(DrawPen.Thickness / 2);
                    if (drawRect.Width - halfPenWidth * 2 > 0 && drawRect.Height - halfPenWidth * 2 > 0)
                    {
                        Rect innerRect = new Rect(drawRect.Left + halfPenWidth, drawRect.Top + halfPenWidth, drawRect.Width - 2 * halfPenWidth, drawRect.Height - 2 * halfPenWidth);

                        drawingContext?.DrawRectangle(FillBrush, null, innerRect);
                    }
                }
                else
                {
                    drawRect = new Rect();
                }
            }
            catch { }
        }

        public Rect GetMaxRect()
        {
            return maxRect;
        }

        /// <summary>
        /// Use to calculate the point drawn at a fixed angle
        /// </summary>
        /// <param name="currentPoint">
        /// Current point
        /// </param>
        /// <param name="startPoint">
        /// Start point
        /// </param>
        /// <param name="pageBound">
        /// Maximum drawing area
        /// </param>
        /// <returns>
        /// Return the calculated point
        /// </returns>
        internal Point CalcAnglePoint(Point currentPoint, Point startPoint, Rect pageBound)
        {
            Vector angleVector = currentPoint - startPoint;
            Point originPoint = new Point(startPoint.X, startPoint.Y - angleVector.Length);
            Vector orignVector = originPoint - startPoint;
            Rect checkRect = pageBound;
            int angle = (int)Vector.AngleBetween(orignVector, angleVector);
            if (angle < 0)
            {
                angle += 360;
            }
            int mod = angle % 45;
            int quot = angle / 45;
            Point anglePoint = currentPoint;

            int rotateAngle = 0;
            if (mod < 22)
            {
                Matrix rotateMatrix = new Matrix();
                rotateAngle = quot * 45;
                rotateMatrix.RotateAt(rotateAngle, startPoint.X, startPoint.Y);
                anglePoint = rotateMatrix.Transform(originPoint);
                anglePoint = new Point((int)anglePoint.X, (int)anglePoint.Y);
            }
            else
            {
                Matrix rotateMatrix = new Matrix();
                rotateAngle = (quot + 1) * 45;
                rotateMatrix.RotateAt(rotateAngle, startPoint.X, startPoint.Y);
                anglePoint = rotateMatrix.Transform(originPoint);
                anglePoint = new Point((int)anglePoint.X, (int)anglePoint.Y);
            }


            if (checkRect.Contains(anglePoint) == false)
            {
                switch (rotateAngle)
                {
                    case 0:
                        {
                            anglePoint.X = startPoint.X;
                            anglePoint.Y = Math.Max(checkRect.Top, Math.Min(anglePoint.Y, startPoint.Y));
                        }
                        break;
                    case 45:
                        {
                            double addValue = Math.Min(anglePoint.X - startPoint.X, checkRect.Right - startPoint.X);
                            addValue = Math.Min(addValue, startPoint.Y - checkRect.Top);
                            anglePoint.X = startPoint.X + addValue;
                            anglePoint.Y = startPoint.Y - addValue;
                        }
                        break;
                    case 90:
                        {
                            anglePoint.X = startPoint.X + Math.Min(anglePoint.X - startPoint.X, checkRect.Right - startPoint.X);
                            anglePoint.Y = startPoint.Y;
                        }
                        break;
                    case 135:
                        {
                            double addValue = Math.Min(anglePoint.X - startPoint.X, checkRect.Right - startPoint.X);
                            addValue = Math.Min(addValue, checkRect.Bottom - startPoint.Y);
                            anglePoint.X = startPoint.X + addValue;
                            anglePoint.Y = startPoint.Y + addValue;
                        }
                        break;
                    case 180:
                        {
                            anglePoint.X = startPoint.X;
                            anglePoint.Y = Math.Min(anglePoint.Y, checkRect.Bottom);
                        }
                        break;
                    case 225:
                        {
                            double addValue = Math.Min(startPoint.X - anglePoint.X, startPoint.X - checkRect.Left);
                            addValue = Math.Min(addValue, checkRect.Bottom - startPoint.Y);
                            anglePoint.X = startPoint.X - addValue;
                            anglePoint.Y = startPoint.Y + addValue;
                        }
                        break;
                    case 270:
                        {
                            anglePoint.X = startPoint.X - Math.Min(startPoint.X - anglePoint.X, startPoint.X - checkRect.Left);
                            anglePoint.Y = startPoint.Y;
                        }
                        break;
                    case 315:
                        {
                            double addValue = Math.Min(startPoint.X - anglePoint.X, startPoint.X - checkRect.Left);
                            addValue = Math.Min(addValue, startPoint.Y - checkRect.Top);
                            anglePoint.X = startPoint.X - addValue;
                            anglePoint.Y = startPoint.Y - addValue;
                        }
                        break;
                    case 360:
                        {
                            anglePoint.X = startPoint.X;
                            anglePoint.Y = Math.Max(checkRect.Top, Math.Min(anglePoint.Y, startPoint.Y));
                        }
                        break;
                    default:
                        break;
                }
            }
            return anglePoint;
        }
    }
    #endregion
}