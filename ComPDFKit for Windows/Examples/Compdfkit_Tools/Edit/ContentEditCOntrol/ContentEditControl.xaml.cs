using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using Compdfkit_Tools.Edit;
using Compdfkit_Tools.Helper;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{
    public partial class ContentEditControl : UserControl, INotifyPropertyChanged
    {
        #region Property
        public PDFViewControl PdfViewControl = new PDFViewControl();
        public PDFContentEditControl pdfContentEditControl = new PDFContentEditControl();
        private CPDFDisplaySettingsControl displaySettingsControl = null;

        private PDFEditEvent pdfTextCreateParam;
        private PDFEditEvent lastPDFEditEvent = null;
        private List<PDFEditEvent> lastPDFEditMultiEvents = null;
        private PanelState panelState = PanelState.GetInstance();
        private KeyEventHandler KeyDownHandler;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand CloseTabCommand;
        public ICommand ExpandPropertyPanelCommand;

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

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;

        #endregion

        public ContentEditControl()
        {
            InitializeComponent();
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
                    ExpandRightPropertyPanel(pdfContentEditControl, Visibility.Visible);
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

        public void ClearPDFEditState(ToggleButton ignoreBtn = null)
        {
            List<ToggleButton> clearBtnList = new List<ToggleButton>()
            {
                PDFTextEditButton,
                PDFImageEditButton
            };

            foreach (ToggleButton item in clearBtnList)
            {
                if (ignoreBtn == item)
                {
                    continue;
                }
                item.IsChecked = false;
            }
        }
         
        private void PDFTextEditButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton senderBtn = sender as ToggleButton;
            if (senderBtn != null && PdfViewControl != null)
            {
                ClearPDFEditState(senderBtn);
                if (senderBtn.IsChecked == true)
                {
                    PDFEditEvent createParam = new PDFEditEvent();
                    createParam.EditType = CPDFEditType.EditText;
                    createParam.IsBold = false;
                    createParam.IsItalic = false;
                    createParam.FontSize = 14;
                    createParam.FontName = "Helvetica";
                    createParam.FontColor = Colors.Black;
                    createParam.TextAlign = TextAlignType.AlignLeft;
                    createParam.Transparency = 255;

                    if (PdfViewControl.PDFView != null && PdfViewControl.PDFView.Document != null)
                    {
                        CPDFDocument pdfDoc = PdfViewControl.PDFView.Document;
                        PdfViewControl.PDFView.ToolManager.EnableClickCreate = true;
                        PdfViewControl.PDFView.ToolManager.ClickCreateWidth = 100;
                        if (pdfDoc.PageCount > 0)
                        {
                            CPDFPage pdfPage = pdfDoc.PageAtIndex(0);
                            CPDFEditPage editPage = pdfPage.GetEditPage();
                            editPage.BeginEdit(CPDFEditType.EditText);
                            createParam.SystemFontNameList.AddRange(editPage.GetFontList());
                            editPage.EndEdit();
                        }
                    }
                  
                    PdfViewControl.PDFView?.SetPDFEditType(CPDFEditType.EditText);
                    PdfViewControl.PDFView?.SetPDFEditCreateType(CPDFEditType.EditText);
                    PdfViewControl.PDFView?.SetMouseMode(MouseModes.PDFEdit);
                    PdfViewControl.PDFView?.ReloadDocument();

                    PdfViewControl.PDFView?.SetPDFEditParam(createParam);
                    pdfContentEditControl.SetPDFTextEditData(createParam);
                    panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                    pdfTextCreateParam = createParam;
                }
                else
                {
                    PdfViewControl.PDFView?.SetPDFEditCreateType(CPDFEditType.None);
                    PdfViewControl.PDFView?.SetPDFEditType(CPDFEditType.EditImage | CPDFEditType.EditText);
                    PdfViewControl.PDFView?.SetMouseMode(MouseModes.PDFEdit);
                    PdfViewControl.PDFView?.ReloadDocument();
                    pdfContentEditControl.ClearContentControl();
                    panelState.RightPanel = PanelState.RightPanelState.None;
                }
            }

        }

        private void PDFImageEditButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton senderBtn = sender as ToggleButton;
            if (senderBtn != null && PdfViewControl != null)
            {
                panelState.RightPanel = PanelState.RightPanelState.None;
                senderBtn.IsChecked = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    ClearPDFEditState(senderBtn);
                    PdfViewControl.PDFView?.ClearSelectPDFEdit();
                    PdfViewControl.PDFView?.SetPDFEditType(CPDFEditType.EditImage | CPDFEditType.EditText);
                    PdfViewControl.PDFView?.SetMouseMode(MouseModes.PDFEdit);
                    PdfViewControl.PDFView?.ReloadDocument();
                     
                    PdfViewControl.PDFView?.SetPDFEditCreateType(CPDFEditType.EditImage);
                    PdfViewControl.PDFView?.AddPDFEditImage(openFileDialog.FileName); 
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

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.UndoManager?.Undo();
            }
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                PdfViewControl.PDFView.UndoManager?.Redo();
            }
        }

        public void SetViewSettings(Visibility visibility, CPDFDisplaySettingsControl displaySettingsControl = null)
        {
            this.PropertyContainer.Child = displaySettingsControl;
            this.PropertyContainer.Visibility = visibility;
        }

        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
            botaControl.ReplaceFunctionEnabled = true;
        }

        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.PDFView.PDFEditCommandHandler += PDFView_PDFEditCommandHandler;
        }

        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.PDFView.PDFEditCommandHandler -= PDFView_PDFEditCommandHandler;
        }
         
        public void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            PdfViewControl.PDFView = pdfViewer;
            PDFGrid.Child = PdfViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            pdfContentEditControl.InitWithPDFViewer(pdfViewer);
            PdfViewControl.PDFView.PDFEditActiveHandler -= PDFView_PDFEditActiveHandler; 
            PdfViewControl.PDFView.PDFEditActiveHandler += PDFView_PDFEditActiveHandler;
            PdfViewControl.PDFView.UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFView.UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            DataContext = this;
            if(pdfViewer!=null)
            {
                pdfViewer.EnableMultiSelectEdit = true;
                pdfViewer.PDFEditMultiActiveHandler -= PDFEditMultiActiveHandler;
                pdfViewer.PDFEditMultiActiveHandler += PDFEditMultiActiveHandler;
                if (KeyDownHandler != null)
                {
                    pdfViewer.RemoveHandler(KeyDownEvent, KeyDownHandler);
                }
                KeyDownHandler = new KeyEventHandler(PDFView_KeyDown);
                pdfViewer.AddHandler(KeyDownEvent, KeyDownHandler, false, true);
            }
        }

        private void PDFEditEmptyPanel()
        {
            PropertyContainer.Child = pdfContentEditControl;

            if (pdfTextCreateParam != null && PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                if (PdfViewControl.PDFView.GetPDFEditCreateType() == CPDFEditType.EditText)
                {
                    pdfContentEditControl.SetPDFTextEditData(pdfTextCreateParam);
                }
                else if (PdfViewControl.PDFView.GetPDFEditCreateType() == CPDFEditType.None)
                {
                    pdfContentEditControl.ClearContentControl();

                }
            }
            else
            {
                pdfContentEditControl.ClearContentControl();
            }
        }

        private void PDFEditMultiActiveHandler(object sender, List<PDFEditEvent> e)
        {
            lastPDFEditEvent = null;
            lastPDFEditMultiEvents = e;
            if(e==null)
            {
                PDFEditEmptyPanel();
                return;
            }

            if(e.Count>1)
            {
               List<CPDFEditType> editList= e.AsEnumerable().Select(x=>x.EditType).Distinct().ToList();
               
                if(editList.Count>1)
                {
                    PDFEditEmptyPanel();
                    return;
                }

                if (editList[0]==CPDFEditType.EditText)
                {
                    pdfContentEditControl.SetPDFTextMultiEditData(e);
                    return;
                }

                if (editList[0]==CPDFEditType.EditImage)
                {
                    UIElement pageView = sender as UIElement;
                    if (pageView != null)
                    {
                        pageView.MouseLeftButtonUp += PageView_MouseLeftButtonUp;
                    }
                    pdfContentEditControl.SetPDFImageMultiEditData(e);
                }
            }
        }

        public void PDFView_KeyDown(object sender, KeyEventArgs e)
        {
            if (PdfViewControl.PDFView.MouseMode != MouseModes.PDFEdit)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypePreWord, false);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeNextWord, false);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeSectionBegin, false);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeSectionEnd, false);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Key == Key.Left)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypePreCharPlace, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeNextCharPlace, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeUpCharPlace, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeDownCharPlace, true);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (e.SystemKey == Key.Up)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLocationLineBegin, false);
                    e.Handled = true;
                }

                if (e.SystemKey == Key.Down)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeLineEnd, false);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Left)
                {
                    PdfViewControl.PDFView.MoveEditArea(new Point(-5, 0));
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    PdfViewControl.PDFView.MoveEditArea(new Point(5, 0));
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    PdfViewControl.PDFView.MoveEditArea(new Point(0, -5));
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    PdfViewControl.PDFView.MoveEditArea(new Point(0, 5));
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (e.Key == Key.Left)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypePreWord, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeNextWord, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeSectionBegin, true);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    PdfViewControl.PDFView.JumpEditingLoction(CEditingLocation.CEditingLoadTypeSectionEnd, true);
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// Text and Image Selected Event
        /// </summary>
        private void PDFView_PDFEditActiveHandler(object sender, PDFEditEvent e)
        {
            lastPDFEditEvent = e;
            lastPDFEditMultiEvents = null;

            if (e == null)
            {
                PropertyContainer.Child = pdfContentEditControl;

                if (pdfTextCreateParam != null && PdfViewControl != null && PdfViewControl.PDFView != null)
                {
                    if (PdfViewControl.PDFView.GetPDFEditCreateType() == CPDFEditType.EditText)
                    {
                        pdfContentEditControl.SetPDFTextEditData(pdfTextCreateParam);
                    }
                    else if (PdfViewControl.PDFView.GetPDFEditCreateType() == CPDFEditType.None)
                    {
                        pdfContentEditControl.ClearContentControl();

                    }
                }
                else
                {
                    pdfContentEditControl.ClearContentControl();
                }
                return;
            }

            if (e.EditType == CPDFEditType.EditText)
            {
                pdfContentEditControl.SetPDFTextEditData(e, true);
                return;
            }

            if (e.EditType == CPDFEditType.EditImage && PdfViewControl != null)
            {
                UIElement pageView = sender as UIElement;
                if (pageView != null)
                {
                    pageView.MouseLeftButtonUp += PageView_MouseLeftButtonUp;
                }
                pdfContentEditControl.SetPDFImageEditData(e);
                return;
            }
        }

        private void PageView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement pageView = sender as UIElement;
            if (pageView != null)
            {
                pageView.MouseLeftButtonUp -= PageView_MouseLeftButtonUp;
            }
            if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
            {
                pdfContentEditControl.SetPDFImageEditData(lastPDFEditEvent);
            }
        }
        
        public void ClearViewerControl()
        {
            if (BotaContainer.Child is CPDFBOTABarControl botaControl)
            {
                botaControl.ReplaceFunctionEnabled = false;
            }
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
        }
        
        #region Property changed
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }
        #endregion

        #region Context menu

        private void PDFView_PDFEditCommandHandler(object sender, PDFEditCommand e)
        {
            if (e == null)
            {
                return;
            }

            if (e.EditType == CPDFEditType.EditText)
            {
                e.Handle = true;
                PDFEditTextContextMenu(sender, e);
            }

            if (e.EditType == CPDFEditType.EditImage)
            {
                e.Handle = true;
                PDFEditImageContextMenu(sender, e);
            }

            if(e.EditType== (CPDFEditType.EditText | CPDFEditType.EditImage))
            {
                e.Handle |= true;
                PDFEditMultiContextMenu(sender, e);
            }
        }
        
        private void PDFEditTextContextMenu(object sender, PDFEditCommand editCommand)
        {
            editCommand.PopupMenu = new ContextMenu();
            if (lastPDFEditEvent != null || lastPDFEditMultiEvents!=null)
            {
                if(PdfViewControl.PDFView.Document.GetPermissionsInfo().AllowsCopying)
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
            }
            else
            {
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                if (editCommand.TextAreaCopied)
                {
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                }
            }
        }

        private void PDFEditImageContextMenu(object sender, PDFEditCommand editCommand)
        {
            editCommand.PopupMenu = new ContextMenu();
            if (lastPDFEditEvent != null)
            {
                MenuItem rotateLeftMenu = new MenuItem();
                rotateLeftMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RotateLeft");
                rotateLeftMenu.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.Rotate = -90;
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                        pdfContentEditControl.SetRotationText(lastPDFEditEvent.CurrentRotated);
                        pdfContentEditControl.RefreshThumb();
                    }
                };
                editCommand.PopupMenu.Items.Add(rotateLeftMenu);

                MenuItem rotateRightMenu = new MenuItem();
                rotateRightMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RotateRight");
                rotateRightMenu.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.Rotate = 90;
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                        pdfContentEditControl.SetRotationText(lastPDFEditEvent.CurrentRotated);
                        pdfContentEditControl.RefreshThumb();
                    }
                };
                editCommand.PopupMenu.Items.Add(rotateRightMenu);

                MenuItem replaceMenu = new MenuItem();
                replaceMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Replace");
                replaceMenu.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            lastPDFEditEvent.ReplaceImagePath = openFileDialog.FileName;
                            lastPDFEditEvent.UpdatePDFEditByEventArgs();
                            PdfViewControl.PDFView?.ClearSelectPDFEdit();
                            pdfContentEditControl.RefreshThumb();
                        }
                    }
                };
                editCommand.PopupMenu.Items.Add(replaceMenu);

                MenuItem exportMenu = new MenuItem();
                exportMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Export");
                exportMenu.Click += (o, p) =>
                {
                    if (PdfViewControl != null && PdfViewControl.PDFView != null)
                    {
                        Dictionary<int, List<System.Drawing.Bitmap>> imageDict = PdfViewControl.PDFView.GetSelectedImages();
                        if (imageDict != null && imageDict.Count > 0)
                        {
                            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                string choosePath = folderBrowser.SelectedPath;
                                string openPath = choosePath;
                                foreach (int pageIndex in imageDict.Keys)
                                {
                                    List<System.Drawing.Bitmap> imageList = imageDict[pageIndex];
                                    foreach (System.Drawing.Bitmap image in imageList)
                                    {
                                        string savePath = System.IO.Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                                        image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        openPath = savePath;
                                    }
                                }
                                Process.Start("explorer", "/select,\"" + openPath + "\"");
                            }
                        }
                    }
                };
                editCommand.PopupMenu.Items.Add(exportMenu);

                MenuItem opacityMenu = new MenuItem();
                opacityMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Opacity");
                editCommand.PopupMenu.Items.Add(opacityMenu);

                AppendOpacityMenu(opacityMenu);

                MenuItem horizonMirror = new MenuItem();
                horizonMirror.Header = LanguageHelper.CommonManager.GetString("Menu_HFlip");
                horizonMirror.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.HorizontalMirror = true;
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                    }
                };
                editCommand.PopupMenu.Items.Add(horizonMirror);

                MenuItem verticalMirror = new MenuItem();
                verticalMirror.Header = LanguageHelper.CommonManager.GetString("Menu_VFlip");
                verticalMirror.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.VerticalMirror = true;
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                    }
                };
                editCommand.PopupMenu.Items.Add(verticalMirror);

                MenuItem cropMenu = new MenuItem();
                cropMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Crop");
                cropMenu.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.ClipImage = true;
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                    }
                };
                editCommand.PopupMenu.Items.Add(cropMenu);

                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                if (editCommand.TextAreaCopied)
                {
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                }
            }
            else
            {
                editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
            }
        }

        private void PDFEditMultiContextMenu(object sender, PDFEditCommand editCommand)
        {
            if(editCommand!=null)
            {
                editCommand.PopupMenu = new ContextMenu();
                if (lastPDFEditMultiEvents!=null)
                {
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    if(CustomCommands.PasteWithoutStyle.CanExecute(null, (UIElement)sender))
                    {
                        editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                    }
                }
                else
                {
                    editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    if (CustomCommands.PasteWithoutStyle.CanExecute(null, (UIElement)sender))
                    {
                        editCommand.PopupMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                    }
                }
            }
        }
        
        private void AppendOpacityMenu(MenuItem parentMenu)
        {
            List<int> opacityList = new List<int>()
            {
                25,50,75,100
            };

            foreach (int opacity in opacityList)
            {
                MenuItem opacityMenu = new MenuItem();
                opacityMenu.Header = string.Format("{0}%", opacity);
                opacityMenu.Click += (o, p) =>
                {
                    if (lastPDFEditEvent != null && lastPDFEditEvent.EditType == CPDFEditType.EditImage)
                    {
                        lastPDFEditEvent.Transparency = (int)Math.Ceiling(opacity * 255 / 100D);
                        lastPDFEditEvent.UpdatePDFEditByEventArgs();
                    }
                };
                parentMenu.Items.Add(opacityMenu);
            }
        }

        #endregion
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
    }
}
