using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComPDFKitViewer;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFAnnotation;
using ComPDFKitViewer.Widget;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class FormControl : UserControl, INotifyPropertyChanged
    {
        #region Property

        public PDFViewControl PdfViewControl;
        public FromPropertyControl FromPropertyControl = new FromPropertyControl();
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        public event PropertyChangedEventHandler PropertyChanged;
        private CPDFDisplaySettingsControl displaySettingsControl;

        private PanelState panelState = PanelState.GetInstance();

        public bool CanUndo
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    return PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo;
                }
                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    return PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo;
                }

                return false;
            }
        }
        public bool CanSave
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    if (PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo ||
                        PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo)
                    {
                        return true;
                    }
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
            PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
            PdfViewControl.MouseRightButtonDownHandler += PdfViewControl_MouseRightButtonDownHandler;
            InitialPDFViewControl(PdfViewControl);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
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

        private void PDFToolManager_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (e.IsCreate&& e.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                CPDFAnnotation baseWidget = PdfViewControl.PDFToolManager.GetCPDFAnnotation();
                if (baseWidget != null)
                {
                    CPDFWidget  widget =(baseWidget as CPDFWidget);
                    switch (widget.WidgetType)
                    {
                        case C_WIDGET_TYPE.WIDGET_NONE:
                            break;
                        case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                            widget.SetFieldName("Button" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                            widget.SetFieldName("Checkbox" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                            widget.SetFieldName("Radio button");
                            break;
                        case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                            widget.SetFieldName("Text" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                            widget.SetFieldName("Combobox" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_LISTBOX:
                            widget.SetFieldName("List" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                            widget.SetFieldName("Signature" + GetTime());
                            break;
                        case C_WIDGET_TYPE.WIDGET_UNKNOWN:
                            break;
                        default:
                            break;
                    }
                    PdfViewControl.UpdateAnnotFrame();
                }
            }
         }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            if (e.annotType== C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                BaseWidget baseWidget = PdfViewControl.GetCacheHitTestWidget();
                if (baseWidget!=null)
                {
                    switch ((baseWidget.GetAnnotData().Annot as CPDFWidget).WidgetType)
                    {
                        case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                        case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                        case C_WIDGET_TYPE.WIDGET_LISTBOX:
                            panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                            break;
                        default:
                            break;
                    }
                    AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                        PdfViewControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                        baseWidget.GetAnnotData().PageIndex,
                        baseWidget.GetAnnotData().Annot);

                    FromPropertyControl.SetPropertyForType(
                        annotParam, 
                        baseWidget.GetAnnotData().Annot, 
                        PdfViewControl.PDFViewTool.GetCPDFViewer().GetDocument());
                } 
            }
        }

        private string GetTime()
        {
            DateTime dateTime = DateTime.Now;
            return " " + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
         
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
                PdfViewControl.PDFToolManager.ClearSelect();
                PdfViewControl.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
                PdfViewControl.PDFToolManager.ClearSelect();
                PdfViewControl.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
            }
        }

        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null && CanUndo)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Undo();
            }
        }

        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null && CanRedo)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager?.Redo();
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

        public void InitWithPDFViewer(PDFViewControl pDFViewControl)
        {
            PdfViewControl = pDFViewControl;

            PdfViewControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;

            PdfViewControl.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            PDFFormTool.InitWithPDFViewer(pDFViewControl, FromPropertyControl);
            FloatPageTool.InitWithPDFViewer(pDFViewControl);
            PDFGrid.Child = PdfViewControl;

            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;

            OnPropertyChanged("CanRedo");
            OnPropertyChanged("CanUndo");
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
            PDFFormTool.InitWithPDFViewer(newPDFViewer, FromPropertyControl);
            FromPropertyControl.SetPDFViewer(newPDFViewer);
            PDFFormTool.ClearAllToolState();
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged += UndoManager_PropertyChanged;

            newPDFViewer.CustomSignHandle = true;
        }

        private void PdfViewControl_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            ContextMenu ContextMenu = PdfViewControl.GetRightMenu();
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            switch (e.hitTestType)
            {
                case MouseHitTestType.Annot:
                case MouseHitTestType.SelectRect:
                    CreateFormContextMenu(sender, ref ContextMenu, e.annotType);
                    break;
                default:
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    PdfViewControl.CreateViewerMenu(sender, ref ContextMenu);
                    break;
            }
            PdfViewControl.SetRightMenu(ContextMenu);
        }
        private void CreateFormContextMenu(object sender, ref ContextMenu menu, C_ANNOTATION_TYPE annotType)
        {
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    break;
                default:
                    break;
            }
        }

        public void UnloadEvent()
        {
            PdfViewControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            panelState.PropertyChanged -= PanelState_PropertyChanged;
        }

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
            PdfViewControl.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_NONE);
        }

        #endregion
         
    }
}
