using System.Windows.Controls;
using Compdfkit_Tools.Data;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using Compdfkit_Tools.Helper;
using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation.Form;
using PasswordBoxPlus.Helper;
using ComPDFKit.NativeMethod;

namespace Compdfkit_Tools.PDFControl
{
    public partial class AnnotationControl : UserControl, INotifyPropertyChanged
    {
        #region Property
        private bool isFirstLoad = true;
        public PDFViewControl PDFViewControl = new PDFViewControl();
        public CPDFAnnotationControl PDFAnnotationControl = null;
        private CPDFDisplaySettingsControl displaySettingsControl = null;

        private PanelState panelState = PanelState.GetInstance();

        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand CloseTabCommand;
        public ICommand ExpandPropertyPanelCommand;

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

        private bool CanSave
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

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;

        #endregion

        public AnnotationControl()
        {
            InitializeComponent();
            DataContext = this;
            PDFAnnotationControl = new CPDFAnnotationControl();
            CPDFAnnotationType[] annotationProperties =
            {
                    CPDFAnnotationType.Highlight, CPDFAnnotationType.Underline, CPDFAnnotationType.Strikeout,
                    CPDFAnnotationType.Squiggly, CPDFAnnotationType.Freehand, CPDFAnnotationType.FreeText,
                    CPDFAnnotationType.Note, CPDFAnnotationType.Circle, CPDFAnnotationType.Square,
                    CPDFAnnotationType.Arrow, CPDFAnnotationType.Line, CPDFAnnotationType.Image,
                    CPDFAnnotationType.Stamp, CPDFAnnotationType.Signature, CPDFAnnotationType.Link,
                    CPDFAnnotationType.Audio
            };
            AnnotationBarControl.InitAnnotationBar(annotationProperties);
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;

            UndoBtn.ToolTip = LanguageHelper.CommonManager.GetString("Tooltip_Undo");
            RedoBtn.ToolTip = LanguageHelper.CommonManager.GetString("Tooltip_Redo");
        }

        #region Init PDFViewer
        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            PDFViewControl.PDFView = pdfViewer;
            PDFGrid.Child = PDFViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            InitialPDFViewControl(PDFViewControl);
        }
        #endregion

