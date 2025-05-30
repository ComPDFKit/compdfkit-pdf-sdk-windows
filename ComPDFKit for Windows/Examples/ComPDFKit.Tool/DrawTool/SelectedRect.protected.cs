﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public partial class SelectedRect
    {
        #region Properties
        /// <summary>
        /// Current control point drawing style.
        /// </summary>
        protected DrawPointType currentDrawPointType
        {
            get;
            set;
        }

        /// <summary>
        /// Current drag drawing style.
        /// </summary>
        protected DrawMoveType currentDrawMoveType { get; set; }

        /// <summary>
        /// Current click hit control point.
        /// </summary>
        protected PointControlType hitControlType
        {
            get;
            set;
        }

        /// <summary>
        /// Mouse down position information.
        /// </summary>
        protected Point mouseDownPoint { get; set; }

        /// <summary>
        /// Whether the mouse is pressed.
        /// </summary>
        protected bool isMouseDown { get; set; }

        protected bool isInRotate { get; set; } = false;

        protected bool isInScaling { get; set; } = false;

        /// <summary>
        /// Whether proportional scaling is required.
        /// </summary>
        protected bool isProportionalScaling { get; set; } = false;

        /// <summary>
        /// Current control point size.
        /// </summary>
        protected int pointSize { get; set; } = 4;

        /// <summary>
        /// Rectangular minimum width.
        /// </summary>
        public int RectMinWidth { get; set; } = 10;

        /// <summary>
        /// Rectangular minimum height.
        /// </summary>
        public int RectMinHeight { get; set; } = 10;

        /// <summary>
        /// Current set of ignore points.
        /// </summary>
        protected List<PointControlType> ignorePoints { get; set; } = new List<PointControlType>();

        /// <summary>
        /// Current set of drawing rectangles (original data).
        /// </summary>
        protected Rect SetDrawRect
        {
            get;
            set;
        } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Current drawing rectangle (calculated during operation).
        /// </summary>
        protected Rect drawRect
        {
            get;
            set;
        } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Maximum range that can be drawn.
        /// </summary>
        protected Rect maxRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Current center point of the drawing rectangle.
        /// </summary>
        protected Point drawCenterPoint { get; private set; } = new Point(0, 0);

        /// <summary>
        /// When the mouse is pressed, the cached rectangle.
        /// </summary>
        protected Rect cacheRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Current control point coordinates.
        /// </summary>
        protected List<Point> controlPoints { get; set; } = new List<Point>();

        protected Point centerPoint = new Point();

        protected Point rotationPoint = new Point();

        protected Point dragRotationPoint = new Point();

        /// <summary>
        /// Move offset during movement.
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// Current drawing rectangle (calculated during operation).
        /// </summary>
        protected Thickness clipThickness = new Thickness(0, 0, 0, 0);

        private Pen editPen { get; set; } = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2) { DashStyle = DashStyles.Dash };

        private Pen editHoverPen { get; set; } = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2) { DashStyle = DashStyles.Dash };

        private bool showCreatTextRect = false;

        protected bool isOutSideScaling = false;

        /// <summary>
        /// Current actual display width and height of PDFVIewer.
        /// </summary>
        protected double PDFViewerActualWidth { get; set; } = 0;

        protected double PDFViewerActualHeight { get; set; } = 0;

        protected int rotateAngle { get; set; } = 0;

        protected int pageRotation { get; set; } = 0;

        protected double rectPadding = 6;

        protected double currentZoom = 1;

        protected SelectedAnnotData selectedRectData = new SelectedAnnotData();

        protected bool disable = false;

        protected List<Point> rotateControlPoints = new List<Point>();

        protected Rect rotateRect = new Rect();

        public bool IsPath { get; set; } = false;

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
            centerPoint.X = (int)(currentRect.Left + currentRect.Right) / 2;
            centerPoint.Y = (int)(currentRect.Top + currentRect.Bottom) / 2;

            controlPoints.Add(new Point(currentRect.Left, currentRect.Top));
            controlPoints.Add(new Point(currentRect.Left, centerPoint.Y));
            controlPoints.Add(new Point(currentRect.Left, currentRect.Bottom));
            controlPoints.Add(new Point(centerPoint.X, currentRect.Bottom));
            controlPoints.Add(new Point(currentRect.Right, currentRect.Bottom));
            controlPoints.Add(new Point(currentRect.Right, centerPoint.Y));
            controlPoints.Add(new Point(currentRect.Right, currentRect.Top));
            controlPoints.Add(new Point(centerPoint.X, currentRect.Top));

            if (canRotate)
            {
                rotationPoint = new Point(centerPoint.X, currentRect.Top - 30);
                switch (pageRotation)
                {
                    case 0:
                        rotationPoint = new Point(centerPoint.X, currentRect.Top - 30);
                        break;

                    case 1:
                        rotationPoint = new Point(currentRect.Right + 30, centerPoint.Y);
                        break;

                    case 2:
                        rotationPoint = new Point(centerPoint.X, currentRect.Bottom + 30);
                        break;

                    case 3:
                        rotationPoint = new Point(currentRect.Left - 30, centerPoint.Y);
                        break;

                    default:
                        break;
                }
            }
        }

        protected List<Point> GetControlPoint(Rect currentRect)
        {
            List<Point> controlCurrentPoints = new List<Point>();
            controlCurrentPoints.Clear();
            int centerX = (int)(currentRect.Left + currentRect.Right) / 2;
            int centerY = (int)(currentRect.Top + currentRect.Bottom) / 2;

            controlCurrentPoints.Add(new Point(currentRect.Left, currentRect.Top));
            controlCurrentPoints.Add(new Point(currentRect.Left, centerY));
            controlCurrentPoints.Add(new Point(currentRect.Left, currentRect.Bottom));
            controlCurrentPoints.Add(new Point(centerX, currentRect.Bottom));
            controlCurrentPoints.Add(new Point(currentRect.Right, currentRect.Bottom));
            controlCurrentPoints.Add(new Point(currentRect.Right, centerY));
            controlCurrentPoints.Add(new Point(currentRect.Right, currentRect.Top));
            controlCurrentPoints.Add(new Point(centerX, currentRect.Top));
            return controlCurrentPoints;
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

            if (hitControlType == PointControlType.Rotate)
            {
                SetRotateByMousePoint(mousePoint);
                return false;
            }

            if (!isOutSideScaling)
            {
                if (selectedRectData.rotationAngle != 0)
                {
                    return RotateScaling(mousePoint);
                }
                else
                {
                    return NormalScaling(mousePoint);
                }
            }
            else
            {
                return OutSideScaling(mousePoint);
            }
        }

        public void SetOutSideScaling(bool IsOutSideScaling)
        {
            isOutSideScaling = IsOutSideScaling;
        }

        private Size GetProportionalScalingSize(double width, double height)
        {
            double minHeight = RectMinHeight + 2 * rectPadding * currentZoom;
            double minWidth = RectMinWidth + 2 * rectPadding * currentZoom;
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

        private void SetRotateByMousePoint(Point mousePoint)
        {
            dragRotationPoint = mousePoint;
            Vector moveVector = (mousePoint - centerPoint);
            rotateAngle = (int)(Math.Atan2(moveVector.X, -moveVector.Y) * 180  / Math.PI) - pageRotation * 90;
        }

        /// <summary>
        /// Draw the algorithm in the form of normal scaling (drag a point, only scale in one direction).
        /// </summary>
        /// <param name="mousePoint">Current mouse position.</param>
        /// <returns></returns>
        protected bool NormalScaling(Point mousePoint)
        {
            try
            {
                double left = 0, right = 0, top = 0, bottom = 0;
                double minHeight = RectMinHeight + 2 * rectPadding * currentZoom;
                double minWidth = RectMinWidth + 2 * rectPadding * currentZoom;

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
                                if (left < maxRect.Left)
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

                    case PointControlType.MiddleBottom:
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
                                    top = bottom - height;
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
                            Point offsetPos = CalcMoveBound(cacheRect, ((Point)(mousePoint - mouseDownPoint)), maxRect);
                            left = cacheRect.Left + offsetPos.X;
                            right = cacheRect.Right + offsetPos.X;
                            top = cacheRect.Top + offsetPos.Y;
                            bottom = cacheRect.Bottom + offsetPos.Y;
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
            catch (Exception ex)
            {
            }
            return false;
        }

        protected bool RotateScaling(Point mouseMovePoint)
        {
            Point rotatePoint = new Point();
            Point hitControlUIPos = new Point();
            if (hitControlType < PointControlType.Body)
            {
                hitControlUIPos = rotateControlPoints[(int)hitControlType];
            }

            hitControlUIPos = GetRotateUIPoint(hitControlUIPos);
            Point centerPoint = new Point((rotateRect.Left + rotateRect.Right) / 2, (rotateRect.Top + rotateRect.Bottom) / 2);
            Vector moveVector = mouseMovePoint - mouseDownPoint;
            Vector hitVector = hitControlUIPos - centerPoint;

            Rect tmpRect = cacheRect;
            if (hitControlType == PointControlType.LeftTop
                || hitControlType == PointControlType.LeftBottom
                || hitControlType == PointControlType.RightTop
                || hitControlType == PointControlType.RightBottom)
            {
                if (isProportionalScaling)
                {
                    double vectorAngle = Vector.AngleBetween(moveVector, hitVector);
                    double newLenght = Math.Cos(Math.PI / 180.0 * vectorAngle) * moveVector.Length;

                    hitVector.Normalize();
                    hitVector.X *= newLenght;
                    hitVector.Y *= newLenght;
                    rotatePoint = new Point(hitControlUIPos.X + hitVector.X, hitControlUIPos.Y + hitVector.Y);
                }
            }

            switch (hitControlType)
            {
                case PointControlType.LeftTop:
                    {
                        Point rightBottomPoint = rotateControlPoints[(int)PointControlType.RightBottom];
                        Point rightBottomUIPos = GetRotateUIPoint(rightBottomPoint);
                        centerPoint = new Point((rotatePoint.X + rightBottomUIPos.X) / 2, (rotatePoint.Y + rightBottomUIPos.Y) / 2);

                        Matrix rotateMatrix = new Matrix();
                        rotateMatrix.RotateAt(-rotateAngle, centerPoint.X, centerPoint.Y);

                        Point leftTopPoint = rotateMatrix.Transform(rotatePoint);
                        rightBottomPoint = rotateMatrix.Transform(rightBottomUIPos);
                        tmpRect = new Rect(leftTopPoint, rightBottomPoint);
                    }
                    break;

                case PointControlType.LeftBottom:
                    {
                        Point rightTopPoint = rotateControlPoints[(int)PointControlType.RightTop];
                        Point rightTopUIPos = GetRotateUIPoint(rightTopPoint);
                        centerPoint = new Point((rotatePoint.X + rightTopUIPos.X) / 2, (rotatePoint.Y + rightTopUIPos.Y) / 2);

                        Matrix rotateMatrix = new Matrix();
                        rotateMatrix.RotateAt(-rotateAngle, centerPoint.X, centerPoint.Y);

                        Point leftBottomPoint = rotateMatrix.Transform(rotatePoint);
                        rightTopPoint = rotateMatrix.Transform(rightTopUIPos);
                        tmpRect = new Rect(leftBottomPoint, rightTopPoint);
                    }
                    break;

                case PointControlType.RightTop:
                    {
                        Point leftBottomPoint = rotateControlPoints[(int)PointControlType.LeftBottom];
                        Point leftBottomUIPos = GetRotateUIPoint(leftBottomPoint);
                        centerPoint = new Point((rotatePoint.X + leftBottomUIPos.X) / 2, (rotatePoint.Y + leftBottomUIPos.Y) / 2);

                        Matrix rotateMatrix = new Matrix();
                        rotateMatrix.RotateAt(-rotateAngle, centerPoint.X, centerPoint.Y);

                        Point rightTopPoint = rotateMatrix.Transform(rotatePoint);
                        leftBottomPoint = rotateMatrix.Transform(leftBottomUIPos);
                        tmpRect = new Rect(leftBottomPoint, rightTopPoint);
                    }
                    break;

                case PointControlType.RightBottom:
                    {
                        Point leftTopPoint = rotateControlPoints[(int)PointControlType.LeftTop];
                        Point leftTopUIPos = GetRotateUIPoint(leftTopPoint);
                        centerPoint = new Point((rotatePoint.X + leftTopUIPos.X) / 2, (rotatePoint.Y + leftTopUIPos.Y) / 2);

                        Matrix rotateMatrix = new Matrix();
                        rotateMatrix.RotateAt(-rotateAngle, centerPoint.X, centerPoint.Y);

                        Point rightBottomPoint = rotateMatrix.Transform(rotatePoint);
                        leftTopPoint = rotateMatrix.Transform(leftTopUIPos);
                        tmpRect = new Rect(leftTopPoint, rightBottomPoint);
                    }
                    break;

                case PointControlType.Body:
                case PointControlType.Line:
                    {
                        Point offsetPos = (Point)(mouseMovePoint - mouseDownPoint);
                        double left = cacheRect.Left + offsetPos.X;
                        double right = cacheRect.Right + offsetPos.X;
                        double top = cacheRect.Top + offsetPos.Y;
                        double bottom = cacheRect.Bottom + offsetPos.Y;
                        tmpRect = new Rect(new Point(left,top), new Point(right,bottom));
                    }
                    break;

                default:
                    break;
            }

            List<Point> tempPoints = new List<Point>
            {
                new Point(tmpRect.Left, tmpRect.Top),
                new Point(tmpRect.Right, tmpRect.Top),
                new Point(tmpRect.Right, tmpRect.Bottom),
                new Point(tmpRect.Left, tmpRect.Bottom)
            };

            List<Point> boundPoint = new List<Point>();
            Point center = new Point((tmpRect.Left + tmpRect.Right) / 2, (tmpRect.Top + tmpRect.Bottom) / 2);
            foreach (Point point in tempPoints)
            {
                float x = (float)(center.X + (point.X - center.X) * Math.Cos(rotateAngle * Math.PI / 180) - (point.Y - center.Y) * Math.Sin(rotateAngle * Math.PI / 180));
                float y = (float)(center.Y + (point.X - center.X) * Math.Sin(rotateAngle * Math.PI / 180) + (point.Y - center.Y) * Math.Cos(rotateAngle * Math.PI / 180));
                boundPoint.Add(new Point(x, y));
            }

            Rect boundRect = new Rect(new Point(boundPoint.Min(p => p.X), boundPoint.Min(p => p.Y)), new Point(boundPoint.Max(p => p.X), boundPoint.Max(p => p.Y)));
            if (maxRect.Contains(boundRect))
            {
                drawRect = tmpRect;
            }
            else
            {
                if(hitControlType == PointControlType.Body || hitControlType == PointControlType.Line)
                {
                    Point boundRectCenterPos = new Point((boundRect.Left + boundRect.Right) / 2, (boundRect.Top + boundRect.Bottom) / 2);
                    Point maxRectCenterPos = new Point((maxRect.Left + maxRect.Right) / 2, (maxRect.Top + maxRect.Bottom) / 2);
                    Vector moveCenterVector = boundRectCenterPos - maxRectCenterPos;
                    Point moveOffsetPos = new Point(0, 0);
                    if (Math.Abs(moveCenterVector.X) > (maxRect.Width - boundRect.Width) / 2)
                    {
                        double moveLength = maxRectCenterPos.X - boundRectCenterPos.X;
                        moveOffsetPos.X = Math.Abs(moveLength) - (maxRect.Width - boundRect.Width) / 2;
                        if (moveLength < 0)
                        {
                            moveOffsetPos.X *= -1;
                        }
                    }

                    if (Math.Abs(moveCenterVector.Y) > (maxRect.Height - boundRect.Height) / 2)
                    {
                        double moveLength = maxRectCenterPos.Y - boundRectCenterPos.Y;
                        moveOffsetPos.Y = Math.Abs(moveLength) - (maxRect.Height - boundRect.Height) / 2;
                        if (moveLength < 0)
                        {
                            moveOffsetPos.Y *= -1;
                        }
                    }

                    drawRect = new Rect(tmpRect.Left + moveOffsetPos.X, tmpRect.Top + moveOffsetPos.Y, tmpRect.Width, tmpRect.Height);
                }
            }

            moveOffset = new Point(drawRect.X - cacheRect.X, drawRect.Y - cacheRect.Y);
            return true;
        }

        private Point GetRotateUIPoint(Point point)
        {
            Point centerPoint = new Point((rotateRect.Left + rotateRect.Right) / 2, (rotateRect.Top + rotateRect.Bottom) / 2);
            Matrix rotateMatrix = new Matrix();
            rotateMatrix.RotateAt(rotateAngle, centerPoint.X, centerPoint.Y);
            return rotateMatrix.Transform(point);
        }

        /// <summary>
        /// Provisional logic, to be further improved, not yet used: Draw the algorithm in the form of normal scaling (drag a point, only scale in one direction).
        /// </summary>
        /// <param name="mousePoint">Current mouse position.</param>
        /// <returns></returns>
        protected bool OutSideScaling(Point mousePoint)
        {
            try
            {
                double left = 0, right = 0, top = 0, bottom = 0;
                double minHeight = RectMinHeight + 2 * rectPadding * currentZoom;
                double minWidth = RectMinWidth + 2 * rectPadding * currentZoom;

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
                                if (left < maxRect.Left)
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

                    case PointControlType.MiddleBottom:
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
                                    top = bottom - height;
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
                            double newleft = maxRect.Left - SetDrawRect.Width + 10;
                            double newright = maxRect.Right + SetDrawRect.Width - 10;
                            double newtop = maxRect.Top - SetDrawRect.Height + 10;
                            double newbottom = maxRect.Bottom + SetDrawRect.Height - 10;
                            if (newleft < 0)
                            {
                                newleft = 0;
                            }
                            Rect newMaxRect = new Rect(newleft, newtop, newright - newleft, newbottom - newtop);

                            Point OffsetPos = CalcMoveBound(cacheRect, ((Point)(mousePoint - mouseDownPoint)), newMaxRect);
                            left = cacheRect.Left + OffsetPos.X;
                            right = cacheRect.Right + OffsetPos.X;
                            top = cacheRect.Top + OffsetPos.Y;
                            bottom = cacheRect.Bottom + OffsetPos.Y;
                        }
                        break;

                    default:
                        break;
                }

                //if (left < maxRect.Left)
                //{
                //    left = maxRect.Left;
                //}

                //if (top < maxRect.Top)
                //{
                //    top = maxRect.Top;
                //}

                if (right > maxRect.Right + SetDrawRect.Width - 10)
                {
                    if (left > maxRect.Right - 10)
                    {
                        left = maxRect.Right - 10;
                    }
                    right = maxRect.Right + SetDrawRect.Width - 10;
                }

                if (bottom > maxRect.Bottom + SetDrawRect.Height - 10)
                {
                    if (top > maxRect.Bottom - 10)
                    {
                        top = maxRect.Bottom - 10;
                    }
                    bottom = maxRect.Bottom + SetDrawRect.Height - 10;
                }

                drawRect = new Rect(left, top, right - left, bottom - top);
                moveOffset = new Point(drawRect.X - cacheRect.X, drawRect.Y - cacheRect.Y);
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
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
                    case PointControlType.MiddleBottom:
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
            RotateTransform rotateTransform = new RotateTransform(rotateAngle, centerPoint.X, centerPoint.Y);
            if (canRotate && !isInScaling)
            {
                Point currentRotationPoint = isInRotate ? dragRotationPoint : rotationPoint;
                double angleInRadians = rotateAngle * (Math.PI / 180);
                double sinValue = Math.Sin(angleInRadians);
                double cosValue = Math.Cos(angleInRadians);
                double rotatedX = 0;
                double rotatedY = 0;
                switch (pageRotation)
                {
                    case 0:
                         rotatedX = currentRotationPoint.X - pointSize * sinValue;
                         rotatedY = currentRotationPoint.Y + pointSize * cosValue;
                        break;

                    case 1:
                        rotatedX = currentRotationPoint.X - pointSize * cosValue;
                        rotatedY = currentRotationPoint.Y + pointSize * sinValue;
                        break;

                    case 2:
                        rotatedX = currentRotationPoint.X + pointSize * sinValue;
                        rotatedY = currentRotationPoint.Y - pointSize * cosValue;
                        break;

                    case 3:
                        rotatedX = currentRotationPoint.X + pointSize * sinValue;
                        rotatedY = currentRotationPoint.Y - pointSize * cosValue;
                        break;

                    default:
                        break;
                }

                GeometryGroup rotateGroup = new GeometryGroup();
                LineGeometry moveLineGeometry = new LineGeometry(centerPoint, new Point(rotatedX, rotatedY));
                EllipseGeometry ellipseGeometry = new EllipseGeometry(currentRotationPoint, PointSize, pointSize);
                rotateGroup.Children.Add(moveLineGeometry);
                rotateGroup.Children.Add(ellipseGeometry);
                if (!isInRotate)
                {
                    rotateGroup.Children.Remove(moveLineGeometry);
                    LineGeometry stopLineGeometry = new LineGeometry(centerPoint, new Point(currentRotationPoint.X, currentRotationPoint.Y + pointSize));
                    switch (pageRotation)
                    {
                        case 0:
                            stopLineGeometry = new LineGeometry(centerPoint, new Point(currentRotationPoint.X, currentRotationPoint.Y + pointSize));

                            break;

                        case 1:
                            stopLineGeometry = new LineGeometry(centerPoint, new Point(currentRotationPoint.X - pointSize, currentRotationPoint.Y ));

                            break;

                        case 2:
                            stopLineGeometry = new LineGeometry(centerPoint, new Point(currentRotationPoint.X, currentRotationPoint.Y - pointSize));

                            break;

                        case 3:
                            stopLineGeometry = new LineGeometry(centerPoint, new Point(currentRotationPoint.X + pointSize, currentRotationPoint.Y));
                            break;

                        default:
                            break;
                    }

                    rotateGroup.Children.Add(stopLineGeometry);
                    drawingContext.PushTransform(rotateTransform);
                }

                drawingContext?.DrawGeometry(BorderBrush, PointPen, rotateGroup);
                if (!isInRotate)
                {

                    drawingContext.Pop();
                }
            }
            if (!isInRotate)
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

                drawingContext.PushTransform(rotateTransform);
                drawingContext?.DrawGeometry(BorderBrush, PointPen, controlGroup);
                drawingContext.Pop();
            }
        }

        protected void DrawCropPoint(DrawingContext drawingContext, List<PointControlType> ignoreList, int PointSize, Pen PointPen, SolidColorBrush BorderBrush)
        {
            //GeometryGroup controlGroup = new GeometryGroup();
            //controlGroup.FillRule = FillRule.Nonzero;
            clipThickness.Left = (SetDrawRect.Left - drawRect.Left) / currentZoom;
            clipThickness.Top = (SetDrawRect.Top - drawRect.Top) / currentZoom;
            clipThickness.Right = (SetDrawRect.Right - drawRect.Right) / currentZoom;
            clipThickness.Bottom = (SetDrawRect.Bottom - drawRect.Bottom) / currentZoom;
            List<Point> controlCurrentPoints = GetControlPoint(drawRect);
            CombinedGeometry controlGroup = new CombinedGeometry();
            RectangleGeometry paintGeometry = new RectangleGeometry();
            paintGeometry.Rect = SetDrawRect;
            controlGroup.Geometry1 = paintGeometry;
            RectangleGeometry moveGeometry = new RectangleGeometry();
            Rect clippedBorder = drawRect;
            if (clippedBorder.IsEmpty == false)
            {
                moveGeometry.Rect = drawRect;
            }
            controlGroup.Geometry2 = moveGeometry;
            controlGroup.GeometryCombineMode = GeometryCombineMode.Exclude;
            //Left Top Corner
            if (!ignoreList.Contains(PointControlType.LeftTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[0].X - PointSize, controlCurrentPoints[0].Y - PointSize, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[0].X - PointSize, controlCurrentPoints[0].Y - PointSize, PointSize * 4, PointSize));
            }

            //Left Center
            if (!ignoreList.Contains(PointControlType.LeftMiddle))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[1].X - PointSize, (controlCurrentPoints[1].Y + controlCurrentPoints[1].Y - PointSize * 5) / 2, PointSize, PointSize * 5));
            }

            //Left Bottom Corner
            if (!ignoreList.Contains(PointControlType.LeftBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[2].X - PointSize, controlCurrentPoints[2].Y - PointSize * 3, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[2].X - PointSize, controlCurrentPoints[2].Y, PointSize * 4, PointSize));
            }

            //Bottom Center
            if (!ignoreList.Contains(PointControlType.MiddleBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect((controlCurrentPoints[3].X + controlCurrentPoints[3].X - PointSize * 5) / 2, controlCurrentPoints[3].Y, PointSize * 5, PointSize));
            }

            //Bottom Right Corner
            if (!ignoreList.Contains(PointControlType.RightBottom))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[4].X, controlCurrentPoints[4].Y - PointSize * 3, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[4].X - PointSize * 3, controlCurrentPoints[4].Y, PointSize * 4, PointSize));
            }

            //Right Center
            if (!ignoreList.Contains(PointControlType.RightMiddle))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[5].X, (controlCurrentPoints[5].Y + controlCurrentPoints[5].Y - PointSize * 5) / 2, PointSize, PointSize * 5));
            }

            //Right Top Corner
            if (!ignoreList.Contains(PointControlType.RightTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[6].X, controlCurrentPoints[6].Y - PointSize, PointSize, PointSize * 4));
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect(controlCurrentPoints[6].X - PointSize * 4, controlCurrentPoints[6].Y - PointSize, PointSize * 4, PointSize));
            }

            //Top Center
            if (!ignoreList.Contains(PointControlType.MiddleTop))
            {
                drawingContext?.DrawRectangle(BorderBrush, null, new Rect((controlCurrentPoints[7].X + controlCurrentPoints[7].X - PointSize * 5) / 2, controlCurrentPoints[7].Y - PointSize, PointSize * 5, PointSize));
            }
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x3F, 0x00, 0x00, 0x00));
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
        protected void DrawMoveBounds(DrawingContext drawDc, PointControlType controltype, Pen activePen, Brush moveBrush, Rect moveRect, Pen RectPen = null)
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
                case PointControlType.MiddleBottom:
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
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(moveRect.Left, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, moveRect.Top), new Point(moveRect.Left, 0));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Top), new Point(moveRect.Right, 0));

                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(moveRect.Left, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, moveRect.Bottom), new Point(moveRect.Left, PDFViewerActualHeight));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Bottom), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                case PointControlType.Body:
                case PointControlType.Line:
                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Top), new Point(moveRect.Left, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Top), new Point(PDFViewerActualWidth, moveRect.Top));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, moveRect.Top), new Point(moveRect.Left, 0));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Top), new Point(moveRect.Right, 0));

                    drawDc?.DrawLine(activePen, new Point(0, moveRect.Bottom), new Point(moveRect.Left, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Bottom), new Point(PDFViewerActualWidth, moveRect.Bottom));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Left, moveRect.Bottom), new Point(moveRect.Left, PDFViewerActualHeight));
                    drawDc?.DrawLine(activePen, new Point(moveRect.Right, moveRect.Bottom), new Point(moveRect.Right, PDFViewerActualHeight));
                    break;
                default:
                    break;
            }
            drawDc?.DrawRectangle(moveBrush, RectPen, moveRect);
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
            selectedRectData.rotationAngle = rotateAngle;
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
