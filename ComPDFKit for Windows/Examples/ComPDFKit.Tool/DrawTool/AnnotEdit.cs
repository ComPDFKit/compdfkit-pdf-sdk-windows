using ComPDFKit.Import;
using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    internal class AnnotEdit : DrawingVisual
    {
        #region public Attributes

        /// <summary>
        /// Data changing event
        /// </summary>
        public event EventHandler<SelectedAnnotData> DataChanging;

        /// <summary>
        /// Data changed event
        /// </summary>
        public event EventHandler<SelectedAnnotData> DataChanged;

        #endregion

        #region private Attributes

        protected DefaultDrawParam drawParam = new DefaultDrawParam();

        /// <summary>
        /// Max drawing range
        /// </summary>
        protected Rect maxRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Is mouse down
        /// </summary>
        protected bool isMouseDown { get; set; }

        protected DrawingContext drawDC
        {
            get;
            set;
        }

        /// <summary>
        /// Current annotation's control points collection (96 DPI coordinate points, minus page offset)
        /// </summary>
        private PointCollection activePoints { get; set; } = new PointCollection();

        /// <summary>
        /// Current annotation's drawing range box (96 DPI coordinate points, minus page offset)
        /// </summary>
        private Rect activeRect { get; set; } = new Rect();

        private int hitIndex = -1;

        protected AnnotData annotData { get; set; }

        /// <summary>
        /// Position information recorded when the mouse is pressed 
        /// </summary>
        protected Point mouseDownPoint { get; set; }

        /// <summary>
        /// Mouse last draw position information
        /// </summary>
        protected Point mouseEndDrawPoint { get; set; }

        /// <summary>
        /// Move offset during movement
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// Point size
        /// </summary>
        protected int pointSize { get; set; } = 6;

        protected SelectedAnnotData annotEditData = new SelectedAnnotData();

        //Measure
        private PointCollection leftLine { get; set; } = new PointCollection();
        private PointCollection rightLine { get; set; } = new PointCollection();
        private PointCollection crossLine { get; set; } = new PointCollection();

        private Point[] moveLeftLine;
        private Point[] moveRightLine;
        private Point[] moveCrossLine;

        #endregion

        /// <summary>
        /// Re-locate child elements
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
        public AnnotEdit(DefaultDrawParam defaultDrawParam) : base()
        {
            drawParam = defaultDrawParam;
        }

        public bool SetAnnotObject(AnnotData Data)
        {
            activePoints.Clear();
            if (Data == null)
            {
                return false;
            }
            annotData = Data;

            annotEditData = new SelectedAnnotData();
            annotEditData.annotData = annotData;

            SetMaxRect(annotData.PaintOffset);
            switch (annotData.Annot.Type)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_NONE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNKOWN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    for (int i = 0; i < (annotData.Annot as CPDFLineAnnotation).Points.Length; i++)
                    {
                        Point point = DpiHelper.PDFPointToStandardPoint(new Point((annotData.Annot as CPDFLineAnnotation).Points[i].x, (annotData.Annot as CPDFLineAnnotation).Points[i].y));
                        point = new Point(
                            point.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                            point.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom
                            );
                        activePoints.Add(point);
                    }
                    if ((annotData.Annot as CPDFLineAnnotation).IsMeasured())
                    {
                        CRect rawRect = annotData.Annot.GetRect();
                        Rect rect = DataConversionForWPF.CRectConversionForRect(rawRect);
                        rect = DpiHelper.PDFRectToStandardRect(rect);
                        activeRect = new Rect(
                            rect.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                            rect.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom,
                            rect.Width * annotData.CurrentZoom,
                            rect.Height * annotData.CurrentZoom
                            );
                        CalcMeasurePoints(annotData.Annot as CPDFLineAnnotation);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    {
                        for (int i = 0; i < (annotData.Annot as CPDFPolygonAnnotation).Points.Count; i++)
                        {
                            Point point = DpiHelper.PDFPointToStandardPoint(new Point((annotData.Annot as CPDFPolygonAnnotation).Points[i].x, (annotData.Annot as CPDFPolygonAnnotation).Points[i].y));
                            point = new Point(
                                point.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                                point.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom
                                );
                            activePoints.Add(point);
                        }

                        CRect rawRect = annotData.Annot.GetRect();
                        Rect rect = DataConversionForWPF.CRectConversionForRect(rawRect);
                        rect = DpiHelper.PDFRectToStandardRect(rect);
                        activeRect = new Rect(
                            rect.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                            rect.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom,
                            rect.Width * annotData.CurrentZoom,
                            rect.Height * annotData.CurrentZoom
                            );
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    {
                        for (int i = 0; i < (annotData.Annot as CPDFPolylineAnnotation).Points.Count; i++)
                        {
                            Point point = DpiHelper.PDFPointToStandardPoint(new Point((annotData.Annot as CPDFPolylineAnnotation).Points[i].x, (annotData.Annot as CPDFPolylineAnnotation).Points[i].y));
                            point = new Point(
                                point.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                                point.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom
                                );
                            activePoints.Add(point);
                        }

                        CRect rawRect = annotData.Annot.GetRect();
                        Rect rect = DataConversionForWPF.CRectConversionForRect(rawRect);
                        rect = DpiHelper.PDFRectToStandardRect(rect);
                        activeRect = new Rect(
                            rect.X * annotData.CurrentZoom + annotData.PaintOffset.X - annotData.CropLeft * annotData.CurrentZoom,
                            rect.Y * annotData.CurrentZoom + annotData.PaintOffset.Y - annotData.CropTop * annotData.CurrentZoom,
                            rect.Width * annotData.CurrentZoom,
                            rect.Height * annotData.CurrentZoom
                            );
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CARET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POPUP:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FILEATTACHMENT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SCREEN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_PRINTERMARK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TRAPNET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WATERMARK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_3D:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INTERCHANGE:
                    break;
                default:
                    break;
            }
            return true;
        }

        public void SetMaxRect(Rect rect)
        {
            maxRect = rect;
        }

        public int GetHitIndex(Point point)
        {
            for (int i = 0; i < activePoints.Count; i++)
            {
                Vector checkVector = activePoints[i] - point;
                if (checkVector.Length < pointSize)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Calculate the control points of the measurement annotation
        /// </summary>
        /// <param name="Annot">
        /// Measurement annotation object
        /// </param>
        private void CalcMeasurePoints(CPDFLineAnnotation Annot)
        {
            leftLine.Clear();
            crossLine.Clear();
            rightLine.Clear();
            CPDFDistanceMeasure LineMeasure = Annot.GetDistanceMeasure();
            double LeadLength = LineMeasure.GetLeadLength() / 72D * 96D;
            double LeadOffset = LineMeasure.GetLeadOffset() / 72D * 96D;
            double LeadExtension = LineMeasure.GetLeadExtension() / 72D * 96D;

            List<Point> orderPoints = activePoints.AsEnumerable().OrderBy(x => x.X).ToList();
            Vector lineVector = (activePoints[1] - activePoints[0]) * annotData.CurrentZoom;
            lineVector.Normalize();
            Vector leadEndVector = lineVector * (Math.Abs(LeadLength) + Math.Abs(LeadOffset) + Math.Abs(LeadExtension) * annotData.CurrentZoom);
            Vector leadStartVector = lineVector * (Math.Abs(LeadOffset)) * annotData.CurrentZoom;
            Vector leadCrossVector = lineVector * (Math.Abs(LeadLength) * annotData.CurrentZoom + Math.Abs(LeadOffset));
            Matrix rotateMatrix = new Matrix();
            double angle = LeadLength < 0 ? 90 : -90;
            rotateMatrix.Rotate(angle);
            Point leftEndPoint = rotateMatrix.Transform(leadEndVector) + activePoints[0];
            Point leftStartPoint = rotateMatrix.Transform(leadStartVector) + activePoints[0];
            Point leftCrossPoint = rotateMatrix.Transform(leadCrossVector) + activePoints[0];

            leftLine.Add(leftStartPoint);
            leftLine.Add(leftCrossPoint);
            crossLine.Add(leftCrossPoint);

            lineVector = activePoints[1] - activePoints[0];
            rightLine.Add(leftStartPoint + lineVector);
            rightLine.Add(leftCrossPoint + lineVector);
            crossLine.Add(leftCrossPoint + lineVector);
            activePoints.Insert(1, new Point(
                (crossLine[0].X + crossLine[1].X) / 2,
                (crossLine[0].Y + crossLine[1].Y) / 2
                ));
        }

        #region Event

        public virtual void OnMouseLeftButtonDown(Point downPoint)
        {
            isMouseDown = true;
            hitIndex = -1;
            mouseDownPoint = downPoint;
            moveOffset = new Point(0, 0);
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, downPoint);
            if (hitResult != null && hitResult.VisualHit is DrawingVisual)
            {
                hitIndex = GetHitIndex(downPoint);
            }
        }

        public virtual void OnMouseMove(Point mousePoint, out bool Tag)
        {
            Tag = false;
            if (isMouseDown)
            {
                Tag = isMouseDown;
                mouseEndDrawPoint = mousePoint;
                Point newOffset = new Point(
                    mouseEndDrawPoint.X - mouseDownPoint.X,
                    mouseEndDrawPoint.Y - mouseDownPoint.Y
                    );

                Point movePoint = CheckMoveOffSet(activePoints.ToList(), maxRect, newOffset);

                if (movePoint.X == 0)
                {
                    newOffset.X = moveOffset.X;
                }
                if (movePoint.Y == 0)
                {
                    newOffset.Y = moveOffset.Y;
                }
                moveOffset = newOffset;
                Draw();
                InvokeDataChangEvent(false);
            }
        }

        public virtual void OnMouseLeftButtonUp(Point upPoint)
        {
            if (annotData == null)
                return;

            isMouseDown = false;
            if (annotData.AnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
            {
                if ((annotData.Annot as CPDFLineAnnotation).IsMeasured())
                {
                    activePoints.Clear();
                    if (moveLeftLine == null)
                    {
                        moveLeftLine = leftLine.ToArray();
                    }

                    if (moveRightLine == null)
                    {
                        moveRightLine = rightLine.ToArray();
                    }

                    if (moveCrossLine == null)
                    {
                        moveCrossLine = crossLine.ToArray();
                    }

                    activePoints.Add(moveLeftLine[0]);
                    activePoints.Add(moveLeftLine[1]);
                    activePoints.Add(moveRightLine[0]);
                    activePoints.Add(moveRightLine[1]);
                    activePoints.Add(moveCrossLine[0]);
                    activePoints.Add(moveCrossLine[1]);
                }
            }

            moveLeftLine = null;
            moveRightLine = null;
            moveCrossLine = null;
            if (moveOffset != new Point(0, 0))
            {
                InvokeDataChangEvent(true);
            }

            moveOffset = new Point(0, 0);
            mouseDownPoint = new Point();
            mouseEndDrawPoint = new Point();
            hitIndex = -1;

            SetAnnotObject(annotData);
        }

        /// <summary>
        /// Used to notify events during/after drawing data
        /// </summary>
        /// <param name="isFinish">
        /// Is the data change complete
        /// </param>
        protected void InvokeDataChangEvent(bool isFinish)
        {
            PointCollection ActivePoints1 = new PointCollection();
            for (int i = 0; i < activePoints.Count; i++)
            {
                Point currentPoint = activePoints[i];
                if (hitIndex == -1)
                {
                    if (annotData.AnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                    {
                        if (!(annotData.Annot as CPDFLineAnnotation).IsMeasured())
                        {
                            currentPoint.X += moveOffset.X;
                            currentPoint.Y += moveOffset.Y;
                        }
                    }
                    else
                    {
                        currentPoint.X += moveOffset.X;
                        currentPoint.Y += moveOffset.Y;
                    }
                }
                ActivePoints1.Add(currentPoint);
            }
            annotEditData.Points = ActivePoints1;
            if (isFinish)
            {
                DataChanged?.Invoke(this, annotEditData);
            }
            else
            {
                DataChanging?.Invoke(this, annotEditData);
            }
        }

        #endregion

        #region Draw

        public void Draw()
        {
            Dispatcher.Invoke(() =>
            {
                if (annotData == null)
                {
                    return;
                }
                drawDC = RenderOpen();

                switch (annotData.Annot.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        {
                            if ((annotData.Annot as CPDFLineAnnotation).IsMeasured())
                            {
                                DrawLineMeasure(drawDC);
                            }
                            else
                            {
                                DrawLine(drawDC);
                            }
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                        DrawPolyLineMeasure(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                        DrawPolygonMeasure(drawDC);
                        break;
                    default:
                        break;
                }

                drawDC?.Close();
                drawDC = null;
            });
        }

        private Point CheckPointBound(Point checkPoint, Rect bound)
        {
            if (checkPoint.X < bound.Left)
            {
                checkPoint.X = bound.Left;
            }
            if (checkPoint.X > bound.Right)
            {
                checkPoint.X = bound.Right;
            }
            if (checkPoint.Y < bound.Top)
            {
                checkPoint.Y = bound.Top;
            }
            if (checkPoint.Y > bound.Bottom)
            {
                checkPoint.Y = bound.Bottom;
            }

            return checkPoint;
        }

        private Point CheckMoveOffSet(List<Point> checkPoints, Rect bound, Point moveOffset)
        {
            double left = 0;
            double top = 0;
            double right = 0;
            double bottom = 0;

            if (checkPoints != null && checkPoints.Count > 0)
            {
                left = checkPoints.AsEnumerable().Select(p => p.X).Min();
                right = checkPoints.AsEnumerable().Select(p => p.X).Max();

                top = checkPoints.AsEnumerable().Select(p => p.Y).Min();
                bottom = checkPoints.AsEnumerable().Select(p => p.Y).Max();
            }

            Point movePoint = moveOffset;

            if (left + moveOffset.X < bound.Left || right + moveOffset.X > bound.Right)
            {
                movePoint.X = 0;
            }

            if (top + moveOffset.Y < bound.Top || bottom + moveOffset.Y > bound.Bottom)
            {
                movePoint.Y = 0;
            }

            return movePoint;
        }

        private void DrawLine(DrawingContext drawingContext)
        {
            PathGeometry drawPath = new PathGeometry();
            PathFigure drawFigure = new PathFigure();

            PolyLineSegment polySegment = new PolyLineSegment();
            if (hitIndex != -1 && hitIndex < activePoints.Count)
            {
                if (mouseEndDrawPoint != new Point() && !activePoints.Contains(mouseEndDrawPoint))
                {
                    activePoints[hitIndex] = CheckPointBound(mouseEndDrawPoint, maxRect);
                }
            }

            Point StartPoint = activePoints[0];
            if (hitIndex == -1)
            {
                StartPoint.X += moveOffset.X;
                StartPoint.Y += moveOffset.Y;
            }

            drawFigure.StartPoint = StartPoint;
            for (int i = 1; i < activePoints.Count; i++)
            {
                Point currentPoint = activePoints[i];
                if (hitIndex == -1)
                {
                    currentPoint.X += moveOffset.X;
                    currentPoint.Y += moveOffset.Y;
                }
                polySegment.Points.Add(currentPoint);
            }

            if (polySegment.Points.Count > 0)
            {
                drawFigure.Segments.Add(polySegment);
            }

            if (drawFigure.Segments.Count > 0)
            {
                drawPath.Figures.Add(drawFigure);
            }

            foreach (Point controlPoint in activePoints)
            {
                Point drawPoint = new Point(
                    controlPoint.X,
                    controlPoint.Y);
                if (hitIndex == -1)
                {
                    drawPoint.X += moveOffset.X;
                    drawPoint.Y += moveOffset.Y;
                }
                drawingContext?.DrawEllipse(drawParam.EditControlLineBrush, drawParam.EditControlLinePen, (drawPoint), pointSize, pointSize);
            }
            if (isMouseDown)
            {
                drawingContext?.DrawGeometry(null, drawParam.EditLinePen, drawPath);
            }
        }

        private void DrawLineMeasure(DrawingContext drawingContext)
        {
            foreach (Point controlPoint in activePoints)
            {
                Point drawPoint = new Point(
                    controlPoint.X,
                    controlPoint.Y);
                if (hitIndex == -1)
                {
                    drawPoint.X += moveOffset.X;
                    drawPoint.Y += moveOffset.Y;
                }
                drawingContext?.DrawEllipse(drawParam.EditControlLineBrush, drawParam.EditControlLinePen, (drawPoint), pointSize, pointSize);
            }
            Rect drawRect = activeRect;
            drawRect.X += moveOffset.X;
            drawRect.Y += moveOffset.Y;
            if (isMouseDown)
            {
                PointCollection drawLeftPoints = new PointCollection();
                PointCollection drawRightPoints = new PointCollection();
                PointCollection drawCrossPoints = new PointCollection();
                moveLeftLine = leftLine.ToArray();
                moveRightLine = rightLine.ToArray();
                moveCrossLine = crossLine.ToArray();
                switch (hitIndex)
                {
                    case 0://Left
                        {
                            moveLeftLine[0].X += moveOffset.X;
                            moveLeftLine[0].Y += moveOffset.Y;

                            moveLeftLine[0].X = Math.Max(maxRect.Left, moveLeftLine[0].X);
                            moveLeftLine[0].X = Math.Min(maxRect.Right, moveLeftLine[0].X);
                            moveLeftLine[0].Y = Math.Max(maxRect.Top, moveLeftLine[0].Y);
                            moveLeftLine[0].Y = Math.Min(maxRect.Bottom, moveLeftLine[0].Y);

                            Vector newVector = moveLeftLine[0] - rightLine[0];
                            Vector leftVector = leftLine[1] - leftLine[0];

                            double angle = leftLine[0].Y < crossLine[0].Y ? -90 : 90;
                            newVector.Normalize();
                            newVector = newVector * leftVector.Length;
                            Matrix rotateMatrix = new Matrix();
                            rotateMatrix.Rotate(angle);
                            moveLeftLine[1] = moveLeftLine[0] + newVector * rotateMatrix;
                            moveRightLine[0] = rightLine[0];
                            moveRightLine[1] = moveRightLine[0] + newVector * rotateMatrix;
                            moveCrossLine[0] = moveLeftLine[1];
                            moveCrossLine[1] = moveRightLine[1];
                        }
                        break;
                    case 1:// Center
                        {
                            Point centerPoint = new Point(
                            (crossLine[0].X + crossLine[1].X) / 2,
                            (crossLine[0].Y + crossLine[1].Y) / 2
                            );
                            Point movePoint = new Point(centerPoint.X, centerPoint.Y);
                            movePoint.X += moveOffset.X;
                            movePoint.Y += moveOffset.Y;
                            Vector ruleVector = crossLine[1] - crossLine[0];

                            bool rateMove = true;
                            if (ruleVector.X == 0)
                            {
                                movePoint.Y = centerPoint.Y;
                                rateMove = false;
                            }
                            if (ruleVector.Y == 0)
                            {
                                movePoint.X = centerPoint.X;
                                rateMove = false;
                            }
                            if (rateMove)
                            {
                                Vector moveVector = movePoint - centerPoint;
                                double moveLength = moveVector.Length;
                                double ruleLength = ruleVector.Length;
                                ruleVector.Normalize();
                                moveVector.Normalize();
                                Vector crossVector = new Vector(-ruleVector.Y, ruleVector.X);
                                crossVector.Normalize();

                                if (Math.Abs(Vector.AngleBetween(moveVector, crossVector)) > 90)
                                {
                                    crossVector.Negate();
                                }
                                Point saveCenter = crossVector * moveLength + centerPoint;
                                double halfLenght = ruleLength / 2;
                                Point SaveRight = ruleVector * halfLenght + saveCenter;
                                Point saveLeft = saveCenter - ruleVector * halfLenght;

                                moveCrossLine[0] = saveLeft;
                                moveCrossLine[1] = SaveRight;
                                moveLeftLine[1] = saveLeft;
                                moveRightLine[1] = SaveRight;
                                moveLeftLine[0] = leftLine[0];
                                moveRightLine[0] = rightLine[0];
                            }
                            else
                            {
                                Point moveOffset = new Point(
                                    movePoint.X - centerPoint.X,
                                    movePoint.Y - centerPoint.Y);
                                moveCrossLine[0].X += moveOffset.X;
                                moveCrossLine[0].Y += moveOffset.Y;
                                moveCrossLine[1].X += moveOffset.X;
                                moveCrossLine[1].Y += moveOffset.Y;

                                moveLeftLine[1].X += moveOffset.X;
                                moveLeftLine[1].Y += moveOffset.Y;

                                moveRightLine[1].X += moveOffset.X;
                                moveRightLine[1].Y += moveOffset.Y;
                            }

                        }
                        break;
                    case 2://Right
                        {
                            moveRightLine[0].X += moveOffset.X;
                            moveRightLine[0].Y += moveOffset.Y;

                            Vector newVector = moveRightLine[0] - leftLine[0];
                            Vector leftVector = rightLine[1] - rightLine[0];

                            double angle = (rightLine[0].Y + leftLine[0].Y) / 2 > (crossLine[0].Y + crossLine[1].Y) / 2 ? -90 : 90;
                            newVector.Normalize();
                            newVector = newVector * leftVector.Length;
                            Matrix rotateMatrix = new Matrix();
                            rotateMatrix.Rotate(angle);
                            moveLeftLine[1] = moveLeftLine[0] + newVector * rotateMatrix;
                            moveLeftLine[0] = leftLine[0];
                            moveRightLine[1] = moveRightLine[0] + newVector * rotateMatrix;
                            moveCrossLine[0] = moveLeftLine[1];
                            moveCrossLine[1] = moveRightLine[1];
                        }
                        break;
                    case -1:
                        moveLeftLine[0].X += moveOffset.X;
                        moveLeftLine[0].Y += moveOffset.Y;
                        moveLeftLine[1].X += moveOffset.X;
                        moveLeftLine[1].Y += moveOffset.Y;

                        moveRightLine[0].X += moveOffset.X;
                        moveRightLine[0].Y += moveOffset.Y;
                        moveRightLine[1].X += moveOffset.X;
                        moveRightLine[1].Y += moveOffset.Y;

                        moveCrossLine[0] = moveLeftLine[1];
                        moveCrossLine[1] = moveRightLine[1];
                        break;
                    default:
                        break;
                }
                //Left
                drawLeftPoints.Add(new Point(
                    moveLeftLine[0].X,
                    moveLeftLine[0].Y));
                drawLeftPoints.Add(new Point(
                   moveLeftLine[1].X,
                   moveLeftLine[1].Y));

                //Right
                drawRightPoints.Add(new Point(
                    moveRightLine[0].X,
                    moveRightLine[0].Y));
                drawRightPoints.Add(new Point(
                   moveRightLine[1].X,
                   moveRightLine[1].Y));

                //Middle
                drawCrossPoints.Add(new Point(
                    moveCrossLine[0].X,
                    moveCrossLine[0].Y));

                drawCrossPoints.Add(new Point(
                    moveCrossLine[1].X,
                    moveCrossLine[1].Y));


                drawingContext?.DrawLine(drawParam.EditLinePen, drawLeftPoints[0], drawLeftPoints[1]);
                drawingContext?.DrawLine(drawParam.EditLinePen, drawRightPoints[0], drawRightPoints[1]);
                drawingContext?.DrawLine(drawParam.EditLinePen, drawCrossPoints[0], drawCrossPoints[1]);
            }
            else
            {
                drawingContext?.DrawRectangle(null, drawParam.EditLinePen, drawRect);
            }
        }

        private void DrawPolyLineMeasure(DrawingContext drawingContext)
        {
            PathGeometry drawPath = new PathGeometry();
            PathFigure drawFigure = new PathFigure();

            PolyLineSegment polySegment = new PolyLineSegment();
            if (hitIndex != -1 && hitIndex < activePoints.Count)
            {
                if (mouseEndDrawPoint != new Point() && !activePoints.Contains(mouseEndDrawPoint))
                {
                    activePoints[hitIndex] = CheckPointBound(mouseEndDrawPoint, maxRect);
                }
            }


            Point StartPoint = activePoints[0];
            if (hitIndex == -1)
            {
                StartPoint.X += moveOffset.X;
                StartPoint.Y += moveOffset.Y;
            }
            drawFigure.StartPoint = StartPoint;
            for (int i = 1; i < activePoints.Count; i++)
            {
                Point currentPoint = activePoints[i];
                if (hitIndex == -1)
                {
                    currentPoint.X += moveOffset.X;
                    currentPoint.Y += moveOffset.Y;
                }
                polySegment.Points.Add(currentPoint);
            }

            if (polySegment.Points.Count > 0)
            {
                drawFigure.Segments.Add(polySegment);
            }

            if (drawFigure.Segments.Count > 0)
            {
                drawPath.Figures.Add(drawFigure);
            }
            foreach (Point controlPoint in activePoints)
            {
                Point drawPoint = new Point(
                    controlPoint.X,
                    controlPoint.Y);
                if (hitIndex == -1)
                {
                    drawPoint.X += moveOffset.X;
                    drawPoint.Y += moveOffset.Y;
                }
                drawingContext?.DrawEllipse(drawParam.EditControlLineBrush, drawParam.EditControlLinePen, (drawPoint), pointSize, pointSize);
            }
            Rect drawRect = activeRect;
            drawRect.X += moveOffset.X;
            drawRect.Y += moveOffset.Y;
            if (isMouseDown)
            {
                drawingContext?.DrawGeometry(null, drawParam.EditLinePen, drawPath);
            }
            else
            {
                drawingContext?.DrawRectangle(null, drawParam.EditLinePen, drawRect);
            }
        }

        private void DrawPolygonMeasure(DrawingContext drawingContext)
        {
            PathGeometry drawPath = new PathGeometry();
            PathFigure drawFigure = new PathFigure();

            PolyLineSegment polySegment = new PolyLineSegment();

            if (hitIndex != -1 && hitIndex < activePoints.Count)
            {
                if (mouseEndDrawPoint != new Point() && !activePoints.Contains(mouseEndDrawPoint))
                {
                    activePoints[hitIndex] = CheckPointBound(mouseEndDrawPoint, maxRect);
                }
            }

            Point StartPoint = activePoints[0];
            if (hitIndex == -1)
            {
                StartPoint.X += moveOffset.X;
                StartPoint.Y += moveOffset.Y;
            }
            drawFigure.StartPoint = StartPoint;
            for (int i = 1; i < activePoints.Count; i++)
            {
                Point currentPoint = activePoints[i];
                if (hitIndex == -1)
                {
                    currentPoint.X += moveOffset.X;
                    currentPoint.Y += moveOffset.Y;
                }
                polySegment.Points.Add(currentPoint);
            }

            if (polySegment.Points.Count > 0)
            {
                polySegment.Points.Add(drawFigure.StartPoint);
                drawFigure.Segments.Add(polySegment);
            }

            if (drawFigure.Segments.Count > 0)
            {
                drawPath.Figures.Add(drawFigure);
            }
            foreach (Point controlPoint in activePoints)
            {
                Point drawPoint = new Point(
                    controlPoint.X,
                    controlPoint.Y);
                if (hitIndex == -1)
                {
                    drawPoint.X += moveOffset.X;
                    drawPoint.Y += moveOffset.Y;
                }
                drawingContext?.DrawEllipse(drawParam.EditControlLineBrush, drawParam.EditControlLinePen, (drawPoint), pointSize, pointSize);
            }
            Rect drawRect = activeRect;
            drawRect.X += moveOffset.X;
            drawRect.Y += moveOffset.Y;
            if (isMouseDown)
            {
                drawingContext?.DrawGeometry(null, drawParam.EditLinePen, drawPath);
            }
            else
            {
                drawingContext?.DrawRectangle(null, drawParam.EditLinePen, drawRect);
            }
        }

        int i = 0;
        public virtual void ClearDraw()
        {
            drawDC = RenderOpen(); 
            drawDC?.Close();
            drawDC = null;
        }

        #endregion
    }
}
