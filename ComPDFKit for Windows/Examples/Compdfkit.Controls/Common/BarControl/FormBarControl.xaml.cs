using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.Tool;
using ComPDFKit.Tool.SettingParam;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFFormBarControl : UserControl, INotifyPropertyChanged
    {
        private string textField = LanguageHelper.ToolBarManager.GetString("Button_TextField");
        private string checkBox = LanguageHelper.ToolBarManager.GetString("Button_Chb");
        private string radioButton = LanguageHelper.ToolBarManager.GetString("Button_Rdo");
        private string listBox = LanguageHelper.ToolBarManager.GetString("Button_ListBox");
        private string comboBox = LanguageHelper.ToolBarManager.GetString("Button_Cmb");
        private string pushButton = LanguageHelper.ToolBarManager.GetString("Button_Btn");
        private string signature = LanguageHelper.ToolBarManager.GetString("Button_Sig");
        private bool isFirstLoad = true;
        enum FromType
        {
            UnKnown = -1,
            Textbox,
            CheckBox,
            RadioButton,
            ListBox,
            ComboBox,
            Pushbutton,
            Signature,
        }

        #region Data

        public event EventHandler<C_WIDGET_TYPE> AnnotationPropertyChanged;
        private Dictionary<string, string> ButtonDict;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private PDFViewControl pdfViewer;

        private FromPropertyControl fromPropertyControl = null;

        #endregion

        #region Create Default UI

        public CPDFFormBarControl()
        {
            ButtonDict = new Dictionary<string, string>
            {
                {textField,"M0.75 2.25H20.25V17.75H0.75V2.25ZM18.75 11.75V3.75H2.25V16.25H14.25L18.75 11.75ZM11.75 5.5V8H10.25V7H8.5V13H9.25V14.5H6.25V13H7V7H5.25V8H3.75V5.5H11.75Z"},
                {checkBox, "M18.75 1.75H2.25V18.25H18.75V1.75ZM17.25 3.25V16.75H3.75V3.25H17.25ZM16.032 7.52861L14.968 6.47139L9.538 11.935L7.03204 9.41291L5.96796 10.4701L9.53873 14.0641L16.032 7.52861Z"},
                {radioButton,"M10.5 1.25C5.66751 1.25 1.75 5.16751 1.75 10C1.75 14.8325 5.66751 18.75 10.5 18.75C15.3325 18.75 19.25 14.8325 19.25 10C19.25 5.16751 15.3325 1.25 10.5 1.25ZM10.5 2.75C14.5041 2.75 17.75 5.99594 17.75 10C17.75 14.0041 14.5041 17.25 10.5 17.25C6.49594 17.25 3.25 14.0041 3.25 10C3.25 5.99594 6.49594 2.75 10.5 2.75ZM10.5 13C12.1569 13 13.5 11.6569 13.5 10C13.5 8.34315 12.1569 7 10.5 7C8.84315 7 7.5 8.34315 7.5 10C7.5 11.6569 8.84315 13 10.5 13Z"},
                {listBox,"M20.25 2.25H0.75V17.75H20.25V2.25ZM18.75 10.75V16.25H2.25V10.75H18.75ZM18.75 9.25V3.75H2.25V9.25H18.75ZM6.5 5.5H3.5V7.5H6.5V5.5ZM3.5 12.5H6.5V14.5H3.5V12.5Z"},
                {comboBox,"M0.75 2.25H20.25V10.75H16.25V17.75H0.75V10.75V9.25V2.25ZM2.25 10.75H14.75V16.25H2.25V10.75ZM18.75 9.25H16.25H2.25V3.75H18.75V9.25ZM15.5 8L17.5 5.5H13.5L15.5 8Z"},
                {pushButton,"M20.5 3H0.5V17H20.5V3ZM10.0918 10.3271C10.0918 12.3794 8.91406 13.6626 7.04199 13.6626C5.15674 13.6626 3.98779 12.3794 3.98779 10.3271C3.98779 8.27051 5.17432 6.99609 7.04199 6.99609C8.90967 6.99609 10.0918 8.2749 10.0918 10.3271ZM5.3457 10.3271C5.3457 11.6719 5.99609 12.5376 7.04199 12.5376C8.07471 12.5376 8.73389 11.6719 8.73389 10.3271C8.73389 8.97803 8.07031 8.12109 7.04199 8.12109C6.00928 8.12109 5.3457 8.97803 5.3457 10.3271ZM12.6758 13.5V11.562L13.3042 10.8062L15.1455 13.5H16.7363L14.2622 9.93604L16.5737 7.15869H15.0972L12.7549 10.0063H12.6758V7.15869H11.3486V13.5H12.6758Z"},
                {signature, "M7.08813 17.5815C8.62427 17.5815 9.82397 16.6738 10.6047 15.1504H20.5V13.8428H11.1316C11.4363 12.8652 11.614 11.7227 11.6584 10.4658C12.0583 10.3833 12.4773 10.3325 12.8962 10.3325C13.0422 10.3325 13.1248 10.4023 13.1248 10.5039C13.1248 10.98 12.801 11.3862 12.801 11.9956C12.801 12.5669 13.22 12.9414 13.8103 12.9414C14.4691 12.9414 15.1903 12.455 15.808 11.9475L16.1336 11.6713L16.6994 11.1736C16.9478 10.9597 17.1384 10.8149 17.2444 10.8149C17.3279 10.8149 17.3739 10.9147 17.4151 11.0681L17.5009 11.4329L17.5572 11.6434C17.7076 12.1446 18.0105 12.6875 18.8821 12.6875C19.0167 12.6875 19.2501 12.6207 19.5822 12.4871L19.8911 12.3562L20.2483 12.1926L19.988 10.8914L19.6787 11.0317L19.4291 11.1369L19.2391 11.2071C19.1591 11.2334 19.1014 11.2466 19.0662 11.2466C18.9293 11.2466 18.8686 11.0858 18.8206 10.8544L18.739 10.4051C18.6308 9.83926 18.4286 9.19629 17.6252 9.19629C17.0135 9.19629 16.3006 9.71182 15.7085 10.2028L15.3988 10.4649L15.1211 10.7027C14.8619 10.9218 14.6627 11.0752 14.5593 11.0752C14.5022 11.0752 14.4768 11.0498 14.4768 10.98C14.4768 10.925 14.4923 10.8566 14.513 10.7785L14.5802 10.5192C14.6009 10.4256 14.6165 10.3262 14.6165 10.2246C14.6165 9.41846 14.0325 8.93604 13.0803 8.93604C12.5916 8.93604 12.1091 8.98047 11.6394 9.06934C11.4109 5.62891 9.68433 3.08984 7.24683 3.08984C5.39966 3.08984 4.10474 4.49902 4.10474 6.46045C4.10474 8.60596 5.46313 10.4023 6.94849 11.5703C6.23755 12.2876 5.69165 13.0747 5.35522 13.8428H0.5V15.1504H5.0061C4.99341 15.252 4.98706 15.3472 4.98706 15.4487C4.98706 16.75 5.72339 17.5815 7.08813 17.5815ZM7.99585 10.6689C6.79614 9.74219 5.50122 8.32031 5.50122 6.44775C5.50122 5.31152 6.21851 4.49268 7.25317 4.49268C8.94165 4.49268 10.1477 6.61914 10.262 9.44385C9.43042 9.74854 8.6687 10.1675 7.99585 10.6689ZM1.39766 12.9448L2.32073 12.0218L3.2438 12.9448L3.99599 12.1926L3.0761 11.2727L3.99599 10.3528L3.24379 9.60065L2.32073 10.5237L1.39666 9.60601L0.64764 10.355L1.5717 11.2727L0.648633 12.1958L1.39766 12.9448ZM9.6272 13.8428H6.95483C7.24048 13.354 7.64673 12.8462 8.14185 12.3765L8.77184 12.737L9.7795 11.8768C9.53409 11.7317 9.35213 11.6084 9.23364 11.5068C9.54468 11.3037 9.8811 11.1133 10.2302 10.9482C10.1477 12.0464 9.93823 13.0303 9.6272 13.8428ZM7.25317 16.1851C6.73267 16.1851 6.45972 15.8359 6.45972 15.3091C6.45972 15.2583 6.46606 15.2075 6.47241 15.1504H8.948C8.47192 15.8042 7.89429 16.1851 7.25317 16.1851Z"},
            };
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoad)
            {
                foreach (KeyValuePair<string, string> data in ButtonDict)
                {
                    string Path = data.Value;
                    string name = data.Key;

                    Geometry annotationGeometry = Geometry.Parse(Path);
                    Path path = new Path
                    {
                        Width = 20,
                        Height = 20,
                        Data = annotationGeometry,
                        Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D))
                    };
                    CreateButtonForPath(path, name);
                }
                isFirstLoad = false;
            }
        }

        private void CreateButtonForPath(Path path, String name)
        {
            StackPanel stackPanel = new StackPanel();
            TextBlock textBlock = new TextBlock();
            if (path != null)
            {
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                stackPanel.Children.Add(path);
            }
            if (!string.IsNullOrEmpty(name))
            {
                textBlock.Text = name;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Margin = new Thickness(8, 0, 0, 0);
                textBlock.FontSize = 12;

                stackPanel.Children.Add(textBlock);
            }

            Style style = (Style)FindResource("RoundMarginToggleButtonStyle");
            ToggleButton button = new ToggleButton();
            button.BorderThickness = new Thickness(0);
            button.Padding = new Thickness(10, 5, 10, 5);
            button.Tag = name;
            button.ToolTip = name;
            button.Style = style;
            button.Content = stackPanel;
            button.Click -= FormBtn_Click;
            button.Click += FormBtn_Click;
            FormGrid.Children.Add(button);
        }

        #endregion

        #region Even Process
        public void InitWithPDFViewer(PDFViewControl pdfViewer, FromPropertyControl FromProperty)
        {
            this.pdfViewer = pdfViewer;
            fromPropertyControl = FromProperty;
        }


        public void CheckedButtonForName(string name)
        {
            foreach (UIElement child in FormGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    if (toggle.Tag.ToString() == name)
                    {
                        toggle.IsChecked = true;
                    }
                    else
                    {
                        toggle.IsChecked = false;
                    }
                }
            }
        }

        public void ClearAllToolState()
        {
            foreach (UIElement child in FormGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void ClearToolState(UIElement sender)
        {
            foreach (UIElement child in FormGrid.Children)
            {
                if (child is ToggleButton toggle && (child as ToggleButton) != (sender as ToggleButton))
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void FormBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearToolState(sender as ToggleButton);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_NONE);
            fromPropertyControl.SetPropertyForType(null, null,null);
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                switch (StringToType((sender as ToggleButton).Tag.ToString()))
                {
                    case FromType.Textbox:
                        CreateTextBox();
                        return;
                    case FromType.CheckBox:
                        CreateCheckBox();
                        return;
                    case FromType.RadioButton:
                        CreateRadioBtn();
                        return;
                    case FromType.ListBox:
                        CreateListBox();
                        return;
                    case FromType.ComboBox:
                        CreateComboBox();
                        return;
                    case FromType.Pushbutton:
                        CreatePushBtn();
                        return;
                    case FromType.Signature:
                        CreateSign();
                        return;
                    case FromType.UnKnown:
                    default:
                        break;
                }
                pdfViewer.SetToolType(ToolType.WidgetEdit);
            }
            else
            {
                pdfViewer.SetToolType(ToolType.WidgetEdit);
            }
        }

        private FromType StringToType(string type)
        {
            if (type == textField) return FromType.Textbox;

            if (type == checkBox) return FromType.CheckBox;

            if (type == radioButton) return FromType.RadioButton;

            if (type == listBox) return FromType.ListBox;

            if (type == comboBox) return FromType.ComboBox;

            if (type == pushButton) return FromType.Pushbutton;

            if (type == signature) return FromType.Signature;

            return FromType.UnKnown;
        }
        #endregion

        #region Create Form
        private string GetTime()
        {
            DateTime dateTime = DateTime.Now;
            return " " + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void CreateTextBox()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_TEXTFIELD);
            TextBoxParam textBoxParam = new TextBoxParam();
            textBoxParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            textBoxParam.WidgetType = C_WIDGET_TYPE.WIDGET_TEXTFIELD;
            textBoxParam.LineWidth = 1;
            textBoxParam.FontName = "Helvetica";
            textBoxParam.LineColor =new byte[] {0,0,0 };
            textBoxParam.FontColor = new byte[] { 0, 0, 0 };
            textBoxParam.Transparency = 255;
            textBoxParam.HasLineColor = true;
            textBoxParam.FieldName = "Text"+ GetTime();
            pdfViewer.SetAnnotParam(textBoxParam);
        }

        private void CreateRadioBtn()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_RADIOBUTTON);
            RadioButtonParam radioButtonParam = new RadioButtonParam();
            radioButtonParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            radioButtonParam.WidgetType = C_WIDGET_TYPE.WIDGET_RADIOBUTTON;
            radioButtonParam.CheckStyle = C_CHECK_STYLE.CK_CIRCLE;
            radioButtonParam.BorderStyle = C_BORDER_STYLE.BS_SOLID;
            radioButtonParam.LineColor = new byte[] { 0, 0, 0 };
            radioButtonParam.FontColor = new byte[] { 0, 0, 0 };
            radioButtonParam.Transparency = 255;
            radioButtonParam.HasLineColor = true;
            radioButtonParam.LineWidth = 2;
            radioButtonParam.FieldName = "Radio button" + GetTime();
            pdfViewer.SetAnnotParam(radioButtonParam);
        }

        private void CreateCheckBox()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_CHECKBOX);
            CheckBoxParam checkBoxParam = new CheckBoxParam();
            checkBoxParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            checkBoxParam.WidgetType = C_WIDGET_TYPE.WIDGET_CHECKBOX;
            checkBoxParam.CheckStyle = C_CHECK_STYLE.CK_CHECK;
            checkBoxParam.BorderStyle = C_BORDER_STYLE.BS_SOLID;
            checkBoxParam.LineColor = new byte[] { 0, 0, 0 };
            checkBoxParam.FontColor = new byte[] { 0, 0, 0 };
            checkBoxParam.Transparency = 255;
            checkBoxParam.HasLineColor = true;
            checkBoxParam.LineWidth = 1;
            checkBoxParam.FieldName = "Checkbox" + GetTime();
            pdfViewer.SetAnnotParam(checkBoxParam);
        }

        private void CreateComboBox()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_COMBOBOX);
            ComboBoxParam comboBoxParam = new ComboBoxParam();
            comboBoxParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            comboBoxParam.WidgetType = C_WIDGET_TYPE.WIDGET_COMBOBOX;
            comboBoxParam.LineColor = new byte[] { 0, 0, 0 };
            comboBoxParam.FontColor = new byte[] { 0, 0, 0 };
            comboBoxParam.Transparency = 255;
            comboBoxParam.HasLineColor = true;
            comboBoxParam.LineWidth = 1;
            comboBoxParam.FieldName = "Combobox" + GetTime();
            pdfViewer.SetAnnotParam(comboBoxParam);
        }

        private void CreateListBox()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_LISTBOX);
            ListBoxParam listBoxParam = new ListBoxParam();
            listBoxParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            listBoxParam.WidgetType = C_WIDGET_TYPE.WIDGET_LISTBOX;
            listBoxParam.LineColor = new byte[] { 0, 0, 0 };
            listBoxParam.FontColor = new byte[] { 0, 0, 0 };
            listBoxParam.Transparency = 255;
            listBoxParam.HasLineColor = true;
            listBoxParam.LineWidth = 1;
            listBoxParam.FieldName = "List" + GetTime();
            pdfViewer.SetAnnotParam(listBoxParam);
        }

        private void CreatePushBtn()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_PUSHBUTTON);
            PushButtonParam pushButtonParam = new PushButtonParam();
            pushButtonParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            pushButtonParam.WidgetType = C_WIDGET_TYPE.WIDGET_PUSHBUTTON;
            pushButtonParam.Text = "Push Button";
            pushButtonParam.LineWidth = 1;
            pushButtonParam.HasLineColor = true;
            pushButtonParam.Action = C_ACTION_TYPE.ACTION_TYPE_URI;
            pushButtonParam.Uri = @"https://www.compdf.com";
            pushButtonParam.LineColor = new byte[] { 0, 0, 0 };
            pushButtonParam.FontColor = new byte[] { 0, 0, 0 };
            pushButtonParam.Transparency = 255;
            pushButtonParam.FieldName = "Button" + GetTime();
            pdfViewer.SetAnnotParam(pushButtonParam);
        }

        private void CreateSign()
        {
            pdfViewer.SetToolType(ToolType.WidgetEdit);
            pdfViewer.SetCreateWidgetType(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS);
            SignatureParam signatureParam = new SignatureParam();
            signatureParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            signatureParam.WidgetType = C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS;
            signatureParam.LineWidth = 1;
            signatureParam.HasLineColor = true;
            signatureParam.LineColor = new byte[] { 0, 0, 0 };
            signatureParam.FontColor = new byte[] { 0, 0, 0 };
            signatureParam.Transparency = 255;
            signatureParam.FieldName= "Signature" + GetTime();
            pdfViewer.SetAnnotParam(signatureParam);
        }

        #endregion

    }
}
