using ComPDFKitViewer.PdfViewer;
using System;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFInfoControl : UserControl
    {
        public CPDFViewer pdfViewer;

        public event EventHandler CloseInfoEvent;

        public CPDFInfoControl()
        {
            InitializeComponent();

        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
            CPDFAbstractInfoControl.InitWithPDFViewer(pdfViewer);
            CPDFCreateInfoControl.InitWithPDFViewer(pdfViewer);
            CPDFSecurityInfoControl.InitWithPDFViewer(pdfViewer);
        }
        private void CloseInfoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseInfoEvent?.Invoke(this, new EventArgs());
        }
    }
}
