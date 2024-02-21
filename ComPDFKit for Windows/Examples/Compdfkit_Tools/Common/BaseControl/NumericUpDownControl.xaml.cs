using System;
using System.Globalization;
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
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericUpDownControl), new UIPropertyMetadata());

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata(HandleMaxValueChanged));
        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(NumericUpDownControl), new UIPropertyMetadata());
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata(HandleMinValueChanged));
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        
        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty IsValueValidProperty =
            DependencyProperty.Register(nameof(IsValueValid), typeof(bool), typeof(NumericUpDownControl), new UIPropertyMetadata());
        public bool IsValueValid
        {
            get => (bool)GetValue(IsValueValidProperty);
            set => SetValue(IsValueValidProperty, value);
        }
        
        private static void HandleMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownControl source = (NumericUpDownControl) d;
            source.Validator.MinValue = (int) e.NewValue;
        }
        
        private static void HandleMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownControl source = (NumericUpDownControl) d;
            source.Validator.MaxValue = (int) e.NewValue;
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
            if(int.TryParse(TextBox.Text, out int num))
            {
                if (num <= Maximum && num >= Minimum)
                {
                    IsValueValid = true;
                    return;
                }
            }
            IsValueValid = false;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBox.Text, out int num))
            {
                int newNum = num + 1;
                if (newNum > Maximum)
                {
                    TextBox.Text = Maximum.ToString();
                }
                else if(newNum < Minimum)
                {
                    TextBox.Text = Minimum.ToString();
                }
                else
                {
                    TextBox.Text = newNum.ToString();
                }
            }
            else
            {
                TextBox.Text = Minimum.ToString();
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBox.Text, out int num))
            {
                int newNum = num - 1;
                if (newNum > Maximum)
                {
                    TextBox.Text = Maximum.ToString();
                }
                else if (newNum < Minimum)
                {
                    TextBox.Text = Minimum.ToString();
                }
                else
                {
                    TextBox.Text = newNum.ToString();
                }
            }
            else
            {
                TextBox.Text = Minimum.ToString();
            }
        }
    }
    
    public class CustomValidationRule : ValidationRule
    {
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            if (int.TryParse(value.ToString(), out int num))
            {
                if (num <= MaxValue && num >= MinValue)
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, $"Value must be between {MinValue} and {MaxValue}.");
                }
            }
            else
            {
                return new ValidationResult(false, "Value must be an integer.");
            }
        }
    }

}

