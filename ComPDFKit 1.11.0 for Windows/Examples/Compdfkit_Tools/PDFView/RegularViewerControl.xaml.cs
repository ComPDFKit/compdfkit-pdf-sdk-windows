using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using PasswordBoxPlus.Helper;

namespace Compdfkit_Tools.PDFView
{
    public partial class RegularViewerControl : UserControl, INotifyPropertyChanged
    {
        public PDFViewControl PdfViewControl = new PDFViewControl();
        public CPDFAnnotationControl PDFAnnotationControl = new CPDFAnnotationControl();
        private SignatureStatusBarControl signatureStatusBarControl;
        private CPDFDisplaySettingsControl displaySettingsControl = null;
        private PanelState panelState = PanelState.GetInstance();
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> OnCanSaveChanged;

        private bool CanSave
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFView != null)
                {
                    return PdfViewControl.PDFView.UndoManager.CanSave;
                }

                return false;
            }
        }

        public RegularViewerControl()
        {
            InitializeComponent();
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.IsLeftPanelExpand))
            {
                ExpandLeftPanel(panelState.IsLeftPanelExpand);
            }
            else if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
                {
                    ExpandRightPropertyPanel(PDFAnnotationControl, Visibility.Visible);
                }
                else if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {
                    ExpandRightPropertyPanel(displaySettingsControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);

                }
            }
        }

        public void ExpandLeftPanel(bool isExpand)
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

        public void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        #region Init PDFViewer

        private void InitialControl()
        {
            PdfViewControl.PDFView?.SetMouseMode(MouseModes.Viewer);
            PdfViewControl.PDFView?.SetShowLink(true);
            PDFGrid.Child = PdfViewControl;
            PdfViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            PdfViewControl.PDFView.SetFormFieldHighlight(true);
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            PdfViewControl.PDFView = pdfViewer;
            PDFGrid.Child = PdfViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            InitialControl();
            DataContext = this;
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
                PdfViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
                PdfViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
                PdfViewControl.PDFView.WidgetClickHandler += PDFView_WidgetClickHandler;
            }
        }

        private void PDFView_WidgetClickHandler(object sender, WidgetArgs e)
        {
            if ((e is WidgetSignArgs args))
            {
                var signatureWidget = args.Sign;
                if (signatureWidget != null)
                {
                    CPDFSignature sig = signatureWidget.GetSignature(PdfViewControl.PDFView.Document);
                    if (signatureWidget.IsSigned() && sig != null && sig?.SignerList.Count > 0)
                    {
                        return;
                    }
                }

                if (args.WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                    panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                    CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                    signatureProperty.SetFormProperty(args, PdfViewControl.PDFView);
                    PropertyContainer.Child = signatureProperty;
                }
            }
        }

        public void CancelWidgetClickHandler()
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
            }
        }

        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
        }

        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

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

        #endregion

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            SignatureStatusBorder.Child = null;
            displaySettingsControl = null;
        }

        #region PropertyChanged

        /// <summary>
        /// Undo Redo Event Noitfy
        /// </summary>
        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Context Menu

        private void ExtraImage_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.ExtraImage_Click(PdfViewControl.PDFView.GetSelectedImages());
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.CopyImage_Click(PdfViewControl.PDFView.GetSelectedImages());
        }

        private void PDFView_AnnotCommandHandler(object sender, AnnotCommandArgs e)
        {
            if (e != null && e.CommandType == CommandType.Context)
            {
                if (e.PressOnSelectedText)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                }
                else if (e.CommandTarget == TargetType.ImageSelection)
                {
                    if (PdfViewControl?.PDFView != null && PdfViewControl.PDFView.GetSelectImageCount() > 0)
                    {
                        e.Handle = true;
                        e.PopupMenu = new ContextMenu();

                        MenuItem imageCopyMenu = new MenuItem();
                        imageCopyMenu = new MenuItem();
                        imageCopyMenu.Header = "Copy Images";
                        WeakEventManager<MenuItem, RoutedEventArgs>.AddHandler(imageCopyMenu, "Click", CopyImage_Click);
                        imageCopyMenu.CommandParameter = e;
                        e.PopupMenu.Items.Add(imageCopyMenu);

                        MenuItem imageExtraMenu = new MenuItem();
                        imageExtraMenu = new MenuItem();
                        imageExtraMenu.Header = "Extract Images";
                        WeakEventManager<MenuItem, RoutedEventArgs>.AddHandler(imageExtraMenu, "Click", ExtraImage_Click);
                        imageExtraMenu.CommandParameter = e;
                        e.PopupMenu.Items.Add(imageExtraMenu);
                    }
                }
                else
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    //if (PdfViewControl.CheckHasForm())

                    MenuItem fitWidthMenu = new MenuItem();
                    fitWidthMenu.Header = LanguageHelper.CommonManager.GetString("Menu_AutoSize");
                    fitWidthMenu.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeFitMode(FitMode.FitWidth);
                        }
                    };

                    e.PopupMenu.Items.Add(fitWidthMenu);

                    MenuItem fitSizeMenu = new MenuItem();
                    fitSizeMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RealSize");
                    fitSizeMenu.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeFitMode(FitMode.FitSize);
                        }
                    };

                    e.PopupMenu.Items.Add(fitSizeMenu);

                    MenuItem zoomInMenu = new MenuItem();
                    zoomInMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomIn");
                    zoomInMenu.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList, PdfViewControl.PDFView.ZoomFactor + 0.01, true);
                            PdfViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomInMenu);

                    MenuItem zoomOutMenu = new MenuItem();
                    zoomOutMenu.Header = LanguageHelper.CommonManager.GetString("Menu_ZoomOut");
                    zoomOutMenu.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList, PdfViewControl.PDFView.ZoomFactor - 0.01, false);
                            PdfViewControl.PDFView?.Zoom(newZoom);
                        }
                    };

                    e.PopupMenu.Items.Add(zoomOutMenu);
                    e.PopupMenu.Items.Add(new Separator());

                    MenuItem singleView = new MenuItem();
                    singleView.Header = LanguageHelper.CommonManager.GetString("Menu_SinglePage");
                    singleView.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeViewMode(ViewMode.Single);
                        }
                    };

                    e.PopupMenu.Items.Add(singleView);

                    MenuItem singleContinuousView = new MenuItem();
                    singleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_SingleContinuous");
                    singleContinuousView.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeViewMode(ViewMode.SingleContinuous);
                        }
                    };

                    e.PopupMenu.Items.Add(singleContinuousView);

                    MenuItem doubleView = new MenuItem();
                    doubleView.Header = LanguageHelper.CommonManager.GetString("Menu_DoublePage");
                    doubleView.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeViewMode(ViewMode.Double);
                        }
                    };

                    e.PopupMenu.Items.Add(doubleView);

                    MenuItem doubleContinuousView = new MenuItem();
                    doubleContinuousView.Header = LanguageHelper.CommonManager.GetString("Menu_DoubleContinuous");
                    doubleContinuousView.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ChangeViewMode(ViewMode.DoubleContinuous);
                        }
                    };
                    e.PopupMenu.Items.Add(doubleContinuousView);

                    {
                        MenuItem resetForms = new MenuItem();
                        resetForms.Header = LanguageHelper.CommonManager.GetString("Menu_Reset");
                        resetForms.Click += (o, p) =>
                        {
                            if (PdfViewControl != null)
                            {
                                PdfViewControl.PDFView?.ResetForm(null);
                            }
                        };
                        e.PopupMenu.Items.Add(new Separator());
                        e.PopupMenu.Items.Add(resetForms);
                    }
                }
            }

            if (e != null && e.CommandType == CommandType.Copy)
            {
                if (PdfViewControl?.PDFView == null) return;
                if (!PdfViewControl.PDFView.Document.GetPermissionsInfo().AllowsCopying)
                {
                    if(!PasswordHelper.UnlockWithOwnerPassword(PdfViewControl.PDFView.Document))
                    {
                        return;
                    }
                }
                e.DoCommand();
            }
        }

        private void PopupMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PdfViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
        }
    }
}