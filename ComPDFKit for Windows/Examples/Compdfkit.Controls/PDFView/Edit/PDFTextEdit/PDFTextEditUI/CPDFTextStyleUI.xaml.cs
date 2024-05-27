using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Edit
{
    public partial class CPDFTextStyleUI : UserControl
    {
        public event EventHandler<string> TextFontChanged;
        public event EventHandler<bool> TextBoldChanged;
        public event EventHandler<bool> TextItalicChanged;
        public event EventHandler<double> TextSizeChanged;
        public CPDFTextStyleUI()
        {
            InitializeComponent();
        }

        public void SetFontNames(List<string> fontNames)
        {
            FontNameComboBox.ItemsSource = null;
            if (fontNames != null && fontNames.Count > 0)
            {
                List<ComboBoxItem> fontNameList = new List<ComboBoxItem>();
                foreach (string fontName in fontNames)
                {
                    fontNameList.Add(new ComboBoxItem()
                    {
                        Content = fontName,
                        VerticalContentAlignment=VerticalAlignment.Center,
                        HorizontalContentAlignment=HorizontalAlignment.Left
                    });
                }
                FontNameComboBox.ItemsSource = fontNameList;
            }
        }

        public void SelectFontName(string fontName)
        {
            if(string.IsNullOrEmpty(fontName))
            {
                return;
            }

            List<ComboBoxItem> fontNameList = FontNameComboBox.ItemsSource as List<ComboBoxItem>;
            if (fontNameList != null && fontNameList.Count>0)
            {
                int selectIndex = -1;
                for(int i=0;i<fontNameList.Count; i++)
                {
                    ComboBoxItem checkItem= fontNameList[i];
                    if(checkItem.Content!=null && checkItem.Content.ToString().ToLower()==fontName.ToLower() )
                    {
                        selectIndex=i; 
                        break;
                    }
                }

                FontNameComboBox.SelectedIndex = selectIndex;
            }
        }

        public void SetFontStyle(bool isBold,bool isItalic)
        {
            if (isBold == false && isItalic == false)
            {
                FontStyleBox.SelectedIndex = 0;
                return;
            }

            if (isBold && isItalic == false)
            {
                FontStyleBox.SelectedIndex = 1;
                return;
            }

            if (isBold == false && isItalic)
            {
                FontStyleBox.SelectedIndex = 0;
                return;
            }

            if (isBold  && isItalic )
            {
                FontStyleBox.SelectedIndex = 3;
             
            }
        }

        public void SetFontSize(double newFontSize)
        {
            if (FontSizeTextBox != null)
            {
                if (newFontSize - (int)(newFontSize) > 0)
                {
                    FontSizeTextBox.Text = ((int)newFontSize).ToString();
                }
                else
                {
                    FontSizeTextBox.Text = ((int)(newFontSize)).ToString();
                }
            }
        }

        private void FontNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FontNameComboBox.SelectedIndex==-1)
            {
                TextFontChanged?.Invoke(this,string.Empty);
                return;
            }

            ComboBoxItem selectItem= FontNameComboBox.SelectedItem as ComboBoxItem;
            if(selectItem != null && selectItem.Content != null)
            {
                TextFontChanged?.Invoke(this, selectItem.Content.ToString());
            }
        }

        private void FontStyleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = Math.Max(0, FontStyleBox.SelectedIndex);
            switch(selectIndex)
            {
                case 0:
                    TextBoldChanged?.Invoke(this,false);
                    TextItalicChanged?.Invoke(this, false);
                    break;
                case 1:
                    TextBoldChanged?.Invoke(this, true);
                    TextItalicChanged?.Invoke(this, false);
                    break;
                case 2:
                    TextBoldChanged?.Invoke(this, false);
                    TextItalicChanged?.Invoke(this, true);
                    break;
                case 3:
                    TextBoldChanged?.Invoke(this, true);
                    TextItalicChanged?.Invoke(this, true);
                    break;
                default:
                    break;
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = FontSizeComboBox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                if (int.TryParse(selectItem.Content.ToString(), out int newFontSize))
                {
                    FontSizeTextBox.Text = (newFontSize).ToString();
                    TextSizeChanged?.Invoke(this, newFontSize);
                }
            }
        }
    }
}
