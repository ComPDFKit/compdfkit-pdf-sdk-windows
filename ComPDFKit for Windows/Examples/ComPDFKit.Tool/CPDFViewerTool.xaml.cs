using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using ComPDFKitViewer.Annot;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using ComPDFKitViewer.Widget;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UndoAction = ComPDFKitViewer.Helper.UndoAction;

namespace ComPDFKit.Tool
{
    public struct MouseEventObject
    {
        public MouseEventArgs mouseButtonEventArgs;
        public MouseHitTestType hitTestType;
        public C_ANNOTATION_TYPE annotType;
        /// <summary>
        /// Identifies whether the object is created
        /// </summary>
        public bool IsCreate;
        public bool IsDrawing;
        public bool IsMersured;
        public object Data;
    }

    public enum MouseHitTestType
    {
        Unknown,
        Text,
        Annot,
        SelectRect,
        Widget,
        TextEdit,
        ImageEdit,
        ImageSelect,
        MultiTextEdit,
        SelectedPageRect,
    }

    public enum ToolType
    {
        None = -1,
        Viewer,
        Pan,
        CreateAnnot,
        WidgetEdit,
        ContentEdit,
        Customize,
        SelectedPage,
    }

    public partial class CPDFViewerTool : UserControl
    {
        public bool IsDocumentModified
        {
            get => isDocumentModified;
            set
            {
                isDocumentModified = value;
                DocumentModifiedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CPDFViewerTool()
        {
            InitializeComponent();
            BindCommand();
            Application.Current.Exit += Current_Exit;
            InsertSelectImageView();
            InsertAnnotView();
            InsertAnnotEditView();
            InsertWidgetView();
            InsertSelectedRectView();
            InsertMultiSelectedRectView();
            InsertCustomizeToolView();
            InsertSelectTextView();
            //Frame Select
            InsertFrameSelectToolView();
            InsertTextEditView();
            InsertPageSelectedRectView();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            GetCPDFViewer().Dispose();
        }

        protected override Visual GetVisualChild(int index)
        {
            return base.GetVisualChild(index);
        }

        protected override int VisualChildrenCount => base.VisualChildrenCount;

        public event EventHandler<MouseEventObject> MouseLeftButtonDownHandler;
        public event EventHandler<MouseEventObject> MouseLeftButtonUpHandler;
        public event EventHandler<MouseEventObject> MouseMoveHandler;
        public event EventHandler<MouseEventObject> MouseRightButtonDownHandler;
        public event EventHandler DrawChanged;
        public event EventHandler DocumentModifiedChanged;

        private ToolType currentModel = ToolType.Viewer;

        DefaultSettingParam defaultSettingParam = new DefaultSettingParam();
        DefaultDrawParam defaultDrawParam = new DefaultDrawParam();
        MeasureSetting measureSetting = new MeasureSetting();

        Point Point = new Point();
        Point CachePoint = new Point();

        private bool isMultiSelected;
        private bool isDocumentModified = false;



        public bool CanAddTextEdit = true;

        protected bool isContinueCreateTextEdit = false;

        public bool GetIsMultiSelected()
        {
            return isMultiSelected;
        }

        /// <summary>
        /// Set whether continuous text editing is required
        /// </summary>
        /// <param name="isContinueCreateTextEdit"></param>
        public void SetContinueCreateTextEdit(bool isContinueCreateTextEdit)
        {

            this.isContinueCreateTextEdit = isContinueCreateTextEdit;
            CanAddTextEdit = true;
        }


        /// <summary>
        ///  Does it support multiple selection
        /// </summary>
        /// <param name="isMulti">true Can MultiSelected</param>
        public void SetIsMultiSelected(bool isMulti)
        {
            isMultiSelected = isMulti;
        }

        public DefaultSettingParam GetDefaultSettingParam()
        {
            return defaultSettingParam;
        }

        public DefaultDrawParam GetDefaultDrawParam()
        {
            return defaultDrawParam;
        }

        /// <summary>
        /// Set default painting parameters
        /// </summary>
        /// <param name="defaultDrawParam"></param>
        public void SetDefaultDrawParam(DefaultDrawParam defaultDrawParam = null)
        {
            if (defaultDrawParam == null)
            {
                this.defaultDrawParam = new DefaultDrawParam();
            }
            else
            {
                this.defaultDrawParam = defaultDrawParam;
            }
        }

        public MeasureSetting GetMeasureSetting()
        {
            return measureSetting;
        }

        public bool IsSelectRectMousePoint()
        {
            if (DrawSelectRectDownEvent() && cacheHitTestAnnot != null)
            {
                return true;
            }
            return false;
        }

        private void LinkAnnotAction(BaseAnnot annot)
        {
            AnnotData data = annot.GetAnnotData();
            CPDFLinkAnnotation linkAnnot = data.Annot as CPDFLinkAnnotation;
            CPDFAction action = linkAnnot.GetLinkAction();
            if (action != null)
            {
                ActionProcess(action);
            }
            else
            {
                CPDFDestination dest = linkAnnot.GetDestination(PDFViewer.GetDocument());
                if (dest != null)
                {
                    CPDFGoToAction gotoAction = new CPDFGoToAction();
                    gotoAction.SetDestination(PDFViewer.GetDocument(), dest);
                    ActionProcess(gotoAction);
                }
            }
        }

        public void ActionProcess(CPDFAction action)
        {
            if (action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case C_ACTION_TYPE.ACTION_TYPE_NAMED:
                    {
                        CPDFNamedAction namedAction = action as CPDFNamedAction;
                        string namedStr = namedAction.GetName();
                        switch (namedStr)
                        {
                            case "FirstPage":
                                {
                                    PDFViewer?.GoToPage(0, new Point(0, 0));
                                    break;
                                }
                            case "LastPage":
                                {
                                    PDFViewer?.GoToPage(PDFViewer.GetDocument().PageCount - 1, new Point(0, 0));
                                    break;
                                }
                            case "NextPage":
                                if (PDFViewer != null)
                                {
                                    int nextIndex = PDFViewer.CurrentRenderFrame.PageIndex + 1;
                                    if (nextIndex < PDFViewer.GetDocument().PageCount)
                                    {
                                        PDFViewer.GoToPage(nextIndex, new Point(0, 0));
                                    }
                                }
                                break;
                            case "PrevPage":
                                if (PDFViewer != null)
                                {
                                    int prevIndex = PDFViewer.CurrentRenderFrame.PageIndex - 1;
                                    if (prevIndex >= 0)
                                    {
                                        PDFViewer.GoToPage(prevIndex, new Point(0, 0));
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    }

                case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                    if (PDFViewer != null)
                    {
                        CPDFGoToAction gotoAction = action as CPDFGoToAction;
                        CPDFDestination dest = gotoAction.GetDestination(PDFViewer.GetDocument());
                        if (dest != null)
                        {
                            Size pageSize = DataConversionForWPF.CSizeConversionForSize(PDFViewer.GetDocument().GetPageSize(dest.PageIndex));
                            PDFViewer.GoToPage(dest.PageIndex, new Point(dest.Position_X, pageSize.Height - dest.Position_Y));
                        }
                    }
                    break;

                case C_ACTION_TYPE.ACTION_TYPE_GOTOR:
                    if (PDFViewer != null)
                    {
                        CPDFGoToRAction gotorAction = action as CPDFGoToRAction;
                        CPDFDestination dest = gotorAction.GetDestination(PDFViewer.GetDocument());
                        if (dest != null)
                        {
                            Size pageSize = DataConversionForWPF.CSizeConversionForSize(PDFViewer.GetDocument().GetPageSize(dest.PageIndex));
                            PDFViewer.GoToPage(dest.PageIndex, new Point(dest.Position_X, pageSize.Height - dest.Position_Y));
                        }
                    }
                    break;

                case C_ACTION_TYPE.ACTION_TYPE_URI:
                    {
                        CPDFUriAction uriAction = action as CPDFUriAction;
                        string uri = uriAction.GetUri();
                        try
                        {
                            if (!string.IsNullOrEmpty(uri))
                            {
                                Process.Start(uri);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    break;

                default:
                    break;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (isContinueCreateTextEdit)
            {

                if (lastSelectedRect != null)
                {
                    CanAddTextEdit = false;
                }
                else
                {
                    CanAddTextEdit = true;
                }
            }

            if (PDFViewer == null || PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }

            if (!HitTestBorder())
            {
                RemovePopTextUI();
            }
            Focus();
            Mouse.Capture(this, CaptureMode.SubTree);
            MouseEventObject mouseEventObject = new MouseEventObject
            {
                mouseButtonEventArgs = e,
                hitTestType = MouseHitTestType.Unknown,
                annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE,
                IsCreate = false
            };
            if (isDrawSelectRect)
            {
                if (e.ClickCount == 2)
                {
                    // Refresh the currently selected annotation object to ensure it is the latest
                    AnnotHitTest();
                    if (cacheHitTestAnnot is FreeTextAnnot)
                    {
                        BuildPopTextUI(cacheHitTestAnnot);
                        isDrawSelectRect = false;
                        mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                        mouseEventObject.IsMersured = cacheHitTestAnnot.GetAnnotData().Annot.IsMeasured();
                        MouseLeftButtonDownHandler?.Invoke(this, mouseEventObject);
                        return;
                    }
                    if (cacheHitTestAnnot is StickyNoteAnnot)
                    {
                        (cacheHitTestAnnot as StickyNoteAnnot).PopStickyNote();
                    }
                }
                // Click inside the selected rectangle area
                if (DrawSelectRectDownEvent() && cacheHitTestAnnot != null)
                {
                    mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                    mouseEventObject.IsMersured = cacheHitTestAnnot.GetAnnotData().Annot.IsMeasured();
                    MouseLeftButtonDownHandler?.Invoke(this, mouseEventObject);
                    return;
                }
                else
                {
                    CleanSelectedRect();
                }
            }

            if (IsDrawEditAnnot)
            {
                // Click inside the selected rectangle area
                if (DrawEditAnnotDownEvent() && cacheHitTestAnnot != null)
                {
                    mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                    mouseEventObject.IsMersured = cacheHitTestAnnot.GetAnnotData().Annot.IsMeasured();
                    MouseLeftButtonDownHandler?.Invoke(this, mouseEventObject);
                    return;
                }
            }

            Point = e.GetPosition(this);

            // Annotation selection effect
            if ((currentModel == ToolType.Pan
                || currentModel == ToolType.CreateAnnot)
                && AnnotHitTest()
                && IsCanSave()
                && !PDFViewer.GetIsShowStampMouse()
                )
            {
                //if (!IsCacheRedaction)
                {
                    if (cacheHitTestAnnot?.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_LINK && currentModel != ToolType.CreateAnnot)
                    {
                        LinkAnnotAction(cacheHitTestAnnot);
                    }
                    else
                    {
                        List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                        {
                            C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                        };
                        if (cacheHitTestAnnot != null && list.Contains(cacheHitTestAnnot.CurrentType))
                        {
                            mouseEventObject.IsMersured = cacheHitTestAnnot.GetAnnotData().Annot.IsMeasured();
                            SetEditAnnotObject();
                        }
                        else
                        {
                            SelectedAnnot();
                            CleanDrawSelectImage();
                        }
                        isDrawSelectRect = true;
                    }
                }
                mouseEventObject.hitTestType = MouseHitTestType.Annot;
                if (cacheHitTestAnnot != null)
                {
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                }
                else
                {
                    mouseEventObject.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;
                }
            }

            // Form selected effect
            else if ((currentModel == ToolType.Pan || currentModel == ToolType.Viewer) && AnnotWidgetHitTest())
            {
                mouseEventObject.hitTestType = MouseHitTestType.Widget;
                mouseEventObject.annotType = cacheMoveWidget.GetAnnotData().AnnotType;
                FormClickProcess();
            }
            else if ((currentModel == ToolType.Pan || currentModel == ToolType.Viewer))
            {
                if (DrawDownSelectImage(true))
                {
                    mouseEventObject.hitTestType = MouseHitTestType.ImageSelect;
                }
                else
                {
                    ReDrawSelectImage();
                }
            }

            // Form creation mode
            else if (currentModel == ToolType.WidgetEdit)
            {
                if (AnnotWidgetHitTest())
                {
                    cacheHitTestAnnot = PDFViewer?.AnnotHitTest() as BaseWidget;
                    SelectedAnnot();
                    mouseEventObject.hitTestType = MouseHitTestType.Annot;

                    mouseEventObject.annotType = cacheMoveWidget.GetAnnotData().AnnotType;
                }
            }
            // Content editing mode
            else if (currentModel == ToolType.ContentEdit)
            {
                OpenSelectedMulti(isMultiSelected);

                if (!PDFViewer.GetIsShowStampMouse())
                {
                    DrawTextEditDownEvent(true);
                }

                if (lastSelectedRect != null)
                {
                    //Multi selection processing optimization, other click effects
                    DrawEndFrameSelect();
                    if (!Keyboard.IsKeyDown(multiKey) || !isMultiSelected)
                    {
                        CleanSelectedMultiRect();
                        OpenSelectedMulti(false);
                        if (PDFViewer.CurrentRenderFrame != null)
                        {
                            currentZoom = PDFViewer.CurrentRenderFrame.ZoomFactor;
                            if (PDFViewer.CurrentRenderFrame.IsCacheEditPage == true && currentModel == ToolType.ContentEdit)
                            {
                                SetEditTextRect(PDFViewer.CurrentRenderFrame);
                                if (selectedEditPageIndex != -1 && selectedEditAreaIndex != -1)
                                {
                                    DrawSelectedEditAreaForIndex();
                                }
                            }
                        }
                        ReDrawSelectedMultiRect();
                    }
                    if (lastSelectedRect == null)
                    {
                        return;
                    }
                    SelectedMultiRect(lastSelectedRect.GetRect(), lastSelectedRect.GetMaxRect(), SelectedType.PDFEdit);
                    HideDrawSelectedMultiRect();
                    lastSelectedRect.DataChanged -= SelectedRect_DataChanged;
                    lastSelectedRect.DataChanged += SelectedRect_DataChanged;
                }
                else
                {
                    if (Keyboard.IsKeyDown(multiKey) && isMultiSelected)
                    {
                        DelMultiSelectRect();
                    }

                    if (HitTestMultiSelectedRect())
                    {
                        mouseEventObject.hitTestType = MouseHitTestType.MultiTextEdit;
                    }
                    else
                    {
                        //Clear the currently selected object
                        startSelectedRect = null;
                        startSelectedIndex = -1;
                        startSelectedPageIndex = -1;
                        startSelectedEditAreaObject = null;

                        CleanSelectedMultiRect();
                        OpenSelectedMulti(false);
                        if (PDFViewer.CurrentRenderFrame != null)
                        {
                            currentZoom = PDFViewer.CurrentRenderFrame.ZoomFactor;
                            if (PDFViewer.CurrentRenderFrame.IsCacheEditPage == true && currentModel == ToolType.ContentEdit)
                            {
                                SetEditTextRect(PDFViewer.CurrentRenderFrame);
                                if (selectedEditPageIndex != -1 && selectedEditAreaIndex != -1)
                                {
                                    DrawSelectedEditAreaForIndex();
                                }
                            }
                        }
                        ReDrawSelectedMultiRect();
                    }
                }
            }

            else if (currentModel == ToolType.SelectedPage)
            {
                if (HitTestPageSelectedRect())
                {

                }
                else
                {
                    CleanPageSelectedRect();
                    CreatePageSelectdRect();
                }
                mouseEventObject.hitTestType = MouseHitTestType.SelectedPageRect;
            }
            MouseLeftButtonDownHandler?.Invoke(this, mouseEventObject);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (PDFViewer == null || PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }

            MouseEventObject mouseEventObject = new MouseEventObject
            {
                mouseButtonEventArgs = e,
                hitTestType = MouseHitTestType.Unknown,
                annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE,
                IsCreate = false
            };

            if (isDrawSelectRect || IsDrawEditAnnot)
            {
                mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                if (cacheHitTestAnnot != null)
                {
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                }
                else
                {
                    mouseEventObject.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;
                }
                MouseLeftButtonUpHandler?.Invoke(this, mouseEventObject);
                ReleaseMouseCapture();
                return;
            }
            MouseLeftButtonUpHandler?.Invoke(this, mouseEventObject);
            ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (PDFViewer == null || PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }

            Cursor oldCursor = this.Cursor;
            Cursor newCursor = this.Cursor;
            MouseEventObject mouseEventObject = new MouseEventObject
            {
                mouseButtonEventArgs = e,
                hitTestType = MouseHitTestType.Unknown,
                annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE,
                IsCreate = false
            };

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                List<ToolType> allowModeList = new List<ToolType>()
                    {
                        ToolType.Pan,
                        ToolType.Viewer
                    };
                if (allowModeList.Contains(currentModel))
                {
                    newCursor = Cursors.Arrow;
                    if (caheMoveAnnot is BaseWidget)
                    {
                        BaseWidget widget = (BaseWidget)caheMoveAnnot;
                        if (widget.GetFormType() == PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON && PDFViewer != null)
                        {
                            newCursor = Cursors.Hand;
                        }
                    }
                    if (caheMoveAnnot is LinkAnnot)
                    {
                        newCursor = Cursors.Hand;
                    }
                }
            }

            if (!isDrawSelectRect && !IsDrawEditAnnot)
            {
                if (AnnotMoveHitTest())
                {
                    if (isCacheRedaction)
                    {
                        (caheMoveAnnot as RedactionAnnot).SetIsMouseHover(true);
                        (caheMoveAnnot as RedactionAnnot).Draw();
                    }

                    mouseEventObject.annotType = caheMoveAnnot.GetAnnotData().AnnotType;
                    mouseEventObject.IsMersured = caheMoveAnnot.GetAnnotData().Annot.IsMeasured();
                }
                else
                {
                    if (isCacheRedaction)
                    {
                        isCacheRedaction = false;
                        (caheMoveAnnot as RedactionAnnot).SetIsMouseHover(false);
                        (caheMoveAnnot as RedactionAnnot).Draw();
                    }

                    caheMoveAnnot = null;
                    if ((currentModel == ToolType.Pan || currentModel == ToolType.Viewer))
                    {
                        DrawMoveSelectImage();
                    }
                }
            }
            else
            {
                if (AnnotMoveHitTest())
                {
                    if (DrawSelectRectDownEvent() == false && Mouse.LeftButton != MouseButtonState.Pressed)
                    {
                        mouseEventObject.annotType = caheMoveAnnot.GetAnnotData().AnnotType;
                    }

                    if (isCacheRedaction)
                    {
                        (caheMoveAnnot as RedactionAnnot)?.SetIsMouseHover(true);
                        (caheMoveAnnot as RedactionAnnot)?.Draw();
                    }
                }
                else
                {
                    if (isCacheRedaction)
                    {
                        (caheMoveAnnot as RedactionAnnot)?.SetIsMouseHover(false);
                        (caheMoveAnnot as RedactionAnnot)?.Draw();
                    }
                }

                if (mouseEventObject.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_NONE)
                {
                    mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                    if (cacheHitTestAnnot != null)
                    {
                        mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                        mouseEventObject.IsMersured = cacheHitTestAnnot.GetAnnotData().Annot.IsMeasured();
                    }
                    else
                    {
                        mouseEventObject.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;
                    }
                }
            }

            MouseMoveHandler?.Invoke(this, mouseEventObject);
            PDFViewer.SetCustomMousePoint(Mouse.GetPosition(this).Y, Mouse.GetPosition(this).X);
            if (oldCursor != newCursor)
            {
                this.Cursor = newCursor;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            MouseEventObject mouseEventObject = new MouseEventObject
            {
                mouseButtonEventArgs = e,
                hitTestType = MouseHitTestType.Unknown,
                annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE,
                IsCreate = false
            };

            if (currentModel == ToolType.Pan || currentModel == ToolType.Viewer)
            {
                if (GetMousePointToTextSelectInfo())
                {
                    mouseEventObject.hitTestType = MouseHitTestType.Text;
                }
            }

            if (isDrawSelectRect)
            {
                // Click inside the selected rectangle area
                if (DrawSelectRectDownEvent() && cacheHitTestAnnot != null)
                {
                    mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                    MouseRightButtonDownHandler?.Invoke(this, mouseEventObject);
                    return;
                }
            }

            if (IsDrawEditAnnot)
            {
                // Click inside the selected rectangle area
                if (DrawEditAnnotDownEvent() && cacheHitTestAnnot != null)
                {
                    mouseEventObject.hitTestType = MouseHitTestType.SelectRect;
                    mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                    MouseRightButtonDownHandler?.Invoke(this, mouseEventObject);
                    return;
                }
            }

            Point = e.GetPosition(this);

            // Annotation selection effect
            if ((currentModel == ToolType.Pan || currentModel == ToolType.CreateAnnot))
            {
                if (AnnotHitTest())
                {

                    if (!isCacheRedaction)
                    {
                        if (cacheHitTestAnnot?.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_LINK && currentModel != ToolType.CreateAnnot)
                        {
                            LinkAnnotAction(cacheHitTestAnnot);
                        }
                        else
                        {
                            List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                        {
                            C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                        };
                            if (cacheHitTestAnnot != null && list.Contains(cacheHitTestAnnot.CurrentType))
                            {
                                SetEditAnnotObject();
                                DrawEditAnnotLayer();
                            }
                            else
                            {
                                SelectedAnnot();
                                DrawSelectedLayer();
                            }
                            isDrawSelectRect = true;
                        }
                    }
                    mouseEventObject.hitTestType = MouseHitTestType.Annot;
                    if (cacheHitTestAnnot != null)
                    {
                        mouseEventObject.annotType = cacheHitTestAnnot.GetAnnotData().AnnotType;
                    }
                    else
                    {
                        mouseEventObject.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;
                    }
                }
                else
                {
                    CleanSelectedRect();
                }
            }

            // Form selection effect
            if ((currentModel == ToolType.Pan || currentModel == ToolType.Viewer) && AnnotWidgetHitTest())
            {
                mouseEventObject.hitTestType = MouseHitTestType.Widget;
                mouseEventObject.annotType = cacheMoveWidget.GetAnnotData().AnnotType;
                FormClickProcess();
            }

            // Form creation mode
            if (currentModel == ToolType.WidgetEdit)
            {
                if (AnnotWidgetHitTest())
                {
                    cacheHitTestAnnot = PDFViewer?.AnnotHitTest() as BaseWidget;
                    SelectedAnnot();
                    DrawSelectedLayer();
                    mouseEventObject.hitTestType = MouseHitTestType.Annot;

                    mouseEventObject.annotType = cacheMoveWidget.GetAnnotData().AnnotType;
                }
                else
                {
                    CleanSelectedRect();
                }
            }

            // Content editing mode
            if (currentModel == ToolType.ContentEdit)
            {
                if (e.ClickCount == 1)
                {
                    DrawTextEditDownEvent(false);
                    if (GetLastSelectedRect() != null)
                    {
                        EditAreaObject editAreaObject = GetEditAreaObjectForRect(lastSelectedRect);
                        switch (editAreaObject.cPDFEditArea.Type)
                        {
                            case CPDFEditType.EditText:
                                mouseEventObject.hitTestType = MouseHitTestType.TextEdit;
                                break;
                            case CPDFEditType.EditImage:
                                mouseEventObject.hitTestType = MouseHitTestType.ImageEdit;
                                break;
                            default:
                                break;
                        }
                    } 
                }
            }
            else
            {
                if ((currentModel == ToolType.Viewer || currentModel == ToolType.Pan) && mouseEventObject.hitTestType == MouseHitTestType.Unknown && DrawDownSelectImage(false))
                {
                    mouseEventObject.hitTestType = MouseHitTestType.ImageSelect;
                }
            }
            MouseRightButtonDownHandler?.Invoke(this, mouseEventObject);
        }

        public bool GetIsCropMode()
        {
            if (lastSelectedRect != null)
            {
                return lastSelectedRect.GetCurrentDrawPointType() == DrawPointType.Crop;
            }
            return false;
        }

        public void SetCropMode(bool crop)
        {
            if (lastSelectedRect != null)
            {
                List<PointControlType> ignoreList = new List<PointControlType>();
                if (crop)
                {
                    lastSelectedRect.SetCurrentDrawPointType(DrawPointType.Crop);
                    ignoreList.Add(PointControlType.Body);
                    // Initialize ClipRect
                    ClipThickness = new Thickness(0, 0, 0, 0);
                    if (editArea.TryGetValue(lastSelectedRect, out EditAreaObject editAreaObject))
                    {
                        cropIndex = editAreaObject.EditAreaIndex;
                    }
                    lastSelectedRect.DataChanged -= LastSelectedRect_DataChanged;
                }
                else
                {
                    lastSelectedRect.SetCurrentDrawPointType(DrawPointType.Square);
                    cropIndex = -1;
                    lastSelectedRect.DataChanged += LastSelectedRect_DataChanged;
                }
                lastSelectedRect.SetIgnorePoints(ignoreList);
                lastSelectedRect.Draw();
            }
        }

        internal void SetToolType(ToolType model)
        {
            currentModel = model;
            CPDFViewer pdfViewer = GetCPDFViewer();

            if (pdfViewer != null)
            {
                if (currentModel == ToolType.WidgetEdit)
                {
                    pdfViewer.IsHideFormShow = true;
                }
                else
                {
                    pdfViewer.IsHideFormShow = false;
                }
            }
        }

        public ToolType GetToolType()
        {
            return currentModel;
        }

        public void SavePoint()
        {
            CachePoint = Point;
        }

        public void CleanPoint()
        {
            CachePoint = new Point();
        }

        private void CPDFViewerTool_Loaded(object sender, RoutedEventArgs e)
        {
            PDFViewer.DrawChanged += PDFViewer_DrawChanged;
            PDFViewer.UndoManager.HistoryChanged += UndoManager_HistoryChanged;
            PDFViewer.MouseEnter += PDFViewer_MouseEnter;
            PDFViewer.MouseLeave += PDFViewer_MouseLeave;
        }

        private void PDFViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            PDFViewer.IsVisibilityMouse(false);
        }

        private void PDFViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            PDFViewer.IsVisibilityMouse(true);
        }

        private void UndoManager_HistoryChanged(object sender, KeyValuePair<UndoAction, IHistory> data)
        {
            IsDocumentModified = true;
        }

        private void CPDFViewerTool_Unloaded(object sender, RoutedEventArgs e)
        {
            PDFViewer.DrawChanged -= PDFViewer_DrawChanged;
        }

        private void PDFViewer_DrawChanged(object sender, EventArgs e)
        {
            SizeChangeds();
            DrawChanged?.Invoke(this, e);
        }

        public void SizeChangeds()
        {
            if (IsLoaded)
            {
                if (cacheHitTestAnnot != null)
                {
                    BaseLayer baseLayer1 = PDFViewer.GetViewForTag(PDFViewer.GetAnnotViewTag());
                    bool Update = (baseLayer1 as AnnotLayer).GetUpdate(ref cacheHitTestAnnot);
                    if (Update)
                    {
                        if (IsDrawEditAnnot)
                        {
                            SetEditAnnotObject();
                            DrawEditAnnotLayer();
                        }

                        if (isDrawSelectRect)
                        {
                            List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                            {
                                C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                                C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                                C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                            };

                            if (cacheHitTestAnnot != null && list.Contains(cacheHitTestAnnot.CurrentType))
                            {
                                SetEditAnnotObject();
                                DrawEditAnnotLayer();
                            }
                            else
                            {
                                SelectedAnnot();
                            }
                            DrawSelectedLayer();
                        }
                    }
                    else
                    {
                        SelectedAnnot(null);
                    }
                }
                else if (selectedPageIndex != -1 && selectedAnnotIndex != -1)
                {
                    BaseLayer baseLayer1 = PDFViewer.GetViewForTag(PDFViewer.GetAnnotViewTag());
                    cacheHitTestAnnot = (baseLayer1 as AnnotLayer).GetSelectedAnnot(ref selectedPageIndex, ref selectedAnnotIndex);
                    if (cacheHitTestAnnot != null)
                    {
                        List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                        {
                            C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                        };
                        if (cacheHitTestAnnot != null && list.Contains(cacheHitTestAnnot.CurrentType))
                        {
                            SetEditAnnotObject();
                            DrawEditAnnotLayer();
                        }
                        else
                        {
                            SelectedAnnot();
                        }
                        DrawSelectedLayer();
                        isDrawSelectRect = true;
                    }
                }
                if (PDFViewer.CurrentRenderFrame != null)
                {
                    currentZoom = PDFViewer.CurrentRenderFrame.ZoomFactor;
                    if (PDFViewer.CurrentRenderFrame.IsCacheEditPage == true && currentModel == ToolType.ContentEdit)
                    {
                        SetEditTextRect(PDFViewer.CurrentRenderFrame);
                        if (selectedEditPageIndex != -1 && selectedEditAreaIndex != -1)
                        {
                            DrawSelectedEditAreaForIndex();
                        }
                    }
                }
                ReDrawSelectedMultiRect();
                ReDrawWidget();
                ReDrawSelectText();
                ReDrawSelectImage();
                UpdateFormHitPop();
                UpdateTextPop();
            }
        }

        private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
