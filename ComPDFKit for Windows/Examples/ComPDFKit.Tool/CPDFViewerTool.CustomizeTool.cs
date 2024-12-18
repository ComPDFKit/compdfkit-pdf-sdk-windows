using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFDocument;
using ComPDFKitViewer.BaseObject;
using ComPDFKit.PDFAnnotation.Form;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        /// <summary>
        ///Identity whether to draw the custom tool
        /// </summary>
        protected bool isDrawAnnot { get; set; }

        public event EventHandler<List<AnnotParam>> DeleteChanged;

        int customizeToolViewTag = -1;

        private bool UnCheckCustomizeToolViewerModel()
        {
            if (currentModel == ToolType.Customize)
            {
                return false;
            }
            return true;
        }

        private void InsertCustomizeToolView()
        {
            CreateCustomizeTool createAnnotTool = new CreateCustomizeTool();
            createAnnotTool.PDFViewer = PDFViewer;
            int customizeToolViewindex = PDFViewer.GetMaxViewIndex();
            createAnnotTool.DeleteChanged += CreateAnnotTool_DeleteChanged;
            PDFViewer.InsertView(customizeToolViewindex, createAnnotTool);
            customizeToolViewTag= createAnnotTool.GetResTag();
        }

        private void CreateAnnotTool_DeleteChanged(object sender, List<AnnotParam> e)
        {
            DeleteChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Begin to draw the custom tool
        /// </summary>
        public void DrawStartCustomizeTool(CustomizeToolType ToolType)
        {
            if (UnCheckCustomizeToolViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            if (index < 0)
            {
                return;
            }
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
            BaseLayer layer = PDFViewer.GetViewForTag(PDFViewer.GetAnnotViewTag());
            (baseLayer as CreateCustomizeTool).ErasePageIndex = -1;
            if (ToolType == CustomizeToolType.kErase)
            {
                (baseLayer as CreateCustomizeTool).SetAnnotLayer(layer as AnnotLayer);
                (baseLayer as CreateCustomizeTool).ErasePageIndex = index;
            }
           (baseLayer as CreateCustomizeTool).StartDraw(point, cPDFPage, paintRect, pageBound, ToolType);
        }

        /// <summary>
        /// Custom tool move call
        /// </summary>
        public void DrawMoveCustomizeTool()
        {
            if (UnCheckCustomizeToolViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            (baseLayer as CreateCustomizeTool).MoveDraw(point, PDFViewer.GetZoom());
        }

        /// <summary>
        /// Custom tool end call
        /// </summary>
        public void DrawEndCustomizeTool()
        {
            if (UnCheckCustomizeToolViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            (baseLayer as CreateCustomizeTool).EndDraw();
            return;
        }

        /// <summary>
        /// Clear the custom tool
        /// </summary>
        public void CleanCustomizeTool()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            (baseLayer as CreateCustomizeTool).ClearDraw();
        }

        /// <summary>
        /// Set the thickness of the custom tool
        /// </summary>
        public void SetEraseZoom(double eraseZoom)
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            (baseLayer as CreateCustomizeTool)?.SetEraseZoom(eraseZoom);
        }

        /// <summary>
        /// Clear the custom tool
        /// </summary>
        public void SetDefEraseThickness(double defEraseThickness)
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
            (baseLayer as CreateCustomizeTool)?.SetDefEraseThickness(defEraseThickness);
        }

        /// <summary>
        ///  Clear the custom tool
        /// </summary>
        public void SetEraseBrush(SolidColorBrush drawBrush)
        {
            if(PDFViewer!=null)
            {
                BaseLayer baseLayer = PDFViewer.GetViewForTag(customizeToolViewTag);
                (baseLayer as CreateCustomizeTool)?.SetEraseBrush(drawBrush);
            }
        }
    }
}
    