using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Path = System.IO.Path;
using ComPDFKit.Controls.Measure;
using ComPDFKit.Tool;
using static ComPDFKit.Controls.Helper.PanelState;
using System.Reflection;

namespace Measure
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        private PDFViewControl passwordViewer;
        private PDFViewControl pdfViewControl;

        private MeasureControl measureControl = new MeasureControl();
        CPDFDisplaySettingsControl displayPanel = new CPDFDisplaySettingsControl();

        private int[] zoomLevelList = { 10, 25, 50, 100, 150, 200, 300, 400, 500, 1000 };

        public event PropertyChangedEventHandler PropertyChanged;

        private PanelState panelState = PanelState.GetInstance();

        private bool _canSave = false;
        public bool CanSave
        {

            get => _canSave;
            set
            {
                _canSave = value;
                OnPropertyChanged();
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
            BotaSideTool.AddBOTAContent(BOTATools.Thumbnail | BOTATools.Outline | BOTATools.Bookmark | BOTATools.Search|BOTATools.Annotation);
            LoadDefaultDocument();
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
                {
                    RightPanelButton.IsChecked= true;
                    ViewSettingBtn.IsChecked = false;
                }
                else if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {

                    RightPanelButton.IsChecked = false;
                    ViewSettingBtn.IsChecked = true;
                }
                else
                {
                    RightPanelButton.IsChecked = false;
                    ViewSettingBtn.IsChecked = false;
                }
            }
        }

        #region Load Document
        private void LoadDocument()
        {
            if (pdfViewControl.GetCPDFViewer().GetDocument() == null)
            {
                return;
            }
            if(ViewModeBox!=null && ViewModeBox.SelectedIndex==1)
            {
                pdfViewControl.PDFToolManager?.SetToolType(ToolType.Pan);
            }
            else
            {
                pdfViewControl.PDFToolManager?.SetToolType(ToolType.Viewer);
            }
            
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;

            pdfViewControl.GetCPDFViewer().SetFitMode(FitMode.FitWidth);
            pdfViewControl.PDFViewTool.GetDefaultSettingParam().IsOpenMeasure = true;
            CPDFSaclingControl.InitWithPDFViewer(pdfViewControl);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewControl.GetCPDFViewer().GetZoom() * 100)));

            BotaSideTool.InitWithPDFViewer(pdfViewControl);
            BotaSideTool.SelectBotaTool(BOTATools.Thumbnail);
            ViewSettingBtn.IsChecked = false;

            LoadMeasureRes();
            pdfViewControl.PDFViewTool.DocumentModifiedChanged -= PDFViewTool_DocumentModifiedChanged;
            pdfViewControl.PDFViewTool.DocumentModifiedChanged += PDFViewTool_DocumentModifiedChanged;

        }

        private void PDFViewTool_DocumentModifiedChanged(object sender, EventArgs e)
        { 
            CanSave = pdfViewControl.PDFViewTool.IsDocumentModified;
        }

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "PDF32000_2008.pdf";
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
            LeftToolPanelButton.IsChecked = true;
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
            if (RightPanelButton != null)
            {
                if (RightPanelButton.IsChecked == true)
                {
                    RightPanelButton.IsChecked = false;
                    measureControl.ExpandRightPropertyPanel(Visibility.Collapsed);
                }
            }
            if (ViewSettingBtn != null)
            {
                if (ViewSettingBtn.IsChecked == true)
                {
                    measureControl.ExpandViewSettings(Visibility.Visible);
                }
                else
                {
                    measureControl.ExpandViewSettings(Visibility.Collapsed);
                    panelState.RightPanel = RightPanelState.None;
                }
            }
        }

        private void ControlRightPanel()
        {
            if (ViewSettingBtn != null)
            {
                if (ViewSettingBtn.IsChecked == true)
                {
                    ViewSettingBtn.IsChecked = false;
                    measureControl.ExpandViewSettings(Visibility.Collapsed);
                }
            }
            if (RightPanelButton != null)
            {
                if (RightPanelButton.IsChecked == true)
                {
                    if (measureControl.measurePropertyControl != null)
                    {
                        measureControl.ExpandRightPropertyPanel(Visibility.Visible);
                        if ((bool)ViewSettingBtn.IsChecked)
                        {
                            ViewSettingBtn.IsChecked = false;
                        }
                    }
                }
                else
                {
                    measureControl.ExpandRightPropertyPanel(Visibility.Collapsed);
                }
            }
        }

        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowViewSettings();
        }

        private void RightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ControlRightPanel();
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl != null)
            {
                double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() + 0.01, true);
                pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
            }
        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewControl != null)
            {
                double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() - 0.01, false);
                pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
            }
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
                    PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " password encrypted.");
                    PasswordUI.ClearPassword();
                    PopupBorder.Visibility = Visibility.Visible;
                    PasswordUI.Visibility = Visibility.Visible;
                }
                else
                {
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
            double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() + 0.01, true);
            pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewControl.GetCPDFViewer().GetZoom() - 0.01, false);
            pdfViewControl.GetCPDFViewer().SetZoom(newZoom);
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

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #region Measure

        private void LoadMeasureRes()
        {
            panelState.RightPanel = RightPanelState.None;
            FloatPageTool.Visibility = Visibility.Collapsed;
            PDFGrid.Child = null;
            RightPanelButton.Visibility = Visibility.Visible;
            PDFGrid.Child = measureControl;
            displayPanel.InitWithPDFViewer(pdfViewControl);
            measureControl.InitWithPDFViewer(pdfViewControl);
            measureControl.SetSettingsControl(displayPanel);
            measureControl.ExpandEvent -= MeasureControl_ExpandEvent;
            measureControl.ExpandEvent += MeasureControl_ExpandEvent;
        }

        private void MeasureControl_ExpandEvent(object sender, EventArgs e)
        {
            RightPanelButton.IsChecked = true;
            ControlRightPanel();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if ((string)item.Content == "Viewer")
            {
                panelState.RightPanel = RightPanelState.None;
                measureControl.ClearAllToolState();
                measureControl.ExpandNullRightPropertyPanel(Visibility.Collapsed);
                RightPanelButton.IsChecked = false;
                if (pdfViewControl != null && pdfViewControl.PDFViewTool != null)
                {
                    pdfViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    pdfViewControl.PDFViewTool.GetDefaultSettingParam().IsOpenMeasure = false;
                }
                RightPanelButton.Visibility = Visibility.Collapsed;
                measureControl.ClearViewerControl();
                PDFGrid.Child = pdfViewControl;
                FloatPageTool.Visibility = Visibility.Visible;
                FloatPageTool.InitWithPDFViewer(pdfViewControl);
            }
            else if ((string)item.Content == "Measurement")
            {
                LoadMeasureRes();
                if (pdfViewControl != null && pdfViewControl.PDFViewTool != null)
                {
                    pdfViewControl.PDFToolManager.SetToolType(ToolType.Pan);
                    pdfViewControl.PDFViewTool.GetDefaultSettingParam().IsOpenMeasure = true;
                }
            }
        }


        #endregion
    }
}
