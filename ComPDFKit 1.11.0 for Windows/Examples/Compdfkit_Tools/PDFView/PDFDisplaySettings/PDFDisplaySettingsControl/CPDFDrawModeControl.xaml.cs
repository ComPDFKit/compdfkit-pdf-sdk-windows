using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFDrawModeControl : UserControl
    {
        public CPDFViewer pdfViewer;

        public Dictionary<string, DrawModes> GetDrawMode = new Dictionary<string, DrawModes>();

        public CPDFDrawModeControl()
        {
            InitializeComponent();
            CPDFDrawModeUI.Loaded += CPDFDrawModeUI_Loaded;
        }

        private void CPDFDrawModeUI_Loaded(object sender, RoutedEventArgs e)
        {
            GetDrawMode.Clear();
            GetDrawMode.Add("Normal", DrawModes.Draw_Mode_Normal);
            GetDrawMode.Add("Soft", DrawModes.Draw_Mode_Soft);
            GetDrawMode.Add("Dark", DrawModes.Draw_Mode_Dark);
            GetDrawMode.Add("Green", DrawModes.Draw_Mode_Green);
            GetDrawMode.Add("Custom", DrawModes.Draw_Mode_Custom);
            CPDFDrawModeUI.SetDrawModeEvent += CPDFDrawModeUI_SetDrawModeEvent;
        }

        private void CPDFDrawModeUI_SetDrawModeEvent(object sender, string e)
        {
            pdfViewer.SetDrawMode(GetDrawMode[(sender as RadioButton).Tag as string]);
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
        }
    }
}
