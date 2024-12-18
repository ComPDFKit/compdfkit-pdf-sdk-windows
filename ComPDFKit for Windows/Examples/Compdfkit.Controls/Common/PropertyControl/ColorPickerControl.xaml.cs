using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using RadioButton = System.Windows.Controls.RadioButton;
using UserControl = System.Windows.Controls.UserControl;

namespace ComPDFKit.Controls.Common
{
    public partial class ColorPickerControl : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ColorChanged;



        public Visibility TransparentBtnProperty
        {
            get { return (Visibility)GetValue(TransparentBtnPropertyProperty); }
            set { SetValue(TransparentBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransparentBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransparentBtnPropertyProperty =
            DependencyProperty.Register("TransparentBtnProperty", typeof(Visibility), typeof(ColorPickerControl), new PropertyMetadata(Visibility.Visible));
         
        public void SetIsChecked(int index)
        {
            switch (index)
            {
                case -1:
                    TransparentBtn.IsChecked = true;
                    break;
                case 0:
                    FirstBtn.IsChecked = true;
                    break;
                case 1:
                    SecondBtn.IsChecked = true;
                    break;
                case 2:
                    ThirdBtn.IsChecked = true;
                    break;
                case 3:
                    FourthBtn.IsChecked = true;
                    break;
                case 4:
                    CustomColorRadioButton.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        public void SetButtonColor(List<SolidColorBrush> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        FirstBtn.Background = list[i];
                        break;
                    case 1:
                        SecondBtn.Background = list[i];
                        break;
                    case 2:
                        ThirdBtn.Background = list[i];
                        break;
                    case 3:
                        FourthBtn.Background = list[i];
                        break;
                    default:
                        break;
                }
            }
        }

        public List<string> GetButtonColor()
        {
            List<string> brushes = new List<string>();
            if (TransparentBtn.Visibility == Visibility.Visible)
            {
                brushes.Add(TransparentBtn.Background.ToString());
            }
            else
            {
                brushes.Add(FirstBtn.Background.ToString());
            }
            brushes.Add(SecondBtn.Background.ToString());
            brushes.Add(ThirdBtn.Background.ToString());
            brushes.Add(FourthBtn.Background.ToString());
            return brushes;
        }

        public bool GetTransparentBtnVisibility()
        {
            if (TransparentBtnProperty == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void SetCheckedForColor(Color color)
        {
            bool Transparent =GetTransparentBtnVisibility();
            List<string> brushes = GetButtonColor();
            int index = brushes.IndexOf(color.ToString());
            if (index < 0)
            {
                SetIsChecked(4);
                CustomBrush = new SolidColorBrush(color);
            }
            else
            {
                if (Transparent && index == 0)
                {
                    index = -1;
                }
                SetIsChecked(index);
            }
        }

        public static readonly DependencyProperty CanNoneProperty = DependencyProperty.Register("CanNone", typeof(bool), typeof(ColorPickerControl), new PropertyMetadata(false));
        public bool CanNone
        {
            get { return (bool)GetValue(CanNoneProperty); }
            set { SetValue(CanNoneProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(ColorPickerControl), new PropertyMetadata(Brushes.Red));
        public Brush Brush
        {
            get
            {
                return (Brush)GetValue(BrushProperty);
            }
            set
            {
                SetValue(BrushProperty, value);
                OnColorChanged();
            }
        }

        public static readonly DependencyProperty CustomBrushProperty = DependencyProperty.Register("CustomBrush", typeof(Brush), typeof(ColorPickerControl), new PropertyMetadata(Brushes.Red));
        public Brush CustomBrush
        {
            get
            {
                return (Brush)GetValue(CustomBrushProperty);
            }
            set
            {
                SetValue(CustomBrushProperty, value);
            }
        }

        public ColorPickerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Brush GetBrush()
        {
            return Brush;
        }


        private void CustomColorRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                Brush= CustomBrush = brush;
            }
        }

        private void OnColorChanged()
        {
            ColorChanged?.Invoke(this, null);
        }

        private void ColorRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                Brush = radioButton.Background;
            }
        }

    }
}
