using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ComPDFKit.Viewer.Layer;
using System.Windows.Media;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer;
using ComPDFKit.PDFAnnotation;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        bool isDrawSelectRect = false;
        int selectedRectViewTag = -1;
        private bool isOutSideScaling = false;
        public event EventHandler<SelectedAnnotData> SelectedDataChanging;
        public event EventHandler<SelectedAnnotData> SelectedDataChanged;

        private void InsertSelectedRectView()
        {
            int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();
            SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.Annot);
            selectedRect.SetDrawMoveType(DrawMoveType.kDefault);
            customizeLayer.Children.Add(selectedRect);
            selectedRect.DataChanged += SelectedRect_DataChanged;
            selectedRect.DataChanging += SelectedRect_DataChanging;
            PDFViewer.InsertView(selectedRectViewIndex, customizeLayer);
            selectedRectViewTag= customizeLayer.GetResTag();
        }

        /// <summary>
        /// Set whether the border can be moved outside the border
        /// </summary>
        /// <param name="IsOutSideScaling"></param>
        public void SetOutSideScaling(bool IsOutSideScaling)
        {
            isOutSideScaling = IsOutSideScaling;
        }

        private void SelectedRect_DataChanging(object sender, SelectedAnnotData e)
        {
            SelectedDataChanging?.Invoke(this, e);
        }

        private void SelectedRect_DataChanged(object sender, SelectedAnnotData e)
        {
            SelectedDataChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Start to draw the rectangle
        /// </summary>
        public void DrawStartSelectedRect()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.Draw();
                selectedRect.OnMouseLeftButtonDown(point);
                isDrawSelectRect = true;
            }
        }

        public Cursor GetMoveSelectedRectCursor()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                return selectedRect.GetCursor(point, this.Cursor);
            }
            return this.Cursor;
        }

        /// <summary>
        /// Draw the rectangle when dragging
        /// </summary>
        public bool DrawMoveSelectedRect()
        {
            bool DrawTag = false;
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.SetOutSideScaling(isOutSideScaling);
                selectedRect.OnMouseMove(point, out DrawTag,PDFViewer.ActualWidth,PDFViewer.ActualHeight);
                selectedRect.Draw();
            }
            return DrawTag;
        }

        /// <summary>
        /// End of drawing the rectangle
        /// </summary>
        public void DrawEndSelectedRect()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.OnMouseLeftButtonUp(point);
                selectedRect.Draw();
            }
        }

        /// <summary>
        /// Clear the rectangle drawing
        /// </summary>
        public void CleanSelectedRect()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.ClearDraw();
                isDrawSelectRect = false;
            }
        }

        private void SelectedAnnot()
        {
            if (!isHitTestLink&& cacheHitTestAnnot?.CurrentType== C_ANNOTATION_TYPE.C_ANNOTATION_LINK)
            {
                return ;
            }
            if (isHitTestRedact && cacheHitTestAnnot?.CurrentType!= C_ANNOTATION_TYPE.C_ANNOTATION_REDACT)
            {
                return ;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);
            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.SetAnnotData(cacheHitTestAnnot.GetAnnotData());
            }
        }

        private void SelectedAnnot(AnnotData annotData)
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);
            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                if (annotData==null)
                {
                    selectedRect.ClearDraw();
                }
                else
                {
                    selectedRect.SetAnnotData(annotData);
                }
            }
        }

        /// <summary>
        /// Refresh the drawing
        /// </summary>
        private void DrawSelectedLayer()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);
            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                selectedRect.Draw();
            }
        }
        
        /// <summary>
        /// Identify whether the mouse is on the rectangle
        /// </summary>
        /// <returns></returns>
        private bool DrawSelectRectDownEvent()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectedRectViewTag);

            SelectedRect selectedRect = CommonHelper.FindVisualChild<SelectedRect>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                if (selectedRect.GetHitControlIndex(Mouse.GetPosition(this)) != PointControlType.None)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
