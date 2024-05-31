using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Edit
{
    public partial class CPDFImageRotateUI : UserControl
    {
        #region 
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

        #endregion

        #region 

        private void RotateLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            RotationChanged?.Invoke(this, -90);
        }

        private void RotateRightBtn_Click(object sender, RoutedEventArgs e)
        {
            RotationChanged?.Invoke(this, 90);
        }

        #endregion 
    }
}
