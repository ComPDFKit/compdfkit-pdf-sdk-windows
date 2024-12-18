using ComPDFKit.Import;
using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.DrawTool
{
    internal class CreateAnnotTool : CustomizeLayer
    {
        public event EventHandler<bool> UpdateAnnotHandler;
        public event EventHandler<AnnotParam> CreateFreetextCanceled;
        public event EventHandler<MeasureEventArgs> MeasureChanged;

        public static DependencyProperty PopupTextAttachDataProperty = DependencyProperty.Register("PopupTextAttachData", typeof(CPDFAnnotation), typeof(CPDFViewerTool));

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
        protected bool isDrawAnnot { get; set; }

        /// <summary>   
        /// Current zoom factor
        /// </summary>
        private double zoomFactor { get; set; } = 1;

        /// <summary>
        /// Draw rectangle
        /// </summary>
        protected Rect drawRect { get; set; }

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

        protected DrawingContext drawDC { get; set; }

        protected CPDFAnnotation cPDFAnnotation
        {
            get;
            set;
        }

        protected CPDFViewer cPDFViewer { get; set; }

        protected List<Point> inkDrawPoints = new List<Point>();

        /// <summary>
        /// The collection of points measured for annotation drawing
        /// </summary>
        protected PointCollection drawPoints { get; set; } = new PointCollection();

        protected double textPadding { get; set; } = 10;

        protected Border lastTextBorder;

        protected TextBox lastTextui;

        protected Point freeTextPoint { get; set; }

        protected MeasureSetting measureSetting = new MeasureSetting();

        protected DefaultDrawParam defaultDrawParam = new DefaultDrawParam();

        protected DefaultSettingParam defaultSettingParam = new DefaultSettingParam();

        #endregion

        public CreateAnnotTool(MeasureSetting setting, DefaultDrawParam drawParam, DefaultSettingParam settingParam)
        {
            measureSetting = setting;
            defaultDrawParam = drawParam;
            defaultSettingParam = settingParam;
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

        public List<Point> GetInkDrawPoints()
        {
            List<Point> points = new List<Point>
            {
                new Point((mouseStartPoint.X - pageBound.Left+(cropPoint.X*zoomFactor)) / zoomFactor,
                    (mouseStartPoint.Y - pageBound.Top + (cropPoint.Y*zoomFactor)) / zoomFactor)
            };
            foreach (Point item in inkDrawPoints)
            {
                points.Add(new Point((item.X - pageBound.Left + (cropPoint.X * zoomFactor)) / zoomFactor,
                        (item.Y - pageBound.Top + (cropPoint.Y * zoomFactor)) / zoomFactor));
            }
            return points;
        }

        public List<Point> GetMeasureDrawPoints()
        {
            List<Point> points = new List<Point>();
            foreach (Point item in drawPoints)
            {
                points.Add(new Point((item.X - pageBound.Left + (cropPoint.X * zoomFactor)) / zoomFactor,
                        (item.Y - pageBound.Top + (cropPoint.Y * zoomFactor)) / zoomFactor));
            }
            if (defaultSettingParam.IsCreateSquarePolygonMeasure)
            {
                if (points.Count == 2)
                {
                    Rect rect = new Rect(points[0], points[1]);
                    points.Clear();
                    points.Add(rect.TopLeft);
                    points.Add(rect.BottomLeft);
                    points.Add(rect.BottomRight);
                    points.Add(rect.TopRight);
                }
                else if (points.Count == 1)
                {
                    Rect checkRect = pageBound;

                    // Current drawing during the movement process.
                    Point checkPoint = mouseEndPoint;
                    checkPoint.X = Math.Max(checkRect.Left, checkPoint.X);
                    checkPoint.X = Math.Min(checkRect.Right, checkPoint.X);
                    checkPoint.Y = Math.Max(checkRect.Top, checkPoint.Y);
                    checkPoint.Y = Math.Min(checkRect.Bottom, checkPoint.Y);

                    List<Point> drawPointsList = new List<Point>
                    {
                        new Point((drawPoints[0].X - pageBound.Left + (cropPoint.X * zoomFactor)) / zoomFactor,
                            (drawPoints[0].Y - pageBound.Top + (cropPoint.Y * zoomFactor)) / zoomFactor),
                        new Point((checkPoint.X - pageBound.Left + (cropPoint.X * zoomFactor)) / zoomFactor,
                            (checkPoint.Y - pageBound.Top + (cropPoint.Y * zoomFactor)) / zoomFactor)
                    };

                    Rect rect = new Rect(drawPointsList[0], drawPointsList[1]);
                    points.Clear();
                    points.Add(rect.TopLeft);
                    points.Add(rect.BottomLeft);
                    points.Add(rect.BottomRight);
                    points.Add(rect.TopRight);
                }
            }
            return points;
        }

        public void SetIsProportionalScaling(bool isOpen)
        {
            isProportionalScaling = isOpen;
        }

        #region Draw
        public CPDFAnnotation StartDraw(Point downPoint, Point cropPoint, CPDFPage cPDFPage, Rect maxRect, Rect pageBound, C_ANNOTATION_TYPE annotType, CPDFViewer viewer, double zoom)
        {
            RemoveTextBox();
            mouseStartPoint = downPoint;
            mouseEndPoint = downPoint;
            isDrawAnnot = true;
            this.maxRect = maxRect;
            zoomFactor = zoom;
            moveOffset = new Point();
            int newIndex = cPDFPage.GetAnnotCount();
            cPDFAnnotation = cPDFPage.CreateAnnot(annotType);
            if (cPDFAnnotation != null)
            {
                cPDFAnnotation.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                cPDFAnnotation.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                List<CPDFAnnotation> annotList = cPDFPage.GetAnnotations();
                cPDFAnnotation = annotList[newIndex];
                cPDFViewer = viewer;
                drawPoints.Add(downPoint);
                this.cropPoint = cropPoint;
                this.pageBound = pageBound;
                DPIRect = new Rect();
                if (annotType != C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON)
                {
                    defaultSettingParam.IsCreateSquarePolygonMeasure = false;
                }
            }
            return cPDFAnnotation;
        }

        public void MoveDraw(Point downPoint, double zoom)
        {
            if (isDrawAnnot)
            {
                moveOffset = new Point(
                    mouseEndPoint.X - downPoint.X,
                    mouseEndPoint.Y - downPoint.Y
                    );
                mouseEndPoint = downPoint;
                zoomFactor = zoom;
                Draw();
            }
        }

        public Rect EndDraw()
        {
            if (isDrawAnnot)
            {
                if (cPDFAnnotation != null && cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_TEXT)
                {
                    if (DPIRect.Equals(new Rect()))
                    {
                        DPIRect = drawRect = new Rect(mouseStartPoint.X, mouseStartPoint.Y, 32 * zoomFactor, 32 * zoomFactor);
                    }
                    else
                    {
                        DPIRect = drawRect = new Rect(mouseEndPoint.X, mouseEndPoint.Y, 32 * zoomFactor, 32 * zoomFactor);
                    }
                }

                if (cPDFAnnotation is CPDFPolylineAnnotation)
                {
                    double left = drawPoints.AsEnumerable().Select(x => x.X).Min();
                    double right = drawPoints.AsEnumerable().Select(x => x.X).Max();
                    double top = drawPoints.AsEnumerable().Select(x => x.Y).Min();
                    double bottom = drawPoints.AsEnumerable().Select(x => x.Y).Max();
                    DPIRect = new Rect(left, top, right - left, bottom - top);
                }

                Rect standardRect = new Rect(
                        (DPIRect.Left - pageBound.X + (cropPoint.X * zoomFactor)) / zoomFactor, (DPIRect.Top - pageBound.Y + (cropPoint.Y * zoomFactor)) / zoomFactor,
                        DPIRect.Width / zoomFactor, DPIRect.Height / zoomFactor);
                isDrawAnnot = false;
                freeTextPoint = new Point((mouseStartPoint.X - pageBound.X) / zoomFactor, (mouseStartPoint.Y - pageBound.Y) / zoomFactor);
                mouseStartPoint = new Point();
                mouseEndPoint = new Point();
                moveOffset = new Point();
                pageBound = new Rect();
                DPIRect = new Rect();
                cPDFAnnotation = null;
                inkDrawPoints.Clear();
                drawPoints.Clear();
                return DpiHelper.StandardRectToPDFRect(standardRect);
            }
            return new Rect();
        }

        public override void Draw()
        {
            Dispatcher.Invoke(() =>
            {
                if (cPDFAnnotation == null)
                {
                    return;
                }

                drawDC = Open();

                switch (cPDFAnnotation.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                        DPIRect = drawRect = new Rect(mouseStartPoint.X, mouseStartPoint.Y, 32, 32);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                        DrawText();
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        if ((cPDFAnnotation as CPDFLineAnnotation).IsMeasured())
                        {
                            DrawLineMeasure(drawDC);
                        }
                        else
                        {
                            DrawLine(drawDC);
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                        DrawSquare(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                        DrawCircle(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                        DrawInk(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                        DrawLink(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                        DrawPolygonMeasure(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                        DrawPolyLineMeasure(drawDC);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                        DrawRedact(drawDC);
                        break;
                    default:
                        break;
                }
                Present();
            });
        }

        public virtual void ClearDraw()
        {
            RemoveTextBox();
            Open();
            Present();
        }

        private void DrawCircle(DrawingContext drawingContext)
        {
            CPDFCircleAnnotation circleAnnot = (cPDFAnnotation as CPDFCircleAnnotation);
            Pen DrawPen = new Pen(new SolidColorBrush(Color.FromRgb(circleAnnot.LineColor[0], circleAnnot.LineColor[1], circleAnnot.LineColor[2])), (int)Math.Ceiling(circleAnnot.LineWidth / 72 * 96 * zoomFactor));

            SolidColorBrush FillBrush = new SolidColorBrush(Colors.Transparent);
            if (circleAnnot.HasBgColor)
            {
                FillBrush = new SolidColorBrush(Color.FromRgb(circleAnnot.BgColor[0], circleAnnot.BgColor[1], circleAnnot.BgColor[2]));
            }

            DrawPen.Brush.Opacity = circleAnnot.Transparency / 255D;
            FillBrush.Opacity = circleAnnot.Transparency / 255D;

            if (circleAnnot.Dash != null && circleAnnot.Dash.Length > 0 && circleAnnot.LineWidth > 0)
            {
                DashStyle dash = new DashStyle();
                foreach (var offset in circleAnnot.Dash)
                {
                    dash.Dashes.Add(offset / circleAnnot.LineWidth);
                }
                DrawPen.DashStyle = dash;
                DrawPen.DashCap = PenLineCap.Flat;
            }

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
            DPIRect = drawRect = new Rect(mLeft, mUp, mRight - mLeft, mDown - mUp);

            double centerX = (drawRect.Left + drawRect.Width / 2);
            double centerY = (drawRect.Top + drawRect.Height / 2);
            double radiusX = drawRect.Width / 2 - DrawPen.Thickness;
            double radiusY = drawRect.Height / 2 - DrawPen.Thickness;
            if (radiusX <= 0 || radiusY <= 0)
            {
                drawingContext.DrawEllipse(DrawPen.Brush, null, new Point(centerX, centerY), (drawRect.Width / 2), (drawRect.Height / 2));
            }
            else
            {
                drawingContext?.DrawEllipse(null, DrawPen, new Point(centerX, centerY), radiusX, radiusY);

                if ((int)(drawRect.Width / 2 - DrawPen.Thickness) > 0 && (int)(drawRect.Height / 2 - DrawPen.Thickness) > 0)
                {
                    drawingContext?.DrawEllipse(FillBrush, null, new Point(centerX, centerY), (drawRect.Width / 2 - DrawPen.Thickness), (drawRect.Height / 2 - DrawPen.Thickness));
                }
            }
        }

        private void DrawSquare(DrawingContext drawingContext)
        {
            CPDFSquareAnnotation squareAnnot = (cPDFAnnotation as CPDFSquareAnnotation);
            Pen DrawPen = new Pen(new SolidColorBrush(Color.FromRgb(squareAnnot.LineColor[0], squareAnnot.LineColor[1], squareAnnot.LineColor[2])), (int)Math.Ceiling(squareAnnot.LineWidth / 72 * 96 * zoomFactor));

            SolidColorBrush FillBrush = new SolidColorBrush(Colors.Transparent);
            if (squareAnnot.HasBgColor)
            {
                FillBrush = new SolidColorBrush(Color.FromRgb(squareAnnot.BgColor[0], squareAnnot.BgColor[1], squareAnnot.BgColor[2]));
            }

            DrawPen.Brush.Opacity = squareAnnot.Transparency / 255D;
            FillBrush.Opacity = squareAnnot.Transparency / 255D;

            if (squareAnnot.Dash != null && squareAnnot.Dash.Length > 0 && squareAnnot.LineWidth > 0)
            {
                DashStyle dash = new DashStyle();
                foreach (var offset in squareAnnot.Dash)
                {
                    dash.Dashes.Add(offset / squareAnnot.LineWidth);
                }
                DrawPen.DashStyle = dash;
                DrawPen.DashCap = PenLineCap.Flat;
            }

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
        }

        private void DrawLine(DrawingContext drawingContext)
        {
            CPDFLineAnnotation annotLine = (cPDFAnnotation as CPDFLineAnnotation);
            Pen DrawPen = new Pen(new SolidColorBrush(
                Color.FromRgb(
                    annotLine.LineColor[0],
                    annotLine.LineColor[1],
                    annotLine.LineColor[2])),
                (int)Math.Ceiling(annotLine.LineWidth == 0 ? 0.5 : annotLine.LineWidth * zoomFactor));

            DrawPen.Brush.Opacity = annotLine.Transparency / 255D;

            if (annotLine.Dash != null && annotLine.Dash.Length > 0 && annotLine.LineWidth > 0)
            {
                DashStyle dash = new DashStyle();
                foreach (var offset in annotLine.Dash)
                {
                    dash.Dashes.Add(offset / annotLine.LineWidth);
                }
                DrawPen.DashStyle = dash;
                DrawPen.DashCap = PenLineCap.Flat;
            }

            ArrowHelper drawLine = new ArrowHelper();

            if (isProportionalScaling)
            {
                mouseEndPoint = CalcAnglePoint(mouseEndPoint, mouseStartPoint, pageBound);
                DPIRect = new Rect(mouseStartPoint, mouseEndPoint);
                drawLine.LineStart = mouseStartPoint;
                drawLine.LineEnd = mouseEndPoint;
                drawLine.ArrowLength = (uint)Math.Max(DrawPen.Thickness * 3, 12 * zoomFactor);
                drawLine.StartSharp = annotLine.HeadLineType;
                drawLine.EndSharp = annotLine.TailLineType;
                drawingContext.DrawGeometry(null, DrawPen, drawLine.BuildArrowBody());
            }
            else
            {
                Point checkPoint = mouseEndPoint;
                if (mouseEndPoint.X < maxRect.Left)
                {
                    checkPoint.X = maxRect.Left;
                }
                if (mouseEndPoint.X > maxRect.Right)
                {
                    checkPoint.X = maxRect.Right;
                }
                if (mouseEndPoint.Y < maxRect.Top)
                {
                    checkPoint.Y = maxRect.Top;
                }
                if (mouseEndPoint.Y > maxRect.Bottom)
                {
                    checkPoint.Y = maxRect.Bottom;
                }
                mouseEndPoint = checkPoint;
                DPIRect = new Rect(mouseStartPoint, mouseEndPoint);
                drawLine.LineStart = mouseStartPoint;
                drawLine.LineEnd = mouseEndPoint;
                drawLine.ArrowLength = (uint)Math.Max(DrawPen.Thickness * 3, 12 * zoomFactor);
                drawLine.StartSharp = annotLine.HeadLineType;
                drawLine.EndSharp = annotLine.TailLineType;
                drawingContext.DrawGeometry(null, DrawPen, drawLine.BuildArrowBody());
            }
        }

        private void DrawInk(DrawingContext drawingContext)
        {
            CPDFInkAnnotation annotLine = (cPDFAnnotation as CPDFInkAnnotation);
            if (annotLine == null || annotLine.IsValid() == false)
            {
                return;
            }
            byte transparent = annotLine.GetTransparency();
            Pen DrawPen = new Pen(new SolidColorBrush(Color.FromArgb(transparent, annotLine.InkColor[0], annotLine.InkColor[1], annotLine.InkColor[2])), annotLine.Thickness * zoomFactor);
            DrawPen.StartLineCap = PenLineCap.Round;
            DrawPen.EndLineCap = PenLineCap.Round;
            PathGeometry pathDraw = new PathGeometry();
            Point CurrentPoint = mouseEndPoint;
            Rect MaxRect = pageBound;
            if (CurrentPoint.X > MaxRect.Right)
            {
                CurrentPoint.X = MaxRect.Right;
            }
            if (CurrentPoint.X < MaxRect.Left)
            {
                CurrentPoint.X = MaxRect.Left;
            }
            if (CurrentPoint.Y > MaxRect.Bottom)
            {
                CurrentPoint.Y = MaxRect.Bottom;
            }
            if (CurrentPoint.Y < MaxRect.Top)
            {
                CurrentPoint.Y = MaxRect.Top;
            }
            inkDrawPoints.Add(CurrentPoint);
            pathDraw.Figures = new PathFigureCollection();
            PathFigure pathFigure = new PathFigure();
            pathDraw.Figures.Add(pathFigure);
            pathFigure.StartPoint = mouseStartPoint;
            foreach (Point addPoint in inkDrawPoints)
            {
                LineSegment lineSegment = new LineSegment(addPoint, true);
                lineSegment.IsSmoothJoin = true;
                pathFigure.Segments.Add(lineSegment);
            }
            if (annotLine.Dash != null && annotLine.Dash.Length > 0)
            {
                DashStyle dash = new DashStyle();
                foreach (var offset in annotLine.Dash)
                {
                    dash.Dashes.Add(offset);
                }
                DrawPen.DashStyle = dash;
                DrawPen.DashCap = PenLineCap.Flat;
            }
            Rect checkRect = pageBound;
            RectangleGeometry rectGeometry = new RectangleGeometry();
            drawRect = rectGeometry.Rect = checkRect;
            drawingContext?.PushClip(rectGeometry);
            drawingContext?.DrawGeometry(null, DrawPen, pathDraw);
        }

        private void DrawLink(DrawingContext drawingContext)
        {
            Pen DrawPen = defaultDrawParam.LinkPen;
            DrawPen.Thickness *= zoomFactor;

            SolidColorBrush FillBrush = defaultDrawParam.LinkBrush;

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
        }

        private void DrawRedact(DrawingContext drawingContext)
        {
            Pen DrawPen = defaultDrawParam.RedactPen;

            if (cPDFAnnotation != null && cPDFAnnotation.IsValid() && cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_REDACT)
            {
                CPDFRedactAnnotation redactAnnot = cPDFAnnotation as CPDFRedactAnnotation;
                if (redactAnnot.OutlineColor != null && redactAnnot.OutlineColor.Length == 3)
                {
                    DrawPen = new Pen(new SolidColorBrush(Color.FromRgb(redactAnnot.OutlineColor[0], redactAnnot.OutlineColor[1], redactAnnot.OutlineColor[2])), DrawPen.Thickness);
                }
            }
            SolidColorBrush FillBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));

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
        }

        private void DrawPolyLineMeasure(DrawingContext drawingContext)
        {
            CPDFPolylineAnnotation polyLine = (cPDFAnnotation as CPDFPolylineAnnotation);
            byte[] bytes = polyLine.LineColor;
            Color color = ParamConverter.ConverterByteForColor(bytes);
            color.A = polyLine.GetTransparency();
            Pen DrawPen = new Pen(new SolidColorBrush(color), polyLine.GetBorderWidth());
            SolidColorBrush TextBrush = Brushes.Red;

            if (polyLine.IsMeasured())
            {
                CPDFPerimeterMeasure measureInfo = polyLine.GetPerimeterMeasure();
                if (measureInfo != null && measureInfo.TextAttribute != null && measureInfo.TextAttribute.FontColor != null && measureInfo.TextAttribute.FontColor.Length >= 3)
                {
                    byte[] fontColor = measureInfo.TextAttribute.FontColor;
                    TextBrush = new SolidColorBrush(Color.FromRgb(fontColor[0], fontColor[1], fontColor[2]));
                }

                if (polyLine.Dash != null && polyLine.Dash.Length > 0)
                {
                    DashStyle dash = new DashStyle();
                    foreach (var offset in polyLine.Dash)
                    {
                        dash.Dashes.Add(offset / polyLine.LineWidth);
                    }
                    DrawPen.DashStyle = dash;
                    DrawPen.DashCap = PenLineCap.Flat;
                }
            }

            if (isProportionalScaling)
            {
                if (drawPoints != null && drawPoints.Count > 0)
                {
                    mouseEndPoint = CalcAnglePoint(mouseEndPoint, drawPoints[drawPoints.Count - 1], pageBound);
                }
            }
            Point checkPoint = mouseEndPoint;
            checkPoint.X = Math.Max(pageBound.Left, checkPoint.X);
            checkPoint.X = Math.Min(pageBound.Right, checkPoint.X);
            checkPoint.Y = Math.Max(pageBound.Top, checkPoint.Y);
            checkPoint.Y = Math.Min(pageBound.Bottom, checkPoint.Y);

            if (drawPoints.Count > 0)
            {
                PathGeometry drawPath = new PathGeometry();
                PathFigure drawFigure = new PathFigure();

                drawFigure.StartPoint = drawPoints[0];
                PolyLineSegment polySegment = new PolyLineSegment();

                for (int i = 1; i < drawPoints.Count; i++)
                {
                    polySegment.Points.Add(drawPoints[i]);
                }
                polySegment.Points.Add(checkPoint);
                if (polySegment.Points.Count > 0)
                {
                    drawFigure.Segments.Add(polySegment);
                }
                if (drawFigure.Segments.Count > 0)
                {
                    drawPath.Figures.Add(drawFigure);
                }
                double totalInch = 0;
                if (drawPoints.Count > 1)
                {
                    for (int i = 0; i < drawPoints.Count - 1; i++)
                    {
                        totalInch += measureSetting.GetMeasureLength(drawPoints[i], drawPoints[i + 1], zoomFactor);
                    }
                }
                double currentInch = measureSetting.GetMeasureLength(drawPoints[drawPoints.Count - 1], checkPoint, zoomFactor);
                totalInch += currentInch;

                drawingContext?.DrawGeometry(null, DrawPen, drawPath);
                Point closePoint = drawPoints[drawPoints.Count - 1];
                Vector movevector = checkPoint - closePoint;

                FormattedText moveText = new FormattedText(
                    string.Format("{0} {1}", measureSetting.GetPrecisionData(currentInch), measureSetting.RulerTranslateUnit),
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("YaHei"),
                    16,
                    TextBrush);

                FormattedText totalText = new FormattedText(
                   string.Format("{0} {1}", measureSetting.GetPrecisionData(totalInch), measureSetting.RulerTranslateUnit),
                   CultureInfo.GetCultureInfo("en-us"),
                   FlowDirection.LeftToRight,
                   new Typeface("YaHei"),
                   16,
                   TextBrush);

                if (movevector.Length > moveText.Width + textPadding)
                {
                    if (checkPoint.X >= closePoint.X)
                    {
                        Point linePoint = new Point(closePoint.X + movevector.Length, closePoint.Y);
                        Point drawPoint = new Point(
                            linePoint.X - moveText.Width - textPadding,
                            linePoint.Y - moveText.Height);

                        Vector anglevector = linePoint - closePoint;

                        RotateTransform transform = new RotateTransform();
                        transform.CenterX = closePoint.X;
                        transform.CenterY = closePoint.Y;
                        double angle = Vector.AngleBetween(movevector, anglevector);
                        transform.Angle = -angle;

                        drawingContext?.PushTransform(transform);
                        drawingContext?.DrawText(moveText, drawPoint);
                        if (totalInch > currentInch)
                        {
                            drawingContext?.DrawText(totalText, new Point(
                                drawPoint.X + moveText.Width + textPadding * 2,
                                drawPoint.Y
                                ));
                        }
                        drawingContext.Pop();
                    }
                    else
                    {
                        Point linePoint = new Point(closePoint.X - movevector.Length, closePoint.Y);
                        Point drawPoint = new Point(
                            linePoint.X + textPadding,
                            linePoint.Y - moveText.Height);

                        Vector anglevector = linePoint - closePoint;

                        RotateTransform transform = new RotateTransform();
                        transform.CenterX = closePoint.X;
                        transform.CenterY = closePoint.Y;
                        double angle = Vector.AngleBetween(movevector, anglevector);
                        transform.Angle = -angle;

                        drawingContext?.PushTransform(transform);
                        drawingContext?.DrawText(moveText, drawPoint);
                        if (totalInch > currentInch)
                        {
                            drawingContext?.DrawText(totalText,
                                new Point(
                                drawPoint.X - totalText.Width - textPadding * 2,
                                drawPoint.Y
                                ));
                        }
                        drawingContext.Pop();
                    }
                }

                double left = drawPoints.AsEnumerable().Select(x => x.X).Min();
                double right = drawPoints.AsEnumerable().Select(x => x.X).Max();
                double top = drawPoints.AsEnumerable().Select(x => x.Y).Min();
                double bottom = drawPoints.AsEnumerable().Select(x => x.Y).Max();
                DPIRect = new Rect(left, top, right - left, bottom - top);

                MeasureEventArgs measureEvent = new MeasureEventArgs();
                if (drawPoints.Count < 2)
                {
                    measureEvent.Angle = 0;
                }
                else
                {
                    Vector standVector = drawPoints[drawPoints.Count - 1] - drawPoints[drawPoints.Count - 2];
                    Vector endvector = closePoint - checkPoint;
                    measureEvent.Angle = (int)Math.Abs(Vector.AngleBetween(endvector, standVector));
                }
                measureEvent.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                measureEvent.RulerTranslate = measureSetting.RulerTranslate;
                measureEvent.RulerBase = measureSetting.RulerBase;
                measureEvent.RulerBaseUnit = measureSetting.RulerBaseUnit;
                measureEvent.Precision = measureSetting.Precision;
                measureEvent.Type = CPDFMeasureType.CPDF_PERIMETER_MEASURE;
                measureEvent.Distance = totalText.Text;

                MeasureChanged?.Invoke(this, measureEvent);
            }
        }

        private void DrawPolygonMeasure(DrawingContext drawingContext)
        {
            CPDFPolygonAnnotation polygonAnnot = (cPDFAnnotation as CPDFPolygonAnnotation);
            byte[] bytes = polygonAnnot.LineColor;
            Color color = ParamConverter.ConverterByteForColor(bytes);
            color.A = polygonAnnot.GetTransparency();
            Pen DrawPen = new Pen(new SolidColorBrush(color), polygonAnnot.GetBorderWidth());
            Pen EndDrawPen = new Pen(Brushes.Black, polygonAnnot.GetBorderWidth());
            SolidColorBrush TextBrush = Brushes.Red;

            if (polygonAnnot.IsMeasured())
            {
                CPDFAreaMeasure measureInfo = polygonAnnot.GetAreaMeasure();
                if (measureInfo != null && measureInfo.TextAttribute != null && measureInfo.TextAttribute.FontColor != null && measureInfo.TextAttribute.FontColor.Length >= 3)
                {
                    byte[] fontColor = measureInfo.TextAttribute.FontColor;
                    TextBrush = new SolidColorBrush(Color.FromRgb(fontColor[0], fontColor[1], fontColor[2]));
                }

                if (polygonAnnot.Dash != null && polygonAnnot.Dash.Length > 0)
                {
                    DashStyle dash = new DashStyle();
                    foreach (var offset in polygonAnnot.Dash)
                    {
                        dash.Dashes.Add(offset / polygonAnnot.LineWidth);
                    }
                    DrawPen.DashStyle = dash;
                    DrawPen.DashCap = PenLineCap.Flat;
                }
            }

            if (isProportionalScaling)
            {
                if (drawPoints != null && drawPoints.Count > 0)
                {
                    mouseEndPoint = CalcAnglePoint(mouseEndPoint, drawPoints[drawPoints.Count - 1], pageBound);
                }
            }

            Point checkPoint = mouseEndPoint;
            checkPoint.X = Math.Max(pageBound.Left, checkPoint.X);
            checkPoint.X = Math.Min(pageBound.Right, checkPoint.X);
            checkPoint.Y = Math.Max(pageBound.Top, checkPoint.Y);
            checkPoint.Y = Math.Min(pageBound.Bottom, checkPoint.Y);

            PointCollection points = drawPoints.Clone();
            if (defaultSettingParam.IsCreateSquarePolygonMeasure && drawPoints.Count == 1)
            {
                Point star = points[0];
                Rect rect = new Rect(star, checkPoint);
                points.Clear();
                points.Add(rect.TopLeft);
                points.Add(rect.BottomLeft);
                points.Add(rect.BottomRight);
                points.Add(rect.TopRight);
            }

            if (points.Count > 0)
            {
                CPDFBorderEffector borderEffector = polygonAnnot.GetAnnotBorderEffector();
                if (borderEffector != null && borderEffector.BorderIntensity != C_BORDER_INTENSITY.C_INTENSITY_ZERO && borderEffector.BorderType != C_BORDER_TYPE.C_BORDER_TYPE_STRAIGHT)
                {
                    //Draw the example line connected by the start point and the end point.
                    if (points.Count == 1)
                    {
                        Pen dashedPen = new Pen(Brushes.Gray, 1);
                        dashedPen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
                        drawingContext?.DrawLine(dashedPen, points[0], checkPoint);
                    }

                    double left = drawPoints.AsEnumerable().Select(x => x.X).Min();
                    double right = drawPoints.AsEnumerable().Select(x => x.X).Max();
                    double top = drawPoints.AsEnumerable().Select(x => x.Y).Min();
                    double bottom = drawPoints.AsEnumerable().Select(x => x.Y).Max();
                    DPIRect = new Rect(left, top, right - left, bottom - top);

                    polygonAnnot.SetAnnotBorderEffector(borderEffector);
                    drawPoints.Add(checkPoint);
                    List<Point> measurePoint = new List<Point>();
                    measurePoint = GetMeasureDrawPoints();
                    drawPoints.RemoveAt(drawPoints.Count - 1);
                    List<CPoint> cPoints = new List<CPoint>();
                    foreach (Point item in measurePoint)
                    {
                        cPoints.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(item)));
                    }

                    polygonAnnot.SetPoints(cPoints);
                    polygonAnnot.UpdateAp();
                    cPDFViewer.UpdateAnnotFrame();
                }
                else
                {
                    PathGeometry drawPath = new PathGeometry();
                    PathFigure drawFigure = new PathFigure();

                    drawFigure.StartPoint = points[0];
                    PolyLineSegment polySegment = new PolyLineSegment();

                    for (int i = 1; i < points.Count; i++)
                    {
                        polySegment.Points.Add(points[i]);
                    }

                    if (defaultSettingParam.IsCreateSquarePolygonMeasure)
                    {
                        polySegment.Points.Add(points[0]);
                    }
                    else
                    {
                        //Add the current point during the movement.
                        polySegment.Points.Add(checkPoint);
                    }

                    if (polySegment.Points.Count > 0)
                    {
                        drawFigure.Segments.Add(polySegment);
                    }

                    if (drawFigure.Segments.Count > 0)
                    {
                        drawPath.Figures.Add(drawFigure);
                    }

                    //Draw the line segment.
                    drawingContext?.DrawGeometry(null, DrawPen, drawPath);

                    //Draw the example line connected by the start point and the end point.
                    if (points.Count > 1)
                    {
                        if (defaultSettingParam.IsCreateSquarePolygonMeasure)
                        {
                            drawingContext?.DrawLine(DrawPen, points[0], polySegment.Points.Last());
                        }
                        else
                        {
                            drawingContext?.DrawLine(EndDrawPen, points[0], polySegment.Points.Last());
                        }
                    }

                    //Calculate the length.
                    double totalInch = 0;
                    if (points.Count > 1)
                    {
                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            totalInch += measureSetting.GetMeasureLength(points[i], points[i + 1], zoomFactor);
                        }
                    }

                    double currentInch = measureSetting.GetMeasureLength(points[points.Count - 1], checkPoint, zoomFactor);
                    if (defaultSettingParam.IsCreateSquarePolygonMeasure)
                    {
                        currentInch = measureSetting.GetMeasureLength(points[points.Count - 1], points[0], zoomFactor);
                    }

                    totalInch += currentInch;
                    Point closePoint = points[points.Count - 1];
                    Vector movevector = checkPoint - closePoint;
                    if (polygonAnnot.IsMeasured())
                    {
                        FormattedText moveText = new FormattedText(
                            string.Format("{0} {1}", measureSetting.GetPrecisionData(currentInch), measureSetting.RulerTranslateUnit),
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface("YaHei"),
                            16,
                            TextBrush);

                        FormattedText totalText = new FormattedText(
                           string.Format("{0} {1}", measureSetting.GetPrecisionData(totalInch), measureSetting.RulerTranslateUnit),
                           CultureInfo.GetCultureInfo("en-us"),
                           FlowDirection.LeftToRight,
                           new Typeface("YaHei"),
                           16,
                         TextBrush);

                        //Judge the text display form.
                        if (movevector.Length > moveText.Width + textPadding || defaultSettingParam.IsCreateSquarePolygonMeasure)
                        {
                            if (checkPoint.X >= closePoint.X)
                            {
                                Point linePoint = new Point(closePoint.X + movevector.Length, closePoint.Y);
                                Point drawPoint = new Point(
                                    linePoint.X - moveText.Width - textPadding,
                                    linePoint.Y - moveText.Height);

                                Vector anglevector = linePoint - closePoint;

                                RotateTransform transform = new RotateTransform();
                                transform.CenterX = closePoint.X;
                                transform.CenterY = closePoint.Y;
                                double angle = Vector.AngleBetween(movevector, anglevector);
                                transform.Angle = -angle;

                                drawingContext?.PushTransform(transform);
                                if (!defaultSettingParam.IsCreateSquarePolygonMeasure)
                                {
                                    drawingContext?.DrawText(moveText, drawPoint);
                                }
                                if (totalInch > currentInch)
                                {
                                    drawingContext?.DrawText(totalText, new Point(
                                        drawPoint.X + moveText.Width + textPadding * 2,
                                        drawPoint.Y
                                        ));
                                }
                                drawingContext.Pop();
                            }
                            else
                            {
                                Point linePoint = new Point(closePoint.X - movevector.Length, closePoint.Y);
                                Point drawPoint = new Point(
                                    linePoint.X + textPadding,
                                    linePoint.Y - moveText.Height);

                                Vector anglevector = linePoint - closePoint;
                                RotateTransform transform = new RotateTransform();
                                transform.CenterX = closePoint.X;
                                transform.CenterY = closePoint.Y;
                                double angle = Vector.AngleBetween(movevector, anglevector);
                                transform.Angle = -angle;

                                drawingContext?.PushTransform(transform);
                                if (!defaultSettingParam.IsCreateSquarePolygonMeasure)
                                {
                                    drawingContext?.DrawText(moveText, drawPoint);
                                }
                                if (totalInch > currentInch)
                                {
                                    drawingContext?.DrawText(totalText,
                                        new Point(
                                        drawPoint.X - totalText.Width - textPadding * 2,
                                        drawPoint.Y
                                        ));
                                }
                                drawingContext.Pop();
                            }
                        }
                    }

                    if (defaultSettingParam.IsCreateSquarePolygonMeasure)
                    {
                        double deleft = points.AsEnumerable().Select(x => x.X).Min();
                        double deright = points.AsEnumerable().Select(x => x.X).Max();
                        double detop = points.AsEnumerable().Select(x => x.Y).Min();
                        double debottom = points.AsEnumerable().Select(x => x.Y).Max();
                        DPIRect = new Rect(deleft, detop, deright - deleft, debottom - detop);
                    }
                    else
                    {
                        double left = drawPoints.AsEnumerable().Select(x => x.X).Min();
                        double right = drawPoints.AsEnumerable().Select(x => x.X).Max();
                        double top = drawPoints.AsEnumerable().Select(x => x.Y).Min();
                        double bottom = drawPoints.AsEnumerable().Select(x => x.Y).Max();
                        DPIRect = new Rect(left, top, right - left, bottom - top);
                    }

                    MeasureEventArgs measureEvent = new MeasureEventArgs();
                    if (points.Count < 2)
                    {
                        measureEvent.Angle = 0;
                    }
                    else
                    {
                        Vector standVector = points[points.Count - 1] - points[points.Count - 2];
                        Vector endvector = closePoint - checkPoint;
                        measureEvent.Angle = (int)Math.Abs(Vector.AngleBetween(endvector, standVector));
                        if (defaultSettingParam.IsCreateSquarePolygonMeasure)
                        {
                            measureEvent.Angle = 90;
                        }
                    }

                    List<Point> pon = new List<Point>();
                    if (!defaultSettingParam.IsCreateSquarePolygonMeasure)
                    {
                        points.Add(checkPoint);
                    }
                    foreach (Point drawPoint in points)
                    {
                        Point savePoint = new Point(
                            (drawPoint.X - pageBound.Left) + cropPoint.X,
                            (drawPoint.Y - pageBound.Top) + cropPoint.Y);
                        pon.Add(DpiHelper.StandardPointToPDFPoint(new Point(
                           (float)drawPoint.X / zoomFactor,
                            (float)drawPoint.Y / zoomFactor
                            )));
                    }

                    double area = measureSetting.ComputePolygonArea(pon.ToList());
                    double ratio = measureSetting.GetMeasureAreaRatio();
                    double rate = measureSetting.RulerTranslate / measureSetting.RulerBase;
                    double inch = area * ratio * ratio * rate * rate;

                    //measureEvent.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                    //measureEvent.RulerTranslate = measureSetting.RulerTranslate;
                    //measureEvent.RulerBase = measureSetting.RulerBase;
                    //measureEvent.RulerBaseUnit = measureSetting.RulerBaseUnit;
                    //measureEvent.Precision = measureSetting.Precision;
                    //measureEvent.Type = CPDFMeasureType.CPDF_AREA_MEASURE;
                    //measureEvent.Distance = totalText.Text;
                    //  measureEvent.Area = string.Format("{0} sq {1}", measureSetting.GetPrecisionData(inch), measureSetting.RulerTranslateUnit);

                    MeasureChanged?.Invoke(this, measureEvent);
                }
            }
        }

        private void DrawLineMeasure(DrawingContext drawingContext)
        {
            CPDFLineAnnotation polyLine = (cPDFAnnotation as CPDFLineAnnotation);
            byte[] bytes = polyLine.LineColor;
            Color color = ParamConverter.ConverterByteForColor(bytes);
            color.A = polyLine.GetTransparency();
            Pen DrawPen = new Pen(new SolidColorBrush(color), polyLine.GetBorderWidth());
            SolidColorBrush TextBrush = Brushes.Red;

            if (polyLine.IsMeasured())
            {
                CPDFDistanceMeasure measureInfo = polyLine.GetDistanceMeasure();
                if (measureInfo != null && measureInfo.TextAttribute != null && measureInfo.TextAttribute.FontColor != null && measureInfo.TextAttribute.FontColor.Length >= 3)
                {
                    byte[] fontColor = measureInfo.TextAttribute.FontColor;
                    TextBrush = new SolidColorBrush(Color.FromRgb(fontColor[0], fontColor[1], fontColor[2]));
                }

                if (polyLine.Dash != null && polyLine.Dash.Length > 0)
                {
                    DashStyle dash = new DashStyle();
                    foreach (var offset in polyLine.Dash)
                    {
                        dash.Dashes.Add(offset / polyLine.LineWidth);
                    }
                    DrawPen.DashStyle = dash;
                    DrawPen.DashCap = PenLineCap.Flat;
                }
            }

            if (isProportionalScaling)
            {
                mouseEndPoint = CalcAnglePoint(mouseEndPoint, mouseStartPoint, pageBound);
            }
            Point checkPoint = mouseEndPoint;
            checkPoint.X = Math.Max(pageBound.Left, checkPoint.X);
            checkPoint.X = Math.Min(pageBound.Right, checkPoint.X);
            checkPoint.Y = Math.Max(pageBound.Top, checkPoint.Y);
            checkPoint.Y = Math.Min(pageBound.Bottom, checkPoint.Y);

            double inch = measureSetting.GetMeasureLength(mouseStartPoint, checkPoint, zoomFactor);
            ArrowHelper drawLine = new ArrowHelper();
            drawLine.LineStart = mouseStartPoint;
            drawLine.LineEnd = checkPoint;
            drawLine.ArrowLength = (uint)Math.Max(DrawPen.Thickness * 3, 12 * zoomFactor * zoomFactor);
            drawLine.StartSharp = polyLine.HeadLineType;
            drawLine.EndSharp = polyLine.TailLineType;
            drawLine.BuildArrowBody();

            drawingContext?.DrawGeometry(null, DrawPen, drawLine.Body);

            drawingContext.DrawGeometry(null, DrawPen, drawLine.BuildArrowBody());

            FormattedText formattedText = new FormattedText(
                string.Format("{0} {1}", measureSetting.GetPrecisionData(inch), measureSetting.RulerTranslateUnit),
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("YaHei"),
                16,
                TextBrush);

            Vector movevector = checkPoint - mouseStartPoint;
            if (movevector.Length > formattedText.Width + textPadding)
            {
                if (checkPoint.X >= mouseStartPoint.X)
                {
                    Point linePoint = new Point(mouseStartPoint.X + movevector.Length, mouseStartPoint.Y);
                    Point drawPoint = new Point(
                        linePoint.X - formattedText.Width - textPadding,
                        linePoint.Y - formattedText.Height);

                    Vector anglevector = linePoint - mouseStartPoint;

                    RotateTransform transform = new RotateTransform();
                    transform.CenterX = mouseStartPoint.X;
                    transform.CenterY = mouseStartPoint.Y;
                    double angle = Vector.AngleBetween(movevector, anglevector);
                    transform.Angle = -angle;

                    drawingContext?.PushTransform(transform);
                    drawingContext?.DrawText(formattedText, drawPoint);
                    drawingContext.Pop();
                }
                else
                {
                    Point linePoint = new Point(mouseStartPoint.X - movevector.Length, mouseStartPoint.Y);
                    Point drawPoint = new Point(
                        linePoint.X + textPadding,
                        linePoint.Y - formattedText.Height);

                    Vector anglevector = linePoint - mouseStartPoint;

                    RotateTransform transform = new RotateTransform();
                    transform.CenterX = mouseStartPoint.X;
                    transform.CenterY = mouseStartPoint.Y;
                    double angle = Vector.AngleBetween(movevector, anglevector);
                    transform.Angle = -angle;

                    drawingContext?.PushTransform(transform);
                    drawingContext?.DrawText(formattedText, drawPoint);
                    drawingContext.Pop();
                }
            }
            DPIRect = new Rect(mouseStartPoint, checkPoint);
            if (drawPoints.Count <= 1)
            {
                drawPoints.Add(checkPoint);
            }
            else
            {
                drawPoints[1] = checkPoint;
            }

            Vector standVector = new Vector(1, 0);

            MeasureEventArgs measureEvent = new MeasureEventArgs();
            measureEvent.Angle = (int)Math.Abs(Vector.AngleBetween(movevector, standVector));
            measureEvent.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
            measureEvent.RulerTranslate = measureSetting.RulerTranslate;
            measureEvent.RulerBase = measureSetting.RulerBase;
            measureEvent.RulerBaseUnit = measureSetting.RulerBaseUnit;
            measureEvent.Precision = measureSetting.Precision;
            Vector moveVector = checkPoint - mouseStartPoint;
            measureEvent.MousePos = new Point(
                (int)Math.Abs(moveVector.X / zoomFactor / 96D * 72D),
                (int)Math.Abs(moveVector.Y / zoomFactor / 96D * 72D));

            measureEvent.Type = CPDFMeasureType.CPDF_DISTANCE_MEASURE;
            measureEvent.Distance = formattedText.Text;

            MeasureChanged?.Invoke(this, measureEvent);
        }

        #endregion
        public void MultipleClick(Point downPoint)
        {
            if(!drawPoints.Contains(downPoint))
            {
                drawPoints.Add(downPoint);
            }
        }

        public Rect GetMaxRect()
        {
            return maxRect;
        }

        public void CreateTextBox()
        {
            try
            {
                if (cPDFAnnotation != null && cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
                {
                    CPDFFreeTextAnnotation annotFreeText = (cPDFAnnotation as CPDFFreeTextAnnotation);

                    TextBox textui = new TextBox();

                    DashedBorder textBorder = new DashedBorder();
                    textBorder.Child = textui;
                    textui.Width = 200;
                    CTextAttribute textAttribute = annotFreeText.FreeTextDa;
                    byte transparency = annotFreeText.GetTransparency();
                    textui.FontSize = DpiHelper.PDFNumToStandardNum(textAttribute.FontSize * zoomFactor);
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.White;
                    byte[] colorArray = new byte[3];
                    if (annotFreeText.Transparency > 0)
                    {
                        borderColor = Color.FromRgb(annotFreeText.LineColor[0], annotFreeText.LineColor[1], annotFreeText.LineColor[2]);
                    }

                    if (annotFreeText.HasBgColor)
                    {
                        backgroundColor = Color.FromRgb(annotFreeText.BgColor[0], annotFreeText.BgColor[1], annotFreeText.BgColor[2]);
                    }
                    Point MousePoint = new Point((mouseStartPoint.X - pageBound.X), (mouseStartPoint.Y - pageBound.Y));
                    textBorder.MaxWidth = (pageBound.Width - MousePoint.X - cropPoint.X);
                    textBorder.MaxHeight = (pageBound.Height - MousePoint.Y - cropPoint.Y);

                    textui.Foreground = new SolidColorBrush(textColor);
                    textui.Background = new SolidColorBrush(backgroundColor);
                    textui.MinHeight = 40;
                    textui.MinWidth = 200;

                    textBorder.Padding = new Thickness(0);
                    textBorder.BorderBrush = new SolidColorBrush(borderColor);
                    double rawWidth = annotFreeText.GetBorderWidth();
                    double drawWidth = DpiHelper.PDFNumToStandardNum(rawWidth * zoomFactor);
                    textBorder.BorderThickness = new Thickness(drawWidth);
                    if (annotFreeText.BorderStyle != C_BORDER_STYLE.BS_SOLID && annotFreeText.Dash != null && annotFreeText.Dash.Length > 0)
                    {
                        //补充保存虚线样式
                        DoubleCollection dashCollection = new DoubleCollection();
                        foreach (float num in annotFreeText.Dash)
                        {
                            dashCollection.Add(num);
                        }
                        textBorder?.DrawDashBorder(true, drawWidth, rawWidth, dashCollection);
                    }

                    textui.BorderThickness = new Thickness(0);
                    textui.Text = annotFreeText.Content;

                    string fontName = string.Empty;
                    string fontFamily = string.Empty;
                    CPDFFont.GetFamilyStyleName(annotFreeText.FreeTextDa.FontName, ref fontFamily, ref fontName);
                    textui.FontFamily = new FontFamily(fontFamily);

                    textui.AcceptsReturn = true;
                    textui.TextWrapping = TextWrapping.Wrap;
                    textui.TextAlignment = TextAlignment.Left;

                    switch (annotFreeText.Alignment)
                    {
                        case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                            textui.TextAlignment = TextAlignment.Left;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                            textui.TextAlignment = TextAlignment.Right;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                            textui.TextAlignment = TextAlignment.Center;
                            break;
                        default:
                            break;
                    }
                    textBorder.SetValue(Canvas.LeftProperty, mouseStartPoint.X);
                    textBorder.SetValue(Canvas.TopProperty, mouseStartPoint.Y);

                    lastTextui = textui;
                    lastTextBorder = textBorder;
                    textui.Loaded += (object sender, RoutedEventArgs e) =>
                    {
                        textui.Focus();
                        textui.CaretIndex = textui.Text.Length;
                        textui.SetValue(PopupTextAttachDataProperty, cPDFAnnotation);
                        UpdateAnnotHandler?.Invoke(this, false);
                    };

                    textui.LostFocus += (object sender, RoutedEventArgs e) =>
                    {
                        CPDFAnnotation currentAnnot = textui.GetValue(PopupTextAttachDataProperty) as CPDFAnnotation;
                        AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFViewer.GetDocument(), currentAnnot);
                        if (currentAnnot != null && currentAnnot.IsValid())
                        {
                            CPDFFreeTextAnnotation updateFreeText = currentAnnot as CPDFFreeTextAnnotation;
                            if (textui.Text != string.Empty || updateFreeText.GetBorderWidth() != 0)
                            {
                                updateFreeText.SetContent(textui.Text);
                                Rect changeRect = new Rect(
                                DpiHelper.StandardNumToPDFNum(freeTextPoint.X),
                                 DpiHelper.StandardNumToPDFNum(freeTextPoint.Y),
                                    DpiHelper.StandardNumToPDFNum(textBorder.ActualWidth / zoomFactor),
                                      DpiHelper.StandardNumToPDFNum(textBorder.ActualHeight / zoomFactor));

                                updateFreeText.SetRect(new CRect(
                                     (float)changeRect.Left,
                                      (float)changeRect.Bottom,
                                       (float)changeRect.Right,
                                        (float)changeRect.Top
                                    ));
                                updateFreeText.UpdateAp();
                                FreeTextAnnotHistory freeTextAnnotHistory = new FreeTextAnnotHistory();
                                annotParam = ParamConverter.AnnotConverter(cPDFViewer.GetDocument(), currentAnnot);
                                annotParam.AnnotIndex = currentAnnot.Page.GetAnnotCount() - 1;
                                freeTextAnnotHistory.CurrentParam = (FreeTextParam)annotParam;
                                freeTextAnnotHistory.PDFDoc = cPDFViewer.GetDocument();
                                cPDFViewer.UndoManager.AddHistory(freeTextAnnotHistory);
                                UpdateAnnotHandler?.Invoke(this, true);
                                cPDFViewer.UndoManager?.InvokeHistoryChanged(this, new KeyValuePair<ComPDFKitViewer.Helper.UndoAction, IHistory>(ComPDFKitViewer.Helper.UndoAction.Custom, freeTextAnnotHistory));
                                freeTextPoint = new Point(0, 0);
                            }
                            else
                            {
                                updateFreeText.RemoveAnnot();
                                CreateFreetextCanceled?.Invoke(this, annotParam);
                            }
                        }
                        RemoveTextBox();
                    };

                    BaseLayer createAnnotTool = this;
                    if (createAnnotTool != null)
                    {
                        createAnnotTool.Children.Add(textBorder);
                        createAnnotTool.Arrange();
                    }

                    textui.LayoutUpdated += (object sender, EventArgs e) =>
                    {
                        createAnnotTool.Arrange();
                    };
                }
            }
            catch
            {

            }

        }

        public void RemoveTextBox()
        {
            if (lastTextBorder == null)
            {
                return;
            }
            BaseLayer removeLayer = this;
            removeLayer.Children.Remove(lastTextBorder);
        }

        private void DrawText()
        {
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
            Rect drawRect = new Rect(mLeft, mUp, mRight - mLeft, mDown - mUp);
            DPIRect = drawRect;
            try
            {
                if (lastTextui != null && lastTextBorder != null && cPDFAnnotation != null && cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
                {
                    CPDFFreeTextAnnotation annotFreeText = (cPDFAnnotation as CPDFFreeTextAnnotation);

                    CTextAttribute textAttribute = annotFreeText.FreeTextDa;
                    byte transparency = annotFreeText.GetTransparency();
                    lastTextui.FontSize = DpiHelper.PDFNumToStandardNum(textAttribute.FontSize * zoomFactor);
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.Transparent;
                    byte[] colorArray = new byte[3];
                    if (annotFreeText.Transparency > 0)
                    {
                        borderColor = Color.FromArgb(annotFreeText.Transparency, annotFreeText.LineColor[0], annotFreeText.LineColor[1], annotFreeText.LineColor[2]);
                    }

                    if (annotFreeText.HasBgColor)
                    {
                        backgroundColor = Color.FromArgb(annotFreeText.Transparency, annotFreeText.BgColor[0], annotFreeText.BgColor[1], annotFreeText.BgColor[2]);
                    }

                    Border parentUI = lastTextui.Parent as Border;
                    if (parentUI != null)
                    {
                        parentUI.SetValue(Canvas.LeftProperty, drawRect.Left);
                        parentUI.SetValue(Canvas.TopProperty, drawRect.Top);

                        // The width is incorrect
                        if (mouseEndPoint.X >= mouseStartPoint.X)
                        {
                            parentUI.MaxWidth = (pageBound.Right - drawRect.X - cropPoint.X);
                        }
                        else
                        {
                            parentUI.MaxWidth = (drawRect.Right - pageBound.X - cropPoint.X);
                        }

                        if (mouseEndPoint.Y >= mouseStartPoint.Y)
                        {
                            parentUI.MaxHeight = (pageBound.Bottom - drawRect.Y - cropPoint.Y);
                        }
                        else
                        {
                            parentUI.MaxHeight = (drawRect.Bottom - pageBound.Y - cropPoint.Y);
                        }

                    }
                    lastTextui.MinWidth = drawRect.Width;
                    lastTextui.MinHeight = drawRect.Height;
                    lastTextui.Foreground = new SolidColorBrush(textColor);
                    lastTextui.Background = new SolidColorBrush(backgroundColor);
                    lastTextBorder.Padding = new Thickness(0);
                    lastTextBorder.BorderBrush = new SolidColorBrush(borderColor);
                    double rawWidth = annotFreeText.GetBorderWidth();
                    double drawWidth = DpiHelper.PDFNumToStandardNum(rawWidth * zoomFactor);
                    lastTextBorder.BorderThickness = new Thickness(drawWidth);
                    lastTextui.BorderThickness = new Thickness(0);
                    lastTextui.Text = annotFreeText.Content;
                    lastTextui.Opacity = annotFreeText.Transparency;
                    if (annotFreeText.BorderStyle != C_BORDER_STYLE.BS_SOLID && annotFreeText.Dash != null && annotFreeText.Dash.Length > 0)
                    {
                        //补充保存虚线样式
                        DashedBorder dashBorder = (DashedBorder)lastTextBorder;
                        DoubleCollection dashCollection = new DoubleCollection();
                        foreach (float num in annotFreeText.Dash)
                        {
                            dashCollection.Add(num);
                        }
                        dashBorder.DrawDashBorder(true, drawWidth, rawWidth, dashCollection);
                    }

                    string fontName = string.Empty;
                    string fontFamily = string.Empty;
                    CPDFFont.GetFamilyStyleName(annotFreeText.FreeTextDa.FontName, ref fontFamily, ref fontName);
                    lastTextui.FontFamily = new FontFamily(fontFamily);

                    lastTextui.FontWeight = IsBold(textAttribute.FontName) ? FontWeights.Bold : FontWeights.Normal;
                    lastTextui.FontStyle = IsItalic(textAttribute.FontName) ? FontStyles.Italic : FontStyles.Normal;

                    lastTextui.AcceptsReturn = true;
                    lastTextui.TextWrapping = TextWrapping.Wrap;
                    lastTextui.TextAlignment = TextAlignment.Left;

                    switch (annotFreeText.Alignment)
                    {
                        case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                            lastTextui.TextAlignment = TextAlignment.Left;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                            lastTextui.TextAlignment = TextAlignment.Right;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                            lastTextui.TextAlignment = TextAlignment.Center;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {

            }
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

        public bool IsCanSave()
        {
            if (cPDFAnnotation == null)
                return false;

            if (cPDFAnnotation is CPDFPolygonAnnotation)
            {
                if (drawPoints.Count <= 2)
                    return false;
            }

            return true;
        }

        public bool IsCreating()
        {
            return cPDFAnnotation != null;
        }
    }
}
