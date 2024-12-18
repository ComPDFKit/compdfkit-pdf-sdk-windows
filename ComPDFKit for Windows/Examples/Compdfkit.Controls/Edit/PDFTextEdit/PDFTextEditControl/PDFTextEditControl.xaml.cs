using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFTextEditControl : UserControl, INotifyPropertyChanged
    {
        #region Property
        public CPDFViewerTool ToolView { get; private set; }
        public List<TextEditParam> EditEvents { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isMultiSelected = true;
        public bool IsMultiSelected
        {
            get => _isMultiSelected;
            set
            {
                UpdateProper(ref _isMultiSelected, value);
            }
        }

        private bool _showBorder;
        public bool ShowBorder
        {
            get => _showBorder;
            set
            {
                UpdateProper(ref _showBorder, value);
            }
        }
        #endregion 

        public PDFTextEditControl()
        {
            DataContext = this;
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
        public void SetPDFTextEditData(List<TextEditParam> newEvents)
        {
            EditEvents = newEvents.Where(newEvent => newEvent.EditIndex >= 0 && newEvent.EditType == CPDFEditType.EditText).ToList();
            TextEditParam defaultEvent = EditEvents.FirstOrDefault();
            if (EditEvents.Count > 0)
            {
                GetTextArea(out List<CPDFEditTextArea> textArea, out CPDFPage pdfPage, out CPDFEditPage editPage);

                List<string> sysfontList = new List<string>();
                if (textArea != null)
                {
                    sysfontList = textArea.FirstOrDefault().GetFontList();
                }
                if (sysfontList.Count == 0)
                {
                    sysfontList.Add("Helvetica");
                    sysfontList.Add("Courier New");
                    sysfontList.Add("Times New Roman");
                }
                if (sysfontList.Contains(defaultEvent.FontName) == false && string.IsNullOrEmpty(defaultEvent.FontName) == false)
                {
                    sysfontList.Add(defaultEvent.FontName);
                }

                TextStyleUI.SetFontNames(sysfontList);
                TextStyleUI.SelectFontName(defaultEvent.FontName);
                TextStyleUI.SetFontStyle(defaultEvent.IsBold, defaultEvent.IsItalic);
                TextStyleUI.SetFontSize(defaultEvent.FontSize);

                FontOpacitySlider.Tag = "false";
                FontOpacitySlider.Value = ((int)(Math.Ceiling(defaultEvent.Transparency * 100 / 255D))) / 100D;
                FontOpacitySlider.Tag = "true";

                TextAlignUI.SetFontAlign(defaultEvent.TextAlign);
                if (defaultEvent.FontColor != null && defaultEvent.FontColor.Length == 3)
                {
                    FontColorUI.SetCheckedForColor(Color.FromRgb(
                        defaultEvent.FontColor[0],
                        defaultEvent.FontColor[1],
                        defaultEvent.FontColor[2]));
                }
            }
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

        private void SliderOpacity_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            FontOpacitySlider.Tag = "true";
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, fontSize, fontColor[0], fontColor[1], fontColor[2], (byte)(FontOpacitySlider.Value * 255), isBold, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255));
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().Transparency = (byte)(FontOpacitySlider.Value * 255);
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
            }
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OpacityTextBox != null && FontOpacitySlider != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(FontOpacitySlider.Value * 100D));
            }

            if (FontOpacitySlider.Tag == null || FontOpacitySlider.Tag.ToString() == "false")
            {
                return;
            }

            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, fontSize, fontColor[0], fontColor[1], fontColor[2], (byte)(FontOpacitySlider.Value * 255), isBold, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255));
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontTransparency((byte)(FontOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents?.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().Transparency = (byte)(FontOpacitySlider.Value * 255);
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
            }
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            FontOpacitySlider.Tag = "false";
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
            TextMarkupUI.TextUnderlineChanged -= TextMarkupUI_TextUnderlineChanged;
            TextMarkupUI.TextStrikethroughChanged -= TextMarkupUI_TextStrikethroughChanged;

            TextStyleUI.TextFontChanged += TextStyleUI_TextFontChanged;
            TextStyleUI.TextBoldChanged += TextStyleUI_TextBoldChanged;
            TextStyleUI.TextItalicChanged += TextStyleUI_TextItalicChanged;
            TextStyleUI.TextSizeChanged += TextStyleUI_TextSizeChanged;
            TextAlignUI.TextAlignChanged += TextAlignUI_TextAlignChanged;
            FontColorUI.ColorChanged += FontColorUI_ColorChanged;
            TextMarkupUI.TextUnderlineChanged += TextMarkupUI_TextUnderlineChanged;
            TextMarkupUI.TextStrikethroughChanged += TextMarkupUI_TextStrikethroughChanged;

            IsMultiSelected = ToolView.GetIsMultiSelected();
            ShowBorder = ToolView.GetEditPen() == null || ToolView.GetEditPen().Thickness != 0;
        }

        #endregion

        #region Property changed
        private void TextStyleUI_TextSizeChanged(object sender, double e)
        {
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, (float)e, fontColor[0], fontColor[1], fontColor[2], transparency, isBold, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontSize((float)e, true);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontSize((float)e, true))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().FontSize = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
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
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, fontSize, newBrush.Color.R, newBrush.Color.G, newBrush.Color.B, transparency, isBold, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontColor(newBrush.Color.R, newBrush.Color.G, newBrush.Color.B);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontColor(newBrush.Color.R, newBrush.Color.G, newBrush.Color.B))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                    ToolView.GetCPDFViewer()?.UpdateRenderFrame();
                }
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && newBrush != null)
            {
                byte[] Color = new byte[3];
                Color[0] = newBrush.Color.R;
                Color[1] = newBrush.Color.G;
                Color[2] = newBrush.Color.B;
                EditEvents.FirstOrDefault().FontColor = Color;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
            }
        }

        private void TextAlignUI_TextAlignChanged(object sender, TextAlignType e)
        {
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (textAreas[0].SelectLineRects != null && textAreas[0].SelectLineRects.Count > 0)
                {
                    result = textAreas[0].SetTextRangeAlign(e);
                }
                else
                {
                    result = textAreas[0].SetTextAreaAlign(e);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    bool result;
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
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().TextAlign = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
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
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, fontSize, fontColor[0], fontColor[1], fontColor[2], transparency, isBold, e);
                }
                else
                {
                    result = textAreas[0].SetCharsFontItalic(e);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontItalic(e))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().IsItalic = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
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
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(fontName, fontSize, fontColor[0], fontColor[1], fontColor[2], transparency, e, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontBold(e);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontBold(e))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().IsBold = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
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
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (string.IsNullOrEmpty(textAreas[0].SelectText))
                {
                    string fontName = "Helvetica";
                    float fontSize = 14;
                    byte[] fontColor = { 0, 0, 0 };
                    byte transparency = 255;
                    bool isBold = false;
                    bool isItalic = false;
                    textAreas[0].GetTextStyle(ref fontName, ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
                    result = textAreas[0].SetCurTextStyle(e, fontSize, fontColor[0], fontColor[1], fontColor[2], transparency, isBold, isItalic);
                }
                else
                {
                    result = textAreas[0].SetCharsFontName(e);
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    if (textArea.SetCharsFontName(e))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && textAreas.Count > 0)
            {
                EditEvents.FirstOrDefault().FontName = e;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
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
                    FontOpacitySlider.Value = newOpacity / 100.0;
                }
            }
        }

        private void TextMarkupUI_TextUnderlineChanged(object sender, bool e)
        {
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if(e)
                {
                    result = textAreas[0].AddUnderline();
                }
                else
                {
                    result = textAreas[0].RemoveUnderline();
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    bool result;
                    if (e)
                    {
                        result = textArea.AddUnderline();
                    }
                    else
                    {
                        result = textArea.RemoveUnderline();
                    }

                    if (result)
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        private void TextMarkupUI_TextStrikethroughChanged(object sender, bool e)
        {
            GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (textAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                bool result;
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(textAreas[0].GetFrame());
                if (e)
                {
                    result = textAreas[0].AddStrikethrough();
                }
                else
                {
                    result = textAreas[0].RemoveStrikethrough();
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, textAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditTextArea textArea in textAreas)
                {
                    bool result;
                    if (e)
                    {
                        result = textArea.AddStrikethrough();
                    }
                    else
                    {
                        result = textArea.RemoveStrikethrough();
                    }

                    if (result)
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        #endregion

        #region Text Edit
        private void GetTextArea(out List<CPDFEditTextArea> textAreas, out CPDFPage pdfPage, out CPDFEditPage editPage)
        {
            textAreas = new List<CPDFEditTextArea>();
            editPage = null;
            pdfPage = null;
            if (ToolView == null)
            {
                return;
            }

            if (EditEvents != null && EditEvents.Count>0 )
            {
                try
                {
                    CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
                    pdfPage = pdfDoc.PageAtIndex(EditEvents.FirstOrDefault().PageIndex);
                    editPage = pdfPage.GetEditPage();
                    List<CPDFEditArea> editAreas = editPage.GetEditAreaList();
                    foreach (TextEditParam editEvent in EditEvents)
                    {
                        if (editAreas != null && editAreas.Count > editEvent.EditIndex)
                        {
                            textAreas.Add(editAreas[editEvent.EditIndex] as CPDFEditTextArea);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        #endregion

        private void chkMulti_Click(object sender, RoutedEventArgs e)
        {
            ToolView.SetIsMultiSelected((e.Source as CheckBox).IsChecked.GetValueOrDefault());
        }

        private void chkEditPen_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source as CheckBox).IsChecked.GetValueOrDefault())
            {
                ToolView.SetEditPen(null);
            }
            else
            {
                ToolView.SetEditPen(new Pen()
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Thickness = 0
                });
            }
            ShowBorder = ToolView.GetEditPen() == null || ToolView.GetEditPen().Thickness != 0;

            ToolView.GetCPDFViewer().UpdateRenderFrame();
        }

        protected bool UpdateProper<T>(ref T properValue,
                               T newValue,
                               [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

