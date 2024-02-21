using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compdfkit_Tools.Common
{
    public partial class WritableComboBoxControl : UserControl
    {

        public event EventHandler<string> TextChanged;

        public string SelectedIndex
        {
            get { return (string)GetValue(SelectedIndexProperty); }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(string), typeof(WritableComboBoxControl), new PropertyMetadata("0"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WritableComboBoxControl), new PropertyMetadata(""));

        public int MaxPageIndex
        {
            get { return (int)GetValue(MaxPageIndexProperty); }
            set { 
                SetValue(MaxPageIndexProperty, value);
                UpDataPagesInRange();
            }
        }
        // Using a DependencyProperty as the backing store for MaxPageRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxPageIndexProperty =
            DependencyProperty.Register("MaxPageIndex", typeof(int), typeof(WritableComboBoxControl), new FrameworkPropertyMetadata(0));

        private void UpDataPagesInRange()
        {
            if (ComboBox.SelectedItem == null)
            {
                return;
            }
            if (ComboBox.SelectedItem as ComboBoxItem == null)
            {
                return;
            }
            if ((ComboBox.SelectedItem as ComboBoxItem).Tag != null)
            {
                switch ((ComboBox.SelectedItem as ComboBoxItem).Tag.ToString())
                {
                    case "AllPages":
                        Text = "1-" + MaxPageIndex;
                        TextChanged?.Invoke( null, Text);
                        break;
                    case "OddPages":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageIndex; i++)
                            {
                                if (i % 2 != 0 || MaxPageIndex == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }

                            }
                            Text = pageRange;
                            TextChanged?.Invoke(null, Text);

                            break;
                        }
                    case "EvenPages":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageIndex; i++)
                            {
                                if (i % 2 == 0 || MaxPageIndex == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }

                            }
                            Text = pageRange;
                            TextChanged?.Invoke(null, Text);

                            break;
                        }
                    case "CustomPages":
                        Text = TextBox.Text;
                        TextChanged?.Invoke(null, Text);
                        break;
                    default:
                        break;
                }

            }
        }

        public WritableComboBoxControl()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == comboBox.Items.Count - 1)
            {
                TextBox.Visibility = Visibility.Visible;
            }
            else
            {
                TextBox.Visibility = Visibility.Hidden;
            }
            UpDataPagesInRange();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ComboBox.Focus();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9,-]+").IsMatch(e.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComboBox.SelectedIndex == ComboBox.Items.Count - 1)
            {
                Text = TextBox.Text;
            }
            else { Text = ""; }

            TextChanged?.Invoke(null, Text);
        }
    }
}
