using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Edit
{
    public partial class CPDFImageFlipUI : UserControl
    {
        public event EventHandler<bool> FlipChanged;
        public CPDFImageFlipUI()
        {
            InitializeComponent();
        }

        #region UI Event
        private void FlipVertical_Click(object sender, RoutedEventArgs e)
        {
            FlipChanged?.Invoke(this, true);
        }

        private void FlipHorizontal_Click(object sender, RoutedEventArgs e)
        {
            FlipChanged?.Invoke(this, false);
        }
        #endregion 
    }
}
