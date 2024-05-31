using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComPDFKit.Controls.Common
{
    /// <summary>
    /// Interaction logic for CPDFRotationControl.xaml
    /// </summary>
    public partial class CPDFRotationControl : UserControl, INotifyPropertyChanged
    {

        private string _rotationText = "0";
        public string RotationText
        {
            get => _rotationText;
            set
            {
                if (string.IsNullOrEmpty(value))
                { 
                    return;
                }

                int result = 0;

                if (int.TryParse(value, out result))
                {
                    if (result > NumericUpDownControl.Maximum)
                    {
                        value = NumericUpDownControl.Maximum.ToString();
                    }
                    if (result < NumericUpDownControl.Minimum)
                    {
                        value = NumericUpDownControl.Minimum.ToString();
                    }
                    if (UpdateProper(ref _rotationText, value))
                    {
                        RotationChanged?.Invoke(this, EventArgs.Empty);
                    }
                } 
            }
        }

        public event EventHandler RotationChanged;

        public CPDFRotationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void RotationBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                if (btn.Name == "CounterclockwiseBtn")
                {
                    RotationText = (int.Parse(RotationText) - 45).ToString();
                }
                else if (btn.Name == "ResetBtn")
                {
                    RotationText = "0";
                }
                else if (btn.Name == "ClockwiseBtn")
                {
                    RotationText = (int.Parse(RotationText) + 45).ToString();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool UpdateProper<T>(ref T properValue,
    T newValue,
    [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }
    }
}
