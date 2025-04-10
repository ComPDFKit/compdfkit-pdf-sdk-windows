using System.Windows;
using System.Windows.Controls;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Comparison
{
    public partial class CompareProgressControl : UserControl
    {
        public event RoutedEventHandler CloseClick;
        public CompareProgressControl()
        {
            InitializeComponent();
            ProgressTxb.Text = LanguageHelper.CompareManager.GetString("Tip_Progress");
        }
        
        public void SetValue(int value)
        {
            CompareProgressBar.Value = value;
        }
        
        public void SetValue(double value)
        {
            CompareProgressBar.Value = (int)(value * 100);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CloseClick?.Invoke(this, null);
        }
    }
}