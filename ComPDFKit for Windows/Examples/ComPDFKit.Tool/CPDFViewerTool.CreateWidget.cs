using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using ComPDFKitViewer.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        BaseWidget cacheMoveWidget;
        int createWidgetTag = -1;

        public BaseWidget GetCacheHitTestWidget()
        {
            return cacheMoveWidget;
        }
        private void InsertWidgetView()
        {
            CreateWidgetTool createAnnotTool = new CreateWidgetTool();
            int annotViewindex = PDFViewer.GetMaxViewIndex();
            PDFViewer.InsertView(annotViewindex, createAnnotTool);
            createWidgetTag = createAnnotTool.GetResTag();
        }

        protected bool AnnotWidgetHitTest()
        {
            BaseAnnot baseAnnot = PDFViewer.AnnotHitTest();
            if (baseAnnot != null)
            {
                if ((baseAnnot as BaseWidget) != null)
                {
                    cacheMoveWidget = baseAnnot as BaseWidget;
                    return true;
                }
            }
            cacheMoveWidget = null;
            return false;
        }

        public CPDFAnnotation StartDrawWidget(C_WIDGET_TYPE WidgetType)
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            if (index < 0)
            {
                return null;
            }
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
            Point cropPoint = new Point();
            if (PDFViewer.GetIsCrop())
            {
                CRect cRect = cPDFPage.GetCropBounds();
                cropPoint.X = DpiHelper.PDFNumToStandardNum(cRect.left);
                cropPoint.Y = DpiHelper.PDFNumToStandardNum(cRect.top);
            }
            return (baseLayer as CreateWidgetTool).StartDraw(point, cropPoint, cPDFPage, paintRect, pageBound, WidgetType);
        }

        public void SetDrawWidgetType(C_WIDGET_TYPE WidgetType)
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            (baseLayer as CreateWidgetTool).SetDrawType(WidgetType);
        }

        /// <summary>
        /// Widget move in create mode
        /// </summary>
        /// <param name="Hide">
        /// Identify whether the default size style of the widget form is displayed
        /// </param>
        public void MoveDrawWidget(bool Hide)
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            (baseLayer as CreateWidgetTool).MoveDraw(point, PDFViewer.GetZoom(), PDFViewer.ActualWidth, PDFViewer.ActualHeight, Hide, paintRect);
        }

        public Rect EndDrawWidget()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            return (baseLayer as CreateWidgetTool).EndDraw();
        }

        public void ClearDrawWidget()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            if (baseLayer is CreateWidgetTool)
            {
                (baseLayer as CreateWidgetTool).ClearDraw();
                (baseLayer as CreateWidgetTool).SetDrawType(C_WIDGET_TYPE.WIDGET_NONE);
            }
        }

        public void ReDrawWidget()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(createWidgetTag);
            if (baseLayer is CreateWidgetTool)
            {
                (baseLayer as CreateWidgetTool).ReDrawWidget(PDFViewer.GetZoom());
            }
        }
    }
}
