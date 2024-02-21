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
    /// Interaction logic for MetrixRadioControl.xaml
    /// </summary>
    public partial class MatrixRadioControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedTagProperty =
    DependencyProperty.Register("SelectedTag", typeof(int), typeof(MatrixRadioControl), new PropertyMetadata(4));
        public int SelectedTag
        {
            get { return (int)GetValue(SelectedTagProperty); }
            set
            {
                SetValue(SelectedTagProperty, value);
            }
        }

        private int _selectedTagValue = 4;
        public int SelectedTagValue
        {
            get => _selectedTagValue;
            set
            {
                if (UpdateProper(ref _selectedTagValue, value))
                {
                    SelectedTag = value;
                }
            }
        }

        public MatrixRadioControl()
        {
            InitializeComponent(); 
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rdo = sender as RadioButton;
            SelectedTagValue = int.Parse((string)rdo.Tag);
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
