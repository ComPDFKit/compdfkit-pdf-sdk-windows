using ComPDFKit.Tool.SettingParam;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ComPDFKit.Tool.Help.ImportWin32;

namespace ComPDFKit.Tool.DrawTool
{

    public enum MulitiDrawMoveType
    {
        Default,
        Alone
    }


    public class MultiSelectedRect : DrawingVisual
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
        protected DefaultDrawParam drawParam = new DefaultDrawParam();

        protected DrawingContext drawDC { get; set; }

        /// <summary>
        /// DataChanging event
        /// </summary>
        public event EventHandler<Point> DataChanging;

        /// <summary>
        /// DataChanged event
        /// </summary>
        public event EventHandler<Point> DataChanged;

        protected SelectedType selectedType = SelectedType.None;

        /// <summary>
        /// Minimum width of the rectangle
        /// </summary>
        protected int rectMinWidth { get; set; } = 10;

        /// <summary>
        /// Minimum height of the rectangle
        /// </summary>
        protected int rectMinHeight { get; set; } = 10;

        /// <summary>
        /// Identify whether the mouse is pressed
        /// </summary>
        protected bool isMouseDown { get; set; }

        /// <summary>
        /// Current hit control point
        /// </summary>
        protected PointControlType hitControlType { get; set; }

        /// <summary>
        /// Location information recorded when the mouse is pressed
        /// </summary>
        protected Point mouseDownPoint { get; set; }

        /// <summary>
        /// Current set ignore points
        /// </summary>
        protected List<PointControlType> ignorePoints { get; set; } = new List<PointControlType>();

        /// <summary>
        /// Current control point coordinates
        /// </summary>
        protected List<Point> controlPoints { get; set; } = new List<Point>();

        /// <summary>
        /// Move offset
        /// </summary>
        protected Point moveOffset { get; set; } = new Point(0, 0);

        /// <summary>
        /// Current PDFVIewer's actual display width
        /// </summary>
        protected double PDFViewerActualWidth { get; set; } = 0;

        /// <summary>
        /// Current PDFVIewer's actual display height
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
        /// Current multi-select drawing style
        /// </summary>
        protected MulitiDrawMoveType currentDrawType { get; set; } = MulitiDrawMoveType.Default;

        /// <summary>
        /// Control point size
        /// </summary>
        protected int pointSize { get; set; } = 4;

        /// <summary>
        /// Current drawing rectangle (calculated during operation)
        /// </summary>
        protected Rect drawRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Default outermost rectangle of the drawing style (calculated during operation)
        /// </summary>
        protected Rect drawDefaultRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary> 
        /// Padding between the border and the content
        /// </summary>
        protected double rectPadding = 5;

        /// <summary>
        /// Mouse down cache rectangle
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
        /// Identify whether the mouse is pressed
        /// </summary>
        protected bool isProportionalScaling { get; set; } = false;

        /// <summary>
        /// Array passed from outside for multiple selection
        /// </summary>
        protected List<SelectedRect> selectedRects = new List<SelectedRect>();
        protected Dictionary<SelectedRect,KeyValuePair<int,int>> RelationDict=new Dictionary<SelectedRect, KeyValuePair<int, int>>();

        protected bool isHover = false;

        protected bool isSelected = false;

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

        public MultiSelectedRect(DefaultDrawParam defaultDrawParam, SelectedType type) : base()
        {
            drawParam = defaultDrawParam;
            currentDrawPointType = DrawPointType.Circle;
            selectedType = type;
        }

        public void SetMulitSelectedRect(SelectedRect selectedobject,int pageIndex,int editIndex)
        {
            selectedRects.Add(selectedobject);
            RelationDict[selectedobject] = new KeyValuePair<int, int>(pageIndex, editIndex);
        }

        public bool GetRelationKey(SelectedRect selectedobject,out int pageIndex,out int editIndex)
        {
            pageIndex = -1;
            editIndex = -1;
            if(RelationDict!=null && RelationDict.ContainsKey(selectedobject))
            {
                KeyValuePair<int, int> relateData = RelationDict[selectedobject];
                pageIndex = relateData.Key;
                editIndex = relateData.Value;
                return true;
            }

            return false;
        }
        /// <summary>
        /// delete
        /// </summary>
        /// <param name="selectedobject"></param>
        public void DelMulitSelectedRect(SelectedRect selectedobject)
        {
            selectedRects.Remove(selectedobject);
            RelationDict.Remove(selectedobject);
        }

