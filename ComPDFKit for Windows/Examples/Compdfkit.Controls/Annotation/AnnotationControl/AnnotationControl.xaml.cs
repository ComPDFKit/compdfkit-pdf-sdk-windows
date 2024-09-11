using System.Windows.Controls;
using ComPDFKit.Controls.Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.PDFPage;
using ComPDFKitViewer.Widget;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.DrawTool;
using System.Collections.Generic;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class AnnotationControl : UserControl, INotifyPropertyChanged
    {
        #region Property
        private bool isFirstLoad = true;
        public PDFViewControl PDFViewControl;
        public CPDFAnnotationControl PDFAnnotationControl = null;
        private CPDFDisplaySettingsControl displaySettingsControl = null;
        public FromPropertyControl FromPropertyControl = new FromPropertyControl();

        private PanelState panelState = PanelState.GetInstance();

        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand CloseTabCommand;
        public ICommand ExpandPropertyPanelCommand;

        public bool CanUndo
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    return PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo;
                }
                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    return PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo;
                }

                return false;
            }
        }

        private bool CanSave
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    if (PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo ||
                        PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;

        #endregion

        public AnnotationControl()
        {
            InitializeComponent();
            DataContext = this;
            PDFAnnotationControl = new CPDFAnnotationControl();
            CPDFAnnotationType[] annotationProperties =
            {
                    CPDFAnnotationType.Highlight, CPDFAnnotationType.Underline, CPDFAnnotationType.Strikeout,
                    CPDFAnnotationType.Squiggly, CPDFAnnotationType.Freehand, CPDFAnnotationType.FreeText,
                    CPDFAnnotationType.Note, CPDFAnnotationType.Circle, CPDFAnnotationType.Square,
                    CPDFAnnotationType.Arrow, CPDFAnnotationType.Line, CPDFAnnotationType.Image,
                    CPDFAnnotationType.Stamp, CPDFAnnotationType.Signature, CPDFAnnotationType.Link,
                    CPDFAnnotationType.Audio
            };
            AnnotationBarControl.InitAnnotationBar(annotationProperties);

            UndoBtn.ToolTip = LanguageHelper.CommonManager.GetString("Tooltip_Undo");
            RedoBtn.ToolTip = LanguageHelper.CommonManager.GetString("Tooltip_Redo");
        }

        #region Init PDFViewer
        public void InitWithPDFViewer(PDFViewControl pdfViewer)
        {
            PDFViewControl = pdfViewer;
            PDFGrid.Child = PDFViewControl;
            FloatPageTool.InitWithPDFViewer(PDFViewControl);
            InitialPDFViewControl(PDFViewControl);
        }

        #endregion

        #region Public Method

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
            PDFViewControl?.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_NONE);
        }

        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
        }

        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

        public void SetPropertyContainer(UIElement uiElement)
        {
            PropertyContainer.Child = uiElement;
        }

        public void ClearAllToolState()
        {
            this.AnnotationBarControl.ClearAllToolState();
        }

        public void SetToolBarContainerVisibility(Visibility visibility)
        {
            this.ToolBarContainer.Visibility = visibility;
        }

        #endregion

        #region Load Unload custom control

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PDFViewControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            PDFViewControl.MouseRightButtonDownHandler += PDFViewControl_MouseRightButtonDownHandler;

            InitialPDFViewControl(PDFViewControl);
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            if (e.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                BaseWidget baseWidget = PDFViewControl.GetCacheHitTestWidget();
                if (baseWidget != null)
                {
                    AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
PDFViewControl.PDFViewTool.GetCPDFViewer().GetDocument(),
baseWidget.GetAnnotData().PageIndex,
baseWidget.GetAnnotData().Annot);
                    if ((annotParam as WidgetParm).WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                    {
                        CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                        signatureProperty.SetFormProperty(annotParam, PDFViewControl, baseWidget.GetAnnotData().Annot);
                        PropertyContainer.Child = signatureProperty;
                    } 
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PDFViewControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
        }


        private void AnnotationBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationBarControl.AnnotationPropertyChanged -= AnnotationBarControl_AnnotationPropertyChanged;
            AnnotationBarControl.AnnotationCancel -= AnnotationBarControl_AnnotationCancel;

            AnnotationBarControl.AnnotationPropertyChanged += AnnotationBarControl_AnnotationPropertyChanged;
            AnnotationBarControl.AnnotationCancel += AnnotationBarControl_AnnotationCancel;
        }

        private void AnnotationBarControl_Unloaded(object sender, RoutedEventArgs e)
        {
            AnnotationBarControl.AnnotationPropertyChanged -= AnnotationBarControl_AnnotationPropertyChanged;
            AnnotationBarControl.AnnotationCancel -= AnnotationBarControl_AnnotationCancel;

        }

        #endregion

        #region Annotation

        public void InitialPDFViewControl(PDFViewControl newPDFViewer)
        {
            PDFAnnotationControl.SetPDFViewer(newPDFViewer);
            PDFAnnotationControl.AnnotationCancel();
            AnnotationBarControl.ClearAllToolState();
            ExpandRightPropertyPanel(null, Visibility.Collapsed);
            PDFAnnotationControl.ClearAnnotationBar -= PdfAnnotationControl_ClearAnnotationBar;
            PDFAnnotationControl.ClearAnnotationBar += PdfAnnotationControl_ClearAnnotationBar;
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
            PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            PDFAnnotationControl.ClearAnnotationBar -= PdfAnnotationControl_ClearAnnotationBar;
            PDFViewControl.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            PDFViewControl.PDFViewTool.AnnotChanged -= PDFViewTool_AnnotChanged;
            PDFViewControl.PDFViewTool.AnnotChanged += PDFViewTool_AnnotChanged;
        }

        private void PDFViewTool_AnnotChanged(object sender, object e)
        { 
                OnAnnotEditHandler?.Invoke(this, EventArgs.Empty); 
        }

        private void PDFToolManager_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (e.IsCreate)
            {
                OnAnnotEditHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PDFViewControl_MouseRightButtonDownHandler(object sender, ComPDFKit.Tool.MouseEventObject e)
        {
            ContextMenu ContextMenu = PDFViewControl.GetRightMenu();
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            switch (e.hitTestType)
            {
                case MouseHitTestType.Annot:
                case MouseHitTestType.SelectRect:
                    CreateAnnotContextMenu(sender, ref ContextMenu, e.annotType);
                    break;
                case MouseHitTestType.Text:
                    CreateSelectTextContextMenu(sender, ref ContextMenu);
                    break;
                case MouseHitTestType.ImageSelect:
                    CreateSelectImageContextMenu(sender, ref ContextMenu);
                    break;
                default:
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    PDFViewControl.CreateViewerMenu(sender, ref ContextMenu);
                    break;
            }
            PDFViewControl.SetRightMenu(ContextMenu);
        }

        private void CreateSelectTextContextMenu(object sender, ref ContextMenu menu)
        {
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
        }

        private void CreateAnnotContextMenu(object sender, ref ContextMenu menu, C_ANNOTATION_TYPE annotType)
        {
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Play"), Command = MediaCommands.Play, CommandTarget = (UIElement)sender, CommandParameter = (sender as CPDFViewerTool).GetCacheHitTestAnnot() });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    break;
                default:
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    break;
            }
        }

        private void CreateSelectImageContextMenu(object sender, ref ContextMenu menu)
        {
            if (menu == null)
            {
                menu = new ContextMenu();
            }
            MenuItem copyImage = new MenuItem();
            copyImage.Header = "Copy Image";
            copyImage.Click += CopyImage_Click;
            menu.Items.Add(copyImage);

            MenuItem extractImage = new MenuItem();
            extractImage.Header = "Extract Image";
            extractImage.Click += ExtractImage_Click;
            menu.Items.Add(extractImage);
        }

        private void ExtractImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PageImageItem image = null;
                Dictionary<int, List<PageImageItem>> pageImageDict = PDFViewControl.FocusPDFViewTool.GetSelectImageItems();
                if (pageImageDict != null && pageImageDict.Count > 0)
                {
                    foreach (int pageIndex in pageImageDict.Keys)
                    {
                        List<PageImageItem> imageItemList = pageImageDict[pageIndex];
                        image = imageItemList[0];
                        break;
                    }
                }

                if (image == null)
                {
                    return;
                }

                CPDFPage page = PDFViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
                string savePath = Path.Combine(folderDialog.SelectedPath, Guid.NewGuid() + ".jpg");
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
                page.GetImgSelection().GetImgBitmap(image.ImageIndex, tempPath);

                Bitmap bitmap = new Bitmap(tempPath);
                bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                Process.Start("explorer", "/select,\"" + savePath + "\"");
            }
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            PageImageItem image = null;
            Dictionary<int, List<PageImageItem>> pageImageDict = PDFViewControl.FocusPDFViewTool.GetSelectImageItems();
            if (pageImageDict != null && pageImageDict.Count > 0)
            {
                foreach (int pageIndex in pageImageDict.Keys)
                {
                    List<PageImageItem> imageItemList = pageImageDict[pageIndex];
                    image = imageItemList[0];
                    break;
                }
            }

            if (image == null)
            {
                return;
            }

            CPDFPage page = PDFViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
            page.GetImgSelection().GetImgBitmap(image.ImageIndex, tempPath);

            Bitmap bitmap = new Bitmap(tempPath);
            BitmapImage imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageData = new BitmapImage();
                imageData.BeginInit();
                imageData.StreamSource = ms;

                imageData.CacheOption = BitmapCacheOption.OnLoad;
                imageData.EndInit();
                imageData.Freeze();
                Clipboard.SetImage(imageData);
                bitmap.Dispose();
                File.Delete(tempPath);
            }
        }

        public void UnloadEvent()
        {
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            PDFAnnotationControl.ClearAnnotationBar -= PdfAnnotationControl_ClearAnnotationBar;
            PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PDFViewControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            PDFAnnotationControl.UnLoadPDFViewHandler();
        }

        private void PdfAnnotationControl_ClearAnnotationBar(object sender, EventArgs e)
        {
            AnnotationBarControl.ClearAllToolState();
        }

        public void SetViewSettings(Visibility visibility, CPDFDisplaySettingsControl displaySettingsControl = null)
        {
            this.PropertyContainer.Child = displaySettingsControl;
            this.PropertyContainer.Visibility = visibility;
        }
        #endregion

        #region Expand and collapse Panel

        public void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        public void ExpandLeftPanel(bool isExpand)
        {
            BotaContainer.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            Splitter.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            if (isExpand)
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(320);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(15);
            }
            else
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(0);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }

        #endregion

        #region UI

        private double CheckZoomLevel(double zoom, bool IsGrowth)
        {
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

        private void ToolExpand_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton expandBtn = sender as ToggleButton;
            if (expandBtn != null)
            {
                bool isExpand = expandBtn.IsChecked == true;
                ExpandLeftPanel(isExpand);
            }
        }

        private void EditLink_Click(object sender, RoutedEventArgs e)
        {
            PropertyContainer.Visibility = Visibility.Visible;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
                PDFViewControl.PDFToolManager.ClearSelect();
                PDFViewControl.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                (BotaContainer.Child as CPDFBOTABarControl).LoadAnnotationList();

            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
                PDFViewControl.PDFToolManager.ClearSelect();
                PDFViewControl.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                (BotaContainer.Child as CPDFBOTABarControl).LoadAnnotationList();
            }
        }

        #endregion

        #region Property changed
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.IsLeftPanelExpand))
            {
                ExpandLeftPanel(panelState.IsLeftPanelExpand);
            }
            else if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
                {
                    ExpandRightPropertyPanel(PDFAnnotationControl, Visibility.Visible);
                }
                else if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {
                    ExpandRightPropertyPanel(displaySettingsControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);
                }
            }
        }
        #endregion

        #region Event handle
        private void AnnotationBarControl_AnnotationCancel(object sender, EventArgs e)
        {
            PDFAnnotationControl.AnnotationCancel();
            if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
            {
                panelState.RightPanel = PanelState.RightPanelState.None;
            }
        }

        private void AnnotationBarControl_AnnotationPropertyChanged(object sender, CPDFAnnotationType e)
        {
            PDFAnnotationControl.LoadAnnotationPanel(e);
            if (e != CPDFAnnotationType.Audio && e != CPDFAnnotationType.Image)
            {
                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            }

            if (e == CPDFAnnotationType.Link)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().SetLinkHighlight(true);
            }
        }

        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanUndo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
            }
        }

        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanRedo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
            }
        }

        private void CommandBinding_Executed_Highlight(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Highlight);
        }

        private void CommandBinding_Executed_Underline(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Underline);
        }

        private void CommandBinding_Executed_Strikeout(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Strikeout);
        }

        private void CommandBinding_Executed_Squiggly(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Squiggly);
        }
        #endregion

    }
}