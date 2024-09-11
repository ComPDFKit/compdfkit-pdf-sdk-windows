using ComPDFKit.Tool.SettingParam;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public class PageSelectedRect : DrawingVisual
    {
        /// <summary>
        /// A method that repositions child elements
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
        protected DefaultDrawParam DrawParam = new DefaultDrawParam();

        protected DrawingContext drawDC { get; set; }

        /// <summary>
        /// Data changing event
        /// </summary>
        public event EventHandler<Rect> DataChanging;

        /// <summary>
        /// Data changed event
        /// </summary>
        public event EventHandler<Rect> DataChanged;

        /// <summary>
        /// Minimum width of the rectangle
        /// </summary>
        protected int rectMinWidth { get; set; } = 10;

        /// <summary>
        /// Minimum height of the rectangle
        /// </summary>
        protected int rectMinHeight { get; set; } = 10;

        /// <summary>
        /// Identifies whether the mouse is pressed
        /// </summary>
        protected bool isMouseDown { get; set; }

        /// <summary>
        /// Current click hit control point
        /// </summary>
        protected PointControlType hitControlType { get; set; }

        /// <summary>
        /// Location information recorded when the mouse is pressed
        /// </summary>
        protected Point mouseDownPoint { get; set; }

        /// <summary>
        /// Current set ignore point
        /// </summary>
        protected List<PointControlType> ignorePoints { get; set; } = new List<PointControlType>();

        /// <summary>
        /// Current control point coordinates
        /// </summary>
        protected List<Point> controlPoints { get; set; } = new List<Point>();

        /// <summary>
        /// Move offset during movement
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// Current PDFVIewer actual display width
        /// </summary>
        protected double PDFViewerActualWidth { get; set; } = 0;

        /// <summary>
        /// Current PDFVIewer actual display height
        /// </summary>
        protected double PDFViewerActualHeight { get; set; } = 0;

        /// <summary>
        /// Current control point drawing style
        /// </summary>
        protected DrawPointType currentDrawPointType { get; set; }

        /// <summary>
        /// Current drag drawing style
        /// </summary>
        protected DrawMoveType currentDrawMoveType { get; set; }

        /// <summary>
        /// Control point size
        /// </summary>
        protected int pointSize { get; set; } = 4;

        /// <summary>
        /// Current drawing rectangle (calculated during operation)
        /// </summary>
        protected Rect drawRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Draw border and content internal padding
        /// </summary>
        protected double rectPadding = 5;

        /// <summary>
        /// Cache the rectangle when the mouse is pressed
        /// </summary>
        protected Rect cacheRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Current set drawing rectangle (original data)
        /// </summary>
        protected Rect setDrawRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Maximum drawable range
        /// </summary>
        protected Rect maxRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Mouse start drawing point
        /// </summary>
        protected Point mouseStartPoint { get; set; }

        /// <summary>
        /// Mouse end drawing point
        /// </summary>
        protected Point mouseEndPoint { get; set; }

        /// <summary>
        /// Identifies whether proportional scaling is required
        /// </summary>
        protected bool isProportionalScaling { get; set; } = false;

        /// <summary>
        /// Identifies whether to draw a hover state
        /// </summary>
        protected bool isHover = false;

        /// <summary>
        /// Identifies whether to draw a selected state
        /// </summary>
        protected bool isSelected = false;

        protected bool isDrawCreateSelected = false;

        public PageSelectedRect(DefaultDrawParam defaultDrawParam)
        {
            DrawParam = defaultDrawParam;
            currentDrawPointType = DrawPointType.Square;
        }

        public void SetIsHover(bool hover)
        {
            isHover = hover;
        }

        public bool GetIsHover()
        {
            return isHover;
        }

        public void SetIsSelected(bool selected)
        {
            isSelected = selected;
        }

        public bool GetIsSelected()
        {
            return isSelected;
        }

        /// <summary>
        /// Get the original set Rect, not the calculated filled
        /// </summary>
        /// <param name="newRect">
        /// The new rectangle to be set
        /// </param>
        public Rect GetRect()
        {
            Rect rect = new Rect(drawRect.X + rectPadding, drawRect.Y + rectPadding, Math.Max(0, drawRect.Width - 2 * rectPadding), Math.Max(0, drawRect.Height - 2 * rectPadding));
            return rect;
        }

        public void SetRect(Rect newRect)
        {
            newRect = new Rect(newRect.X - rectPadding, newRect.Y - rectPadding, newRect.Width + 2 * rectPadding, newRect.Height + 2 * rectPadding);
            setDrawRect = drawRect = newRect;
        }

        public void SetMaxRect(Rect rect)
        {
            maxRect = rect;
        }

        public Rect GetMaxRect()
        {
            return maxRect;
        }


        public void SetDrawMoveType(DrawMoveType drawType)
        {
            currentDrawMoveType = drawType;
        }

        public DrawMoveType GetDrawMoveType()
        {
            return currentDrawMoveType;
        }

        public void SetPDFViewerActualSize(double width, double height)
        {
            PDFViewerActualWidth = width;
            PDFViewerActualHeight = height;
        }

        public virtual void OnMouseLeftButtonDown(Point downPoint)
        {
            isMouseDown = true;
            hitControlType = PointControlType.None;
            mouseDownPoint = downPoint;
            moveOffset = new Point(0, 0);
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, downPoint);
            if (hitResult != null && hitResult.VisualHit is DrawingVisual)
            {
                hitControlType = GetHitControlIndex(downPoint);
                if (hitControlType != PointControlType.None)
                {
                    cacheRect = drawRect;
                }
            }
        }

        public virtual void OnMouseLeftButtonUp(Point upPoint)
        {
            if (isDrawCreateSelected)
            {
                Draw();
                if ((int)upPoint.X != (int)mouseDownPoint.X || (int)upPoint.Y != (int)mouseDownPoint.Y)
                {
                    InvokeDataChangEvent(true);
                }
            }
            else
            {
                if (isMouseDown && hitControlType != PointControlType.None)
                {
                    isMouseDown = false;
                    cacheRect = setDrawRect = drawRect;
                    Draw();
                    if ((int)upPoint.X != (int)mouseDownPoint.X || (int)upPoint.Y != (int)mouseDownPoint.Y)
                    {
                        InvokeDataChangEvent(true);
                    }
                }
                moveOffset = new Point(0, 0);
            }
            isDrawCreateSelected = false;
        }

        public virtual void OnMouseMove(Point mousePoint, out bool Tag, double width, double height)
        {
            Tag = false;
            Tag = isMouseDown;
            SetPDFViewerActualSize(width, height);
            if (isDrawCreateSelected)
            {
                mouseEndPoint = mousePoint;
                Draw();
            }
            else
            {
                if (isMouseDown && hitControlType != PointControlType.None)
                {
                    if (CalcHitPointMove(mousePoint))
                    {
                        Draw();
                        if ((int)mousePoint.X != (int)mouseDownPoint.X || (int)mousePoint.Y != (int)mouseDownPoint.Y)
                        {
                            InvokeDataChangEvent(false);
                        }
                    }
                }
            }
        }

        public void CreateRect(Point downPoint, Point cropPoint, Rect maxRect, double width, double height)
        {
            mouseEndPoint = mouseStartPoint = downPoint;
            this.maxRect = maxRect;
            PDFViewerActualWidth = width;
            PDFViewerActualHeight = height;
            isDrawCreateSelected = true;
        }

        public void Draw()
        {
            if (isDrawCreateSelected)
            {
                CreateRect();
            }
            else
            {
                EditRect();
            }
        }

        private void CreateRect()
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = RenderOpen();
                SolidColorBrush bgsolidColorBrush = DrawParam.PageSelectedBgBrush;
                Pen bgpen = DrawParam.PageSelectedRectLinePen;
                CombinedGeometry clipGeometry = new CombinedGeometry();
                clipGeometry.Geometry1 = new RectangleGeometry() { Rect = maxRect };
                clipGeometry.Geometry2 = new RectangleGeometry() { Rect = drawRect };
                clipGeometry.GeometryCombineMode = GeometryCombineMode.Exclude;
                drawDC?.DrawGeometry(bgsolidColorBrush, bgpen, clipGeometry);


                SolidColorBrush solidColorBrush = DrawParam.PageSelectedRectFillBrush;
                Pen pen = DrawParam.PageSelectedRectLinePen;

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
                Rect DPIRect = new Rect(mLeft, mUp, mRight - mLeft, mDown - mUp);
                int halfPenWidth = (int)Math.Ceiling(pen.Thickness / 2);
                double drawWidth = DPIRect.Width - halfPenWidth * 2;
                double drawHeight = DPIRect.Height - halfPenWidth * 2;

                if (drawWidth > 0 && drawHeight > 0)
                {
                    drawRect = new Rect(
                        (int)DPIRect.Left + halfPenWidth,
                        (int)DPIRect.Top + halfPenWidth,
                        (int)DPIRect.Width - halfPenWidth * 2,
                        (int)DPIRect.Height - halfPenWidth * 2);
                    drawDC?.DrawRectangle(solidColorBrush, pen, drawRect);


                    CalcControlPoint(drawRect);
                    SolidColorBrush PointBrush = DrawParam.PageSelectedPointBorderBrush;
                    Pen PointPen = DrawParam.PageSelectedPointPen;
                    GetPointBrushAndPen(ref PointBrush, ref PointPen);
                    switch (currentDrawPointType)
                    {
                        case DrawPointType.Circle:
                            DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                            break;
                        case DrawPointType.Square:
                            DrawSquarePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                            break;
                    }
                }
                drawDC?.Close();
                drawDC = null;
            });
        }

        private void EditRect()
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = RenderOpen();

                SolidColorBrush bgsolidColorBrush = DrawParam.PageSelectedBgBrush;
                Pen bgpen = DrawParam.PageSelectedRectLinePen;
                CombinedGeometry clipGeometry = new CombinedGeometry();
                clipGeometry.Geometry1 = new RectangleGeometry() { Rect = maxRect };
                clipGeometry.Geometry2 = new RectangleGeometry() { Rect = drawRect };
                clipGeometry.GeometryCombineMode = GeometryCombineMode.Exclude;
                drawDC?.DrawGeometry(bgsolidColorBrush, bgpen, clipGeometry);

                CalcControlPoint(drawRect);

                SolidColorBrush solidColorBrush = DrawParam.PageSelectedRectFillBrush;
                Pen pen = DrawParam.PageSelectedRectLinePen;

                SolidColorBrush PointBrush = DrawParam.PageSelectedPointBorderBrush;
                Pen PointPen = DrawParam.PageSelectedPointPen;
                GetPointBrushAndPen(ref PointBrush, ref PointPen);

                switch (currentDrawMoveType)
                {
                    case DrawMoveType.kDefault:
                        break;
                    case DrawMoveType.kReferenceLine:
                        if (isMouseDown == true)
                        {
                            SolidColorBrush moveBrush = DrawParam.PDFEditMultiMoveBrush;
                            Pen movepen = DrawParam.PDFEditMultiMovePen;
                            GetMoveBrushAndPen(ref moveBrush, ref movepen);
                            DrawMoveBounds(drawDC, hitControlType, movepen, moveBrush, drawRect);
                        }
                        drawDC?.DrawRectangle(solidColorBrush, pen, drawRect);
                        break;
                    default:
                        break;
                }
                switch (currentDrawPointType)
                {
                    case DrawPointType.Circle:
                        DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                        break;
                    case DrawPointType.Square:
                        DrawSquarePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                        break;
                }
                drawDC?.Close();
                drawDC = null;
            });
        }

        public virtual void ClearDraw()
        {
            setDrawRect = drawRect = new Rect();
            drawDC = RenderOpen();
            drawDC?.Close();
            drawDC = null;
        }


        private void GetMoveBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            colorBrush = DrawParam.PageSelectedMoveBrush;
            pen = DrawParam.PageSelectedMovePen;
        }

        private void GetBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            if (isHover)
            {
                colorBrush = DrawParam.PageSelectedRectFillHoverBrush;
                pen = DrawParam.PageSelectedRectLineHoverPen;
            }
            else
            {
                if (isSelected)
                {
                    colorBrush = DrawParam.SPageSelectedRectFillBrush;
                    pen = DrawParam.SPageSelectedRectLinePen;
                }
                else
                {
                    colorBrush = DrawParam.PageSelectedRectFillBrush;
                    pen = DrawParam.PageSelectedRectLinePen;
                }
            }
        }

        private void GetPointBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            if (isHover)
            {
                colorBrush = DrawParam.PageSelectedRectFillHoverBrush;
                pen = DrawParam.PageSelectedRectLineHoverPen;
            }
            else
            {
                if (isSelected)
                {
                    colorBrush = DrawParam.SPageSelectedPointBorderBrush;
                    pen = DrawParam.SPageSelectedPointPen;
                }
                else
                {
                    colorBrush = DrawParam.PageSelectedPointBorderBrush;
                    pen = DrawParam.PageSelectedPointHoverPen;
                }
            }
        }

        /// <summary>
        /// Inner drawing circle point
        /// </summary>
        /// <param name="drawingContext">
        /// Drawing context
        /// </param>
        /// <param name="ControlPoints">
        /// Dataset of ignored points. Collection of positions where points need to be drawn.
        /// </param>
        /// <param name="PointSize">
        /// Size of the point
        /// </param>
        /// <param name="PointPen">
        /// Point brush
        /// </param>
        /// <param name="BorderBrush">
        /// Border brush for drawing points
        /// </param>
        protected void DrawCirclePoint(DrawingContext drawingContext, List<PointControlType> ignoreList, int PointSize, Pen PointPen, SolidColorBrush BorderBrush)
        {
            GeometryGroup controlGroup = new GeometryGroup();
            controlGroup.FillRule = FillRule.Nonzero;

            List<Point> ignorePointsList = new List<Point>();
            // Get specific points
            foreach (PointControlType type in ignoreList)
            {
                if ((int)type < controlPoints.Count)
                {
                    ignorePointsList.Add(controlPoints[(int)type]);
                }
            }
            for (int i = 0; i < controlPoints.Count; i++)
            {
                Point controlPoint = controlPoints[i];

                if (ignorePointsList.Contains(controlPoint))
                {
                    continue;
                }
                EllipseGeometry circlPoint = new EllipseGeometry(controlPoint, PointSize, PointSize);
                controlGroup.Children.Add(circlPoint);
            }
            drawingContext?.DrawGeometry(BorderBrush, PointPen, controlGroup);
        }

        /// <summary>
        /// Inner drawing square
        /// </summary>
        /// <param name="drawingContext">
        /// Drawing context
        /// </param>
        /// <param name="ControlPoints">
        /// Collection of positions where points need to be drawn
        /// </param>
        /// <param name="PointSize">
        /// Collection of positions where points need to be drawn
        /// </param>
        /// <param name="PointPen">
        /// The brush for drawing points
        /// </param>
        /// <param name="BorderBrush">
        /// The brush for drawing the border of points
        /// </param>
        protected void DrawSquarePoint(DrawingContext drawingContext, List<PointControlType> ignoreList, int PointSize, Pen PointPen, SolidColorBrush BorderBrush)
        {
            GeometryGroup controlGroup = new GeometryGroup();
            controlGroup.FillRule = FillRule.Nonzero;
            List<Point> ignorePointsList = new List<Point>();
            // Get specific points
            foreach (PointControlType type in ignoreList)
            {
                if ((int)type < controlPoints.Count)
                {
                    ignorePointsList.Add(controlPoints[(int)type]);
                }
            }
            for (int i = 0; i < controlPoints.Count; i++)
            {
                Point controlPoint = controlPoints[i];
                if (ignorePointsList.Contains(controlPoint))
                {
                    continue;
                }
                RectangleGeometry rectPoint = new RectangleGeometry(new Rect(controlPoint.X - PointSize, controlPoint.Y - PointSize,
                    PointSize * 2, PointSize * 2), 1, 1);
                controlGroup.Children.Add(rectPoint);
            }
            drawingContext?.DrawGeometry(BorderBrush, PointPen, controlGroup);
        }

        /// <summary>
        /// Draw the reference line in the moving state
        /// </summary>
        /// <param name="drawDc">
        /// Drawing context handle
        /// </param>
        /// <param name="controltype">
        /// Current selected control point type
        /// </param>
        /// <param name="activePen">
        /// Line brush
        /// </param>
        /// <param name="moveBrush">
        /// Rectangle brush
        /// </param>
        /// <param name="moveRect">
        /// Current rectangle to be drawn
        /// </param>
        protected void DrawMoveBounds(DrawingContext drawDc, PointControlType controltype, Pen activePen, Brush moveBrush, Rect moveRect)
        {
            switch (controltype)
            {
                case PointControlType.LeftTop:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, 0), new Point(moveRect.Left, PDFViewerActualHeight));
                    break;
                case PointControlType.LeftMiddle:
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, 0), new Point(moveRect.Left, PDFViewerActualHeight));
                    break;
                case PointControlType.LeftBottom:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, 0), new Point(moveRect.Left, PDFViewerActualHeight));
                    break;
                case PointControlType.MiddlBottom:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    break;
                case PointControlType.RightBottom:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, 0), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                case PointControlType.RightMiddle:
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, 0), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                case PointControlType.RightTop:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, 0), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                case PointControlType.MiddleTop:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    break;
                case PointControlType.Rotate:
                    break;
                case PointControlType.Body:
                case PointControlType.Line:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, 0), new Point(moveRect.Left, PDFViewerActualHeight));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, 0), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                default:
                    break;
            }
            drawDc?.DrawRectangle(moveBrush, null, moveRect);
        }

        /// <summary>
        /// Calculate the current control point
        /// </summary>
        /// <param name="currentRect">
        /// Control point target rectangle
        /// </param>
        protected void CalcControlPoint(Rect currentRect)
        {
            controlPoints.Clear();
            int centerX = (int)(currentRect.Left + currentRect.Right) / 2;
            int centerY = (int)(currentRect.Top + currentRect.Bottom) / 2;

            controlPoints.Add(new Point(currentRect.Left, currentRect.Top));
            controlPoints.Add(new Point(currentRect.Left, centerY));
            controlPoints.Add(new Point(currentRect.Left, currentRect.Bottom));
            controlPoints.Add(new Point(centerX, currentRect.Bottom));
            controlPoints.Add(new Point(currentRect.Right, currentRect.Bottom));
            controlPoints.Add(new Point(currentRect.Right, centerY));
            controlPoints.Add(new Point(currentRect.Right, currentRect.Top));
            controlPoints.Add(new Point(centerX, currentRect.Top));
        }

        /// <summary>
        /// Calculate the movement of the hit point
        /// </summary>
        /// <param name="mousePoint">
        /// Current mouse position
        /// </param>
        /// <returns>
        /// Return true if the calculation is successful
        /// </returns>
        protected bool CalcHitPointMove(Point mousePoint)
        {
            if (isMouseDown == false || hitControlType == PointControlType.None)
            {
                return false;
            }
            return NormalScaling(mousePoint);
        }

        /// <summary>
        /// Draw the algorithm of the normal scaling form (drag a point, only scale in one direction)
        /// </summary>
        /// <param name="mousePoint">
        /// Current mouse position
        /// </param>
        /// <returns>
        /// Return true if the calculation is successful
        /// </returns>
        protected bool NormalScaling(Point mousePoint)
        {
            double mLeft = cacheRect.Left;
            double mRight = cacheRect.Right;
            double mUp = cacheRect.Top;
            double mDown = cacheRect.Bottom;
            double TmpLeft = mLeft, TmpRight = mRight, TmpUp = mUp, TmpDown = mDown;
            Point centerPoint = new Point((cacheRect.Right + cacheRect.Left) / 2, (cacheRect.Bottom + cacheRect.Top) / 2);
            Point moveVector = (Point)(mousePoint - centerPoint);
            moveVector = ProportionalScalingOffsetPos(moveVector);
            switch (hitControlType)
            {
                case PointControlType.LeftTop:
                    TmpLeft = centerPoint.X + moveVector.X;
                    TmpRight = cacheRect.Right;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpLeft = TmpRight - rectMinWidth;
                    }
                    TmpUp = centerPoint.Y + moveVector.Y;
                    TmpDown = cacheRect.Bottom;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpUp = TmpDown - rectMinHeight;
                    }
                    break;
                case PointControlType.LeftMiddle:
                    TmpLeft = centerPoint.X + moveVector.X;
                    TmpRight = cacheRect.Right;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpLeft = TmpRight - rectMinWidth;
                    }
                    TmpUp = cacheRect.Top;
                    TmpDown = cacheRect.Bottom;
                    break;
                case PointControlType.LeftBottom:
                    TmpLeft = centerPoint.X + moveVector.X;
                    TmpRight = cacheRect.Right;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpLeft = TmpRight - rectMinWidth;
                    }
                    TmpUp = cacheRect.Top;
                    TmpDown = centerPoint.Y + moveVector.Y;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpDown = TmpUp + rectMinHeight;
                    }
                    break;
                case PointControlType.MiddlBottom:
                    TmpLeft = cacheRect.Left;
                    TmpRight = cacheRect.Right;
                    TmpUp = cacheRect.Top;
                    TmpDown = centerPoint.Y + moveVector.Y;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpDown = TmpUp + rectMinHeight;
                    }
                    break;
                case PointControlType.RightBottom:
                    TmpLeft = cacheRect.Left;
                    TmpRight = centerPoint.X + moveVector.X;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpRight = TmpLeft + rectMinWidth;
                    }
                    TmpUp = cacheRect.Top;
                    TmpDown = centerPoint.Y + moveVector.Y;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpDown = TmpUp + rectMinHeight;
                    }
                    break;
                case PointControlType.RightMiddle:
                    TmpLeft = cacheRect.Left;
                    TmpRight = centerPoint.X + moveVector.X;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpRight = TmpLeft + rectMinWidth;
                    }
                    TmpUp = cacheRect.Top;
                    TmpDown = cacheRect.Bottom;
                    break;
                case PointControlType.RightTop:
                    TmpLeft = cacheRect.Left;
                    TmpRight = centerPoint.X + moveVector.X;
                    if (TmpLeft + rectMinWidth > TmpRight)
                    {
                        TmpRight = TmpLeft + rectMinWidth;
                    }
                    TmpUp = centerPoint.Y + moveVector.Y;
                    TmpDown = cacheRect.Bottom;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpUp = TmpDown - rectMinHeight;
                    }
                    break;
                case PointControlType.MiddleTop:
                    TmpLeft = cacheRect.Left;
                    TmpRight = cacheRect.Right;
                    TmpUp = centerPoint.Y + moveVector.Y;
                    TmpDown = cacheRect.Bottom;
                    if (TmpUp + rectMinHeight > TmpDown)
                    {
                        TmpUp = TmpDown - rectMinHeight;
                    }
                    break;
                case PointControlType.Body:
                case PointControlType.Line:
                    Point OffsetPos = CalcMoveBound(cacheRect, ((Point)(mousePoint - mouseDownPoint)), maxRect);
                    TmpLeft = cacheRect.Left + OffsetPos.X;
                    TmpRight = cacheRect.Right + OffsetPos.X;
                    TmpUp = cacheRect.Top + OffsetPos.Y;
                    TmpDown = cacheRect.Bottom + OffsetPos.Y;
                    break;
                default:
                    break;
            }
            if (TmpLeft < maxRect.Left)
            {
                TmpLeft = maxRect.Left;
            }
            if (TmpUp < maxRect.Top)
            {
                TmpUp = maxRect.Top;
            }
            if (TmpRight > maxRect.Right)
            {
                TmpRight = maxRect.Right;
            }
            if (TmpDown > maxRect.Bottom)
            {
                TmpDown = maxRect.Bottom;
            }

            if (TmpRight - TmpLeft < 0.0 || TmpDown - TmpUp < 0.0)
            {
                return false;
            }
            drawRect = new Rect(TmpLeft, TmpUp, TmpRight - TmpLeft, TmpDown - TmpUp);
            moveOffset = new Point(drawRect.X - cacheRect.X, drawRect.Y - cacheRect.Y);
            return true;
        }

        /// <summary>
        /// Proportional scaling offset calibration.
        /// </summary>
        /// <param name="movePoint">
        /// Current mouse position
        /// </param>
        /// <returns>
        /// Return the calibrated offset value
        /// </returns>
        protected Point ProportionalScalingOffsetPos(Point movePoint)
        {
            if (isProportionalScaling)
            {
                Point offsetPos = movePoint;
                double ratioX = cacheRect.Width > 0 ? cacheRect.Height / cacheRect.Width : 1;
                double ratioY = cacheRect.Height > 0 ? cacheRect.Width / cacheRect.Height : 1;
                switch (hitControlType)
                {
                    case PointControlType.LeftTop:
                    case PointControlType.RightBottom:
                        offsetPos = new Point(movePoint.X, Math.Abs(movePoint.X) * ratioX * (movePoint.X < 0 ? -1 : 1));
                        break;
                    case PointControlType.LeftBottom:
                    case PointControlType.RightTop:
                        offsetPos = new Point(movePoint.X, Math.Abs(movePoint.X) * ratioX * (movePoint.X < 0 ? 1 : -1));
                        break;
                    case PointControlType.LeftMiddle:
                        offsetPos = new Point(movePoint.X, Math.Abs(movePoint.X) * ratioX * (movePoint.X < 0 ? 1 : -1));
                        break;
                    case PointControlType.RightMiddle:
                        offsetPos = new Point(movePoint.X, Math.Abs(movePoint.X) * ratioX * (movePoint.X < 0 ? -1 : 1));
                        break;
                    case PointControlType.MiddlBottom:
                        offsetPos = new Point(Math.Abs(movePoint.Y) * ratioY * (movePoint.Y < 0 ? 1 : -1), movePoint.Y);
                        break;
                    case PointControlType.MiddleTop:
                        offsetPos = new Point(Math.Abs(movePoint.Y) * ratioY * (movePoint.Y < 0 ? -1 : 1), movePoint.Y);
                        break;
                    default:
                        break;
                }
                return offsetPos;
            }
            else
            {
                return movePoint;
            }
        }

        /// <summary>
        /// Calc the offset of the current rectangle in the maximum rectangle range
        /// </summary>
        /// <param name="currentRect">
        /// The rectangle cached when pressed.
        /// </param>
        /// <param name="offsetPoint">
        /// Equivalent to the offset value when pressed.
        /// </param>
        /// <param name="maxRect">
        /// The maximum rectangle range.
        /// </param>
        /// <returns>
        /// Return the offset value after the calculation.
        /// </returns>
        protected Point CalcMoveBound(Rect currentRect, Point offsetPoint, Rect maxRect)
        {
            double cLeft = currentRect.Left;
            double cRight = currentRect.Right;
            double cUp = currentRect.Top;
            double cDown = currentRect.Bottom;

            double TmpLeft = cLeft + offsetPoint.X;
            double TmpRight = cRight + offsetPoint.X;
            double TmpUp = cUp + offsetPoint.Y;
            double TmpDown = cDown + offsetPoint.Y;
            if (TmpLeft < maxRect.Left)
            {
                TmpRight = (cRight - cLeft) + maxRect.Left;
                TmpLeft = maxRect.Left;
            }
            if (TmpUp < maxRect.Top)
            {
                TmpDown = (cDown - cUp) + maxRect.Top;
                TmpUp = maxRect.Top;
            }
            if (TmpRight > maxRect.Right)
            {
                TmpLeft = maxRect.Right - (cRight - cLeft);
                TmpRight = maxRect.Right;
            }
            if (TmpDown > maxRect.Bottom)
            {
                TmpUp = maxRect.Bottom - (cDown - cUp);
                TmpDown = maxRect.Bottom;
            }
            offsetPoint = new Point(TmpLeft - cLeft, TmpUp - cUp);
            return offsetPoint;
        }

        /// <summary>
        /// Get which control point the coordinate is on
        /// </summary>
        /// <param name="clickPoint">
        /// The coordinate of the click
        /// </param>
        /// <returns>
        /// Return the control point type
        /// </returns>
        public PointControlType GetHitControlIndex(Point point)
        {
            try
            {
                HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
                if (hitResult != null && hitResult.VisualHit is DrawingVisual)
                {
                    List<PointControlType> ignoreList = GetIgnorePoints();

                    List<Point> IgnorePointsList = new List<Point>();
                    foreach (PointControlType type in ignoreList)
                    {
                        if ((int)type < controlPoints.Count)
                        {
                            IgnorePointsList.Add(controlPoints[(int)type]);
                        }
                    }
                    for (int i = 0; i < controlPoints.Count; i++)
                    {
                        Point checkPoint = controlPoints[i];

                        if (IgnorePointsList.Contains(checkPoint))
                        {
                            continue;
                        }
                        switch (currentDrawPointType)
                        {
                            case DrawPointType.Circle:
                                Vector checkVector = checkPoint - point;
                                if (checkVector.Length < pointSize)
                                {
                                    return (PointControlType)i;
                                }
                                break;
                            case DrawPointType.Square:

                                Rect checkRect = new Rect(Math.Max(checkPoint.X - pointSize, 0), Math.Max(checkPoint.Y - pointSize, 0), pointSize * 2, pointSize * 2);
                                if (checkRect.Contains(point))
                                {
                                    return (PointControlType)i;
                                }
                                break;

                            case DrawPointType.Crop:
                                Rect cropRect = new Rect(Math.Max(checkPoint.X - pointSize, 0), Math.Max(checkPoint.Y - pointSize, 0), pointSize * 2, pointSize * 2);
                                if (cropRect.Contains(point))
                                {
                                    return (PointControlType)i;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (drawRect.Contains(point))
                    {
                        Rect rect = new Rect(Math.Max(drawRect.X + rectPadding, 0), Math.Max(drawRect.Y + rectPadding, 0), drawRect.Width - 2 * rectPadding, drawRect.Height - 2 * rectPadding);
                        if (rect.Contains(point))
                        {
                            if (!ignoreList.Contains(PointControlType.Body))
                            {
                                return PointControlType.Body;
                            }
                        }
                        if (!ignoreList.Contains(PointControlType.Body))
                        {
                            return PointControlType.Line;
                        }
                    }
                }
            }
            catch (Exception ex) 
            { 

            }

            return PointControlType.None;
        }

        /// <summary>
        /// Get the current set of ignored points
        /// </summary>
        /// The dataset of ignored points
        /// </returns>
        private List<PointControlType> GetIgnorePoints()
        {
            List<PointControlType> IgnorePointsList = new List<PointControlType>();
            foreach (PointControlType type in ignorePoints)
            {
                IgnorePointsList.Add(type);
            }
            return IgnorePointsList;
        }

        /// <summary>
        /// Notify events during/after drawing data
        /// </summary>
        /// <param name="isFinish">
        /// Identifies whether the data has changed
        /// </param>
        protected void InvokeDataChangEvent(bool isFinish)
        {
            if (isFinish)
            {
                DataChanged?.Invoke(this, drawRect);
            }
            else
            {
                DataChanging?.Invoke(this, drawRect);
            }
        }

    }
}
