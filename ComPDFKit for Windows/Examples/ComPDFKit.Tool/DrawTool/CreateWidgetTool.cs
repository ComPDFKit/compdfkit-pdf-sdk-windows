using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFPage;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.Help;
using System.Windows.Annotations;

namespace ComPDFKit.Tool.DrawTool
{
    internal class CreateWidgetTool : CustomizeLayer
    {
        protected CPDFAnnotation cPDFAnnotation { get; set; }

        /// <summary>
        /// End point of the drawing
        /// </summary>
        protected Point mouseEndPoint { get; set; }

        /// <summary>
        /// Crop point
        /// </summary>
        protected Point cropPoint { get; set; }

        protected bool hideDraw { get; set; } = false;

        /// <summary>
        ///Identify whether the annotation is being created
        /// </summary>
        protected bool isDrawAnnot { get; set; }

        /// <summary>
        /// Max drawing range rectangle
        /// </summary>
        protected Rect maxRect { get; set; }

        /// <summary>
        /// Start drawing point
        /// </summary>
        protected Point mouseStartPoint { get; set; }

        /// <summary>
        /// Original page range rectangle (calculate offset in continuous mode)
        /// </summary>
        protected Rect pageBound { get; set; }

        /// <summary>
        /// Standard DPI rectangle (without removing half of the pen thickness)
        /// </summary>
        protected Rect DPIRect { get; set; }

        /// <summary>
        /// Used to set the current zoom ratio
        /// </summary>
        private double zoomFactor { get; set; } = 1;

        protected DrawingContext drawDC { get; set; }

        private double minWidth = 15;

        private double minHeight = 15;

        private double defaultWidth { get; set; } = 0;

        private double defaultHeight { get; set; } = 0;

        private double PDFViewerActualWidth { get; set; } = 0;
        private double PDFViewerActualHeight { get; set; } = 0;

        private C_WIDGET_TYPE currentWidgetType = C_WIDGET_TYPE.WIDGET_UNKNOWN;

        protected DefaultDrawParam drawParam = new DefaultDrawParam();
        
        public void SetDrawType(C_WIDGET_TYPE WidgetType)
        {
            currentWidgetType = WidgetType;
        }

        public void ReDrawWidget(double zoom)
        {
            zoomFactor = zoom;
            Draw();
        }

        public CPDFAnnotation StartDraw(Point downPoint, Point cropPoint, CPDFPage cPDFPage, Rect maxRect, Rect pageBound, C_WIDGET_TYPE WidgetType)
        {
            if (WidgetType== C_WIDGET_TYPE.WIDGET_NONE)
            {
                return null;
            }
            mouseStartPoint = downPoint;
            isDrawAnnot = true;
            this.maxRect = maxRect;
            int newIndex=cPDFPage.GetAnnotCount();
            cPDFAnnotation = cPDFPage.CreateWidget(WidgetType);
            if(cPDFAnnotation!=null)
            {
                cPDFAnnotation.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                cPDFAnnotation.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                List<CPDFAnnotation> annotList= cPDFPage.GetAnnotations();
               cPDFAnnotation = annotList[newIndex];
            }
            this.cropPoint = cropPoint;
            this.pageBound = pageBound;
            DPIRect = new Rect();
            return cPDFAnnotation;
        }

        public void MoveDraw(Point downPoint, double zoom, double width, double height, bool Hide, Rect maxRect)
        {
            hideDraw = Hide;
            PDFViewerActualWidth = width;
            PDFViewerActualHeight = height;

            mouseEndPoint = downPoint;
            zoomFactor = zoom;
            if (!isDrawAnnot)
            {
                this.maxRect = maxRect;
            }
            Draw();
        }

        public Rect EndDraw()
        {
            if (isDrawAnnot)
            {
                Rect rect = DPIRect;
                if (rect.Width<=1&& rect.Height <= 1)
                {
                    rect = new Rect(mouseStartPoint.X, mouseStartPoint.Y, defaultWidth * zoomFactor, defaultHeight * zoomFactor);
                }
                if (rect.Width < minWidth)
                {
                    rect.Width = defaultWidth * zoomFactor;
                }
                if (rect.Height < minHeight)
                {
                    rect.Height = defaultHeight * zoomFactor;
                }
                rect.Intersect(maxRect);
                
                Rect StandardRect = new Rect(
                        (rect.Left - pageBound.X + (cropPoint.X * zoomFactor)) / zoomFactor,
                        (rect.Top - pageBound.Y + (cropPoint.Y * zoomFactor)) / zoomFactor,
                        rect.Width / zoomFactor, rect.Height / zoomFactor);
                isDrawAnnot = false;
                mouseStartPoint = new Point();
                mouseEndPoint = new Point();
                pageBound = new Rect();
                DPIRect = new Rect();
                cPDFAnnotation = null;
                return DpiHelper.StandardRectToPDFRect(StandardRect);
            }
            return new Rect();
        }

        public override void Draw()
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();

                if (hideDraw||currentWidgetType == C_WIDGET_TYPE.WIDGET_NONE||(cPDFAnnotation == null && isDrawAnnot))
                {
                    Present();
                    return;
                }

                Point DrawPoint = new Point();
                if (isDrawAnnot)
                {
                    DrawPoint = mouseStartPoint;
                }
                else
                {
                    DrawPoint = mouseEndPoint;
                }
                if (!maxRect.Contains(DrawPoint))
                {
                    Present();
                    return;
                }

                drawDC?.DrawLine(drawParam.CreateWidgetPen, new Point(0, DrawPoint.Y), new Point(PDFViewerActualWidth, DrawPoint.Y));
                drawDC?.DrawLine(drawParam.CreateWidgetPen, new Point(DrawPoint.X, 0), new Point(DrawPoint.X, PDFViewerActualHeight));
                if (!isDrawAnnot)
                {
                    switch (currentWidgetType)
                    {
                        case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                            DefaultPushButton();
                            break;
                        case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                        case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                            DefaultRadioButtonOrCheckBox();
                            break;
                        case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                        case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                            DefaultTextBoxOrComboBox();
                            break;
                        case C_WIDGET_TYPE.WIDGET_LISTBOX:
                            DefaultListBox();
                            break;
                        case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                            DefaultSign();
                            break;
                        case C_WIDGET_TYPE.WIDGET_UNKNOWN:
                            break;
                        default:
                            break;
                    }
                    Rect rect = new Rect(mouseEndPoint.X, mouseEndPoint.Y, defaultWidth * zoomFactor, defaultHeight * zoomFactor);

                    DPIRect = rect;
                    drawDC?.DrawRectangle(null, drawParam.CreateWidgetPen, rect);
                }
                else
                {
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
                    drawDC?.DrawRectangle(null, drawParam.CreateWidgetPen, DPIRect);
                }
                Present();
            });
        }

        private void DefaultRadioButtonOrCheckBox()
        {
            defaultWidth = 30;
            defaultHeight = 30;
        }

        private void DefaultTextBoxOrComboBox()
        {
            defaultWidth = 200;
            defaultHeight = 40;
        }

        private void DefaultListBox()
        {
            defaultWidth = 200;
            defaultHeight = 130;
        }

        private void DefaultPushButton()
        {
            defaultWidth = 200;
            defaultHeight = 50;
        }

        private void DefaultSign()
        {
            defaultWidth = 200;
            defaultHeight = 80;
        }
        public virtual void ClearDraw()
        {
            Open();
            Present();
        }
    }
}
