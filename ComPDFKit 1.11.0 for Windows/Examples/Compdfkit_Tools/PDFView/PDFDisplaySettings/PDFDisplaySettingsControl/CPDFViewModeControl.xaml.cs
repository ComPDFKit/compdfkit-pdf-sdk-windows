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
    public partial class CPDFViewModeControl : UserControl
    {
        public CPDFViewer pdfViewer;

        public string CurrentContinuousMode = "Continuous";
        public string CurrentViewMode = "Single";

        public Dictionary<Tuple<string, string>, ComPDFKitViewer.ViewMode> GetViewMode = new Dictionary<Tuple<string, string>, ComPDFKitViewer.ViewMode>();
        
        public CPDFViewModeControl()
        {
            InitializeComponent();
            CPDFViewModeUI.Loaded += CPDFViewModeUI_Loaded;
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
        }

        private void CPDFViewModeUI_Loaded(object sender, RoutedEventArgs e)
        {
            GetViewMode.Clear();
            GetViewMode.Add(Tuple.Create("Continuous", "Single"), ComPDFKitViewer.ViewMode.SingleContinuous);
            GetViewMode.Add(Tuple.Create("Continuous", "Double"), ComPDFKitViewer.ViewMode.DoubleContinuous);
            GetViewMode.Add(Tuple.Create("Continuous", "Book"), ComPDFKitViewer.ViewMode.BookContinuous);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Single"), ComPDFKitViewer.ViewMode.Single);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Double"), ComPDFKitViewer.ViewMode.Double);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Book"), ComPDFKitViewer.ViewMode.Book);
            CPDFViewModeUI.SetContinuousEvent += CPDFViewModeUI_SetContinuousEvent;
            CPDFViewModeUI.SetViewModeEvent += CPDFViewModeUI_SetViewModeEvent;
        }

        private void CPDFViewModeUI_SetContinuousEvent(object sender, string e)
        {
            CurrentContinuousMode = (sender as RadioButton).Tag as string;
            pdfViewer.ChangeViewMode(GetViewMode[Tuple.Create(CurrentContinuousMode, CurrentViewMode)]);
        }

        private void CPDFViewModeUI_SetViewModeEvent(object sender, string e)
        {
            CurrentViewMode = (sender as RadioButton).Tag as string;
            pdfViewer.ChangeViewMode(GetViewMode[Tuple.Create(CurrentContinuousMode, CurrentViewMode)]);
        } 
    }
}
