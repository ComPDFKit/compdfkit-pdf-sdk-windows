using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.Controls.PDFView;
using ComPDFKit.Tool;
using System.Reflection;
using System.Linq;
using ComPDFKit.Controls.PDFControlUI; 

namespace AnnotationViewControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window, INotifyPropertyChanged
    {
        #region Properties
        
        private string currentMode = "Annotation";
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private PDFViewControl pdfViewer;
        private PDFViewControl passwordViewer;
        private AnnotationControl annotationControl = new AnnotationControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
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
        
        public bool LeftToolPanelButtonIsChecked
        {
            get => panelState.IsLeftPanelExpand;
            set
            {
                panelState.IsLeftPanelExpand = value;
                OnPropertyChanged();
            }
        }

        public bool RightToolPanelButtonIsChecked
        {
            get
            {
                return (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel);
            }
            set
            {
                panelState.RightPanel = (value) ? PanelState.RightPanelState.PropertyPanel : PanelState.RightPanelState.None;
                OnPropertyChanged();
            }
        }

        public bool ViewSettingBtnIsChecked
        {
            get
            {
                return (panelState.RightPanel == PanelState.RightPanelState.ViewSettings);
            }
            set
            {
                panelState.RightPanel = (value) ? PanelState.RightPanelState.ViewSettings : PanelState.RightPanelState.None;
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
            DataContext = this;
        }

        #region Load document

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Annotations_Sample_File.pdf";
            pdfViewer.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            if (pdfViewer != null && pdfViewer.PDFViewTool != null)
            {
                CPDFViewer viewer = pdfViewer.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = viewer?.GetDocument();
                if (pdfDoc == null)
                {
                    return;
                }

                //pdfViewer.PDFView.InfoChanged -= PdfViewer_InfoChanged;
                //pdfViewer.PDFView.InfoChanged += PdfViewer_InfoChanged;
                PDFGrid.Child = annotationControl;

                annotationControl.PDFViewControl = pdfViewer;
                regularViewerControl.PdfViewControl = pdfViewer;
                annotationControl.InitWithPDFViewer(pdfViewer);
                annotationControl.ClearAllToolState();
                annotationControl.ExpandRightPropertyPanel(null, Visibility.Collapsed);

                annotationControl.OnCanSaveChanged -= AnnotationControl_OnCanSaveChanged;
                annotationControl.OnCanSaveChanged += AnnotationControl_OnCanSaveChanged;
                annotationControl.OnAnnotEditHandler -= PdfAnnotationControl_RefreshAnnotList;
                annotationControl.OnAnnotEditHandler += PdfAnnotationControl_RefreshAnnotList;

                //annotationControl.PDFViewControl.PDFView.SetFormFieldHighlight(true);
                PasswordUI.Closed -= PasswordUI_Closed;
                PasswordUI.Canceled -= PasswordUI_Canceled;
                PasswordUI.Confirmed -= PasswordUI_Confirmed;
                PasswordUI.Closed += PasswordUI_Closed;
                PasswordUI.Canceled += PasswordUI_Canceled;
                PasswordUI.Confirmed += PasswordUI_Confirmed;
                ModeComboBox.SelectedIndex = 1;
                //annotationControl.PDFViewControl.PDFView.ChangeFitMode(FitMode.FitWidth);
                CPDFSaclingControl.InitWithPDFViewer(annotationControl.PDFViewControl);
                //CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(annotationControl.PDFViewControl.PDFView.ZoomFactor * 100)));
                displaySettingsControl.InitWithPDFViewer(annotationControl.PDFViewControl);
                ViewSettingBtn.IsChecked = false;
                botaBarControl.InitWithPDFViewer(annotationControl.PDFViewControl);
                botaBarControl.AddBOTAContent(new[]
                {
                    BOTATools.Thumbnail, BOTATools.Outline, BOTATools.Bookmark, BOTATools.Annotation, BOTATools.Search
                });
                botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
                annotationControl.SetBOTAContainer(botaBarControl);
                annotationControl.InitialPDFViewControl(annotationControl.PDFViewControl);
                panelState.PropertyChanged -= PanelState_PropertyChanged;
                panelState.PropertyChanged += PanelState_PropertyChanged;
                displaySettingsControl.SplitModeChanged -= DisplaySettingsControl_SplitModeChanged;
                displaySettingsControl.SplitModeChanged += DisplaySettingsControl_SplitModeChanged;
                annotationControl.PDFViewControl.PDFViewTool.DocumentModifiedChanged -= PDFViewTool_DocumentModifiedChanged;
                annotationControl.PDFViewControl.PDFViewTool.DocumentModifiedChanged += PDFViewTool_DocumentModifiedChanged;
            }
        }

        private void PDFViewTool_DocumentModifiedChanged(object sender, EventArgs e)
        { 
            CanSave = annotationControl.PDFViewControl.PDFViewTool.IsDocumentModified;
        }

        private void DisplaySettingsControl_SplitModeChanged(object sender, CPDFViewModeUI.SplitMode e)
        {
            pdfViewer.SetSplitViewMode(e);
        }

        private void OpenFile()
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (!string.IsNullOrEmpty(filePath) && annotationControl.PDFViewControl != null)
            {
                if (pdfViewer != null && pdfViewer.PDFToolManager != null)
                {
                    string oldFilePath = pdfViewer.PDFToolManager.GetDocument().FilePath;
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
                    pdfViewer?.PDFToolManager?.GetDocument().Release();
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
            }
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
                    pdfViewer = passwordViewer;
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
        
        #region Load Unload custom control

        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            pdfViewer = new PDFViewControl();
            LoadDefaultDocument();
        }
        
        private void LoadCustomControl()
        {
            regularViewerControl.InitWithPDFViewer(pdfViewer);
            regularViewerControl.PdfViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
            regularViewerControl.SetBOTAContainer(null);
            regularViewerControl.SetBOTAContainer(botaBarControl);
            regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
            PDFGrid.Child = regularViewerControl;

        }
        #endregion
        
        #region Event handle

        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        { 
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }
        
        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RightPanel")
            {
                OnPropertyChanged(nameof(RightToolPanelButtonIsChecked));
                OnPropertyChanged(nameof(ViewSettingBtnIsChecked));
            }
        }
        
        private void AnnotationControl_OnCanSaveChanged(object sender, bool e)
        {
            this.CanSave = e;
        }

        private void PdfAnnotationControl_RefreshAnnotList(object sender, EventArgs e)
        {
            botaBarControl.LoadAnnotationList();
        }
        
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            pdfViewer.PDFViewTool.IsDocumentModified = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item.Content as string == currentMode)
            {
                return;
            }
            ClearPanelState();
            
            if (currentMode == "Viewer")
            {
                regularViewerControl.ClearViewerControl();
            }
            else if (currentMode == "Annotation")
            {
                annotationControl.ClearViewerControl();
            }

            if (item.Content as string == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFViewTool != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    regularViewerControl.PdfViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    regularViewerControl.InitWithPDFViewer(pdfViewer);
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Content as string == "Annotation")
            {
                if (annotationControl.PDFViewControl != null && annotationControl.PDFViewControl.PDFViewTool != null)
                {
                    PDFGrid.Child = annotationControl;
                    annotationControl.PDFViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    annotationControl.InitWithPDFViewer(pdfViewer);
                    annotationControl.SetBOTAContainer(botaBarControl);
                    annotationControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            currentMode = item.Content as string;
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewer);
            PopupBorder.Visibility = Visibility.Visible;
        }

        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                    PanelState.RightPanelState.ViewSettings : PanelState.RightPanelState.None;
        }

        private void RightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                    PanelState.RightPanelState.PropertyPanel : PanelState.RightPanelState.None;
        }
        
        private void ClearPanelState()
        {
            LeftToolPanelButtonIsChecked = false;
            ViewSettingBtnIsChecked = false;
            RightToolPanelButtonIsChecked = false;
        }

        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            LeftToolPanelButtonIsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Search);
        }
        
        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion

        #region Save file
        /// <summary>
        /// Save the file to another PDF file.
        /// </summary>
        public void SaveAsFile()
        {
            {
                if (pdfViewer != null && pdfViewer.PDFToolManager != null && pdfViewer.PDFToolManager.GetDocument() != null)
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFToolManager.GetDocument();
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
            if (pdfViewer != null && pdfViewer.PDFToolManager != null && pdfViewer.PDFToolManager.GetDocument() != null)
            {
                try
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFToolManager.GetDocument();
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
                catch (Exception ex)
                {

                }
            }
        }
        #endregion
    }
}