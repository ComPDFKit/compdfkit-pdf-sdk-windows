using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ComPDFKit.PDFDocument;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using ComPDFKitViewer.PdfViewer; 

namespace DocsEditor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        private bool isFirstLoad = true;
        private string currentMode = "Page Edit";

        private PDFViewControl passwordViewer;
        private PDFViewControl pdfViewControl;
        private CPDFAnnotationControl pdfAnnotationControl = null;
        private CPDFSearchControl searchControl = null;
        private CPDFPageEditControl pageEditControl = null;
        private CPDFDisplaySettingsControl displayPanel = new CPDFDisplaySettingsControl();
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };

        public bool CanSave
        {
            get
            {
                if (pdfViewControl != null && pdfViewControl.PDFView != null)
                {
                    return pdfViewControl.PDFView.UndoManager.CanSave;
                }
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }
        
        #region Load document

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Sample_File_Windows.pdf";
            LeftToolPanelButton.IsEnabled = false;
            SearchButton.IsEnabled = false;
            RightPanelButton.IsEnabled = false;
            ViewSettingBtn.IsEnabled = false;
            pdfViewControl = new PDFViewControl();
            pdfViewControl.PDFView.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            if (pdfViewControl.PDFView.Document == null)
            {
                return;
            }
            pdfViewControl.PDFView?.SetMouseMode(MouseModes.Viewer);
            pdfViewControl.PDFView?.Load();
            pdfViewControl.PDFView?.SetShowLink(true);
            if (ViewComboBox.SelectedIndex == 0)
            {
                PDFGrid.Child = pdfViewControl;
            }
            else
            {
                ToolBarContainer.Visibility = Visibility.Visible;
                if (pageEditControl == null)
                {
                    pageEditControl = new CPDFPageEditControl();
                }
                pageEditControl.ExitPageEdit -= PageEditControl_ExitPageEdit;
                pageEditControl.ExitPageEdit += PageEditControl_ExitPageEdit;

                pageEditControl.PageMoved -= PageEditControl_PageMoved;
                pageEditControl.PageMoved += PageEditControl_PageMoved;

                CPDFPageEditBarControl.PageEditEvent -= CPDFPageEditBarControl_PageEditEvent;
                CPDFPageEditBarControl.PageEditEvent += CPDFPageEditBarControl_PageEditEvent;
                pageEditControl.LoadThumbnails(pdfViewControl.PDFView);
                PDFGrid.Child = pageEditControl;
                FloatPageTool.Visibility = Visibility.Collapsed;
            }
            pdfViewControl.PDFView.InfoChanged -= PdfViewer_InfoChanged;
            pdfViewControl.PDFView.InfoChanged += PdfViewer_InfoChanged;
            pdfViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            pdfViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
            pdfViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            pdfViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            pdfViewControl.PDFView.SetFormFieldHighlight(true);
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;

            pdfViewControl.PDFView.ChangeFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(pdfViewControl.PDFView);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewControl.PDFView.ZoomFactor * 100)));

            ViewSettingBtn.IsChecked = false;
            PropertyContainer.Child = null;
            PropertyContainer.Visibility = Visibility.Collapsed;
            FloatPageTool.InitWithPDFViewer(pdfViewControl.PDFView);
            BotaSideTool.InitWithPDFViewer(pdfViewControl.PDFView);
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
        }

        #endregion

        #region Context menu

        private void PDFView_AnnotCommandHandler(object sender, AnnotCommandArgs e)
        {
            if (e != null && e.CommandType == CommandType.Context)
            {
                if (e.PressOnSelectedText)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                }
                else if (e.CommandTarget == TargetType.ImageSelection)
                {
                    if (pdfViewControl != null && pdfViewControl.PDFView != null && pdfViewControl.PDFView.GetSelectImageCount() > 0)
                    {
                        e.Handle = true;
                        e.PopupMenu = new ContextMenu();

                        MenuItem imageCopyMenu = new MenuItem();
                        imageCopyMenu = new MenuItem();
                        imageCopyMenu.Header = "Copy Images";
                        WeakEventManager<MenuItem, RoutedEventArgs>.AddHandler(imageCopyMenu, "Click", CopyImage_Click);
                        imageCopyMenu.CommandParameter = e;
                        e.PopupMenu.Items.Add(imageCopyMenu);

                        MenuItem imageExtraMenu = new MenuItem();
                        imageExtraMenu = new MenuItem();
                        imageExtraMenu.Header = "Extract Images";
                        WeakEventManager<MenuItem, RoutedEventArgs>.AddHandler(imageExtraMenu, "Click", ExtraImage_Click);
                        imageExtraMenu.CommandParameter = e;
                        e.PopupMenu.Items.Add(imageExtraMenu);
                    }
                }
                else if (e.CommandTarget == TargetType.WidgetView)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                }
                else
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();

                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new Separator());

                    MenuItem fitWidthMenu = new MenuItem();
                    fitWidthMenu.Header = LanguageHelper.CommonManager.GetString("Menu_AutoSize");
                    fitWidthMenu.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeFitMode(FitMode.FitWidth);
                        }
                    };

                    e.PopupMenu.Items.Add(fitWidthMenu);

                    MenuItem fitSizeMenu = new MenuItem();
                    fitSizeMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RealSize");
                    fitSizeMenu.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeFitMode(FitMode.FitSize);
                        }
                    };

                    e.PopupMenu.Items.Add(fitSizeMenu);

                    MenuItem zoomInMenu = new MenuItem();
                    zoomInMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomIn");
                    zoomInMenu.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor + 0.01, true);
                            pdfViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomInMenu);

                    MenuItem zoomOutMenu = new MenuItem();
                    zoomOutMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomOut");
                    zoomOutMenu.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor - 0.01, false);
                            pdfViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomOutMenu);
                    e.PopupMenu.Items.Add(new Separator());

                    MenuItem singleView = new MenuItem();
                    singleView.Header = LanguageHelper.CommonManager.GetString("Menu_SinglePage");
                    singleView.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeViewMode(ViewMode.Single);
                        }
                    };

                    e.PopupMenu.Items.Add(singleView);

                    MenuItem singleContinuousView = new MenuItem();
                    singleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_SingleContinuous");
                    singleContinuousView.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeViewMode(ViewMode.SingleContinuous);
                        }
                    };

                    e.PopupMenu.Items.Add(singleContinuousView);

                    MenuItem doubleView = new MenuItem();
                    doubleView.Header = LanguageHelper.CommonManager.GetString("Menu_DoublePage");
                    doubleView.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeViewMode(ViewMode.Double);
                        }
                    };

                    e.PopupMenu.Items.Add(doubleView);

                    MenuItem doubleContinuousView = new MenuItem();
                    doubleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_DoubleContinuous");
                    doubleContinuousView.Click += (o, p) =>
                    {
                        if (pdfViewControl != null)
                        {
                            pdfViewControl.PDFView?.ChangeViewMode(ViewMode.DoubleContinuous);
                        }
                    };
                    e.PopupMenu.Items.Add(doubleContinuousView);
                }
            }
            else
            {
                e.DoCommand();
            }
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<int, List<Bitmap>> imageDict = pdfViewControl.PDFView?.GetSelectedImages();

            if (imageDict != null && imageDict.Count > 0)
            {
                foreach (int pageIndex in imageDict.Keys)
                {
                    List<Bitmap> imageList = imageDict[pageIndex];
                    foreach (Bitmap image in imageList)
                    {
                        MemoryStream ms = new MemoryStream();
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        BitmapImage imageData = new BitmapImage();
                        imageData.BeginInit();
                        imageData.StreamSource = ms;
                        imageData.CacheOption = BitmapCacheOption.OnLoad;
                        imageData.EndInit();
                        imageData.Freeze();
                        Clipboard.SetImage(imageData);
                        break;
                    }
                }
            }
        }

        private void ExtraImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string choosePath = folderDialog.SelectedPath;
                string openPath = choosePath;
                Dictionary<int, List<Bitmap>> imageDict = pdfViewControl.PDFView?.GetSelectedImages();

                if (imageDict != null && imageDict.Count > 0)
                {
                    foreach (int pageIndex in imageDict.Keys)
                    {
                        List<Bitmap> imageList = imageDict[pageIndex];
                        foreach (Bitmap image in imageList)
                        {
                            string savePath = System.IO.Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                            image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            openPath = savePath;
                        }
                    }
                }
                Process.Start("explorer", "/select,\"" + openPath + "\"");

            }
        }

        #endregion

        #region password
        private void PasswordUI_Canceled(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        private void PasswordUI_Closed(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFView != null && passwordViewer.PDFView.Document != null)
            {
                passwordViewer.PDFView.Document.UnlockWithPassword(e);
                if (passwordViewer.PDFView.Document.IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    pdfViewControl.PDFView.Document.Release();
                    pdfViewControl = passwordViewer;
                    LoadDocument();
                }
                else
                {
                    PasswordUI.SetShowError("error", Visibility.Visible);
                }
            }
        }

        #endregion

        #region Expand and collapse Panel

        public void ExpandRightPropertyPanel(UIElement properytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = properytPanel;
            PropertyContainer.Visibility = visible;
            if (visible == Visibility.Hidden || visible == Visibility.Collapsed)
            {
                RightPanelButton.IsChecked = false;
            }
        }

        private void ExpandLeftPanel(bool isExpand)
        {
            BotaSideTool.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
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
        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewControl.PDFView);
            PopupBorder.Visibility = Visibility.Visible;
        }

        private void CPDFInfoControl_CloseInfoEvent(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }


        private void OpenFile()
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (!string.IsNullOrEmpty(filePath) && pdfViewControl != null)
            {
                if (pdfViewControl.PDFView != null && pdfViewControl.PDFView.Document != null)
                {
                    string oldFilePath = pdfViewControl.PDFView.Document.FilePath;
                    if (oldFilePath.ToLower() == filePath.ToLower())
                    {
                        return;
                    }
                    else
                    {
                        pdfViewControl.PDFView.Document.Release();
                    }
                }

                passwordViewer = new PDFViewControl();
                passwordViewer.PDFView.InitDocument(filePath);
                if (passwordViewer.PDFView.Document == null)
                {
                    MessageBox.Show("Open File Failed");
                    return;
                }

                if (passwordViewer.PDFView.Document.IsLocked)
                {
                    PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                    PasswordUI.ClearPassword();
                    PopupBorder.Visibility = Visibility.Visible;
                    PasswordUI.Visibility = Visibility.Visible;
                }
                else
                {
                    pdfViewControl.PDFView.Document.Release();
                    pdfViewControl = passwordViewer;
                    LoadDocument();
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void ControlLeftPanel()
        {
            if (LeftToolPanelButton != null)
            {
                bool isExpand = LeftToolPanelButton.IsChecked == true;
                ExpandLeftPanel(isExpand);
            }
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ControlLeftPanel();
        }

        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            pdfViewControl.PDFView.UndoManager.CanSave = false;
        }

        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }


        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            LeftToolPanelButton.IsChecked = true;
            BotaSideTool.SelectBotaTool(BOTATools.Search);
        }

        private void ControlRightPanel()
        {
            if ((bool)ViewSettingBtn.IsChecked)
            {
                ViewSettingBtn.IsChecked = false;
            }
            if (RightPanelButton != null)
            {
                if (RightPanelButton.IsChecked == true)
                {
                    ExpandRightPropertyPanel(pdfAnnotationControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);
                }
            }
        }

        private void RightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ControlRightPanel();
        }



        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowViewSettings();
        }

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


        #endregion

        #region Save file

        /// <summary>
        /// Save the file to another PDF file.
        /// </summary>
        public void SaveAsFile()
        {
            {
                if (pdfViewControl != null && pdfViewControl.PDFView != null && pdfViewControl.PDFView.Document != null)
                {
                    CPDFDocument pdfDoc = pdfViewControl.PDFView.Document;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "(*.pdf)|*.pdf";
                    saveDialog.DefaultExt = ".pdf";
                    saveDialog.OverwritePrompt = true;

                    if (saveDialog.ShowDialog() == true)
                    {
                        pdfDoc.WriteToFilePath(saveDialog.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Save the file in the current path.
        /// </summary>
        private void SaveFile()
        {
            if (pdfViewControl != null && pdfViewControl.PDFView != null && pdfViewControl.PDFView.Document != null)
            {
                CPDFDocument pdfDoc = pdfViewControl.PDFView.Document;
                if (pdfDoc.WriteToLoadedPath())
                {
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "(*.pdf)|*.pdf";
                saveDialog.DefaultExt = ".pdf";
                saveDialog.OverwritePrompt = true;

                if (saveDialog.ShowDialog() == true)
                {
                    pdfDoc.WriteToFilePath(saveDialog.FileName);
                }
            }
        }

        #endregion

        #region Selection changed

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFirstLoad)
            {
                isFirstLoad = false;
                return;
            }

            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            switch (currentMode)
            {
                case "Viewer":
                    break;
                case "Page Edit":
                    ToolBarContainer.Visibility = Visibility.Collapsed;
                    pageEditControl.ExitPageEdit -= PageEditControl_ExitPageEdit;
                    pageEditControl.PageMoved -= PageEditControl_PageMoved;
                    CPDFPageEditBarControl.PageEditEvent -= CPDFPageEditBarControl_PageEditEvent;
                    FloatPageTool.Visibility = Visibility.Visible;
                    pdfViewControl.PDFView.ReloadDocument();
                    LeftToolPanelButton.IsEnabled = true;
                    SearchButton.IsEnabled = true;
                    RightPanelButton.IsEnabled = true;
                    ViewSettingBtn.IsEnabled = true;
                    break;
                default:
                    break;
            }
            if ((string)item.Content == "Viewer")
            {
                ToolBarContainer.Visibility = Visibility.Collapsed;
                PDFGrid.Child = pdfViewControl;
            }
            else if ((string)item.Content == "Page Edit")
            {
                ToolBarContainer.Visibility = Visibility.Visible;
                if (pageEditControl == null)
                {
                    pageEditControl = new CPDFPageEditControl();
                }
                pageEditControl.ExitPageEdit += PageEditControl_ExitPageEdit;
                pageEditControl.PageMoved += PageEditControl_PageMoved;
                CPDFPageEditBarControl.PageEditEvent += CPDFPageEditBarControl_PageEditEvent;
                pageEditControl.LoadThumbnails(pdfViewControl.PDFView);
                PDFGrid.Child = pageEditControl;
                LeftToolPanelButton.IsChecked = false;
                LeftToolPanelButton.IsEnabled = false;
                SearchButton.IsEnabled = false;
                RightPanelButton.IsChecked = false;
                RightPanelButton.IsEnabled = false;
                ViewSettingBtn.IsChecked = false;
                ViewSettingBtn.IsEnabled = false;
                ExpandLeftPanel(false);
                ExpandRightPropertyPanel(null, Visibility.Collapsed);
                FloatPageTool.Visibility = Visibility.Collapsed;
            }
            currentMode = (string)item.Content;
        }

        #endregion

        #region Event handle
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BotaSideTool.AddBOTAContent(new []{BOTATools.Thumbnail , BOTATools.Outline , BOTATools.Bookmark , BOTATools.Search});
            pdfAnnotationControl = new CPDFAnnotationControl();
            LoadDefaultDocument();
        }
        
        private void CPDFPageEditBarControl_PageEditEvent(object sender, string e)
        {
            pageEditControl.PageEdit(e);
        }

        private void PageEditControl_ExitPageEdit(object sender, EventArgs e)
        {
            ViewComboBox.SelectedIndex = 0;
        }

        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        private void PageEditControl_PageMoved(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl == null || pdfViewControl.PDFView == null)
            {
                return;
            }
            BotaSideTool.InitWithPDFViewer(pdfViewControl.PDFView);
        }

        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        {
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }

        #endregion

        #region Property changed

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Shortcut
        private void CommandBinding_Executed_Open(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }

        private void CommandBinding_Executed_Save(object sender, ExecutedRoutedEventArgs e)
        {
            if (CanSave)
            {
                SaveFile();
            }
        }

        private void CommandBinding_Executed_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsFile();
        }

        private void CommandBinding_Executed_ControlLeftPanel(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                LeftToolPanelButton.IsChecked = !LeftToolPanelButton.IsChecked;
                ControlLeftPanel();
            }
        }

        private void CommandBinding_Executed_ControlRightPanel(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                RightPanelButton.IsChecked = !RightPanelButton.IsChecked;
                ControlRightPanel();
            }
        }

        private void CommandBinding_Executed_Bookmark(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                ExpandLeftPanel(true);
                LeftToolPanelButton.IsChecked = true;
                BotaSideTool.SelectBotaTool(BOTATools.Bookmark);
            }
        }

        private void CommandBinding_Executed_Outline(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                ExpandLeftPanel(true);
                LeftToolPanelButton.IsChecked = true;
                BotaSideTool.SelectBotaTool(BOTATools.Outline);
            }
        }

        private void CommandBinding_Executed_Thumbnail(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                ExpandLeftPanel(true);
                LeftToolPanelButton.IsChecked = true;
                BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
            }
        }

        private void CommandBinding_Executed_Search(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                ExpandLeftPanel(true);
                LeftToolPanelButton.IsChecked = true;
                BotaSideTool.SelectBotaTool(BOTATools.Search);
            }
        }

        private void CommandBinding_Executed_ScaleAdd(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor + 0.01, true);
            pdfViewControl.PDFView?.Zoom(newZoom);
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor - 0.01, false);
            pdfViewControl.PDFView?.Zoom(newZoom);
        }

        private void ShowViewSettings()
        {
            if (ViewSettingBtn != null)
            {
                if (ViewSettingBtn.IsChecked == true)
                {
                    displayPanel = new CPDFDisplaySettingsControl();
                    displayPanel.InitWithPDFViewer(pdfViewControl.PDFView);
                    PropertyContainer.Child = displayPanel;
                    PropertyContainer.Visibility = Visibility.Visible;
                    if ((bool)RightPanelButton.IsChecked)
                    {
                        RightPanelButton.IsChecked = false;
                    }
                }
                else
                {
                    PropertyContainer.Child = null;
                    PropertyContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CommandBinding_Executed_DisplaySettings(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentMode != "Page Edit")
            {
                ViewSettingBtn.IsChecked = !ViewSettingBtn.IsChecked;
                ShowViewSettings();
            }
        }

        private void CommandBinding_Executed_DocumentInfo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PopupBorder.Visibility != Visibility.Visible)
            {
                PasswordUI.Visibility = Visibility.Collapsed;
                FileInfoUI.Visibility = Visibility.Visible;
                FileInfoControl.InitWithPDFViewer(pdfViewControl.PDFView);
                PopupBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FileInfoUI.Visibility = Visibility.Collapsed;
                PopupBorder.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