        /// <summary>
        /// get selectedRects Index
        /// </summary>
        /// <param name="selectedobject"></param>
        /// <returns></returns>
        public int GetMulitSelectedRectIndex(SelectedRect selectedobject)
        {
          return selectedRects.IndexOf(selectedobject);
        }

        public List<SelectedRect> GetMulitSelectList()
        {
            return selectedRects==null ? new List<SelectedRect>() : selectedRects;
        }

        public SelectedType GetSelectedType()
        {
            return selectedType;
        }

        public void SetSelectedType(SelectedType type)
        {
            if (selectedType != type)
            {
                selectedRects.Clear();
                RelationDict.Clear();
            }
            selectedType = type;
        }

        public void CleanMulitSelectedRect()
        {
            selectedRects.Clear();
            RelationDict.Clear();
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

        public float GetZoomX()
        {
            return (float)(drawRect.Width / drawDefaultRect.Width);
        }

        public float GetZoomY()
        {
            return (float)(drawRect.Height / drawDefaultRect.Height);
        }

        /// <summary>
        /// Multiple selection of movement distance
        /// </summary>
        /// <returns></returns>
        public float GetChangeX()
        {
            return (float)(drawRect.Width - drawDefaultRect.Width);
        }

        /// <summary>
        /// Multiple selection of movement distance
        /// </summary>
        /// <returns></returns>
        public float GetChangeY()
        {
            return (float)(drawRect.Height - drawDefaultRect.Height);
        }


        public void Draw()
        {
            switch (currentDrawType)
            {
                case MulitiDrawMoveType.Default:
                    Dispatcher.Invoke(() =>
                    {
                        if (drawDefaultRect.IsEmpty == false && drawDefaultRect.Width > 0 && drawDefaultRect.Height > 0)
                        {
                            drawDC = RenderOpen();
                            CalcControlPoint(drawDefaultRect);

                            SolidColorBrush solidColorBrush = drawParam.SPDFEditMultiRectFillBrush;
                            Pen pen = drawParam.SPDFEditMultiRectLinePen;
                            GetBrushAndPen(ref solidColorBrush, ref pen);
                            if (selectedRects.Count >= 1)
                            {
                                foreach (SelectedRect item in selectedRects)
                                {
                                    Rect rect = item.GetRect();
                                    rect.X -= rectPadding;
                                    rect.Y -= rectPadding;
                                    rect.Width += rectPadding;
                                    rect.Height += rectPadding;
                                    drawDC?.DrawRectangle(solidColorBrush, pen, rect);
                                }
                            }

                            SolidColorBrush PointBrush = drawParam.PDFEditMultiPointBorderBrush;
                            Pen PointPen = drawParam.PDFEditMultiPointPen;
                            GetPointBrushAndPen(ref PointBrush, ref PointPen);

                            switch (currentDrawMoveType)
                            {
                                case DrawMoveType.kDefault:
                                    break;
                                case DrawMoveType.kReferenceLine:
                                    if (isMouseDown == true)
                                    {
                                        SolidColorBrush moveBrush = drawParam.PDFEditMultiMoveBrush;
                                        Pen movepen = drawParam.PDFEditMultiMovePen;
                                        GetMoveBrushAndPen(ref moveBrush, ref movepen);
                                        if (selectedType == SelectedType.PDFEdit)
                                        {
                                            DrawMoveBounds(drawDC, hitControlType, movepen, moveBrush, drawRect, drawParam.PDFEditMultiMoveRectPen);
                                        }
                                        else
                                        {
                                            DrawMoveBounds(drawDC, hitControlType, movepen, moveBrush, drawRect);
                                        }
                                    }
                                    drawDC?.DrawRectangle(solidColorBrush, pen, drawDefaultRect);
                                    break;
                                default:
                                    break;
                            }

                            switch (currentDrawPointType)
                            {
                                case DrawPointType.Circle:
                                    if (selectedType == SelectedType.PDFEdit)
                                    {
                                        //Edit Settings Frame
                                        DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, new SolidColorBrush(Color.FromRgb(71, 126, 222)));
                                    }
                                    else
                                    {
                                        DrawCirclePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                                    }
                                    break;
                                case DrawPointType.Square:
                                    DrawSquarePoint(drawDC, GetIgnorePoints(), pointSize, PointPen, PointBrush);
                                    break;
                            }
                            drawDC?.Close();
                            drawDC = null;
                        }

                    });
                    break;
                case MulitiDrawMoveType.Alone:
                    CalcControlPoint(drawDefaultRect);
                    foreach (SelectedRect selectRect in selectedRects)
                    {
                        selectRect.Draw();
                    }
                    break;
                default:
                    break;
            }
        }

