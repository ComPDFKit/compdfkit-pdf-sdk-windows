using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using Compdfkit_Tools.PDFView;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System.Windows.Input;
using ComPDFKitViewer.AnnotEvent;
using Compdfkit_Tools.Measure;

namespace PDFViewer
{
    public partial class MainPage : UserControl, INotifyPropertyChanged
    {
        #region Properties

        private string currentMode = "Viewer";
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };

        public PDFViewControl pdfViewer;
        private PDFViewControl passwordViewer;
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private AnnotationControl annotationControl = new AnnotationControl();
        private FormControl formControl = new FormControl();
        private ContentEditControl contentEditControl = new ContentEditControl();
        private PageEditControl pageEditControl = new PageEditControl();
        private MeasureControl measureControl = new MeasureControl();
        private DigitalSignatureControl digitalSignatureControl = new DigitalSignatureControl();
        private SignatureStatusBarControl signatureStatusBarControl = new SignatureStatusBarControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();

        private PanelState panelState = PanelState.GetInstance();

        public event EventHandler FileChangeEvent;
        public event Func<string[], bool> CheckExistBeforeOpenFileEvent;
        public event EventHandler<string> AfterSaveAsFileEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _canSave = false;
        /// <summary>
        /// Whether the save operation can be performed.
        /// </summary>
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
            get => panelState.RightPanel == PanelState.RightPanelState.PropertyPanel;
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

