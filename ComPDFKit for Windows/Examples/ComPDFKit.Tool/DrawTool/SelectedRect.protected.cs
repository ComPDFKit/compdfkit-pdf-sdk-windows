using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public partial class SelectedRect
    {
        #region Properties

        /// <summary>
        /// 当前控制点的绘制样式
        /// </summary>
        protected DrawPointType currentDrawPointType { get; set; }

        /// <summary>
        /// 当前拖拽的绘制样式
        /// </summary>
        protected DrawMoveType currentDrawMoveType { get; set; }

        /// <summary>
        /// 当前点击命中的控制点
        /// </summary>
        protected PointControlType hitControlType { get; set; }

        /// <summary>
        /// 鼠标按下时记录的位置信息
        /// </summary>
        protected Point mouseDownPoint { get; set; }

        /// <summary>
        /// 鼠标是否按下
        /// </summary>
        protected bool isMouseDown { get; set; }

        /// <summary>
        /// 是否需要等比例缩放
        /// </summary>
        protected bool isProportionalScaling { get; set; } = false;

        /// <summary>
        /// 控制点大小
        /// </summary>
        protected int pointSize { get; set; } = 4;

        /// <summary>
        /// 矩形的最小宽度
        /// </summary>
        protected int rectMinWidth { get; set; } = 10;

        /// <summary>
        /// 矩形的最小高度
        /// </summary>
        protected int RectMinHeight { get; set; } = 10;

        /// <summary>
        /// 当前设置的忽略点
        /// </summary>
        protected List<PointControlType> ignorePoints { get; set; } = new List<PointControlType>();

        /// <summary>
        /// 当前设置的绘制矩形（原始数据）
        /// </summary>
        protected Rect SetDrawRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 当前绘制的矩形（操作过程中计算得到）
        /// </summary>
        protected Rect drawRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 可绘制的最大范围
        /// </summary>
        protected Rect maxRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 当前绘制的矩形的中心点
        /// </summary>
        protected Point drawCenterPoint { get; private set; } = new Point(0, 0);

        /// <summary>
        /// 鼠标按下时缓存的矩形
        /// </summary>
        protected Rect cacheRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 当前控制点的坐标
        /// </summary>
        protected List<Point> controlPoints { get; set; } = new List<Point>();

        /// <summary>
        /// 移动过程中的偏移值
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// 当前PDFVIewer的实际显示宽高
        /// </summary>
        protected double PDFViewerActualWidth { get; set; } = 0;

        protected double PDFViewerActualHeight { get; set; } = 0;

        protected double rectPadding = 6;

        protected double currentZoom = 1;

        protected SelectedAnnotData selectedRectData = new SelectedAnnotData();

        protected bool disable = false;


        #endregion

        #region Functions

        /// <summary>
        /// Calcuate the control points
        /// </summary>
        /// <param name="currentRect">
        /// Control points in the target rectangle
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
        /// Calcuate the offset of the current rectangle in the maximum rectangle range
        /// </summary>
        /// <param name="currentRect">
        /// The rectangle cached when pressed
        /// </param>
        /// <param name="offsetPoint">
        /// Equivalent to the offset value when pressed
        /// </param>
        /// <param name="maxRect">
        /// The maximum rectangle range.
        /// </param>
        /// <returns></returns>
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
        /// Calculate the movement of the hit point
        /// </summary>
        /// <param name="mousePoint">
        /// Current mouse position
        /// </param>
        /// <returns>
        /// Whether the movement is successful
        /// </returns>
        protected bool CalcHitPointMove(Point mousePoint)
        {
            if (isMouseDown == false || hitControlType == PointControlType.None)
            {
                return false;
            }
            return NormalScaling(mousePoint);
        }

        private Size GetProportionalScalingSize(double width, double height)
        {
            double minHeight = RectMinHeight + 2 * rectPadding * currentZoom;
            double minWidth = rectMinWidth + 2 * rectPadding * currentZoom;
            if (minWidth > width || minHeight > height)
            {
                if (cacheRect.Width >= cacheRect.Height)
                {
                    width = cacheRect.Width * minHeight / cacheRect.Height;
                    height = minHeight;
                }
                else
                {
                    height = cacheRect.Height * minWidth / cacheRect.Width;
                    width = minWidth;
                }
            }

            return new Size(width, height);
        }

        /// <summary>
        /// 绘制常规缩放形式的算法（拖拽一个点，只向一个方向缩放）
        /// </summary>
        /// <param name="mousePoint">当前鼠标所在位置</param>
        /// <returns></returns>
        protected bool NormalScaling(Point mousePoint)
        {
            double left = 0, right = 0, top = 0, bottom = 0;
            double minHeight = RectMinHeight + 2 * rectPadding * currentZoom;
            double minWidth = rectMinWidth + 2 * rectPadding * currentZoom;

            Point centerPoint = new Point((cacheRect.Right + cacheRect.Left) / 2, (cacheRect.Bottom + cacheRect.Top) / 2);
            Point moveVector = (Point)(mousePoint - centerPoint);
            moveVector = ProportionalScalingOffsetPos(moveVector);

            switch (hitControlType)
            {
                case PointControlType.LeftTop:
                    {
                        left = centerPoint.X + moveVector.X;
                        right = cacheRect.Right;
                        top = centerPoint.Y + moveVector.Y;
                        bottom = cacheRect.Bottom;
                        if (isProportionalScaling)
                        {
                            Size size = GetProportionalScalingSize(right - left, bottom - top);
                            left = right - size.Width;
                            top = bottom - size.Height;
                            if(left < maxRect.Left)
                            {
                                double tmpWidth = right - left;
                                left = maxRect.Left;
                                double width = right - left;
                                double height = (bottom - top) * width / tmpWidth;
                                top = bottom - height;
                            }

                            if (top < maxRect.Top)
                            {
                                double tmpHeight = bottom - top;
                                top = maxRect.Top;
                                double height = bottom - top;
                                double width = (right - left) * height / tmpHeight;
                                left = right - width;
                            }
                        }
                        else
                        {
                            if (left + minWidth > right)
                            {
                                left = right - minWidth;
                            }

                            if (top + minHeight > bottom)
                            {
                                top = bottom - minHeight;
                            }
                        }
                    }
                    break;

                case PointControlType.LeftMiddle:
                    {
                        left = centerPoint.X + moveVector.X;
                        right = cacheRect.Right;
                        top = cacheRect.Top;
                        bottom = cacheRect.Bottom;
                        if (left + minWidth > right)
                        {
                            left = right - minWidth;
                        }
                    }
                    break;

                case PointControlType.LeftBottom:
                    {
                        left = centerPoint.X + moveVector.X;
                        right = cacheRect.Right;
                        top = cacheRect.Top;
                        bottom = centerPoint.Y + moveVector.Y;
                        if (isProportionalScaling)
                        {
                            Size size = GetProportionalScalingSize(right - left, bottom - top);
                            left = right - size.Width;
                            bottom = top + size.Height;
                            if (left < maxRect.Left)
                            {
                                double tmpWidth = right - left;
                                left = maxRect.Left;
                                double width = right - left;
                                double height = (bottom - top) * width / tmpWidth;
                                bottom = top + height;
                            }

                            if (bottom > maxRect.Bottom)
                            {
                                double tmpHeight = bottom - top;
                                bottom = maxRect.Bottom;
                                double height = bottom - top;
                                double width = (right - left) * height / tmpHeight;
                                left = right - width;
                            }
                        }
                        else
                        {
                            if (left + minWidth > right)
                            {
                                left = right - minWidth;
                            }
                            
                            if (top + minHeight > bottom)
                            {
                                bottom = top + minHeight;
                            }
                        }
                    }
                    break;

                case PointControlType.MiddlBottom:
                    {
                        left = cacheRect.Left;
                        right = cacheRect.Right;
                        top = cacheRect.Top;
                        bottom = centerPoint.Y + moveVector.Y;
                        if (top + minHeight > bottom)
                        {
                            bottom = top + minHeight;
                        }
                    }
                    break;

                case PointControlType.RightBottom:
                    {
                        left = cacheRect.Left;
                        right = centerPoint.X + moveVector.X;
                        top = cacheRect.Top;
                        bottom = centerPoint.Y + moveVector.Y;
                        if (isProportionalScaling)
                        {
                            Size size = GetProportionalScalingSize(right - left, bottom - top);
                            right = left + size.Width;
                            bottom = top + size.Height;
                            if (right > maxRect.Right)
                            {
                                double tmpWidth = right - left;
                                right = maxRect.Right;
                                double width = right - left;
                                double height = (bottom - top) * width / tmpWidth;
                                bottom = top + height;
                            }

                            if (bottom > maxRect.Bottom)
                            {
                                double tmpHeight = bottom - top;
                                bottom = maxRect.Bottom;
                                double height = bottom - top;
                                double width = (right - left) * height / tmpHeight;
                                right = left + width;
                            }
                        }
                        else
                        {
                            if (left + minWidth > right)
                            {
                                right = left + minWidth;
                            }

                            if (top + minHeight > bottom)
                            {
                                bottom = top + minHeight;
                            }
                        }
                    }
                    break;

                case PointControlType.RightMiddle:
                    {
                        left = cacheRect.Left;
                        right = centerPoint.X + moveVector.X;
                        top = cacheRect.Top;
                        bottom = cacheRect.Bottom;
                        if (left + minWidth > right)
                        {
                            right = left + minWidth;
                        }
                    }
                    break;

                case PointControlType.RightTop:
                    {
                        left = cacheRect.Left;
                        right = centerPoint.X + moveVector.X;
                        top = centerPoint.Y + moveVector.Y;
                        bottom = cacheRect.Bottom;
                        if (isProportionalScaling)
                        {
                            Size size = GetProportionalScalingSize(right - left, bottom - top);
                            right = left + size.Width;
                            top = bottom - size.Height;
                            if (right > maxRect.Right)
                            {
                                double tmpWidth = right - left;
                                right = maxRect.Right;
                                double width = right - left;
                                double height = (bottom - top) * width / tmpWidth;
                                top =  bottom - height;
                            }

                            if (top < maxRect.Top)
                            {
                                double tmpHeight = bottom - top;
                                top = maxRect.Top;
                                double height = bottom - top;
                                double width = (right - left) * height / tmpHeight;
                                right = left + width;
                            }
                        }
                        else
                        {
                            if (left + minWidth > right)
                            {
                                right = left + minWidth;
                            }

                            if (top + minHeight > bottom)
                            {
                                top = bottom - minHeight;
                            }
                        }
                    }
                    break;

                case PointControlType.MiddleTop:
                    {
                        left = cacheRect.Left;
                        right = cacheRect.Right;
                        top = centerPoint.Y + moveVector.Y;
                        bottom = cacheRect.Bottom;
                        if (top + minHeight > bottom)
                        {
                            top = bottom - minHeight;
                        }
                    }
                    break;

                case PointControlType.Body:
                case PointControlType.Line:
                    {
                        Point OffsetPos = CalcMoveBound(cacheRect, ((Point)(mousePoint - mouseDownPoint)), maxRect);
                        left = cacheRect.Left + OffsetPos.X;
                        right = cacheRect.Right + OffsetPos.X;
                        top = cacheRect.Top + OffsetPos.Y;
                        bottom = cacheRect.Bottom + OffsetPos.Y;
                    }
                    break;

                default:
                    break;
            }

            if (left < maxRect.Left)
            {
                left = maxRect.Left;
            }

            if (top < maxRect.Top)
            {
                top = maxRect.Top;
            }

            if (right > maxRect.Right)
            {
                right = maxRect.Right;
            }

            if (bottom > maxRect.Bottom)
            {
                bottom = maxRect.Bottom;
            }

            drawRect = new Rect(left, top, right - left, bottom - top);
            moveOffset = new Point(drawRect.X - cacheRect.X, drawRect.Y - cacheRect.Y);
            return true;
        }

        /// <summary>
        /// Proportional scaling offset calibration
        /// </summary>
        /// <param name="movePoint">
        /// The current movement point
        /// </param>
        /// <returns>
        /// The offset point after the proportional scaling
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
        /// Inner drawing circle point
        /// </summary>
        /// <param name="drawingContext">
        /// Drawing context
        /// </param>
        /// <param name="ignoreList">
        /// Collection of positions that need to be drawn
        /// </param>
        /// <param name="PointSize">
        /// Size of the point
        /// </param>
        /// <param name="PointPen">
        /// Brush for drawing points
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
        /// Collection of positions that need to be drawn
        /// </param>
        /// <param name="PointSize">
        /// Size of the point
        /// </param>
        /// <param name="PointPen">
        /// Brush for drawing points
        /// </param>
        /// <param name="BorderBrush">
        /// Border brush for drawing points
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
        
        protected void DrawCropPoint(DrawingContext drawingContext, List<PointControlType> ignoreList, int PointSize, Pen PointPen, SolidColorBrush BorderBrush)
        {
            GeometryGroup controlGroup = new GeometryGroup();
            controlGroup.FillRule = FillRule.Nonzero;
            
            //Left Top Corner
            if (!ignoreList.Contains(PointControlType.LeftTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[0].X - PointSize, controlPoints[0].Y - PointSize, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[0].X - PointSize, controlPoints[0].Y - PointSize, PointSize * 4, PointSize));
            }
            
            //Left Center
            if (!ignoreList.Contains(PointControlType.LeftMiddle))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[1].X - PointSize, (controlPoints[1].Y + controlPoints[1].Y - PointSize * 5) / 2, PointSize, PointSize * 5));
            }

            //Left Bottom Corner
            if (!ignoreList.Contains(PointControlType.LeftBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[2].X - PointSize, controlPoints[2].Y - PointSize * 3, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[2].X - PointSize, controlPoints[2].Y, PointSize * 4, PointSize));
            }
            
            //Bottom Center
            if (!ignoreList.Contains(PointControlType.MiddlBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect((controlPoints[3].X + controlPoints[3].X - PointSize * 5) / 2, controlPoints[3].Y, PointSize * 5, PointSize));
            }

            //Bottom Right Corner
            if (!ignoreList.Contains(PointControlType.RightBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[4].X , controlPoints[4].Y - PointSize * 3, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[4].X - PointSize * 3, controlPoints[4].Y, PointSize * 4, PointSize));
            }
            
            //Right Center
            if (!ignoreList.Contains(PointControlType.RightMiddle))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[5].X, (controlPoints[5].Y + controlPoints[5].Y - PointSize * 5) / 2, PointSize, PointSize * 5));
            }
            
            //Right Top Corner
            if (!ignoreList.Contains(PointControlType.RightTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[6].X, controlPoints[6].Y - PointSize, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlPoints[6].X - PointSize * 4, controlPoints[6].Y - PointSize, PointSize * 4, PointSize));
            }
            
            //Top Center
            if (!ignoreList.Contains(PointControlType.MiddleTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect((controlPoints[7].X + controlPoints[7].X - PointSize * 5) / 2, controlPoints[7].Y-PointSize, PointSize * 5, PointSize));
            }
            drawingContext?.DrawGeometry(BorderBrush, PointPen, controlGroup);
        }

        /// <summary>
        /// Draw the reference line in the moving state
        /// </summary>
        /// <param name="drawDc">
        ///  Draw context handle
        /// </param>
        /// <param name="controltype">
        /// Current selected control point type
        /// </param>
        /// <param name="activePen">
        /// Brush for drawing lines
        /// </param>
        /// <param name="moveBrush">
        /// Brush for drawing rectangles
        /// </param>
        /// <param name="moveRect">
        /// Current rectangle to draw
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
        /// Notify the event during/after the drawing data
        /// </summary>
        /// <param name="isFinish">
        /// Identifies whether the data change is complete
        /// </param>
        protected void InvokeDataChangEvent(bool isFinish)
        {
            selectedRectData.Square = GetRect();
            if (isFinish)
            {
                DataChanged?.Invoke(this, selectedRectData);
            }
            else
            {
                DataChanging?.Invoke(this, selectedRectData);
            }
        }

        /// <summary>
        /// Align the rectangle drawing
        /// </summary>
        /// <param name="RectMovePoint">
        /// Move distance required for the aligned algorithm to obtain the rectangle
        /// </param>
        private void DrawAlignRect(Point RectMovePoint)
        {
            double TmpLeft, TmpRight, TmpUp, TmpDown;
            Point OffsetPos = CalcMoveBound(drawRect, RectMovePoint, maxRect);
            TmpLeft = drawRect.Left + OffsetPos.X;
            TmpRight = drawRect.Right + OffsetPos.X;
            TmpUp = drawRect.Top + OffsetPos.Y;
            TmpDown = drawRect.Bottom + OffsetPos.Y;
            SetDrawRect = drawRect = new Rect(TmpLeft, TmpUp, TmpRight - TmpLeft, TmpDown - TmpUp);
            Draw();
        }


        /// <summary>
        /// Get the current set of ignore points
        /// </summary>
        /// <returns>
        /// Data set of ignored points
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

        #endregion
    }
}
