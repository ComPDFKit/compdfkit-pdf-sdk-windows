using ComPDFKit.PDFDocument;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using Compdfkit_Tools.PDFView;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ComPDFKit.DigitalSign;

namespace DigitalSignature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentMode = "Viewer";
        private bool isFirstLoad = true;
        private PDFViewControl pdfViewer;
        private PDFViewControl passwordViewer;
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private DigitalSignatureControl digitalSignatureControl = new DigitalSignatureControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();
        private SignatureStatusBarControl signatureStatusBarControl = new SignatureStatusBarControl();
        private PanelState panelState = PanelState.GetInstance();
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        #region Properties

        /// <summary>
        /// Whether the save operation can be performed.
        /// </summary>
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
        
        /// <summary>
        /// Whether the left panel is expanded.
        /// </summary>
        public bool LeftToolPanelButtonIsChecked
        {
            get => panelState.IsLeftPanelExpand;
            set
            {
                panelState.IsLeftPanelExpand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the right panel is expanded.
        /// </summary>
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

        /// <summary>
        /// Whether the view setting panel is expanded.
        /// </summary>
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

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            digitalSignatureControl.AfterFillSignature -= DigitalSignatureControl_AfterFillSignature;
            digitalSignatureControl.AfterFillSignature += DigitalSignatureControl_AfterFillSignature;
            Closing += MainWindow_Closing;
        }

        #region Load Document

        /// <summary>
        /// Open a PDF file.
        /// </summary>
        /// <param name="filePath">The path of the PDF file.</param>
        private void OpenFile(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = CommonHelper.GetExistedPathOrEmpty();
            }
            string oldFilePath = pdfViewer.PDFView.Document.FilePath;

            if (!string.IsNullOrEmpty(filePath) && regularViewerControl.PdfViewControl != null)
            {
                if (pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
                {
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
                    pdfViewer.PDFView.Document.Release();
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
            }
        }

        /// <summary>
        /// Load the default PDF file when the application is started.
        /// </summary>
        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Signatures_Sample_File.pdf";
            pdfViewer.PDFView.InitDocument(defaultFilePath);
            LoadDocument();
        }

        /// <summary>
        /// Load the custom controls for the PDF viewer.
        /// </summary>
        private void LoadCustomControl()
        {
            regularViewerControl.PdfViewControl = pdfViewer;
            regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
            regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            regularViewerControl.SetBOTAContainer(null);
            regularViewerControl.SetBOTAContainer(botaBarControl);
            regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
            PDFGrid.Child = regularViewerControl;
            
            SignatureHelper.InitEffectiveSignatureList(pdfViewer.PDFView.Document);
            digitalSignatureControl.OnCanSaveChanged -= DigitalSignatureControl_OnCanSaveChanged;
            digitalSignatureControl.OnCanSaveChanged += DigitalSignatureControl_OnCanSaveChanged;
            digitalSignatureControl.SignatureStatusChanged -= DigitalSignatureControl_OnSignatureStatusChanged;
            digitalSignatureControl.SignatureStatusChanged += DigitalSignatureControl_OnSignatureStatusChanged;
            signatureStatusBarControl.OnViewSignatureButtonClicked -= ViewAllSignatures;
            signatureStatusBarControl.OnViewSignatureButtonClicked += ViewAllSignatures;
            
            SignatureHelper.VerifySignatureList(pdfViewer.PDFView.Document);
            digitalSignatureControl.LoadUndoManagerEvent(pdfViewer.PDFView);
            signatureStatusBarControl.SetStatus(SignatureHelper.SignatureList);
            regularViewerControl.SetSignatureStatusBarControl(signatureStatusBarControl);
        }

        /// <summary>
        /// Load current PDF file and initialize event handlers.
        /// </summary>
        private void LoadDocument()
        {
            if (pdfViewer.PDFView.Document == null)
            {
                return;
            }

            pdfViewer.PDFView.Load();
            pdfViewer.PDFView.SetShowLink(true);

            pdfViewer.PDFView.InfoChanged -= PdfViewer_InfoChanged;
            pdfViewer.PDFView.InfoChanged += PdfViewer_InfoChanged;

            pdfViewer.PDFView.SetFormFieldHighlight(true);
            pdfViewer.CustomSignHandle = true;

            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;
            ModeComboBox.SelectedIndex = 0;
            botaBarControl.InitWithPDFViewer(pdfViewer.PDFView);
            botaBarControl.AddBOTAContent(new []{BOTATools.Thumbnail , BOTATools.Outline , BOTATools.Bookmark , BOTATools.Search , BOTATools.Annotation , BOTATools.Signature});
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
            botaBarControl.DeleteSignatureEvent -= BotaControlOnDeleteSignatureEvent;
            botaBarControl.DeleteSignatureEvent += BotaControlOnDeleteSignatureEvent;
            botaBarControl.ViewCertificateEvent -= digitalSignatureControl.ViewCertificateEvent;
            botaBarControl.ViewCertificateEvent += digitalSignatureControl.ViewCertificateEvent;
            botaBarControl.ViewSignatureEvent -= digitalSignatureControl.ViewSignatureEvent;    
            botaBarControl.ViewSignatureEvent += digitalSignatureControl.ViewSignatureEvent;
            
            displaySettingsControl.InitWithPDFViewer(pdfViewer.PDFView);
            LoadCustomControl();
            pdfViewer.PDFView.ChangeFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(pdfViewer.PDFView);
            
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        /// <summary>
        /// Event handler for confirming the password. Open the PDF file if the password is correct.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
                else
                {
                    PasswordUI.SetShowError("Wrong Password", Visibility.Visible);
                }
            }
        }

        /// <summary>
        /// Event handler for canceling password input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordUI_Canceled(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Event handler for closing the password input dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordUI_Closed(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command for the OpenFileBtn click event. Opens a PDF file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// Command for opening a PDF file saved with a signature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalSignatureControl_AfterFillSignature(object sender, string e)
        {
            OpenFile(e);
        }
        
        /// <summary>
        /// When the RightPanel state is changed, decide whether to display the ViewSettingPanel or RightToolPanel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RightPanel")
            {
                OnPropertyChanged(nameof(RightToolPanelButtonIsChecked));
                OnPropertyChanged(nameof(ViewSettingBtnIsChecked));
            }
        }

        /// <summary>
        /// Event handler for deleting a signature from the BOTA. Set the CanSave property to true and update the signature status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BotaControlOnDeleteSignatureEvent(object sender, EventArgs e)
        {
            pdfViewer.PDFView.UndoManager.CanSave = true;
            DigitalSignatureControl_OnSignatureStatusChanged(sender, e);
            //this.CanSave = true;
        }
        
        /// <summary>
        /// Event handler for updating a signature. Update the signature status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalSignatureControl_OnSignatureStatusChanged(object sender, EventArgs e)
        {
            SignatureHelper.InitEffectiveSignatureList(pdfViewer.PDFView.Document);
            SignatureHelper.VerifySignatureList(pdfViewer.PDFView.Document);
            signatureStatusBarControl.SetStatus(SignatureHelper.SignatureList);
            botaBarControl.LoadSignatureList();
        }

        /// <summary>
        /// Event handler for updating the CanSave property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalSignatureControl_OnCanSaveChanged(object sender, bool e)
        {
            this.CanSave = e;
        }

        /// <summary>
        /// Event handler for the ZoomInBtn click event. Zoom in the PDF file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        {
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }
        
        /// <summary>
        /// Event handler for the SaveAsFileBtn click event. Save the PDF file and set the CanSave property to false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            pdfViewer.PDFView.UndoManager.CanSave = false;
        }

        /// <summary>
        /// Event handler for the LeftToolPanelButton click event. Expand or collapse the left panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
        }

        /// <summary>
        /// Event handler for the ComboBox selection changed event. Switch between the Viewer and Digital Signature modes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item.Content as string == currentMode)
            {
                return;
            }
            ClearPanelState();
            if(ViewSettingBtn != null)
                ViewSettingBtn.IsChecked = false;
            if(RightPanelButton != null)
                RightPanelButton.IsChecked = false;
            
            if (currentMode == "Viewer")
            {
                regularViewerControl.ClearViewerControl();
            }
            else if (currentMode == "Digital Signature")
            {
                digitalSignatureControl.ClearViewerControl();
            }

            if (item.Content as string == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFView != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
                    regularViewerControl.PdfViewControl = pdfViewer;
                    regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                    regularViewerControl.SetSignatureStatusBarControl(signatureStatusBarControl);
                }
            }
            else if (item.Content as string == "Digital Signature")
            {
                if (digitalSignatureControl.PDFViewControl != null && digitalSignatureControl.PDFViewControl.PDFView != null)
                {
                    PDFGrid.Child = digitalSignatureControl;
                    digitalSignatureControl.PDFViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
                    digitalSignatureControl.PDFViewControl = pdfViewer;
                    digitalSignatureControl.InitWithPDFViewer(pdfViewer.PDFView);
                    digitalSignatureControl.SetBOTAContainer(botaBarControl);
                    digitalSignatureControl.SetDisplaySettingsControl(displaySettingsControl);
                    digitalSignatureControl.SetSignatureStatusBarControl(signatureStatusBarControl);
                }
            }
            currentMode = item.Content as string;
        }
        
        /// <summary>
        /// Event handler for ViewSignatureBtn click event. Expand the bota control and select the Signature tool.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewAllSignatures(object sender, EventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Signature);
        }

        /// <summary>
        /// Event handler for the SearchBtn click event. Expand the bota control and select the Search tool.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Search);
        }

        /// <summary>
        /// Event handler for the RightPanelButton click event. Expand or collapse the right panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                PanelState.RightPanelState.PropertyPanel : PanelState.RightPanelState.None;
        }

        /// <summary>
        /// Event handler for the ViewSettingBtn click event. Expand or collapse the view setting panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                    PanelState.RightPanelState.ViewSettings : PanelState.RightPanelState.None;
        }
        
        /// <summary>
        /// Close all the expanded panels.
        /// </summary>
        private void ClearPanelState()
        {
            LeftToolPanelButtonIsChecked = false;
            ViewSettingBtnIsChecked = false;
            RightToolPanelButtonIsChecked = false;
        }

        /// <summary>
        /// Event handler for the PageInfoBtn click event. Expand the file info window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewer.PDFView);
            PopupBorder.Visibility = Visibility.Visible;

        }

        /// <summary>
        /// Event handler for the CloseFileInfoBtn click event. Collapse the file info window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Event handler for the control loaded event. Load the default PDF file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pdfViewer = new PDFViewControl();
            LoadDefaultDocument();
        }
        
        /// <summary>
        /// Event handler for the control unloaded event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        
        /// <summary>
        /// Event handler for closing the window. Prompt to save the changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(CanSave)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save the changes to the document?", 
                    "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
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
                if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFView.Document;
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
        public void SaveFile()
        {
            if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
            {
                try
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFView.Document;
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