        private Visibility _notDocsEditorVisible = Visibility.Visible;
        /// <summary>
        /// Whether the Document Editor panel is visible.
        /// </summary>
        public Visibility NotDocsEditorVisible
        {
            get => _notDocsEditorVisible;

            set
            {
                _notDocsEditorVisible = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region Load document

        /// <summary>
        /// Initialize the PDFViewer and load the PDF file.
        /// </summary>
        /// <param name="filePath"></param>
        public void InitWithFilePath(string filePath)
        {
            pdfViewer = new PDFViewControl();
            pdfViewer.PDFView.InitDocument(filePath);
        }

        public void InitWithDocument(CPDFDocument document)
        {
            pdfViewer = new PDFViewControl();
            pdfViewer.PDFView.InitDocument(document);
        }

        /// <summary>
        /// Init controls and set events.
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
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;

            pdfViewer.PDFView.ChangeFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(pdfViewer.PDFView);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.PDFView.ZoomFactor * 100)));

            ViewSettingBtn.IsChecked = false;
            botaBarControl.InitWithPDFViewer(pdfViewer.PDFView);
            ModeComboBox.SelectedIndex = 0;
            botaBarControl.AddBOTAContent(new[] { BOTATools.Thumbnail, BOTATools.Outline, BOTATools.Bookmark, BOTATools.Annotation, BOTATools.Search });
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
            botaBarControl.DeleteSignatureEvent -= BotaControlOnDeleteSignatureEvent;
            botaBarControl.DeleteSignatureEvent += BotaControlOnDeleteSignatureEvent;
            botaBarControl.ViewCertificateEvent -= digitalSignatureControl.ViewCertificateEvent;
            botaBarControl.ViewCertificateEvent += digitalSignatureControl.ViewCertificateEvent;
            botaBarControl.ViewSignatureEvent -= digitalSignatureControl.ViewSignatureEvent;
            botaBarControl.ViewSignatureEvent += digitalSignatureControl.ViewSignatureEvent;

            displaySettingsControl.InitWithPDFViewer(pdfViewer.PDFView);
            LoadCustomControl();
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;

            pdfViewer.PDFView.SetShowLink(Properties.Settings.Default.IsHighlightLinkArea);
            pdfViewer.PDFView.SetFormFieldHighlight(Properties.Settings.Default.IsHighlightFormArea);
            pdfViewer.PDFView.SetScrollStepDivisor(Properties.Settings.Default.Divisor / 100.0);
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

        public void SetFeatureMode(string featureName)
        {
            if (!string.IsNullOrEmpty(featureName))
            {
                switch (featureName)
                {
                    case "Viewer":
                        ModeComboBox.SelectedIndex = 0;
                        break;
                    case "Annotations":
                        ModeComboBox.SelectedIndex = 1;
                        break;
                    case "Forms":
                        ModeComboBox.SelectedIndex = 2;
                        break;
                    case "Signatures":
                        ModeComboBox.SelectedIndex = 5;
                        break;
                    case "Document Editor":
                        ModeComboBox.SelectedIndex = 4;
                        break;
                    case "Content Editor":
                        ModeComboBox.SelectedIndex = 3;
                        break;
                    case "Measurement":
                        ModeComboBox.SelectedIndex = 6;
                        break;
                    default:
                        break;
                }
            }
        }

        internal void SetPDFViewer(PDFViewControl newPdfViewer)
        {
            if (newPdfViewer != null)
            {
                pdfViewer = newPdfViewer;
            }
        }

        #endregion

        #region Password

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
                    FileChangeEvent?.Invoke(null, EventArgs.Empty);
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

        #region Load  custom control

        /// <summary>
        /// Load the custom controls for the PDF viewer.
        /// </summary>
        private void LoadCustomControl()
        {
            regularViewerControl.PdfViewControl = pdfViewer;
            regularViewerControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
            regularViewerControl.OnCanSaveChanged += ControlOnCanSaveChanged;
            regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
            regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            regularViewerControl.SetBOTAContainer(null);
            regularViewerControl.SetBOTAContainer(botaBarControl);
            regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
            PDFGrid.Child = regularViewerControl;

            SignatureHelper.InitEffectiveSignatureList(pdfViewer.PDFView.Document);
            SignatureHelper.VerifySignatureList(pdfViewer.PDFView.Document);
            digitalSignatureControl.LoadUndoManagerEvent(pdfViewer.PDFView);
            signatureStatusBarControl.SetStatus(SignatureHelper.SignatureList);
        }

        /// <summary>
        /// Event handler for the Loaded event of the MainPage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDocument();
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
        /// Event handler for deleting a signature from the BOTA. Set the CanSave property to true and update the signature status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BotaControlOnDeleteSignatureEvent(object sender, EventArgs e)
        {
            pdfViewer.PDFView.UndoManager.CanSave = true;
            DigitalSignatureControl_OnSignatureStatusChanged(sender, e);
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

        #endregion

        #region Private Command Event

        /// <summary>
        /// Close all the expanded panels.  
        /// </summary>
        private void ClearPanelState()
        {
            LeftToolPanelButtonIsChecked = false;
            ViewSettingBtnIsChecked = false;
            RightToolPanelButtonIsChecked = false;
        }
        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if ((string)item.Tag == currentMode)
            {
                return;
            }

            ClearPanelState();
            if (ViewSettingBtn != null)
                ViewSettingBtn.IsChecked = false;
            if (RightPanelButton != null)
                RightPanelButton.IsChecked = false;

            if (pdfViewer != null && pdfViewer.PDFView != null)
            {
                // pdfViewer.PDFView.ToolManager.EnableClickCreate = false;
            }

            if (currentMode == "Viewer")
            {
                regularViewerControl.ClearViewerControl();
            }
            else if (currentMode == "Annotation")
            {
                annotationControl.UnloadEvent();
                annotationControl.ClearViewerControl();
            }
            else if (currentMode == "Form")
            {
                formControl.UnloadEvent();
                formControl.ClearViewerControl();
            }
            else if (currentMode == "Content Editor")
            {
                contentEditControl.ClearViewerControl();
                contentEditControl.ClearPDFEditState();
            }
            else if (currentMode == "Document Editor")
            {
                pageEditControl.ExitPageEdit -= PageEditControl_ExitPageEdit;
                NotDocsEditorVisible = Visibility.Visible;
            }
            else if (currentMode == "Digital Signature")
            {
                RightPanelButton.Visibility = Visibility.Visible;
                digitalSignatureControl.UnloadEvent();
                digitalSignatureControl.ClearViewerControl();
                botaBarControl.RemoveBOTAContent(BOTATools.Signature);
            }
            else if (currentMode == "Measurement")
            {
                RightPanelButton.Visibility = Visibility.Visible;
                measureControl.ClearAllToolState();
                measureControl.ClearViewerControl();
            }

            if (item.Tag as string == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFView != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    pdfViewer.PDFView.SetMouseMode(MouseModes.Viewer);
                    regularViewerControl.PdfViewControl = pdfViewer;
                    regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
                    regularViewerControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    regularViewerControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Tag as string == "Annotation")
            {
                annotationControl.SetToolBarContainerVisibility(Visibility.Visible);
                if (annotationControl.PDFViewControl != null && annotationControl.PDFViewControl.PDFView != null)
                {
                    PDFGrid.Child = annotationControl;
                    pdfViewer.PDFView.SetMouseMode(MouseModes.AnnotCreate);
                    annotationControl.PDFViewControl.PDFView.SetToolParam(new AnnotHandlerEventArgs());
                    annotationControl.PDFViewControl = pdfViewer;
                    annotationControl.InitWithPDFViewer(pdfViewer.PDFView);
                    annotationControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    annotationControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    annotationControl.OnAnnotEditHandler -= PdfFormControlRefreshAnnotList;
                    annotationControl.OnAnnotEditHandler += PdfFormControlRefreshAnnotList;
                    annotationControl.InitialPDFViewControl(annotationControl.PDFViewControl);
                    annotationControl.SetBOTAContainer(botaBarControl);
                    annotationControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Tag as string == "Form")
            {
                formControl.SetToolBarContainerVisibility(Visibility.Visible);
                if (formControl.PdfViewControl != null && formControl.PdfViewControl.PDFView != null)
                {
                    PDFGrid.Child = formControl;
                    pdfViewer.PDFView.SetMouseMode(MouseModes.FormEditTool);
                    formControl.PdfViewControl = pdfViewer;
                    formControl.InitWithPDFViewer(pdfViewer.PDFView);
                    formControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    formControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    formControl.OnAnnotEditHandler -= PdfFormControlRefreshAnnotList;
                    formControl.OnAnnotEditHandler += PdfFormControlRefreshAnnotList;
                    formControl.SetBOTAContainer(botaBarControl);
                    formControl.InitialPDFViewControl(formControl.PdfViewControl);
                    formControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Tag as string == "Content Editor")
            {
                if (contentEditControl.pdfContentEditControl != null && contentEditControl.PdfViewControl.PDFView != null)
                {
                    pdfViewer.PDFView?.SetPDFEditType(CPDFEditType.EditText | CPDFEditType.EditImage);
                    pdfViewer.PDFView?.SetPDFEditCreateType(CPDFEditType.None);
                    pdfViewer.PDFView?.SetMouseMode(MouseModes.PDFEdit);
                    pdfViewer.PDFView?.ReloadDocument();

                    pdfViewer.PDFView?.SetSplitMode(SplitMode.None);

                    PDFGrid.Child = contentEditControl;
                    pdfViewer.PDFView.SetMouseMode(MouseModes.PDFEdit);
                    contentEditControl.PdfViewControl = pdfViewer;
                    contentEditControl.InitWithPDFViewer(pdfViewer.PDFView);
                    contentEditControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    contentEditControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    contentEditControl.SetBOTAContainer(botaBarControl);
                    contentEditControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Tag as string == "Document Editor")
            {
                pageEditControl.PDFViewControl = pdfViewer;
                pageEditControl.ExitPageEdit += PageEditControl_ExitPageEdit;
                PDFGrid.Child = pageEditControl;
                NotDocsEditorVisible = Visibility.Collapsed;
            }
            else if (item.Tag as string == "Digital Signature")
            {
                if (contentEditControl.pdfContentEditControl != null && contentEditControl.PdfViewControl.PDFView != null)
                {
                    RightPanelButton.Visibility = Visibility.Collapsed;
                    PDFGrid.Child = digitalSignatureControl;
                    digitalSignatureControl.PDFViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
                    digitalSignatureControl.PDFViewControl = pdfViewer;
                    botaBarControl.AddBOTAContent(BOTATools.Signature);
                    digitalSignatureControl.InitWithPDFViewer(pdfViewer.PDFView);
                    digitalSignatureControl.SetBOTAContainer(botaBarControl);
                    digitalSignatureControl.SetDisplaySettingsControl(displaySettingsControl);
                    digitalSignatureControl.SetSignatureStatusBarControl(signatureStatusBarControl);
                    signatureStatusBarControl.OnViewSignatureButtonClicked -= ViewAllSignatures;
                    signatureStatusBarControl.OnViewSignatureButtonClicked += ViewAllSignatures;
                    digitalSignatureControl.OnCanSaveChanged -= DigitalSignatureControl_OnCanSaveChanged;
                    digitalSignatureControl.OnCanSaveChanged += DigitalSignatureControl_OnCanSaveChanged;
                    digitalSignatureControl.SignatureStatusChanged -= DigitalSignatureControl_OnSignatureStatusChanged;
                    digitalSignatureControl.SignatureStatusChanged += DigitalSignatureControl_OnSignatureStatusChanged;
                    digitalSignatureControl.AfterFillSignature -= DigitalSignatureControl_AfterFillSignature;
                    digitalSignatureControl.AfterFillSignature += DigitalSignatureControl_AfterFillSignature;
                }
            }
            else if (item.Tag as string == "Measurement")
            {
                if (contentEditControl.pdfContentEditControl != null && contentEditControl.PdfViewControl.PDFView != null)
                {
                    RightPanelButton.Visibility = Visibility.Visible;
                    PDFGrid.Child = measureControl;
                    pdfViewer.PDFView.SetMouseMode(MouseModes.PanTool);
                    measureControl.InitWithPDFViewer(pdfViewer, pdfViewer.PDFView);
                }
            }
            currentMode = item.Tag as string;
            RightToolPanelButtonIsChecked = false;
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

        private void PageEditControl_ExitPageEdit(object sender, EventArgs e)
        {
            ModeComboBox.SelectedIndex = 0;
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewer.PDFView);
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

        private void CPDFTitleBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFTitleBarControl.OpenFileEvent -= CPDFTitleBarControl_OpenFileEvent;
            CPDFTitleBarControl.OpenFileEvent += CPDFTitleBarControl_OpenFileEvent;

            CPDFTitleBarControl.SaveAsFileEvent -= CPDFTitleBarControl_SaveAsFileEvent;
            CPDFTitleBarControl.SaveAsFileEvent += CPDFTitleBarControl_SaveAsFileEvent;

            CPDFTitleBarControl.SaveFileEvent -= CPDFTitleBarControl_SaveFileEvent;
            CPDFTitleBarControl.SaveFileEvent += CPDFTitleBarControl_SaveFileEvent;

            CPDFTitleBarControl.FlattenEvent -= CPDFTitleBarControl_FlattenEvent;
            CPDFTitleBarControl.FlattenEvent += CPDFTitleBarControl_FlattenEvent;
        }

        private void CPDFTitleBarControl_FlattenEvent(object sender, EventArgs e)
        {
            if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
            {
                string savePath = CommonHelper.GetGeneratePathOrEmpty("PDF files (*.pdf)|*.pdf", pdfViewer.PDFView.Document.FileName + "_Flattened.pdf");
                if (!string.IsNullOrEmpty(savePath))
                {
                    if (CanSave)
                    {
                        SaveFile();
                        pdfViewer.PDFView.UndoManager.CanSave = false;
                    }
                    CPDFDocument document = CPDFDocument.InitWithFilePath(pdfViewer.PDFView.Document.FilePath);
                    if (document?.WriteFlattenToFilePath(savePath) == true)
                    {
                        System.Diagnostics.Process.Start("Explorer.exe", "/select," + savePath);
                    }
                    document?.Release();
                }
            }
        }

        private void CPDFTitleBarControl_SaveFileEvent(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void CPDFTitleBarControl_SaveAsFileEvent(object sender, EventArgs e)
        {
            SaveAsFile();
        }

        private void CPDFTitleBarControl_OpenFileEvent(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Refresh the annotation list when a annotation is edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PdfFormControlRefreshAnnotList(object sender, EventArgs e)
        {
            botaBarControl.LoadAnnotationList();
        }

        /// <summary>
        /// When a CanSave property of a control is changed, the CanSave property of current page is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlOnCanSaveChanged(object sender, bool e)
        {
            this.CanSave = e;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Open and Save file

        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            pdfViewer.PDFView.UndoManager.CanSave = false;
        }

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

                if ((bool)CheckExistBeforeOpenFileEvent?.Invoke(new string[] { filePath, oldFilePath }))
                {
                    return;
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
                    FileChangeEvent?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// Save the file to another PDF file.
        /// </summary>
        public void SaveAsFile()
        {
            {
                if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFView.Document;
                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "(*.pdf)|*.pdf",
                        DefaultExt = ".pdf",
                        OverwritePrompt = true
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (pdfDoc.WriteToFilePath(saveDialog.FileName))
                        {
                            AfterSaveAsFileEvent?.Invoke(this, saveDialog.FileName);
                        }
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
                    if (!string.IsNullOrEmpty(pdfDoc.FilePath))
                    {
                        if (pdfDoc.WriteToLoadedPath())
                        {
                            return;
                        }
                    }

                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "(*.pdf)|*.pdf",
                        DefaultExt = ".pdf",
                        OverwritePrompt = true
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (pdfDoc.WriteToFilePath(saveDialog.FileName))
                        {
                            AfterSaveAsFileEvent?.Invoke(this, saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }
        #endregion

        #region Command Binding

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
            panelState.IsLeftPanelExpand = !panelState.IsLeftPanelExpand;
        }

        private void CommandBinding_Executed_ControlRightPanel(object sender, ExecutedRoutedEventArgs e)
        {
            if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
            {
                panelState.RightPanel = PanelState.RightPanelState.None;
            }
            else
            {
                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            }
        }

        private void CommandBinding_Executed_Bookmark(object sender, ExecutedRoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Bookmark);
        }

        private void CommandBinding_Executed_Outline(object sender, ExecutedRoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Outline);
        }

        private void CommandBinding_Executed_Thumbnail(object sender, ExecutedRoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
        }

        private void CommandBinding_Executed_Annotation(object sender, ExecutedRoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Annotation);
        }

        private void CommandBinding_Executed_Search(object sender, ExecutedRoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Search);
        }

        private void CommandBinding_Executed_ScaleAdd(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewer.PDFView.ZoomFactor + 0.01, true);
            pdfViewer.PDFView?.Zoom(newZoom);
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            double newZoom = CheckZoomLevel(pdfViewer.PDFView.ZoomFactor - 0.01, false);
            pdfViewer.PDFView?.Zoom(newZoom);
        }

        private void CommandBinding_Executed_DisplaySettings(object sender, ExecutedRoutedEventArgs e)
        {
            panelState.RightPanel = PanelState.RightPanelState.ViewSettings;
        }

        private void CommandBinding_Executed_DocumentInfo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PopupBorder.Visibility != Visibility.Visible)
            {
                PasswordUI.Visibility = Visibility.Collapsed;
                FileInfoControl.Visibility = Visibility.Visible;
                FileInfoControl.InitWithPDFViewer(pdfViewer.PDFView);
                FileInfoControl.CloseInfoEvent -= CPDFInfoControl_CloseInfoEvent;
                FileInfoControl.CloseInfoEvent += CPDFInfoControl_CloseInfoEvent;
                PopupBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FileInfoControl.Visibility = Visibility.Collapsed;
                PopupBorder.Visibility = Visibility.Collapsed;
                this.Focus();
            }
        }

        private void CPDFInfoControl_CloseInfoEvent(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
