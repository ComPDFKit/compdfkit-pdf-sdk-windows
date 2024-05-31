using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using System.Windows.Input;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class ComboBoxProperty : UserControl
    {
        public bool isFontCmbClicked;
        public bool isFontStyleCmbClicked;

        private ComboBoxParam widgetParam = null;
        private CPDFComboBoxWidget cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;
        private CPDFDocument cPDFDocument = null;
        private Dictionary<string, string> itemlists = null;

        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        bool IsLoadedData = false;

        public ComboBoxProperty()
        {
            InitializeComponent();
        }

        #region Loaded
        public void SetProperty(AnnotParam annotParam, CPDFAnnotation annotation, CPDFDocument doc, PDFViewControl cPDFViewer)
        {
            widgetParam = (ComboBoxParam)annotParam;
            cPDFAnnotation = (CPDFComboBoxWidget)annotation;
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
            FormFieldCmb.SelectedIndex = (int)ParamConverter.ConverterWidgetFormFlags(widgetParam.Flags, widgetParam.IsHidden);
            BorderColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.LineColor));
            BackgroundColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.BgColor));
            TextColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(widgetParam.FontColor));
            string familyName = string.Empty;
            string styleName = string.Empty;

            CPDFFont.GetFamlyStyleName(widgetParam.FontName, ref familyName, ref styleName);

            FontCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys.ToList();

            SetFontName(familyName);
            SetFontStyle(styleName);
            SetFontSize(widgetParam.FontSize);

            itemlists = widgetParam.OptionItems;

            if (itemlists != null)
            {
                foreach (string key in itemlists.Keys)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = key;
                    item.Tag = itemlists[key];
                    itemsListBox.Items.Add(item);
                }
                CheckListCount();
            }
            TopTabControl.SelectedIndex = 2;

            IsLoadedData = true;
            
            FontCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys.ToList();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void SetFontSize(double size)
        {
            int index = SizeList.IndexOf((int)size);
            FontSizeCmb.SelectedIndex = index;
        }

        private void SetFontStyle(string fontStyle)
        {
            FontStyleCmb.SelectedValue = fontStyle;
        }

        private void SetFontName(string fontName)
        {
            FontCmb.SelectedValue = fontName;
            if (FontCmb.SelectedValue != null)
            {
                FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontCmb.SelectedValue.ToString()];
            }
        }


        #endregion
        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                string text = (sender as TextBox).Text;
                if (cPDFAnnotation.GetFieldName() != text)
                {
                    ComboBoxHistory history = new ComboBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                    cPDFAnnotation.SetFieldName((sender as TextBox).Text);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                ComboBoxHistory history = new ComboBoxHistory();
                history.Action = HistoryAction.Update;
                history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                cPDFAnnotation.SetFlags( ParamConverter.GetFormFlags((ParamConverter.FormField)(sender as ComboBox).SelectedIndex, cPDFAnnotation));
                pdfViewerControl.UpdateAnnotFrame();
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BorderColorPickerControl.Brush).Color.B;

                byte[] oldColor = new byte[3];
                cPDFAnnotation.GetWidgetBorderRGBColor(ref oldColor);
                if (!oldColor.SequenceEqual(Color))
                {
                    CheckBoxHistory history = new CheckBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetWidgetBorderRGBColor(Color);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
                
            }
        }

        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color.B;
                
                byte[] oldColor = new byte[3];
                cPDFAnnotation.GetWidgetBgRGBColor(ref oldColor);
                if (!oldColor.SequenceEqual(Color))
                {
                    ComboBoxHistory history = new ComboBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetWidgetBgRGBColor(Color);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                byte[] Color = new byte[3];
                Color[0] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.R;
                Color[1] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.G;
                Color[2] = ((SolidColorBrush)TextColorPickerControl.Brush).Color.B;
                CTextAttribute cTextAttribute = cPDFAnnotation.GetTextAttribute();

                if (!cTextAttribute.FontColor.SequenceEqual(Color))
                {
                    ComboBoxHistory history = new ComboBoxHistory();
                    history.Action = HistoryAction.Update;
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cTextAttribute.FontColor = Color;
                    cPDFAnnotation.SetTextAttribute(cTextAttribute);
                    cPDFAnnotation.UpdateFormAp();
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
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

                    ComboBoxHistory history = new ComboBoxHistory();
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
            }
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (isFontStyleCmbClicked)
                {
                    ComboBoxHistory history = new ComboBoxHistory();
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
            }
        }

        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                ComboBoxHistory history = new ComboBoxHistory();
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

        private void txtItemInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (itemlists.ContainsKey(txtItemInput.Text.Trim()))
            {
                btnAddItem.IsEnabled = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(txtItemInput.Text))
                    btnAddItem.IsEnabled = true;
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            itemlists.Add(txtItemInput.Text, txtItemInput.Text);
            ListBoxItem item = new ListBoxItem();
            item.Content = txtItemInput.Text;
            item.Tag = txtItemInput.Text;
            itemsListBox.Items.Add(item);
            UpdateListItems();
            txtItemInput.Text = "";
            txtItemInput.Focus();
            btnAddItem.IsEnabled = false;
        }
        private void UpdateListItems()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            foreach (ListBoxItem item in itemsListBox.Items)
            {
                if (item.Content != null && item.Tag != null)
                {
                    pairs.Add(item.Content.ToString(), item.Tag.ToString());
                }

            }

            int optionsCount = cPDFAnnotation.GetItemsCount();
            for (int i = 0; i < optionsCount; i++)
            {
                cPDFAnnotation.RemoveOptionItem(0);
            }

            int addIndex = 0;
            foreach (string key in pairs.Keys)
            {
                cPDFAnnotation.AddOptionItem(addIndex, pairs[key], key);
                addIndex++;
            }

            if (itemsListBox.SelectedIndex > -1)
            {
                cPDFAnnotation.SelectItem(itemsListBox.SelectedIndex);
            }
            pdfViewerControl.UpdateAnnotFrame();
            CheckListCount();
        }
        private void CheckListCount()
        {
            if (itemsListBox.Items.Count > 0)
                TipPanel.Visibility = Visibility.Visible;
            else
                TipPanel.Visibility = Visibility.Collapsed;
        }


        private void itemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                btnDelete.IsEnabled = true;
                if (itemsListBox.SelectedIndex <= 0)
                {
                    btnMoveUp.IsEnabled = false;
                }
                else
                {
                    btnMoveUp.IsEnabled = true;
                }

                if (itemsListBox.SelectedIndex >= itemsListBox.Items.Count - 1)
                {
                    btnMoveDown.IsEnabled = false;
                }
                else
                {
                    btnMoveDown.IsEnabled = true;
                }

                txtItemInput.Text = (itemsListBox.SelectedItem as ListBoxItem).Content.ToString();
                txtItemInput.SelectAll();
            }

            if (itemsListBox.SelectedItems.Count <= 0)
            {
                btnDelete.IsEnabled = false;
                btnMoveDown.IsEnabled = false;
                btnMoveUp.IsEnabled = false;
            }

            if (itemsListBox.SelectedIndex >= 0)
            {
                cPDFAnnotation.SelectItem(itemsListBox.SelectedIndex);
                pdfViewerControl.UpdateAnnotFrame();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (itemsListBox.SelectedItems.Count > 0)
            {
                if ((itemsListBox.SelectedItem as ListBoxItem).Content != null)
                    itemlists.Remove((itemsListBox.SelectedItem as ListBoxItem).Content.ToString());

                itemsListBox.Items.Remove(itemsListBox.SelectedItem as ListBoxItem);

                btnDelete.IsEnabled = false;
                UpdateListItems();
                txtItemInput.Text = "";
            }
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem newitem = new ListBoxItem();
            newitem.Content = (itemsListBox.SelectedItem as ListBoxItem).Content;
            newitem.Tag = (itemsListBox.SelectedItem as ListBoxItem).Tag;

            int index = itemsListBox.SelectedIndex;
            if (index - 1 >= 0)
            {
                itemsListBox.Items.Insert(index - 1, newitem);
                itemsListBox.Items.Remove(itemsListBox.SelectedItem);
                itemsListBox.SelectedIndex = index - 1;
                itemsListBox.Focus();
                UpdateListItems();
            }
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem newitem = new ListBoxItem();
            newitem.Content = (itemsListBox.SelectedItem as ListBoxItem).Content;
            newitem.Tag = (itemsListBox.SelectedItem as ListBoxItem).Tag;

            int index = itemsListBox.SelectedIndex;
            if (index + 1 <= itemsListBox.Items.Count)
            {
                itemsListBox.Items.Remove(itemsListBox.SelectedItem);
                itemsListBox.Items.Insert(index + 1, newitem);
                itemsListBox.SelectedIndex = index + 1;
                itemsListBox.Focus();
                UpdateListItems();
            }
        }

        private void FontCmb_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ComboBox)
            {
                isFontCmbClicked = true;
            }
        }

        private void FontStyleCmb_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ComboBox)
            {
                isFontStyleCmbClicked = true;
            }
        }
    }
}
