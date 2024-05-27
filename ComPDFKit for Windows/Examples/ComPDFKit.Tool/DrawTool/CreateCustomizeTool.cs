using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.Annot;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public enum CustomizeToolType
    {
        kUnknown,
        kErase,
    }
    internal class CreateCustomizeTool : CustomizeLayer
    {
        /// <summary>
        /// Start drawing point
        /// </summary>
        protected Point mouseStartPoint { get; set; }

        /// <summary>
        /// End drawing point
        /// </summary>
        protected Point mouseEndPoint { get; set; }

        /// <summary>
        /// Identifies whether the mouse is pressed
        /// </summary>
        protected bool isMouseDown { get; set; }

        /// <summary>
        /// Zoom factor
        /// </summary>
        private double zoomFactor { get; set; } = 1;

        /// <summary>
        /// Rect for drawing
        /// </summary>
        protected Rect drawRect { get; set; }

        /// <summary>
        /// Max drawing rect
        /// </summary>
        protected Rect maxRect { get; set; }

        /// <summary>
        /// Original page range rectangle (calculate offset in continuous mode)
        /// </summary>
        protected Rect pageBound { get; set; }

        /// <summary>
        /// Standard DPI rectangle (without removing half of the pen thickness)
        /// </summary>
        protected Rect DPIRect { get; set; }
        public SolidColorBrush FillBrush;
        public Pen DrawPen;
        public event EventHandler<List<AnnotParam>> DeleteChanged;
        protected DrawingContext drawDC { get; set; }
        private CustomizeToolType customizeToolType { get; set; } = CustomizeToolType.kUnknown;
        private CPDFPage currentPage = null;
        private AnnotLayer annotLayer { get; set; } = null;

        private double defEraseThickness = 5;

        private double eraseZoom = 1;
        private SolidColorBrush eraseBrush { get; set; } = new SolidColorBrush(Colors.LightGray);
        internal CPDFViewer PDFViewer { get; set; }
        public CreateCustomizeTool()
        {

        }

        public void SetDefEraseThickness(double defEraseThickness)
        {
            this.defEraseThickness = defEraseThickness;
        }

        public void SetEraseZoom(double eraseZoom)
        {
            this.eraseZoom = eraseZoom;
        }

        public void SetEraseBrush(SolidColorBrush drawBrush)
        {
            if(drawBrush == null)
            {
                return;
            }
            eraseBrush = drawBrush;
        }

        public void SetAnnotLayer(AnnotLayer layer)
        {
            annotLayer = layer;
        }

        public void StartDraw(Point downPoint, CPDFPage cPDFPage, Rect maxRect, Rect pageBound, CustomizeToolType ToolType)
        {
            customizeToolType = ToolType;
            mouseStartPoint = downPoint;
            isMouseDown = true;
            this.maxRect = maxRect;
            this.pageBound = pageBound;
            DPIRect = new Rect();
            currentPage = cPDFPage;
        }

        public void MoveDraw(Point downPoint, double zoom)
        {
            if (isMouseDown)
            {
                mouseEndPoint = downPoint;
                zoomFactor = zoom;
                DrawTool();
            }
        }

        public void EndDraw()
        {
            if (isMouseDown)
            {
                isMouseDown = false;
                mouseStartPoint = new Point();
                mouseEndPoint = new Point();
                pageBound = new Rect();
                DPIRect = new Rect();
                currentPage = null;
                annotLayer = null;
            }
        }

        public void DrawTool()
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();

                switch (customizeToolType)
                {
                    case CustomizeToolType.kUnknown:
                        break;
                    case CustomizeToolType.kErase:
                        DrawErase(drawDC);
                        break;
                    default:
                        break;
                }

                Present();
            });
        }

        public void ClearDraw()
        {
            Open();
            Present();
        }

        /// <summary>
        /// Erase the hand-drawn
        /// </summary>
        /// <returns>
        /// 1 delete 0 erase -1 not intersect
        /// </returns>
        private int ErasePoint(InkAnnot AnnotCore, Rect eraseRect)
        {
            Rect rawErase = eraseRect;
            List<List<Point>> addPointList = new List<List<Point>>();
            List<List<Point>> RawPointList = new List<List<Point>>();
            CPDFInkAnnotation AnnotInk = (AnnotCore.GetAnnotData().Annot as CPDFInkAnnotation);
            bool isErasePoint = false;
            if (AnnotInk.InkPath != null)
            {
                foreach (var Item in AnnotInk.InkPath)
                {
                    List<Point> PointList = new List<Point>();
                    foreach (var RawPoint in Item)
                    {
                        PointList.Add(DpiHelper.PDFPointToStandardPoint(new Point(RawPoint.x, RawPoint.y)));
                    }
                    RawPointList.Add(PointList);
                }
            }
            foreach (List<Point> Item in RawPointList)
            {
                List<Point> addItem = new List<Point>();

                foreach (Point checkPoint in Item)
                {
                    if (rawErase.Contains(checkPoint) == false)
                    {
                        addItem.Add(checkPoint);
                    }
                    else
                    {
                        isErasePoint = true;
                        if (addItem.Count > 2)
                        {
                            addPointList.Add(addItem);
                        }
                        addItem = new List<Point>();
                    }
                }
                if (addItem.Count > 2)
                {
                    addPointList.Add(addItem);
                }
            }
            RawPointList = addPointList;

            if (addPointList.Count == 0)
            {
                //delete annot
                return 1;
            }

            List<List<CPoint>> inkPathList = new List<List<CPoint>>();
            CPDFInkAnnotation annotInk = (AnnotCore.GetAnnotData().Annot as CPDFInkAnnotation);
            foreach (List<Point> inkNode in RawPointList)
            {
                List<CPoint> inkPath = new List<CPoint>();
                foreach (Point addPoint in inkNode)
                {
                    inkPath.Add(new CPoint((float)DpiHelper.StandardNumToPDFNum(addPoint.X), (float)DpiHelper.StandardNumToPDFNum(addPoint.Y)));
                }

                inkPathList.Add(inkPath);
            }

            if (isErasePoint)
            {
                if (!annotInk.IsValid())
                {
                    return -1;
                }
             
                annotInk.SetInkPath(inkPathList);
                annotInk.UpdateAp();
                AnnotCore.Draw();
            }

            return isErasePoint ? 0 : -1;
        }
        private void DrawErase(DrawingContext drawingContext)
        {
            Rect drawRect = new Rect(mouseEndPoint.X - defEraseThickness* eraseZoom, mouseEndPoint.Y - defEraseThickness * eraseZoom, defEraseThickness * 2* eraseZoom, defEraseThickness * 2* eraseZoom); 
            Rect eraseRect = new Rect((drawRect.Left - pageBound.Left)/zoomFactor,
                       (drawRect.Top - pageBound.Top)/zoomFactor,
                        drawRect.Width / zoomFactor,
                        drawRect.Height / zoomFactor);
            drawingContext?.DrawEllipse(eraseBrush, null, new Point(mouseEndPoint.X, mouseEndPoint.Y), defEraseThickness* eraseZoom, defEraseThickness* eraseZoom);
            if (annotLayer==null)
            {
                return;
            }
            List<BaseAnnot> annotControlList= annotLayer.GetAnnotListForType(C_ANNOTATION_TYPE.C_ANNOTATION_INK);
            GroupHistory historyGroup=new GroupHistory();
            CPDFDocument pdfDoc = PDFViewer?.GetDocument();
            List<AnnotParam> paramList = new List<AnnotParam>();
            foreach (var item in annotControlList)
            {
                InkAnnot ink = item as InkAnnot;
                int Tag = ErasePoint(ink, eraseRect);
                if (Tag == 1)
                {
                    CPDFAnnotation delAnnot = item.GetAnnotData().Annot;
                    AnnotHistory annotHistory = ParamConverter.CreateHistory(delAnnot);
                    AnnotParam annotParam = null;
                    if (pdfDoc != null)
                    {
                        annotParam = ParamConverter.CPDFDataConverterToAnnotParam(pdfDoc, delAnnot.Page.PageIndex, delAnnot);
                        annotHistory.CurrentParam = annotParam;
                        annotHistory.Action=HistoryAction.Remove;
                        annotHistory.PDFDoc=pdfDoc;
                        historyGroup.Histories.Add(annotHistory);
                    }

                    if (delAnnot.RemoveAnnot())
                    {
                        if(annotParam != null)
                        {
                            paramList.Add(annotParam);
                        }
                        historyGroup.Histories.Add(annotHistory);
                    }
                }
            }

            if(historyGroup.Histories.Count > 0 && PDFViewer!=null)
            {
                PDFViewer.UndoManager.AddHistory(historyGroup);
            }
            if(paramList.Count > 0)
            {
                DeleteChanged?.Invoke(this, paramList);
            }
        }
    }
}
