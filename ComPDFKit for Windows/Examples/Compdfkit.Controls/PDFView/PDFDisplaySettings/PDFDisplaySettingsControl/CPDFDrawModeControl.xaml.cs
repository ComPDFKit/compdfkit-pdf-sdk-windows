using ComPDFKitViewer;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFDrawModeControl : UserControl
    {
        public PDFViewControl ViewControl;

        public Dictionary<string, DrawMode> GetDrawMode = new Dictionary<string, DrawMode>();

        public CPDFDrawModeControl()
        {
            InitializeComponent();
            CPDFDrawModeUI.Loaded += CPDFDrawModeUI_Loaded;
        }

        private void CPDFDrawModeUI_Loaded(object sender, RoutedEventArgs e)
        {
            GetDrawMode.Clear();
            GetDrawMode.Add("Normal", DrawMode.Normal);
            GetDrawMode.Add("Soft", DrawMode.Soft);
            GetDrawMode.Add("Dark", DrawMode.Dark);
            GetDrawMode.Add("Green", DrawMode.Green);
            GetDrawMode.Add("Custom", DrawMode.Custom);
            CPDFDrawModeUI.SetDrawModeEvent -= CPDFDrawModeUI_SetDrawModeEvent;
            CPDFDrawModeUI.SetDrawModeEvent += CPDFDrawModeUI_SetDrawModeEvent;
        }

        private void CPDFDrawModeUI_SetDrawModeEvent(object sender, string e)
        {
            if(ViewControl != null && ViewControl.PDFViewTool!=null)
            {
                CPDFViewer pdfViewer=ViewControl.PDFViewTool.GetCPDFViewer();
                pdfViewer?.SetDrawModes(GetDrawMode[(sender as RadioButton).Tag as string]);
            }
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
        }
    }
}
