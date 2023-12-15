using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compdfkit_Tools.Common
{
    public partial class NumericUpDownControl : UserControl
    {
        private string regixString = "[^0-9-]";
        private string nRegixString = @"[^0-9-]+";

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(NumericUpDownControl), new UIPropertyMetadata());

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty MaxiumProperty =
           DependencyProperty.Register("Maxium", typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata());
        public int Maxium
        {
            get { return (int)GetValue(MaxiumProperty); }
            set { SetValue(MaxiumProperty, value); }
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NumericUpDownControl), new UIPropertyMetadata());
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
    DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata());
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public NumericUpDownControl()
        {
            InitializeComponent();
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Minimum > 0)
            {
                e.Handled = new Regex(regixString).IsMatch(e.Text);
            }
            else
            {
                e.Handled = new Regex(nRegixString).IsMatch(e.Text);

            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyStates == Keyboard.GetKeyStates(Key.LeftCtrl) || e.KeyStates == Keyboard.GetKeyStates(Key.RightCtrl)) && e.KeyStates == Keyboard.GetKeyStates(Key.V))
                e.Handled = true;
            else
                e.Handled = false;
        }

        public static int GetMinAbsoluteValue(int min, int max)
        {
            if (min <= 0 && max >= 0)
            {
                return 0;
            }
            else
            {
                return Math.Abs(min) < Math.Abs(max) ? min : max;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox.Text))
            {
                if (int.Parse(TextBox.Text) > Maxium)
                {
                    TextBox.Text = Maxium.ToString();
                }

                if (int.Parse(TextBox.Text) < Minimum)
                {
                    TextBox.Text = Minimum.ToString();
                }
            }
            else
            {
                TextBox.Text = GetMinAbsoluteValue(Minimum, Maxium).ToString();
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBox.Text, out int num))
            {
                TextBox.Text = (++num).ToString();
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBox.Text, out int num))
            {
                TextBox.Text = (--num).ToString();
            }
        }
    }
}

