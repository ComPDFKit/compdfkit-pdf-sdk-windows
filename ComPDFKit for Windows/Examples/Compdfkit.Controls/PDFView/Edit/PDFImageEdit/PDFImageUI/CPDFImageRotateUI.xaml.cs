using System;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Edit
{
    public partial class CPDFImageRotateUI : UserControl
    {
        public event EventHandler<double> RotationChanged;
        public CPDFImageRotateUI()
        {
            InitializeComponent();
        }

        public Orientation Orientation
        {
            get
            {
                return ImageRotateUI.Orientation;
            }
            set
            {
                ImageRotateUI.Orientation = value;
            }
        }

        private void RotateLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            RotationChanged?.Invoke(this, -90);
        }

        private void RotateRightBtn_Click(object sender, RoutedEventArgs e)
        {
            RotationChanged?.Invoke(this, 90);
        }
    }
}
