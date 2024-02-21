using ComPDFKit.PDFDocument;
using Compdfkit_Tools.Helper;
using ComPDFKitViewer.PdfViewer;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFAbstractInfoControl : UserControl
    {
        public CPDFViewer pdfViewer;
        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
            InitializeAbstractInfo(pdfViewer.Document);
        }

        public CPDFAbstractInfoControl()
        {
            InitializeComponent();
        }

        private void InitializeAbstractInfo(CPDFDocument cpdfDocument)
        {
            FileNameTextBlock.Text = cpdfDocument.FileName;
            FileSizeTextBlock.Text = CommonHelper.GetFileSize(cpdfDocument.FilePath);
            TitleTextBlock.Text = cpdfDocument.GetInfo().Title;
            AuthorTextBlock.Text = cpdfDocument.GetInfo().Author;
            SubjectTextBlock.Text = cpdfDocument.GetInfo().Subject;
            KeywordTextBlock.Text = cpdfDocument.GetInfo().Keywords;
        }
    }
}
