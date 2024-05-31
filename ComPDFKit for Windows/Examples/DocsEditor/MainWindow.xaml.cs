using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ComPDFKit.PDFDocument;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using ComPDFKit.Tool;

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

        /// <summary>
        /// Whether the save operation can be performed.
        /// </summary>
        private bool _canSave = true;
        public bool CanSave
        {
            get => _canSave;
            set
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
        
        #region Load document

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Sample_File_Windows.pdf";
            LeftToolPanelButton.IsEnabled = false;
            SearchButton.IsEnabled = false;
            RightPanelButton.IsEnabled = false;
            ViewSettingBtn.IsEnabled = false;
            pdfViewControl = new PDFViewControl();
            pdfViewControl.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            if (pdfViewControl.PDFToolManager.GetDocument() == null)
            {
                return;
            }
            pdfViewControl.PDFToolManager?.SetToolType(ToolType.Viewer);
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
                pageEditControl.LoadThumbnails(pdfViewControl);
                pageEditControl.viewControl = pdfViewControl;
                PDFGrid.Child = pageEditControl;
                FloatPageTool.Visibility = Visibility.Collapsed;
            }
            
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;

            pdfViewControl.PDFViewTool?.GetCPDFViewer().SetFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(pdfViewControl);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewControl.PDFViewTool.GetCPDFViewer().GetZoom() * 100)));


            ViewSettingBtn.IsChecked = false;
            PropertyContainer.Child = null;
            PropertyContainer.Visibility = Visibility.Collapsed;
            FloatPageTool.InitWithPDFViewer(pdfViewControl);
            BotaSideTool.InitWithPDFViewer(pdfViewControl);
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);

            pdfViewControl.PDFViewTool.DocumentModifiedChanged -= PDFViewTool_DocumentModifiedChanged;
            pdfViewControl.PDFViewTool.DocumentModifiedChanged += PDFViewTool_DocumentModifiedChanged;
        }

        private void PDFViewTool_DocumentModifiedChanged(object sender, EventArgs e)
        {
            CanSave = pdfViewControl.PDFViewTool.IsDocumentModified;
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
            if (passwordViewer != null && passwordViewer.PDFToolManager != null && passwordViewer.PDFToolManager.GetDocument() != null)
            {
                passwordViewer.PDFToolManager.GetDocument().UnlockWithPassword(e);
                if (passwordViewer.PDFToolManager.GetDocument().IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    pdfViewControl.PDFToolManager.GetDocument().Release();
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
            FileInfoControl.InitWithPDFViewer(pdfViewControl);
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
                if (pdfViewControl.PDFToolManager != null && pdfViewControl.PDFToolManager.GetDocument() != null)
                {
                    string oldFilePath = pdfViewControl.PDFToolManager.GetDocument().FilePath;
                    if (oldFilePath.ToLower() == filePath.ToLower())
                    {
                        return;
                    }
                    else
                    {
                        pdfViewControl.PDFToolManager.GetDocument().Release();
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
            pdfViewControl.PDFViewTool.IsDocumentModified = false;
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

        /// <summary>
        /// Save the file in the current path.
        /// </summary>
        private void SaveFile()
        {
            if (pdfViewControl != null && pdfViewControl.PDFToolManager != null && pdfViewControl.PDFToolManager.GetDocument() != null)
            {
                CPDFDocument pdfDoc = pdfViewControl.PDFToolManager.GetDocument();
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
                    //pdfViewControl.PDFView.ReloadDocument();
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
                pageEditControl.LoadThumbnails(pdfViewControl);
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
            if (pdfViewControl == null)
            {
                return;
            }
            BotaSideTool.InitWithPDFViewer(pdfViewControl);
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
            double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() + 0.01, true);
            pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() - 0.01, false);
            pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
        }

        private void ShowViewSettings()
        {
            if (ViewSettingBtn != null)
            {
                if (ViewSettingBtn.IsChecked == true)
                {
                    displayPanel = new CPDFDisplaySettingsControl();
                    displayPanel.InitWithPDFViewer(pdfViewControl);
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
