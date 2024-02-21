using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compdfkit_Tools.Common
{
    public partial class DropDownNumberBoxControl : UserControl
    {
        private string regixString = "[^0-9]+";

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(DropDownNumberBoxControl), new UIPropertyMetadata());

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DropDownNumberBoxControl), new UIPropertyMetadata());

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty MaxiumProperty =
            DependencyProperty.Register("Maxium", typeof(int), typeof(DropDownNumberBoxControl), new UIPropertyMetadata());

        public int Maxium
        {
            get { return (int)GetValue(MaxiumProperty); }
            set { SetValue(MaxiumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
    DependencyProperty.Register("Minimum", typeof(int), typeof(DropDownNumberBoxControl), new UIPropertyMetadata());

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public event EventHandler<string> InputEnterEvent;
        public event EventHandler<string> SetPresetEvent;

        public DropDownNumberBoxControl()
        {
            InitializeComponent();
        }


        public void InitPresetNumberArray(List<int> presetNumber)
        {
            List<string> array = new List<string>();
            //array.Add("Actual size");
            //array.Add("Suitable width");
            //array.Add("Single page size");
            for (int i = 0; i < presetNumber.Count; i++)
            {
                array.Add(presetNumber[i].ToString());
            }
            ComboBox.ItemsSource = array;
            ComboBox.SelectedIndex = -1;
        }

        public void SelectValueItem(int valueData)
        {
            try
            {
                foreach (string checkItem in ComboBox.Items)
                {
                    if(int.TryParse(checkItem, out int itemValue) && valueData== itemValue)
                    {
                        ComboBox.SelectedItem = checkItem;
                        return;
                    }
                }
                ComboBox.SelectedIndex= -1;
            }
            catch(Exception ex)
            {

            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex(regixString).IsMatch(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputEnterEvent?.Invoke(sender, Text);
            }
            if ((e.KeyStates == Keyboard.GetKeyStates(Key.LeftCtrl) || e.KeyStates == Keyboard.GetKeyStates(Key.RightCtrl)) && e.KeyStates == Keyboard.GetKeyStates(Key.V))
                e.Handled = true;
            else
                e.Handled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox.Text))
            {
                int num;
                int.TryParse(TextBox.Text,out num);
                if (num > Maxium)
                {
                    TextBox.Text = Maxium.ToString();
                }

                if (num < Minimum)
                {
                    TextBox.Text = Minimum.ToString();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null)
            {
                TextBox.Text = (sender as ComboBox).SelectedItem.ToString();
                SetPresetEvent?.Invoke(sender, TextBox.Text);
            }
        }
    }
}

