using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool;
using ComPDFKitViewer;
using ComPDFKit.Tool.DrawTool;

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
                if (pdfViewControl != null && pdfViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    if (pdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo ||
                        pdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public string AppInfo
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name + " " + string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3)); }
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
            BotaSideTool.AddBOTAContent(new[] { BOTATools.Thumbnail, BOTATools.Outline, BOTATools.Bookmark, BOTATools.Search });
            LoadDefaultDocument();
        }

        #region Load Document
        private void LoadDocument()
        {
            if (pdfViewControl.PDFToolManager.GetDocument() == null)
            {
                return;
            }
            pdfViewControl.PDFToolManager?.SetToolType(ToolType.Viewer);
            PDFGrid.Child = pdfViewControl;
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;
            
            pdfViewControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            pdfViewControl.MouseRightButtonDownHandler += PDFViewControl_MouseRightButtonDownHandler;

            pdfViewControl.PDFViewTool.GetCPDFViewer().SetFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(pdfViewControl);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewControl.PDFViewTool.GetCPDFViewer().GetZoom() * 100)));

            FloatPageTool.InitWithPDFViewer(pdfViewControl);
            BotaSideTool.InitWithPDFViewer(pdfViewControl);
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
            ViewSettingBtn.IsChecked = false;
            PropertyContainer.Child = null;
            PropertyContainer.Visibility = Visibility.Collapsed;
        }

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Sample_File_Windows.pdf";
            pdfViewControl = new PDFViewControl();
            pdfViewControl.InitDocument(defaultFilePath);
            LoadDocument();
        }
        #endregion

        #region Password
        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFToolManager != null && passwordViewer.PDFToolManager.GetDocument() != null)
            {
                passwordViewer.PDFToolManager.GetDocument().UnlockWithPassword(e);
                if (passwordViewer.PDFToolManager.GetDocument().IsLocked == false)
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
                    displayPanel.InitWithPDFViewer(pdfViewControl);
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
                double newZoom = CheckZoomLevel(pdfViewControl.PDFViewTool.GetCPDFViewer().GetZoom() + 0.01, true);
                pdfViewControl.PDFViewTool.GetCPDFViewer().SetZoom(newZoom);
            }
        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl != null)
            {
                double newZoom = CheckZoomLevel(pdfViewControl.PDFViewTool.GetCPDFViewer().GetZoom() - 0.01, false);
                pdfViewControl.PDFViewTool.GetCPDFViewer().SetZoom(newZoom);
            }
        }

        private void NextPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pdfViewControl.PDFViewTool.GetCPDFViewer()?.GoToPage(pdfViewControl.PDFViewTool.GetCPDFViewer().CurrentRenderFrame.PageIndex + 1,new System.Windows.Point(0,0));
        }

        private void PrevPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pdfViewControl.PDFViewTool.GetCPDFViewer()?.GoToPage(pdfViewControl.PDFViewTool.GetCPDFViewer().CurrentRenderFrame.PageIndex - 1, new System.Windows.Point(0, 0));
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewControl);
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
                if (pdfViewControl != null && pdfViewControl.PDFToolManager != null && pdfViewControl.PDFToolManager.GetDocument() != null)
                {
                    CPDFDocument pdfDoc = pdfViewControl.PDFToolManager.GetDocument();
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
            if (pdfViewControl != null && pdfViewControl.PDFToolManager != null && pdfViewControl.PDFToolManager.GetDocument() != null)
            {
                CPDFDocument pdfDoc = pdfViewControl.PDFToolManager.GetDocument();
                if (pdfDoc.WriteToLoadedPath())
                {
                    pdfViewControl.PDFViewTool.IsDocumentModified = false;
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
                        pdfViewControl.PDFViewTool.IsDocumentModified = false;
                    }
                }
            }
        }

        private void OpenFile()
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (!string.IsNullOrEmpty(filePath) && pdfViewControl != null)
            {
                if (pdfViewControl.PDFToolManager != null && pdfViewControl.PDFToolManager.GetDocument() != null)
                {
                    string oldFilePath = pdfViewControl.PDFToolManager.GetDocument().FilePath;
                    if (oldFilePath.ToLower() == filePath.ToLower())
                    {
                        return;
                    }
                }

                passwordViewer = new PDFViewControl();
                passwordViewer.InitDocument(filePath);
                if (passwordViewer.PDFToolManager.GetDocument() == null)
                {
                    MessageBox.Show("Open File Failed");
                    return;
                }

                if (passwordViewer.PDFToolManager.GetDocument().IsLocked)
                {
                    PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                    PasswordUI.ClearPassword();
                    PopupBorder.Visibility = Visibility.Visible;
                    PasswordUI.Visibility = Visibility.Visible;
                }
                else
                {
                    pdfViewControl.PDFToolManager.GetDocument().Release();
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

        private void ExtractImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PageImageItem image = null;
                Dictionary<int, List<PageImageItem>> pageImageDict = pdfViewControl.FocusPDFViewTool.GetSelectImageItems();
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

                CPDFPage page = pdfViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
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
            Dictionary<int, List<PageImageItem>> pageImageDict = pdfViewControl.FocusPDFViewTool.GetSelectImageItems();
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

            CPDFPage page = pdfViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
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

        private void CreateImageContextMenu(object sender, ref ContextMenu menu)
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
        
        private void PDFViewControl_MouseRightButtonDownHandler(object sender, ComPDFKit.Tool.MouseEventObject e)
        {
            ContextMenu menu = pdfViewControl.GetRightMenu();
            if (menu == null)
            {
                menu = new ContextMenu();
            }
            switch (e.hitTestType)
            {
                case MouseHitTestType.ImageSelect:
                    CreateImageContextMenu(sender, ref menu);
                    break;
                default:
                    pdfViewControl.CreateViewerMenu(sender,ref menu);
                    break;
            }
            pdfViewControl.SetRightMenu(menu);
        }

        #endregion

        #region Close Window

        protected override void OnClosing(CancelEventArgs e)
        {
            if (pdfViewControl.PDFViewTool.IsDocumentModified)
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
            //double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor + 0.01, true);
            //pdfViewControl.PDFView?.Zoom(newZoom);
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            //double newZoom = CheckZoomLevel(pdfViewControl.PDFView.ZoomFactor - 0.01, false);
            //pdfViewControl.PDFView?.Zoom(newZoom);
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
                FileInfoControl.InitWithPDFViewer(pdfViewControl);
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
