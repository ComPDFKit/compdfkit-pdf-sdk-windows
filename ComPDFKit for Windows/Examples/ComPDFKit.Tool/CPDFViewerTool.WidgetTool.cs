using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Widget;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Tool
{
    public class WidgetClickArgs: EventArgs
    {
        public bool Handled { get; set; }
        public BaseWidget Widget { get; set; }
        public UIElement UI {  get; set; }
    }
    public partial class CPDFViewerTool
    {
        public static DependencyProperty PopupAttachDataProperty = DependencyProperty.Register("PopupAttachData", typeof(BaseAnnot), typeof(CPDFViewerTool));

        public event EventHandler<WidgetClickArgs> WidgetActionHandler;
        public event EventHandler<CPDFAnnotation> WidgetCreatedHandler;

        private CustomizeLayer formPopLayer=null;
        // Inner default pop-up control
        private bool isInternalPopup;

        public bool IsAnnotReadOnly(BaseAnnot checkAnnot)
        {
            AnnotData annotData = checkAnnot?.GetAnnotData();
            if (annotData != null && annotData.Annot?.IsValid()==true)
            {
                return annotData.Annot.GetIsReadOnly();
            }
            return false;
        }

        public bool ShowFormHitPop(BaseWidget hitForm)
        {
            List<C_WIDGET_TYPE> formTypeList = new List<C_WIDGET_TYPE>()
            {
                C_WIDGET_TYPE.WIDGET_TEXTFIELD,
                C_WIDGET_TYPE.WIDGET_LISTBOX,
                C_WIDGET_TYPE.WIDGET_COMBOBOX
            };

            if(hitForm != null && formTypeList.Contains(hitForm.GetFormType()))
            {
                UIElement newUI = null;
                if (IsAnnotReadOnly(hitForm)) 
                {
                    return false;
                }
             
                switch(hitForm.GetFormType())
                {
                    case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                        newUI=BuildPopTextUI(hitForm);
                        break;
                    case C_WIDGET_TYPE.WIDGET_LISTBOX:
                        newUI=BuildPopListBoxUI(hitForm);
                        break;
                    case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                        newUI=BuildPopComboBoxUI(hitForm);
                        break;
                    default:
                        break;
                }
                if (newUI!=null)
                {
                    ShowFormHitPop(newUI,hitForm);
                    isInternalPopup = true;
                }
            }
            return false;
        }

        public bool ShowFormHitPop(UIElement customui,BaseWidget hitForm)
        {
            if (customui!= null && hitForm!=null)
            {
                isInternalPopup = false;
                customui.SetValue(PopupAttachDataProperty, hitForm);
                HideWidgetHitPop();
                if(formPopLayer==null)
                {
                    formPopLayer= new CustomizeLayer();
                }
                int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
                formPopLayer.Children.Clear();
                formPopLayer.Children.Add(customui);
                formPopLayer.Arrange();
                PDFViewer.InsertView(selectedRectViewIndex, formPopLayer);
            }
            return false;
        }

        /// <summary>
        /// Remove Form pop-up control
        /// </summary>
        public void HideWidgetHitPop()
        {
            PDFViewer?.RemoveView(formPopLayer);
        }

        private string GetFontName(string pdfFontName)
        {
            string fontName;
            switch (GetFontType(pdfFontName))
            {
                case FontType.Courier:
                    fontName = "Courier New";
                    break;
                case FontType.Helvetica:
                    fontName = "Arial";
                    break;
                case FontType.Times_Roman:
                    fontName = "Times New Roman";
                    break;
                default:
                    fontName = "Arial";
                    break;
            }
            return fontName;
        }

        private void SetFormRotateTransform(FrameworkElement formui,AnnotData annotData)
        {
            RotateTransform rotateTrans = new RotateTransform();
            rotateTrans.Angle = -90 * annotData.Rotation;
            rotateTrans.CenterX = annotData.PaintRect.Width / 2;
            rotateTrans.CenterY = annotData.PaintRect.Height / 2;
            Rect rotateRect = rotateTrans.TransformBounds(annotData.PaintRect);

            formui.Width = rotateRect.Width;
            formui.Height = rotateRect.Height;
            formui.SetValue(Canvas.LeftProperty, annotData.PaintRect.Left + rotateTrans.CenterX - rotateRect.Width / 2);
            formui.SetValue(Canvas.TopProperty, annotData.PaintRect.Top + rotateTrans.CenterY - rotateRect.Height / 2);

            rotateTrans.Angle = 90 * annotData.Rotation;
            rotateTrans.CenterX = rotateRect.Width / 2;
            rotateTrans.CenterY = rotateRect.Height / 2;
            formui.RenderTransform = rotateTrans;
        }

        protected UIElement BuildPopTextUI(BaseWidget textForm)
        {
            try
            {
                if (textForm != null && textForm.GetFormType() == C_WIDGET_TYPE.WIDGET_TEXTFIELD)
                {
                    AnnotData annotData = textForm.GetAnnotData();
                    CPDFTextWidget textWidget = annotData.Annot as CPDFTextWidget;
                    if (textWidget == null)
                    {
                       return null;
                    }

                    TextBox textui = new TextBox();
                    CTextAttribute textAttribute = textWidget.GetTextAttribute();
                    byte transparency = textWidget.GetTransparency();
                    textui.FontSize = textAttribute.FontSize* annotData.CurrentZoom/72D*96D;
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.White;
                    byte[] colorArray = new byte[3];
                    if (textWidget.GetWidgetBorderRGBColor(ref colorArray))
                    {
                        borderColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    if (textWidget.GetWidgetBgRGBColor(ref colorArray))
                    {
                        backgroundColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    textui.Foreground = new SolidColorBrush(textColor);
                    textui.BorderBrush = new SolidColorBrush(borderColor);
                    textui.Background = new SolidColorBrush(backgroundColor);

                    textui.BorderThickness = new Thickness(textWidget.GetBorderWidth()*annotData.CurrentZoom);
                    textui.Text = textWidget.Text;

                    textui.FontFamily = new FontFamily(GetFontName(textAttribute.FontName));
                    textui.FontWeight = IsBold(textAttribute.FontName) ? FontWeights.Bold : FontWeights.Normal;
                    textui.FontStyle = IsItalic(textAttribute.FontName) ? FontStyles.Italic : FontStyles.Normal;


                    if (textWidget.IsMultiLine)
                    {
                        textui.AcceptsReturn = true;
                        textui.TextWrapping = TextWrapping.Wrap;
                    }
                    else
                    {
                        textui.VerticalContentAlignment = VerticalAlignment.Center;
                    }

                    switch (textWidget.Alignment)
                    {
                        case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                            textui.TextAlignment = TextAlignment.Left;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                            textui.TextAlignment = TextAlignment.Right;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                            textui.TextAlignment = TextAlignment.Center;
                            break;
                        default:
                            break;
                    }

                    SetFormRotateTransform(textui, annotData);
                    textui.Loaded += (object sender, RoutedEventArgs e) =>
                    {
                        textui.Focus();
                        textui.CaretIndex = textui.Text.Length;
                    };
                    CPDFViewer viewer = GetCPDFViewer();
                    textui.LostFocus += (object sender, RoutedEventArgs e) =>
                    {
                        WidgetClickArgs eventparam = new WidgetClickArgs();
                        BaseWidget currentForm = textui.GetValue(PopupAttachDataProperty) as BaseWidget;
                        eventparam.Widget = currentForm;
                        eventparam.UI = textui;
                        WidgetActionHandler?.Invoke(this, eventparam);

                        if (currentForm != null && eventparam.Handled == false)
                        {
                            AnnotData formData = currentForm.GetAnnotData();
                            CPDFTextWidget updateWidget = formData.Annot as CPDFTextWidget;

                            if (updateWidget!=null && updateWidget.Text!=textui.Text)
                            {
                                CPDFDocument doc= viewer.GetDocument();
                                TextBoxHistory textHistory=new TextBoxHistory();
                                textHistory.Action= HistoryAction.Update;
                                textHistory.PDFDoc = doc;
                                textHistory.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                updateWidget.SetText(textui.Text);
                                textHistory.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                updateWidget.UpdateFormAp();
                                viewer?.UpdateAnnotFrame();
                                viewer?.UndoManager.AddHistory(textHistory);
                            }
                        }
                    };

                    return textui;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        protected UIElement BuildPopListBoxUI(BaseWidget listboxForm)
        {
            try
            {
                if (listboxForm != null && listboxForm.GetFormType() == C_WIDGET_TYPE.WIDGET_LISTBOX)
                {
                    AnnotData annotData = listboxForm.GetAnnotData();
                    ListBox listui = new ListBox();
                    CPDFListBoxWidget listWidget = annotData.Annot as CPDFListBoxWidget;
                    if (listWidget == null)
                    {
                        return null;
                    }

                    CTextAttribute textAttribute = listWidget.GetTextAttribute();
                    byte transparency = listWidget.GetTransparency();
                    listui.FontSize = textAttribute.FontSize * annotData.CurrentZoom;
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.White;
                    byte[] colorArray = new byte[3];
                    if (listWidget.GetWidgetBorderRGBColor(ref colorArray))
                    {
                        borderColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    if (listWidget.GetWidgetBgRGBColor(ref colorArray))
                    {
                        backgroundColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    listui.Foreground = new SolidColorBrush(textColor);
                    listui.BorderBrush = new SolidColorBrush(borderColor);
                    listui.Background = new SolidColorBrush(backgroundColor);
                    listui.BorderThickness = new Thickness(listWidget.GetBorderWidth() * annotData.CurrentZoom);
                    listui.FontFamily = new FontFamily(GetFontName(textAttribute.FontName));
                    listui.FontStyle = IsItalic(textAttribute.FontName) ? FontStyles.Italic : FontStyles.Normal;
                    listui.FontWeight = IsBold(textAttribute.FontName) ? FontWeights.Bold : FontWeights.Normal;

                    CWidgetItem selectItem = listWidget.GetSelectedItem();
                    CWidgetItem[] listItems = listWidget.LoadWidgetItems();

                    if (listItems != null)
                    {
                        foreach (CWidgetItem item in listItems)
                        {
                            ListBoxItem addItem = new ListBoxItem()
                            {
                                FontSize = listui.FontSize,
                                Content = item.Text
                            };
                            listui.Items.Add(addItem);
                            if (selectItem == null)
                            {
                                continue;
                            }
                            if (selectItem.Value == item.Value && selectItem.Text == item.Text)
                            {
                                listui.SelectedItem = addItem;
                            }
                        }
                    }

                    SetFormRotateTransform(listui, annotData);

                    CPDFViewer viewer = GetCPDFViewer();
                    listui.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                    {
                        WidgetClickArgs eventparam = new WidgetClickArgs();
                        BaseWidget currentForm = listui.GetValue(PopupAttachDataProperty) as BaseWidget;
                        eventparam.Widget = currentForm;
                        eventparam.UI = listui;
                        WidgetActionHandler?.Invoke(this, eventparam);

                        if (currentForm != null && eventparam.Handled == false)
                        {
                            AnnotData formData = currentForm.GetAnnotData();
                            CPDFListBoxWidget updateWidget = formData.Annot as CPDFListBoxWidget;
                            if (updateWidget != null)
                            {
                                int selectIndex = -1;
                                if (listItems != null && listItems.Length > 0)
                                {
                                    for (int i = 0; i < listItems.Length; i++)
                                    {
                                        CWidgetItem item = listItems[i];
                                     
                                        if (selectItem != null && selectItem.Text == item.Text && selectItem.Value == item.Value)
                                        {
                                            selectIndex = i;
                                            break;
                                        }
                                    }
                                }

                                if (selectIndex != listui.SelectedIndex)
                                {
                                    CPDFDocument doc = viewer.GetDocument();
                                    ListBoxHistory listboxHistory = new ListBoxHistory();
                                    listboxHistory.Action = ComPDFKitViewer.Helper.HistoryAction.Update;
                                    listboxHistory.PDFDoc = doc;
                                    listboxHistory.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                    updateWidget.SelectItem(listui.SelectedIndex);
                                    listboxHistory.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                    updateWidget?.UpdateFormAp();
                                    viewer?.UpdateAnnotFrame();
                                    viewer?.UndoManager.AddHistory(listboxHistory);
                                }
                            }
                        }
                    };

                    return listui;
                }
            }
            catch (Exception ex)
            {

            }
           
            return null;
        }

        protected UIElement BuildPopComboBoxUI(BaseWidget comboboxForm)
        {
            try
            {
                if (comboboxForm != null && comboboxForm.GetFormType() == C_WIDGET_TYPE.WIDGET_COMBOBOX)
                {
                    AnnotData annotData = comboboxForm.GetAnnotData();
                    ComboBox comboboxui=new ComboBox();
                    CPDFComboBoxWidget comboboxWidget=annotData.Annot as CPDFComboBoxWidget;
                    if(comboboxWidget==null)
                    {
                        return null;
                    }

                    CWidgetItem[] comboboxItems = comboboxWidget.LoadWidgetItems();
                    CWidgetItem selectItem= comboboxWidget.GetSelectedItem();
                    CTextAttribute textAttribute = comboboxWidget.GetTextAttribute();
                    byte transparency = comboboxWidget.GetTransparency();
                    comboboxui.FontSize = textAttribute.FontSize*annotData.CurrentZoom/72D*96D;
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.White;
                    byte[] colorArray = new byte[3];
                    if (comboboxWidget.GetWidgetBorderRGBColor(ref colorArray))
                    {
                        borderColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    if (comboboxWidget.GetWidgetBgRGBColor(ref colorArray))
                    {
                        backgroundColor = Color.FromRgb(colorArray[0], colorArray[1], colorArray[2]);
                    }

                    if (comboboxItems != null)
                    {
                        foreach (CWidgetItem item in comboboxItems)
                        {
                            ComboBoxItem comboItem = new ComboBoxItem();
                            comboItem.FontSize = comboboxui.FontSize;
                            comboItem.Content = item.Text;
                            comboboxui.Items.Add(comboItem);

                            if (selectItem == null)
                            {
                                continue;
                            }
                            if (selectItem.Value == item.Value && selectItem.Text == item.Text)
                            {
                                comboboxui.SelectedItem = comboItem;
                            }
                        }
                    }

                    comboboxui.Text = comboboxWidget.GetSelectedItem().Text;
                    comboboxui.Foreground = new SolidColorBrush(textColor);
                    comboboxui.BorderBrush = new SolidColorBrush(borderColor);
                    comboboxui.Background = new SolidColorBrush(backgroundColor);
                    comboboxui.BorderThickness = new Thickness(comboboxWidget.GetBorderWidth()*annotData.CurrentZoom);
                    comboboxui.FontFamily = new FontFamily(GetFontName(textAttribute.FontName));
                    comboboxui.FontStyle = IsItalic(textAttribute.FontName) ? FontStyles.Italic : FontStyles.Normal;
                    comboboxui.FontWeight = IsBold(textAttribute.FontName) ? FontWeights.Bold : FontWeights.Normal;
                    SetFormRotateTransform(comboboxui, annotData);

                    CPDFViewer viewer = GetCPDFViewer();
                    comboboxui.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                    {
                        WidgetClickArgs eventparam = new WidgetClickArgs();
                        BaseWidget currentForm= comboboxui.GetValue(PopupAttachDataProperty) as BaseWidget;
                        eventparam.Widget = currentForm;
                        eventparam.UI = comboboxui;
                        WidgetActionHandler?.Invoke(this, eventparam);

                        if (currentForm != null && eventparam.Handled == false)
                        {
                            AnnotData formData = currentForm.GetAnnotData();
                            CPDFComboBoxWidget updateWidget = formData.Annot as CPDFComboBoxWidget;
                            if (updateWidget != null)
                            {
                                int selectIndex = -1;
                                if (comboboxItems != null && comboboxItems.Length > 0)
                                {
                                    for (int i = 0; i < comboboxItems.Length; i++)
                                    {
                                        CWidgetItem item = comboboxItems[i];

                                        if (selectItem != null && selectItem.Text == item.Text && selectItem.Value == item.Value)
                                        {
                                            selectIndex = i;
                                            break;
                                        }
                                    }
                                }

                                if (selectIndex != comboboxui.SelectedIndex)
                                {
                                    CPDFDocument doc = viewer.GetDocument();
                                    ComboBoxHistory comboboxHistory = new ComboBoxHistory();
                                    comboboxHistory.Action = ComPDFKitViewer.Helper.HistoryAction.Update;
                                    comboboxHistory.PDFDoc = doc;
                                    comboboxHistory.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                    updateWidget.SelectItem(comboboxui.SelectedIndex);
                                    comboboxHistory.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, formData.PageIndex, formData.Annot);
                                    updateWidget?.UpdateFormAp();
                                    viewer?.UpdateAnnotFrame();
                                    viewer?.UndoManager.AddHistory(comboboxHistory);
                                }
                            }
                        }
                    };

                    return comboboxui;
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }

        private void UpdateTextUI(TextBox textui,BaseWidget hitform)
        {
            if(textui!=null && hitform!=null)
            {
                AnnotData annotData = hitform.GetAnnotData();
                CPDFTextWidget textWidget = annotData.Annot as CPDFTextWidget;
                if (textWidget == null)
                {
                    return;
                }
                CTextAttribute textAttribute = textWidget.GetTextAttribute();
                textui.FontSize = textAttribute.FontSize * annotData.CurrentZoom;
                SetFormRotateTransform(textui, annotData);
                textui.SetValue(PopupAttachDataProperty, hitform);
                formPopLayer?.Arrange();
            }
        }

        private void UpdateListBoxUI(ListBox listboxui,BaseWidget hitform)
        {
            if(listboxui!=null &&hitform!=null)
            {
                AnnotData annotData = hitform.GetAnnotData();
                CPDFListBoxWidget listWidget=annotData.Annot as CPDFListBoxWidget;
                if(listWidget == null)
                {
                    return;
                }

                CTextAttribute textAttribute = listWidget.GetTextAttribute();
                listboxui.FontSize = textAttribute.FontSize * annotData.CurrentZoom;
                listboxui.BorderThickness = new Thickness(listWidget.GetBorderWidth() * annotData.CurrentZoom);
                foreach(ListBoxItem item in listboxui.Items)
                {
                    item.FontSize = listboxui.FontSize;
                }
                SetFormRotateTransform(listboxui, annotData);
                listboxui.SetValue(PopupAttachDataProperty, hitform);
                formPopLayer?.Arrange();
            }
        }

        private void UpdateComboboxUI(ComboBox comboboxui,BaseWidget hitform)
        {
            if (comboboxui != null && hitform != null)
            {
                AnnotData annotData = hitform.GetAnnotData();
                CPDFComboBoxWidget comboboxWidget = annotData.Annot as CPDFComboBoxWidget;
                if (comboboxWidget == null)
                {
                    return;
                }

                CTextAttribute textAttribute = comboboxWidget.GetTextAttribute();
                comboboxui.FontSize = textAttribute.FontSize * annotData.CurrentZoom;
                comboboxui.BorderThickness = new Thickness(comboboxWidget.GetBorderWidth() * annotData.CurrentZoom);
               
                foreach (ComboBoxItem item in comboboxui.Items)
                {
                    item.FontSize = comboboxui.FontSize;
                }
                SetFormRotateTransform(comboboxui, annotData);
                comboboxui.SetValue(PopupAttachDataProperty, hitform);
                formPopLayer?.Arrange();
            }
        }

        protected void UpdateFormHitPop()
        {
            if(formPopLayer==null || formPopLayer.Children.Count==0 || !isInternalPopup)
            {
                return;
            }
            FrameworkElement popui = formPopLayer.Children[0] as FrameworkElement;
            if(popui==null)
            {
                return;
            }
            try
            {
                BaseWidget hitForm = popui.GetValue(PopupAttachDataProperty) as BaseWidget;
                if(hitForm==null)
                {
                    return;
                }
                AnnotLayer annotLayer = PDFViewer.GetViewForTag(PDFViewer.GetAnnotViewTag()) as AnnotLayer;
                BaseAnnot baseAnnot = hitForm;
                annotLayer.GetUpdate(ref baseAnnot);
                hitForm = baseAnnot as BaseWidget;

                if (hitForm.GetFormType()==C_WIDGET_TYPE.WIDGET_TEXTFIELD && (popui is TextBox))
                {
                    UpdateTextUI((TextBox)popui, hitForm);
                }

                if (hitForm.GetFormType() == C_WIDGET_TYPE.WIDGET_LISTBOX && (popui is ListBox))
                {
                    UpdateListBoxUI((ListBox)popui,hitForm);
                }

                if (hitForm.GetFormType() == C_WIDGET_TYPE.WIDGET_COMBOBOX && (popui is ComboBox))
                {
                    UpdateComboboxUI((ComboBox)popui, hitForm);
                }
            }
            catch(Exception ex)
            {

            }
        }

        protected void FormClickProcess()
        {
            BaseWidget currentForm = PDFViewer?.AnnotHitTest() as BaseWidget;
            if (currentForm != null)
            {
                List<C_WIDGET_TYPE> formTypeList = new List<C_WIDGET_TYPE>()
                {
                    C_WIDGET_TYPE.WIDGET_CHECKBOX,
                    C_WIDGET_TYPE.WIDGET_RADIOBUTTON,
                    C_WIDGET_TYPE.WIDGET_PUSHBUTTON
                };

                if(formTypeList.Contains(currentForm.GetFormType()))
                {
                    WidgetClickArgs eventparam = new WidgetClickArgs();
                    eventparam.Widget = currentForm;
                    WidgetActionHandler?.Invoke(this, eventparam);
                    if (eventparam.Handled == false)
                    {
                        switch (currentForm.GetFormType())
                        {
                            case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                                FormRadioButtonClick(currentForm);
                                break;
                            case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                                FormCheckBoxClick(currentForm);
                                break;
                            case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                                FormPushButtonClick(currentForm);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void FormRadioButtonClick(BaseWidget clickForm)
        {
            if(clickForm!=null && clickForm.GetFormType()==C_WIDGET_TYPE.WIDGET_RADIOBUTTON)
            {
                AnnotData formData = clickForm.GetAnnotData();
                CPDFRadioButtonWidget updateWidget = formData.Annot as CPDFRadioButtonWidget;
                CPDFViewer viewer = GetCPDFViewer();
                CPDFDocument doc = viewer?.GetDocument();
                if (viewer != null && doc != null)
                {
                    if (updateWidget.IsChecked() == false)
                    {
                        GroupHistory historyGroup=new GroupHistory();

                        for (int i = 0; i < doc.PageCount; i++)
                        {
                            SetFormButtonChecked(i, doc, updateWidget.GetFieldName(), false, historyGroup);
                        }
                        RadioButtonHistory history=new RadioButtonHistory();
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, updateWidget.Page.PageIndex, updateWidget);
                        history.PDFDoc= doc;
                        history.Action=HistoryAction.Update;
                        updateWidget.SetChecked(true);
                        updateWidget.UpdateFormAp();
                        viewer.UpdateAnnotFrame();
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(doc, updateWidget.Page.PageIndex, updateWidget);
                        historyGroup?.Histories.Add(history);
                        viewer?.UndoManager?.AddHistory(historyGroup);
                    }
                }
            }
        }

        private void FormCheckBoxClick(BaseWidget clickForm) 
        {
            if (clickForm != null && clickForm.GetFormType() == C_WIDGET_TYPE.WIDGET_CHECKBOX)
            {
                AnnotData formData = clickForm.GetAnnotData();
                CPDFCheckBoxWidget updateWidget = formData.Annot as CPDFCheckBoxWidget;
                bool isCheck = !updateWidget.IsChecked();
                CPDFViewer viewer = GetCPDFViewer();
                CPDFDocument doc = viewer?.GetDocument();
                if (viewer != null && doc != null)
                {
                    GroupHistory historyGroup = new GroupHistory();

                    for (int i = 0; i < doc.PageCount; i++)
                    {
                        SetFormButtonChecked(i, doc, updateWidget.GetFieldName(), isCheck, historyGroup);
                    }
                    if(historyGroup.Histories.Count > 0)
                    {
                        viewer?.UndoManager?.AddHistory(historyGroup);
                    }
                    updateWidget.UpdateFormAp();
                    viewer.UpdateAnnotFrame();
                }
            }
        }

        private void FormPushButtonClick(BaseWidget clickForm)
        {
            if (clickForm != null && clickForm.GetFormType() == C_WIDGET_TYPE.WIDGET_PUSHBUTTON)
            {
                AnnotData formData = clickForm.GetAnnotData();
                CPDFPushButtonWidget updateWidget = formData.Annot as CPDFPushButtonWidget;
                CPDFViewer viewer = GetCPDFViewer();
                CPDFDocument doc = viewer?.GetDocument();
                if (viewer != null && doc != null)
                {
                    CPDFAction action = updateWidget.GetButtonAction();
                    PDFActionHandler(action, doc, viewer);
                }
            }
        }

        internal void SetFormButtonChecked(int pageIndex, CPDFDocument currentDoc, string fieldName, bool isCheck,GroupHistory historyGroup)
        {
            if (pageIndex < 0 || currentDoc==null || pageIndex>=currentDoc.PageCount)
            {
                return;
            }
            CPDFPage docPage = currentDoc.PageAtIndex(pageIndex, false);
            if (docPage == null)
            {
                return;
            }
            List<CPDFAnnotation> docAnnots = docPage.GetAnnotations();

            if (docAnnots != null && docAnnots.Count > 0)
            {
                foreach (CPDFAnnotation annotCore in docAnnots)
                {
                    if (annotCore == null || annotCore.Type != C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                    {
                        continue;
                    }
                    CPDFWidget widget = (CPDFWidget)annotCore;
                    if (widget != null)
                    {
                        if (widget.WidgetType == C_WIDGET_TYPE.WIDGET_RADIOBUTTON && widget.GetFieldName() == fieldName)
                        {
                            RadioButtonHistory history = new RadioButtonHistory();
                            history.PDFDoc = currentDoc;
                            history.Action = HistoryAction.Update;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(currentDoc, pageIndex, widget);
                            widget.ResetForm();
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(currentDoc, pageIndex, widget);
                            historyGroup?.Histories.Add(history);
                        }

                        if (widget.WidgetType == C_WIDGET_TYPE.WIDGET_CHECKBOX && widget.GetFieldName() == fieldName)
                        {
                            CheckBoxHistory history = new CheckBoxHistory();
                            history.PDFDoc = currentDoc;
                            history.Action = HistoryAction.Update;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(currentDoc, pageIndex, widget);
                            (widget as CPDFCheckBoxWidget)?.SetChecked(isCheck);
                            widget.UpdateFormAp();
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(currentDoc, pageIndex, widget);
                            historyGroup?.Histories.Add(history);
                        }
                    }
                }
            }
        }

        internal void PDFActionHandler(CPDFAction action,CPDFDocument currentDoc,CPDFViewer viewer)
        {
            if (action != null && currentDoc!=null)
            {
                try
                {
                    switch (action.ActionType)
                    {
                        case C_ACTION_TYPE.ACTION_TYPE_NAMED:
                            {
                                CPDFNamedAction namedAction = action as CPDFNamedAction;
                                string name = namedAction.GetName();
                                int pageIndex = -1;
                                if (name.ToLower() == "firstpage")
                                {
                                    pageIndex = 0;
                                }
                                if (name.ToLower() == "lastpage")
                                {
                                    pageIndex = currentDoc.PageCount - 1;
                                }
                                if (name.ToLower() == "nextpage" && viewer.CurrentRenderFrame != null)
                                {
                                    pageIndex = Math.Min(viewer.CurrentRenderFrame.PageIndex + 1, currentDoc.PageCount - 1);
                                }
                                if (name.ToLower() == "prevpage" && viewer.CurrentRenderFrame != null)
                                {
                                    pageIndex = Math.Max(viewer.CurrentRenderFrame.PageIndex - 1, 0);
                                }
                                if (pageIndex != -1)
                                {
                                    viewer.GoToPage(pageIndex, new Point(0, 0));
                                }
                            }
                            break;
                        case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                            {
                                CPDFGoToAction gotoAction = action as CPDFGoToAction;
                                CPDFDestination dest = gotoAction.GetDestination(currentDoc);
                                if (dest != null)
                                {
                                    viewer.GoToPage(dest.PageIndex, new Point(0, 0));
                                }
                            }
                            break;
                        case C_ACTION_TYPE.ACTION_TYPE_GOTOR:
                            {
                                CPDFGoToRAction gotorAction = action as CPDFGoToRAction;
                                CPDFDestination dest = gotorAction.GetDestination(currentDoc);
                                if (dest != null)
                                {
                                    viewer.GoToPage(dest.PageIndex, new Point(0, 0));
                                }
                            }
                            break;
                        case C_ACTION_TYPE.ACTION_TYPE_URI:
                            {
                                CPDFUriAction uriAction = action as CPDFUriAction;
                                string uri = uriAction.GetUri();
                                if (!string.IsNullOrEmpty(uri))
                                {
                                    Process.Start(uri);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        internal void InvokeWidgetCreated(CPDFAnnotation annot)
        {
            if(annot!=null && annot.IsValid())
            {
                WidgetCreatedHandler?.Invoke(this, annot);
            }
        }
    }
}
