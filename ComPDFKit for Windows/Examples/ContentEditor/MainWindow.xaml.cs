using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.PDFView;
using ComPDFKitViewer;
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
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool;

namespace ContentEditorViewControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window, INotifyPropertyChanged
    {
        #region Properties

        private PanelState panelState = PanelState.GetInstance();
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private PDFViewControl viewControl;
        private PDFViewControl passwordViewer;
        private ContentEditControl contentEditControl = new ContentEditControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
        public event PropertyChangedEventHandler PropertyChanged;
        private string currentMode = "Viewer";

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
            string defaultFilePath = "ComPDFKit_Sample_File_Windows.pdf";
            viewControl.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            PDFGrid.Child = contentEditControl;
            
            regularViewerControl.PdfViewControl = viewControl;
            contentEditControl.PdfViewControl = viewControl;
            contentEditControl.InitWithPDFViewer(viewControl);
            InitialPDFViewControl();

            contentEditControl.OnAnnotEditHandler -= PdfContentEditControlRefreshAnnotList;
            contentEditControl.OnAnnotEditHandler += PdfContentEditControlRefreshAnnotList;

            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;

            ViewComboBox.SelectedIndex = 1;
            contentEditControl.PdfViewControl.GetCPDFViewer().SetFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(contentEditControl.PdfViewControl);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(contentEditControl.PdfViewControl.GetCPDFViewer().GetZoom() * 100)));

            ViewSettingBtn.IsChecked = false;
            botaBarControl.InitWithPDFViewer(contentEditControl.PdfViewControl);
            botaBarControl.AddBOTAContent(new[] { BOTATools.Thumbnail, BOTATools.Outline, BOTATools.Bookmark, BOTATools.Annotation, BOTATools.Search });
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
            contentEditControl.SetBOTAContainer(botaBarControl);
            displaySettingsControl.InitWithPDFViewer(contentEditControl.PdfViewControl);
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
            if (!string.IsNullOrEmpty(filePath) && contentEditControl.PdfViewControl != null)
            {
                if (viewControl.PDFToolManager != null && viewControl.PDFToolManager.GetDocument() != null)
                {
                    string oldFilePath = viewControl.PDFToolManager.GetDocument().FilePath;
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
                    viewControl.PDFToolManager.GetDocument().Release();
                    viewControl = passwordViewer;
                    LoadDocument();
                }
            }
        }

        #endregion

        #region Password

        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFViewTool != null)
            {
                CPDFViewer pdfViewer=passwordViewer.PDFViewTool.GetCPDFViewer();

                passwordViewer.PDFToolManager.GetDocument().UnlockWithPassword(e);
                if (passwordViewer.PDFToolManager.GetDocument().IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    viewControl = passwordViewer;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewControl = new PDFViewControl();
            LoadDefaultDocument();
        }
        #endregion

        #region Annotation

        private void InitialPDFViewControl()
        {
            contentEditControl.ExpandRightPropertyPanel(null, Visibility.Collapsed);
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
            regularViewerControl.PdfViewControl.PDFViewTool.IsDocumentModified = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
            contentEditControl.PdfViewControl.GetCPDFViewer().GoToPage(1, new Point(100, 100));
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
            else if (currentMode == "Content Edit")
            {
                contentEditControl.ClearViewerControl();
            }
            if ((string)item.Content == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFViewTool != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    regularViewerControl.PdfViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    regularViewerControl.PdfViewControl = viewControl;
                    regularViewerControl.InitWithPDFViewer(viewControl);
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if ((string)item.Content == "Content Edit")
            {
                contentEditControl.PdfViewControl = viewControl;
                contentEditControl.InitWithPDFViewer(viewControl);
                displaySettingsControl.SetVisibilityWhenContentEdit(Visibility.Collapsed);
                if (contentEditControl.pdfContentEditControl != null && contentEditControl.PdfViewControl!= null)
                {
                    PDFGrid.Child = contentEditControl;
                    viewControl.SetToolType(ToolType.ContentEdit);
                    contentEditControl.SetBOTAContainer(botaBarControl);
                    contentEditControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            currentMode = item.Content as string;
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(viewControl);
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

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RightPanel")
            {
                OnPropertyChanged(nameof(RightToolPanelButtonIsChecked));
                OnPropertyChanged(nameof(ViewSettingBtnIsChecked));
            }
        }

        private void PdfContentEditControlRefreshAnnotList(object sender, EventArgs e)
        {
            botaBarControl.LoadAnnotationList();
        }


        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
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
            if (viewControl != null && viewControl.PDFViewTool != null )
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc= pdfViewer?.GetDocument();
                if (pdfDoc == null)
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

        /// <summary>
        /// Save the file in the current path.
        /// </summary>
        private void SaveFile()
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                if (pdfViewer == null)
                {
                    return;
                }
                try
                {
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
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

