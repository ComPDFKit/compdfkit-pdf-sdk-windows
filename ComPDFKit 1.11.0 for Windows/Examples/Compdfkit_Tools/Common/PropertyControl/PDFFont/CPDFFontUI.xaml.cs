using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.Common
{
    public partial class CPDFFontUI : UserControl, INotifyPropertyChanged
    {
        private string regular = LanguageHelper.PropertyPanelManager.GetString("Font_Regular");
        private string bold = LanguageHelper.PropertyPanelManager.GetString("Font_Bold");
        private string italic = LanguageHelper.PropertyPanelManager.GetString("Font_Oblique");
        private string boldItalic = LanguageHelper.PropertyPanelManager.GetString("Font_BoldOblique");
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler FontFamilyChanged;
        public event EventHandler FontStyleChanged;
        public event EventHandler FontSizeChanged;
        public event EventHandler FontAlignChanged;

        public string FontFamilyValue
        {
            get => FontFamilyComboBox.SelectedItem.ToString();
            set
            {
                if (value == "Courier New")
                {
                    FontFamilyComboBox.SelectedIndex = 0;
                }
                else if (value == "Arial")
                {
                    FontFamilyComboBox.SelectedIndex = 1;

                }
                else
                {
                    FontFamilyComboBox.SelectedIndex = 2;
                }

            }
        }

        private bool _isBold;
        public bool IsBold
        {
            get => _isBold;
            set
            {
                _isBold = value;
                if (_isBold && IsItalic)
                {
                    FontStyleComboBox.SelectedIndex = 3;
                }
                else if (_isBold && !IsItalic)
                {
                    FontStyleComboBox.SelectedIndex = 1;
                }
                else if (!_isBold && IsItalic)
                {
                    FontStyleComboBox.SelectedIndex = 2;
                }
                else
                {
                    FontStyleComboBox.SelectedIndex = 0;
                }
            }
        }

        private bool _isItalic;
        public bool IsItalic
        {
            get => _isItalic;
            set
            {
                _isItalic = value;
                if (IsBold && _isItalic)
                {
                    FontStyleComboBox.SelectedIndex = 3;
                }
                else if (IsBold && !_isItalic)
                {
                    FontStyleComboBox.SelectedIndex = 1;
                }
                else if (!IsBold && _isItalic)
                {
                    FontStyleComboBox.SelectedIndex = 2;
                }
                else
                {
                    FontStyleComboBox.SelectedIndex = 0;
                }
            }
        }

        private TextAlignment _textAlignment;
        public TextAlignment TextAlignment
        {
            get
            {
                if ((bool)LeftAlignRadioButton.IsChecked)
                {
                    return TextAlignment.Left;
                }
                else if ((bool)CenterAlignRadioButton.IsChecked)
                {
                    return TextAlignment.Center;
                }
                else
                {
                    return TextAlignment.Right;
                }
            }

            set
            {
                if (TextAlignment.Left == value)
                {
                    LeftAlignRadioButton.IsChecked = true;
                }
                else if (TextAlignment.Center == value)
                {
                    CenterAlignRadioButton.IsChecked = true;
                }
                else
                {
                    RightAlignRadioButton.IsChecked = true;
                }
            }
        }

        private int _fontSizeValue = 20;
        public int FontSizeValue
        {
            get => _fontSizeValue;
            set
            {
                _fontSizeValue = value;
                OnPropertyChanged(nameof(FontSizeValue));
                FontSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public CPDFFontUI()
        {
            InitializeComponent();
            InitComboBox();
            this.DataContext = this;
        }

        public void InitComboBox()
        {
            List<string> fontNameList = new List<string>()
            {
                {"Courier" },
                {"Helvetica" },
                {"Times" }
            };
            FontFamilyComboBox.ItemsSource = fontNameList;
            FontFamilyComboBox.SelectedIndex = 1;

            List<string> fontStyleList = new List<string>()
            {
                regular,
                bold,
                italic,
                boldItalic
            };
            FontStyleComboBox.ItemsSource = fontStyleList;
            FontStyleComboBox.SelectedIndex = 0;

            List<int> fontSizeList = new List<int>()
            {
                {6},
                {8},
                {9},
                {10},
                {12},
                {14},
                {18},
                {20},
                {24},
                {26},
                {28},
                {32},
                {30},
                {32},
                {48},
                {72}
            };
            FontSizeComboBox.InitPresetNumberArray(fontSizeList);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontFamilyTextBox.Text = (sender as ComboBox).SelectedItem.ToString();
            FontFamilyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FontStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontStyleTextBox.Text = (sender as ComboBox).SelectedItem.ToString();
            if (FontStyleTextBox.Text == regular)
            {
                IsBold = false;
                IsItalic = false;
            }
            else if (FontStyleTextBox.Text == italic)
            {
                IsBold = false;
                IsItalic = true;
            }
            else if (FontStyleTextBox.Text == bold)
            {
                IsBold = true;
                IsItalic = false;
            }
            else
            {
                IsBold = true;
                IsItalic = true;
            }
            FontStyleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AlignRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FontAlignChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}