        private void GetMoveBrushAndPen(ref SolidColorBrush colorBrush, ref Pen pen)
        {
            switch (selectedType)
            {
                case SelectedType.None:
                    break;
                case SelectedType.Annot:
                    //colorBrush = DrawParam.AnnotMoveBrush;
                    //pen = DrawParam.AnnotMovePen;
                    break;
                case SelectedType.PDFEdit:
                    colorBrush = drawParam.PDFEditMultiMoveBrush;
                    pen = drawParam.PDFEditMultiMovePen;
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
                    //if (isHover)
                    //{
                    //    colorBrush = DrawParam.AnnotRectFillBrush;
                    //    pen = DrawParam.AnnotRectHoverPen;
                    //}
                    //else
                    //{
                    //    colorBrush = DrawParam.AnnotRectFillBrush;
                    //    pen = DrawParam.AnnotRectLinePen;
                    //}
                    break;
                case SelectedType.PDFEdit:
                    if (isHover)
                    {
                        colorBrush = drawParam.PDFEditMultiRectFillHoverBrush;
                        pen = drawParam.PDFEditMultiRectLineHoverPen;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            colorBrush = drawParam.SPDFEditMultiRectFillBrush;
                            pen = drawParam.SPDFEditMultiRectLinePen;
                        }
                        else
                        {
                            colorBrush = drawParam.PDFEditMultiRectFillBrush;
                            pen = drawParam.PDFEditMultiRectLinePen;
                        }
                    }
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
                    //colorBrush = DrawParam.AnnotPointBorderBrush;
                    //pen = DrawParam.AnnotPointPen;
                    break;
                case SelectedType.PDFEdit:
                    if (isHover)
                    {
                        colorBrush = drawParam.PDFEditMultiRectFillHoverBrush;
                        pen = drawParam.PDFEditMultiPointHoverPen;
                    }
                    else
                    {
                        if (isSelected)
                        {
                            colorBrush = drawParam.SPDFEditMultiPointBorderBrush;
                            pen = drawParam.SPDFEditMultiPointPen;
                        }
                        else
                        {
                            colorBrush = drawParam.PDFEditMultiPointBorderBrush;
                            pen = drawParam.PDFEditMultiPointPen;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Internal drawing circle point
        /// </summary>
        /// <param name="drawingContext">
        /// Drawing context
        /// </param>
        /// <param name="ignoreList">
        ///Collection of positions where points need to be drawn 
        /// </param>
        /// <param name="PointSize">
        /// Point size
        /// </param>
        /// <param name="PointPen">
        /// Drawing point brush
        /// </param>
        /// <param name="BorderBrush">
        /// Brush for drawing point border
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
        /// Internal drawing square
        /// </summary>
        /// <param name="drawingContext">
        /// Drawing context
        /// </param>
        /// <param name="ControlPoints">
        /// Collection of positions where points need to be drawn
        /// </param>
        /// <param name="PointSize">
        /// Size of the point
        /// </param>
        /// <param name="PointPen">
        /// Brush for drawing point
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

        /// <summary>
        /// Draw the reference line in the moving state
        /// </summary>
        /// <param name="drawDc">
        /// Drawing context
        /// </param>
        /// <param name="controltype">
        /// Drawing context
        /// </param>
        /// <param name="activePen">
        /// Drawing context
        /// </param>
        /// <param name="moveBrush">
        /// Move brush
        /// </param>
        /// <param name="moveRect">
        /// Move rectangle
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

        public virtual void ClearDraw()
        {
            drawDefaultRect = setDrawRect = drawRect = new Rect();
            drawDC = RenderOpen();
            drawDC?.Close();
            drawDC = null;
        }

        /// <summary>
        /// Calculate the current control point
        /// </summary>
        /// <param name="currentRect">
        /// Target rectangle where the control point is located
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
        /// Get the original set Rect, not the one that has been calculated for padding
        /// </summary>
        public Rect GetRect()
        {
            Rect rect = new Rect(drawRect.X + rectPadding, drawRect.Y + rectPadding, Math.Max(0, drawRect.Width - 2 * rectPadding), Math.Max(0, drawRect.Height - 2 * rectPadding));
            return rect;
        }

        public void SetRect(Rect newRect)
        {
            newRect = new Rect(newRect.X - rectPadding, newRect.Y - rectPadding, newRect.Width + 2 * rectPadding, newRect.Height + 2 * rectPadding);

            if (drawDefaultRect != new Rect())
            {
                newRect.Union(drawDefaultRect);
                newRect.Intersect(maxRect);
            }
            drawDefaultRect = newRect;
            setDrawRect = drawRect = newRect;
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

        public void SetMaxRect(Rect rect)
        {
            maxRect = rect;
        }

        public Rect GetMaxRect()
        {
            return maxRect;
        }

        public void SetIsProportionalScaling(bool isProportionalScaling)
        {
            this.isProportionalScaling = isProportionalScaling;
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
        /// Set the type that needs to be ignored
        /// </summary>
        /// <param name="types">
        /// Collection of point types that need to be shielded
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
        /// Set all points to be ignored
        /// </summary>
        public void SetIgnorePointsAll()
        {
            ignorePoints.Clear();
            ignorePoints.Add(PointControlType.LeftTop);
            ignorePoints.Add(PointControlType.LeftMiddle);
            ignorePoints.Add(PointControlType.LeftBottom);
            ignorePoints.Add(PointControlType.MiddlBottom);
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
            ignorePoints.Add(PointControlType.MiddlBottom);
            ignorePoints.Add(PointControlType.RightBottom);
            ignorePoints.Add(PointControlType.RightMiddle);
            ignorePoints.Add(PointControlType.RightTop);
            ignorePoints.Add(PointControlType.MiddleTop);
            ignorePoints.Add(PointControlType.Rotate);
            ignorePoints.Add(PointControlType.Body);
            ignorePoints.Add(PointControlType.Line);
        }

        /// <summary>
        /// Calculate the movement of the hit point
        /// </summary>
        /// <param name="mousePoint">
        /// Current mouse position
        /// </param>
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// Proportional scaling offset calibration
        /// </summary>
        /// <param name="movePoint">
        /// Offset value
        /// </param>
        /// <returns>
        /// Offset value after calibration
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
        /// Set left alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignLeftForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignLeft(drawRect, maxRect));
        }

        /// <summary>
        /// Set horizontal center alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignHorizonCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignHorizonCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set right alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignRightForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignRight(drawRect, maxRect));
        }

        /// <summary>
        /// Set top alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignTopForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignTop(drawRect, maxRect));
        }

        /// <summary>
        /// Set vertical center alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignVerticalCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignVerticalCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set Align center within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignHorizonVerticalCenterForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignHorizonVerticalCenter(drawRect, maxRect));
        }

        /// <summary>
        /// Set bottom alignment within the set maximum rectangle
        /// </summary>
        public virtual void SetAlignBottomForMaxRect()
        {
            DrawAlignRect(AlignmentsHelp.SetAlignBottom(drawRect, maxRect));
        }

        /// <summary>
        /// Draw the rectangle of the alignment function
        /// </summary>
        /// <param name="RectMovePoint">
        /// Move distance required for the rectangle obtained by the alignment algorithm
        /// </param>
        private void DrawAlignRect(Point RectMovePoint)
        {
            double TmpLeft, TmpRight, TmpUp, TmpDown;
            Point OffsetPos = CalcMoveBound(drawRect, RectMovePoint, maxRect);
            TmpLeft = drawRect.Left + OffsetPos.X;
            TmpRight = drawRect.Right + OffsetPos.X;
            TmpUp = drawRect.Top + OffsetPos.Y;
            TmpDown = drawRect.Bottom + OffsetPos.Y;
            setDrawRect = drawRect = new Rect(TmpLeft, TmpUp, TmpRight - TmpLeft, TmpDown - TmpUp);
            Draw();
        }

        /// <summary>
        /// Calculate the offset of the current rectangle within the maximum rectangle range
        /// </summary>
        /// <param name="currentRect">
        /// The rectangle cached when pressed 
        /// </param>
        /// <param name="offsetPoint">
        /// The offset value equivalent to when pressed
        /// </param>
        /// <param name="maxRect">
        /// The maximum rectangle range
        /// </param>
        /// <returns>
        /// Offset value after calculation
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
        /// Used for notification events during the drawing data process/completion.
        /// </summary>
        /// <param name="isFinish">
        /// Identifies whether the data has been changed
        /// </param>
        protected void InvokeDataChangEvent(bool isFinish)
        {
            if (isFinish)
            {
                DataChanged?.Invoke(this, moveOffset);
            }
            else
            {
                DataChanging?.Invoke(this, moveOffset);
            }
        }

        /// <summary>
        /// Get the current set of ignored points
        /// </summary>
        /// <returns>
        /// Dataset of ignored points
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
        /// Get which control point the coordinate is on
        /// </summary>
        /// <param name="clickPoint">
        /// Coordinate point
        /// </param>
        /// <returns>
        /// Control point type
        /// </returns>
        public PointControlType GetHitControlIndex(Point point)
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

                                if (Math.Abs(point.X - checkPoint.X) < wlen && checkVector.Length < drawRect.Height / 3)
                                {
                                    return (PointControlType)i;
                                }
                            }
                            if ((PointControlType)i == PointControlType.LeftMiddle)
                            {
                                if (Math.Abs(point.X - checkPoint.X) < wlen && checkVector.Length < drawRect.Height / 3)
                                {
                                    return (PointControlType)i;
                                }
                            }

