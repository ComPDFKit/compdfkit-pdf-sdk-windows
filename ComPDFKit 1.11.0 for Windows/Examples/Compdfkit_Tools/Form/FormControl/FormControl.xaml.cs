using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using Compdfkit_Tools.Helper;
using PasswordBoxPlus.Helper;
using Path = System.Windows.Shapes.Path;

namespace Compdfkit_Tools.PDFControl
{
    public partial class FormControl : UserControl, INotifyPropertyChanged
    {
        #region Property

        public PDFViewControl PdfViewControl = new PDFViewControl();
        public FromPropertyControl FromPropertyControl = new FromPropertyControl();
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        public event PropertyChangedEventHandler PropertyChanged;
        private CPDFDisplaySettingsControl displaySettingsControl;

        private PanelState panelState = PanelState.GetInstance();

        public bool CanUndo
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFView != null)
                {
                    return PdfViewControl.PDFView.UndoManager.CanUndo;
                }
                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFView != null)
                {
                    return PdfViewControl.PDFView.UndoManager.CanRedo;
                }

                return false; 
            }
        }
        public bool CanSave
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

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;

        #endregion

        public FormControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        #region Load Unload custom control
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitialPDFViewControl(PdfViewControl);
            PdfViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
        }
         
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
        }

        #endregion

        #region Expand and collapse Panel

        public void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible; 
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

        #endregion

        #region Private Command Method
        private void PDFView_WidgetClickHandler(object sender, WidgetArgs e)
        {
            if (e is WidgetSignArgs)
            {
                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            }
            else
            {
                panelState.RightPanel = PanelState.RightPanelState.None;
            }
        }

        private void PDFView_AnnotActiveHandler(object sender, AnnotAttribEvent e)
        {
            if (e == null || e.IsAnnotCreateReset)
            {
                FromPropertyControl.SetPropertyForType(null, null);
            }
            else
            {
                switch (e.GetAnnotTypes())
                {
                    case AnnotArgsType.WidgetViewForm:
                        WidgetArgs formArgs = e.GetAnnotHandlerEventArgs(AnnotArgsType.WidgetViewForm).First() as WidgetArgs;
                        switch (formArgs.WidgetType)
                        {
                            case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                            case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                            case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                                break;
                            default:
                                break;
                        }
                        FromPropertyControl.SetPropertyForType(formArgs, e);
                        break;
                }
            }
        }

        private string GetTime()
        {
            DateTime dateTime = DateTime.Now;
            return " " + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void PDFView_AnnotEditHandler(object sender, List<AnnotEditEvent> e)
        {
            if (e != null && e.Count > 0)
            {
                AnnotEditEvent editEvent = e[e.Count - 1];
                if (editEvent.EditAction == ActionType.Add)
                {
                    if (PdfViewControl.PDFView.ToolManager.CurrentAnnotArgs != null)
                    {
                        WidgetArgs widgetArgs = PdfViewControl.PDFView.ToolManager.CurrentAnnotArgs as WidgetArgs;
                        if (widgetArgs != null)
                        {
                            switch (widgetArgs.WidgetType)
                            {
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                                    widgetArgs.FieldName = "Button" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_CHECKBOX:
                                    widgetArgs.FieldName = "Checkbox" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                                    widgetArgs.FieldName = "Radio button" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                                    widgetArgs.FieldName = "Text" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                                    widgetArgs.FieldName = "Combobox" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                                    widgetArgs.FieldName = "List" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                                    widgetArgs.FieldName = "Signature" + GetTime();
                                    break;
                                case ComPDFKit.PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_UNKNOWN:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    PdfViewControl.PDFView.SelectAnnotation(editEvent.PageIndex, editEvent.AnnotIndex);
                }
                else if (editEvent.EditAction == ActionType.Del)
                {
                    FromPropertyControl.CleanProperty();
                }
            }
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
                    if (PdfViewControl != null && PdfViewControl.PDFView != null && PdfViewControl.PDFView.GetSelectImageCount() > 0)
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
                else if (e.CommandTarget == TargetType.WidgetView)
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                }
                else
                {
                    e.Handle = true;
                    e.PopupMenu = new ContextMenu();

                    e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    e.PopupMenu.Items.Add(new Separator());

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
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList,PdfViewControl.PDFView.ZoomFactor + 0.01, true);
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
                            double newZoom = CommandHelper.CheckZoomLevel(zoomLevelList,PdfViewControl.PDFView.ZoomFactor - 0.01, false);
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

                    MenuItem resetFormMenu = new MenuItem();
                    resetFormMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Reset");
                    resetFormMenu.Click += (o, p) =>
                    {
                        if (PdfViewControl != null)
                        {
                            PdfViewControl.PDFView?.ResetForm(null);
                        }
                    };
                    e.PopupMenu.Items.Add(new Separator());
                    e.PopupMenu.Items.Add(resetFormMenu);
                }
            }
            else
            {
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
                }
                e?.DoCommand();
            }
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.CopyImage_Click(PdfViewControl.PDFView.GetSelectedImages());
        }

        private void ExtraImage_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.ExtraImage_Click(PdfViewControl.PDFView.GetSelectedImages());
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.UndoManager?.Undo();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.UndoManager?.Redo();
            }
        }

        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null && CanUndo)
            {
                PdfViewControl.PDFView.UndoManager?.Undo();
            }
        }

        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null && CanRedo)
            {
                PdfViewControl.PDFView.UndoManager?.Redo();
            }
        }
        
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                    ExpandRightPropertyPanel(FromPropertyControl, Visibility.Visible);
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
        
        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }
        #endregion

        #region Public Method

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            PdfViewControl.PDFView = pdfViewer;
            PDFFormTool.InitWithPDFViewer(pdfViewer, FromPropertyControl);
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            PDFGrid.Child = PdfViewControl;

            panelState.PropertyChanged -= PanelState_PropertyChanged; 
            panelState.PropertyChanged += PanelState_PropertyChanged; 
        }

        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
        }
        
        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

        public void ClearAllToolState()
        {
            this.PDFFormTool.ClearAllToolState();
        }

        public void SetToolBarContainerVisibility(Visibility visibility)
        {
            this.ToolBarContainer.Visibility = visibility;
        }
        
        public void InitialPDFViewControl(PDFViewControl newPDFViewer)
        {
            PDFFormTool.InitWithPDFViewer(newPDFViewer.PDFView, FromPropertyControl);
            FromPropertyControl.SetPDFViewer(newPDFViewer.PDFView);
            PDFFormTool.ClearAllToolState();
            newPDFViewer.PDFView.AnnotEditHandler -= PDFView_AnnotEditHandler;
            newPDFViewer.PDFView.AnnotActiveHandler -= PDFView_AnnotActiveHandler;
            newPDFViewer.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
            PdfViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            newPDFViewer.PDFView.AnnotEditHandler += PDFView_AnnotEditHandler;
            newPDFViewer.PDFView.AnnotActiveHandler += PDFView_AnnotActiveHandler;
            newPDFViewer.PDFView.WidgetClickHandler += PDFView_WidgetClickHandler;
            newPDFViewer.CustomSignHandle = true;
        }

        public void UnloadEvent()
        {
            PdfViewControl.PDFView.AnnotEditHandler -= PDFView_AnnotEditHandler;
            PdfViewControl.PDFView.AnnotActiveHandler -= PDFView_AnnotActiveHandler;
            PdfViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
            //panelState.PropertyChanged -= PanelState_PropertyChanged;
        }

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
        }

        #endregion

        
    }
}
