using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Multiple selection of movement distance
        /// </summary>
        public float ChangeX { get; set; }

        /// <summary>
        /// Multiple selection of movement distance
        /// </summary>
        public float ChangeY { get; set; }
    }

    public partial class CPDFViewerTool
    {
        public event EventHandler<MultiSelectedData> MultiDataChanging;
        public event EventHandler<MultiSelectedData> MultiDataChanged;
        public int MultiSelectedRectViewTag { get; set; } = -1;

        private List<int> editAreaMultiIndex = new List<int>();
        private int multiPage = -1;
        private bool isOpen = false;
        private Key multiKey = Key.LeftCtrl;

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
            MultiSelectedRectViewTag = customizeLayer.GetResTag();
            //multiSelectedRect.Children.Add(multiSelectedRect);
        }

        private void MultiSelectedRect_DataChanging(object sender, Point e)
        {
            MultiSelectedData multiSelectedAnnotData = new MultiSelectedData();
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {
                multiSelectedAnnotData.ZoomX = multiSelectedRect.GetZoomX();
                multiSelectedAnnotData.ZoomY = multiSelectedRect.GetZoomY();
                multiSelectedAnnotData.ChangeX = multiSelectedRect.GetChangeX();
                multiSelectedAnnotData.ChangeY = multiSelectedRect.GetChangeY();
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
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (isOpen && multiSelectedRect != null)
            {
                multiSelectedAnnotData.ZoomX = multiSelectedRect.GetZoomX();
                multiSelectedAnnotData.ZoomY = multiSelectedRect.GetZoomY();
                multiSelectedAnnotData.ChangeX = multiSelectedRect.GetChangeX();
                multiSelectedAnnotData.ChangeY = multiSelectedRect.GetChangeY();
                multiSelectedAnnotData.MoveOffset = e;
                multiSelectedAnnotData.ObjectType = multiSelectedRect.GetSelectedType();
                multiSelectedAnnotData.MultiObjectIndex = new List<int>();
                multiSelectedAnnotData.MultiObjectIndex.AddRange(editAreaMultiIndex);
                multiSelectedAnnotData.PageIndex = multiPage;
                MultiDataChanged?.Invoke(this, multiSelectedAnnotData);
            }
        }

        /// <summary>
        /// Set multiple selection shortcut keys
        /// </summary>
        /// <param name="multikey"></param>
        public void SetMultiSelectKey(Key multikey)
        {
            multiKey = multikey;
        }

        private void OpenSelectedMulti(bool open)
        {
            isOpen = open;
        }

        public bool HitTestMultiSelectedRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
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
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            bool open = isOpen;
            if (!Keyboard.IsKeyDown(multiKey))
            {
                open = false;
            }
            if (open && multiSelectedRect != null)
            {
                lastSelectedRect.ClearDraw();
                lastSelectedRect.HideDraw();
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
                if (startSelectedRect != null && startSelectedPageIndex == multiPage && !editAreaMultiIndex.Contains(startSelectedIndex))
                {
                    //Add the first selected object
                    editAreaMultiIndex.Add(startSelectedIndex);

                    SelectedRect startselectedrect = new SelectedRect(GetDefaultDrawParam(), type);
                    startselectedrect.SetEditPen(editPen, editHoverPen);
                    startselectedrect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                    startselectedrect.SetRect(startSelectedRect.GetRect(), currentZoom);
                    startselectedrect.SetMaxRect(MaxRect);
                    editAreaList.Add(startselectedrect, startSelectedEditAreaObject);
                    multiSelectedRect.Children.Add(startselectedrect);

                    multiSelectedRect.SetMulitSelectedRect(startselectedrect, startSelectedEditAreaObject.PageIndex, startSelectedEditAreaObject.EditAreaIndex);
                    multiSelectedRect.SetRect(startselectedrect.GetRect());
                    multiSelectedRect.SetMaxRect(startselectedrect.GetMaxRect());
                    multiSelectedRect.Draw();
                }
                startSelectedRect = null;
                startSelectedIndex = -1;
                startSelectedPageIndex = -1;
                startSelectedEditAreaObject = null;
                editAreaMultiIndex.Add(editAreaIndex);
                multiSelectedRect.SetSelectedType(type);
                SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), type);
                selectedRect.SetEditPen(editPen, editHoverPen);
                selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                selectedRect.SetRect(selectedRects, currentZoom);
                selectedRect.SetMaxRect(MaxRect);
                EditAreaObject editAreaObject = GetEditAreaObjectForRect(lastSelectedRect);
                editAreaList.Add(selectedRect, editAreaObject);
                multiSelectedRect.Children.Add(selectedRect);
                multiSelectedRect.SetMulitSelectedRect(selectedRect, editAreaObject.PageIndex, editAreaObject.EditAreaIndex);

                multiSelectedRect.SetRect(selectedRects);
                multiSelectedRect.SetMaxRect(MaxRect);
                multiSelectedRect.Draw();
            }
            else
            {
                //Remember the first selected object
                isOpen = false;
                GetSelectedEditAreaForIndex(out int pageIndex, out int editAreaIndex);
                SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), type);
                selectedRect.SetEditPen(editPen, editHoverPen);
                selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                selectedRect.SetRect(selectedRects, currentZoom);
                selectedRect.SetMaxRect(MaxRect);
                EditAreaObject editAreaObject = GetEditAreaObjectForRect(lastSelectedRect);
                if (startSelectedIndex != editAreaIndex || startSelectedPageIndex != pageIndex)
                {
                    startSelectedIndex = editAreaIndex;
                    startSelectedPageIndex = pageIndex;
                    startSelectedEditAreaObject = editAreaObject;
                    startSelectedRect = selectedRect;
                    editAreaList.Add(startSelectedRect, editAreaObject);
                }
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
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (multiSelectedRect != null)
            {
                multiSelectedRect.Children.Clear();
                multiSelectedRect.CleanMulitSelectedRect();
                editAreaMultiIndex.Clear();
                //Delete Multiple Selection Record List
                editAreaList.Clear();
            }
        }

        public void DrawStartSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.Draw();
                multiSelectedRect.OnMouseLeftButtonDown(point);
            }
        }

        public void DrawMoveSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));

            if (multiSelectedRect != null&& multiSelectedRect.Children.Count>0)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
            }
        }

        public void DrawEndSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));

            if (multiSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                multiSelectedRect.OnMouseLeftButtonUp(point);
            }
        }

        public void ReDrawSelectedMultiRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            DrawEndFrameSelect();
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
            //Delete Multiple Selection Box
            foreach (int item in editAreaMultiIndex)
            {
                SelectedRect OldRect = GetEditAreaForIndex(multiPage, item);
                if (OldRect != null)
                {
                    multiSelectedRect.SetSelectedType(SelectedType.PDFEdit);
                    //Optimize logic to prevent overlapping of multiple selected objects
                    //SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
                    //selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                    //selectedRect.SetRect(OldRect.GetRect(), currentZoom);
                    //selectedRect.SetMaxRect(OldRect.GetMaxRect());
                    //multiSelectedRect.Children.Add(selectedRect);
                    multiSelectedRect.SetMulitSelectedRect(OldRect, multiPage,item);
                    multiSelectedRect.SetRect(OldRect.GetRect());
                    multiSelectedRect.SetMaxRect(OldRect.GetMaxRect());
                    OldRect.HideDraw();
                }
            }
        }
    }
}
