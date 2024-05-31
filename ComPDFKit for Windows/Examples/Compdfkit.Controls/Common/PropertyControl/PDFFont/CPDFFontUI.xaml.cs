using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFFontUI : UserControl, INotifyPropertyChanged
    {
        internal bool isFirstLoad = true;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler FontFamilyChanged;
        public event EventHandler FontStyleChanged;
        public event EventHandler FontSizeChanged;
        public event EventHandler FontAlignChanged;

        private string _familyName = string.Empty;
        public string FamilyName
        {
            get => _familyName;
            set
            {
                if (UpdateProper(ref _familyName, value))
                {
                    if (FontFamilyComboBox.Items.Contains(_familyName))
                    {
                        FontFamilyComboBox.SelectedItem = _familyName;
                    }
                }
            }
        }

        private string _styleName = string.Empty;
        public string StyleName
        {
            get => _styleName;
            set
            {
                if (UpdateProper(ref _styleName, value))
                {
                    if (FontStyleComboBox.Items.Contains(_styleName))
                    {
                        FontStyleComboBox.SelectedItem = _styleName;
                    }
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
                OnPropertyChanged();
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
            FontFamilyComboBox.ItemsSource = CPDFFont.GetFontNameDictionary().Keys;
            FontFamilyComboBox.SelectedIndex = 0;

            FontStyleComboBox.ItemsSource = CPDFFont.GetFontNameDictionary()[FontFamilyComboBox.SelectedValue.ToString()];
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

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFirstLoad)
            {
                var fontNames = CPDFFont.GetFontNameDictionary().Keys.ToList();
                var defaultIndex = fontNames.FindIndex(x => x == "Helvetica");
                if (defaultIndex == -1)
                {
                    defaultIndex = fontNames.FindIndex(x => x == "Arial");
                }
                FontFamilyComboBox.SelectedIndex = defaultIndex;
                FontStyleComboBox.SelectedIndex = 0;
                FamilyName = FontFamilyComboBox.SelectedValue.ToString();
                isFirstLoad = false;
                return;
            }

            var styleNames = CPDFFont.GetFontNameDictionary()[FontFamilyComboBox.SelectedValue.ToString()];
            FontStyleComboBox.ItemsSource = styleNames; 

            FamilyName = FontFamilyComboBox.SelectedValue.ToString();
            if (styleNames.Count != 0)
            {
                FontStyleComboBox.SelectedIndex = 0;
            }

            FontFamilyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FontStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StyleName = FontStyleComboBox.SelectedValue?.ToString();
            FontStyleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AlignRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FontAlignChanged?.Invoke(this, EventArgs.Empty);
        }

        protected bool UpdateProper<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
            {
                return false;
            }

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}