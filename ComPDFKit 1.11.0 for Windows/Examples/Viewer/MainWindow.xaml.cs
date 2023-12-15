using ComPDFKit.PDFDocument;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
namespace Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        private PDFViewControl passwordViewer;
        private PDFViewControl pdfViewControl;
        CPDFDisplaySettingsControl displayPanel = new CPDFDisplaySettingsControl();

        private int[] zoomLevelList = { 10, 25, 50, 100, 150, 200, 300, 400, 500, 1000 };

        public event PropertyChangedEventHandler PropertyChanged;
        private KeyEventHandler KeyDownHandler;

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
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BotaSideTool.AddBOTAContent(new []{BOTATools.Thumbnail , BOTATools.Outline , BOTATools.Bookmark , BOTATools.Search} );
            LoadDefaultDocument();
        }

        #region Load Document
        private void LoadDocument()
        {
            if (pdfViewControl.PDFView.Document == null)
            {
                return;
            }
            pdfViewControl.PDFView?.SetMouseMode(MouseModes.Viewer);
            pdfViewControl.PDFView?.Load();
            pdfViewControl.PDFView?.SetShowLink(true);
            PDFGrid.Child = pdfViewControl;
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

            FloatPageTool.InitWithPDFViewer(pdfViewControl.PDFView);
            BotaSideTool.InitWithPDFViewer(pdfViewControl.PDFView);
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
            ViewSettingBtn.IsChecked = false;
            PropertyContainer.Child = null;
            PropertyContainer.Visibility = Visibility.Collapsed;
        }

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Sample_File_Windows.pdf";
            pdfViewControl = new PDFViewControl();
            pdfViewControl.PDFView.InitDocument(defaultFilePath);
            LoadDocument();
        }
        #endregion

        #region Password
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
                    pdfViewControl = passwordViewer;
                    LoadDocument();
                }
                else
                {
                    PasswordUI.SetShowError("Wrong Password", Visibility.Visible);
                }
            }
        }

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
        #endregion

        #region Expand or Hide Panel 

        private void ControlLeftPanel()
        {
            if (LeftToolPanelButton != null)
            {
                bool isExpand = LeftToolPanelButton.IsChecked == true;
                ExpandLeftPanel(isExpand);
            }
        }

        private void ExpandLeftPanel(bool isExpand)
        {
            BotaSideTool.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            Splitter.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            if (isExpand)
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(260);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(15);
            }
            else
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(0);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }

        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            BotaSideTool.SelectBotaTool(BOTATools.Search);
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton expandBtn = sender as ToggleButton;
            if (expandBtn != null)
            {
                bool isExpand = expandBtn.IsChecked == true;
                ExpandLeftPanel(isExpand);
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

        private void ShowViewSettings()
        {
            if (ViewSettingBtn != null)
            {
                if (ViewSettingBtn.IsChecked == true)
                {
                    displayPanel.InitWithPDFViewer(pdfViewControl.PDFView);
                    PropertyContainer.Child = displayPanel;
                    PropertyContainer.Visibility = Visibility.Visible; 
                }
                else
                {
                    PropertyContainer.Child = null;
                    PropertyContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowViewSettings();
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl != null)
            {
                double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor + 0.01, true);
                pdfViewControl.PDFView.Zoom(newZoom);
            }
        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl != null)
            {
                double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor - 0.01, false);
                pdfViewControl.PDFView.Zoom(newZoom);
            }
        }

        private void NextPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pdfViewControl.PDFView?.GoToPage(pdfViewControl.PDFView.CurrentIndex + 1);
        }

        private void PrevPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pdfViewControl.PDFView?.GoToPage(pdfViewControl.PDFView.CurrentIndex - 1);
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewControl.PDFView);
            PopupBorder.Visibility = Visibility.Visible;
        }

        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Open and save file

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

        private void SaveFile()
        {
            if (pdfViewControl != null && pdfViewControl.PDFView != null && pdfViewControl.PDFView.Document != null)
            {
                CPDFDocument pdfDoc = pdfViewControl.PDFView.Document;
                if (pdfDoc.WriteToLoadedPath())
                {
                    pdfViewControl.PDFView.UndoManager.CanSave = false;
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "(*.pdf)|*.pdf";
                saveDialog.DefaultExt = ".pdf";
                saveDialog.OverwritePrompt = true;

                if (saveDialog.ShowDialog() == true)
                {
                    if (pdfDoc.WriteToFilePath(saveDialog.FileName))
                    {
                        pdfViewControl.PDFView.UndoManager.CanSave = false;
                    }
                }
            }
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

        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        #endregion

        #region Context Menu

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
                            string savePath = Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                            image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            openPath = savePath;
                        }
                    }
                }
                Process.Start("explorer", "/select,\"" + openPath + "\"");
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
                else
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    //if (pdfViewControl.CheckHasForm())

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

                    {
                        MenuItem resetForms = new MenuItem();
                        resetForms.Header = LanguageHelper.CommonManager.GetString("Menu_Reset");
                        resetForms.Click += (o, p) =>
                        {
                            if (pdfViewControl != null)
                            {
                                pdfViewControl.PDFView?.ResetForm(null);
                            }
                        };
                        e.PopupMenu.Items.Add(new Separator());
                        e.PopupMenu.Items.Add(resetForms);
                    }
                }
            }

            if (e != null && e.CommandType == CommandType.Copy)
            {
                e.DoCommand();
            }
        }

        #endregion

        #region Close Window

        protected override void OnClosing(CancelEventArgs e)
        {
            if (pdfViewControl.PDFView.UndoManager.CanSave)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save your changes before closing the application？", "Message", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.No)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

        #region PropertyChanged

        /// <summary>
        /// Zoom
        /// </summary> 
        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        {
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }

        /// <summary>
        /// Undo Redo Event Noitfy
        /// </summary>
        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

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
            LeftToolPanelButton.IsChecked = !LeftToolPanelButton.IsChecked;
            ControlLeftPanel();
        }

        private void CommandBinding_Executed_Bookmark(object sender, ExecutedRoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            LeftToolPanelButton.IsChecked = true;
            BotaSideTool.SelectBotaTool(BOTATools.Bookmark);
        }

        private void CommandBinding_Executed_Outline(object sender, ExecutedRoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            LeftToolPanelButton.IsChecked = true;
            BotaSideTool.SelectBotaTool(BOTATools.Outline);
        }

        private void CommandBinding_Executed_Thumbnail(object sender, ExecutedRoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            LeftToolPanelButton.IsChecked = true;
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
        }

        private void CommandBinding_Executed_Search(object sender, ExecutedRoutedEventArgs e)
        {
            ExpandLeftPanel(true);
            LeftToolPanelButton.IsChecked = true;
            BotaSideTool.SelectBotaTool(BOTATools.Search);
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

        private void CommandBinding_Executed_DisplaySettings(object sender, ExecutedRoutedEventArgs e)
        {
            ViewSettingBtn.IsChecked = !ViewSettingBtn.IsChecked;
            ShowViewSettings();
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
