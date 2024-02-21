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

namespace Compdfkit_Tools.Common
{
    /// <summary>
    /// Interaction logic for CPDFTileControl.xaml
    /// </summary>
    public partial class CPDFTileControl : UserControl, INotifyPropertyChanged
    {
        private bool _fullScreen = false;
        public bool IsFullScreen
        {
            get => _fullScreen;
            set
            {
                if (UpdateProper(ref _fullScreen, value))
                {
                    FullScreenChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private string _verticalSpacingValue = "0";
        public string VerticalSpacingValue
        {
            get => _verticalSpacingValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (int.Parse(value) > VerticalNumericControl.Maximum)
                {
                    value = VerticalNumericControl.Maximum.ToString();
                }
                if (int.Parse(value) < VerticalNumericControl.Minimum)
                {
                    value = VerticalNumericControl.Minimum.ToString();
                }
                if (!string.IsNullOrEmpty(value) && UpdateProper(ref _verticalSpacingValue, value))
                {
                    VerticalSpacingChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private string _horizontalSpacingValue = "0";
        public string HorizontalSpacingValue
        {
            get => _horizontalSpacingValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (int.Parse(value) > HorizontalNumericControl.Maximum)
                {
                    value = VerticalNumericControl.Maximum.ToString();
                }
                if (int.Parse(value) < HorizontalNumericControl.Minimum)
                {
                    value = HorizontalNumericControl.Minimum.ToString();
                }
                if (!string.IsNullOrEmpty(value) && UpdateProper(ref _horizontalSpacingValue, value))
                {
                    HorizontalSpacingChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
         
        public event EventHandler FullScreenChanged;
        public event EventHandler VerticalSpacingChanged;
        public event EventHandler HorizontalSpacingChanged;

        public CPDFTileControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TileChk_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk != null)
            {
                if ((bool)chk.IsChecked)
                {
                    IsFullScreen = true;
                }
                else
                {
                    IsFullScreen = false;
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
