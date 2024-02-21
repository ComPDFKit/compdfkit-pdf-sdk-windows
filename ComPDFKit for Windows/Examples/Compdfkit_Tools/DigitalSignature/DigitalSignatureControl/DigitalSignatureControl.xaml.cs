using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Compdfkit_Tools.Helper;
using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKitViewer;
using PasswordBoxPlus.Helper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class DigitalSignatureControl : UserControl, INotifyPropertyChanged
    {
        #region Properties
        
        private bool isFirstLoad = true;
        public PDFViewControl PDFViewControl = new PDFViewControl();
        private SignatureStatusBarControl signatureStatusBarControl;
        private PanelState panelState = PanelState.GetInstance();
        private CPDFDisplaySettingsControl displaySettingsControl = null;
        private CPDFSignatureWidget currentSignatureWidget;
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler<string> AfterFillSignature;
        public event EventHandler SignatureStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Whether the undo operation can be performed.
        /// </summary>
        public bool CanUndo
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFView != null)
                {
                    return PDFViewControl.PDFView.UndoManager.CanUndo;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether the redo operation can be performed.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFView != null)
                {
                    return PDFViewControl.PDFView.UndoManager.CanRedo;
                }

                return false;
            }
        }

        /// <summary>
        /// Whether the save operation can be performed.
        /// </summary>
        public bool CanSave
        {
            get
            {
                if (PDFViewControl != null && PDFViewControl.PDFView != null)
                {
                    return PDFViewControl.PDFView.UndoManager.CanSave;
                }

                return false;
            }
        }
        #endregion
        
        /// <summary>
        /// A digital signature control should be used in a window, and it should be initialized with a PDFViewer.
        /// Certificates will be saved in the TrustedFolder.
        /// </summary>
        public DigitalSignatureControl()
        {
            InitializeComponent();
            DataContext = this;
            string trustedFolder = AppDomain.CurrentDomain.BaseDirectory + @"\TrustedFolder\";
            if (!Directory.Exists(trustedFolder))
            {
                Directory.CreateDirectory(trustedFolder);
            }
            CPDFSignature.SignCertTrustedFolder = trustedFolder;
        }

        #region Public Method
        
        /// <summary>
        /// Disconnect all the specified elements that are already the logical children of another element. And reset bar control status.
        /// </summary>
        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
            SignatureStatusBorder.Child = null;
            DigitalSignatureBarControl.ClearAllToolState();
        }

        /// <summary>
        /// Init controls with pdfViewer, and load events.
        /// </summary>
        /// <param name="pdfViewer"></param>
        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            PDFViewControl.PDFView = pdfViewer;
            PDFGrid.Child = PDFViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            
            PDFViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            PDFViewControl.PDFView.SetShowLink(true);
            PDFViewControl.PDFView.SetFormFieldHighlight(true);
            PDFViewControl.PDFView.UndoManager.ClearHistory();
            
            DigitalSignatureBarControl.DigitalSignatureActionChanged -= DigitalSignatureBarControl_DigitalSignatureActionChanged;
            DigitalSignatureBarControl.DigitalSignatureActionChanged += DigitalSignatureBarControl_DigitalSignatureActionChanged;
            
            PDFViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
            PDFViewControl.PDFView.WidgetClickHandler += PDFView_WidgetClickHandler;
            PDFViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
            
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }
        
        /// <summary>
        /// Separately, init PDFView and load undo manager event. Only use for ensuring SaveBtn is enabled after deleting digital signature on Viewer mode.
        /// </summary>
        /// <param name="pdfViewer"></param>
        public void LoadUndoManagerEvent(CPDFViewer pdfViewer)
        {
            PDFViewControl.PDFView = pdfViewer;
            PDFViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PDFViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
        }

        /// <summary>
        /// Set child for BOTAContainer with BOTABarControl.
        /// </summary>
        /// <param name="botaControl"></param>
        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            BotaContainer.Child = botaControl;
        }

        /// <summary>
        /// Create a certificate info dialog with signature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Signature to init certificate</param>
        public void ViewCertificateEvent(object sender, CPDFSignature e)
        {
            Window parentWindow = Window.GetWindow((DependencyObject)sender);
            ViewCertificateDialog dialog = new ViewCertificateDialog()
            {
                Owner = parentWindow
            };
            dialog.InitCertificateList(e);
            dialog.CertificateInfoControl.TrustCertificateEvent += (o, args) =>
            {
                SignatureStatusChanged?.Invoke(this, null);
            };

            if (parentWindow is VerifyDigitalSignatureControl verifyControl)
            {
                dialog.CertificateInfoControl.TrustCertificateEvent += (o, args) =>
                {
                    verifyControl.InitWithSignature(e);
                };
            }
            dialog.ShowDialog();
        }

        /// <summary>
        /// Set display settings control.
        /// </summary>
        /// <param name="displaySettingsControl"></param>
        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

        /// <summary>
        /// Set visibility of SignatureStatusBarControl according its status.
        /// </summary>
        /// <param name="signatureStatusBarControl"></param>
        public void SetSignatureStatusBarControl(SignatureStatusBarControl signatureStatusBarControl)
        {
            this.signatureStatusBarControl = signatureStatusBarControl;
            SignatureStatusBorder.Child = this.signatureStatusBarControl;
            if (signatureStatusBarControl.Status != SignatureStatus.None)
            {
                SignatureStatusBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SignatureStatusBorder.Visibility = Visibility.Collapsed;
            }
        }
        
        /// <summary>
        /// Create a signature info dialog with signature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Signature to be displayed</param>
        public void ViewSignatureEvent(object sender, CPDFSignature e)
        {
            Window parentWindow = Window.GetWindow((DependencyObject)sender);
            VerifyDigitalSignatureControl dialog = new VerifyDigitalSignatureControl()
            {
                Owner = parentWindow
            };
            dialog.ViewCertificateEvent -= ViewCertificateEvent;
            dialog.ViewCertificateEvent += ViewCertificateEvent;
            dialog.InitWithSignature(e);
            dialog.ShowDialog();
        }
        
        #endregion

        #region Private Method

        /// <summary>
        /// Get current time as a string.
        /// </summary>
        /// <returns></returns>
        private string GetTime()
        {
            DateTime dateTime = DateTime.Now;
            return " " + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Create a signature field.
        /// </summary>
        private void CreateSign()
        {
            PDFViewControl.PDFView.SetMouseMode(MouseModes.FormEditTool);
            WidgetSignArgs signArgs = new WidgetSignArgs
            {
                LineWidth = 1,
                LineColor = Colors.Black,
                FieldName = "Signature" + GetTime()
            };
            PDFViewControl.PDFView.SetToolParam(signArgs);
        }
        
        /// <summary>
        /// Expand or collapse left panel.
        /// </summary>
        /// <param name="isExpand"></param>
        private void ExpandLeftPanel(bool isExpand)
        {
            BotaContainer.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
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

        /// <summary>
        /// Expand or collapse right panel.
        /// </summary>
        /// <param name="propertytPanel"></param>
        /// <param name="visible"></param>
        private void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        #endregion

        #region Private Command Event
        
        /// <summary>
        /// Click event of signature field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PDFView_WidgetClickHandler(object sender, WidgetArgs e)
        {
            var signatureWidget = (e as WidgetSignArgs).Sign;
            CPDFSignature sig = signatureWidget.GetSignature(PDFViewControl.PDFView.Document);
            if (signatureWidget.IsSigned() && sig!=null && sig?.SignerList.Count > 0)
            {
                ViewSignatureEvent(sender, sig);
            }
            else
            {
                Window parentWindow = Window.GetWindow((DependencyObject)sender);
                AddCertificationDialog addCertificationControl = new AddCertificationDialog
                {
                    Owner = parentWindow
                };
                currentSignatureWidget = signatureWidget;
                addCertificationControl.FillSignatureEvent -= AddCertificationControl_FillSignatureEvent;
                addCertificationControl.FillSignatureEvent += AddCertificationControl_FillSignatureEvent;
                addCertificationControl.ShowDialog(); 
            }
        }

        /// <summary>
        /// Event of filling a signature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCertificationControl_FillSignatureEvent(object sender, CertificateAccess e)
        {
            FillDigitalSignatureDialog fillDigitalSignatureDialog = new FillDigitalSignatureDialog
            {
                FilePath = e.filePath,
                Password = e.password,
                SignatureWidget = currentSignatureWidget,
                Document = PDFViewControl.PDFView.Document,
                Owner = Window.GetWindow(this)
            };
            fillDigitalSignatureDialog.AfterFillSignature += FillDigitalSignatureDialog_AfterFillSignature; ;
            fillDigitalSignatureDialog.ShowDialog();
        }

        /// <summary>
        /// Event of after filling a signature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FillDigitalSignatureDialog_AfterFillSignature(object sender, string e)
        {
            AfterFillSignature?.Invoke(this, e);
        }

        /// <summary>
        /// Click event of buttons in digital SignatureBarControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DigitalSignatureBarControl_DigitalSignatureActionChanged(object sender, Common.CPDFDigitalSignatureBarControl.DigitalSignatureAction e)
        {
            if (e == Common.CPDFDigitalSignatureBarControl.DigitalSignatureAction.AddSignatureField)
            {
                CreateSign();
            }
            else if (e == Common.CPDFDigitalSignatureBarControl.DigitalSignatureAction.Signing)
            {
                PDFViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            }
            else if (e == Common.CPDFDigitalSignatureBarControl.DigitalSignatureAction.VerifySignature)
            {
                ToggleButton button = sender as ToggleButton;
                button.IsChecked = false;
                SignatureStatusChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Property changed event of PanelState.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.IsLeftPanelExpand))
            {
                ExpandLeftPanel(panelState.IsLeftPanelExpand);
            }
            else if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {
                    ExpandRightPropertyPanel(displaySettingsControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);

                }
            }
        }

        /// <summary>
        /// Event of UndoManager property changed.
        /// </summary>
        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }

        /// <summary>
        /// Command event of undo operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanUndo)
            {
                PDFViewControl.PDFView.UndoManager?.Undo();
            }
        }

        /// <summary>
        /// Command event of redo operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanUndo)
            {
                PDFViewControl.PDFView.UndoManager?.Redo();
            }
        }

        /// <summary>
        /// Click event of undo button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanUndo)
            {
                PDFViewControl.PDFView.UndoManager?.Undo();
            }
        }

        /// <summary>
        /// Click event of redo button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanRedo)
            {
                PDFViewControl.PDFView.UndoManager?.Redo();
            }
        }
        
        #endregion

        #region ContextMenu
        
        /// <summary>
        /// Right click context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PDFView_AnnotCommandHandler(object sender, AnnotCommandArgs e)
        {
            if (e != null && e.CommandType == CommandType.Context)
            {
                if (e.CommandTarget == TargetType.WidgetView)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    var sign = e.Sign.GetSignature(PDFViewControl.PDFView.Document);
                    if (e.Sign.IsSigned() && sign != null && sign.SignerList.Any())
                    {
                        MenuItem DeleteMenu = new MenuItem()
                            { Header = LanguageHelper.CommonManager.GetString("Menu_Delete") };
                        DeleteMenu.Click += (o, args) =>
                        {
                            PDFViewControl.PDFView.Document.RemoveSignature(sign, true);
                            e.Sign.ResetForm();
                            e.Sign.SetIsLocked(false);
                            PDFViewControl.PDFView.ReloadVisibleAnnots();
                            
                            PDFViewControl.PDFView.UndoManager.CanSave = true;
                            SignatureStatusChanged?.Invoke(this, null);
                        };
                        e.PopupMenu.Items.Add(DeleteMenu);
                    }
                    else
                    {
                        e.PopupMenu.Items.Add(new MenuItem()
                            { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                        e.PopupMenu.Items.Add(new MenuItem()
                            { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                        e.PopupMenu.Items.Add(new MenuItem()
                            { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    }
                }
                else if (e.PressOnSelectedText)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                }
                else
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();

                    e.PopupMenu.Items.Add(new MenuItem()
                        { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new Separator());

                    MenuItem fitWidthMenu = new MenuItem();
                    fitWidthMenu.Header = LanguageHelper.CommonManager.GetString("Menu_AutoSize");
                    fitWidthMenu.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeFitMode(FitMode.FitWidth);
                        }
                    };

                    e.PopupMenu.Items.Add(fitWidthMenu);

                    MenuItem fitSizeMenu = new MenuItem();
                    fitSizeMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RealSize");
                    fitSizeMenu.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeFitMode(FitMode.FitSize);
                        }
                    };

                    e.PopupMenu.Items.Add(fitSizeMenu);

                    MenuItem zoomInMenu = new MenuItem();
                    zoomInMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomIn");
                    zoomInMenu.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList,
                                PDFViewControl.PDFView.ZoomFactor + 0.01, true);
                            PDFViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomInMenu);

                    MenuItem zoomOutMenu = new MenuItem();
                    zoomOutMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomOut");
                    zoomOutMenu.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList,
                                PDFViewControl.PDFView.ZoomFactor - 0.01, false);
                            PDFViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomOutMenu);
                    e.PopupMenu.Items.Add(new Separator());

                    MenuItem singleView = new MenuItem();
                    singleView.Header = LanguageHelper.CommonManager.GetString("Menu_SinglePage");
                    singleView.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeViewMode(ViewMode.Single);
                        }
                    };

                    e.PopupMenu.Items.Add(singleView);

                    MenuItem singleContinuousView = new MenuItem();
                    singleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_SingleContinuous");
                    singleContinuousView.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeViewMode(ViewMode.SingleContinuous);
                        }
                    };

                    e.PopupMenu.Items.Add(singleContinuousView);

                    MenuItem doubleView = new MenuItem();
                    doubleView.Header = LanguageHelper.CommonManager.GetString("Menu_DoublePage");
                    doubleView.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeViewMode(ViewMode.Double);
                        }
                    };

                    e.PopupMenu.Items.Add(doubleView);

                    MenuItem doubleContinuousView = new MenuItem();
                    doubleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_DoubleContinuous");
                    doubleContinuousView.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ChangeViewMode(ViewMode.DoubleContinuous);
                        }
                    };
                    e.PopupMenu.Items.Add(doubleContinuousView);

                    MenuItem resetFormMenu = new MenuItem();
                    resetFormMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Reset");
                    resetFormMenu.Click += (o, p) =>
                    {
                        if (PDFViewControl != null)
                        {
                            PDFViewControl.PDFView?.ResetForm(null);
                        }
                    };
                    e.PopupMenu.Items.Add(new Separator());
                    e.PopupMenu.Items.Add(resetFormMenu);
                }
            }
            if (e != null && e.CommandType == CommandType.Copy)
            {
                if (PDFViewControl?.PDFView == null) return;
                if (!PDFViewControl.PDFView.Document.GetPermissionsInfo().AllowsCopying)
                {
                    if(!PasswordHelper.UnlockWithOwnerPassword(PDFViewControl.PDFView.Document))
                    {
                        return;
                    }
                }
            }
            e?.DoCommand();
        }
        #endregion

        #region Load Unload Event
        
        public void UnloadEvent()
        {
            PDFViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
        }
        
        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
         
        /// <summary>
        /// Unload event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PDFViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
        }
        
        #endregion

        
    }
}
