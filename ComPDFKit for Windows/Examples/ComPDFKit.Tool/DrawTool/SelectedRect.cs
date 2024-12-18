using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public enum PointControlType
    {
        None = -1,
        LeftTop,
        LeftMiddle,
        LeftBottom,
        MiddleBottom,
        RightBottom,
        RightMiddle,
        RightTop,
        MiddleTop, 
        Body,
        Line, 
        Rotate
    }

    public enum SelectedType
    {
        None = -1,
        Annot,
        PDFEdit
    }

    public enum DrawPointType
    {
        Circle,
        Square, 
        Crop, 
    }

    public enum DrawMoveType
    {
        kDefault,
        kReferenceLine,
        kRotatable
    }

    public class SelectedAnnotData
    {
        /// <summary>
        /// Current size of the rectangle
        /// </summary>
        public Rect Square { get; set; }

        /// <summary>
        /// Current points of the rectangle
        /// </summary>
        public PointCollection Points { get; set; }

        public AnnotData annotData { get; set; }

        public int rotationAngle { get; set; }
    }

    public partial class SelectedRect : DrawingVisual
    { 
        /// <summary>
        /// Re-layout child elements
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
        public event EventHandler<SelectedAnnotData> DataChanging;

        /// <summary>
        /// Data changed event
        /// </summary>
        public event EventHandler<SelectedAnnotData> DataChanged;

        protected bool isHover = false;

        protected bool isSelected = false;

        protected bool canRotate = false;

        protected SelectedType selectedType = SelectedType.None;

        public SelectedType GetSelectedType()
        {
            return selectedType;
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

        public void SetCurrentDrawPointType(DrawPointType type)
        {
            currentDrawPointType = type;
        }

        public DrawPointType GetCurrentDrawPointType()
        {
            return currentDrawPointType;
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
                //Crop judgment point
                if (currentDrawPointType == DrawPointType.Crop)
                {
                    hitControlType = GetHitCropControlIndex(downPoint);
                }
                else 
                {
                    hitControlType = GetHitControlIndex(downPoint);
                }

                if (hitControlType != PointControlType.None)
                {
                    cacheRect = drawRect;
                    rotateRect = drawRect;
                    rotateControlPoints = controlPoints.ToList();

                    if (hitControlType != PointControlType.Rotate)
                    {
                        isInScaling = true;
                    }
                    else
                    {
                        isInRotate = true;
                    }
                }
            }
        }

        public virtual void OnMouseLeftButtonUp(Point upPoint)
        {
            if (isMouseDown && hitControlType != PointControlType.None)
            {
                isMouseDown = false;
                isInScaling = false;
                isInRotate = false;
                cacheRect = SetDrawRect = drawRect;
                Draw();
                if ((int)upPoint.X != (int)mouseDownPoint.X || (int)upPoint.Y != (int)mouseDownPoint.Y)
                {
                    InvokeDataChangEvent(true);
                }
            }
            moveOffset = new Point(0, 0);
        }

        public virtual void OnMouseMove(Point mousePoint, out bool Tag, double width, double height)
        {
            PDFViewerActualWidth = width;
            PDFViewerActualHeight = height;
            Tag = false;
            if (isMouseDown && hitControlType != PointControlType.None)
            {
                Tag = isMouseDown;
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

        public Cursor GetCursor(Point downPoint, Cursor cursor)
        { 
            if (isMouseDown)
            {
                return cursor;
            }
            hitControlType = GetHitControlIndex(downPoint);
            switch (hitControlType)
            {
                case PointControlType.LeftTop:
                case PointControlType.RightBottom:
                    return Cursors.SizeNWSE; 
                case PointControlType.LeftMiddle:
                case PointControlType.RightMiddle:
                    return Cursors.SizeWE; 
                case PointControlType.LeftBottom:
                case PointControlType.RightTop:
                    return Cursors.SizeNESW;
                case PointControlType.MiddleBottom:
                case PointControlType.MiddleTop:
                    return Cursors.SizeNS;
                case PointControlType.Body:
                    return Cursors.Arrow;
                case PointControlType.Line:
                    return Cursors.SizeAll;
                case PointControlType.Rotate:
                    return CommonHelper.RotationCursor;
                default:
                    return Cursors.Arrow;
            }
        }

        public SelectedRect(DefaultDrawParam defaultDrawParam, SelectedType type) : base()
        {
            DrawParam = defaultDrawParam;
            currentDrawPointType = DrawPointType.Square;
            selectedType = type;
        }

        public void Draw()
        {
            Dispatcher.Invoke(() =>
            {
                Rect currentRect = SetDrawRect;
                drawDC = RenderOpen();
                switch (currentDrawMoveType)
                {
                    case DrawMoveType.kDefault:
                        currentRect = drawRect;
                        CalcControlPoint(currentRect);
                        break;
                    case DrawMoveType.kReferenceLine:
                        CalcControlPoint(currentRect);
                        if (isMouseDown == true)
                        {
                            SolidColorBrush moveBrush = DrawParam.AnnotMoveBrush;
                            Pen movepen = DrawParam.AnnotMovePen;

                            GetMoveBrushAndPen(ref moveBrush, ref movepen); 
                            if (selectedType == SelectedType.PDFEdit)
                            {
                                DrawMoveBounds(drawDC, hitControlType, movepen, moveBrush, drawRect, DrawParam.PDFEditMoveRectPen);
                            }
                            else
                            {
                                DrawMoveBounds(drawDC, hitControlType, movepen, moveBrush, drawRect);
                            }
                        }
                        break;
                    default:
                        break;
                }

                SolidColorBrush solidColorBrush = DrawParam.AnnotRectFillBrush;
                Pen pen = DrawParam.AnnotRectLinePen;
                GetBrushAndPen(ref solidColorBrush, ref pen);
                RotateTransform rotateTransform = new RotateTransform(rotateAngle, centerPoint.X, centerPoint.Y);
                drawDC.PushTransform(rotateTransform); 
                drawDC?.DrawRectangle(solidColorBrush, pen, currentRect);
                drawDC.Pop();
                SolidColorBrush PointBrush = DrawParam.AnnotPointBorderBrush;
                Pen PointPen = DrawParam.AnnotPointPen;
                GetPointBrushAndPen(ref PointBrush, ref PointPen);

                switch (currentDrawPointType)
                {
                    case DrawPointType.Circle:
                        if (selectedType == SelectedType.PDFEdit)
                        {
                            DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);  
                        }
                        else
                        {
                            DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                        }
                        break;
                    case DrawPointType.Square: 
                        DrawSquarePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                        break;
                    case DrawPointType.Crop:
                        DrawCropPoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                        break;
                }
                drawDC?.Close();
                drawDC = null;
            });
        }
          
        private void GetMoveBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            switch (selectedType)
            {
                case SelectedType.None:
                    break;
                case SelectedType.Annot:
                    colorBrush = DrawParam.AnnotMoveBrush;
                    pen = DrawParam.AnnotMovePen;
                    break; 
                case SelectedType.PDFEdit:
                    colorBrush = DrawParam.PDFEditMoveBrush;
                    pen = DrawParam.PDFEditMovePen;
                    break;
                default:
                    break;
            }
        }

        private void GetPointBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            switch (selectedType)
            {
                case SelectedType.None:
                    break;
                case SelectedType.Annot:
                    colorBrush = DrawParam.AnnotPointBorderBrush;
                    pen = DrawParam.AnnotPointPen;
                    break;
                case SelectedType.PDFEdit:
                    if (isHover)
                    {
                        colorBrush = DrawParam.PDFEditRectFillHoverBrush;
                        pen = DrawParam.PDFEditPointHoverPen;
                    }
                    else if (currentDrawPointType == DrawPointType.Crop)
                    {
                        colorBrush = DrawParam.SPDFEditCropBorderBrush; 
                        //new SolidColorBrush((DrawParam.SPDFEditPointPen.Brush as SolidColorBrush).Color);
                        pen = DrawParam.SPDFEditPointPen.Clone();
                        pen.DashStyle = DashStyles.Solid;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            colorBrush = DrawParam.SPDFEditPointBorderBrush;
                            pen = DrawParam.SPDFEditPointPen;
                        }
                        else
                        {
                            colorBrush = DrawParam.PDFEditPointBorderBrush;
                            pen = DrawParam.PDFEditPointPen;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void GetBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            switch (selectedType)
            {
                case SelectedType.None:
                    break;
                case SelectedType.Annot:
                    if (isHover)
                    {
                        colorBrush = DrawParam.AnnotRectFillBrush;
                        pen = DrawParam.AnnotRectHoverPen;
                    }
                    else
                    {
                        colorBrush = DrawParam.AnnotRectFillBrush;
                        pen = DrawParam.AnnotRectLinePen;
                    }
                    break;
                case SelectedType.PDFEdit:
                    if (isHover)
                    {
                        colorBrush = DrawParam.PDFEditRectFillHoverBrush;
                        pen = editHoverPen;//DrawParam.PDFEditRectLineHoverPen;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            colorBrush = DrawParam.SPDFEditRectFillBrush;
                            pen = DrawParam.SPDFEditRectLinePen;
                        }
                        else
                        {
                            colorBrush = DrawParam.PDFEditRectFillBrush;
                            //init Color
                            if (showCreatTextRect)
                            {
                                pen = DrawParam.PDFEditRectLinePen;
                            }
                            else
                            {
                                pen = editPen;
                            }
                            // editPen; //editPen;//// DrawParam.PDFEditRectLinePen; 
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public void SetShowCreatTextRect(bool ShowCreatTextRect)
        {
            showCreatTextRect = ShowCreatTextRect;
        }

        public void SetEditPen(Pen editPen = null, Pen editHoverPen = null)
        {
            if (editPen == null)
            {
                this.editPen = DrawParam.PDFEditRectLinePen;
            }
            else
            {
                this.editPen = new Pen(editPen.Brush, editPen.Thickness);
            }
            if (editHoverPen == null)
            {
                this.editHoverPen = DrawParam.PDFEditRectLineHoverPen;
            }
            else
            {
                this.editHoverPen = editHoverPen;
            }
        }

        public virtual void ClearDraw()
        {
            SetDrawRect = drawRect = new Rect();
            rotateAngle = 0;
            drawDC = RenderOpen();
            drawDC?.Close();
            drawDC = null;
        }

        /// <summary>
        /// Hide the drawing
        /// </summary>
        public virtual void HideDraw()
        {
            drawDC = RenderOpen();
            drawDC?.Close();
        }

        public void SetRect(Rect newRect, double zoom)
        {
            if (newRect == Rect.Empty || newRect == null)
            {
                return;
            }
            newRect = new Rect((newRect.X - rectPadding * zoom), (newRect.Y - rectPadding * zoom), (newRect.Width + 2 * rectPadding * zoom), (newRect.Height + 2 * rectPadding * zoom));
            currentZoom = zoom;
            SetDrawRect = drawRect = newRect;
            drawCenterPoint = new Point(drawRect.Left + drawRect.Width / 2, drawRect.Top + drawRect.Height / 2);
        }

        /// <summary>
        /// Get the original set Rect, not the calculated fill
        /// </summary>
        /// <param name="newRect">
        /// The new rect to set
        /// </param>
        public Rect GetRect() 
        {
            Rect rect = new Rect(drawRect.X + rectPadding * currentZoom, drawRect.Y + rectPadding * currentZoom, Math.Max(RectMinWidth, drawRect.Width - 2 * rectPadding * currentZoom), Math.Max(RectMinHeight, drawRect.Height - 2 * rectPadding * currentZoom));
            return rect;
        }

        public int GetRotation()
        {
            return rotateAngle;
        }

        public void SetRotation(int rotationAngle)
        {
            this.rotateAngle = rotationAngle;
        }

        public void SetRectPadding(double rectPadding)
        {
            this.rectPadding = rectPadding;
        }

        public double GetRectPadding()
        {
            return rectPadding;
        }

        public Rect GetDrawRect()
        {
            return drawRect;
        }

        /// <summary>
        /// Obtain cropped and actual region margin
        /// </summary>
        /// <returns></returns>
        public Thickness GetClipThickness()
        {
            return clipThickness;
        }

        /// <summary>
        /// Get ClipRect
        /// </summary>
        /// <returns></returns>
        public Rect GetClipRect()
        {
            Rect drawrect = new Rect(0, 0, 0, 0);
            drawrect.X = SetDrawRect.X - clipThickness.Left * currentZoom;
            drawrect.Y = SetDrawRect.Y - clipThickness.Top * currentZoom;
            drawrect.Width = SetDrawRect.Width - clipThickness.Right * currentZoom + clipThickness.Left * currentZoom;
            drawrect.Height = SetDrawRect.Height - clipThickness.Bottom * currentZoom + clipThickness.Top * currentZoom;
            return drawrect;
        }

        /// <summary>
        /// Set cropping and actual area margins
        /// </summary>
        /// <returns></returns>
        public void SetClipThickness(Thickness rect)
        {
            try
            {
                Rect drawrect = new Rect(0, 0, 0, 0);
                drawrect.X = SetDrawRect.X - rect.Left * currentZoom;
                drawrect.Y = SetDrawRect.Y - rect.Top * currentZoom;
                drawrect.Width = SetDrawRect.Width - rect.Right * currentZoom + rect.Left * currentZoom;
                drawrect.Height = SetDrawRect.Height - rect.Bottom * currentZoom + rect.Top * currentZoom;
                drawRect = drawrect;
                clipThickness = rect;
            }
            catch { }
        }

        public void SetMaxRect(Rect rect)
        {
            maxRect = rect;
        }

        public Rect GetMaxRect()
        {
            return maxRect;
        }

        public void SetAnnotData(AnnotData annotData, CPDFViewer viewer)
        {
            SetIgnorePoints(new List<PointControlType>());
            SetIsProportionalScaling(false);
            SetRoationHandle(false);
            isProportionalScaling = false;
            switch (annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                    DisableAll();
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    SetIgnorePointsAll();
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    SetIsProportionalScaling(true);
                    SetRoationHandle(true);
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    SetIgnorePointsAll();
                    break;

                default:
                    break;
            }

            SetMaxRect(annotData.PaintOffset);
            if(annotData.AnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_STAMP)
            {
                CRect sourceRect = new CRect();
                annotData.Annot.GetSourceRect(ref sourceRect);
                if (!sourceRect.IsEmpty)
                {
                    RenderData renderData = viewer.GetCurrentRenderPageForIndex(annotData.PageIndex);
                    Rect zoomRect = new Rect(sourceRect.left / 72 * 96 * annotData.CurrentZoom, sourceRect.top / 72 * 96 * annotData.CurrentZoom, sourceRect.width() / 72 * 96 * annotData.CurrentZoom, sourceRect.height() / 72 * 96 * annotData.CurrentZoom);
                    Rect rotateRect = zoomRect;
                    rotateRect.X += renderData.PageBound.X - renderData.CropLeft * annotData.CurrentZoom;
                    rotateRect.Y += renderData.PageBound.Y - renderData.CropTop * annotData.CurrentZoom;
                    SetRect(rotateRect, annotData.CurrentZoom);
                    rotateAngle = -(annotData.Annot as CPDFStampAnnotation).AnnotationRotator.GetRotation();
                    pageRotation = annotData.Annot.Page.Rotation;
                }
                else
                {
                    SetRect(annotData.PaintRect, annotData.CurrentZoom);
                }
            }
            else
            {
                SetRect(annotData.PaintRect, annotData.CurrentZoom);
            }

            //SetRotation(annotData.Rotation);
            selectedRectData = new SelectedAnnotData();
            selectedRectData.annotData = annotData;
        }

        private void SetRoationHandle(bool canRotate)
        {
            this.canRotate = canRotate;
        }

        public void SetIsProportionalScaling(bool isProportionalScaling)
        {
            this.isProportionalScaling = isProportionalScaling;
            ignorePoints.Clear();
            if (isProportionalScaling)
            {
                ignorePoints.Add(PointControlType.LeftMiddle);
                ignorePoints.Add(PointControlType.MiddleBottom);
                ignorePoints.Add(PointControlType.RightMiddle);
                ignorePoints.Add(PointControlType.MiddleTop);
                ignorePoints.Add(PointControlType.Rotate);
            } 
        }

        public void SetDrawType(DrawPointType drawType)
        {
            currentDrawPointType = drawType;
        }

        public void SetDrawMoveType(DrawMoveType drawType)
        {
            currentDrawMoveType = drawType;
        }

        /// <summary>
        /// Set the types that need to be ignored
        /// </summary>
        /// <param name="types">
        /// The collection of point types that need to be ignored
        /// </param>
        public void SetIgnorePoints(List<PointControlType> types)
        {
            ignorePoints.Clear();
            foreach (PointControlType type in types)
            {
                ignorePoints.Add(type);
            }
        }

        /// <summary>
        /// Set Edit that need to be ignored
        /// </summary>
        /// <param name="types">
        /// The collection of point types that need to be ignored
        /// </param>
        public void SetEditIgnorePoints(List<PointControlType> ignoreTextPoints, List<PointControlType> ignoreImagePoints, DrawPointType drawEditPointType, bool IsText = true)
        {
            SetCurrentDrawPointType(drawEditPointType);
            if (IsText)
            {
                ignorePoints.Clear();
                foreach (PointControlType type in ignoreTextPoints)
                {
                    ignorePoints.Add(type);
                }
            }
            else
            {
                ignorePoints.Clear();
                foreach (PointControlType type in ignoreImagePoints)
                {
                    ignorePoints.Add(type);
                }
            }
        }

        /// <summary>
        /// Ignore all points
        /// </summary>
        public void SetIgnorePointsAll()
        {
            ignorePoints.Clear();
            ignorePoints.Add(PointControlType.LeftTop);
            ignorePoints.Add(PointControlType.LeftMiddle);
            ignorePoints.Add(PointControlType.LeftBottom);
            ignorePoints.Add(PointControlType.MiddleBottom);
            ignorePoints.Add(PointControlType.RightBottom);
            ignorePoints.Add(PointControlType.RightMiddle);
            ignorePoints.Add(PointControlType.RightTop);
            ignorePoints.Add(PointControlType.MiddleTop);
        }

        /// <summary>
        /// Disable all functions
        /// </summary>
        public void DisableAll()
        {
            ignorePoints.Clear();
            ignorePoints.Add(PointControlType.LeftTop);
            ignorePoints.Add(PointControlType.LeftMiddle);
            ignorePoints.Add(PointControlType.LeftBottom);
            ignorePoints.Add(PointControlType.MiddleBottom);
            ignorePoints.Add(PointControlType.RightBottom);
            ignorePoints.Add(PointControlType.RightMiddle);
            ignorePoints.Add(PointControlType.RightTop);
            ignorePoints.Add(PointControlType.MiddleTop);
            ignorePoints.Add(PointControlType.Rotate);
            ignorePoints.Add(PointControlType.Body);
            ignorePoints.Add(PointControlType.Line);
        }

        /// <summary>
        /// Set the left alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignLeftForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignLeft(drawRect, maxRect));
        }

        /// <summary>
        /// Set horizontal center alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignHorizonCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignHorizonCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set horizontal right alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignRightForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignRight(drawRect, maxRect));
        }

        /// <summary>
        /// Set the top alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignTopForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignTop(drawRect, maxRect));
        }

        /// <summary>
        /// Set vertical center alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignVerticalCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignVerticalCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set vertical center alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignHorizonVerticalCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignHorizonVerticalCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set the bottom alignment in the set maximum rectangle
        /// </summary>
        public virtual void SetAlignBottomForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignBottom(drawRect, maxRect));
        }


        /// <summary>
        /// Get which control point the coordinate is on
        /// </summary>
        /// <param name="clickPoint">
        /// The point to check
        /// </param>
        /// <returns>
        /// The control point type
        /// </returns>
        public PointControlType GetHitControlIndex(Point point, bool isIgnore = true)
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

                    if (canRotate)
                    {
                        // Convert the rotation angle from degrees to radians
                        double angleRad = rotateAngle * Math.PI / 180.0;

                        // Calculate the sine and cosine of the angle
                        double cosAngle = Math.Cos(angleRad);
                        double sinAngle = Math.Sin(angleRad);

                        // Translate checkPoint to the origin (centerPoint becomes the origin)
                        double translatedX = checkPoint.X - centerPoint.X;
                        double translatedY = checkPoint.Y - centerPoint.Y;

                        // Apply the rotation matrix
                        double rotatedX = translatedX * cosAngle - translatedY * sinAngle;
                        double rotatedY = translatedX * sinAngle + translatedY * cosAngle;

                        // Translate the point back to its original position
                        checkPoint.X = rotatedX + centerPoint.X;
                        checkPoint.Y = rotatedY + centerPoint.Y;
                    }

                    if (isIgnore && IgnorePointsList.Contains(checkPoint))
                    {
                        continue;
                    }

                    switch (currentDrawPointType)
                    {
                        case DrawPointType.Circle:
                            if (IgnorePointsList.Contains(checkPoint))
                            {
                                continue;
                            }
                            Vector checkVector = checkPoint - point;
                            double wlen = drawRect.Width;
                            if (wlen > 50)
                            {
                                wlen = 20;
                            }
                            else
                            {
                                wlen = wlen / 3;
                            }
                            double hlen = drawRect.Height;
                            if (hlen > 50)
                            {
                                hlen = 20;
                            }
                            else
                            {
                                hlen = wlen / 3;
                            }
                            if ((PointControlType)i == PointControlType.RightMiddle)
                            {
                                if (Math.Abs(point.X - checkPoint.X) < wlen && checkVector.Length < drawRect.Height/3)
                                {
                                    return (PointControlType)i;
                                }
                            }
                            if ((PointControlType)i == PointControlType.LeftMiddle)
                            {
                                if (Math.Abs(point.X - checkPoint.X) < wlen && checkVector.Length < drawRect.Height/3)
                                {
                                    return (PointControlType)i;
                                }
                            }

                            if ((PointControlType)i == PointControlType.MiddleTop)
                            {
                                if (Math.Abs(point.Y - checkPoint.Y) < hlen && checkVector.Length < drawRect.Width/3)
                                {
                                    return (PointControlType)i;
                                }
                            }

                            if ((PointControlType)i == PointControlType.MiddleBottom)
                            {
                                if (Math.Abs(point.Y - checkPoint.Y) < hlen && checkVector.Length < drawRect.Width/3)
                                {
                                    return (PointControlType)i;
                                }
                            }
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

                            if (canRotate)
                            {
                                Point hitRotationPoint = new Point(centerPoint.X + (rotationPoint.X - centerPoint.X) * Math.Cos(rotateAngle * Math.PI / 180) - (rotationPoint.Y - centerPoint.Y) * Math.Sin(rotateAngle * Math.PI / 180),
                                    centerPoint.Y + (rotationPoint.X - centerPoint.X) * Math.Sin(rotateAngle * Math.PI / 180) + (rotationPoint.Y - centerPoint.Y) * Math.Cos(rotateAngle * Math.PI / 180));
                                Vector checkVector1 = point - hitRotationPoint;

                                if (checkVector1.Length < pointSize)
                                {
                                    return PointControlType.Rotate;
                                } 
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

                if (canRotate)
                {
                    bool isIn = DataConversionForWPF.IsPointInRotatedRectangle(point, drawRect, rotateAngle);
                    if(isIn)
                    {
                        double rectWidth = (drawRect.Width - 2 * rectPadding > 0) ? drawRect.Width - 2 * rectPadding : 0;
                        double rectHeight = (drawRect.Height - 2 * rectPadding > 0) ? drawRect.Height - 2 * rectPadding : 0;
                        Rect rect = new Rect(Math.Max(drawRect.X + rectPadding, 0), Math.Max(drawRect.Y + rectPadding, 0), rectWidth, rectHeight);
                        isIn = DataConversionForWPF.IsPointInRotatedRectangle(point, rect, rotateAngle);
                        if (isIn)
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
                else
                {
                    if (drawRect.Contains(point))
                    {
                        double rectWidth = (drawRect.Width - 2 * rectPadding > 0) ? drawRect.Width - 2 * rectPadding : 0;
                        double rectHeight = (drawRect.Height - 2 * rectPadding > 0) ? drawRect.Height - 2 * rectPadding : 0;
                        Rect rect = new Rect(Math.Max(drawRect.X + rectPadding, 0), Math.Max(drawRect.Y + rectPadding, 0), rectWidth, rectHeight);
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

            return PointControlType.None;
        }

        /// <summary>
        /// The position of the points in the cropping box
        /// </summary>
        /// <param name="point"></param>
        /// <param name="isIgnore"></param>
        /// <returns></returns>
        public PointControlType GetHitCropControlIndex(Point point, bool isIgnore = true)
        {
            List<Point> controlCurrentPoints = GetControlPoint(drawRect);
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            if (hitResult != null && hitResult.VisualHit is DrawingVisual)
            {
                List<PointControlType> ignoreList = GetIgnorePoints();

                List<Point> IgnorePointsList = new List<Point>();
                foreach (PointControlType type in ignoreList)
                {
                    if ((int)type < controlCurrentPoints.Count)
                    {
                        IgnorePointsList.Add(controlCurrentPoints[(int)type]);
                    }
                }
                for (int i = 0; i < controlCurrentPoints.Count; i++)
                {
                    Point checkPoint = controlCurrentPoints[i];

                    if (isIgnore && IgnorePointsList.Contains(checkPoint))
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
                    double rectWidth = (drawRect.Width - 2 * rectPadding > 0) ? drawRect.Width - 2 * rectPadding : 0;
                    double rectHeight = (drawRect.Height - 2 * rectPadding > 0) ? drawRect.Height - 2 * rectPadding : 0;
                    Rect rect = new Rect(Math.Max(drawRect.X + rectPadding, 0), Math.Max(drawRect.Y + rectPadding, 0), rectWidth, rectHeight);
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
            return PointControlType.None;
        }
    }
}
