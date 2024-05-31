using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFArrowUI : UserControl, INotifyPropertyChanged
    {
        public event EventHandler ArrowChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private int _selectedIndex = 1;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
                OnArrowChanged();
            }
        }

        public CPDFArrowUI()
        {
            InitializeComponent();
            this.DataContext = this;  
        }

        private void OnArrowChanged()
        {
            ArrowChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RotateContent(double angle)
        {
            try
            {
                foreach (ComboBoxItem boxItem in ArrowBox.Items)
                {
                    Path drawPath = boxItem.Content as Path;
                    if (drawPath != null)
                    {
                        RotateTransform rotateTransform = new RotateTransform(angle);
                        rotateTransform.CenterX = boxItem.ActualWidth / 2;
                        rotateTransform.CenterY = boxItem.ActualHeight / 2;
                        drawPath.LayoutTransform = rotateTransform;
                    }
                }
            }
            catch(Exception ex)
            {

            }
           
        }
    }
}
