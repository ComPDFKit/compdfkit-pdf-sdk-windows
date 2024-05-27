using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
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
                pageEditControl.viewControl = _pdfViewControl;
                pageEditControl.LoadThumbnails(_pdfViewControl);
                pageEditControl.isThumbInitialized = false;
            }
        }

        //When the order or total number of pages changes
        public event RoutedEventHandler PageMoved;
        public event EventHandler ExitPageEdit;

        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler OnAnnotEditHandler;
      
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
