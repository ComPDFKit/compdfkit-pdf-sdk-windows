using ComPDFKit.Tool;
using ComPDFKit.Controls.PDFControl;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFContentEditControl : UserControl
    {
        #region  

        private PDFImageEditControl PDFImageEditControl = new PDFImageEditControl();
        private PDFTextEditControl PDFTextEditControl = new PDFTextEditControl();

        public PDFViewControl ViewControl { get; private set; }
        public PDFEditParam EditEvent { get; set; }
        
        #endregion
        public PDFContentEditControl()
        {
            InitializeComponent();
        }

        public void InitWithPDFViewer(PDFViewControl newPDFView)
        {
            ViewControl = newPDFView;
            PDFImageEditControl.InitWithPDFViewer(newPDFView.PDFViewTool);
            PDFTextEditControl.InitWithPDFViewer(newPDFView.PDFViewTool);
        }
         
        public void SetRotationText(float rotation)
        {
            PDFImageEditControl.SetRotationText(rotation);
        }

        public void SetPDFTextEditData(TextEditParam editEvent, bool isTemp = false)
        {
            if (!isTemp)
            {
                PDFTextEditControl.SetPDFTextEditData(editEvent);
                ContentEditContainer.Child = PDFTextEditControl;
            }
            else
            {
                PDFTextEditControl tempPDFTextEditControl = new PDFTextEditControl();
                tempPDFTextEditControl.InitWithPDFViewer(ViewControl.PDFViewTool);
                tempPDFTextEditControl.SetPDFTextEditData(editEvent);
                ContentEditContainer.Child = tempPDFTextEditControl;
            }
        }

        //public void SetPDFTextMultiEditData(List<PDFEditEvent> editEventList)
        //{
        //    PDFTextEditControl.SetPDFTextMultiEditData(editEventList);
        //    ContentEditContainer.Child = PDFTextEditControl;
        //}

        public void ClearContentControl()
        {
            ContentEditContainer.Child = null;
        }

        public void SetPDFImageEditData(ImageEditParam editEvent)
        {
            PDFImageEditControl.SetPDFImageEditData(editEvent);
            ContentEditContainer.Child = PDFImageEditControl;
        }

        //public void SetPDFImageMultiEditData(List<PDFEditEvent> editEventList)
        //{
        //    PDFImageEditControl.SetPDFImageMultiEditData(editEventList);
        //    ContentEditContainer.Child = PDFImageEditControl;
        //}

        public void RefreshThumb()
        {
            PDFImageEditControl?.SetImageThumb();
        }
    }
}
