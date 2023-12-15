using ComPDFKitViewer.PdfViewer;
using ComPDFKitViewer;
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

namespace Compdfkit_Tools.Edit
{
    /// <summary>
    /// Interaction logic for PDFContentEditControl.xaml
    /// </summary>
    public partial class PDFContentEditControl : UserControl
    {
        private UIElement contentEditPanel = null;
        private UIElement tempContentEditPanel = null;

        private PDFImageEditControl PDFImageEditControl = new PDFImageEditControl();
        private PDFTextEditControl PDFTextEditControl = new PDFTextEditControl();

        public CPDFViewer PDFView { get; private set; }
        public PDFEditEvent EditEvent { get; set; }

        public PDFContentEditControl()
        {
            InitializeComponent();
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            PDFView = newPDFView;
            PDFImageEditControl.InitWithPDFViewer(newPDFView);
        }

        public void SetPDFTextEditData(PDFEditEvent editEvent, bool isTemp = false)
        {
            if (!isTemp)
            {
                PDFTextEditControl.SetPDFTextEditData(editEvent);
                ContentEditContainer.Child = PDFTextEditControl;
            }
            else
            {
                PDFTextEditControl tempPDFTextEditControl = new PDFTextEditControl();
                tempPDFTextEditControl.SetPDFTextEditData(editEvent);
                ContentEditContainer.Child = tempPDFTextEditControl;
            }
        }

        public void SetPDFTextMultiEditData(List<PDFEditEvent> editEventList)
        {
            PDFTextEditControl.SetPDFTextMultiEditData(editEventList);
            ContentEditContainer.Child = PDFTextEditControl;
        }

        public void ClearContentControl()
        {
            ContentEditContainer.Child = null;
        }

        public void SetPDFImageEditData(PDFEditEvent editEvent)
        {
            PDFImageEditControl.SetPDFImageEditData(editEvent);
            ContentEditContainer.Child = PDFImageEditControl;
        }

        public void SetPDFImageMultiEditData(List<PDFEditEvent> editEventList)
        {
            PDFImageEditControl.SetPDFImageMultiEditData(editEventList);
            ContentEditContainer.Child = PDFImageEditControl;
        }

        public void RefreshThumb()
        {
            PDFImageEditControl?.SetImageThumb();
        }
    }
}
