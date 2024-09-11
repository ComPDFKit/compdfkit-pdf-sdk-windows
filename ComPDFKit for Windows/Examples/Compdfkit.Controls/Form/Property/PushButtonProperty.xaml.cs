using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using System.Windows.Input;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class PushButtonProperty : UserControl,INotifyPropertyChanged
    {
        public bool isFontStyleCmbClicked;
        public bool isFontCmbClicked;

        private PushButtonParam widgetParam = null;
        private CPDFPushButtonWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        private string _HintText;
        public string HintText
        {
            get => _HintText;
            set => SetField(ref _HintText, value);
        }

        bool IsLoadedData = false;
        public PushButtonProperty()
        {
            InitializeComponent();
            DataContext = this;
        }


        #region Loaded 
        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (PushButtonParam)annotParam;
            cPDFAnnotation = (CPDFPushButtonWidget)annotation;
            pdfViewerControl = cPDFViewer;
            cPDFDocument = doc;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeCmb.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);
            FieldNameText.Text = widgetParam.FieldName;
            TopTabControl.SelectedIndex = 2; 

            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            BorderColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.LineColor));
            BackgroundColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.BgColor));
            TextColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.FontColor));

            string familyName = string.Empty;
            string styleName = string.Empty;

            CPDFFont.GetFamilyStyleName(widgetParam.FontName,ref familyName,ref styleName);

            FontCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys.ToList();

            SetFontName(familyName);
            SetFontStyle(styleName); 
            SetFontSize(widgetParam.FontSize);
            
            ItemText.Text = widgetParam.Text;
            SetActionContext();
            IsLoadedData = true;

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void SetActionContext()
        {
            switch (widgetParam.Action)
            {
                case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                    TextAlignmentCmb.SelectedIndex = 1;
                    ActionContentText.Visibility = Visibility.Visible;
                    ActionContentText.Text = (widgetParam.DestinationPageIndex + 1).ToString();
                    break;
                case C_ACTION_TYPE.ACTION_TYPE_URI:
                    TextAlignmentCmb.SelectedIndex = 2;
                    ActionContentText.Visibility = Visibility.Visible;
                    ActionContentText.Text = widgetParam.Uri;
                    break;
                default:
                    TextAlignmentCmb.SelectedIndex = 0;
                    ActionContentText.Text = "";
                    break;
            }
        }
        private void SetFontSize(double size)
        {
            int index = SizeList.IndexOf((int)size);
            FontSizeCmb.SelectedIndex = index;
        }

        private void SetFontStyle(bool IsItalic, bool IsBold)
        {
            int index = 0;
            if (IsItalic && IsBold)
            {
                index = 3;
            }
            else if (IsItalic)
            {
                index = 2;
            }
            else if (IsBold)
            {
                index = 1;
            }
            FontStyleCmb.SelectedIndex = index;
        }

        private void SetFontName(string fontName)
        {
            FontCmb.SelectedValue = fontName;
            if (FontCmb.SelectedValue != null)
            {
                FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
            }
        }

        private void SetFontStyle(string fontStyle)
        {
            FontStyleCmb.SelectedValue = fontStyle;
        }
        #endregion

        #region Updata

        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                cPDFAnnotation.SetFieldName((sender as TextBox).Text);
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }


        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                cPDFAnnotation.SetFlags(ParamConverter.GetFormFlags((ParamConverter.FormField)(sender as ComboBox).SelectedIndex, cPDFAnnotation));
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }


        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.B;
                cPDFAnnotation.SetWidgetBorderRGBColor(Color);
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }


        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.B;
                cPDFAnnotation.SetWidgetBgRGBColor(Color);
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.B;
                CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                cTextAttribute.FontColor = Color;
                cPDFAnnotation.SetTextAttribute(cTextAttribute);
                cPDFAnnotation.UpdateFormAp();
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void FontCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (isFontCmbClicked)
                {
                    FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
                    FontStyleCmb.SelectedIndex = 0;

                    PushButtonHistory history = new PushButtonHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

                    CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                    string psFontName = string.Empty;
                    CPDFFont.GetPostScriptName(FontCmb.SelectedValue.ToString(), FontStyleCmb.SelectedValue.ToString(), ref psFontName);
                    cTextAttribute.FontName = psFontName;
                    cPDFAnnotation.SetTextAttribute(cTextAttribute);
                    cPDFAnnotation.UpdateFormAp();
                    pdfViewerControl.UpdateAnnotFrame();

                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
                else
                {
                    FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
                }
                isFontCmbClicked = false;
            }
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (isFontStyleCmbClicked)
                {
                    PushButtonHistory history = new PushButtonHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

                    CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                    string psFontName = string.Empty;
                    CPDFFont.GetPostScriptName(FontCmb.SelectedValue.ToString(), FontStyleCmb.SelectedValue.ToString(), ref psFontName);
                    cTextAttribute.FontName = psFontName;
                    cPDFAnnotation.SetTextAttribute(cTextAttribute);
                    cPDFAnnotation.UpdateFormAp();
                    pdfViewerControl.UpdateAnnotFrame();

                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
                isFontStyleCmbClicked = false;
            }
        }

        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();
                cTextAttribute.FontSize = Convert.ToSingle((sender as ComboBox).SelectedItem);
                cPDFAnnotation.SetTextAttribute(cTextAttribute);
                cPDFAnnotation.UpdateFormAp();
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }
        #endregion

        private void TextAlignmentCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                ActionContentText.Style = null;
                switch (TextAlignmentCmb.SelectedIndex)
                {
                    case 0:
                        ActionContentText.Visibility = Visibility.Collapsed;
                        RemoveAction();
                        break;
                    case 1:
                        ActionContentText.Visibility = Visibility.Visible;
                        HintText = LanguageHelper.PropertyPanelManager.GetString("Holder_Jump") + pdfViewerControl.GetCPDFViewer().GetDocument().PageCount;
                        ActionContentText.Style = this.FindResource("txtboxStyle") as Style;
                        ActionContentText.Text = "";
                        break;
                    case 2:
                        ActionContentText.Visibility = Visibility.Visible;
                        HintText = "https://www.compdf.com";
                        ActionContentText.Style = this.FindResource("txtboxStyle") as Style;
                        ActionContentText.Text = "";
                        break;
                    default:
                        break;
                }
                if (ActionContentText.Visibility != Visibility.Collapsed && ActionContentText != null)
                    AddAction();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void RemoveAction()
        {
            CPDFAction action = new CPDFAction(C_ACTION_TYPE.ACTION_TYPE_UNKNOWN);
            cPDFAnnotation.SetButtonAction(action);
        }

        private void AddAction()
        {
            Dictionary<C_ACTION_TYPE, string> ActionDict = new Dictionary<C_ACTION_TYPE, string>();
            if (TextAlignmentCmb.SelectedIndex == 1 && !string.IsNullOrEmpty(TextAlignmentCmb.Text))
            {
                int page = 0;
                int.TryParse(ActionContentText.Text.Trim(), out page);
                if (page <= 0 || page > pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument().PageCount)
                {
                    page = 1;
                }
                if (page - 1 >= 0)
                {
                    CPDFGoToAction gotoAction = new CPDFGoToAction();
                    CPDFDestination destination = new CPDFDestination();
                    destination.Position_X = 0;
                    destination.Position_Y = 0;
                    destination.PageIndex = page - 1;
                    gotoAction.SetDestination(cPDFDocument, destination);
                    cPDFAnnotation.SetButtonAction(gotoAction);
                }
            }
            if (TextAlignmentCmb.SelectedIndex == 2)
            {
                CPDFUriAction uriAction = new CPDFUriAction();
                if (string.IsNullOrEmpty(ActionContentText.Text.Trim()))
                {
                    uriAction.SetUri(@"https://www.compdf.com");
                }
                else
                {
                    uriAction.SetUri(ActionContentText.Text.Trim());
                }
                cPDFAnnotation.SetButtonAction(uriAction);
            }

            pdfViewerControl.UpdateAnnotFrame();
        }
        private void ItemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                cPDFAnnotation.SetButtonTitle((sender as TextBox).Text);
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void ActionContentText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                PushButtonHistory history = new PushButtonHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                AddAction();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void FontCmb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ComboBox)
            {
                isFontCmbClicked = true;
            }
        } 

        private void FontStyleCmb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ComboBox)
            {
                isFontStyleCmbClicked = true;
            }
        }
    }
}
