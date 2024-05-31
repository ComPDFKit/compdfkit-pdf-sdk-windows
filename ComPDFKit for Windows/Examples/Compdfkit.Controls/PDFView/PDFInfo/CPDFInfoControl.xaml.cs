using System;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFInfoControl : UserControl
    {
        public PDFViewControl ViewControl;

        public event EventHandler CloseInfoEvent;

        public CPDFInfoControl()
        {
            InitializeComponent();

        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            CPDFAbstractInfoControl.InitWithPDFViewer(viewControl);
            CPDFCreateInfoControl.InitWithPDFViewer(viewControl);
            CPDFSecurityInfoControl.InitWithPDFViewer(viewControl);
        }
        private void CloseInfoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseInfoEvent?.Invoke(this, new EventArgs());
        }
    }
}