                            if ((PointControlType)i == PointControlType.MiddleTop)
                            {
                                if (Math.Abs(point.Y - checkPoint.Y) < hlen && checkVector.Length < drawRect.Width / 3)
                                {
                                    return (PointControlType)i;
                                }
                            }

                            if ((PointControlType)i == PointControlType.MiddlBottom)
                            {
                                if (Math.Abs(point.Y - checkPoint.Y) < hlen && checkVector.Length < drawRect.Width / 3)
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
                            break;
                        default:
                            break;
                    }
                }

                Rect defrect = drawRect;

                defrect.X -= rectPadding;
                defrect.Y -= rectPadding;
                defrect.Width += rectPadding;
                defrect.Height += rectPadding;
                if (drawRect.Contains(point))
                {
                    Rect rect = new Rect(
                        Math.Max(drawRect.X + rectPadding, 0),
                        Math.Max(drawRect.Y + rectPadding, 0),
                        drawRect.Width - 2 * rectPadding,
                        drawRect.Height - 2 * rectPadding);
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

        /// <summary>
        /// Get the rectangle where the current point is located
        /// </summary>
        /// <param name="clickPoint">
        /// Coordinate point
        /// </param>
        /// <returns>
        /// Control point type
        /// </returns>
        public SelectedRect GetHitControlRect(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            if (hitResult != null && hitResult.VisualHit is DrawingVisual)
            {
                foreach (SelectedRect selectedRect in selectedRects) {
                    Rect defrect = selectedRect.GetRect();
                    defrect.X -= rectPadding;
                    defrect.Y -= rectPadding;
                    defrect.Width += rectPadding;
                    defrect.Height += rectPadding;

                    if (defrect.Contains(point))
                    {
                        Rect rect = new Rect(
                            Math.Max(defrect.X + rectPadding, 0),
                            Math.Max(defrect.Y + rectPadding, 0),
                            defrect.Width - 2 * rectPadding,
                            defrect.Height - 2 * rectPadding);
                        if (rect.Contains(point))
                        {
                            return selectedRect;
                        }
                    }

                }

                
            }
            return null;
        }
    }
}
