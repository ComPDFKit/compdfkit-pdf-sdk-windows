using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
namespace ComPDFKit.Controls.Edit
{
    public partial class PDFTextEditControl : UserControl
    {
        #region Property
        public CPDFViewerTool ToolView { get; private set; }
        public TextEditParam EditEvent { get; set; }

        //public List<PDFEditEvent> EditMultiEvents { get; set; }

        #endregion 

        public PDFTextEditControl()
        {
            InitializeComponent();
            Loaded += PDFTextEditControl_Loaded;
        }

        #region Init PDFView
        public void InitWithPDFViewer(CPDFViewerTool newPDFView)
        {
            ToolView = newPDFView;
        }
        #endregion

        #region UI
        public void SetPDFTextEditData(TextEditParam newEvent)
        {
            if (newEvent.EditIndex<0)
            {
                EditEvent = null;
            }
            else
            {
                EditEvent = newEvent;
            }
            if (newEvent != null && newEvent.EditType == CPDFEditType.EditText)
            {
                GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
                List<string> sysfontList = new List<string>();
                if (textArea != null)
                {
                    sysfontList = textArea.GetFontList();
                }
                if (sysfontList.Count == 0)
                {
                    sysfontList.Add("Helvetica");
                    sysfontList.Add("Courier New");
                    sysfontList.Add("Times New Roman");
                }
                if (sysfontList.Contains(newEvent.FontName) == false && string.IsNullOrEmpty(newEvent.FontName) == false)
                {
                    sysfontList.Add(newEvent.FontName);
                }

                TextStyleUI.SetFontNames(sysfontList);
                TextStyleUI.SelectFontName(newEvent.FontName);
                TextStyleUI.SetFontStyle(newEvent.IsBold, newEvent.IsItalic);
                TextStyleUI.SetFontSize(newEvent.FontSize);
                OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Ceiling(newEvent.Transparency * 100 / 255D)));
                FontOpacitySlider.Value = ((int)(Math.Ceiling(newEvent.Transparency * 100 / 255D))) / 100D;
                TextAlignUI.SetFontAlign(newEvent.TextAlign);
                if (newEvent.FontColor != null && newEvent.FontColor.Length == 3)
                {
                    FontColorUI.SetCheckedForColor(Color.FromRgb(
                        newEvent.FontColor[0],
                        newEvent.FontColor[1],
                        newEvent.FontColor[2]));
                }

            }
            EditEvent = newEvent;
        }

        //public void SetPDFTextMultiEditData(List<PDFEditEvent> editEvents)
        //{
        //    EditEvent = null;
        //    EditMultiEvents = null;
        //    if(editEvents!=null && editEvents.Count>0)
        //    {
        //        PDFEditEvent editEvent= editEvents[0];

        //        if (editEvent != null && editEvent.EditType == CPDFEditType.EditText)
        //        {
        //            if (editEvent.SystemFontNameList != null && editEvent.SystemFontNameList.Count == 0)
        //            {
        //                editEvent.SystemFontNameList.Add("Helvetica");
        //                editEvent.SystemFontNameList.Add("Courier New");
        //                editEvent.SystemFontNameList.Add("Times New Roman");
        //            }
        //            if (editEvent.SystemFontNameList.Contains(editEvent.FontName) == false && string.IsNullOrEmpty(editEvent.FontName) == false)
        //            {
        //                editEvent.SystemFontNameList.Add(editEvent.FontName);
        //            }

        //            TextStyleUI.SetFontNames(editEvent.SystemFontNameList);
        //            TextStyleUI.SelectFontName(editEvent.FontName);
        //            TextStyleUI.SetFontStyle(editEvent.IsBold, editEvent.IsItalic);
        //            TextStyleUI.SetFontSize(editEvent.FontSize);
        //            OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Ceiling(editEvent.Transparency * 100 / 255D)));
        //            FontOpacitySlider.Value = ((int)(Math.Ceiling(editEvent.Transparency * 100 / 255D))) / 100D;
        //            TextAlignUI.SetFontAlign(editEvent.TextAlign);
        //            FontColorUI.SetCheckedForColor(editEvent.FontColor);
        //        }
        //    }

        //    EditMultiEvents=editEvents;

        //}
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontSize((float)slider.Value, false))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.FontSize = slider.Value;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.FontSize = slider.Value;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void SliderOpacity_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.Transparency = (byte)(FontOpacitySlider.Value * 255);
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (OpacityTextBox != null && FontOpacitySlider != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(FontOpacitySlider.Value * 100));
            }

            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.Transparency = (byte)(FontOpacitySlider.Value * 255);
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "false";
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }

            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontSize((float)slider.Value, false))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.FontSize = slider.Value;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.FontSize = slider.Value;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        #endregion

        #region Loaded
        private void PDFTextEditControl_Loaded(object sender, RoutedEventArgs e)
        {

            TextStyleUI.TextFontChanged -= TextStyleUI_TextFontChanged;
            TextStyleUI.TextBoldChanged -= TextStyleUI_TextBoldChanged;
            TextStyleUI.TextItalicChanged -= TextStyleUI_TextItalicChanged;
            TextStyleUI.TextSizeChanged -= TextStyleUI_TextSizeChanged;
            TextAlignUI.TextAlignChanged -= TextAlignUI_TextAlignChanged;
            FontColorUI.ColorChanged -= FontColorUI_ColorChanged;

            TextStyleUI.TextFontChanged += TextStyleUI_TextFontChanged;
            TextStyleUI.TextBoldChanged += TextStyleUI_TextBoldChanged;
            TextStyleUI.TextItalicChanged += TextStyleUI_TextItalicChanged;
            TextStyleUI.TextSizeChanged += TextStyleUI_TextSizeChanged;
            TextAlignUI.TextAlignChanged += TextAlignUI_TextAlignChanged;
            FontColorUI.ColorChanged += FontColorUI_ColorChanged;
        }

        #endregion

        #region Property changed

        private void TextStyleUI_TextSizeChanged(object sender, double e)
        {
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontSize((float)e, false))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.FontSize = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.FontSize = e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void FontColorUI_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush newBrush = FontColorUI.Brush as SolidColorBrush;
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null && newBrush != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontColor(newBrush.Color.R, newBrush.Color.G, newBrush.Color.B))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null && newBrush != null)
            {
                byte[] Color = new byte[3];
                Color[0] = newBrush.Color.R;
                Color[1] = newBrush.Color.G;
                Color[2] = newBrush.Color.B;
                EditEvent.FontColor = Color;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }
        }

        private void TextAlignUI_TextAlignChanged(object sender, TextAlignType e)
        {
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                bool result = false;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SelectLineRects != null && textArea.SelectLineRects.Count > 0)
                {
                    result = textArea.SetTextRangeAlign(e);
                }
                else
                {
                    result = textArea.SetTextAreaAlign(e);
                }
                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }

            if (EditEvent != null && textArea == null)
            {
                EditEvent.TextAlign = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.TextAlign = e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void TextStyleUI_TextItalicChanged(object sender, bool e)
        {
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontItalic(e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.IsItalic = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.IsItalic = e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void TextStyleUI_TextBoldChanged(object sender, bool e)
        {
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontBold(e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.IsBold = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.IsBold = e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void TextStyleUI_TextFontChanged(object sender, string e)
        {
            GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textArea.GetFrame());
                if (textArea.SetCharsFontName(e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }
                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textArea);
                    editPage.EndEdit();
                }
            }
            if (EditEvent != null && textArea == null)
            {
                EditEvent.FontName = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvent);
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.FontName = e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void OpacityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = OpacityComboBox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                if (double.TryParse(selectItem.Content.ToString().TrimEnd('%'), out double newOpacity))
                {
                    OpacityTextBox.Text = selectItem.Content.ToString();
                    FontOpacitySlider.Value = newOpacity / 100.0;
                }
            }
        }

        #endregion

        #region Text Edit
        private void GetTextArea(out CPDFEditTextArea textArea, out CPDFPage pdfPage, out CPDFEditPage editPage)
        {
            textArea = null;
            editPage = null;
            pdfPage = null;
            if (ToolView == null)
            {
                return;
            }
            if (EditEvent != null)
            {
                try
                {
                    CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
                    pdfPage = pdfDoc.PageAtIndex(EditEvent.PageIndex);
                    editPage = pdfPage.GetEditPage();
                    List<CPDFEditArea> editAreas = editPage.GetEditAreaList();
                    if (editAreas != null && editAreas.Count > EditEvent.EditIndex)
                    {
                        textArea = editAreas[EditEvent.EditIndex] as CPDFEditTextArea;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfViewer.GetDocument();
                pdfPage = pdfDoc.PageAtIndex(0);
                editPage = pdfPage.GetEditPage();
                editPage.BeginEdit(CPDFEditType.EditText);
                editPage.EndEdit();
            }
        }
        #endregion
    }
}
