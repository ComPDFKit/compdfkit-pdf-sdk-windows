using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.DrawTool;
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    /// <summary>
    /// Single text block related parameters
    /// </summary>
    public class EditAreaObject
    {
        internal CPDFEditArea cPDFEditArea { get; set; }

        internal CPDFEditPage cPDFEditPage { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        internal int PageIndex { get; set; }

        /// <summary>
        /// EditArea index of the current page
        /// </summary>
        internal int EditAreaIndex { get; set; }

        /// <summary>
        /// Draw the rectangle of the complete page
        /// </summary>
        public Rect PageBound { get; set; }

        /// <summary>
        /// Draw the rectangle of the visible range
        /// </summary>
        public Rect PaintRect { get; set; }

        /// <summary>
        /// State of the current control point
        /// </summary>
        public PointControlType ControlType { get; set; }
    }

    public enum CEditingLocation
    {
        CEditingLocationLineBegin = 0,
        CEditingLoadTypeLineEnd,
        CEditingLoadTypeSectionBegin,
        CEditingLoadTypeSectionEnd,
        CEditingLoadTypePreWord,
        CEditingLoadTypeNextWord,
        CEditingLoadTypePreCharPlace,
        CEditingLoadTypeNextCharPlace,
        CEditingLoadTypeUpCharPlace,
        CEditingLoadTypeDownCharPlace,
    }

    public enum AlignModes
    {
        AlignNone,
        AlignLeft,
        AlignVerticalCenter,
        AlignRight,
        AlignTop,
        AlignHorizonCenter,
        AlignBottom,
        DistributeVertical,
        DistributeHorizontal
    }

    public partial class CPDFViewerTool
    {
        /// <summary>
        /// Expend the area for taking pictures to avoid the edge of the picture not being completely covered due to accuracy
        /// </summary>
        private double editPadding = 5;
        private Point pointtest;
        private int pageindex;
        private SelectedRect lastSelectedRect = null;
        //start Multiple selection of first data record
        private SelectedRect startSelectedRect = null;
        private EditAreaObject startSelectedEditAreaObject = null;
        private int startSelectedIndex = -1;
        private int startSelectedPageIndex = -1;
        //end

        private SelectedRect lastHoverRect = null;
        private int cropIndex = -1;
        private int textEditTag = -1;
        private double currentZoom;
        private EditAreaObject currentEditAreaObject = null;
        private int selectedEditPageIndex = -1;
        private bool drawCaret = true;
        private int selectedEditAreaIndex = -1;
        private bool selectAllCharsForLine = false;
        private CPoint rawHitPos;
        private CPDFEditType contentEditType = CPDFEditType.EditText | CPDFEditType.EditImage;

        /// <summary>
        /// Save Current Crop Box
        /// </summary>
        private Thickness ClipThickness = new Thickness(0, 0, 0, 0);

        /// <summary>
        /// Input variable string
        /// </summary>
        private StringBuilder delayTextBuilder { get; set; }
        private int delayCount = 0;

        /// <summary>
        /// Node of the object used for drawing when selecting/creating text in the current view tree, for easy removal
        /// </summary>
        private int operateChildrenIndex = -1;

        /// <summary>
        ///  Pressed point of selected text
        /// </summary>
        private Point pressPoint;

        private Dictionary<SelectedRect, EditAreaObject> editArea = new Dictionary<SelectedRect, EditAreaObject>();

        /// <summary>
        /// Multiple selection record list
        /// </summary>
        private Dictionary<SelectedRect, EditAreaObject> editAreaList = new Dictionary<SelectedRect, EditAreaObject>();

        /// <summary>
        /// Cache the hit test rectangle area when loading the text block each time
        /// </summary>
        private List<SelectedRect> hitTestRects = new List<SelectedRect>();

        private List<SelectedRect> image = new List<SelectedRect>();

        private List<SelectedRect> text = new List<SelectedRect>();

        protected List<PointControlType> ignoreTextPoints { get; set; } = new List<PointControlType>();

        protected List<PointControlType> ignoreImagePoints { get; set; } = new List<PointControlType>();

        protected DrawPointType drawEditPointType = DrawPointType.Square;

        protected Pen editPen = null;

        protected Pen editHoverPen = null;

        /// <summary>
        /// Cursor movement changes event
        /// </summary>
        public event EventHandler CaretVisualAreaChanged;

        /// <summary>
        /// Edit border point style settings
        /// </summary>
        /// <param name="ignoreTextPoints"></param>
        /// <param name="ignoreImagePoints"></param>
        /// <param name="drawEditPointType"></param>
        public void SetEditIgnorePints(List<PointControlType> ignoreTextPoints, List<PointControlType> ignoreImagePoints, DrawPointType drawEditPointType)
        {
            this.ignoreTextPoints = ignoreTextPoints;
            this.ignoreImagePoints = ignoreImagePoints;
            this.drawEditPointType = drawEditPointType;
        }

        /// <summary>
        /// Edit preliminary display of border style status
        /// </summary>
        /// <param name="editPen"></param>
        public void SetEditPen(Pen editPen = null, Pen editHoverPen = null)
        {
            this.editPen = editPen;
            this.editHoverPen = editHoverPen;
        }

        public Pen GetEditPen()
        {
            return editPen;
        }

        /// <summary>
        /// Restore Crop Box
        /// </summary>
        /// <param name="rect"></param>
        public void SetClipThickness(Thickness rect = new Thickness())
        {
            this.ClipThickness = rect;
        }

        /// <summary>
        /// Restore Crop Box
        /// </summary>
        /// <param name="rect"></param>
        public Thickness GetClipThickness()
        {
            return this.ClipThickness;
        }

        private void InsertTextEditView()
        {
            int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();

            PDFViewer.InsertView(selectedRectViewIndex, customizeLayer);
            textEditTag = customizeLayer.GetResTag();

            AddHandler(KeyDownEvent, new RoutedEventHandler(KeyInputEventHandler));
        }

        /// <summary>
        /// Set selected
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="editAreaIndex"></param>
        public void SelectedEditAreaForIndex(int pageIndex, int editAreaIndex, bool drawCaret = true)
        {
            selectAllCharsForLine = false;
            selectedEditPageIndex = pageIndex;
            selectedEditAreaIndex = editAreaIndex;
            this.drawCaret = drawCaret;
        }

        /// <summary>
        /// Get the current set edit index
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="editAreaIndex"></param>
        public void GetSelectedEditAreaForIndex(out int pageIndex, out int editAreaIndex)
        {
            pageIndex = selectedEditPageIndex;
            editAreaIndex = selectedEditAreaIndex;
        }

        /// <summary>
        /// Refresh the cursor and selected drawing effect
        /// </summary>
        public void DrawSelectedEditAreaForIndex()
        {
            if (lastSelectedRect != null)
            {
                lastSelectedRect.SetIsHover(false);
                lastSelectedRect.SetIsSelected(false);
                lastSelectedRect.SetIgnorePointsAll();
                lastSelectedRect.Draw();
            }
            foreach (KeyValuePair<SelectedRect, EditAreaObject> item in editArea)
            {
                if (selectedEditPageIndex == item.Value.PageIndex &&
                    selectedEditAreaIndex == item.Value.EditAreaIndex)
                {
                    if (currentEditAreaObject != null)
                    {
                        PointControlType controlType = currentEditAreaObject.ControlType;
                        currentEditAreaObject = item.Value;
                        currentEditAreaObject.ControlType = controlType;
                    }
                    else
                    {
                        currentEditAreaObject = item.Value;
                        currentEditAreaObject.ControlType = PointControlType.Body;
                    }

                    CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
                    if (selectAllCharsForLine)
                    {
                        caretVisual.SetZoom(0);
                    }
                    else
                    {
                        caretVisual.SetZoom(currentZoom);
                    }

                    caretVisual.SetPaintOffset(item.Key.GetMaxRect());
                    if (currentEditAreaObject.cPDFEditArea.Type == CPDFEditType.EditText && !selectAllCharsForLine)
                    {
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea, drawCaret);
                    }
                    lastSelectedRect = item.Key;

                    item.Key.SetIsSelected(true);
                    List<PointControlType> ignorePoints = new List<PointControlType>();
                    //Set Box Selection Style
                    if (selectedEditAreaIndex == cropIndex)
                    {
                        item.Key.SetCurrentDrawPointType(DrawPointType.Crop);
                        if (!ClipThickness.Equals(new Thickness(0, 0, 0, 0)))
                        {
                            //Restore Crop Box
                            item.Key.SetClipThickness(ClipThickness);
                        }
                        ignorePoints.Add(PointControlType.Body);
                    }
                    else
                    {
                        lastSelectedRect.DataChanged -= LastSelectedRect_DataChanged;
                        lastSelectedRect.DataChanged += LastSelectedRect_DataChanged;
                    }
                    if (item.Value.cPDFEditArea is CPDFEditTextArea)
                    {
                        item.Key.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType);
                    }
                    else
                    {
                        if (selectedEditAreaIndex == cropIndex)
                        {
                            item.Key.SetIgnorePoints(ignorePoints);
                        }
                        else
                        {
                            item.Key.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType, false);
                        }
                    }

                    item.Key.Draw();
                    break;
                }
            }
        }

        /// <summary>
        /// Get the selected box of an annotation according to the index
        /// </summary>
        public SelectedRect GetEditAreaForIndex(int pageIndex, int editAreaIndex)
        {
            foreach (KeyValuePair<SelectedRect, EditAreaObject> item in editArea)
            {
                if (pageIndex == item.Value.PageIndex &&
                    editAreaIndex == item.Value.EditAreaIndex)
                {
                    return item.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Index Get List Value
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="editAreaIndex"></param>
        /// <returns></returns>
        public EditAreaObject GetSelectedForIndex(int pageIndex, int editAreaIndex)
        {
            foreach (KeyValuePair<SelectedRect, EditAreaObject> item in editArea)
            {
                if (pageIndex == item.Value.PageIndex &&
                    editAreaIndex == item.Value.EditAreaIndex)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public void SetCurrentEditType(CPDFEditType editType)
        {
            contentEditType = editType;
        }

        /// <summary>
        /// Refresh the current edit data
        /// </summary>
        /// <param name="currentRenderFrame"></param>
        public void SetEditTextRect(RenderFrame currentRenderFrame)
        {
            lastSelectedRect = null;
            editArea.Clear();
            hitTestRects.Clear();
            image.Clear();
            text.Clear();
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);

            customizeLayer.Children.Clear();
            operateChildrenIndex = -1;
            CaretVisual caretVisual = new CaretVisual(GetDefaultDrawParam());
            customizeLayer.Children.Add(caretVisual);

            currentZoom = currentRenderFrame.ZoomFactor;

            foreach (RenderData item in currentRenderFrame.GetRenderDatas())
            {
                if (item.CPDFEditPageObj == null)
                {
                    continue;
                }
                foreach (CPDFEditArea editArea in item.CPDFEditPageObj.GetEditAreaList())
                {
                    SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
                    selectedRect.SetEditPen(editPen, editHoverPen);
                    selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                    customizeLayer.Children.Add(selectedRect);

                    Rect TextBlock = DataConversionForWPF.CRectConversionForRect(editArea.GetFrame());
                    if (editArea.Type == CPDFEditType.EditImage)
                    {
                        if ((contentEditType & CPDFEditType.EditImage) != CPDFEditType.EditImage)
                        {
                            continue;
                        }
                        TextBlock = DataConversionForWPF.CRectConversionForRect((editArea as CPDFEditImageArea).GetClipRect());
                    }
                    else if (editArea.Type == CPDFEditType.EditText)
                    {
                        if ((contentEditType & CPDFEditType.EditText) != CPDFEditType.EditText)
                        {
                            continue;
                        }
                    }
                    Rect rect = TextBlock;

                    rect.X = (rect.X) * currentRenderFrame.ZoomFactor;
                    rect.Y = (rect.Y) * currentRenderFrame.ZoomFactor;
                    rect.Width *= currentRenderFrame.ZoomFactor;
                    rect.Height *= currentRenderFrame.ZoomFactor;
                    rect = DpiHelper.PDFRectToStandardRect(rect);

                    rect.X += item.PageBound.X;
                    rect.Y += item.PageBound.Y;

                    //PDF对象设置界面矩形
                    selectedRect.SetRectPadding(5);
                    selectedRect.SetRect(rect, currentZoom);
                    selectedRect.SetMaxRect(item.PageBound);
                    selectedRect.SetIgnorePointsAll();
                    selectedRect.Draw();
                    EditAreaObject editAreaObject = new EditAreaObject();
                    editAreaObject.ControlType = PointControlType.None;
                    editAreaObject.cPDFEditArea = editArea;
                    editAreaObject.cPDFEditPage = item.CPDFEditPageObj;
                    editAreaObject.PageIndex = item.PageIndex;
                    editAreaObject.EditAreaIndex = item.CPDFEditPageObj.GetEditAreaList().IndexOf(editArea);
                    editAreaObject.PageBound = item.PageBound;
                    editAreaObject.PaintRect = item.PaintRect;

                    this.editArea.Add(selectedRect, editAreaObject);
                    switch (editArea.Type)
                    {
                        case CPDFEditType.None:
                            break;
                        case CPDFEditType.EditText:
                            text.Add(selectedRect);
                            break;
                        case CPDFEditType.EditImage:
                            image.Add(selectedRect);
                            break;
                        default:
                            break;
                    }
                }
            }
            hitTestRects.AddRange(text);
            hitTestRects.AddRange(image);
        }

        public void DrawStartTextEdit(SelectedRect selectedRect, EditAreaObject editAreaObject)
        {
            Point point = Mouse.GetPosition(this);
            if (!GetIsCropMode())
            {
                if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                {
                    //Crop judgment point position
                    selectedRect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType);
                }
                else
                {
                    selectedRect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType, false);
                }

            }
            if (selectedRect != null)
            {
                selectedRect.Draw();
                selectedRect.OnMouseLeftButtonDown(point);
                isDrawSelectRect = true;
            }
        }

        public void DrawMoveTextEdit(SelectedRect selectedRect, bool selectText)
        {
            Point point = Mouse.GetPosition(this);
            if (selectedRect != null && lastSelectedRect != null)
            {
                //CleanDraw();
                if (selectText)
                {
                    editArea.TryGetValue(lastSelectedRect, out EditAreaObject editAreaObject);
                    if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                    {
                        Rect checkRect = selectedRect.GetDrawRect();
                        if (point.X > checkRect.Right)
                        {
                            point.X = checkRect.Right;
                        }
                        if (point.X < checkRect.Left)
                        {
                            point.X = checkRect.Left;
                        }
                        if (point.Y > checkRect.Bottom)
                        {
                            point.Y = checkRect.Bottom;
                        }
                        if (point.Y < checkRect.Top)
                        {
                            point.Y = checkRect.Top;
                        }
                        Point startPoint = new Point(pressPoint.X - editAreaObject.PageBound.X, pressPoint.Y - editAreaObject.PageBound.Y);
                        Point endPoint = new Point(point.X - editAreaObject.PageBound.X, point.Y - editAreaObject.PageBound.Y);
                        SelectText(editAreaObject.cPDFEditArea as CPDFEditTextArea, startPoint, endPoint);
                        CleanDraw();
                    }
                }
                else
                {
                    selectedRect.SetOutSideScaling(isOutSideScaling);
                    selectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
                    if (selectedEditAreaIndex == cropIndex)
                    {
                        //Refresh ClipRect
                        ClipThickness = selectedRect.GetClipThickness();
                    }
                    else
                    {
                        //reduction
                        ClipThickness = new Thickness(0, 0, 0, 0);
                    }

                    selectedRect.Draw();
                }
            }
        }

        public void DrawMoveMultiTextEdit(MultiSelectedRect selectedRect)
        {
            //Point point = Mouse.GetPosition(this);
            //if (selectedRect != null)
            //{
            //        selectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
            //        selectedRect.Draw();
            //}
        }
        /// <summary>
        /// Mouse click to select text
        /// </summary>
        /// <param name="clickPoint">
        /// Mouse click position
        /// </param>
        /// <param name="isWord">
        /// If true, select the word; if false, select the line
        /// </param>
        public void HandleTextSelectClick(SelectedRect selectedRect, bool isWord)
        {
            Point point = Mouse.GetPosition(this);
            if (selectedRect != null && lastSelectedRect != null)
            {
                editArea.TryGetValue(lastSelectedRect, out EditAreaObject editAreaObject);
                if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                {
                    Rect checkRect = selectedRect.GetDrawRect();
                    if (point.X > checkRect.Right)
                    {
                        point.X = checkRect.Right;
                    }
                    if (point.X < checkRect.Left)
                    {
                        point.X = checkRect.Left;
                    }
                    if (point.Y > checkRect.Bottom)
                    {
                        point.Y = checkRect.Bottom;
                    }
                    if (point.Y < checkRect.Top)
                    {
                        point.Y = checkRect.Top;
                    }
                    Point clickPoint = new Point(point.X - editAreaObject.PageBound.X, point.Y - editAreaObject.PageBound.Y);
                    Point zoomPoint = new Point(clickPoint.X / currentZoom, clickPoint.Y / currentZoom);
                    Point rawPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                    CPDFEditTextArea textArea = editAreaObject.cPDFEditArea as CPDFEditTextArea;

                    List<Rect> selectLineRects = new List<Rect>();
                    if (isWord)
                    {
                        Rect rawRect = DataConversionForWPF.CRectConversionForRect(
                            textArea.GetRectForWordAtPos(DataConversionForWPF.PointConversionForCPoint(rawPoint))
                            );
                        textArea.GetLastSelectChars();
                        selectLineRects.Add(rawRect);
                    }
                    else
                    {
                        textArea.GetSectionAtPos(DataConversionForWPF.PointConversionForCPoint(rawPoint));

                        foreach (CRect item in textArea.SelectLineRects)
                        {
                            selectLineRects.Add(DataConversionForWPF.CRectConversionForRect(item));
                        }
                    }
                    CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
                    caretVisual.SetSelectRect(selectLineRects);
                    caretVisual.Draw(true, false);
                    caretVisual.StopCaret();
                }
            }
        }

        public void DrawEndTextEdit(SelectedRect selectedRect)
        {
            Point point = Mouse.GetPosition(this);

            if (selectedRect != null)
            {
                pressPoint = new Point();
                selectedRect.OnMouseLeftButtonUp(point);
                selectedRect.Draw();
                EditAreaObject editAreaObject = GetEditAreaObjectForRect(selectedRect);
                if (editAreaObject != null)
                {
                    if (editAreaObject.cPDFEditArea.Type == CPDFEditType.EditText)
                    {
                        DrawCaretVisualArea(editAreaObject.cPDFEditArea as CPDFEditTextArea);
                    }
                }
            }
        }

        /// <summary>
        /// Get the text editing object of the specified coordinates
        /// </summary>
        /// <returns>
        /// Return the text editing object of the specified coordinates
        /// </returns>
        public EditAreaObject GetHitTestAreaObject(Point point)
        {
            SelectedRect selectedRect = null;
            foreach (var item in hitTestRects.ToArray())
            {
                int hitIndex = (int)item.GetHitControlIndex(point);
                if (hitIndex != -1)
                {
                    selectedRect = item;
                    break;
                }
            }
            if (selectedRect == null)
            {
                return null;
            }
            if (editArea.TryGetValue(selectedRect, out EditAreaObject editAreaObject))
            {
                return editAreaObject;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Update the background content of a region
        /// </summary>
        /// <param name="renderData">
        /// The key is the page number, and the value is the area to be updated
        /// </param>
        /// <param name="PaintRect">
        /// The area to be updated
        /// </param>
        private void DrawUpdateText(Dictionary<int, Rect> renderData, Rect PaintRect)
        {
            int Tag = PDFViewer.GetBackgroundTag();
            BaseLayer baseLayer = PDFViewer.GetViewForTag(Tag);
            if (baseLayer != null)
            {
                (baseLayer as BackgroundLayer).ReDrawBackgroundForBuffer(ActualHeight, ActualWidth, PDFViewer.GetDocument(), renderData, PaintRect, currentZoom, PDFViewer.GetDrawModes(), PDFViewer.GetPDFBackground(), true, true);
            }
        }

        public EditAreaObject GetEditAreaObjectForRect(SelectedRect selectedRect)
        {
            if (selectedRect == null)
            {
                return null;
            }
            EditAreaObject editAreaObject;
            editArea.TryGetValue(selectedRect, out editAreaObject);
            return editAreaObject;
        }

        public EditAreaObject GetEditAreaObjectListForRect(SelectedRect selectedRect)
        {
            if (selectedRect == null)
            {
                return null;
            }
            EditAreaObject editAreaObject;
            editAreaList.TryGetValue(selectedRect, out editAreaObject);
            return editAreaObject;
        }

        public EditAreaObject GetEditAreaObjectListForIndex(int pageIndex, int editIndex)
        {
            if (editAreaList == null || editAreaList.Count == 0)
            {
                return null;
            }

            foreach (EditAreaObject editArea in editAreaList.Values)
            {
                if (editArea.PageIndex == pageIndex && editArea.EditAreaIndex == editIndex)
                {
                    return editArea;
                }
            }

            return null;
        }

        public SelectedRect GetSelectedRectForEditAreaObject(CPDFEditArea editAreaObject)
        {
            if (editAreaObject == null)
            {
                return null;
            }
            foreach (var item in editArea)
            {
                if (item.Value.cPDFEditArea == editAreaObject)
                {
                    return item.Key;
                }
            }
            return null;
        }

        public SelectedRect GetLastSelectedRect()
        {
            return lastSelectedRect;
        }

        public MultiSelectedRect GetMultiSelectedRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            return multiSelectedRect;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <returns></returns>
        public void DrawTextEditDownEvent(bool drawCaret = true)
        {
            bool tempSelectAllCharsForLine = selectAllCharsForLine;
            TextCompositionManager.RemoveTextInputHandler(this, TextInputEventHandler);
            TextCompositionManager.AddTextInputHandler(this, TextInputEventHandler);
            RefreshInputMethod();
            currentEditAreaObject = null;
            SetPastePoint(new Point(-1, -1));
            SelectedEditAreaForIndex(-1, -1);
            Point point = Mouse.GetPosition(this);
            CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));

            EditAreaObject editAreaObject = null;
            if (lastSelectedRect != null)
            {
                editArea.TryGetValue(lastSelectedRect, out editAreaObject);
                if (drawCaret && editAreaObject.cPDFEditArea is CPDFEditTextArea)
                {
                    (editAreaObject.cPDFEditArea as CPDFEditTextArea).ClearSelectChars();
                    caretVisual.CleanSelectRectDraw();
                }
            }

            //Prioritize the selected status
            List<SelectedRect> checkList = new List<SelectedRect>();
            if (hitTestRects != null && hitTestRects.Count > 0)
            {
                List<SelectedRect> checkedList = hitTestRects.AsEnumerable().Where(x => x.GetIsSelected() == true).ToList();
                List<SelectedRect> unCheckList = hitTestRects.AsEnumerable().Where(x => x.GetIsSelected() == false).ToList();

                checkList.AddRange(checkedList);
                checkList.AddRange(unCheckList);
            }

            foreach (SelectedRect rect in checkList)
            {
                rect.SetIsHover(false);
                rect.SetIsSelected(false);
                PointControlType pointControlType = PointControlType.None;
                if (GetIsCropMode())
                {
                    pointControlType = rect.GetHitCropControlIndex(point, false);
                }
                else
                {

                    pointControlType = rect.GetHitControlIndex(point);
                }
                editArea.TryGetValue(rect, out EditAreaObject editObject);
                if (pointControlType != PointControlType.None)
                {
                    if (lastSelectedRect != null)
                    {
                        if (!lastSelectedRect.Equals(rect))
                        {
                            caretVisual.CleanSelectRectDraw();
                            lastSelectedRect.SetIgnorePointsAll();
                            lastSelectedRect.SetIsSelected(false);
                            lastSelectedRect.Draw();
                            lastSelectedRect.DataChanged -= LastSelectedRect_DataChanged;

                            if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                            {
                                string chars = (editAreaObject.cPDFEditArea as CPDFEditTextArea).GetAllChars();
                                if (string.IsNullOrEmpty(chars))
                                {
                                    int index = editAreaObject.cPDFEditPage.GetEditAreaList().IndexOf(editAreaObject.cPDFEditArea);
                                    editAreaObject.cPDFEditPage.RemoveEditArea(index);
                                    editAreaObject.cPDFEditPage.EndEdit();
                                    lastSelectedRect.ClearDraw();

                                    // Update the index of the currently selected object
                                    if (editObject.EditAreaIndex > index)
                                    {
                                        editObject.EditAreaIndex--;
                                    }
                                    List<CPDFEditArea> editAreas = editAreaObject.cPDFEditPage.GetEditAreaList(true);
                                    if (editAreas.Count > editObject.EditAreaIndex)
                                    {
                                        editObject.cPDFEditArea = editAreas[editObject.EditAreaIndex];
                                    }
                                }
                            }

                            lastSelectedRect = rect;
                            lastSelectedRect.DataChanged += LastSelectedRect_DataChanged;
                            if (!GetIsCropMode())
                            {
                                if (editObject.cPDFEditArea is CPDFEditTextArea)
                                {
                                    rect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType);
                                }
                                else
                                {
                                    rect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType, false);
                                }

                            }

                            rect.SetIsSelected(true);
                            rect.Draw();
                        }
                        else
                        {
                            rect.SetIsSelected(true);
                        }
                    }
                    else
                    {
                        lastSelectedRect = rect;
                        lastSelectedRect.DataChanged += LastSelectedRect_DataChanged;
                        if (!GetIsCropMode())
                        {
                            if (editObject.cPDFEditArea is CPDFEditTextArea)
                            {
                                rect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType);
                            }
                            else
                            {
                                rect.SetEditIgnorePoints(ignoreTextPoints, ignoreImagePoints, drawEditPointType, false);
                            }
                        }
                        rect.SetIsSelected(true);
                        rect.Draw();
                    }

                    SelectedEditAreaForIndex(editObject.PageIndex, editObject.EditAreaIndex, drawCaret);
                    currentEditAreaObject = editObject;
                    currentEditAreaObject.ControlType = pointControlType;
                    if (pointControlType == PointControlType.Body)
                    {
                        caretVisual.SetCaretVisualArea(editObject.cPDFEditArea, currentZoom, rect.GetMaxRect(), point);
                        if (tempSelectAllCharsForLine && !drawCaret)
                        {
                            caretVisual.SetZoom(0);
                            selectAllCharsForLine = true;
                        }

                        SetPastePoint(point);
                        caretVisual.StartTimer();
                        caretVisual.Draw(true, drawCaret);
                        if (!drawCaret)
                        {
                            caretVisual.StopCaret();
                        }

                        Point clickPoint = new Point((point.X - editObject.PageBound.X) / currentZoom, (point.Y - editObject.PageBound.Y) / currentZoom);
                        Point rawPoint = DpiHelper.StandardPointToPDFPoint(clickPoint);
                        rawHitPos = new CPoint((float)rawPoint.X, (float)rawPoint.Y);
                    }
                    else
                    {
                        caretVisual.StopTimer();
                        caretVisual.CleanDraw();
                        if (pointControlType != PointControlType.None)
                        {
                            if (editObject.cPDFEditArea is CPDFEditTextArea)
                            {
                                (editObject.cPDFEditArea as CPDFEditTextArea).SelectAllChars();
                                selectAllCharsForLine = true;
                            }
                        }
                    }

                    pressPoint = point;
                    return;
                }
            }

            // No edit box clicked
            if (lastSelectedRect != null)
            {
                if (lastSelectedRect.GetCurrentDrawPointType() == DrawPointType.Crop)
                {
                    // If the clicked point is not in the range of the cropped image
                    if (!lastSelectedRect.GetDrawRect().Contains(point))
                    {
                        lastSelectedRect.SetCurrentDrawPointType(DrawPointType.Square);
                        cropIndex = -1;
                        lastSelectedRect.SetIgnorePointsAll();
                        lastSelectedRect.SetIsSelected(false);
                        lastSelectedRect.Draw();
                    }
                }
                else
                {
                    lastSelectedRect.SetIgnorePointsAll();
                    lastSelectedRect.SetIsSelected(false);
                    lastSelectedRect.Draw();
                }


                if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                {
                    string chars = (editAreaObject.cPDFEditArea as CPDFEditTextArea).GetAllChars();
                    if (string.IsNullOrEmpty(chars))
                    {
                        int index = editAreaObject.cPDFEditPage.GetEditAreaList().IndexOf(editAreaObject.cPDFEditArea);
                        editAreaObject.cPDFEditPage.RemoveEditArea(index);
                        editAreaObject.cPDFEditPage.EndEdit();
                        lastSelectedRect.ClearDraw();
                    }
                }
                lastSelectedRect.DataChanged -= LastSelectedRect_DataChanged;
                if (lastSelectedRect.GetCurrentDrawPointType() != DrawPointType.Crop)
                {
                    lastSelectedRect = null;
                }
            }
            caretVisual?.StopTimer();
            caretVisual?.CleanDraw();

            return;
        }

        /// <summary>
        /// delete multi selectRect
        /// </summary>
        /// <returns></returns>
        public bool DelMultiSelectRect()
        {
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (multiSelectedRect != null && multiSelectedRect.Children.Count > 1)
            {
                Point point = Mouse.GetPosition(this);
                SelectedRect selectedRect = multiSelectedRect.GetHitControlRect(point);
                if (selectedRect != null)
                {
                    selectedRect = (SelectedRect)multiSelectedRect.Children[multiSelectedRect.GetMulitSelectedRectIndex(selectedRect)];
                    if (selectedRect != null)
                    {
                        EditAreaObject editAreaObject = GetEditAreaObjectListForRect(selectedRect);
                        if (editAreaObject != null)
                        {

                            multiSelectedRect.Children.Remove(selectedRect);
                            multiSelectedRect.DelMulitSelectedRect(selectedRect);
                            editAreaMultiIndex.Remove(editAreaObject.EditAreaIndex);
                            editAreaList.Remove(selectedRect);
                            multiSelectedRect.Draw();
                            PDFViewer.UpdateRenderFrame();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void RightDownEvent()
        {

        }

        private void LastSelectedRect_DataChanged(object sender, SelectedAnnotData e)
        {
            SelectedRect selectedRect = sender as SelectedRect;
            Rect rect = selectedRect.GetRect();
            Rect MaxRect = selectedRect.GetMaxRect();
            if (editArea.TryGetValue(selectedRect, out EditAreaObject editAreaObject))
            {
                Rect OldRect = DataConversionForWPF.CRectConversionForRect(editAreaObject.cPDFEditArea.GetFrame());

                // Get the moved area and set it to the data layer
                Rect PDFRect = new Rect(rect.X - MaxRect.X, rect.Y - MaxRect.Y, rect.Width, rect.Height);
                PDFRect = DpiHelper.StandardRectToPDFRect(PDFRect);

                PDFRect.X = PDFRect.X / currentZoom;
                PDFRect.Y = PDFRect.Y / currentZoom;
                PDFRect.Width = PDFRect.Width / currentZoom;
                PDFRect.Height = PDFRect.Height / currentZoom;

                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                PDFEditHistory editHistory = new PDFEditHistory();
                editHistory.EditPage = editAreaObject.cPDFEditPage;
                editHistory.PageIndex = editAreaObject.PageIndex;
                PDFViewer.UndoManager.AddHistory(editHistory);
                EndEdit();

                // Update the display screen
                Dictionary<int, Rect> keyValuePairs = new Dictionary<int, Rect>();
                Rect NewRect = DataConversionForWPF.CRectConversionForRect(editAreaObject.cPDFEditArea.GetFrame());
                // Calculate the rectangle
                OldRect.Union(NewRect);
                OldRect.X = OldRect.X - editPadding;
                OldRect.Y = OldRect.Y - editPadding;
                OldRect.Width = OldRect.Width + editPadding * 2;
                OldRect.Height = OldRect.Height + editPadding * 2;

                // Calculate the area of the text block that does not need to be displayed completely
                Rect paintRect = currentEditAreaObject.PaintRect;
                Rect zoomPDFPaintRect = new Rect((paintRect.X - currentEditAreaObject.PageBound.X) / currentZoom, (paintRect.Y - currentEditAreaObject.PageBound.Y) / currentZoom, paintRect.Width / currentZoom, paintRect.Height / currentZoom);
                paintRect = DpiHelper.StandardRectToPDFRect(zoomPDFPaintRect);
                OldRect.Intersect(paintRect);

                keyValuePairs.Add(currentEditAreaObject.PageIndex, OldRect);
                DrawUpdateText(keyValuePairs, currentEditAreaObject.PageBound);

                UpdateSelectRect(editAreaObject.cPDFEditArea);

                // Move the moved selected box to the top layer of the selected logic
                switch (editAreaObject.cPDFEditArea.Type)
                {
                    case CPDFEditType.EditText:
                        text.Remove(selectedRect);
                        text.Insert(0, selectedRect);
                        break;
                    case CPDFEditType.EditImage:
                        image.Remove(selectedRect);
                        image.Insert(0, selectedRect);
                        break;
                }
                hitTestRects.Clear();
                hitTestRects.AddRange(text);
                hitTestRects.AddRange(image);
            }
        }

        /// <summary>
        /// Reset the window binding and other states of the input method
        /// </summary>
        private void RefreshInputMethod()
        {
            if (InputMethod.GetIsInputMethodEnabled(this))
            {
                InputMethod.SetIsInputMethodEnabled(this, false);
            }

            if (InputMethod.GetIsInputMethodSuspended(this))
            {
                InputMethod.SetIsInputMethodSuspended(this, false);
            }

            InputMethod.SetIsInputMethodEnabled(this, true);
            InputMethod.SetIsInputMethodSuspended(this, true);
        }

        /// <summary>
        /// Use to handle keyboard event listening for up, down, left, and right cursor switching, and input method positioning
        /// </summary>
        private void KeyInputEventHandler(object sender, RoutedEventArgs e)
        {
            KeyEventArgs ke = e as KeyEventArgs;
            if (currentModel != ToolType.ContentEdit || Keyboard.Modifiers != ModifierKeys.None)
            {
                return;
            }
            switch (ke.Key)
            {
                case Key.Left:
                    if (CanMoveCaret())
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetPrevCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                        CaretVisualAreaChanged?.Invoke(null, null);
                        ke.Handled = true;
                    }
                    break;
                case Key.Right:
                    if (CanMoveCaret())
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetNextCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                        CaretVisualAreaChanged?.Invoke(null, null);
                        ke.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (CanMoveCaret())
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetUpCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                        CaretVisualAreaChanged?.Invoke(null, null);
                        ke.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (CanMoveCaret())
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetDownCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                        CaretVisualAreaChanged?.Invoke(null, null);
                        ke.Handled = true;
                    }
                    break;
                default:
                    {
                        if (lastSelectedRect == null)
                        {
                            break;
                        }
                        IntPtr imeWnd = ImportWin32.ImmGetDefaultIMEWnd(IntPtr.Zero);
                        IntPtr imeHimc = IntPtr.Zero;
                        if (imeWnd != IntPtr.Zero)
                        {
                            imeHimc = ImportWin32.ImmGetContext(imeWnd);
                        }

                        if (imeHimc != IntPtr.Zero && currentEditAreaObject != null)
                        {
                            ImportWin32.CompositionForm showPos = new ImportWin32.CompositionForm();
                            Window parentWnd = Window.GetWindow(this);
                            if (parentWnd != null)
                            {
                                CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
                                Point HeightPoint = caretVisual.GetCaretHighPoint();
                                Point caretPos = new Point(
                                    HeightPoint.X * currentZoom + lastSelectedRect.GetMaxRect().X,
                                   HeightPoint.Y * currentZoom + lastSelectedRect.GetMaxRect().Y);

                                Point screenPoint = TranslatePoint(caretPos, parentWnd);
                                screenPoint = DpiHelper.StandardPointToCurrentPoint(screenPoint);
                                showPos.dwStyle = 0x0002;
                                showPos.ptCurrentPos.x = (int)screenPoint.X;
                                showPos.ptCurrentPos.y = (int)screenPoint.Y;
                                ImportWin32.ImmSetCompositionWindow(imeHimc, ref showPos);
                            }
                        }

                        if (imeHimc != IntPtr.Zero)
                        {
                            ImportWin32.ImmReleaseContext(imeWnd, imeHimc);
                        }
                    }
                    break;
            }
        }

        private bool CanMoveCaret()
        {
            if (currentEditAreaObject == null || currentEditAreaObject.cPDFEditPage == null)
            {
                return false;
            }

            if (currentEditAreaObject.ControlType != PointControlType.Body || currentEditAreaObject.cPDFEditArea == null)
            {
                return false;
            }

            if (currentEditAreaObject.cPDFEditArea.Type != CPDFEditType.EditText)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Text input event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextInputEventHandler(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;

            if (currentEditAreaObject != null)
            {
                SetText(e.Text);
            }
        }

        /// <summary>
        /// Write the current edit data to the memory PDF
        /// </summary>
        private void EndEdit()
        {
            if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
            {
                currentEditAreaObject.cPDFEditPage.EndEdit();
            }
        }

        private void DeleteChars()
        {
            if (currentEditAreaObject != null)
            {
                CPDFEditTextArea textArea = currentEditAreaObject.cPDFEditArea as CPDFEditTextArea;
                if (textArea.SelectLineRects.Count > 0)
                {
                    textArea.DeleteChars();
                }
                else
                {
                    textArea.DeleteChar();
                }

                if (textArea.SelectLineRects.Count > 0)
                {
                    textArea.ClearSelectChars();
                }

                EndEdit();
                DrawCaretVisualArea(textArea);
            }
        }

        /// <summary>
        /// If it is currently empty, set the TextObject to be deleted
        /// </summary>
        /// <param name="areaObject"></param>
        public void RemoveTextBlock(EditAreaObject areaObject = null)
        {
            if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
            {
                foreach (var Rect in hitTestRects)
                {
                    editArea.TryGetValue(Rect, out EditAreaObject editAreaObject);
                    if (editAreaObject != currentEditAreaObject)
                    {
                        continue;
                    }
                    if (editAreaObject.cPDFEditArea is CPDFEditTextArea)
                    {
                        CPDFEditPage editPage = editAreaObject.cPDFEditPage;
                        CPDFEditTextArea textArea = editAreaObject.cPDFEditArea as CPDFEditTextArea;
                        int index = editAreaObject.cPDFEditPage.GetEditAreaList().IndexOf(textArea);
                        editPage.RemoveEditArea(index);
                        editPage.EndEdit();
                        Rect.ClearDraw();
                    }
                }
                SelectedEditAreaForIndex(-1, -1);
            }
            else
            {
                if (areaObject != null && areaObject.cPDFEditPage != null)
                {
                    if (areaObject.cPDFEditArea is CPDFEditTextArea)
                    {
                        CPDFEditPage editPage = areaObject.cPDFEditPage;
                        CPDFEditTextArea textArea = areaObject.cPDFEditArea as CPDFEditTextArea;
                        int index = areaObject.EditAreaIndex;
                        editPage.RemoveEditArea(index);
                        editPage.EndEdit();

                    }
                }
                SelectedEditAreaForIndex(-1, -1);
            }
        }

        /// <summary>
        /// If it is currently empty, set the imageObject to be deleted
        /// </summary>
        /// <param name="areaObject"></param>
        public void RemoveImageBlock(EditAreaObject areaObject = null)
        {
            if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
            {
                foreach (var Rect in hitTestRects)
                {
                    editArea.TryGetValue(Rect, out EditAreaObject editAreaObject);
                    if (editAreaObject != currentEditAreaObject)
                    {
                        continue;
                    }
                    if (editAreaObject.cPDFEditArea is CPDFEditImageArea)
                    {
                        CPDFEditPage editPage = editAreaObject.cPDFEditPage;
                        CPDFEditImageArea textArea = editAreaObject.cPDFEditArea as CPDFEditImageArea;
                        int index = editAreaObject.cPDFEditPage.GetEditAreaList().IndexOf(textArea);
                        editPage.RemoveEditArea(index);
                        editPage.EndEdit();
                        Rect.ClearDraw();
                    }
                }
                SelectedEditAreaForIndex(-1, -1);
            }
            else
            {
                if (areaObject != null && areaObject.cPDFEditPage != null)
                {
                    if (areaObject.cPDFEditArea is CPDFEditImageArea)
                    {
                        CPDFEditPage editPage = areaObject.cPDFEditPage;
                        CPDFEditImageArea textArea = areaObject.cPDFEditArea as CPDFEditImageArea;
                        int index = areaObject.EditAreaIndex;
                        editPage.RemoveEditArea(index);
                        editPage.EndEdit();
                    }
                }
                SelectedEditAreaForIndex(-1, -1);
            }
        }

        public void CleanEditView()
        {
            lastSelectedRect = null;
            editArea.Clear();
            hitTestRects.Clear();
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);

            customizeLayer?.Children.Clear();
            operateChildrenIndex = -1;
        }

        public void SetText(string content)
        {
            Interlocked.Increment(ref delayCount);
            if (delayTextBuilder == null || delayTextBuilder.Length == 0)
            {
                delayTextBuilder = new StringBuilder();
                delayTextBuilder.Append(content);
                Task.Factory.StartNew(DelayThread);
            }
            else
            {
                delayTextBuilder.Append(content);
            }
            return;
        }

        /// <summary>
        /// Because the data given by the input method is given one by one, so get all the data in the thread and then pass it to the underlying library    
        /// </summary>
        private void DelayThread()
        {
            while (delayCount > 0)
            {
                if (delayCount == 1)
                {
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(1);
                }
                Interlocked.Decrement(ref delayCount);
            }

            if (delayTextBuilder != null && delayTextBuilder.Length > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    DelaySetText(delayTextBuilder.ToString());
                });
                delayTextBuilder.Clear();
            }
        }

        /// <summary>
        /// Input text
        /// </summary>
        /// <param name="content"></param>
        private void DelaySetText(string content)
        {
            if (content == string.Empty || currentEditAreaObject == null || currentEditAreaObject.cPDFEditPage == null || content == "\u001b")
            {
                return;
            }
            PDFEditHistory editHistory = new PDFEditHistory();
            editHistory.EditPage = currentEditAreaObject.cPDFEditPage;
            editHistory.PageIndex = currentEditAreaObject.PageIndex;
            CPDFEditTextArea textArea = currentEditAreaObject.cPDFEditArea as CPDFEditTextArea;
            Rect OldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
            GroupHistory groupHistory = new GroupHistory();
            if (content == "\b")
            {
                if (textArea.SelectLineRects.Count > 0)
                {
                    textArea.DeleteChars();
                    groupHistory.Histories.Add(editHistory);
                }
                else
                {
                    textArea.BackSpaceChar();
                    groupHistory.Histories.Add(editHistory);
                    EndEdit();
                }
            }
            else
            {
                if (textArea.SelectLineRects.Count > 0)
                {
                    textArea.DeleteChars();
                    groupHistory.Histories.Add(editHistory);
                }
                PDFEditHistory InsertHistory = new PDFEditHistory();
                InsertHistory.EditPage = currentEditAreaObject.cPDFEditPage;
                InsertHistory.PageIndex = currentEditAreaObject.PageIndex;
                textArea.InsertText(content);
                groupHistory.Histories.Add(InsertHistory);
                EndEdit();
            }
            if (textArea.SelectLineRects.Count > 0)
            {
                textArea.ClearSelectChars();
            }
            UpdateRender(OldRect, textArea);
            if (PDFViewer != null && PDFViewer.UndoManager != null)
            {
                PDFViewer.UndoManager.AddHistory(groupHistory);
            }
            DrawCaretVisualArea(textArea);
        }

        public void UpdateRender(Rect oldRect, CPDFEditArea newTextArea)
        {
            if (currentEditAreaObject == null)
            {
                return;
            }

            if (newTextArea == null)
            {
                return;
            }

            Dictionary<int, Rect> keyValuePairs = new Dictionary<int, Rect>();
            Rect NewRect = DataConversionForWPF.CRectConversionForRect(newTextArea.GetFrame());

            // Calculate the rectangle
            oldRect.Union(NewRect);
            oldRect.X = oldRect.X - editPadding;
            oldRect.Y = oldRect.Y - editPadding;
            oldRect.Width = oldRect.Width + editPadding * 2;
            oldRect.Height = oldRect.Height + editPadding * 2;

            var e = editArea.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.cPDFEditArea.Equals(newTextArea))
                {
                    currentEditAreaObject = e.Current.Value;
                }
            }

            // Calculate the area of the text block that does not need to be displayed completely
            Rect paintRect = currentEditAreaObject.PaintRect;
            Rect zoomPDFPaintRect = new Rect((paintRect.X - currentEditAreaObject.PageBound.X) / currentZoom, (paintRect.Y - currentEditAreaObject.PageBound.Y) / currentZoom, paintRect.Width / currentZoom, paintRect.Height / currentZoom);
            paintRect = DpiHelper.StandardRectToPDFRect(zoomPDFPaintRect);
            oldRect.Intersect(paintRect);

            if (oldRect == Rect.Empty)
            {
                return;
            }

            keyValuePairs.Add(currentEditAreaObject.PageIndex, oldRect);
            DrawUpdateText(keyValuePairs, currentEditAreaObject.PageBound);
            UpdateSelectRect(newTextArea);
            if (newTextArea.Type == CPDFEditType.EditText)
            {
                DrawCaretVisualArea(newTextArea as CPDFEditTextArea);
            }
        }

        public void DrawTest(Rect maxRect, int index)
        {
            SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
            selectedRect.SetEditPen(editPen, editHoverPen);
            selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);

            customizeLayer.Children.Add(selectedRect);
            operateChildrenIndex = customizeLayer.Children.IndexOf(selectedRect);
            pointtest = Mouse.GetPosition(this);
            selectedRect.SetIgnorePointsAll();
            selectedRect.SetRect(new Rect(pointtest.X, pointtest.Y, 0, 0), currentZoom);
            selectedRect.SetMaxRect(maxRect);
            selectedRect.SetShowCreatTextRect(true);
            selectedRect.Draw();
            pageindex = index;
        }

        public Cursor DrawMoveTest(SelectedRect selectedRect1)
        {
            Cursor cursor = Cursors.Arrow;
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);
            if (operateChildrenIndex > 0 && operateChildrenIndex < customizeLayer.Children.Count)
            {
                SelectedRect selectedRect = (customizeLayer.Children[operateChildrenIndex] as SelectedRect);
                Point point = Mouse.GetPosition(this);
                Rect maxRect = selectedRect.GetMaxRect();
                Rect MoveRect = new Rect(pointtest, point);

                double cLeft = MoveRect.Left;
                double cRight = MoveRect.Right;
                double cUp = MoveRect.Top;
                double cDown = MoveRect.Bottom;
                if (cLeft < maxRect.Left)
                {
                    cLeft = maxRect.Left;
                }
                if (cUp < maxRect.Top)
                {
                    cUp = maxRect.Top;
                }
                if (cRight > maxRect.Right)
                {
                    cRight = maxRect.Right;
                }
                if (cDown > maxRect.Bottom)
                {
                    cDown = maxRect.Bottom;
                }
                selectedRect.SetRect(new Rect(cLeft, cUp, cRight - cLeft, cDown - cUp), currentZoom);
                selectedRect.Draw();
            }
            else
            {
                Point point = Mouse.GetPosition(this);

                // No edit box clicked
                if (lastHoverRect != null)
                {
                    lastHoverRect.SetIsHover(false);
                    lastHoverRect.Draw();
                    lastHoverRect = null;
                }

                //Prioritize the selected status
                List<SelectedRect> checkList = new List<SelectedRect>();
                if (hitTestRects != null && hitTestRects.Count > 0)
                {
                    List<SelectedRect> checkedList = hitTestRects.AsEnumerable().Where(x => x.GetIsSelected() == true).ToList();
                    List<SelectedRect> unCheckList = hitTestRects.AsEnumerable().Where(x => x.GetIsSelected() == false).ToList();

                    checkList.AddRange(checkedList);
                    checkList.AddRange(unCheckList);
                }

                foreach (SelectedRect rect in checkList)
                {
                    PointControlType pointControlType = rect.GetHitControlIndex(point, false);
                    MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
                    if (GetIsCropMode())
                    {
                        PointControlType pointCropControlType = rect.GetHitCropControlIndex(point, false);
                        if (pointCropControlType != PointControlType.None)
                        {
                            cursor = GetCursors(pointCropControlType, true);
                            break;
                        }
                    }
                    else
                    {
                        //Multiple selection of mouse styles
                        if (multiSelectedRect != null && multiSelectedRect.Children.Count > 0)
                        {
                            PointControlType pointMultiControlType = multiSelectedRect.GetHitControlIndex(Mouse.GetPosition(this));
                            cursor = GetCursors(pointMultiControlType, true);
                        }
                        if (pointControlType != PointControlType.None)
                        {
                            EditAreaObject editAreaObject = GetEditAreaObjectForRect(rect);
                            if (editAreaObject.cPDFEditArea.Type == CPDFEditType.EditImage)
                            {
                                //image hover
                                if (selectedRect1 == null)
                                {
                                    lastHoverRect = rect;
                                    rect.SetIsHover(true);
                                    rect.Draw();
                                }
                                else
                                {
                                    if (!selectedRect1.Equals(rect))
                                    {
                                        lastHoverRect = rect;
                                        rect.SetIsHover(true);
                                        rect.Draw();
                                    }
                                }
                                cursor = GetCursors(pointControlType, true);
                                break;
                            }
                            else
                            {
                                cursor = GetCursors(pointControlType, false);
                            }
                            // Not selected
                            if (selectedRect1 == null)
                            {
                                if (lastHoverRect != null)
                                {
                                    if (!lastHoverRect.Equals(rect))
                                    {
                                        lastHoverRect.SetIsHover(false);
                                        lastHoverRect.Draw();
                                        lastHoverRect = rect;
                                        rect.SetIsHover(true);
                                        rect.Draw();
                                    }
                                }
                                else
                                {
                                    lastHoverRect = rect;
                                    rect.SetIsHover(true);
                                    rect.Draw();
                                }
                            }
                            else
                            {
                                // Current selected box is inconsistent with hit test object
                                if (!selectedRect1.Equals(rect))
                                {
                                    lastHoverRect = rect;
                                    rect.SetIsHover(true);
                                    rect.Draw();
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return cursor;
        }

        private Cursor GetCursors(PointControlType controlType, bool isImage)
        {
            switch (controlType)
            {
                case PointControlType.LeftTop:
                case PointControlType.RightBottom:
                    return Cursors.SizeNWSE;

                case PointControlType.LeftMiddle:
                case PointControlType.RightMiddle:
                    return Cursors.SizeWE;

                case PointControlType.LeftBottom:
                case PointControlType.RightTop:
                    return Cursors.SizeNESW;

                case PointControlType.MiddlBottom:
                case PointControlType.MiddleTop:
                    return Cursors.SizeNS;

                case PointControlType.Body:
                    if (isImage)
                    {
                        return Cursors.SizeAll;
                    }
                    else
                    {
                        return Cursors.IBeam;
                    }
                case PointControlType.Line:
                    return Cursors.SizeAll;
                default:
                    return Cursors.Arrow;
            }
        }

        public bool DrawEndTest()
        {
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);
            if (operateChildrenIndex > 0 && operateChildrenIndex < customizeLayer.Children.Count)
            {
                SelectedRect selectedRect = (customizeLayer.Children[operateChildrenIndex] as SelectedRect);

                Rect rect = selectedRect.GetRect();
                Rect MaxRect = selectedRect.GetMaxRect();
                // Get the area and set it to the data layer
                Rect PDFRect = new Rect((rect.X - MaxRect.X) / currentZoom, (rect.Y - MaxRect.Y) / currentZoom, rect.Width / currentZoom, rect.Height / currentZoom);
                PDFRect = DpiHelper.StandardRectToPDFRect(PDFRect);

                CPDFPage docPage = PDFViewer.GetDocument().PageAtIndex(pageindex);
                CPDFEditPage EditPage = docPage.GetEditPage();
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                TextEditParam textEditParam = defaultSettingParam.TextEditParamDef;
                textEditParam.EditIndex = EditPage.GetEditAreaList().Count;

                if (string.IsNullOrEmpty(textEditParam.FontName))
                {
                    textEditParam.FontName = "Helvetica";
                }

                if (textEditParam.FontSize <= 0)
                {
                    textEditParam.FontSize = 14;
                }

                if (textEditParam.FontColor == null || textEditParam.FontColor.Length < 3)
                {
                    textEditParam.FontColor = new byte[3] { 0, 0, 0 };
                }

                CPDFEditTextArea textArea = EditPage.CreateNewTextArea(
                    DataConversionForWPF.RectConversionForCRect(PDFRect),
                    textEditParam.FontName,
                    (float)textEditParam.FontSize,
                    textEditParam.FontColor,
                    textEditParam.Transparency,
                    textEditParam.IsBold,
                    textEditParam.IsItalic,
                    textEditParam.TextAlign
                    );

                GroupHistory groupHistory = new GroupHistory();
                if (textArea == null)
                {
                    textEditParam.FontName = "Helvetica";
                    textArea = EditPage.CreateNewTextArea(
                        DataConversionForWPF.RectConversionForCRect(PDFRect),
                        textEditParam.FontName,
                        (float)textEditParam.FontSize,
                        textEditParam.FontColor,
                        textEditParam.Transparency,
                        textEditParam.IsBold,
                        textEditParam.IsItalic,
                        textEditParam.TextAlign
                        );
                }
                // Adjust the size of the creation box for the empty state, so the undo/redo needs to be performed one more time below.
                textArea.InsertText("");

                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.EditPage = EditPage;
                pDFEditHistory.PageIndex = pageindex;
                groupHistory.Histories.Add(pDFEditHistory);

                PDFEditHistory pDFEditHistory1 = new PDFEditHistory();
                pDFEditHistory1.EditPage = EditPage;
                pDFEditHistory1.PageIndex = pageindex;
                groupHistory.Histories.Add(pDFEditHistory1);
                PDFViewer.UndoManager.AddHistory(groupHistory);

                defaultSettingParam.SetPDFEditParamm(textEditParam);

                EditPage.EndEdit();
                customizeLayer.Children.Remove(selectedRect);
                SelectedEditAreaForIndex(pageindex, textEditParam.EditIndex);
                PDFViewer.UpdateRenderFrame();
                return true;
            }

            return false;
        }

        public bool MoveEditArea(Point moveOffset)
        {
            EditAreaObject areaObj = currentEditAreaObject;

            if (PDFViewer == null || GetToolType() != ToolType.ContentEdit)
            {
                return false;
            }

            if (areaObj == null || areaObj.cPDFEditArea == null)
            {
                return false;
            }

            CPDFDocument pdfDoc = PDFViewer.GetDocument();
            if (pdfDoc == null)
            {
                return false;
            }
            CPDFPage pdfPage = pdfDoc.PageAtIndex(areaObj.PageIndex);
            if (pdfPage == null)
            {
                return false;
            }
            CPDFEditArea editArea = areaObj.cPDFEditArea;
            CRect cRect = editArea.GetFrame();
            Rect rect = DataConversionForWPF.CRectConversionForRect(cRect);
            Rect preRect = rect;
            double boundOffset = 5;
            rect.X = Math.Min(pdfPage.PageSize.width - rect.Width - boundOffset, rect.X + moveOffset.X);
            rect.Y = Math.Min(pdfPage.PageSize.height - rect.Height - boundOffset, rect.Y + moveOffset.Y);

            rect.X = Math.Max(rect.X, boundOffset);
            rect.Y = Math.Max(rect.Y, boundOffset);

            editArea.SetFrame(DataConversionForWPF.RectConversionForCRect(rect));
            UpdateSelectRect(editArea);
            if ((int)preRect.Left != (int)rect.Left ||
                (int)preRect.Top != (int)rect.Top ||
                (int)preRect.Width != (int)rect.Width ||
                (int)preRect.Height != (int)rect.Height)
            {
                PDFViewer.UpdateRenderFrame();
            }
            return true;
        }

        public void CropImage(CPDFEditImageArea cPDFEditImageArea)
        {
            SelectedRect selectedRect = null;
            foreach (var item in editArea)
            {
                if (item.Value.cPDFEditArea == cPDFEditImageArea)
                {
                    selectedRect = item.Key;
                }
            }
            selectedRect.SetIgnorePointsAll();
        }

        /// <summary>
        /// Update the selected box related to the specified CPDFEditArea
        /// </summary>
        /// <param name="textArea"></param>
        private void UpdateSelectRect(CPDFEditArea editArea)
        {
            SelectedRect selectedRect = null;
            foreach (var item in this.editArea)
            {
                if (item.Value.cPDFEditArea == editArea)
                {
                    selectedRect = item.Key;
                }
            }
            if (selectedRect != null)
            {
                Rect maxrect = selectedRect.GetMaxRect();
                Rect rect = DataConversionForWPF.CRectConversionForRect(editArea.GetFrame());
                if (editArea.Type == CPDFEditType.EditImage)
                {
                    if ((contentEditType & CPDFEditType.EditImage) == CPDFEditType.EditImage)
                    {
                        rect = DataConversionForWPF.CRectConversionForRect((editArea as CPDFEditImageArea).GetClipRect());
                    }
                }

                rect.X = (rect.X) * currentZoom;
                rect.Y = (rect.Y) * currentZoom;
                rect.Width *= currentZoom;
                rect.Height *= currentZoom;
                rect = DpiHelper.PDFRectToStandardRect(rect);
                rect.X += maxrect.X;
                rect.Y += maxrect.Y;
                selectedRect.SetRect(rect, currentZoom);
                selectedRect.Draw();
            }
        }

        /// <summary>
        /// Select text
        /// </summary>
        private void SelectText(CPDFEditTextArea textArea, Point startPoint, Point endPoint)
        {
            Point zoomStartPoint = new Point(startPoint.X / currentZoom, startPoint.Y / currentZoom);
            Point zoomEndPoint = new Point(endPoint.X / currentZoom, endPoint.Y / currentZoom);
            Point start_point = DpiHelper.StandardPointToPDFPoint(zoomStartPoint);
            Point end_point = DpiHelper.StandardPointToPDFPoint(zoomEndPoint);
            textArea.ClearSelectChars();
            textArea.GetSelectChars(
                DataConversionForWPF.PointConversionForCPoint(start_point),
                DataConversionForWPF.PointConversionForCPoint(end_point)
                );

            CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
            List<Rect> SelectLineRects = new List<Rect>();
            foreach (CRect item in textArea.SelectLineRects)
            {
                SelectLineRects.Add(
                    DataConversionForWPF.CRectConversionForRect(item)
                    );
            }

            caretVisual.SetSelectRect(SelectLineRects);
            caretVisual.Draw(true);
        }

        /// <summary>
        /// Draw the cursor to the specified text block position immediately
        /// </summary>
        /// <param name="textArea"></param>
        private void DrawCaretVisualArea(CPDFEditTextArea textArea, bool drawCaret = true)
        {
            if (textArea == null)
            {
                return;
            }
            CPoint cursorCPoint = new CPoint(0, 0);
            CPoint highCpoint = new CPoint(0, 0);
            textArea.GetTextCursorPoints(ref cursorCPoint, ref highCpoint);
            rawHitPos = cursorCPoint;
            CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));

            Point cursorPoint = DataConversionForWPF.CPointConversionForPoint(cursorCPoint);
            Point pointHigh = DataConversionForWPF.CPointConversionForPoint(highCpoint);
            caretVisual.SetCaretVisualArea(cursorPoint, pointHigh);

            textArea.GetLastSelectChars();
            List<Rect> SelectLineRects = new List<Rect>();
            foreach (CRect item in textArea.SelectLineRects)
            {
                SelectLineRects.Add(DataConversionForWPF.CRectConversionForRect(item));
            }
            caretVisual.SetSelectRect(SelectLineRects);
            if (SelectLineRects.Count > 0)
            {
                if (GetSelectedRectForEditAreaObject(textArea) != null)
                {
                    caretVisual.Draw(true, false);
                    Point HeightPoint = caretVisual.GetCaretHighPoint();
                    Point caretPos = new Point(
                        HeightPoint.X * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().X,
                       HeightPoint.Y * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().Y);
                    SetPastePoint(caretPos);
                }
                caretVisual.StopCaret();
            }
            else
            {
                caretVisual.Draw(true, drawCaret);
                if (drawCaret)
                {
                    if (GetSelectedRectForEditAreaObject(textArea) != null)
                    {
                        Point HeightPoint = caretVisual.GetCaretHighPoint();

                        Point caretPos = new Point(
                            HeightPoint.X * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().X,
                           HeightPoint.Y * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().Y);
                        SetPastePoint(caretPos);
                        caretVisual.StartTimer();
                    }
                    else
                    {
                        caretVisual.StopCaret();
                    }
                }
                else
                {
                    caretVisual.StopCaret();
                }
            }
        }

        /// <summary>
        /// Stop displaying the cursor, but do not erase the selected effect
        /// </summary>
        private void CleanDraw()
        {
            CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
            caretVisual.Draw(true, false);
            caretVisual.StopCaret();
        }


        /// <summary>
        /// Jump cursor to a specific position in a text area.
        /// </summary>
        /// <param name="editingLocation">Cursor position.</param>
        /// <param name="isSelectRanage"> Whether to select text from the current cursor position till the end of the cursor position.</param>
        public void GoToEditingLoction(CEditingLocation editingLocation, bool isSelectRanage)
        {
            EditAreaObject areaObj = currentEditAreaObject;

            if (areaObj == null || !(areaObj.cPDFEditArea is CPDFEditTextArea))
            {
                return;
            }
            if (PDFViewer == null)
            {
                return;
            }

            CPDFEditTextArea textArea = areaObj.cPDFEditArea as CPDFEditTextArea;

            switch (editingLocation)
            {
                case CEditingLocation.CEditingLocationLineBegin:
                    textArea.GetLineBeginCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeLineEnd:
                    textArea.GetLineEndCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeSectionBegin:
                    textArea.GetSectionBeginCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeSectionEnd:
                    textArea.GetSectionEndCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypePreWord:
                    textArea.GetPreWordCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeNextWord:
                    textArea.GetNextWordCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypePreCharPlace:
                    textArea.GetPrevCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeNextCharPlace:
                    textArea.GetNextCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeUpCharPlace:
                    textArea.GetUpCharPlace();
                    break;
                case CEditingLocation.CEditingLoadTypeDownCharPlace:
                    textArea.GetDownCharPlace();
                    break;
                default:
                    return;
            }

            CPoint cursorPoint = new CPoint(-1, -1);
            CPoint pointHigh = new CPoint(0, 0);

            textArea.GetTextCursorPoints(ref cursorPoint, ref pointHigh);
            textArea.ClearSelectChars();

            Rect caretRect = new Rect(new Point(cursorPoint.x, cursorPoint.y), new Point(pointHigh.x, pointHigh.y));
            Point endPoint = DpiHelper.PDFPointToStandardPoint(new Point(caretRect.Left, (caretRect.Top + caretRect.Bottom) / 2));
            Point startPoint = DpiHelper.PDFPointToStandardPoint(new Point(rawHitPos.x, rawHitPos.y));

            startPoint.X = startPoint.X * currentZoom;
            startPoint.Y = startPoint.Y * currentZoom;

            endPoint.X = endPoint.X * currentZoom;
            endPoint.Y = endPoint.Y * currentZoom;

            if (isSelectRanage)
            {
                SelectText(textArea, startPoint, endPoint);
                CleanDraw();
            }
            else
            {
                DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                rawHitPos = cursorPoint;
            }

            Point caretPoint = new Point(endPoint.X + areaObj.PageBound.Left, endPoint.Y + areaObj.PageBound.Top);
            int direction = 1;
            if (caretPoint.X < 0 || caretPoint.X > PDFViewer.ViewportWidth)
            {
                if (caretPoint.X < 0)
                {
                    direction = -1;
                }
                double horizontal = PDFViewer.HorizontalOffset + PDFViewer.ViewportWidth / 2 * direction;
                PDFViewer.SetHorizontalOffset(horizontal);
            }
            if (caretPoint.Y < 0 || caretPoint.Y > PDFViewer.ViewportHeight)
            {
                if (caretPoint.Y < 0)
                {
                    direction = -1;
                }
                double vertical = PDFViewer.VerticalOffset + PDFViewer.ViewportHeight / 2 * direction;
                PDFViewer.SetVerticalOffset(vertical);
            }

        }

        public EditAreaObject CurrentEditAreaObject()
        {
            return currentEditAreaObject;
        }

        #region FrameSelect
        int selectFrameSelectToolTag = -1;
        private void InsertFrameSelectToolView()
        {
            FrameSelectTool frameSelectTool = new FrameSelectTool();
            int annotViewindex = PDFViewer.GetMaxViewIndex();
            PDFViewer.InsertView(annotViewindex, frameSelectTool);
            selectFrameSelectToolTag = frameSelectTool.GetResTag();
        }

        public void DrawFrameSelect()
        {
            if (!isMultiSelected)
            {
                return;
            }
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectFrameSelectToolTag);
            Point point = Mouse.GetPosition(this);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            (baseLayer as FrameSelectTool).StartDraw(point, paintRect, pageBound, PDFViewer.CurrentRenderFrame.ZoomFactor, index);
        }

        public void DrawMoveFrameSelect()
        {
            if (!isMultiSelected)
            {
                return;
            }
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectFrameSelectToolTag);
            Point point = Mouse.GetPosition(this);
            (baseLayer as FrameSelectTool).MoveDraw(point, PDFViewer.CurrentRenderFrame.ZoomFactor);
        }

        int FrameSelectPageIndex = -1;

        public Rect DrawEndFrameSelect()
        {
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return new Rect();
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectFrameSelectToolTag);
            return (baseLayer as FrameSelectTool).EndDraw(ref FrameSelectPageIndex);
        }

        /// <summary>
        /// Add multi Selected
        /// </summary>
        /// <param name="rectFrameSelect"></param>
        public void FrameSelectAddRect(Rect rectFrameSelect)
        {
            if (rectFrameSelect.Width == 0 || rectFrameSelect.Height == 0)
            {
                return;
            }
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }
            RenderFrame currentRenderFrame = PDFViewer.CurrentRenderFrame;
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);

            customizeLayer.Children.Clear();
            CaretVisual caretVisual = new CaretVisual(GetDefaultDrawParam());
            customizeLayer.Children.Add(caretVisual);

            currentZoom = currentRenderFrame.ZoomFactor;
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            foreach (RenderData item in currentRenderFrame.GetRenderDatas())
            {
                if (item.PageIndex == FrameSelectPageIndex)
                {
                    if (item.CPDFEditPageObj == null)
                    {
                        continue;
                    }
                    foreach (CPDFEditArea editArea in item.CPDFEditPageObj.GetEditAreaList())
                    {
                        Rect TextBlock = DataConversionForWPF.CRectConversionForRect(editArea.GetFrame());
                        if (editArea.Type == CPDFEditType.EditImage)
                        {
                            if ((contentEditType & CPDFEditType.EditImage) != CPDFEditType.EditImage)
                            {
                                continue;
                            }
                            TextBlock = DataConversionForWPF.CRectConversionForRect((editArea as CPDFEditImageArea).GetClipRect());
                        }
                        else if (editArea.Type == CPDFEditType.EditText)
                        {
                            if ((contentEditType & CPDFEditType.EditText) != CPDFEditType.EditText)
                            {
                                continue;
                            }
                        }
                        Rect rect = TextBlock;
                        if (rectFrameSelect.IntersectsWith(rect))
                        {
                            SelectedRect selectedRects = GetSelectedRectForEditAreaObject(editArea);
                            SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
                            selectedRect.SetEditPen(editPen, editHoverPen);
                            multiSelectedRect.SetSelectedType(SelectedType.PDFEdit);
                            selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
                            selectedRect.SetRect(selectedRects.GetRect(), currentZoom);
                            selectedRect.SetMaxRect(selectedRects.GetMaxRect());
                            EditAreaObject editAreaObject = null;

                            foreach (var eitem in this.editArea)
                            {
                                if (eitem.Value.cPDFEditArea == editArea)
                                {
                                    editAreaObject = eitem.Value;
                                    break;
                                }
                            }
                            int pageIndex = editAreaObject.PageIndex;
                            if (multiPage != pageIndex && editAreaList.Count > 0)
                            {
                                foreach (int itemIndex in editAreaMultiIndex)
                                {
                                    SelectedRect OldRect = GetEditAreaForIndex(multiPage, itemIndex);
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
                            multiPage = editAreaObject.PageIndex;
                            editAreaMultiIndex.Add(editAreaObject.EditAreaIndex);
                            editAreaList.Add(selectedRect, editAreaObject);
                            multiSelectedRect.Children.Add(selectedRect);
                            multiSelectedRect.SetMulitSelectedRect(selectedRect, editAreaObject.PageIndex, editAreaObject.EditAreaIndex);

                            multiSelectedRect.SetRect(selectedRects.GetRect());
                            multiSelectedRect.SetMaxRect(selectedRects.GetMaxRect());
                            multiSelectedRect.Draw();
                        }
                    }
                }
            }
            PDFViewer.UpdateRenderFrame();
        }

        #endregion

        #region Multiple selection alignment
        public void SetPDFEditAlignment(AlignModes align)
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (multiSelectedRect == null || multiSelectedRect.Children.Count <= 1 || align == AlignModes.AlignNone)
            {
                return;
            }
            Dictionary<AlignModes, Action> processAction = new Dictionary<AlignModes, Action>();
            processAction[AlignModes.AlignLeft] = SetPDFEditAlignLeft;
            processAction[AlignModes.AlignVerticalCenter] = SetPDFEditAlignVerticalCenter;
            processAction[AlignModes.AlignRight] = SetPDFEditAlignRight;
            processAction[AlignModes.AlignTop] = SetPDFEditAlignTop;
            processAction[AlignModes.AlignHorizonCenter] = SetPDFEditAlignHorizonCenter;
            processAction[AlignModes.AlignBottom] = SetPDFEditAlignBottom;
            processAction[AlignModes.DistributeHorizontal] = SetPDFEditDistributeHorizontal;
            processAction[AlignModes.DistributeVertical] = SetPDFEditDistributeVertical;

            if (processAction.ContainsKey(align))
            {
                processAction[align].Invoke();
            }
        }

        private Rect GetDrawAlignRect(Point RectMovePoint, Rect drawRect, Rect maxRect)
        {
            double TmpLeft, TmpRight, TmpUp, TmpDown;
            Point OffsetPos = CalcMoveBound(drawRect, RectMovePoint, maxRect);
            TmpLeft = drawRect.Left + OffsetPos.X;
            TmpRight = drawRect.Right + OffsetPos.X;
            TmpUp = drawRect.Top + OffsetPos.Y;
            TmpDown = drawRect.Bottom + OffsetPos.Y;
            return new Rect(TmpLeft, TmpUp, TmpRight - TmpLeft, TmpDown - TmpUp);
        }

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
            if (TmpLeft <= maxRect.Left)
            {
                TmpRight = (cRight - cLeft) + maxRect.Left;
                TmpLeft = maxRect.Left;
            }
            if (TmpUp <= maxRect.Top)
            {
                TmpDown = (cDown - cUp) + maxRect.Top;
                TmpUp = maxRect.Top;
            }
            if (TmpRight >= maxRect.Right)
            {
                TmpLeft = maxRect.Right - (cRight - cLeft);
                TmpRight = maxRect.Right;
            }
            if (TmpDown >= maxRect.Bottom)
            {
                TmpUp = maxRect.Bottom - (cDown - cUp);
                TmpDown = maxRect.Bottom;
            }
            offsetPoint = new Point(TmpLeft - cLeft, TmpUp - cUp);
            return offsetPoint;
        }

        private void SetPDFEditAlignLeft()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            Rect MaxRect = MultiSelectEditList.GetMaxRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                rect.X = rect.X + item.GetRectPadding();
                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignLeft(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                rect2.X = rect2.X + item.GetRectPadding();
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));

                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditAlignVerticalCenter()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                if (currentZoom < 1)
                {
                    rect.Y = rect.Y + currentZoom;
                }
                else
                {
                    rect.Y = rect.Y;
                }
                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignVerticalCenter(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                if (currentZoom < 1)
                {
                    rect2.Y = rect2.Y + currentZoom;
                }
                else
                {
                    rect2.Y = rect2.Y;
                }
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditAlignRight()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                if (currentZoom < 1)
                {
                    rect.X = rect.X - editPadding;
                }
                else
                {
                    rect.X = rect.X - editPadding;
                }

                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignRight(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                if (currentZoom < 1)
                {
                    rect2.X = rect2.X - editPadding;
                }
                else
                {
                    rect2.X = rect2.X - editPadding;
                }

                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditAlignTop()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                if (editAreaObject == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                rect.Y = rect.Y + editPadding;
                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignTop(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                rect2.Y = rect2.Y + editPadding;
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));

                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditAlignHorizonCenter()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignHorizonCenter(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditAlignBottom()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }

            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                rect.Y = rect.Y - editPadding;
                item.SetRect(GetDrawAlignRect(AlignmentsHelp.SetAlignBottom(rect, drawRect), rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                rect2.Y = rect2.Y - editPadding;
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditDistributeHorizontal()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }
            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            List<Rect> rects = new List<Rect>();
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect item in MultiSelectEditList.GetMulitSelectList())
            {
                Rect rect = item.GetRect();
                rects.Add(rect);
            }
            Dictionary<Rect, Point> rectandpoint = AlignmentsHelp.SetGapDistributeHorizontal(rects, drawRect);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                item.SetRect(GetDrawAlignRect(rectandpoint[rect], rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }

        private void SetPDFEditDistributeVertical()
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect MultiSelectEditList = CommonHelper.FindVisualChild<MultiSelectedRect>(PDFViewer.GetViewForTag(MultiSelectedRectViewTag));
            if (MultiSelectEditList == null || MultiSelectEditList.Children.Count <= 1)
            {
                return;
            }
            Rect maxRect = MultiSelectEditList.GetDrawRect();
            if (maxRect == Rect.Empty || (maxRect.Width * maxRect.Height) == 0)
            {
                return;
            }
            List<Rect> rects = new List<Rect>();
            Rect drawRect = MultiSelectEditList.GetDrawRect();
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(multiPage);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            cPDFEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage);
            foreach (SelectedRect item in MultiSelectEditList.GetMulitSelectList())
            {
                Rect rect = item.GetRect();
                rects.Add(rect);
            }
            Dictionary<Rect, Point> rectandpoint = AlignmentsHelp.SetGapDistributeVertical(rects, drawRect);
            foreach (SelectedRect checkItem in MultiSelectEditList.GetMulitSelectList())
            {
                SelectedRect item = checkItem;
                EditAreaObject editAreaObject = GetEditAreaObjectListForRect(item);
                if (editAreaObject == null)
                {
                    if (MultiSelectEditList.GetRelationKey(item, out int checkPage, out int checkEdit))
                    {
                        editAreaObject = GetEditAreaObjectListForIndex(checkPage, checkEdit);
                    }
                }
                if (item == null)
                {
                    continue;
                }
                PDFEditHistory pDFEditHistory = new PDFEditHistory();
                pDFEditHistory.PageIndex = multiPage;
                pDFEditHistory.EditPage = cPDFEditPage;
                if (editAreaObject == null)
                {
                    continue;
                }
                EditAreaObject newEditAreaObject = GetSelectedForIndex(multiPage, editAreaObject.EditAreaIndex);
                Rect rect = item.GetRect();
                item.SetRect(GetDrawAlignRect(rectandpoint[rect], rect, drawRect), currentZoom);
                Rect rect2 = item.GetRect();
                Rect pageBound = newEditAreaObject.PageBound;
                Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((rect2.Left - pageBound.Left) / currentZoom, (rect2.Top - pageBound.Top) / currentZoom, rect2.Width / currentZoom, rect2.Height / currentZoom));
                editAreaObject.cPDFEditArea.SetFrame(DataConversionForWPF.RectConversionForCRect(PDFRect));
                groupHistory.Histories.Add(pDFEditHistory);
            }
            PDFViewer.UndoManager.AddHistory(groupHistory);
            cPDFEditPage.EndEdit();
            MultiSelectEditList.Draw();
            PDFViewer.UpdateRenderFrame();
        }
        #endregion
    }
}
