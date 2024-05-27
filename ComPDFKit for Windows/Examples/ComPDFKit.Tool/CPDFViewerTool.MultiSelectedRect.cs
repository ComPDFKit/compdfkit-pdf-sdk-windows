using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ComPDFKit.Tool
{
    public class MultiSelectedData
    {
        public int PageIndex { get; set; }

        public List<int> MultiObjectIndex { get; set; }

        /// <summary>
        /// Current multi-select type
        /// </summary>
        public SelectedType ObjectType { get; set; }

        /// <summary>
        /// Move offset value of the whole
        /// </summary>
        public Point MoveOffset { get; set; }

        public float ZoomX { get; set; }

        public float ZoomY { get; set; }
    }

    public partial class CPDFViewerTool
    {
        int multiSelectedRectViewTag = -1;
        List<int> editAreaMultiIndex = new List<int>();
        int multiPage = -1;
        public event EventHandler<MultiSelectedData> MultiDataChanging;
        public event EventHandler<MultiSelectedData> MultiDataChanged;
        private void InsertMultiSelectedRectView()
        {
            int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();
            MultiSelectedRect multiSelectedRect = new MultiSelectedRect(GetDefaultDrawParam(), SelectedType.None);
            multiSelectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
            customizeLayer.Children.Add(multiSelectedRect);
            multiSelectedRect.DataChanged += MultiSelectedRect_DataChanged;
            multiSelectedRect.DataChanging += MultiSelectedRect_DataChanging;
            PDFViewer.InsertView(selectedRectViewIndex, customizeLayer);
            multiSelectedRectViewTag = customizeLayer.GetResTag();
            //multiSelectedRect.Children.Add(multiSelectedRect);
        }

        private void MultiSelectedRect_DataChanging(object sender, Point e)
        {
            MultiSelectedData multiSelectedAnnotData = new MultiSelectedData();
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {
                multiSelectedAnnotData.ZoomX = multiSelectedRect.GetZoomX();
                multiSelectedAnnotData.ZoomY = multiSelectedRect.GetZoomY();
                multiSelectedAnnotData.MoveOffset = e;
                multiSelectedAnnotData.ObjectType = multiSelectedRect.GetSelectedType();
                multiSelectedAnnotData.MultiObjectIndex = new List<int>();
                multiSelectedAnnotData.MultiObjectIndex.AddRange(editAreaMultiIndex);
                multiSelectedAnnotData.PageIndex = multiPage;
                MultiDataChanging?.Invoke(this, multiSelectedAnnotData);
            }
        }

        private void MultiSelectedRect_DataChanged(object sender, Point e)
        {
            MultiSelectedData multiSelectedAnnotData = new MultiSelectedData();
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {
                multiSelectedAnnotData.ZoomX = multiSelectedRect.GetZoomX();
                multiSelectedAnnotData.ZoomY = multiSelectedRect.GetZoomY();
                multiSelectedAnnotData.MoveOffset = e;
                multiSelectedAnnotData.ObjectType = multiSelectedRect.GetSelectedType();
                multiSelectedAnnotData.MultiObjectIndex = new List<int>();
                multiSelectedAnnotData.MultiObjectIndex.AddRange(editAreaMultiIndex);
                multiSelectedAnnotData.PageIndex = multiPage;
                MultiDataChanged?.Invoke(this, multiSelectedAnnotData);
            }
        }

        bool isOpen = false;
        public void OpenSelectedMulti(bool open)
        {
            if (!open)
            {
                CleanSelectedMultiRect();
            }
            isOpen = open;
        }

        public bool HitTestMultiSelectedRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {
                if (multiSelectedRect.GetHitControlIndex(Mouse.GetPosition(this)) != PointControlType.None)
                {
                    return true;
                }
            }
            return false;
        }

        public void SelectedMultiRect(Rect selectedRects, Rect MaxRect, SelectedType type)
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {

                GetSelectedEditAreaForIndex(out int pageIndex, out int editAreaIndex);
                if (multiPage != pageIndex && editAreaMultiIndex.Count > 0)
                {
                    foreach (int item in editAreaMultiIndex)
                    {
                        SelectedRect OldRect = GetEditAreaForIndex(multiPage, item);
                        if (OldRect != null)
                        {
                            OldRect.Draw();
                        }
                    }
                    editAreaMultiIndex.Clear();
                    multiSelectedRect.ClearDraw();
                    multiSelectedRect.CleanMulitSelectedRect();
                    multiPage = pageIndex;
                }
                multiPage = pageIndex;
                editAreaMultiIndex.Add(editAreaIndex);

                multiSelectedRect.SetSelectedType(type);
                SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), type);
                selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                selectedRect.SetRect(selectedRects,currentZoom);
                selectedRect.SetMaxRect(MaxRect);
                multiSelectedRect.Children.Add(selectedRect);
                multiSelectedRect.SetMulitSelectedRect(selectedRect);

                multiSelectedRect.SetRect(selectedRects);
                multiSelectedRect.SetMaxRect(MaxRect);
                multiSelectedRect.Draw();
            }

        }

        public void HideDrawSelectedMultiRect()
        {
            foreach (int item in editAreaMultiIndex)
            {
                SelectedRect OldRect = GetEditAreaForIndex(multiPage, item);
                if (OldRect != null)
                {
                    OldRect.HideDraw();
                }
            }
        }

        public void CleanSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));
            if (multiSelectedRect != null)
            {
                multiSelectedRect.Children.Clear();
                multiSelectedRect.CleanMulitSelectedRect();
                editAreaMultiIndex.Clear();
            }
        }

        public void DrawStartSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.Draw();
                multiSelectedRect.OnMouseLeftButtonDown(point);
            }
        }

        public void DrawMoveSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
            }
        }

        public void DrawEndSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.OnMouseLeftButtonUp(point);
            }
        }

        public void ReDrawSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(multiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                multiSelectedRect.ClearDraw();
                multiSelectedRect.CleanMulitSelectedRect();

                Point point = Mouse.GetPosition(this);

                switch (multiSelectedRect.GetSelectedType())
                {
                    case SelectedType.Annot:
                        SelectAnnot();
                        break;
                    case SelectedType.PDFEdit:
                        SelectPDFEdit(multiSelectedRect);
                        break;
                    default:
                        break;
                }
                multiSelectedRect.Draw();
            }
        }

        private void SelectAnnot()
        {
            CleanSelectedRect();
        }

        private void SelectPDFEdit(MultiSelectedRect multiSelectedRect)
        {
            foreach (int item in editAreaMultiIndex)
            {
                SelectedRect OldRect = GetEditAreaForIndex(multiPage, item);
                if (OldRect != null)
                {
                    multiSelectedRect.SetSelectedType(SelectedType.PDFEdit);
                    SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
                    selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                    selectedRect.SetRect(OldRect.GetRect(),currentZoom);
                    selectedRect.SetMaxRect(OldRect.GetMaxRect());
                    multiSelectedRect.Children.Add(selectedRect);
                    multiSelectedRect.SetMulitSelectedRect(selectedRect);
                    multiSelectedRect.SetRect(OldRect.GetRect());
                    multiSelectedRect.SetMaxRect(OldRect.GetMaxRect());
                    OldRect.HideDraw();
                }
            }
        }
    }
}
