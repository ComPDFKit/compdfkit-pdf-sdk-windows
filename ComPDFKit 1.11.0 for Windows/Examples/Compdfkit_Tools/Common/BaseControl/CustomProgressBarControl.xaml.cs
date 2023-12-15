using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Common
{
    public partial class CustomProgressBarControl : UserControl,INotifyPropertyChanged
    {
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(nameof(ProgressValue), typeof(int), typeof(CustomProgressBarControl), new PropertyMetadata(0));
        public int ProgressValue
        {
            get => (int)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }
        
        public static readonly DependencyProperty ProgressMaxValueProperty =
            DependencyProperty.Register(nameof(ProgressMaxValue), typeof(int), typeof(CustomProgressBarControl), new PropertyMetadata(100));
        public int ProgressMaxValue
        {
            get => (int)GetValue(ProgressMaxValueProperty);
            set => SetValue(ProgressMaxValueProperty, value);
        }
        
        public CustomProgressBarControl()
        {
            InitializeComponent();
            Grid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected void UpdateProper<T>(ref T properValue,
            T newValue,
            [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return;

            properValue = newValue;
            OnPropertyChanged(properName);
        }
    }
}