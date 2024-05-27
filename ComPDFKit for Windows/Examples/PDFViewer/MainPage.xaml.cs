using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.Measure;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.PDFView;
using ComPDFKitViewer;
using ComPDFKitViewer.Widget;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ComPDFKit.Controls.PDFControlUI;
using static ComPDFKit.Tool.CPDFToolManager;

namespace PDFViewer
{
    public partial class MainPage : UserControl, INotifyPropertyChanged
    {
        #region Properties

        private string currentMode = "Viewer";
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };

        private PDFViewControl viewControl;
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

        public PDFViewControl GetPDFViewControl()
        {
            return viewControl;
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
            viewControl = new PDFViewControl();
            viewControl.InitDocument(filePath);
        }

        public void InitWithDocument(CPDFDocument document)
        {
            viewControl = new PDFViewControl();
            viewControl.GetCPDFViewer().InitDoc(document);
        }

        /// <summary>
        /// Init controls and set events.
        /// </summary>
        private void LoadDocument()
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfviewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfviewer?.GetDocument();
                if (pdfDoc == null)
                {
                    return;
                }

                SizeChanged -= MainPage_SizeChanged;
                SizeChanged += MainPage_SizeChanged;

                PasswordUI.Closed -= PasswordUI_Closed;
                PasswordUI.Canceled -= PasswordUI_Canceled;
                PasswordUI.Confirmed -= PasswordUI_Confirmed;
                PasswordUI.Closed += PasswordUI_Closed;
                PasswordUI.Canceled += PasswordUI_Canceled;
                PasswordUI.Confirmed += PasswordUI_Confirmed;