        #region Public Method

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
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
            this.AnnotationBarControl.ClearAllToolState();
        }

        public void SetToolBarContainerVisibility(Visibility visibility)
        {
            this.ToolBarContainer.Visibility = visibility;
        }

        #endregion

        #region Load Unload custom control

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitialPDFViewControl(PDFViewControl);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PDFViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
        }


        private void AnnotationBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationBarControl.AnnotationPropertyChanged += AnnotationBarControl_AnnotationPropertyChanged;
            AnnotationBarControl.AnnotationCancel += AnnotationBarControl_AnnotationCancel;
        }

        private void AnnotationBarControl_Unloaded(object sender, RoutedEventArgs e)
        {
            AnnotationBarControl.AnnotationPropertyChanged -= AnnotationBarControl_AnnotationPropertyChanged;
            AnnotationBarControl.AnnotationCancel -= AnnotationBarControl_AnnotationCancel;

        }

        #endregion

        #region Annotation

        public void InitialPDFViewControl(PDFViewControl newPDFViewer)
        {
            PDFAnnotationControl.SetPDFViewer(newPDFViewer.PDFView);
            PDFAnnotationControl.AnnotationCancel();
            AnnotationBarControl.ClearAllToolState();
            ExpandRightPropertyPanel(null, Visibility.Collapsed);
            PDFAnnotationControl.ClearAnnotationBar -= PdfAnnotationControl_ClearAnnotationBar;
            PDFAnnotationControl.ClearAnnotationBar += PdfAnnotationControl_ClearAnnotationBar;
            PDFViewControl.PDFView.AnnotEditHandler -= PDFView_AnnotEditHandler;
            PDFViewControl.PDFView.AnnotEditHandler += PDFView_AnnotEditHandler;
            PDFViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PDFViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            PDFViewControl.PDFView.AnnotActiveHandler -= PDFView_AnnotActiveHandler;
            PDFViewControl.PDFView.AnnotActiveHandler += PDFView_AnnotActiveHandler;
            
            PDFViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.WidgetClickHandler -= PDFView_WidgetClickHandler;
            PDFViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
            PDFViewControl.PDFView.WidgetClickHandler += PDFView_WidgetClickHandler;
        }

        public void UnloadEvent()
        {
            PDFViewControl.PDFView.AnnotEditHandler -= PDFView_AnnotEditHandler;
            PDFViewControl.PDFView.AnnotActiveHandler -= PDFView_AnnotActiveHandler;
            //panelState.PropertyChanged -= PanelState_PropertyChanged;
        }

        private void PdfAnnotationControl_ClearAnnotationBar(object sender, EventArgs e)
        {
            AnnotationBarControl.ClearAllToolState();
        }

        public void SetViewSettings(Visibility visibility, CPDFDisplaySettingsControl displaySettingsControl = null)
        {
            this.PropertyContainer.Child = displaySettingsControl;
            this.PropertyContainer.Visibility = visibility;
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

        #region Context menu

        private void PDFView_AnnotCommandHandler(object sender, AnnotCommandArgs e)
        {
            switch (e.CommandType)
            {
                case CommandType.Context:
                    e.Handle = true;
                    if (e.CommandTarget == TargetType.Annot)
                    {
                        e.Handle = true;
                        e.PopupMenu = new ContextMenu();
                        if (e.PressOnLink && AnnotationBarControl.CurrentMode == "Link")
                        {
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                            MenuItem propertyMenu = new MenuItem();
                            propertyMenu = new MenuItem();
                            propertyMenu.Header = "Edit";
                            WeakEventManager<MenuItem, RoutedEventArgs>.AddHandler(propertyMenu, "Click", EditLink_Click);
                            propertyMenu.CommandParameter = e;
                            e.PopupMenu.Items.Add(propertyMenu);
                        }
                        else if (e.PressOnAnnot)
                        {
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                        }
                        else if (e.PressOnMedia || e.PressOnSound)
                        {
                            e.Handle = true;
                            e.PopupMenu.Items.Add(new MenuItem() { Header = "Play", Command = MediaCommands.Play, CommandTarget = (UIElement)sender, CommandParameter = e });
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                        }
                        else if (e.PressOnSelectedText)
                        {
                            e.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                            MenuItem highLightMenu = new MenuItem();
                            highLightMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Highlight");
                            highLightMenu.Click += (o, p) =>
                            {
                                TextHighlightAnnotArgs highLightArgs = new TextHighlightAnnotArgs();
                                MouseModes oldMode = PDFViewControl.PDFView.MouseMode;

                                if (PDFAnnotationControl != null)
                                {
                                    highLightArgs.Color = System.Windows.Media.Colors.Red;
                                    highLightArgs.Transparency = 1;
                                    PDFViewControl.PDFView.SetMouseMode(MouseModes.AnnotCreate);
                                    PDFViewControl.PDFView.SetToolParam(highLightArgs);
                                    PDFViewControl.PDFView.SetMouseMode(oldMode);
                                }

                            };

                            e.PopupMenu.Items.Add(highLightMenu);

                            MenuItem underlineMenu = new MenuItem();
                            underlineMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Underline");
                            underlineMenu.Click += (o, p) =>
                            {
                                TextUnderlineAnnotArgs underlineArgs = new TextUnderlineAnnotArgs();
                                MouseModes oldMode = PDFViewControl.PDFView.MouseMode;

                                if (PDFAnnotationControl != null)
                                {
                                    underlineArgs.Color = System.Windows.Media.Colors.Red;
                                    underlineArgs.Transparency = 1;
                                    PDFViewControl.PDFView.SetMouseMode(MouseModes.AnnotCreate);
                                    PDFViewControl.PDFView.SetToolParam(underlineArgs);
                                    PDFViewControl.PDFView.SetMouseMode(oldMode);
                                }
                            };

                            e.PopupMenu.Items.Add(underlineMenu);

                            MenuItem strikeOutMenu = new MenuItem();
                            strikeOutMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Strikeout");
                            strikeOutMenu.Click += (o, p) =>
                            {
                                TextStrikeoutAnnotArgs strikeoutAnnotArgs = new TextStrikeoutAnnotArgs();
                                MouseModes oldMode = PDFViewControl.PDFView.MouseMode;

                                if (PDFAnnotationControl != null)
                                {
                                    strikeoutAnnotArgs.Color = System.Windows.Media.Colors.Red;
                                    strikeoutAnnotArgs.Transparency = 1;
                                    PDFViewControl.PDFView.SetMouseMode(MouseModes.AnnotCreate);
                                    PDFViewControl.PDFView.SetToolParam(strikeoutAnnotArgs);
                                    PDFViewControl.PDFView.SetMouseMode(oldMode);
                                }
                            };

                            e.PopupMenu.Items.Add(strikeOutMenu);

                            MenuItem SquiggleMenu = new MenuItem();
                            SquiggleMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Squiggly");
                            SquiggleMenu.Click += (o, p) =>
                            {
                                TextSquigglyAnnotArgs squigglyAnnotArgs = new TextSquigglyAnnotArgs();
                                MouseModes oldMode = PDFViewControl.PDFView.MouseMode;

                                if (PDFAnnotationControl != null)
                                {
                                    squigglyAnnotArgs.Color = System.Windows.Media.Colors.Red;
                                    squigglyAnnotArgs.Transparency = 1;
                                    PDFViewControl.PDFView.SetMouseMode(MouseModes.AnnotCreate);
                                    PDFViewControl.PDFView.SetToolParam(squigglyAnnotArgs);
                                    PDFViewControl.PDFView.SetMouseMode(oldMode);
                                }
                            };

                            e.PopupMenu.Items.Add(SquiggleMenu);
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
                                    double newZoom = CheckZoomLevel(PDFViewControl.PDFView.ZoomFactor + 0.01, true);
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
                                    double newZoom = CheckZoomLevel(PDFViewControl.PDFView.ZoomFactor - 0.01, false);
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

                    else if (e.CommandTarget == TargetType.ImageSelection)
                    {
                        if (PDFViewControl != null && PDFViewControl.PDFView != null && PDFViewControl.PDFView.GetSelectImageCount() > 0)
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
                    break;
                case CommandType.Copy:
                {
                    if (PDFViewControl == null) return;
                    if (!PDFViewControl.PDFView.Document.GetPermissionsInfo().AllowsCopying)
                    {
                        if(!PasswordHelper.UnlockWithOwnerPassword(PDFViewControl.PDFView.Document))
                        {
                            return;
                        }
                    }
                    e.DoCommand();
                    break;
                }
                    
                case CommandType.Cut:
                case CommandType.Paste:
                case CommandType.Delete:
                    e.DoCommand();
                    break;
                default:
                    break;
            }
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<int, List<Bitmap>> imageDict = PDFViewControl.PDFView?.GetSelectedImages();

                if (imageDict != null && imageDict.Count > 0)
                {
                    foreach (int pageIndex in imageDict.Keys)
                    {
                        List<Bitmap> imageList = imageDict[pageIndex];
                        foreach (Bitmap image in imageList)
                        {
                            MemoryStream ms = new MemoryStream();
                            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            BitmapImage imageData = new BitmapImage();
                            imageData.BeginInit();
                            imageData.StreamSource = ms;
                            imageData.CacheOption = BitmapCacheOption.OnLoad;
                            imageData.EndInit();
                            imageData.Freeze();
                            Clipboard.SetImage(imageData);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ExtraImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string choosePath = folderDialog.SelectedPath;
                string openPath = choosePath;
                try
                {
                    Dictionary<int, List<Bitmap>> imageDict = PDFViewControl.PDFView?.GetSelectedImages();

                    if (imageDict != null && imageDict.Count > 0)
                    {
                        foreach (int pageIndex in imageDict.Keys)
                        {
                            List<Bitmap> imageList = imageDict[pageIndex];
                            foreach (Bitmap image in imageList)
                            {
                                string savePath = Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                                image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                openPath = savePath;
                            }
                        }
                    }
                    Process.Start("explorer", "/select,\"" + openPath + "\"");
                }
                catch (Exception ex)
                {

                }
            }
        }

        #endregion

        #region UI

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

        private void ToolExpand_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton expandBtn = sender as ToggleButton;
            if (expandBtn != null)
            {
                bool isExpand = expandBtn.IsChecked == true;
                ExpandLeftPanel(isExpand);
            }
        }

        private void EditLink_Click(object sender, RoutedEventArgs e)
        {
            PropertyContainer.Visibility = Visibility.Visible;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null)
            {
                PDFViewControl.PDFView.UndoManager?.Undo();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null)
            {
                PDFViewControl.PDFView.UndoManager?.Redo();
            }
        }

        #endregion

        #region Property changed
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
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
        #endregion

        #region Event handle
        private void AnnotationBarControl_AnnotationCancel(object sender, EventArgs e)
        {
            PDFAnnotationControl.AnnotationCancel();
            if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
            {
                panelState.RightPanel = PanelState.RightPanelState.None;
            }
        }

        private void AnnotationBarControl_AnnotationPropertyChanged(object sender, CPDFAnnotationType e)
        { 
            PDFAnnotationControl.LoadAnnotationPanel(e);
            if (e != CPDFAnnotationType.Audio && e != CPDFAnnotationType.Image)
            {
                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            }
        }

        private void PDFView_AnnotActiveHandler(object sender, AnnotAttribEvent e)
        {
            PropertyContainer.Child = PDFAnnotationControl;
            PDFAnnotationControl.SetAnnotEventData(e);
        }
        
        private void PDFView_WidgetClickHandler(object sender, WidgetArgs e)
        {
            if ((e is WidgetSignArgs args))
            {
                var signatureWidget = args.Sign;
                if(signatureWidget != null)
                {
                    CPDFSignature sig = signatureWidget.GetSignature(PDFViewControl.PDFView.Document);
                    if (signatureWidget.IsSigned() && sig!=null && sig?.SignerList.Count > 0)
                    {
                        return;
                    }
                }
                
                if (args.WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                    panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                    CPDFSignatureUI signatureProperty = new CPDFSignatureUI();
                    signatureProperty.SetFormProperty(args, PDFViewControl.PDFView);
                    PropertyContainer.Child = signatureProperty;
                }
            }
        }

        private void PDFView_AnnotEditHandler(object sender, List<AnnotEditEvent> e)
        {
            OnAnnotEditHandler.Invoke(this, null);
        }

        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanUndo)
            {
                PDFViewControl.PDFView.UndoManager?.Undo();
            }
        }

        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PDFViewControl != null && PDFViewControl.PDFView != null && CanRedo)
            {
                PDFViewControl.PDFView.UndoManager?.Redo();
            }
        }

        private void CommandBinding_Executed_Highlight(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Highlight);
        }

        private void CommandBinding_Executed_Underline(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Underline);
        }

        private void CommandBinding_Executed_Strikeout(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Strikeout);
        }

        private void CommandBinding_Executed_Squiggly(object sender, ExecutedRoutedEventArgs e)
        {
            AnnotationBarControl.SetAnnotationType(CPDFAnnotationType.Squiggly);
        }
        #endregion
        
    }
}