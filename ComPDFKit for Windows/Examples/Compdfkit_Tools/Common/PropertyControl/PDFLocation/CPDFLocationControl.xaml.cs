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
    /// Interaction logic for CPDFLocationControl.xaml
    /// </summary>
    public partial class CPDFLocationControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedTagIndexProperty =
    DependencyProperty.Register("SelectedTagIndex", typeof(int), typeof(CPDFLocationControl), new PropertyMetadata(4));
        public int SelectedTagIndex
        {
            get { return (int)GetValue(SelectedTagIndexProperty) ; }
            set
            {
                SetValue(SelectedTagIndexProperty, value);
            }
        }

        private int _selectedTag;
        public int SelectedTag
        {
            get => _selectedTag;
            set
            {
                if(UpdateProper(ref _selectedTag, value))
                {
                    SelectedTagIndex = value;
                }
            }
        }

        private string _horizOffsetValue = "0";
        public string HorizOffsetValue
        {
            get => _horizOffsetValue;
            set
            {
                if (string.IsNullOrEmpty(value) )
                {
                    return;
                }
                if (int.Parse(value) > XNumericControl.Maximum)
                {
                    value = XNumericControl.Maximum.ToString();
                }
                if (int.Parse(value) < XNumericControl.Minimum)
                {
                    value = XNumericControl.Minimum.ToString();
                }
                if (UpdateProper(ref _horizOffsetValue, value))
                {
                    HorizOffsetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private string _vertOffsetValue = "0";
        public string VertOffsetValue
        {
            get => _vertOffsetValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (int.Parse(value) > YNumericControl.Maximum)
                {
                    value = YNumericControl.Maximum.ToString();
                }
                if (int.Parse(value) < YNumericControl.Minimum)
                {
                    value = YNumericControl.Minimum.ToString();
                }
                if ( !string.IsNullOrEmpty(value) && UpdateProper(ref _vertOffsetValue, value))
                {
                    VertOffsetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler HorizOffsetChanged;
        public event EventHandler VertOffsetChanged;


        public CPDFLocationControl()
        {
            InitializeComponent();
            DataContext = this;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