                CPDFSaclingControl.InitWithPDFViewer(viewControl);
                ModeComboBox.SelectedIndex = 0;

                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(pdfviewer.GetZoom() * 100)));

                botaBarControl.AddBOTAContent(new[] { BOTATools.Thumbnail, BOTATools.Outline, BOTATools.Bookmark, BOTATools.Annotation, BOTATools.Search });
                botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
                ViewSettingBtn.IsChecked = false;
                botaBarControl.InitWithPDFViewer(viewControl);
                botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
                displaySettingsControl.InitWithPDFViewer(viewControl);
                LoadCustomControl();
                panelState.PropertyChanged -= PanelState_PropertyChanged;
                panelState.PropertyChanged += PanelState_PropertyChanged;
                displaySettingsControl.SplitModeChanged -= DisplaySettingsControl_SplitModeChanged;
                displaySettingsControl.SplitModeChanged += DisplaySettingsControl_SplitModeChanged;

                viewControl.PDFToolManager.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
                viewControl.PDFToolManager.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;

                pdfviewer.SetLinkHighlight(Properties.Settings.Default.IsHighlightLinkArea);
                pdfviewer.SetFormFieldHighlight(Properties.Settings.Default.IsHighlightFormArea);
                pdfviewer.ScrollStride = Properties.Settings.Default.Divisor;
            }
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            if (e.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                BaseWidget baseWidget = viewControl.GetCacheHitTestWidget();
                if (baseWidget != null)
                {
                    AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                    viewControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                    baseWidget.GetAnnotData().PageIndex,
                    baseWidget.GetAnnotData().Annot);
                    if ((annotParam as WidgetParm).WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                    {
                        var sigWidget = baseWidget.GetAnnotData().Annot as CPDFSignatureWidget;
                        if(sigWidget == null)
                        {
                            return;
                        }
                        var sig = sigWidget.GetSignature(viewControl.GetCPDFViewer().GetDocument());
                        if (sigWidget.IsSigned() && sig != null && sig.SignerList.Count > 0)
                        {
                            return;
                        }
                        if (currentMode == "Annotation")
                        {
                            CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                            signatureProperty.SetFormProperty(annotParam, viewControl, baseWidget.GetAnnotData().Annot);
                            panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                            annotationControl.SetPropertyContainer(signatureProperty);
                        }
                        else if (currentMode == "Viewer")
                        {
                            CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                            signatureProperty.SetFormProperty(annotParam, viewControl, baseWidget.GetAnnotData().Annot);
                            panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                            regularViewerControl.SetPropertyContainer(signatureProperty);
                        }
                    }
                }
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewControl.WindowSizeChange();
        }

        private void DisplaySettingsControl_SplitModeChanged(object sender, ComPDFKit.Controls.PDFControlUI.CPDFViewModeUI.SplitMode e)
        {
            viewControl.SetSplitViewMode(e);
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
                viewControl = newPdfViewer;
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
            if (passwordViewer != null && passwordViewer.PDFViewTool != null)
            {
                CPDFViewer pdfviewer = passwordViewer.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfviewer?.GetDocument();
                if (pdfDoc == null)
                {
                    return;
                }

                pdfDoc.UnlockWithPassword(e);
                if (pdfDoc.IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    viewControl = passwordViewer;
                    LoadDocument();
                    viewControl.PDFViewTool.GetCPDFViewer().UpdateVirtualNodes();
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
            regularViewerControl.PdfViewControl = viewControl;
            regularViewerControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
            regularViewerControl.OnCanSaveChanged += ControlOnCanSaveChanged;
            regularViewerControl.InitWithPDFViewer(viewControl);
            //regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            regularViewerControl.SetBOTAContainer(null);
            regularViewerControl.SetBOTAContainer(botaBarControl);
            regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
            PDFGrid.Child = regularViewerControl;

            SignatureHelper.InitEffectiveSignatureList(viewControl.GetCPDFViewer().GetDocument());
            SignatureHelper.VerifySignatureList(viewControl.GetCPDFViewer().GetDocument());
            signatureStatusBarControl.SetStatus(SignatureHelper.SignatureList);
            viewControl.PDFToolManager.SetToolType(ToolType.Viewer);
            
            viewControl.PDFViewTool.DocumentModifiedChanged -= PDFViewTool_DocumentModifiedChanged;
            viewControl.PDFViewTool.DocumentModifiedChanged += PDFViewTool_DocumentModifiedChanged;
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
            DigitalSignatureControl_OnSignatureStatusChanged(sender, e);
        }

        /// <summary>
        /// Event handler for updating a signature. Update the signature status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalSignatureControl_OnSignatureStatusChanged(object sender, EventArgs e)
        {
            SignatureHelper.InitEffectiveSignatureList(viewControl.GetCPDFViewer().GetDocument());
            SignatureHelper.VerifySignatureList(viewControl.GetCPDFViewer().GetDocument());
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

            if (viewControl != null && viewControl.GetCPDFViewer() != null)
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
                annotationControl.PDFAnnotationControl.AnnotationCancel();
                viewControl.SetIsShowStampMouse(false); 
            }
            else if (currentMode == "Form")
            {
                formControl.UnloadEvent();
                formControl.ClearViewerControl();
                viewControl.SetIsShowStampMouse(false);
                viewControl.GetCPDFViewer().SetFormFieldHighlight(Properties.Settings.Default.IsHighlightFormArea);
            }
            else if (currentMode == "Content Editor")
            {
                botaBarControl.ReplaceFunctionEnabled = false;
                displaySettingsControl.SetVisibilityWhenContentEdit(Visibility.Visible);
                contentEditControl.ClearViewerControl();
                contentEditControl.ClearPDFEditState();
            }
            else if (currentMode == "Document Editor")
            {
                pageEditControl.ExitPageEdit -= PageEditControl_ExitPageEdit;
                NotDocsEditorVisible = Visibility.Visible;
                botaBarControl.LoadThumbnail();
            }
            else if (currentMode == "Digital Signature")
            {
                RightPanelButton.Visibility = Visibility.Visible;
                digitalSignatureControl.ClearViewerControl();
                botaBarControl.RemoveBOTAContent(BOTATools.Signature);
                digitalSignatureControl.UnloadEvent();
            }
            else if (currentMode == "Measurement")
            {
                RightPanelButton.Visibility = Visibility.Visible;
                GetPDFViewControl().PDFViewTool.GetDefaultSettingParam().IsOpenMeasure = false;
                measureControl.ClearAllToolState();
                measureControl.ClearViewerControl();
                measureControl.UnloadEvent();
            }

            if (item.Tag as string == "Viewer")
            {
                regularViewerControl.PdfViewControl = viewControl;
                regularViewerControl.InitWithPDFViewer(viewControl);
                if (regularViewerControl.PdfViewControl != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    viewControl.SetToolType(ToolType.Viewer);
                    regularViewerControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    regularViewerControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Tag as string == "Annotation")
            {
                annotationControl.SetToolBarContainerVisibility(Visibility.Visible);
                PDFGrid.Child = annotationControl;

                viewControl.SetToolType(ToolType.Pan);
                annotationControl.PDFViewControl = viewControl;
                annotationControl.InitWithPDFViewer(viewControl);
                if (annotationControl.PDFViewControl != null)
                {
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
                formControl.PdfViewControl = viewControl;
                formControl.InitWithPDFViewer(viewControl);
                if (formControl.PdfViewControl != null)
                {
                    PDFGrid.Child = formControl;
                    viewControl.SetToolType(ToolType.WidgetEdit);
                    formControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    formControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    formControl.OnAnnotEditHandler -= PdfFormControlRefreshAnnotList;
                    formControl.OnAnnotEditHandler += PdfFormControlRefreshAnnotList;
                    formControl.SetBOTAContainer(botaBarControl);
                    formControl.InitialPDFViewControl(formControl.PdfViewControl);
                    formControl.SetDisplaySettingsControl(displaySettingsControl);
                    viewControl.GetCPDFViewer().SetFormFieldHighlight(true);
                }
            }
            else if (item.Tag as string == "Content Editor")
            {
                contentEditControl.PdfViewControl = viewControl;
                contentEditControl.InitWithPDFViewer(viewControl);
                displaySettingsControl.SetVisibilityWhenContentEdit(Visibility.Collapsed);
                if (contentEditControl.pdfContentEditControl != null && contentEditControl.PdfViewControl != null)
                { 
                    PDFGrid.Child = contentEditControl;
                    viewControl.SetToolType(ToolType.ContentEdit);
                    contentEditControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                    contentEditControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                    contentEditControl.SetBOTAContainer(botaBarControl);
                    contentEditControl.SetDisplaySettingsControl(displaySettingsControl);
                    contentEditControl.PdfViewControl.SetSplitViewMode(CPDFViewModeUI.SplitMode.None);
                }
            }
            else if (item.Tag as string == "Document Editor")
            {
                pageEditControl.PDFViewControl = viewControl;
                pageEditControl.ExitPageEdit += PageEditControl_ExitPageEdit;
                pageEditControl.OnCanSaveChanged -= ControlOnCanSaveChanged;
                pageEditControl.OnCanSaveChanged += ControlOnCanSaveChanged;
                PDFGrid.Child = pageEditControl;
                NotDocsEditorVisible = Visibility.Collapsed;
            }
            else if (item.Tag as string == "Digital Signature")
            {
                if (digitalSignatureControl.PDFViewControl != null)
                {
                    RightPanelButton.Visibility = Visibility.Collapsed;
                    PDFGrid.Child = digitalSignatureControl;
                    viewControl.PDFToolManager.SetToolType(ToolType.Viewer);
                    digitalSignatureControl.PDFViewControl = viewControl;
                    botaBarControl.AddBOTAContent(BOTATools.Signature);
                    digitalSignatureControl.InitWithPDFViewer(viewControl);
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
                if (contentEditControl.pdfContentEditControl != null && viewControl != null)
                {
                    RightPanelButton.Visibility = Visibility.Visible;
                    PDFGrid.Child = measureControl; 
                    viewControl.PDFToolManager.SetToolType(ToolType.Pan);
                    viewControl.SetToolType(ToolType.Pan); 
                    measureControl.InitWithPDFViewer(viewControl);
                    measureControl.SetBOTAContainer(botaBarControl);
                    measureControl.ClearAllToolState();
                    measureControl.SetSettingsControl(displaySettingsControl);
                    GetPDFViewControl().PDFViewTool.GetDefaultSettingParam().IsOpenMeasure = true;
                    measureControl.OnAnnotEditHandler -= PdfFormControlRefreshAnnotList;
                    measureControl.OnAnnotEditHandler += PdfFormControlRefreshAnnotList;
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
            if (viewControl != null && viewControl.GetCPDFViewer() != null && viewControl.GetCPDFViewer().GetDocument() != null)
            {
                string savePath = ComPDFKit.Controls.Helper.CommonHelper.GetGeneratePathOrEmpty("PDF files (*.pdf)|*.pdf", viewControl.GetCPDFViewer().GetDocument().FileName + "_Flattened.pdf");
                if (!string.IsNullOrEmpty(savePath))
                {
                    if (CanSave)
                    {
                        SaveFile();

                        viewControl.PDFViewTool.IsDocumentModified = false;
                    }
                    CPDFDocument document = CPDFDocument.InitWithFilePath(viewControl.GetCPDFViewer().GetDocument().FilePath);
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

        private void PDFViewTool_DocumentModifiedChanged(object sender, EventArgs e)
        {
            CanSave = viewControl.PDFViewTool.IsDocumentModified;
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
            CanSave = false;
        }

        private void OpenFile(string filePath = "")
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfviewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfviewer?.GetDocument();
                if (pdfDoc == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = ComPDFKit.Controls.Helper.CommonHelper.GetExistedPathOrEmpty();
                }
                string oldFilePath = pdfDoc.FilePath;

                if (!string.IsNullOrEmpty(filePath) && regularViewerControl.PdfViewControl != null)
                {
                    if (oldFilePath.ToLower() == filePath.ToLower())
                    {
                        return;
                    }

                    if ((bool)CheckExistBeforeOpenFileEvent?.Invoke(new string[] { filePath, oldFilePath }))
                    {
                        return;
                    }

                    passwordViewer = new PDFViewControl();
                    passwordViewer.InitDocument(filePath);
                    CPDFViewer tempViewer = passwordViewer.PDFViewTool?.GetCPDFViewer();
                    CPDFDocument tempDoc = tempViewer?.GetDocument();
                    if (tempDoc == null)
                    {
                        MessageBox.Show("Open File Failed");
                        return;
                    }

                    if (passwordViewer.GetCPDFViewer().GetDocument().IsLocked)
                    {
                        PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                        PasswordUI.ClearPassword();
                        PopupBorder.Visibility = Visibility.Visible;
                        PasswordUI.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pdfDoc.Release();
                        viewControl = passwordViewer;
                        LoadDocument();
                        FileChangeEvent?.Invoke(null, EventArgs.Empty);
                    }
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
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfviewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfviewer?.GetDocument();
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
        public void SaveFile()
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfviewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfviewer?.GetDocument();
                if (pdfDoc == null)
                {
                    return;
                }
                try
                {
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
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                if (pdfViewer != null)
                {
                    double newZoom = CheckZoomLevel(pdfViewer.GetZoom() + 0.01, true);
                    pdfViewer.SetZoom(newZoom);
                }
            }
        }

        private void CommandBinding_Executed_ScaleSubtract(object sender, ExecutedRoutedEventArgs e)
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                if (pdfViewer != null)
                {
                    double newZoom = CheckZoomLevel(pdfViewer.GetZoom() - 0.01, false);
                    pdfViewer.SetZoom(newZoom);
                }
            }
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
                FileInfoControl.InitWithPDFViewer(viewControl);
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
