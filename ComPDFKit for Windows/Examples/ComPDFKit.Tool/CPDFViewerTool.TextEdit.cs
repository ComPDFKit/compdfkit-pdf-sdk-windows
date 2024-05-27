using ComPDFKit.Import;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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


    public partial class CPDFViewerTool
    {
        /// <summary>
        /// Expend the area for taking pictures to avoid the edge of the picture not being completely covered due to accuracy
        /// </summary>
        private double editPadding = 5;
        Point pointtest;
        int pageindex;
        private SelectedRect lastSelectedRect = null;
        private SelectedRect lastHoverRect = null;
        private int cropIndex = -1;
        private int textEditTag = -1;
        private double currentZoom;
        private EditAreaObject currentEditAreaObject = null;
        int selectedEditPageIndex = -1;
        bool drawCaret = true;
        int selectedEditAreaIndex = -1;
        bool selectAllCharsForLine = false;

        private CPDFEditType contentEditType = CPDFEditType.EditText | CPDFEditType.EditImage;

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
        /// Cache the hit test rectangle area when loading the text block each time
        /// </summary>
        private List<SelectedRect> hitTestRects = new List<SelectedRect>();

        private List<SelectedRect> image = new List<SelectedRect>();

        private List<SelectedRect> text = new List<SelectedRect>();

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
                    }

                    CaretVisual caretVisual = CommonHelper.FindVisualChild<CaretVisual>(PDFViewer.GetViewForTag(textEditTag));
                    if(selectAllCharsForLine)
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
                    if (selectedEditAreaIndex == cropIndex)
                    {
                        item.Key.SetCurrentDrawPointType(DrawPointType.Crop);
                        ignorePoints.Add(PointControlType.Body);
                    }
                    else
                    {
                        lastSelectedRect.DataChanged -= LastSelectedRect_DataChanged;
                        lastSelectedRect.DataChanged += LastSelectedRect_DataChanged;
                    }

                    item.Key.SetIgnorePoints(ignorePoints);
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

        public void DrawStartTextEdit(SelectedRect selectedRect)
        {
            Point point = Mouse.GetPosition(this);
            if (!GetIsCropMode())
            {
                selectedRect.SetIgnorePoints(new List<PointControlType>());
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
                    selectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
                    selectedRect.Draw();
                }
            }
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
            SetPastePoint(new Point(-1,-1));
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

            foreach (SelectedRect rect in hitTestRects)
            {
                rect.SetIsHover(false);
                rect.SetIsSelected(false);
                PointControlType pointControlType = rect.GetHitControlIndex(point);
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
                                rect.SetIgnorePoints(new List<PointControlType>());
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
                            rect.SetIgnorePoints(new List<PointControlType>());
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
                    if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetPrevCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                    }
                    ke.Handled = true;
                    break;
                case Key.Right:
                    if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetNextCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                    }
                    ke.Handled = true;
                    break;
                case Key.Up:
                    if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetUpCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                    }
                    ke.Handled = true;
                    break;
                case Key.Down:
                    if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
                    {
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.GetDownCharPlace();
                        (currentEditAreaObject.cPDFEditArea as CPDFEditTextArea)?.ClearSelectChars();
                        DrawCaretVisualArea(currentEditAreaObject.cPDFEditArea as CPDFEditTextArea);
                    }
                    ke.Handled = true;
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
            if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
            {
                CPDFEditPage editPage = currentEditAreaObject.cPDFEditPage;
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
                DrawCaretVisualArea(textArea);
            }
        }

        public void RemoveTextBlock()
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
        }

        public void RemoveImageBlock()
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

        public void UpdateRender(Rect OldRect, CPDFEditArea NewTextArea)
        {
            if (currentEditAreaObject == null)
            {
                return;
            }
            if (NewTextArea == null)
            {
                return;
            }
            Dictionary<int, Rect> keyValuePairs = new Dictionary<int, Rect>();
            Rect NewRect = DataConversionForWPF.CRectConversionForRect(NewTextArea.GetFrame());

            // Calculate the rectangle
            OldRect.Union(NewRect);
            OldRect.X = OldRect.X - editPadding;
            OldRect.Y = OldRect.Y - editPadding;
            OldRect.Width = OldRect.Width + editPadding * 2;
            OldRect.Height = OldRect.Height + editPadding * 2;

            var e = editArea.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.cPDFEditArea.Equals(NewTextArea))
                {
                    currentEditAreaObject = e.Current.Value;
                }
            }

            // Calculate the area of the text block that does not need to be displayed completely
            Rect paintRect = currentEditAreaObject.PaintRect;
            Rect zoomPDFPaintRect = new Rect((paintRect.X - currentEditAreaObject.PageBound.X) / currentZoom, (paintRect.Y - currentEditAreaObject.PageBound.Y) / currentZoom, paintRect.Width / currentZoom, paintRect.Height / currentZoom);
            paintRect = DpiHelper.StandardRectToPDFRect(zoomPDFPaintRect);
            OldRect.Intersect(paintRect);

            keyValuePairs.Add(currentEditAreaObject.PageIndex, OldRect);
            DrawUpdateText(keyValuePairs, currentEditAreaObject.PageBound);
            UpdateSelectRect(NewTextArea);
            if (NewTextArea.Type == CPDFEditType.EditText)
            {
                DrawCaretVisualArea(NewTextArea as CPDFEditTextArea);
            }
            return;
        }

        public void DrawTest(Rect MaxRect, int index)
        {
            SelectedRect selectedRect = new SelectedRect(GetDefaultDrawParam(), SelectedType.PDFEdit);
            selectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
            BaseLayer customizeLayer = PDFViewer.GetViewForTag(textEditTag);

            customizeLayer.Children.Add(selectedRect);
            operateChildrenIndex = customizeLayer.Children.IndexOf(selectedRect);
            pointtest = Mouse.GetPosition(this);
            selectedRect.SetIgnorePointsAll();
            selectedRect.SetRect(new Rect(pointtest.X, pointtest.Y, 0, 0), currentZoom);
            selectedRect.SetMaxRect(MaxRect);
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
                foreach (SelectedRect rect in hitTestRects)
                {
                    PointControlType pointControlType = rect.GetHitControlIndex(point, false);
                    if (pointControlType != PointControlType.None)
                    {
                        EditAreaObject editAreaObject = GetEditAreaObjectForRect(rect);
                        if (editAreaObject.cPDFEditArea.Type == CPDFEditType.EditImage)
                        {
                            cursor = GetCursors(pointControlType, true);
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
            return cursor;
        }

        private Cursor GetCursors(PointControlType controlType, bool IsImage)
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
                    if (IsImage)
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
            else
            {
                return false;
            }
        }

        public void MoveEditArea(Point moveOffset, CPDFEditArea editArea)
        {
            CRect cRect = editArea.GetFrame();
            Rect rect = DataConversionForWPF.CRectConversionForRect(cRect);
            rect.X += moveOffset.X;
            rect.Y += moveOffset.Y;
            editArea.SetFrame(DataConversionForWPF.RectConversionForCRect(rect));
            UpdateSelectRect(editArea);
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
        private void SelectText(CPDFEditTextArea textArea, Point StartPoint, Point EndPoint)
        {
            Point zoomStartPoint = new Point(StartPoint.X / currentZoom, StartPoint.Y / currentZoom);
            Point zoomEndPoint = new Point(EndPoint.X / currentZoom, EndPoint.Y / currentZoom);
            Point startPoint = DpiHelper.StandardPointToPDFPoint(zoomStartPoint);
            Point endPoint = DpiHelper.StandardPointToPDFPoint(zoomEndPoint);
            textArea.ClearSelectChars();
            textArea.GetSelectChars(
                DataConversionForWPF.PointConversionForCPoint(startPoint),
                DataConversionForWPF.PointConversionForCPoint(endPoint)
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
        private void DrawCaretVisualArea(CPDFEditTextArea textArea, bool DrawCaret = true)
        {
            if (textArea == null)
            {
                return;
            }
            CPoint cursorCPoint = new CPoint(0, 0);
            CPoint highCpoint = new CPoint(0, 0);
            textArea.GetTextCursorPoints(ref cursorCPoint, ref highCpoint);
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
                caretVisual.Draw(true, false);
                Point HeightPoint = caretVisual.GetCaretHighPoint();
                Point caretPos = new Point(
                    HeightPoint.X * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().X,
                   HeightPoint.Y * currentZoom + GetSelectedRectForEditAreaObject(textArea).GetMaxRect().Y);
                SetPastePoint(caretPos);
                caretVisual.StopCaret();
            }
            else
            {
                caretVisual.Draw(true, DrawCaret);
                if (DrawCaret)
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
    }
}
