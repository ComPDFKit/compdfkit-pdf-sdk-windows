using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ComPDFKit.Controls.Helper;
using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer;
using ComPDFKitViewer.Widget;

namespace ComPDFKit.Controls.PDFControl
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
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    return PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo;
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
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
      
                    return PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo;
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
                if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    if (PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo ||
                        PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo)
                    {
                        return true;
                    }
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
        public void InitWithPDFViewer(PDFViewControl pdfViewer)
        {
            PDFViewControl = pdfViewer;
            PDFGrid.Child = PDFViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            
            PDFViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
            PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager.ClearHistory();
            
            DigitalSignatureBarControl.DigitalSignatureActionChanged -= DigitalSignatureBarControl_DigitalSignatureActionChanged;
            DigitalSignatureBarControl.DigitalSignatureActionChanged += DigitalSignatureBarControl_DigitalSignatureActionChanged;
            
            PDFViewControl.PDFViewTool.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            PDFViewControl.PDFViewTool.MouseRightButtonDownHandler += PDFViewControl_MouseRightButtonDownHandler;
            
            PDFViewControl.PDFViewTool.MouseLeftButtonDownHandler -= PDFViewControl_MouseLeftButtonDownHandler;
            PDFViewControl.PDFViewTool.MouseLeftButtonDownHandler += PDFViewControl_MouseLeftButtonDownHandler;
            
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        private void PDFViewControl_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            ContextMenu ContextMenu = PDFViewControl.GetRightMenu();
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            if (PDFViewControl.PDFViewTool.GetToolType() == ToolType.WidgetEdit && e.annotType== C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                BaseWidget baseWidget = PDFViewControl.GetCacheHitTestWidget();
                if(baseWidget == null)
                {
                    return;
                }

                var widget = baseWidget.GetAnnotData().Annot as CPDFWidget;
                if (widget == null || widget.WidgetType != C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                    return;
                }
                var signatureWidget = widget as CPDFSignatureWidget;
                CPDFSignature sig = signatureWidget.GetSignature(PDFViewControl.GetCPDFViewer().GetDocument());
                if (signatureWidget.IsSigned() && sig != null && sig?.SignerList.Count > 0)
                {
                    MenuItem deleteMenu = new MenuItem()
                        { Header = LanguageHelper.CommonManager.GetString("Menu_Delete") };
                    deleteMenu.Click += (o, args) =>
                    {
                        PDFViewControl.GetCPDFViewer().GetDocument().RemoveSignature(sig, true);
                        signatureWidget.ResetForm();
                        signatureWidget.SetIsLocked(false);
                        //PDFViewControl.PDFView.ReloadVisibleAnnots();
                        //PDFViewControl.GetCPDFViewer().UpdateRenderFrame();
                        PDFViewControl.PDFViewTool.IsDocumentModified = true;
                        SignatureStatusChanged?.Invoke(this, null);
                    };
                    ContextMenu.Items.Add(deleteMenu);
                }
                else
                {
                    ContextMenu.Items.Add(new MenuItem()
                        { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    ContextMenu.Items.Add(new MenuItem()
                        { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    ContextMenu.Items.Add(new MenuItem()
                        { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                }
                
            }
            else if(e.hitTestType == MouseHitTestType.Text)
            {
                ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                PDFViewControl.SetRightMenu(ContextMenu);
            }
            else
            {
                ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                PDFViewControl.CreateViewerMenu(sender, ref ContextMenu);
            }

            PDFViewControl.SetRightMenu(ContextMenu);
        }

        private void PDFViewControl_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            if (PDFViewControl.PDFViewTool.GetToolType() == ToolType.WidgetEdit && e.annotType== C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                BaseWidget baseWidget = PDFViewControl.GetCacheHitTestWidget();
                if(baseWidget == null)
                {
                    return;
                }

                var widget = baseWidget.GetAnnotData().Annot as CPDFWidget;
                if (widget == null)
                {
                    return;
                }

                if (widget.WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                    var signatureWidget = widget as CPDFSignatureWidget;
                    CPDFSignature sig = signatureWidget.GetSignature(PDFViewControl.GetCPDFViewer().GetDocument());
                    if (signatureWidget.IsSigned() && sig != null && sig?.SignerList.Count > 0)
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
            }
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
        
        public void UnloadEvent()
        {
            PDFViewControl.PDFViewTool.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            PDFViewControl.PDFViewTool.MouseLeftButtonDownHandler -= PDFViewControl_MouseLeftButtonDownHandler;
            DigitalSignatureBarControl.DigitalSignatureActionChanged -= DigitalSignatureBarControl_DigitalSignatureActionChanged;
            panelState.PropertyChanged -= PanelState_PropertyChanged;
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
            PDFViewControl.PDFToolManager.SetToolType(ToolType.WidgetEdit);
            PDFViewControl.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS);
            
            SignatureParam signatureParam = new SignatureParam
            {
                FieldName = "Signature" + GetTime(),
                CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET,
                WidgetType = C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS,
                LineWidth = 1,
                FontName = "Helvetica",
                LineColor =new byte[] {0,0,0 },
                FontColor = new byte[] { 0, 0, 0 },
                Transparency = 255,
                HasLineColor = true,
            };
            PDFViewControl.SetAnnotParam(signatureParam);
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
        //private void PDFView_WidgetClickHandler(object sender, WidgetArgs e)
        //{
        //    var signatureWidget = (e as WidgetSignArgs).Sign;
        //    CPDFSignature sig = signatureWidget.GetSignature(PDFViewControl.PDFView.Document);
        //    if (signatureWidget.IsSigned() && sig!=null && sig?.SignerList.Count > 0)
        //    {
        //        ViewSignatureEvent(sender, sig);
        //    }
        //    else
        //    {
        //        Window parentWindow = Window.GetWindow((DependencyObject)sender);
        //        AddCertificationDialog addCertificationControl = new AddCertificationDialog
        //        {
        //            Owner = parentWindow
        //        };
        //        currentSignatureWidget = signatureWidget;
        //        addCertificationControl.FillSignatureEvent -= AddCertificationControl_FillSignatureEvent;
        //        addCertificationControl.FillSignatureEvent += AddCertificationControl_FillSignatureEvent;
        //        addCertificationControl.ShowDialog(); 
        //    }
        //}

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
                Document = PDFViewControl.GetCPDFViewer().GetDocument(),
                Owner = Window.GetWindow(this)
            };
            fillDigitalSignatureDialog.AfterFillSignature -= FillDigitalSignatureDialog_AfterFillSignature; ;
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
                PDFViewControl.PDFToolManager.SetToolType(ToolType.Viewer);
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
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanUndo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
            }
        }

        /// <summary>
        /// Command event of redo operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanUndo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
            }
        }

        /// <summary>
        /// Click event of undo button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanUndo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
            }
        }

        /// <summary>
        /// Click event of redo button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFViewTool.GetCPDFViewer() != null && CanRedo)
            {
                PDFViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
            }
        }
        
        #endregion

        #region ContextMenu
        
        /// <summary>
        /// Right click context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PDFView_AnnotCommandHandler(object sender, MouseEventObject e)
        {
            ContextMenu ContextMenu = PDFViewControl.GetRightMenu();
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            switch (e.hitTestType)
            {
                case MouseHitTestType.Annot:
                case MouseHitTestType.SelectRect:
                    break;
                case MouseHitTestType.Text:
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    break;
                default:
                    PDFViewControl.CreateViewerMenu(sender, ref ContextMenu);
                    break;
            }
            PDFViewControl.SetRightMenu(ContextMenu);
        }
        #endregion
    }
}
