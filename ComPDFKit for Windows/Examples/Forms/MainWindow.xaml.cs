using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ComPDFKit.Controls.PDFView;
using ComPDFKitViewer;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;

namespace FormViewControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window, INotifyPropertyChanged
    {
        #region Properties

        private PDFViewControl pdfViewer;
        private PDFViewControl passwordViewer;
        private FormControl formControl = new FormControl();
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
        private PanelState panelState = PanelState.GetInstance();
        private string currentMode = "Viewer";
        public event PropertyChangedEventHandler PropertyChanged;

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
            string defaultFilePath = "ComPDFKit_Forms_Sample_File.pdf";
            pdfViewer.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            formControl.PdfViewControl = pdfViewer;
            regularViewerControl.PdfViewControl = pdfViewer;
            formControl.InitWithPDFViewer(pdfViewer);
            
            formControl.ClearAllToolState();
            formControl.ExpandRightPropertyPanel(null, Visibility.Collapsed);

            formControl.OnAnnotEditHandler -= PdfFormControlRefreshAnnotList;
            formControl.OnAnnotEditHandler += PdfFormControlRefreshAnnotList;
            
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;
            
            ModeComboBox.SelectedIndex = 1;
            formControl.SetToolBarContainerVisibility(Visibility.Visible);
            formControl.PdfViewControl.PDFViewTool.GetCPDFViewer().SetFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(formControl.PdfViewControl);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(formControl.PdfViewControl.PDFViewTool.GetCPDFViewer().GetZoom() * 100)));
            
            ViewSettingBtn.IsChecked = false;
            botaBarControl.InitWithPDFViewer(formControl.PdfViewControl);
            botaBarControl.AddBOTAContent(new []{BOTATools.Thumbnail , BOTATools.Outline , BOTATools.Bookmark , BOTATools.Annotation , BOTATools.Search});
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
            formControl.SetBOTAContainer(botaBarControl);
            formControl.InitialPDFViewControl(formControl.PdfViewControl);
            
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;

            regularViewerControl.PdfViewControl.PDFViewTool.DocumentModifiedChanged -= PDFViewTool_DocumentModifiedChanged;
            regularViewerControl.PdfViewControl.PDFViewTool.DocumentModifiedChanged += PDFViewTool_DocumentModifiedChanged;
        }

        private void PDFViewTool_DocumentModifiedChanged(object sender, EventArgs e)
        {
            CanSave = regularViewerControl.PdfViewControl.PDFViewTool.IsDocumentModified;
        }

        private void OpenFile()
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (!string.IsNullOrEmpty(filePath) && formControl.PdfViewControl != null)
            {
                if (pdfViewer.PDFViewTool != null && pdfViewer.PDFToolManager.GetDocument() != null)
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
                    passwordViewer.PDFToolManager.GetDocument().Release();
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
            }
        }

        private void PdfFormControlRefreshAnnotList(object sender, EventArgs e)
        {
            botaBarControl.LoadAnnotationList();
        }

        #endregion
        
        #region Password
        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFViewTool != null && passwordViewer.PDFToolManager.GetDocument() != null)
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
        #endregion
        
        #region Event handle

        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        { 
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }
        
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            CanSave = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
        }

        bool T = false;

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
            else if (currentMode == "Form")
            {
                formControl.ClearViewerControl();
            }
            
            if ((string)item.Content == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFToolManager != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    regularViewerControl.PdfViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    regularViewerControl.PdfViewControl = pdfViewer;
                    regularViewerControl.InitWithPDFViewer(pdfViewer);
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if ((string)item.Content == "Form")
            {
                if (formControl.PdfViewControl != null && formControl.PdfViewControl.PDFToolManager != null)
                {
                    PDFGrid.Child = formControl;
                    if (T)
                    {
                        formControl.PdfViewControl.PDFToolManager.SetToolType(ToolType.WidgetEdit);
                    }
                    if (IsLoaded)
                    {
                        T = IsLoaded;
                    }

                    formControl.PdfViewControl = pdfViewer;
                    formControl.InitWithPDFViewer(pdfViewer);
                    formControl.SetBOTAContainer(botaBarControl);
                    formControl.SetDisplaySettingsControl(displaySettingsControl);
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

        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Search);
        }
        
        private void ClearPanelState()
        {
            LeftToolPanelButtonIsChecked = false;
            ViewSettingBtnIsChecked = false;
            RightToolPanelButtonIsChecked = false;
        }
        
        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }
        
        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RightPanel")
            {
                OnPropertyChanged(nameof(RightToolPanelButtonIsChecked));
                OnPropertyChanged(nameof(ViewSettingBtnIsChecked));
            }
        }
        
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