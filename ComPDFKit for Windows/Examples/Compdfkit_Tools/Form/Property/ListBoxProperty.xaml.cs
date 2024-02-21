using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{
    public partial class ListBoxProperty : UserControl
    {
        private WidgetListBoxArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null;
        private Dictionary<string, string> itemlists = null;

        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        bool IsLoadedData = false;

        public ListBoxProperty()
        {
            InitializeComponent();
        }


        #region Loaded

        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e)
        {
            widgetArgs = (WidgetListBoxArgs)Args;
            annotAttribEvent = e;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeCmb.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);

            FieldNameText.Text = widgetArgs.FieldName;
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            BorderColorPickerControl.SetCheckedForColor(widgetArgs.LineColor);
            BackgroundColorPickerControl.SetCheckedForColor(widgetArgs.BgColor);
            TextColorPickerControl.SetCheckedForColor(widgetArgs.FontColor);
            SetFontName(widgetArgs.FontName);
            SetFontStyle(widgetArgs.IsItalic, widgetArgs.IsBold);
            SetFontSize(widgetArgs.FontSize);

            itemlists = widgetArgs.ListOptions;

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
            int index = -1;
            List<string> fontFamilyList = new List<string>() { "Helvetica", "Courier", "Times" };
            for (int i = 0; i < fontFamilyList.Count; i++)
            {
                if (fontFamilyList[i].ToLower().Contains(fontName.ToLower())
                    || fontName.ToLower().Contains(fontFamilyList[i].ToLower()))
                {
                    index = i;
                }
            }
            FontCmb.SelectedIndex = index;
        }

        #endregion

        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FieldName, (sender as TextBox).Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FormField, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)BorderColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FillColor, ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontColor, ((SolidColorBrush)TextColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontName, ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString());
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                bool IsItalic = false;
                bool IsBold = false;
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        IsBold = true;
                        break;
                    case 2:
                        IsItalic = true;
                        break;
                    case 3:
                        IsItalic = true;
                        IsBold = true;
                        break;
                    default:
                        break;
                }
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsBold, IsBold);
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsItalic, IsItalic);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontSize, (sender as ComboBox).SelectedItem);
                annotAttribEvent.UpdateAnnot();
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
            annotAttribEvent?.UpdateAttrib(AnnotAttrib.ListOptions, pairs);
            if (itemsListBox.SelectedIndex > -1)
                annotAttribEvent?.UpdateAttrib(AnnotAttrib.DefaultChoice, new List<int>() { itemsListBox.SelectedIndex });

            annotAttribEvent?.UpdateAnnot();
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
                annotAttribEvent?.UpdateAttrib(AnnotAttrib.DefaultChoice, new List<int>() { itemsListBox.SelectedIndex });
                annotAttribEvent?.UpdateAnnot();
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
    }
}
