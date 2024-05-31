using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static ComPDFKit.Controls.PDFControlUI.CPDFViewModeUI;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFViewModeControl : UserControl
    {
        public PDFViewControl ViewControl;

        public event EventHandler<SplitMode> SplitModeChanged;

        public string CurrentContinuousMode = "Continuous";
        public string CurrentViewMode = "Single";

        public Dictionary<Tuple<string, string>, ViewMode> GetViewMode = new Dictionary<Tuple<string, string>, ViewMode>();
        
        public CPDFViewModeControl()
        {
            InitializeComponent();
            CPDFViewModeUI.Loaded += CPDFViewModeUI_Loaded;
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            CPDFViewModeUI.SetContinuousEvent -= CPDFViewModeUI_SetContinuousEvent;
            CPDFViewModeUI.SetViewModeEvent -= CPDFViewModeUI_SetViewModeEvent;
            CPDFViewModeUI.SplitModeChanged -= CPDFViewModeUI_SplitModeChanged;

            CPDFViewModeUI.SetContinuousEvent += CPDFViewModeUI_SetContinuousEvent;
            CPDFViewModeUI.SetViewModeEvent += CPDFViewModeUI_SetViewModeEvent;
            CPDFViewModeUI.SplitModeChanged += CPDFViewModeUI_SplitModeChanged;
        }

        private void CPDFViewModeUI_Loaded(object sender, RoutedEventArgs e)
        {
            GetViewMode.Clear();
            GetViewMode.Add(Tuple.Create("Continuous", "Single"), ViewMode.SingleContinuous);
            GetViewMode.Add(Tuple.Create("Continuous", "Double"), ViewMode.DoubleContinuous);
            GetViewMode.Add(Tuple.Create("Continuous", "Book"), ViewMode.BookContinuous);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Single"), ViewMode.Single);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Double"), ViewMode.Double);
            GetViewMode.Add(Tuple.Create("Discontinuous", "Book"), ViewMode.Book);
        }

        private void CPDFViewModeUI_SplitModeChanged(object sender, SplitMode e)
        {
            SplitModeChanged.Invoke(this, e);
        }

        private void CPDFViewModeUI_SetContinuousEvent(object sender, string e)
        {
            CurrentContinuousMode = (sender as RadioButton).Tag as string;
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                pdfViewer?.SetViewMode(GetViewMode[Tuple.Create(CurrentContinuousMode, CurrentViewMode)]);
            }
        }

        private void CPDFViewModeUI_SetViewModeEvent(object sender, string e)
        {
            CurrentViewMode = (sender as RadioButton).Tag as string;
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                pdfViewer?.SetViewMode(GetViewMode[Tuple.Create(CurrentContinuousMode, CurrentViewMode)]);
            }
        }
    }
}
