using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Controls.Edit;
using ComPDFKit.Controls.Helper;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComPDFKit.Import;
using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Helper;
using ContextMenu = System.Windows.Controls.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using MenuItem = System.Windows.Controls.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;
using System.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class ContentEditControl : UserControl, INotifyPropertyChanged
    {
        #region Property

        private bool isUpdateStartPoint = true;

        public ICommand CloseTabCommand;
        public ICommand ExpandPropertyPanelCommand;

        private CPoint startPoint;
        private CPoint endPoint;

        public PDFViewControl PdfViewControl;
        public PDFContentEditControl pdfContentEditControl = new PDFContentEditControl();
        private CPDFDisplaySettingsControl displaySettingsControl = null;
        private TextEditParam pdfTextCreateParam;
        private PDFEditParam imageAreaParam = null;
        private List<TextEditParam> lastPDFEditMultiEvents = null;
        private PanelState panelState = PanelState.GetInstance();
        private KeyEventHandler KeyDownHandler;
        private bool textAreaCreating = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanUndo
        {
            get
            {
                try
                {
                    if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
                    {
                        CPDFViewerTool viewerTool = PdfViewControl.PDFViewTool;
                        CPDFViewer pdfViewer = viewerTool.GetCPDFViewer();
                        return pdfViewer.UndoManager.CanUndo;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                try
                {
                    if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
                    {
                        CPDFViewerTool viewerTool = PdfViewControl.PDFViewTool;
                        CPDFViewer pdfViewer = viewerTool.GetCPDFViewer();
                        return pdfViewer.UndoManager.CanRedo;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                return false;
            }
        }

        private bool CanSave
        {
            get
            {
                try
                {
                    if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
                    {
                        CPDFViewerTool viewerTool = PdfViewControl.PDFViewTool;
                        CPDFViewer pdfViewer = viewerTool.GetCPDFViewer();
                        return (pdfViewer.UndoManager.CanUndo | pdfViewer.UndoManager.CanRedo);
                    }
                }
                catch (Exception ex)
                {
                    return false;
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
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        #region public method 
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

            PdfViewControl.PDFToolManager.SetCreateContentEditType(CPDFEditType.None);
            PdfViewControl.PDFViewTool.SetCurrentEditType(CPDFEditType.EditText | CPDFEditType.EditImage);
            PdfViewControl.PDFViewTool.GetCPDFViewer().SetIsVisibleCustomMouse(false);
            PdfViewControl.PDFViewTool.GetCPDFViewer().SetIsShowStampMouse(false);
            PdfViewControl.PDFViewTool.SelectedEditAreaForIndex(-1, -1);
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

        public void InitWithPDFViewer(PDFViewControl view)
        {
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged += UndoManager_PropertyChanged;
            PdfViewControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.DrawChanged -= PdfViewControl_DrawChanged;
            PdfViewControl.DrawChanged += PdfViewControl_DrawChanged;

            PdfViewControl = view;
            PDFGrid.Child = PdfViewControl;
            FloatPageTool.InitWithPDFViewer(view);
            pdfContentEditControl.InitWithPDFViewer(view);
            DataContext = this;
            if (PdfViewControl != null)
            {
                PdfViewControl.PDFViewTool.IsManipulationEnabled = true;
                if (KeyDownHandler != null)
                {
                    PdfViewControl.PDFViewTool.RemoveHandler(KeyDownEvent, KeyDownHandler);
                }

                KeyDownHandler = new KeyEventHandler(PDFView_KeyDown);
                PdfViewControl.PDFViewTool.AddHandler(KeyDownEvent, KeyDownHandler, false);
            }
        }

        private void PdfViewControl_DrawChanged(object sender, EventArgs e)
        {
            if (textAreaCreating && PdfViewControl.PDFToolManager.GetCreateContentEditType() == CPDFEditType.EditText)
            {
                textAreaCreating = false;
                int pageIndex = -1;
                CPDFEditArea editAreaArea = PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref pageIndex);
                if (editAreaArea == null)
                {
                    return;
                }
                else
                {
                    if (editAreaArea.Type == CPDFEditType.EditText)
                    {
                        PDFEditParam pDFEditParam = ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), editAreaArea, pageIndex);
                        pdfContentEditControl.SetPDFTextEditData(new List<TextEditParam> { (TextEditParam)pDFEditParam }, true);
                        PropertyContainer.Child = pdfContentEditControl;
                    }
                }
            }
        }

        /// <summary>
        /// Short cut key for PDFView
        /// </summary> 
        public void PDFView_KeyDown(object sender, KeyEventArgs e)
        {
            if (PdfViewControl.PDFViewTool.GetToolType() != ToolType.ContentEdit)
            {
                return;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                PdfViewControl.PDFViewTool.SetMultiSelectKey(e.Key);
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                PdfViewControl.PDFViewTool.SetMultiSelectKey(e.Key);
            }

            int pageIndex = -1;
            CPDFEditTextArea textArea = PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref pageIndex) as CPDFEditTextArea;
            if (textArea == null)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left)
                {
                    textArea.GetPreWordCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    textArea.GetNextWordCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    textArea.GetSectionBeginCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    textArea.GetSectionEndCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (isUpdateStartPoint)
                {
                    startPoint = GetPoint(textArea);
                    isUpdateStartPoint = false;
                }
                if (e.Key == Key.Left)
                {
                    textArea.GetPrevCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    textArea.GetNextCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    textArea.GetUpCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    textArea.GetDownCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (e.SystemKey == Key.Up)
                {
                    textArea.GetLineBeginCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.SystemKey == Key.Down)
                {
                    textArea.GetLineEndCharPlace();
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                CRect textFrame = textArea.GetFrame();
                if (e.Key == Key.Left)
                {
                    textFrame.left -= 5;
                    textArea.SetFrame(textFrame);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    textFrame.left += 5;
                    textArea.SetFrame(textFrame);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    textFrame.top -= 5;
                    textArea.SetFrame(textFrame);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    textFrame.top += 5;
                    textArea.SetFrame(textFrame);
                    e.Handled = true;
                }
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (isUpdateStartPoint)
                {
                    startPoint = GetPoint(textArea);
                    isUpdateStartPoint = false;
                }

                if (e.Key == Key.Left)
                {
                    textArea.GetPreWordCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Right)
                {
                    textArea.GetNextWordCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Up)
                {
                    textArea.GetSectionBeginCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }

                if (e.Key == Key.Down)
                {
                    textArea.GetSectionEndCharPlace();
                    endPoint = GetPoint(textArea);
                    textArea.GetSelectChars(startPoint, endPoint);
                    UpdateTextArea(textArea);
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Panel
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
        #endregion

        #region UI
        private void PDFTextEditButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton senderBtn = sender as ToggleButton;
            if (senderBtn != null && PdfViewControl != null)
            {
                ClearPDFEditState(senderBtn);
                if (senderBtn.IsChecked == true)
                {
                    TextEditParam textEditParam = new TextEditParam();
                    textEditParam.EditType = CPDFEditType.EditText;
                    textEditParam.IsBold = false;
                    textEditParam.IsItalic = false;
                    textEditParam.FontSize = 14;
                    textEditParam.FontName = "Arial";
                    textEditParam.FontColor = new byte[] { 0, 0, 0 };
                    textEditParam.EditIndex = -1;
                    textEditParam.TextAlign = TextAlignType.AlignLeft;
                    textEditParam.Transparency = 255;
                    pdfContentEditControl.SetPDFTextEditData(new List<TextEditParam> { textEditParam });
                    DefaultSettingParam defaultSettingParam = PdfViewControl.PDFViewTool.GetDefaultSettingParam();
                    defaultSettingParam.SetPDFEditParamm(textEditParam);
                    panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                    PdfViewControl.PDFToolManager.SetCreateContentEditType(CPDFEditType.EditText);
                    PdfViewControl.PDFViewTool.SetCurrentEditType(CPDFEditType.EditText);
                }
            }

        }

        private void PDFImageEditButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton senderBtn = sender as ToggleButton;
            if (senderBtn != null && PdfViewControl != null)
            {
                ClearPDFEditState(senderBtn);
                panelState.RightPanel = PanelState.RightPanelState.None;
                senderBtn.IsChecked = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        bool SetImage = PdfViewControl.PDFViewTool.GetCPDFViewer().SetStampMouseImage(openFileDialog.FileName);
                        PdfViewControl.PDFToolManager.SetCreateImagePath(openFileDialog.FileName);
                        PdfViewControl.PDFViewTool.GetCPDFViewer().SetIsVisibleCustomMouse(SetImage);
                        PdfViewControl.PDFViewTool.GetCPDFViewer().SetIsShowStampMouse(SetImage);
                        PdfViewControl.PDFToolManager.SetCreateContentEditType(CPDFEditType.EditImage);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer()?.UndoManager?.Undo();
                PdfViewControl.PDFViewTool.GetCPDFViewer().UpdateRenderFrame();
            }
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer()?.UndoManager?.Redo();
                PdfViewControl.PDFViewTool.GetCPDFViewer().UpdateRenderFrame();
            }
        }
        #endregion

        #region 

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
            PdfViewControl.MouseRightButtonDownHandler += PdfViewControl_MouseRightButtonDownHandler;
        }

        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
        }

        private void PDFEditEmptyPanel()
        {
            PropertyContainer.Child = pdfContentEditControl;

            if (pdfTextCreateParam != null && PdfViewControl != null && PdfViewControl.PDFView != null)
            {
                if (PdfViewControl.PDFToolManager.GetCreateContentEditType() == CPDFEditType.EditText)
                {
                    pdfContentEditControl.SetPDFTextEditData(new List<TextEditParam> { pdfTextCreateParam });
                }
                else if (PdfViewControl.PDFToolManager.GetCreateContentEditType() == CPDFEditType.None)
                {
                    pdfContentEditControl.ClearContentControl();
                }
            }
            else
            {
                pdfContentEditControl.ClearContentControl();
            }
        }

        private CPoint GetPoint(CPDFEditTextArea textArea)
        {
            CPoint caretPoint = new CPoint();
            CPoint caretPointHigh = new CPoint();
            textArea.GetTextCursorPoints(ref caretPoint, ref caretPointHigh);
            var lineHeight = caretPoint.y - caretPointHigh.y;
            CRect caretRect = new CRect(caretPoint.x, caretPoint.y, caretPointHigh.x, caretPointHigh.y);
            caretPoint = new CPoint(caretRect.left, caretRect.top);
            return new CPoint(caretRect.left, (caretRect.top + caretRect.bottom) / 2);
        }

        /// <summary>
        /// Update the text appearance after the text area is changed
        /// </summary> 
        private void UpdateTextArea(CPDFEditTextArea textArea)
        {
            Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
            PdfViewControl.PDFViewTool.UpdateRender(oldRect, textArea);
        }

        /// <summary>
        /// Update the image appearance after the image area is changed
        /// </summary> 
        private void UpdateImageArea(CPDFEditImageArea imageArea)
        {
            Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
            PdfViewControl.PDFViewTool.UpdateRender(oldRect, imageArea);
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
                case MouseHitTestType.TextEdit:
                    CreateTextEditMenu(sender, ref ContextMenu);
                    break;
                case MouseHitTestType.ImageEdit:
                    CreateImageEditMenu(sender, ref ContextMenu);
                    break;
                case MouseHitTestType.Unknown:
                    List<int> pageInts = new List<int>();
                    List<CPDFEditArea> editAreas = PdfViewControl.PDFToolManager.GetSelectedEditAreaListObject(ref pageInts);
                    if (editAreas.Count > 0)
                    {
                        CreateMultiTextEditMenu(sender, ref ContextMenu);
                    }
                    else
                    {
                        ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                        ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                    }
                    break;
                default:
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
                    break;
            }
            PdfViewControl.SetRightMenu(ContextMenu);
        }
        #endregion

        #region Property changed
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanUndo" || e.PropertyName == "CanRedo")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }
        #endregion

        #region Context menu

        private void AppendOpacityMenu(MenuItem parentMenu, CPDFEditArea editArea, CPDFEditType editType)
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
                    if (editArea != null && editType == CPDFEditType.EditImage)
                    {
                        CPDFEditImageArea editImageArea = editArea as CPDFEditImageArea;
                        editImageArea?.SetImageTransparency((byte)Math.Ceiling(opacity * 255 / 100D));
                        pdfContentEditControl.RefreshThumb();
                        UpdateImageArea(editImageArea);
                    }
                    else if (editArea != null && editType == CPDFEditType.EditText)
                    {
                        CPDFEditTextArea editTextArea = editArea as CPDFEditTextArea;
                        editTextArea?.SetCharsFontTransparency((byte)Math.Ceiling(opacity * 255 / 100D));
                        pdfContentEditControl.RefreshThumb();
                        UpdateTextArea(editTextArea);
                    }
                };
                parentMenu.Items.Add(opacityMenu);
            }
        }

        private void PDFToolManager_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (e.IsCreate)
            {
                PdfViewControl.SetIsShowStampMouse(false);
                if (PdfViewControl.PDFToolManager.GetCreateContentEditType() == CPDFEditType.EditImage)
                {
                    PdfViewControl.PDFToolManager.SetCreateContentEditType(CPDFEditType.None);
                }

                if (PdfViewControl.PDFToolManager.GetCreateContentEditType() == CPDFEditType.EditText)
                {
                    textAreaCreating = true;
                }
            }

            int pageIndex = -1;
            CPDFEditArea editArea = PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref pageIndex);
            List<int> pageInts = new List<int>();
            List<CPDFEditArea> editAreas = PdfViewControl.PDFToolManager.GetSelectedEditAreaListObject(ref pageInts);

            if (editArea != null)
            {
                if (editArea.Type == CPDFEditType.EditText)
                {
                    PDFEditParam editParam = ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), editArea, pageIndex);
                    pdfContentEditControl.SetPDFTextEditData(new List<TextEditParam> { (TextEditParam)editParam }, true);
                    PropertyContainer.Child = pdfContentEditControl;
                }
            }
            else if (editAreas != null && editAreas.Count != 0)
            {
                List<CPDFEditTextArea> editTextAreas = editAreas.OfType<CPDFEditTextArea>().ToList();
                editTextAreas.ForEach(textArea => textArea.SelectAllChars());
                if (editAreas.Count == editTextAreas.Count)
                {
                    List<TextEditParam> editParams = editTextAreas.
                        Select(area => ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), area, pageInts.FirstOrDefault())).
                        Cast<TextEditParam>().ToList();
                    pdfContentEditControl.SetPDFTextEditData(editParams, true);
                    PropertyContainer.Child = pdfContentEditControl;
                }
                else if (editTextAreas.Count == 0)
                {
                    List<ImageEditParam> editParams = editAreas.
                        Select(area => ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), area, pageInts.FirstOrDefault())).
                        Cast<ImageEditParam>().ToList();
                    pdfContentEditControl.SetPDFImageEditData(editParams);
                    PropertyContainer.Child = pdfContentEditControl;
                }
                else
                {
                    pdfContentEditControl.ClearContentControl();
                }
            }
            else
            {
                return;
            }
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            isUpdateStartPoint = true;
            if (PdfViewControl.PDFToolManager.GetToolType() != ToolType.ContentEdit)
            {
                PropertyContainer.Child = pdfContentEditControl;
                return;
            }

            int pageIndex = -1;
            CPDFEditArea editAreaArea = PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref pageIndex);
            if (editAreaArea == null)
            {
                if (PdfViewControl.PDFToolManager.GetCreateContentEditType() != CPDFEditType.EditText)
                {
                    pdfContentEditControl.ClearContentControl();
                }
                return;
            }
            else
            {
                if (editAreaArea.Type == CPDFEditType.EditText)
                {
                    PDFEditParam pDFEditParam = ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), editAreaArea, pageIndex);
                    pdfContentEditControl.SetPDFTextEditData(new List<TextEditParam> { (TextEditParam)pDFEditParam }, true);
                    PropertyContainer.Child = pdfContentEditControl;
                }

                else if (editAreaArea.Type == CPDFEditType.EditImage && PdfViewControl != null)
                {
                    UIElement pageView = sender as UIElement;
                    if (pageView != null)
                    {
                        pageView.MouseLeftButtonUp -= PageView_MouseLeftButtonUp;
                        pageView.MouseLeftButtonUp += PageView_MouseLeftButtonUp;
                    }
                    PDFEditParam pDFEditParam = ParamConverter.CPDFDataConverterToPDFEitParam(PdfViewControl.PDFToolManager.GetDocument(), editAreaArea, pageIndex);
                    pdfContentEditControl.SetPDFImageEditData(new List<ImageEditParam> { (ImageEditParam)pDFEditParam });
                    PropertyContainer.Child = pdfContentEditControl;
                }

                else
                {

                }
            }
        }

        private void PdfViewControl_SplitPDFViewToolCreated(object sender, EventArgs e)
        {
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged += UndoManager_PropertyChanged;
        }

        private void PageView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement pageView = sender as UIElement;
            if (pageView != null)
            {
                pageView.MouseLeftButtonUp -= PageView_MouseLeftButtonUp;
            }
            if (imageAreaParam != null)
            {
                pdfContentEditControl.SetPDFImageEditData(new List<ImageEditParam> { (ImageEditParam)imageAreaParam });
            }
        }

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
        }


        private void CommandBinding_Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null && CanUndo)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer()?.UndoManager?.Undo();
            }
        }

        private void CommandBinding_Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (PdfViewControl != null && PdfViewControl.PDFViewTool != null && CanRedo)
            {
                PdfViewControl.PDFViewTool.GetCPDFViewer()?.UndoManager?.Redo();
            }
        }

        private void CreateTextEditMenu(object sender, ref ContextMenu menu)
        {
            int index = -1;
            CPDFEditTextArea textArea =
                PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref index) as CPDFEditTextArea;
            if (textArea != null)
            {
                if (PdfViewControl.PDFToolManager.GetDocument().GetPermissionsInfo().AllowsCopying)
                {
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                }
                menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
            }
            else
            {
                menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_MatchPaste"), Command = CustomCommands.PasteWithoutStyle, CommandTarget = (UIElement)sender });
            }
        }

        private void CreateImageEditMenu(object sender, ref ContextMenu menu)
        {
            int index = -1;
            CPDFEditImageArea imageArea = PdfViewControl.PDFToolManager.GetSelectedEditAreaObject(ref index) as CPDFEditImageArea;

            MenuItem rotateLeftMenu = new MenuItem();
            rotateLeftMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RotateLeft");
            rotateLeftMenu.Click += (o, p) =>
            {
                if (imageArea != null)
                {
                    imageArea.Rotate(-90);
                    pdfContentEditControl.RefreshThumb();
                    UpdateImageArea(imageArea);
                }
            };
            menu.Items.Add(rotateLeftMenu);

            MenuItem rotateRightMenu = new MenuItem();
            rotateRightMenu.Header = LanguageHelper.CommonManager.GetString("Menu_RotateRight");
            rotateRightMenu.Click += (o, p) =>
            {
                if (imageArea != null)
                {
                    imageArea.Rotate(90);
                    pdfContentEditControl.RefreshThumb();
                    UpdateImageArea(imageArea);
                }
            };
            menu.Items.Add(rotateRightMenu);

            MenuItem replaceMenu = new MenuItem();
            replaceMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Replace");
            replaceMenu.Click += (o, p) =>
            {
                if (imageArea != null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        int imageWidth = 0;
                        int imageHeight = 0;
                        byte[] imageData = null;

                        BitmapFrame frame = null;
                        BitmapDecoder decoder = BitmapDecoder.Create(new Uri(openFileDialog.FileName), BitmapCreateOptions.None, BitmapCacheOption.Default);
                        if (decoder.Frames.Count > 0)
                        {
                            frame = decoder.Frames[0];
                        }
                        if (frame != null)
                        {
                            imageData = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                            if (frame.Format != PixelFormats.Bgra32)
                            {
                                FormatConvertedBitmap covert = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, frame.Palette, 0);
                                covert.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                            }
                            else
                            {
                                frame.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                            }
                            imageWidth = frame.PixelWidth;
                            imageHeight = frame.PixelHeight;
                        }

                        imageArea.ReplaceImageArea(imageArea.GetFrame(), imageData, imageWidth, imageHeight);
                        pdfContentEditControl.RefreshThumb();
                        UpdateImageArea(imageArea);
                    }
                }
            };
            menu.Items.Add(replaceMenu);

            MenuItem exportMenu = new MenuItem();
            exportMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Export");
            exportMenu.Click += (o, p) =>
            {
                if (PdfViewControl != null && PdfViewControl.PDFView != null)
                {
                    try
                    {
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                        if (folderBrowserDialog.ShowDialog() == DialogResult.OK && imageArea != null)
                        {
                            string path = Path.GetTempPath();
                            string uuid = Guid.NewGuid().ToString("N");
                            string imagePath = Path.Combine(path, uuid + ".tmp");
                            imageArea.ExtractImage(imagePath);

                            Bitmap bitmapImage = new Bitmap(imagePath);
                            string fileName = Path.Combine(folderBrowserDialog.SelectedPath, uuid + ".jpg");
                            bitmapImage.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            Process.Start("explorer", "/select,\"" + fileName + "\"");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            };
            menu.Items.Add(exportMenu);

            MenuItem opacityMenu = new MenuItem();
            opacityMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Opacity");
            menu.Items.Add(opacityMenu);

            AppendOpacityMenu(opacityMenu, imageArea, CPDFEditType.EditImage);

            MenuItem horizonMirror = new MenuItem();
            horizonMirror.Header = LanguageHelper.CommonManager.GetString("Menu_HFlip");
            horizonMirror.Click += (o, p) =>
            {
                imageArea?.HorizontalMirror();
                pdfContentEditControl.RefreshThumb();
                UpdateImageArea(imageArea);
            };
            menu.Items.Add(horizonMirror);

            MenuItem verticalMirror = new MenuItem();
            verticalMirror.Header = LanguageHelper.CommonManager.GetString("Menu_VFlip");
            verticalMirror.Click += (o, p) =>
            {
                if (imageArea != null)
                {
                    imageArea?.VerticalMirror();
                    pdfContentEditControl.RefreshThumb();
                    UpdateImageArea(imageArea);
                }
            };
            menu.Items.Add(verticalMirror);

            MenuItem cropMenu = new MenuItem();
            cropMenu.Header = LanguageHelper.CommonManager.GetString("Menu_Crop");
            cropMenu.Click += (o, p) =>
            {
                if (imageArea != null)
                {
                    PdfViewControl.PDFViewTool.SetCropMode(!PdfViewControl.PDFViewTool.GetIsCropMode());
                }
            };
            menu.Items.Add(cropMenu);

            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
        }

        private void CreateMultiTextEditMenu(object sender, ref ContextMenu menu)
        {
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
        }
    }
}
#endregion