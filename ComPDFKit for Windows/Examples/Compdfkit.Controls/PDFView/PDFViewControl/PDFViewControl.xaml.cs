using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Widget;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;
using SplitMode = ComPDFKit.Controls.PDFControlUI.CPDFViewModeUI.SplitMode;
using System.Threading;
using System.Threading.Tasks;
using ComPDFKitViewer.Annot;
using System.IO;
using ComPDFKit.Import;
using ComPDFKit.PDFPage;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class PDFViewControl : UserControl
    {
        private CPDFViewerTool splitViewerTool;
        private CPDFToolManager splitToolManager;

        private CPDFViewerTool viewerTool;
        private CPDFToolManager toolManager;

        public string Password { get; set; } = string.Empty;

        public CPDFViewerTool PDFViewTool { get; private set; }

        public CPDFToolManager PDFToolManager { get; private set; }

        public CPDFViewerTool FocusPDFViewTool { get; private set; }

        //public event EventHandler SplitPDFViewToolCreated;
        public event EventHandler FocusPDFViewToolChanged;
        public event EventHandler<MouseEventObject> MouseLeftButtonDownHandler;
        public event EventHandler<MouseEventObject> MouseLeftButtonUpHandler;
        public event EventHandler<MouseEventObject> MouseMoveHandler;
        public event EventHandler<MouseEventObject> MouseRightButtonDownHandler;
        public event EventHandler<MouseWheelZoomArgs> MouseWheelZoomHandler;
        public event EventHandler DrawChanged;

        public PageSelectedData SnapshotData { get; private set; }

        //private ContextMenu RightMenu;
        #region Properties

        private double splitWidthScale = 0.5;
        private double splitHeightScale = 0.5;

        public bool CustomSignHandle { get; set; }

        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        #endregion

        public PDFViewControl()
        {
            InitializeComponent();
            viewerTool = new CPDFViewerTool();
            toolManager = new CPDFToolManager(viewerTool);
            PDFViewTool = viewerTool;
            PDFToolManager = toolManager;
            PDFToolManager.SetToolType(ToolType.Viewer);

            splitViewerTool = new CPDFViewerTool();
            splitToolManager = new CPDFToolManager(splitViewerTool);
            splitToolManager.SetToolType(ToolType.Viewer);

            FocusPDFViewTool = viewerTool;
            PDFView.Child = PDFViewTool;
            VerticalView.Child = splitViewerTool;

            PDFViewTool.SizeChanged -= PDFViewTool_SizeChanged;
            PDFViewTool.GetCPDFViewer().MouseWheelZoomHandler -= PDFViewControl_MouseWheelZoomHandler;
            PDFViewTool.GetCPDFViewer().MouseMove -= PDFViewControl_MouseMove;
            PDFViewTool.DrawChanged -= PDFViewTool_DrawChanged;
            PDFViewTool.PageSelectedChanged -= PDFViewTool_PageSelectedChanged;
            PDFToolManager.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PDFToolManager.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            PDFToolManager.MouseMoveHandler -= PDFToolManager_MouseMoveHandler;
            PDFToolManager.MouseRightButtonDownHandler -= PDFToolManager_MouseRightButtonDownHandler;

            PDFViewTool.SizeChanged += PDFViewTool_SizeChanged;
            PDFViewTool.GetCPDFViewer().MouseWheelZoomHandler += PDFViewControl_MouseWheelZoomHandler;
            PDFViewTool.GetCPDFViewer().MouseMove += PDFViewControl_MouseMove;
            PDFViewTool.DrawChanged += PDFViewTool_DrawChanged;
            PDFViewTool.PageSelectedChanged += PDFViewTool_PageSelectedChanged;
            PDFToolManager.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
            PDFToolManager.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            PDFToolManager.MouseMoveHandler += PDFToolManager_MouseMoveHandler;
            PDFToolManager.MouseRightButtonDownHandler += PDFToolManager_MouseRightButtonDownHandler;

            splitViewerTool.SizeChanged -= SplitViewerTool_SizeChanged;
            splitViewerTool.GetCPDFViewer().MouseWheelZoomHandler -= SplitPDFViewControl_MouseWheelZoomHandler;
            splitViewerTool.DrawChanged -= PDFViewTool_DrawChanged;
            splitViewerTool.PageSelectedChanged -= PDFViewTool_PageSelectedChanged;
            splitToolManager.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            splitToolManager.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            splitToolManager.MouseMoveHandler -= PDFToolManager_MouseMoveHandler;
            splitToolManager.MouseRightButtonDownHandler -= PDFToolManager_MouseRightButtonDownHandler;

            splitViewerTool.SizeChanged += SplitViewerTool_SizeChanged;
            splitViewerTool.GetCPDFViewer().MouseWheelZoomHandler += SplitPDFViewControl_MouseWheelZoomHandler;
            splitViewerTool.DrawChanged += PDFViewTool_DrawChanged;
            splitViewerTool.PageSelectedChanged += PDFViewTool_PageSelectedChanged;
            splitToolManager.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
            splitToolManager.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            splitToolManager.MouseMoveHandler += PDFToolManager_MouseMoveHandler;
            splitToolManager.MouseRightButtonDownHandler += PDFToolManager_MouseRightButtonDownHandler;

            GetCPDFViewer().OnRenderFinish -= PDFViewControl_OnRenderFinish;
            GetCPDFViewer().OnRenderFinish += PDFViewControl_OnRenderFinish;

            SetCursor();
        }

        private void PDFViewControl_OnRenderFinish(object sender, EventArgs e)
        {
            SetCursorStatus();
        }

        private void PDFViewTool_DrawChanged(object sender, EventArgs e)
        {
            FocusPDFViewToolChanged?.Invoke(this, EventArgs.Empty);
            DrawChanged?.Invoke(sender, e);
        }

        private void PDFViewTool_PageSelectedChanged(object sender, PageSelectedData e)
        {
            SnapshotData = e;
        }

        public ContextMenu GetRightMenu()
        {
            FocusPDFViewTool.ContextMenu?.Items.Clear();
            return FocusPDFViewTool.ContextMenu;
        }

        public void SetRightMenu(ContextMenu contextMenu)
        {
            FocusPDFViewTool.ContextMenu = contextMenu;
        }

        private void PDFToolManager_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            Point point = e.mouseButtonEventArgs.GetPosition(this);
            viewerTool.GetCPDFViewer().GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            if (index == -1)
            {
                viewerTool.ContextMenu = null;
                return;
            }
            PDFViewTool.SetPastePoint(point);
            MouseRightButtonDownHandler?.Invoke(sender, e);
        }

        private void PDFToolManager_MouseMoveHandler(object sender, MouseEventObject e)
        {
            MouseMoveHandler?.Invoke(this, e);
        }

        private void PDFToolManager_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            MouseLeftButtonUpHandler?.Invoke(sender, e);
            if (IsHitEmpty)
            {
                List<ToolType> toolTypes = new List<ToolType>()
                {
                    ToolType.Viewer,
                    ToolType.Pan
                };
                ToolType currentMode = PDFViewTool.GetToolType();

                if (GetCPDFViewer().Cursor == panToolCursor || GetCPDFViewer().Cursor == panTool2Cursor)
                {
                    if (toolTypes.Contains(currentMode))
                    {
                        GetCPDFViewer().Cursor = panToolCursor;
                    }
                    else
                    {
                        GetCPDFViewer().Cursor = Cursors.Arrow;
                    }
                }
                IsHitEmpty = false;
            }
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            MouseLeftButtonDownHandler?.Invoke(sender, e);
            ToolType toolType = PDFToolManager.GetToolType();

            if (e.hitTestType != MouseHitTestType.Text && e.hitTestType != MouseHitTestType.SelectedPageRect && e.hitTestType != MouseHitTestType.Annot && e.hitTestType != MouseHitTestType.SelectRect && e.hitTestType != MouseHitTestType.Widget)
            {
                if (toolType == ToolType.Pan)
                {
                    IsHitEmpty = true;
                    hitEmptyPos = e.mouseButtonEventArgs.GetPosition(this);
                    GetCPDFViewer().Cursor = panToolCursor;
                }
            }
        }

        private void PDFViewTool_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void SplitViewerTool_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (VerticalView.Visibility == Visibility.Visible)
            {
                splitWidthScale = VerticalView.ActualWidth / ViewToolGrid.ActualWidth;
            }

            if (HorizontalView.Visibility == Visibility.Visible)
            {
                splitHeightScale = HorizontalView.ActualHeight / ViewToolGrid.ActualHeight;
            }
        }

        public void InitDocument(string Path)
        {
            CPDFDocument pdfDoc = CPDFDocument.InitWithFilePath(Path);
            if (pdfDoc != null)
            {
                PDFViewTool.GetCPDFViewer().InitDoc(pdfDoc);

                PDFViewTool.GetCPDFViewer().SetFitMode(FitMode.FitHeight);
                PDFViewTool.GetCPDFViewer().SetViewMode(ViewMode.SingleContinuous);

                PDFViewTool.SetIsMultiSelected(true);

                splitViewerTool.GetCPDFViewer().InitDoc(pdfDoc);

                splitViewerTool.GetCPDFViewer().SetFitMode(FitMode.FitHeight);
                splitViewerTool.GetCPDFViewer().SetViewMode(ViewMode.SingleContinuous);
            }
        }

        private void PDFViewControl_MouseWheelZoomHandler(object sender, ComPDFKitViewer.MouseWheelZoomArgs e)
        {
            FocusPDFViewTool = PDFViewTool;
            FocusPDFViewToolChanged?.Invoke(this, EventArgs.Empty);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.IsZoom = true;
                PDFViewTool.GetCPDFViewer().SetFitMode(FitMode.FitZoom);
                double zoom = PDFViewTool.GetCPDFViewer().GetZoom();
                PDFViewTool.GetCPDFViewer().SetZoom(CheckZoomLevel(zoom, Convert.ToBoolean(e.WheelBehavior)));
                PDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            }
            MouseWheelZoomHandler?.Invoke(this, e);
        }

        private void SplitPDFViewControl_MouseWheelZoomHandler(object sender, ComPDFKitViewer.MouseWheelZoomArgs e)
        {
            FocusPDFViewTool = splitViewerTool;
            FocusPDFViewToolChanged?.Invoke(this, EventArgs.Empty);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.IsZoom = true;
                splitViewerTool.GetCPDFViewer().SetFitMode(FitMode.FitZoom);
                double zoom = splitViewerTool.GetCPDFViewer().GetZoom();
                splitViewerTool.GetCPDFViewer().SetZoom(CheckZoomLevel(zoom, Convert.ToBoolean(e.WheelBehavior)));
                splitViewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
            }
            MouseWheelZoomHandler?.Invoke(this, e);
        }

        public void SetSplitViewMode(SplitMode splitMode)
        {
            switch (splitMode)
            {
                case SplitMode.None:
                    RemoveSplitViewerTool();
                    break;
                case SplitMode.Vertical:
                    RemoveSplitViewerTool();
                    if (splitViewerTool != null)
                    {
                        VerticalSplitter.Visibility = Visibility.Visible;
                        VerticalView.Visibility = Visibility.Visible;
                        ViewToolGrid.ColumnDefinitions[1].Width = new GridLength(15, GridUnitType.Auto);
                        ViewToolGrid.ColumnDefinitions[2].Width = new GridLength(ViewToolGrid.ActualWidth / 2);
                        VerticalView.Child = splitViewerTool;
                    }
                    break;
                case SplitMode.Horizontal:
                    RemoveSplitViewerTool();
                    if (splitViewerTool != null)
                    {
                        HorizontalSplitter.Visibility = Visibility.Visible;
                        HorizontalView.Visibility = Visibility.Visible;
                        ViewToolGrid.RowDefinitions[1].Height = new GridLength(15, GridUnitType.Auto);
                        ViewToolGrid.RowDefinitions[2].Height = new GridLength(ViewToolGrid.ActualHeight / 2);
                        HorizontalView.Child = splitViewerTool;
                    }
                    break;
            }
            UpdateRenderFrame();
            UpdateAnnotFrame();
        }

        public void UpdateRenderFrame()
        {
            viewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
            splitViewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
        }

        public void UpdateAnnotFrame()
        {
            viewerTool?.GetCPDFViewer()?.UpdateAnnotFrame();
            splitViewerTool?.GetCPDFViewer()?.UpdateAnnotFrame();
        }

        public void SetCropMode(bool isCrop)
        {
            viewerTool?.GetCPDFViewer()?.SetIsCrop(isCrop);
            splitViewerTool?.GetCPDFViewer()?.SetIsCrop(isCrop);

            viewerTool?.GetCPDFViewer()?.UpdateVirtualNodes();
            splitViewerTool?.GetCPDFViewer()?.UpdateVirtualNodes();

            viewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
            splitViewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
        }

        public void SetToolType(ToolType type)
        {
            toolManager?.SetToolType(type);
            splitToolManager?.SetToolType(type);
        }

        public void SetCreateAnnotType(C_ANNOTATION_TYPE type)
        {
            toolManager?.SetCreateAnnotType(type);
            splitToolManager?.SetCreateAnnotType(type);
        }

        public void SetCreateWidgetType(C_WIDGET_TYPE type)
        {
            toolManager?.SetCreateWidgetType(type);
            splitToolManager?.SetCreateWidgetType(type);
        }

        public void SetAnnotParam(AnnotParam param)
        {
            DefaultSettingParam defaultSettingParam = viewerTool.GetDefaultSettingParam();
            DefaultSettingParam splitSettingParam = splitViewerTool.GetDefaultSettingParam();
            defaultSettingParam.SetAnnotParam(param);
            splitSettingParam.SetAnnotParam(param);
        }

        public void SetIsShowStampMouse(bool isShow)
        {
            viewerTool?.GetCPDFViewer()?.SetIsShowStampMouse(isShow);
            splitViewerTool?.GetCPDFViewer()?.SetIsShowStampMouse(isShow);
        }

        public void SetDrawModes(DrawModeData drawMode)
        {
            viewerTool?.GetCPDFViewer()?.SetDrawModes(drawMode.DrawMode);
            splitViewerTool?.GetCPDFViewer()?.SetDrawModes(drawMode.DrawMode);
            if (drawMode.DrawMode == DrawMode.Custom)
            {
                viewerTool?.GetCPDFViewer().SetPDFBackground(drawMode.CustomColor);
                splitViewerTool?.GetCPDFViewer().SetPDFBackground(drawMode.CustomColor);
            }
            UpdateRenderFrame();
        }

        public void SetStampMouseImage(byte[] imageArray, int imageWidth, int imageHeight)
        {
            viewerTool?.GetCPDFViewer()?.SetStampMouseImage(imageArray, imageWidth, imageHeight);
            splitViewerTool?.GetCPDFViewer()?.SetStampMouseImage(imageArray, imageWidth, imageHeight);
        }

        public bool SetStampMouseImage(string filePath)
        {
            if ((bool)(viewerTool?.GetCPDFViewer()?.SetStampMouseImage(filePath)) && (bool)splitViewerTool?.GetCPDFViewer()?.SetStampMouseImage(filePath))
            {
                return true;
            }
            return false;
        }

        public void SetIsVisibleCustomMouse(bool isVisbleCustomMouse)
        {
            viewerTool?.GetCPDFViewer()?.SetIsVisibleCustomMouse(isVisbleCustomMouse);
            splitViewerTool?.GetCPDFViewer()?.SetIsVisibleCustomMouse(isVisbleCustomMouse);
        }

        public CPDFViewer GetCPDFViewer()
        {
            return FocusPDFViewTool?.GetCPDFViewer();
        }

        public BaseWidget GetCacheHitTestWidget()
        {
            return FocusPDFViewTool?.GetCacheHitTestWidget();
        }

        public BaseAnnot GetCacheHitTestAnnot()
        {
            return FocusPDFViewTool?.GetCacheHitTestAnnot();
        }

        public void CreateViewerMenu(object sender, ref ContextMenu contextMenu)
        {
            MenuItem fitWidthMenu = new MenuItem();
            fitWidthMenu.Header = LanguageHelper.CommonManager.GetString("Menu_AutoSize");
            fitWidthMenu.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetFitMode(FitMode.FitWidth);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(fitWidthMenu);

            MenuItem fitSizeMenu = new MenuItem();
            fitSizeMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RealSize");
            fitSizeMenu.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetFitMode(FitMode.FitOriginal);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(fitSizeMenu);

            MenuItem zoomInMenu = new MenuItem();
            zoomInMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomIn");
            zoomInMenu.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetFitMode(FitMode.FitZoom);
                double zoom = FocusPDFViewTool?.GetCPDFViewer()?.GetZoom() ?? 1;
                FocusPDFViewTool?.GetCPDFViewer()?.SetZoom(CheckZoomLevel(zoom, true));
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(zoomInMenu);

            MenuItem zoomOutMenu = new MenuItem();
            zoomOutMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomOut");
            zoomOutMenu.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetFitMode(FitMode.FitZoom);
                double zoom = FocusPDFViewTool?.GetCPDFViewer()?.GetZoom() ?? 1;
                FocusPDFViewTool?.GetCPDFViewer()?.SetZoom(CheckZoomLevel(zoom, false));
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(zoomOutMenu);

            contextMenu.Items.Add(new Separator());

            MenuItem singleView = new MenuItem();
            singleView.Header = LanguageHelper.CommonManager.GetString("Menu_SinglePage");
            singleView.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetViewMode(ViewMode.Single);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(singleView);

            MenuItem singleContinuousView = new MenuItem();
            singleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_SingleContinuous");
            singleContinuousView.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetViewMode(ViewMode.SingleContinuous);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(singleContinuousView);

            MenuItem doubleView = new MenuItem();
            doubleView.Header = LanguageHelper.CommonManager.GetString("Menu_DoublePage");
            doubleView.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetViewMode(ViewMode.Double);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(doubleView);

            MenuItem doubleContinuousView = new MenuItem();
            doubleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_DoubleContinuous");
            doubleContinuousView.Click += (o, p) =>
            {
                FocusPDFViewTool?.GetCPDFViewer()?.SetViewMode(ViewMode.DoubleContinuous);
                FocusPDFViewTool?.GetCPDFViewer()?.UpdateRenderFrame();
            };
            contextMenu.Items.Add(doubleContinuousView);
            contextMenu.Items.Add(new Separator());

            MenuItem resetFormMenu = new MenuItem();
            resetFormMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Reset");
            resetFormMenu.Click += (o, p) =>
            {
                //FocusPDFViewTool?.GetCPDFViewer()?.ResetForm();
            };
            contextMenu.Items.Add(resetFormMenu);
        }

        private void RemoveSplitViewerTool()
        {
            UpdateRenderFrame();
            VerticalSplitter.Visibility = Visibility.Collapsed;
            HorizontalSplitter.Visibility = Visibility.Collapsed;
            VerticalView.Visibility = Visibility.Collapsed;
            HorizontalView.Visibility = Visibility.Collapsed;
            ViewToolGrid.ColumnDefinitions[1].Width = new GridLength(0);
            ViewToolGrid.ColumnDefinitions[2].Width = new GridLength(0);
            ViewToolGrid.RowDefinitions[1].Height = new GridLength(0);
            ViewToolGrid.RowDefinitions[2].Height = new GridLength(0);
            VerticalView.Child = null;
            HorizontalView.Child = null;
        }

        public void WindowSizeChange()
        {
            if (VerticalView.Visibility == Visibility.Visible)
            {
                ViewToolGrid.ColumnDefinitions[2].Width = new GridLength(ViewToolGrid.ActualWidth * splitWidthScale);
            }
            if (HorizontalView.Visibility == Visibility.Visible)
            {
                ViewToolGrid.RowDefinitions[2].Height = new GridLength(ViewToolGrid.ActualHeight * splitHeightScale);
            }
        }

        private void PDFView_GotFocus(object sender, RoutedEventArgs e)
        {
            switch ((sender as Border).Tag)
            {
                case "ViewerTool":
                    FocusPDFViewTool = viewerTool;
                    break;
                case "SplitViewerTool":
                    FocusPDFViewTool = splitViewerTool;
                    break;
            }
            if (FocusPDFViewTool.IsLoaded)
            {
                FocusPDFViewToolChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ReloadDocument()
        {
            viewerTool?.GetCPDFViewer()?.UpdateVirtualNodes();
            splitViewerTool?.GetCPDFViewer()?.UpdateVirtualNodes();
            viewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
            splitViewerTool?.GetCPDFViewer()?.UpdateRenderFrame();
        }

        public void CropPage(CPDFDisplayBox cropBox, Rect cropRect, List<int> pagesList)
        {
            bool needLoad = false;
            CPDFDocument doc = FocusPDFViewTool.GetCPDFViewer().GetDocument();

            foreach (int pageIndex in pagesList)
            {
                if (pageIndex < 0 || pageIndex > doc.PageCount)
                {
                    continue;
                }
                CPDFPage CropPage = doc.PageAtIndex(pageIndex);
                CRect rect = new CRect((float)cropRect.X, (float)(cropRect.Y + cropRect.Height), (float)(cropRect.X + cropRect.Width), (float)cropRect.Y);
                CropPage.CropPage(cropBox, rect);
                CropPage.ReleaseAllAnnotations();
                needLoad = true;
            }

            if (needLoad)
            {
                ReloadDocument();
            }
        }

        #region Private Command Methods
        private double CheckZoomLevel(double zoom, bool IsGrowth)
        {
            zoom += (IsGrowth ? 0.01 : -0.01);
            double standardZoom = 100;
            if (zoom <= 0.01)
            {
                return 0.01;
            }
            if (zoom >= 10)
            {
                return 10;
            }

            zoom *= 100;
            for (int i = 0; i < zoomLevelList.Length - 1; i++)
            {
                if (zoom > zoomLevelList[i] && zoom <= zoomLevelList[i + 1] && IsGrowth)
                {
                    standardZoom = zoomLevelList[i + 1];
                    break;
                }
                if (zoom >= zoomLevelList[i] && zoom < zoomLevelList[i + 1] && !IsGrowth)
                {
                    standardZoom = zoomLevelList[i];
                    break;
                }
            }
            return standardZoom / 100;
        }

        #endregion

        #region automatic scroll
        internal bool EnableAutoScroll { get; set; } = true;
        public bool IsHitEmpty
        {
            get;
            private set;
        }

        private bool cancelTask = false;
        private bool scrollEnable = true;
        private bool isAutomaticScroll = false;
        private Point mouseCurrentPoint;
        private Point middlePressPoint;
        private Point middleMovePoint;
        private Point hitEmptyPos;
        private Task MiddleScrollTask;

        private void PDFView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch ((sender as Border).Tag)
            {
                case "ViewerTool":
                    FocusPDFViewTool = viewerTool;
                    splitToolManager.ClearSelect();
                    break;
                case "SplitViewerTool":
                    FocusPDFViewTool = splitViewerTool;
                    toolManager.ClearSelect();
                    break;
            }
            if (FocusPDFViewTool.IsLoaded && FocusPDFViewTool.GetCPDFViewer()?.CurrentRenderFrame != null)
            {
                FocusPDFViewToolChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetCursorStatus()
        {
            List<ToolType> toolTypes = new List<ToolType>
                {
                      ToolType.Viewer,
                      ToolType.Pan
                };

            ToolType currentMode = PDFViewTool.GetToolType();
            Cursor newCursor = Cursors.Arrow;

            if (toolTypes.Contains(currentMode))
            {
                BaseAnnot hitAnnot = GetCPDFViewer().AnnotHitTest();
                if (hitAnnot != null)
                {
                    if (hitAnnot is LinkAnnot || hitAnnot is BaseWidget)
                    {
                        newCursor = Cursors.Hand;
                    }
                    else
                    {
                        newCursor = annotEditCursor;
                    }
                }
                else
                {
                    if (!PDFViewTool.IsSelectRectMousePoint())
                    {
                        newCursor = (PDFToolManager.GetToolType() == ToolType.Pan) ? panToolCursor : Cursors.Arrow;

                        if (PDFViewTool.IsText())
                        {
                            newCursor = Cursors.IBeam;
                        }
                    }
                }

                if (currentMode == ToolType.CreateAnnot)
                {
                    List<C_ANNOTATION_TYPE> annotTypes = new List<C_ANNOTATION_TYPE>
                    {
                         C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT,
                         C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE,
                         C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY,
                         C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT
                    };

                    if (GetCPDFViewer().AnnotHitTest() == null && PDFViewTool.IsText() && annotTypes.Contains(toolManager.GetAnnotType()))
                    {
                        newCursor = Cursors.IBeam;
                    }
                    else
                    {
                        if (newCursor == Cursors.IBeam || newCursor == panToolCursor || newCursor == Cursors.Arrow)
                        {
                            newCursor = annotEditCursor;
                        }
                    }
                }
            }

            GetCPDFViewer().Cursor = newCursor;
        }

        private void PDFViewControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (GetCPDFViewer().IsRendering)
            {
                GetCPDFViewer().Cursor = Cursors.Wait;
                FocusPDFViewTool.ContextMenu?.Items.Clear();
                return;
            }

            if (isAutomaticScroll)
            {
                middleMovePoint = e.GetPosition(this);
                e.Handled = true;
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && IsHitEmpty)
            {
                Point currentPoint = e.GetPosition(this);
                Vector offset = currentPoint - hitEmptyPos;
                if (Math.Abs(offset.X) > 10)
                {
                    GetCPDFViewer()?.SetHorizontalOffset(GetCPDFViewer().HorizontalOffset - offset.X);
                    hitEmptyPos.X = currentPoint.X;
                }

                if (Math.Abs(offset.Y) > 10)
                {
                    GetCPDFViewer()?.SetVerticalOffset(GetCPDFViewer().VerticalOffset - offset.Y);
                    hitEmptyPos.Y = currentPoint.Y;
                }

                GetCPDFViewer().Cursor = panTool2Cursor;
                e.Handled = true;
                return;
            }

            if (e.LeftButton == MouseButtonState.Released && viewerTool != null)
            {
                List<ToolType> toolTypes = new List<ToolType>
                {
                      ToolType.Viewer,
                      ToolType.Pan
                };
                ToolType currentMode = PDFViewTool.GetToolType();
                bool cursorSet = false;
                Cursor newCursor = GetCPDFViewer().Cursor;

                if (toolTypes.Contains(currentMode))
                {
                    BaseAnnot hitAnnot = GetCPDFViewer().AnnotHitTest();

                    if (hitAnnot != null)
                    {
                        if (hitAnnot is LinkAnnot || hitAnnot is BaseWidget)
                        {
                            newCursor = Cursors.Hand;
                            cursorSet = true;
                        }
                        else
                        {
                            newCursor = annotEditCursor;
                            cursorSet = true;
                        }
                    }
                    else
                    {
                        if (!PDFViewTool.IsSelectRectMousePoint())
                        {
                            newCursor = (PDFToolManager.GetToolType() == ToolType.Pan) ? panToolCursor : Cursors.Arrow;

                            if (PDFViewTool.IsText())
                            {
                                newCursor = Cursors.IBeam;
                            }
                            cursorSet = true;
                        }
                    }

                    if (cursorSet)
                    {
                        e.Handled = true;
                    }
                }

                if (currentMode == ToolType.CreateAnnot)
                {
                    List<C_ANNOTATION_TYPE> annotTypes = new List<C_ANNOTATION_TYPE>
                    {
                         C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT,
                         C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE,
                         C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY,
                         C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT
                    };

                    if (GetCPDFViewer().AnnotHitTest() == null && PDFViewTool.IsText() && annotTypes.Contains(toolManager.GetAnnotType()))
                    {
                        newCursor = Cursors.IBeam;
                    }
                    else
                    {
                        if (newCursor == Cursors.IBeam || newCursor == panToolCursor || newCursor == Cursors.Arrow)
                        {
                            newCursor = annotEditCursor;
                        }
                    }
                }

                if (newCursor != GetCPDFViewer().Cursor)
                {
                    GetCPDFViewer().Cursor = newCursor;
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                isAutomaticScroll ^= true;
            }
            else
            {
                isAutomaticScroll = false;
            }

            if (isAutomaticScroll)
            {
                GetCPDFViewer().Cursor = Cursors.ScrollAll;
                middlePressPoint = e.GetPosition(this);
                middleMovePoint = middlePressPoint;

                if (MiddleScrollTask != null && MiddleScrollTask.Status == TaskStatus.Running)
                {
                    cancelTask = true;
                    MiddleScrollTask.Wait();
                }

                cancelTask = false;
                MiddleScrollTask = Task.Factory.StartNew(() => DoScrollWork());
            }
        }

        private void DoScrollWork()
        {
            try
            {
                while (!cancelTask && isAutomaticScroll)
                {
                    Vector subSize = middleMovePoint - middlePressPoint;
                    Dispatcher.Invoke(() =>
                    {
                        GetCPDFViewer().SetVerticalOffset(GetCPDFViewer().VerticalOffset + subSize.Y);
                        GetCPDFViewer().SetHorizontalOffset(GetCPDFViewer().HorizontalOffset + subSize.X);
                    });
                    Thread.Sleep(100);
                }
                Dispatcher.Invoke(() =>
                {
                    if (GetCPDFViewer().Cursor == Cursors.ScrollAll)
                    {
                        GetCPDFViewer().Cursor = Cursors.Arrow;
                    }
                });
            }
            catch (Exception ex)
            {
                MiddleScrollTask = null;
            }
        }

        #endregion

        #region Hand
        private Cursor panToolCursor;
        private Cursor panTool2Cursor;
        private Cursor annotEditCursor;

        private Cursor LoadCursor(string cursorResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(cursorResource);
            return new Cursor(stream);
        }

        private void SetCursor()
        {
            SetPanToolCursor(LoadCursor("ComPDFKit.Controls.Asset.Cursor.PanTool.cur"));
            SetPanTool2Cursor(LoadCursor("ComPDFKit.Controls.Asset.Cursor.PanTool2.cur"));
            SetAnnotEditCursor(LoadCursor("ComPDFKit.Controls.Asset.Cursor.AnnotEdit.cur"));
        }

        public void SetPanToolCursor(Cursor cursor)
        {
            panToolCursor = cursor;
        }

        public void SetPanTool2Cursor(Cursor cursor)
        {
            panTool2Cursor = cursor;
        }

        public void SetAnnotEditCursor(Cursor cursor)
        {
            annotEditCursor = cursor;
        }
        #endregion
    }
}
