using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer.Layer;
using System;
using System.Windows;
using System.Windows.Input;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        bool IsDrawEditAnnot = false;
        int annotEditViewTag = -1;
        public event EventHandler<SelectedAnnotData> AnnotEditDataChanging;
        public event EventHandler<SelectedAnnotData> AnnotEditDataChanged;
        public event EventHandler<object> AnnotChanged;

        private void InsertAnnotEditView()
        {
            int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();
            AnnotEdit annotEdit = new AnnotEdit(GetDefaultDrawParam());
            annotEdit.DataChanged += AnnotEdit_DataChanged;
            annotEdit.DataChanging += AnnotEdit_DataChanging;
            customizeLayer.Children.Add(annotEdit);
            PDFViewer.InsertView(selectedRectViewIndex, customizeLayer);
            annotEditViewTag = customizeLayer.GetResTag();
        }

        private void AnnotEdit_DataChanging(object sender, SelectedAnnotData e)
        {
            AnnotEditDataChanging?.Invoke(this, e);
        }

        private void AnnotEdit_DataChanged(object sender, SelectedAnnotData e)
        {
            AnnotEditDataChanged?.Invoke(this, e);
        }

        public void StartDrawEditAnnot()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)
            {
                annotEdit.Draw();
                annotEdit.OnMouseLeftButtonDown(point);
                IsDrawEditAnnot = true;
            }
        }

        public bool DrawMoveEditAnnot()
        {
            bool DrawTag = false;
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)
            {
                annotEdit.OnMouseMove(point, out DrawTag);
                if (DrawTag)
                {
                    annotEdit.Draw();
                }
            }
            return DrawTag;
        }

        public void DrawEndEditAnnot()
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)
            {
                annotEdit.OnMouseLeftButtonUp(point);
                annotEdit.Draw();
            }
        }

        public void CleanEditAnnot(bool isDrawEditAnnot = false)
        {
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)  
            {
                annotEdit.ClearDraw();
                IsDrawEditAnnot = isDrawEditAnnot;
            }
        }
  

        private void DrawEditAnnotLayer()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)
            {
                annotEdit.Draw();
            }
        }

        /// <summary>
        /// Press the mouse on the selected rectangle
        /// </summary>
        /// <returns></returns>
        private bool DrawEditAnnotDownEvent()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit selectedRect = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (selectedRect != null)
            {
                if (selectedRect.GetHitIndex(Mouse.GetPosition(this)) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetEditAnnotObject()
        {
            if (cacheHitTestAnnot == null)
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(annotEditViewTag);
            AnnotEdit annotEdit = CommonHelper.FindVisualChild<AnnotEdit>(baseLayer as CustomizeLayer);
            if (annotEdit != null)
            {
                annotEdit.SetAnnotObject(cacheHitTestAnnot.GetAnnotData());
            }
        }
    }   
}
