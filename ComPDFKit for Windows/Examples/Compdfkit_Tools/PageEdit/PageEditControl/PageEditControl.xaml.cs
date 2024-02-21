using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Compdfkit_Tools.PDFControl
{
    public partial class PageEditControl : UserControl
    {
        #region Properties
        private PDFViewControl _pdfViewControl;
        public PDFViewControl PDFViewControl
        {
            get => _pdfViewControl;
            set
            {
                _pdfViewControl = value;
                pageEditControl.pdfViewer = _pdfViewControl.PDFView;
                pageEditControl.LoadThumbnails(pageEditControl.pdfViewer);
                pageEditControl.isThumbInitialized = false;
            }
        }

        //When the order or total number of pages changes
        public event RoutedEventHandler PageMoved;
        public event EventHandler ExitPageEdit;

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;

        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        #endregion
        public PageEditControl()
        {
            InitializeComponent();
            this.DataContext = this;
            pageEditControl.PageMoved -= PageEditControl_PageMoved;
            pageEditControl.PageMoved += PageEditControl_PageMoved;

            CPDFPageEditBarControl.PageEditEvent -= CPDFPageEditBarControl_PageEditEvent;
            CPDFPageEditBarControl.PageEditEvent += CPDFPageEditBarControl_PageEditEvent;

            pageEditControl.ExitPageEdit -= PageEditControl_ExitPageEdit;
            pageEditControl.ExitPageEdit += PageEditControl_ExitPageEdit;
        }

        #region Private Command Methods
        private void PageEditControl_ExitPageEdit(object sender, EventArgs e)
        {
            this.ExitPageEdit?.Invoke(this, EventArgs.Empty);
        }

        private void CPDFPageEditBarControl_PageEditEvent(object sender, string e)
        {
            pageEditControl.PageEdit(e);
        }

        private void PageEditControl_PageMoved(object sender, RoutedEventArgs e)
        {
           PageMoved?.Invoke(this, e);
        }
        #endregion
    }
}
