using ComPDFKit.Tool;
using ComPDFKit.Controls.PDFControl;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFContentEditControl : UserControl
    {
        #region  
        private PDFImageEditControl PDFImageEditControl = new PDFImageEditControl();
        private PDFTextEditControl PDFTextEditControl = new PDFTextEditControl();
        private PDFPathEditControl PDFPathEditControl = new PDFPathEditControl();

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
            PDFTextEditControl.InitWithPDFViewer(newPDFView.PDFViewTool);
            PDFImageEditControl.InitWithPDFViewer(newPDFView.PDFViewTool);
            PDFPathEditControl.InitWithPDFViewer(newPDFView.PDFViewTool);
        }

        public void SetRotationText(float rotation)
        {
            PDFImageEditControl.SetRotationText(rotation);
        }

        public void SetPDFTextEditData(List<TextEditParam> editEvent, bool isTemp = false)
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

        public void SetPDFImageEditData(List<ImageEditParam> editEvent)
        {
            PDFImageEditControl.SetPDFImageEditData(editEvent);
            ContentEditContainer.Child = PDFImageEditControl;
        }

        public void SetPDFPathEditData(List<PathEditParam> editEvent)
        {
            PDFPathEditControl.SetPDFPathEditData(editEvent);
            ContentEditContainer.Child = PDFPathEditControl;
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
